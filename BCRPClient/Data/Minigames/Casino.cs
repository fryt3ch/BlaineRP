using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace BCRPClient.Data.Minigames.Casino
{
    [Script(int.MaxValue)]
    public class Casino 
    {
        public enum Types : sbyte
        {
            None = -1,

            Roulette = 0,
            SlotMachine = 1,
            Blackjack = 2,
        }

        public static bool IsActive => CEF.Browser.IsActive(CEF.Browser.IntTypes.CasinoMinigames);

        public static Types CurrentType { get; private set; } = Types.None;

        public static uint CurrentBet { get; set; }

        public static bool SoundOn { get; set; } = true;

        private static int EscBindIdx { get; set; } = -1;

        public Casino()
        {
            Events.Add("Casino::SetBet", (args) =>
            {
                CurrentBet = Utils.ToUInt32(args[0]);
            });

            Events.Add("CasinoSlots::Spin", async (args) =>
            {
                if (CurrentType == Types.SlotMachine)
                {
                    var curMachine = Data.Locations.Casino.SlotMachine.CurrentMachine;

                    if (curMachine == null)
                        return;

                    int casinoId = -1, machineId = -1;

                    for (int i = 0; i < Data.Locations.Casino.All.Count; i++)
                    {
                        machineId = Array.IndexOf(Data.Locations.Casino.All[i].SlotMachines, curMachine);

                        if (machineId < 0)
                            continue;

                        casinoId = i;

                        break;
                    }

                    if (casinoId < 0 || machineId < 0)
                        return;

                    if (Utils.IsTaskStillPending($"CASINO_SLOTMACHINE_{casinoId}_{machineId}", null))
                    {
                        CEF.Notification.ShowError("Сейчас нельзя сделать это!");

                        return;
                    }

                    var bet = CurrentBet;

                    if (bet < Data.Locations.Casino.SlotMachine.MinBet || bet > Data.Locations.Casino.SlotMachine.MaxBet)
                    {
                        CEF.Notification.ShowError($"На этом автомате разрешены ставки от {Utils.SplitToNumberOf(Data.Locations.Casino.SlotMachine.MinBet.ToString())} до {Utils.SplitToNumberOf(Data.Locations.Casino.SlotMachine.MaxBet.ToString())} фишек!", -1);

                        return;
                    }

                    if (Data.Locations.Casino.LastSent.IsSpam(500, false, true))
                        return;

                    Data.Locations.Casino.LastSent = Sync.World.ServerTime;

                    var res = ((string)await Events.CallRemoteProc("Casino::SLMB", casinoId, machineId, bet))?.Split('^');

                    if (res == null)
                        return;

                    var resultTypeN = byte.Parse(res[0]);

                    var jackpot = decimal.Parse(res[1]);

                    var resA = Data.Locations.Casino.SlotMachine.ReelIconTypes.Seven;
                    var resB = Data.Locations.Casino.SlotMachine.ReelIconTypes.Seven;
                    var resC = Data.Locations.Casino.SlotMachine.ReelIconTypes.Seven;

                    if (resultTypeN == 255)
                    {
                        var typesList = ((Data.Locations.Casino.SlotMachine.ReelIconTypes[])Enum.GetValues(typeof(Data.Locations.Casino.SlotMachine.ReelIconTypes))).ToList();

                        resA = typesList[Utils.Random.Next(0, typesList.Count)];
                        resB = typesList[Utils.Random.Next(0, typesList.Count)];

                        if (resA == resB)
                            typesList.Remove(resA);

                        resC = typesList[Utils.Random.Next(0, typesList.Count)];
                    }
                    else
                    {
                        resA = (Data.Locations.Casino.SlotMachine.ReelIconTypes)resultTypeN;
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
                        RAGE.Game.Audio.StopSound(Data.Locations.Casino.SlotMachine.SoundId);
                    }

                    CEF.Browser.Window.ExecuteJs("Casino.switchSound", state);
                }
            });

            Events.Add("CasinoBlackjack::BtnClick", async (args) =>
            {
                var btnIdx = (int)args[0];

                if (CurrentType == Types.Blackjack)
                {
                    var table = Data.Locations.Casino.Blackjack.CurrentTable;

                    if (table == null)
                        return;

                    int casinoId = -1, tableId = -1;

                    for (int i = 0; i < Data.Locations.Casino.All.Count; i++)
                    {
                        tableId = Array.IndexOf(Data.Locations.Casino.All[i].Blackjacks, table);

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
                            CEF.Notification.ShowError($"На этом автомате разрешены ставки от {Utils.SplitToNumberOf(table.MinBet.ToString())} до {Utils.SplitToNumberOf(table.MaxBet.ToString())} фишек!", -1);

                            return;
                        }

                        var curStateData = table.CurrentStateData;

                        if (curStateData == null || (curStateData[0] != 'I' && curStateData[0] != 'S'))
                        {
                            CEF.Notification.ShowError("Сейчас нельзя сделать это!");

                            return;
                        }

                        if (Data.Locations.Casino.LastSent.IsSpam(500, false, true))
                            return;

                        Data.Locations.Casino.LastSent = Sync.World.ServerTime;

                        var res = (bool)await Events.CallRemoteProc("Casino::BLJSB", casinoId, tableId, bet);

                        if (!res)
                            return;
                    }
                    else if (btnIdx == 0 || btnIdx == 1)
                    {
                        if (Data.Locations.Casino.LastSent.IsSpam(500, false, true))
                            return;

                        Data.Locations.Casino.LastSent = Sync.World.ServerTime;

                        Events.CallRemote("Casino::BLJD", casinoId, tableId, btnIdx == 0 ? 1 : 0);
                    }
                }
            });
        }

        public static async void ShowRoulette(Data.Locations.Casino casino, Data.Locations.Casino.Roulette roulette, decimal chipsBalance)
        {
            if (IsActive)
                return;

            await CEF.Browser.Render(CEF.Browser.IntTypes.CasinoMinigames, true, true);

            CurrentType = Types.Roulette;

            CEF.Browser.Window.ExecuteJs("Casino.draw", (int)CurrentType, roulette.GetCurrestStateString() ?? "null", chipsBalance, roulette.MaxBet, CurrentBet < roulette.MinBet || CurrentBet > roulette.MaxBet ? CurrentBet = roulette.MinBet : CurrentBet, new object[] { });

            CEF.Notification.SetOnTop(true);

            if (!Settings.User.Interface.HideHUD)
                CEF.HUD.ShowHUD(false);

            CEF.Chat.Show(false);

            CEF.Cursor.Show(true, true);

            GameEvents.DisableAllControls(true);

            KeyBinds.DisableAll(KeyBinds.Types.MicrophoneOff, KeyBinds.Types.MicrophoneOn, KeyBinds.Types.Cursor);

            roulette.StartGame();

            if (roulette.LastBets != null)
            {
                foreach (var x in roulette.LastBets)
                    AddLastBet(x);
            }

            EscBindIdx = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
        }

        public static async void ShowSlotMachine(Data.Locations.Casino casino, Data.Locations.Casino.SlotMachine slotMachine, decimal chipsBalance, decimal jackpot)
        {
            if (IsActive)
                return;

            var seatPos = slotMachine.MachineObj.GetWorldPositionOfBone(slotMachine.MachineObj.GetBoneIndexByName("Chair_Seat_01"));

            seatPos.Z += 0.1f;

            var machinePos = slotMachine.MachineObj.GetCoords(false);

            var machineHeading = slotMachine.MachineObj.GetHeading();

            Player.LocalPlayer.Position = seatPos;
            Player.LocalPlayer.SetHeading(machineHeading);

            await CEF.Browser.Render(CEF.Browser.IntTypes.CasinoMinigames, true, true);

            if (SoundOn)
                slotMachine.PlayGreetingSound();

            Data.Locations.Casino.SlotMachine.CurrentMachine = slotMachine;

            Additional.Camera.Enable(Additional.Camera.StateTypes.Empty, Player.LocalPlayer, Player.LocalPlayer, 0, null, null, null);

            Additional.Camera.Position = new Vector3(seatPos.X, seatPos.Y, seatPos.Z + 0.5f);
            Additional.Camera.PointAtPos(RAGE.Game.Object.GetObjectOffsetFromCoords(machinePos.X, machinePos.Y, machinePos.Z, machineHeading, 0f, 0.04f, 1.1f));

            Additional.Camera.Fov = 50;

            Player.LocalPlayer.SetVisible(false, false);

            CurrentType = Types.SlotMachine;

            CEF.Browser.Window.ExecuteJs($"Casino.draw", (int)CurrentType, Data.Locations.Casino.SlotMachine.GetJackpotString(jackpot), chipsBalance, Data.Locations.Casino.SlotMachine.MaxBet, CurrentBet < Data.Locations.Casino.SlotMachine.MinBet || CurrentBet > Data.Locations.Casino.SlotMachine.MaxBet ? CurrentBet = Data.Locations.Casino.SlotMachine.MinBet : CurrentBet);

            CEF.Browser.Window.ExecuteJs("Casino.switchSound", SoundOn);

            CEF.Notification.SetOnTop(true);

            if (!Settings.User.Interface.HideHUD)
                CEF.HUD.ShowHUD(false);

            CEF.Chat.Show(false);

            CEF.Cursor.Show(true, true);

            GameEvents.DisableAllControls(true);

            KeyBinds.DisableAll(KeyBinds.Types.MicrophoneOff, KeyBinds.Types.MicrophoneOn, KeyBinds.Types.Cursor);

            EscBindIdx = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
        }

        public static async void ShowBlackjack(Data.Locations.Casino casino, Data.Locations.Casino.Blackjack blackJack, byte seatIdx, decimal chipsBalance)
        {
            if (IsActive)
                return;

            var pData = Sync.Players.GetData(Player.LocalPlayer);

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
                    Sync.Animations.Stop(Player.LocalPlayer);

                    Sync.Animations.Play(Player.LocalPlayer, actAnim);
                }
            }

            await CEF.Browser.Render(CEF.Browser.IntTypes.CasinoMinigames, true, true);

            Player.LocalPlayer.SetVisible(false, false);

            Additional.Camera.Enable(Additional.Camera.StateTypes.Empty, Player.LocalPlayer, Player.LocalPlayer, 750, null, null, null);

            Additional.Camera.Position = new Vector3(seatPos.X, seatPos.Y, seatPos.Z + 0.75f);
            //Additional.Camera.PointAtPos(blackJack.TableObject.GetOffsetFromInWorldCoords(-0.2246f, 0.21305f, 0.957f));
            Additional.Camera.PointAtPos(blackJack.NPC.Ped.GetOffsetFromInWorldCoords(0.25f, 0.35f, -0.15f));
            Additional.Camera.Fov = 70;

            blackJack.StartGame(seatIdx);

            CurrentType = Types.Blackjack;

            CEF.Browser.Window.ExecuteJs($"Casino.draw", (int)CurrentType, blackJack.GetCurrestStateString(), chipsBalance, Data.Locations.Casino.SlotMachine.MaxBet, CurrentBet < Data.Locations.Casino.SlotMachine.MinBet || CurrentBet > Data.Locations.Casino.SlotMachine.MaxBet ? CurrentBet = Data.Locations.Casino.SlotMachine.MinBet : CurrentBet);

            CEF.Notification.SetOnTop(true);

            if (!Settings.User.Interface.HideHUD)
                CEF.HUD.ShowHUD(false);

            //CEF.Chat.Show(false);

            CEF.Cursor.Show(true, true);

            GameEvents.DisableMove(true);

            KeyBinds.DisableAll(KeyBinds.Types.MicrophoneOff, KeyBinds.Types.MicrophoneOn, KeyBinds.Types.Cursor);

            EscBindIdx = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());

            var stateData = blackJack.CurrentStateData;

            if (stateData != null)
            {
                if (stateData[0] == 'I' || stateData[0] == 'S')
                {
                    if (stateData[0] == 'S')
                    {
                        var myBet = blackJack.NPC.Ped?.GetData<List<Data.Locations.Casino.Blackjack.BetData>>("Bets")?[seatIdx]?.Amount ?? 0;

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
                var curMachine = Data.Locations.Casino.SlotMachine.CurrentMachine;

                if (curMachine == null)
                    return;

                int casinoId = -1, machineId = -1;

                for (int i = 0; i < Data.Locations.Casino.All.Count; i++)
                {
                    machineId = Array.IndexOf(Data.Locations.Casino.All[i].SlotMachines, curMachine);

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

                Additional.Camera.Disable(0);

                Player.LocalPlayer.SetVisible(true, true);

                if (curMachine.MachineObj?.Exists == true)
                {
                    Player.LocalPlayer.Position = curMachine.MachineObj.GetOffsetFromInWorldCoords(0f, -1f, 0.5f);
                }

                GameEvents.DisableAllControls(false);
            }
            else if (CurrentType == Types.Blackjack)
            {
                var curTable = Data.Locations.Casino.Blackjack.CurrentTable;

                if (curTable == null)
                    return;

                int casinoId = -1, tableId = -1;

                for (int i = 0; i < Data.Locations.Casino.All.Count; i++)
                {
                    tableId = Array.IndexOf(Data.Locations.Casino.All[i].Blackjacks, curTable);

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

                Additional.Camera.Disable(750);

                Player.LocalPlayer.SetVisible(true, true);

                if (curTable.TableObject?.Exists == true)
                {

                }

                GameEvents.DisableMove(false);

                curTable.StopGame();
            }
            else if (CurrentType == Types.Roulette)
            {
                Data.Locations.Casino.Roulette.CurrentRoulette?.StopGame();

                GameEvents.DisableAllControls(false);
            }

            CurrentType = Types.None;

            CEF.Browser.Render(CEF.Browser.IntTypes.CasinoMinigames, false);

            //CEF.Notification.ClearAll();

            CEF.Notification.SetOnTop(false);

            CEF.Cursor.Show(false, false);

            if (!Settings.User.Interface.HideHUD)
                CEF.HUD.ShowHUD(true);

            CEF.Chat.Show(true);

            KeyBinds.EnableAll();

            KeyBinds.Unbind(EscBindIdx);

            EscBindIdx = -1;
        }

        public static void AddLastBet(Data.Locations.Casino.Roulette.BetTypes betType)
        {
            if (!IsActive)
                return;

            if (Data.Locations.Casino.Roulette.HoverDatas == null)
                return;

            var colourNum = 0;

            if (betType == Locations.Casino.Roulette.BetTypes._0 || betType == Locations.Casino.Roulette.BetTypes._00)
            {
                colourNum = 2;
            }
            else
            {
                var blackNumbers = Data.Locations.Casino.Roulette.HoverDatas.GetValueOrDefault(Locations.Casino.Roulette.BetTypes.Black)?.HoverNumbers;

                if (blackNumbers != null)
                {
                    if (Array.IndexOf(blackNumbers, (byte)betType) >= 0)
                        colourNum = 1;
                }
            }

            CEF.Browser.Window.ExecuteJs("Casino.addLastNum", new List<object> { colourNum, betType.ToString().Replace("_", "") });
        }

        public static void UpdateStatus(string status)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("Casino.updateGameStatus", status);
        }

        public static void UpdateBalance(decimal balance)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("Casino.updateCurBal", balance);
        }

        public static void ShowBlackjackButton(int num, bool state)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("Casino.showBJButton", num, state);
        }
    }
}
