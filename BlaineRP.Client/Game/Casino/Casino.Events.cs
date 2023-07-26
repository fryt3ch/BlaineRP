using System.Collections.Generic;
using BlaineRP.Client.Game.Animations;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Casino
{
    public partial class Casino
    {
        [Script(int.MaxValue)]
        public class CasinoEvents
        {
            public CasinoEvents()
            {
                Events.Add("Casino::CB",
                    (args) =>
                    {
                        var newBalance = Utils.Convert.ToUInt32(args[0]);

                        if (CasinoMinigames.IsActive)
                        {
                            CasinoMinigames.UpdateBalance(newBalance);
                        }
                        else
                        {
                        }
                    });

                Events.Add("Casino::RLTS",
                    (args) =>
                    {
                        var casinoId = (int)args[0];
                        var rouletteId = (int)args[1];
                        var stateData = (string)args[2];

                        Roulette.OnCurrentStateDataUpdated(casinoId, rouletteId, stateData, false);
                    });

                Events.Add("Casino::BLJS",
                    (args) =>
                    {
                        var casinoId = (int)args[0];
                        var tableId = (int)args[1];
                        var stateData = (string)args[2];

                        Blackjack.OnCurrentStateDataUpdated(casinoId, tableId, stateData, false);
                    });

                Events.Add("Casino::BLJM",
                    async (args) =>
                    {
                        var type = Utils.Convert.ToByte(args[0]);

                        if (type == 0) // player anim
                        {
                            var animType = Utils.Convert.ToByte(args[1]);

                            var player = Entities.Players.GetAtRemote(Utils.Convert.ToUInt16(args[2]));

                            if (player?.Exists != true)
                                return;

                            if (animType == 1)
                                Core.Play(player, new Animation("anim_casino_b@amb@casino@games@blackjack@player", "decline_card_001", 8f, 1f, -1, 32, 0f, false, false, false), -1);
                            else if (animType == 2)
                                Core.Play(player, new Animation("anim_casino_b@amb@casino@games@blackjack@player", "request_card", 8f, 1f, -1, 32, 0f, false, false, false), -1);
                        }
                        else if (type == 1) // chip add
                        {
                            var casinoId = (int)args[1];
                            var tableId = (int)args[2];

                            var casino = Casino.GetById(casinoId);

                            var table = casino.GetBlackjackById(tableId);

                            var ped = table?.NPC?.Ped;

                            if (ped?.Exists != true || table.TableObject?.Exists != true)
                                return;

                            var seatIdx = Utils.Convert.ToByte(args[3]);

                            var amount = Utils.Convert.ToUInt32(args[4]);

                            var player = Entities.Players.GetAtRemote(Utils.Convert.ToUInt16(args[5]));

                            if (player?.Exists == true)
                                Core.Play(player, new Animation("anim_casino_b@amb@casino@games@blackjack@player", "place_bet_small", 8f, 1f, -1, 32, 0f, false, false, false), -1);

                            if (Blackjack.CurrentTable == table && Blackjack.CurrentSeatIdx == seatIdx)
                                if (CasinoMinigames.CurrentType == CasinoMinigames.Types.Blackjack)
                                {
                                    CasinoMinigames.ShowBlackjackButton(0, false);
                                    CasinoMinigames.ShowBlackjackButton(1, false);

                                    CasinoMinigames.ShowBlackjackButton(2, amount <= 0);
                                }

                            var oBets = ped.GetData<List<Blackjack.BetData>>("Bets");

                            if (oBets == null)
                            {
                                oBets = new List<Blackjack.BetData>()
                                {
                                    new Blackjack.BetData(), new Blackjack.BetData(), new Blackjack.BetData(), new Blackjack.BetData(),
                                };

                                ped.SetData("Bets", oBets);
                            }

                            oBets[seatIdx].Amount = amount;

                            if (amount <= 0)
                                return;

                            var tableHeading = table.TableObject.GetHeading();

                            var offsetInfo = Blackjack.BetOffsets[seatIdx][0];

                            var objModelStr = Casino.GetChipPropByAmount(amount);

                            var objModelhash = RAGE.Util.Joaat.Hash(objModelStr);
                            Streaming.RequestModelNow(objModelhash);

                            var coords = table.TableObject.GetOffsetFromInWorldCoords(offsetInfo.X, offsetInfo.Y, offsetInfo.Z);

                            oBets[seatIdx].MapObject?.Destroy();

                            oBets[seatIdx].MapObject =
                                new MapObject(RAGE.Game.Object.CreateObjectNoOffset(objModelhash, coords.X, coords.Y, coords.Z, false, false, false)) { Dimension = uint.MaxValue, };

                            oBets[seatIdx].MapObject.SetRotation(0f, 0f, tableHeading + offsetInfo.RotationZ, 0, false);
                        }
                    });

                Events.Add("Casino::LCWS",
                    (args) =>
                    {
                        var casinoId = (int)args[0];
                        var luckyWheelId = (int)args[1];

                        var casino = Casino.GetById(casinoId);

                        if (!casino.MainColshape.IsInside || AsyncTask.Methods.IsTaskStillPending("CASINO_TASK", null))
                            return;

                        var luckyWheel = casino.GetLuckyWheelById(luckyWheelId);

                        var player = Entities.Players.GetAtRemote(Utils.Convert.ToUInt16(args[2]));

                        var targetZoneType = (LuckyWheel.ZoneTypes)Utils.Convert.ToByte(args[3]);

                        var resultOffset = Utils.Convert.ToSingle(args[4]);

                        luckyWheel.Spin(casinoId, luckyWheelId, player, targetZoneType, resultOffset);
                    });

                var casinoSlotMachineIdle0Anim = Core.GeneralAnimsList.GetValueOrDefault(GeneralTypes.CasinoSlotMachineIdle0);

                if (casinoSlotMachineIdle0Anim != null)
                {
                    casinoSlotMachineIdle0Anim.StartAction = (entity, anim) =>
                    {
                        var ped = entity as PedBase;

                        if (ped?.Exists != true)
                            return;

                        ped.FreezePosition(true);
                        ped.SetCollision(false, true);
                    };

                    casinoSlotMachineIdle0Anim.StopAction = (entity, anim) =>
                    {
                        var ped = entity as PedBase;

                        if (ped?.Exists != true)
                            return;

                        ped.FreezePosition(false);
                        ped.SetCollision(true, false);
                    };
                }
            }
        }
    }
}