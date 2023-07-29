using System.Collections.Generic;
using BlaineRP.Client.Game.Animations;
using BlaineRP.Client.Game.Casino.Games;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Casino
{
    public partial class CasinoEntity
    {
        [Script(int.MaxValue)]
        public class CasinoEvents
        {
            public CasinoEvents()
            {
                Events.Add("Casino::CB",
                    (args) =>
                    {
                        var newBalance = Convert.ToUInt32(args[0]);

                        if (CasinoMinigames.IsActive)
                        {
                            CasinoMinigames.UpdateBalance(newBalance);
                        }
                        else
                        {
                        }
                    }
                );

                Events.Add("Casino::RLTS",
                    (args) =>
                    {
                        var casinoId = (int)args[0];
                        var rouletteId = (int)args[1];
                        var stateData = (string)args[2];

                        Roulette.OnCurrentStateDataUpdated(casinoId, rouletteId, stateData, false);
                    }
                );

                Events.Add("Casino::BLJS",
                    (args) =>
                    {
                        var casinoId = (int)args[0];
                        var tableId = (int)args[1];
                        var stateData = (string)args[2];

                        Blackjack.OnCurrentStateDataUpdated(casinoId, tableId, stateData, false);
                    }
                );

                Events.Add("Casino::BLJM",
                    async (args) =>
                    {
                        var type = Convert.ToByte(args[0]);

                        if (type == 0) // player anim
                        {
                            var animType = Convert.ToByte(args[1]);

                            Player player = Entities.Players.GetAtRemote(Convert.ToUInt16(args[2]));

                            if (player?.Exists != true)
                                return;

                            if (animType == 1)
                                Service.Play(player,
                                    new Animation("anim_casino_b@amb@casino@games@blackjack@player", "decline_card_001", 8f, 1f, -1, 32, 0f, false, false, false),
                                    -1
                                );
                            else if (animType == 2)
                                Service.Play(player, new Animation("anim_casino_b@amb@casino@games@blackjack@player", "request_card", 8f, 1f, -1, 32, 0f, false, false, false), -1);
                        }
                        else if (type == 1) // chip add
                        {
                            var casinoId = (int)args[1];
                            var tableId = (int)args[2];

                            CasinoEntity casino = GetById(casinoId);

                            Blackjack table = casino.GetBlackjackById(tableId);

                            Ped ped = table?.NPC?.Ped;

                            if (ped?.Exists != true || table.TableObject?.Exists != true)
                                return;

                            var seatIdx = Convert.ToByte(args[3]);

                            var amount = Convert.ToUInt32(args[4]);

                            Player player = Entities.Players.GetAtRemote(Convert.ToUInt16(args[5]));

                            if (player?.Exists == true)
                                Service.Play(player, new Animation("anim_casino_b@amb@casino@games@blackjack@player", "place_bet_small", 8f, 1f, -1, 32, 0f, false, false, false), -1);

                            if (Blackjack.CurrentTable == table && Blackjack.CurrentSeatIdx == seatIdx)
                                if (CasinoMinigames.CurrentType == CasinoMinigames.Types.Blackjack)
                                {
                                    CasinoMinigames.ShowBlackjackButton(0, false);
                                    CasinoMinigames.ShowBlackjackButton(1, false);

                                    CasinoMinigames.ShowBlackjackButton(2, amount <= 0);
                                }

                            List<Blackjack.BetData> oBets = ped.GetData<List<Blackjack.BetData>>("Bets");

                            if (oBets == null)
                            {
                                oBets = new List<Blackjack.BetData>()
                                {
                                    new Blackjack.BetData(),
                                    new Blackjack.BetData(),
                                    new Blackjack.BetData(),
                                    new Blackjack.BetData(),
                                };

                                ped.SetData("Bets", oBets);
                            }

                            oBets[seatIdx].Amount = amount;

                            if (amount <= 0)
                                return;

                            float tableHeading = table.TableObject.GetHeading();

                            Vector4 offsetInfo = Blackjack.BetOffsets[seatIdx][0];

                            string objModelStr = GetChipPropByAmount(amount);

                            uint objModelhash = RAGE.Util.Joaat.Hash(objModelStr);
                            Streaming.RequestModelNow(objModelhash);

                            Vector3 coords = table.TableObject.GetOffsetFromInWorldCoords(offsetInfo.X, offsetInfo.Y, offsetInfo.Z);

                            oBets[seatIdx].MapObject?.Destroy();

                            oBets[seatIdx].MapObject = new MapObject(RAGE.Game.Object.CreateObjectNoOffset(objModelhash, coords.X, coords.Y, coords.Z, false, false, false))
                            {
                                Dimension = uint.MaxValue,
                            };

                            oBets[seatIdx].MapObject.SetRotation(0f, 0f, tableHeading + offsetInfo.RotationZ, 0, false);
                        }
                    }
                );

                Events.Add("Casino::LCWS",
                    (args) =>
                    {
                        var casinoId = (int)args[0];
                        var luckyWheelId = (int)args[1];

                        CasinoEntity casino = GetById(casinoId);

                        if (!casino.MainColshape.IsInside || AsyncTask.Methods.IsTaskStillPending("CASINO_TASK", null))
                            return;

                        LuckyWheel luckyWheel = casino.GetLuckyWheelById(luckyWheelId);

                        Player player = Entities.Players.GetAtRemote(Convert.ToUInt16(args[2]));

                        var targetZoneType = (LuckyWheel.ZoneType)Convert.ToByte(args[3]);

                        var resultOffset = Convert.ToSingle(args[4]);

                        luckyWheel.Spin(casinoId, luckyWheelId, player, targetZoneType, resultOffset);
                    }
                );
            }
        }
    }
}