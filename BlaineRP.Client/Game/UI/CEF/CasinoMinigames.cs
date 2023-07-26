using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class CasinoMinigames
    {
        public enum Types : sbyte
        {
            None = -1,

            Roulette = 0,
            SlotMachine = 1,
            Blackjack = 2,
        }

        public static bool IsActive => Browser.IsActive(Browser.IntTypes.CasinoMinigames);

        public static Types CurrentType { get; private set; } = Types.None;

        public static uint CurrentBet { get; set; }

        public static bool SoundOn { get; set; } = true;

        private static int EscBindIdx { get; set; } = -1;

        public CasinoMinigames()
        {
            Events.Add("Casino::SetBet", (args) =>
            {
                CurrentBet = Utils.Convert.ToUInt32(args[0]);
            });

            Events.Add("CasinoSlots::Spin", async (args) =>
            {
                if (CurrentType == Types.SlotMachine)
                {
                    var curMachine = Game.Casino.SlotMachine.CurrentMachine;

                    if (curMachine == null)
                        return;

                    int casinoId = -1, machineId = -1;

                    for (int i = 0; i < Game.Casino.Casino.All.Count; i++)
                    {
                        machineId = Array.IndexOf(Game.Casino.Casino.All[i].SlotMachines, curMachine);

                        if (machineId < 0)
                            continue;

                        casinoId = i;

                        break;
                    }

                    if (casinoId < 0 || machineId < 0)
                        return;

                    if (AsyncTask.Methods.IsTaskStillPending($"CASINO_SLOTMACHINE_{casinoId}_{machineId}", null))
                    {
                        Notification.ShowErrorDefault();

                        return;
                    }

                    var bet = CurrentBet;

                    if (bet < Game.Casino.SlotMachine.MinBet || bet > Game.Casino.SlotMachine.MaxBet)
                    {
                        Notification.ShowError(Locale.Get("CASINO_BET_E_0", Locale.Get("CASINO_CHIPS_1", Game.Casino.SlotMachine.MinBet), Locale.Get("CASINO_CHIPS_1", Game.Casino.SlotMachine.MaxBet)));

                        return;
                    }

                    if (Game.Casino.Casino.LastSent.IsSpam(500, false, true))
                        return;

                    Game.Casino.Casino.LastSent = Game.World.Core.ServerTime;

                    var res = ((string)await Events.CallRemoteProc("Casino::SLMB", casinoId, machineId, bet))?.Split('^');

                    if (res == null)
                        return;

                    var resultTypeN = byte.Parse(res[0]);

                    var jackpot = decimal.Parse(res[1]);

                    var resA = Game.Casino.SlotMachine.ReelIconTypes.Seven;
                    var resB = Game.Casino.SlotMachine.ReelIconTypes.Seven;
                    var resC = Game.Casino.SlotMachine.ReelIconTypes.Seven;

                    if (resultTypeN == 255)
                    {
                        var typesList = ((Game.Casino.SlotMachine.ReelIconTypes[])Enum.GetValues(typeof(Game.Casino.SlotMachine.ReelIconTypes))).ToList();

                        resA = typesList[Utils.Misc.Random.Next(0, typesList.Count)];
                        resB = typesList[Utils.Misc.Random.Next(0, typesList.Count)];

                        if (resA == resB)
                            typesList.Remove(resA);

                        resC = typesList[Utils.Misc.Random.Next(0, typesList.Count)];
                    }
                    else
                    {
                        resA = (Game.Casino.SlotMachine.ReelIconTypes)resultTypeN;
                        resB = resA;
                        resC = resA;
                    }

                    curMachine.Spin(casinoId, machineId, resA, resB, resC, jackpot);
                }
            });

            Events.Add("CasinoSlots::Sound", (args) =>
            {
                var state = (bool)args[0];

                if (state == SoundOn)
                    return;

                if (CurrentType == Types.SlotMachine)
                {
                    SoundOn = state;

                    if (state)
                    {

                    }
                    else
                    {
                        RAGE.Game.Audio.StopSound(Game.Casino.SlotMachine.SoundId);
                    }

                    Browser.Window.ExecuteJs("Casino.switchSound", state);
                }
            });

            Events.Add("CasinoBlackjack::BtnClick", async (args) =>
            {
                var btnIdx = (int)args[0];

                if (CurrentType == Types.Blackjack)
                {
                    var table = Game.Casino.Blackjack.CurrentTable;

                    if (table == null)
                        return;

                    int casinoId = -1, tableId = -1;

                    for (int i = 0; i < Game.Casino.Casino.All.Count; i++)
                    {
                        tableId = Array.IndexOf(Game.Casino.Casino.All[i].Blackjacks, table);

                        if (tableId < 0)
                            continue;

                        casinoId = i;

                        break;
                    }

                    if (casinoId < 0 || tableId < 0)
                        return;

                    if (btnIdx == 2)
                    {
                        var bet = CurrentBet;

                        if (bet < table.MinBet || bet > table.MaxBet)
                        {
                            Notification.ShowError(Locale.Get("CASINO_BET_E_0", Locale.Get("CASINO_CHIPS_1", table.MinBet), Locale.Get("CASINO_CHIPS_1", table.MaxBet)));

                            return;
                        }

                        var curStateData = table.CurrentStateData;

                        if (curStateData == null || (curStateData[0] != 'I' && curStateData[0] != 'S'))
                        {
                            Notification.ShowErrorDefault();

                            return;
                        }

                        if (Game.Casino.Casino.LastSent.IsSpam(500, false, true))
                            return;

                        Game.Casino.Casino.LastSent = Game.World.Core.ServerTime;

                        var res = (bool)await Events.CallRemoteProc("Casino::BLJSB", casinoId, tableId, bet);

                        if (!res)
                            return;
                    }
                    else if (btnIdx == 0 || btnIdx == 1)
                    {
                        if (Game.Casino.Casino.LastSent.IsSpam(500, false, true))
                            return;

                        Game.Casino.Casino.LastSent = Game.World.Core.ServerTime;

                        Events.CallRemote("Casino::BLJD", casinoId, tableId, btnIdx == 0 ? 1 : 0);
                    }
                }
            });
        }

        public static async void ShowRoulette(Game.Casino.Casino casino, Game.Casino.Roulette roulette, decimal chipsBalance)
        {
            if (IsActive)
                return;

            await Browser.Render(Browser.IntTypes.CasinoMinigames, true, true);

            CurrentType = Types.Roulette;

            Browser.Window.ExecuteJs("Casino.draw", (int)CurrentType, roulette.GetCurrestStateString() ?? "null", chipsBalance, roulette.MaxBet, CurrentBet < roulette.MinBet || CurrentBet > roulette.MaxBet ? CurrentBet = roulette.MinBet : CurrentBet, new object[] { });

            Notification.SetOnTop(true);

            if (!Settings.User.Interface.HideHUD)
                HUD.ShowHUD(false);

            Chat.Show(false);

            Cursor.Show(true, true);

            Main.DisableAllControls(true);

            Input.Core.DisableAll(Input.Enums.BindTypes.MicrophoneOff, Input.Enums.BindTypes.MicrophoneOn, Input.Enums.BindTypes.Cursor);

            roulette.StartGame();

            if (roulette.LastBets != null)
            {
                foreach (var x in roulette.LastBets)
                    AddLastBet(x);
            }

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
        }

        public static async void ShowSlotMachine(Game.Casino.Casino casino, Game.Casino.SlotMachine slotMachine, decimal chipsBalance, decimal jackpot)
        {
            if (IsActive)
                return;

            var seatPos = slotMachine.MachineObj.GetWorldPositionOfBone(slotMachine.MachineObj.GetBoneIndexByName("Chair_Seat_01"));

            seatPos.Z += 0.1f;

            var machinePos = slotMachine.MachineObj.GetCoords(false);

            var machineHeading = slotMachine.MachineObj.GetHeading();

            Player.LocalPlayer.Position = seatPos;
            Player.LocalPlayer.SetHeading(machineHeading);

            await Browser.Render(Browser.IntTypes.CasinoMinigames, true, true);

            if (SoundOn)
                slotMachine.PlayGreetingSound();

            Game.Casino.SlotMachine.CurrentMachine = slotMachine;

            Game.Management.Camera.Core.Enable(Game.Management.Camera.Core.StateTypes.Empty, Player.LocalPlayer, Player.LocalPlayer, 0, null, null, null);

            Game.Management.Camera.Core.Position = new Vector3(seatPos.X, seatPos.Y, seatPos.Z + 0.5f);
            Game.Management.Camera.Core.PointAtPos(RAGE.Game.Object.GetObjectOffsetFromCoords(machinePos.X, machinePos.Y, machinePos.Z, machineHeading, 0f, 0.04f, 1.1f));

            Game.Management.Camera.Core.Fov = 50;

            Player.LocalPlayer.SetVisible(false, false);

            CurrentType = Types.SlotMachine;

            Browser.Window.ExecuteJs($"Casino.draw", (int)CurrentType, Game.Casino.SlotMachine.GetJackpotString(jackpot), chipsBalance, Game.Casino.SlotMachine.MaxBet, CurrentBet < Game.Casino.SlotMachine.MinBet || CurrentBet > Game.Casino.SlotMachine.MaxBet ? CurrentBet = Game.Casino.SlotMachine.MinBet : CurrentBet);

            Browser.Window.ExecuteJs("Casino.switchSound", SoundOn);

            Notification.SetOnTop(true);

            if (!Settings.User.Interface.HideHUD)
                HUD.ShowHUD(false);

            Chat.Show(false);

            Cursor.Show(true, true);

            Main.DisableAllControls(true);

            Input.Core.DisableAll(Input.Enums.BindTypes.MicrophoneOff, Input.Enums.BindTypes.MicrophoneOn, Input.Enums.BindTypes.Cursor);

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
        }

        public static async void ShowBlackjack(Game.Casino.Casino casino, Game.Casino.Blackjack blackJack, byte seatIdx, decimal chipsBalance)
        {
            if (IsActive)
                return;

            var pData = PlayerData.GetData(Player.LocalPlayer);

            var realSeatIdx = (seatIdx > 3 ? 0 : seatIdx) + 1;

            var chairIdStr = $"Chair_Seat_0{realSeatIdx}";

            var boneIdx = blackJack.TableObject.GetBoneIndexByName(chairIdStr);

            var seatPos = blackJack.TableObject.GetWorldPositionOfBone(boneIdx);
            var seatRot = blackJack.TableObject.GetWorldRotationOfBone(boneIdx);

            seatPos.Z += 0.1f;

            Player.LocalPlayer.Position = seatPos;
            Player.LocalPlayer.SetHeading(seatRot.Z - 90f);

            if (pData != null)
            {
                var actAnim = pData.ActualAnimation;

                if (actAnim != null)
                {
                    Game.Animations.Core.Stop(Player.LocalPlayer);

                    Game.Animations.Core.Play(Player.LocalPlayer, actAnim);
                }
            }

            await Browser.Render(Browser.IntTypes.CasinoMinigames, true, true);

            Player.LocalPlayer.SetVisible(false, false);

            Game.Management.Camera.Core.Enable(Game.Management.Camera.Core.StateTypes.Empty, Player.LocalPlayer, Player.LocalPlayer, 750, null, null, null);

            Game.Management.Camera.Core.Position = new Vector3(seatPos.X, seatPos.Y, seatPos.Z + 0.75f);
            //Additional.Camera.PointAtPos(blackJack.TableObject.GetOffsetFromInWorldCoords(-0.2246f, 0.21305f, 0.957f));
            Game.Management.Camera.Core.PointAtPos(blackJack.NPC.Ped.GetOffsetFromInWorldCoords(0.25f, 0.35f, -0.15f));
            Game.Management.Camera.Core.Fov = 70;

            blackJack.StartGame(seatIdx);

            CurrentType = Types.Blackjack;

            Browser.Window.ExecuteJs($"Casino.draw", (int)CurrentType, blackJack.GetCurrestStateString(), chipsBalance, Game.Casino.SlotMachine.MaxBet, CurrentBet < Game.Casino.SlotMachine.MinBet || CurrentBet > Game.Casino.SlotMachine.MaxBet ? CurrentBet = Game.Casino.SlotMachine.MinBet : CurrentBet);

            Notification.SetOnTop(true);

            if (!Settings.User.Interface.HideHUD)
                HUD.ShowHUD(false);

            //CEF.Chat.Show(false);

            Cursor.Show(true, true);

            Main.DisableMove(true);

            Input.Core.DisableAll(Input.Enums.BindTypes.MicrophoneOff, Input.Enums.BindTypes.MicrophoneOn, Input.Enums.BindTypes.Cursor);

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());

            var stateData = blackJack.CurrentStateData;

            if (stateData != null)
            {
                if (stateData[0] == 'I' || stateData[0] == 'S')
                {
                    if (stateData[0] == 'S')
                    {
                        var myBet = blackJack.NPC.Ped?.GetData<List<Game.Casino.Blackjack.BetData>>("Bets")?[seatIdx]?.Amount ?? 0;

                        if (myBet <= 0)
                        {
                            ShowBlackjackButton(2, true);
                        }
                    }
                    else
                    {
                        ShowBlackjackButton(2, true);
                    }
                }
                else if (stateData[0] == 'D')
                {
                    var subData = stateData.Split('*');

                    if (seatIdx == byte.Parse(subData[1]))
                    {
                        ShowBlackjackButton(0, true);
                        ShowBlackjackButton(1, true);
                    }
                }
            }
        }

        public static async void Close()
        {
            if (!IsActive)
                return;

            if (CurrentType == Types.SlotMachine)
            {
                var curMachine = Game.Casino.SlotMachine.CurrentMachine;

                if (curMachine == null)
                    return;

                int casinoId = -1, machineId = -1;

                for (int i = 0; i < Game.Casino.Casino.All.Count; i++)
                {
                    machineId = Array.IndexOf(Game.Casino.Casino.All[i].SlotMachines, curMachine);

                    if (machineId < 0)
                        continue;

                    casinoId = i;

                    break;
                }

                if (casinoId < 0 || machineId < 0)
                    return;

                if (!(bool)await Events.CallRemoteProc("Casino::SLML", casinoId, machineId))
                {

                }

                Game.Management.Camera.Core.Disable(0);

                Player.LocalPlayer.SetVisible(true, true);

                if (curMachine.MachineObj?.Exists == true)
                {
                    Player.LocalPlayer.Position = curMachine.MachineObj.GetOffsetFromInWorldCoords(0f, -1f, 0.5f);
                }

                Main.DisableAllControls(false);
            }
            else if (CurrentType == Types.Blackjack)
            {
                var curTable = Game.Casino.Blackjack.CurrentTable;

                if (curTable == null)
                    return;

                int casinoId = -1, tableId = -1;

                for (int i = 0; i < Game.Casino.Casino.All.Count; i++)
                {
                    tableId = Array.IndexOf(Game.Casino.Casino.All[i].Blackjacks, curTable);

                    if (tableId < 0)
                        continue;

                    casinoId = i;

                    break;
                }

                if (casinoId < 0 || tableId < 0)
                    return;

                if (!(bool)await Events.CallRemoteProc("Casino::BLJL", casinoId, tableId))
                {

                }

                Game.Management.Camera.Core.Disable(750);

                Player.LocalPlayer.SetVisible(true, true);

                if (curTable.TableObject?.Exists == true)
                {

                }

                Main.DisableMove(false);

                curTable.StopGame();
            }
            else if (CurrentType == Types.Roulette)
            {
                Game.Casino.Roulette.CurrentRoulette?.StopGame();

                Main.DisableAllControls(false);
            }

            CurrentType = Types.None;

            Browser.Render(Browser.IntTypes.CasinoMinigames, false);

            //CEF.Notification.ClearAll();

            Notification.SetOnTop(false);

            Cursor.Show(false, false);

            if (!Settings.User.Interface.HideHUD)
                HUD.ShowHUD(true);

            Chat.Show(true);

            Input.Core.EnableAll();

            Input.Core.Unbind(EscBindIdx);

            EscBindIdx = -1;
        }

        public static void AddLastBet(Game.Casino.Roulette.BetTypes betType)
        {
            if (!IsActive)
                return;

            if (Game.Casino.Roulette.HoverDatas == null)
                return;

            var colourNum = 0;

            if (betType == Game.Casino.Roulette.BetTypes._0 || betType == Game.Casino.Roulette.BetTypes._00)
            {
                colourNum = 2;
            }
            else
            {
                var blackNumbers = Game.Casino.Roulette.HoverDatas.GetValueOrDefault(Game.Casino.Roulette.BetTypes.Black)?.HoverNumbers;

                if (blackNumbers != null)
                {
                    if (Array.IndexOf(blackNumbers, (byte)betType) >= 0)
                        colourNum = 1;
                }
            }

            Browser.Window.ExecuteJs("Casino.addLastNum", new List<object> { colourNum, betType.ToString().Replace("_", "") });
        }

        public static void UpdateStatus(string status)
        {
            if (!IsActive)
                return;

            Browser.Window.ExecuteJs("Casino.updateGameStatus", status);
        }

        public static void UpdateBalance(decimal balance)
        {
            if (!IsActive)
                return;

            Browser.Window.ExecuteJs("Casino.updateCurBal", balance);
        }

        public static void ShowBlackjackButton(int num, bool state)
        {
            if (!IsActive)
                return;

            Browser.Window.ExecuteJs("Casino.showBJButton", num, state);
        }
    }
}
