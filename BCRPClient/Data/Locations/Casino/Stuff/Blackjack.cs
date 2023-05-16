using BCRPClient.Sync;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Data
{
    public partial class Locations
    {
        public partial class Casino
        {
            public class Blackjack
            {
                public class CardData
                {
                    public CardTypes CardType { get; set; }

                    public byte Value { get; set; }

                    public MapObject MapObject { get; set; }
                }

                public class BetData
                {
                    public decimal Amount { get; set; }

                    public MapObject MapObject { get; set; }
                }

                public enum CardTypes : byte
                {
                    None = 0,

                    Club_Ace,
                    Club_02,
                    Club_03,
                    Club_04,
                    Club_05,
                    Club_06,
                    Club_07,
                    Club_08,
                    Club_09,
                    Club_10,
                    Club_Jack,
                    Club_Queen,
                    Club_King,

                    Dia_Ace,
                    Dia_02,
                    Dia_03,
                    Dia_04,
                    Dia_05,
                    Dia_06,
                    Dia_07,
                    Dia_08,
                    Dia_09,
                    Dia_10,
                    Dia_Jack,
                    Dia_Queen,
                    Dia_King,

                    Hrt_Ace,
                    Hrt_02,
                    Hrt_03,
                    Hrt_04,
                    Hrt_05,
                    Hrt_06,
                    Hrt_07,
                    Hrt_08,
                    Hrt_09,
                    Hrt_10,
                    Hrt_Jack,
                    Hrt_Queen,
                    Hrt_King,

                    Spd_Ace,
                    Spd_02,
                    Spd_03,
                    Spd_04,
                    Spd_05,
                    Spd_06,
                    Spd_07,
                    Spd_08,
                    Spd_09,
                    Spd_10,
                    Spd_Jack,
                    Spd_Queen,
                    Spd_King,
                }

                public static Utils.Vector4[][] CardOffsets = new Utils.Vector4[][]
                {
                    new Utils.Vector4[]
                    {
                        new Utils.Vector4(0.0436f, 0.21205f, 0.948875f, 178.92f),
                        new Utils.Vector4(-0.0636f, 0.213825f, 0.9496f, -180f),
                        new Utils.Vector4(-0.0806f, 0.2137f, 0.950225f, -178.92f),
                        new Utils.Vector4(-0.1006f, 0.21125f, 0.950875f, -177.12f),
                        new Utils.Vector4(-0.1256f, 0.21505f, 0.951875f, 180f),
                        new Utils.Vector4(-0.1416f, 0.21305f, 0.953f, 178.56f),
                        new Utils.Vector4(-0.1656f, 0.21205f, 0.954025f, 180f),
                        new Utils.Vector4(-0.1836f, 0.21255f, 0.95495f, 178.2f),
                        new Utils.Vector4(-0.2076f, 0.21105f, 0.956025f, -177.12f),
                        new Utils.Vector4(-0.2246f, 0.21305f, 0.957f, 180f),
                    },

                    new Utils.Vector4[]
                    {
                        new Utils.Vector4(-0.5765f, 0.2229f, 0.9482f, -67.03f),
                        new Utils.Vector4(-0.558925f, 0.2197f, 0.949175f, -69.12f),
                        new Utils.Vector4(-0.5425f, 0.213025f, 0.9499f, -64.44f),
                        new Utils.Vector4(-0.525925f, 0.21105f, 0.95095f, -67.68f),
                        new Utils.Vector4(-0.509475f, 0.20535f, 0.9519f, -63.72f),
                        new Utils.Vector4(-0.491775f, 0.204075f, 0.952825f, -68.4f),
                        new Utils.Vector4(-0.4752f, 0.197525f, 0.9543f, -64.44f),
                    },

                    new Utils.Vector4[]
                    {
                        new Utils.Vector4(-0.2359f, -0.1091f, 0.9483f, -21.43f),
                        new Utils.Vector4(-0.221025f, -0.100675f, 0.949f, -20.16f),
                        new Utils.Vector4(-0.20625f, -0.092875f, 0.949725f, -16.92f),
                        new Utils.Vector4(-0.193225f, -0.07985f, 0.950325f, -23.4f),
                        new Utils.Vector4(-0.1776f, -0.072f, 0.951025f, -21.24f),
                        new Utils.Vector4(-0.165f, -0.060025f, 0.951825f, -23.76f),
                        new Utils.Vector4(-0.14895f, -0.05155f, 0.95255f, -19.44f),
                    },

                    new Utils.Vector4[]
                    {
                        new Utils.Vector4(0.2325f, -0.1082f, 0.94805f, 22.11f),
                        new Utils.Vector4(0.23645f, -0.0918f, 0.949f, 22.32f),
                        new Utils.Vector4(0.2401f, -0.074475f, 0.950225f, 20.8f),
                        new Utils.Vector4(0.244625f, -0.057675f, 0.951125f, 19.8f),
                        new Utils.Vector4(0.249675f, -0.041475f, 0.95205f, 19.44f),
                        new Utils.Vector4(0.257575f, -0.0256f, 0.9532f, 26.28f),
                        new Utils.Vector4(0.2601f, -0.008175f, 0.954375f, 22.68f),
                    },

                    new Utils.Vector4[]
                    {
                        new Utils.Vector4(0.5737f, 0.2376f, 0.948025f, 69.12f),
                        new Utils.Vector4(0.562975f, 0.2523f, 0.94875f, 67.8f),
                        new Utils.Vector4(0.553875f, 0.266325f, 0.94955f, 66.6f),
                        new Utils.Vector4(0.5459f, 0.282075f, 0.9501f, 70.44f),
                        new Utils.Vector4(0.536125f, 0.29645f, 0.95085f, 70.84f),
                        new Utils.Vector4(0.524975f, 0.30975f, 0.9516f, 67.88f),
                        new Utils.Vector4(0.515775f, 0.325325f, 0.95235f, 69.56f),
                    },
                };

                public static Utils.Vector4[][] BetOffsets = new Utils.Vector4[][]
                {
                    new Utils.Vector4[]
                    {
                        new Utils.Vector4(-0.72855f, 0.17345f, 0.95f, -79.2f),
                    },

                    new Utils.Vector4[]
                    {
                        new Utils.Vector4(-0.30305f, -0.2464f, 0.95f, -18.36f),
                    },

                    new Utils.Vector4[]
                    {
                        new Utils.Vector4(0.278125f, -0.2571f, 0.95f, 12.96f),
                    },

                    new Utils.Vector4[]
                    {
                        new Utils.Vector4(0.712625f, 0.170625f, 0.95f, 72f),
                    },
                };

                public static Blackjack CurrentTable { get; set; }

                public static byte CurrentSeatIdx { get; set; }

                public MapObject TableObject { get; set; }

                public NPC NPC { get; set; }

                public Additional.ExtraLabel TextLabel { get; set; }

                public uint MinBet { get; set; }

                public uint MaxBet { get; set; }

                public string CurrentStateData { get; set; }

                public Blackjack(int CasinoId, int Id, string Model, float PosX, float PosY, float PosZ, float Heading)
                {
                    TableObject = new MapObject(RAGE.Util.Joaat.Hash(Model), new Vector3(PosX, PosY, PosZ), new Vector3(0f, 0f, Heading), 255, Settings.MAIN_DIMENSION)
                    {
                        NotifyStreaming = true, // h4_prop_casino_blckjack_01e
                    };

                    NPC = new NPC($"Casino@Blackjack_{CasinoId}_{Id}", "", NPC.Types.Static, "S_M_Y_Casino_01", RAGE.Game.Object.GetObjectOffsetFromCoords(PosX, PosY, PosZ, Heading, 0f, 0.7f, 1f), Heading + 180f, Settings.MAIN_DIMENSION);

                    NPC.Ped.SetStreamInCustomAction(OnPedStreamIn);
                }

                private static void OnPedStreamIn(Entity entity)
                {
                    var ped = entity as Ped;

                    if (ped == null)
                        return;

                    var randomClothesNumber = Utils.Random.Next(0, 7);

                    if (randomClothesNumber == 0)
                    {
                        ped.SetComponentVariation(0, 3, 0, 0);
                        ped.SetComponentVariation(1, 1, 0, 0);
                        ped.SetComponentVariation(2, 3, 0, 0);
                        ped.SetComponentVariation(3, 1, 0, 0);
                        ped.SetComponentVariation(4, 0, 0, 0);
                        ped.SetComponentVariation(6, 1, 0, 0);
                        ped.SetComponentVariation(7, 2, 0, 0);
                        ped.SetComponentVariation(8, 3, 0, 0);
                        ped.SetComponentVariation(10, 1, 0, 0);
                        ped.SetComponentVariation(11, 1, 0, 0);

                        ped.SetPropIndex(1, 0, 0, false);
                    }
                    else if (randomClothesNumber == 1)
                    {
                        ped.SetComponentVariation(0, 2, 2, 0);
                        ped.SetComponentVariation(1, 1, 0, 0);
                        ped.SetComponentVariation(2, 4, 0, 0);
                        ped.SetComponentVariation(3, 0, 3, 0);
                        ped.SetComponentVariation(4, 0, 0, 0);
                        ped.SetComponentVariation(6, 1, 0, 0);
                        ped.SetComponentVariation(7, 2, 0, 0);
                        ped.SetComponentVariation(8, 1, 0, 0);
                        ped.SetComponentVariation(10, 1, 0, 0);
                        ped.SetComponentVariation(11, 1, 0, 0);
                    }
                    else if (randomClothesNumber == 2)
                    {
                        ped.SetComponentVariation(0, 2, 1, 0);
                        ped.SetComponentVariation(1, 1, 0, 0);
                        ped.SetComponentVariation(2, 2, 0, 0);
                        ped.SetComponentVariation(3, 0, 3, 0);
                        ped.SetComponentVariation(4, 0, 0, 0);
                        ped.SetComponentVariation(6, 1, 0, 0);
                        ped.SetComponentVariation(7, 2, 0, 0);
                        ped.SetComponentVariation(8, 1, 0, 0);
                        ped.SetComponentVariation(10, 1, 0, 0);
                        ped.SetComponentVariation(11, 1, 0, 0);
                    }
                    else if (randomClothesNumber == 3)
                    {
                        ped.SetComponentVariation(0, 2, 0, 0);
                        ped.SetComponentVariation(1, 1, 0, 0);
                        ped.SetComponentVariation(2, 3, 0, 0);
                        ped.SetComponentVariation(3, 1, 3, 0);
                        ped.SetComponentVariation(4, 0, 0, 0);
                        ped.SetComponentVariation(6, 1, 0, 0);
                        ped.SetComponentVariation(7, 2, 0, 0);
                        ped.SetComponentVariation(8, 3, 0, 0);
                        ped.SetComponentVariation(10, 1, 0, 0);
                        ped.SetComponentVariation(11, 1, 0, 0);
                    }
                    else if (randomClothesNumber == 4)
                    {
                        ped.SetComponentVariation(0, 4, 2, 0);
                        ped.SetComponentVariation(1, 1, 0, 0);
                        ped.SetComponentVariation(2, 3, 0, 0);
                        ped.SetComponentVariation(3, 0, 0, 0);
                        ped.SetComponentVariation(4, 0, 0, 0);
                        ped.SetComponentVariation(6, 1, 0, 0);
                        ped.SetComponentVariation(7, 2, 0, 0);
                        ped.SetComponentVariation(8, 1, 0, 0);
                        ped.SetComponentVariation(10, 1, 0, 0);
                        ped.SetComponentVariation(11, 1, 0, 0);
                    }
                    else if (randomClothesNumber == 5)
                    {
                        ped.SetComponentVariation(0, 4, 0, 0);
                        ped.SetComponentVariation(1, 1, 0, 0);
                        ped.SetComponentVariation(2, 0, 0, 0);
                        ped.SetComponentVariation(3, 0, 0, 0);
                        ped.SetComponentVariation(4, 0, 0, 0);
                        ped.SetComponentVariation(6, 1, 0, 0);
                        ped.SetComponentVariation(7, 2, 0, 0);
                        ped.SetComponentVariation(8, 1, 0, 0);
                        ped.SetComponentVariation(10, 1, 0, 0);
                        ped.SetComponentVariation(11, 1, 0, 0);
                    }
                    else if (randomClothesNumber == 6)
                    {
                        ped.SetComponentVariation(0, 4, 1, 0);
                        ped.SetComponentVariation(1, 1, 0, 0);
                        ped.SetComponentVariation(2, 4, 0, 0);
                        ped.SetComponentVariation(3, 1, 0, 0);
                        ped.SetComponentVariation(4, 0, 0, 0);
                        ped.SetComponentVariation(6, 1, 0, 0);
                        ped.SetComponentVariation(7, 2, 0, 0);
                        ped.SetComponentVariation(8, 3, 0, 0);
                        ped.SetComponentVariation(10, 1, 0, 0);
                        ped.SetComponentVariation(11, 1, 0, 0);
                    }

                    Sync.Animations.Play(ped, new Sync.Animations.Animation("anim_casino_b@amb@casino@games@blackjack@dealer", "idle", 8f, 0f, -1, 0, 0f, true, true, true), -1);
                }

                public string GetCurrestStateString()
                {
                    if (TextLabel == null || TextLabel.Text == null)
                        return null;

                    var indexStr = TextLabel.Text.IndexOf("\n\n");

                    if (indexStr < 0)
                        return null;

                    return TextLabel.Text.Substring(indexStr + 2);
                }

                public static void OnCurrentStateDataUpdated(int casinoId, int tableId, string stateData, bool onLoad)
                {
                    var casino = Casino.GetById(casinoId);

                    if (!onLoad && (!casino.MainColshape.IsInside || Utils.IsTaskStillPending("CASINO_TASK", null)))
                        return;

                    var table = casino.GetBlackjackById(tableId);

                    table.CurrentStateData = stateData;

                    if (table.TextLabel == null)
                        return;

                    var stateTask = table.TextLabel.GetData<Additional.ExtraTimer>("StateTask");

                    if (stateTask != null)
                    {
                        stateTask.Dispose();

                        table.TextLabel.ResetData("StateTask");
                    }

                    if (stateData is string str)
                    {
                        if (str[0] == 'I')
                        {
                            var defText = "Ожидание первой ставки...";

                            updateFunc(defText);

                            if (CurrentTable == table)
                            {
                                if (Data.Minigames.Casino.Casino.CurrentType == Minigames.Casino.Casino.Types.Blackjack)
                                {
                                    Data.Minigames.Casino.Casino.ShowBlackjackButton(0, false);
                                    Data.Minigames.Casino.Casino.ShowBlackjackButton(1, false);

                                    Data.Minigames.Casino.Casino.ShowBlackjackButton(2, true);
                                }
                            }

                            if (!onLoad)
                            {
                                var bets = table.NPC?.Ped?.GetData<List<BetData>>("Bets");

                                if (bets != null)
                                {
                                    foreach (var x in bets)
                                    {
                                        x.MapObject?.Destroy();
                                    }

                                    bets.Clear();

                                    table.NPC.Ped.ResetData("Bets");
                                }

                                var dealerHand = table.NPC?.Ped?.GetData<List<CardData>>("DHand");

                                if (dealerHand != null)
                                {
                                    foreach (var x in dealerHand)
                                    {
                                        x.MapObject?.Destroy();
                                    }

                                    dealerHand.Clear();

                                    table.NPC.Ped.ResetData("DHand");
                                }

                                for (int i = 0; i < 4; i++)
                                {
                                    var key = $"PHand{i}";

                                    var hand = table.NPC?.Ped?.GetData<List<CardData>>(key);

                                    if (hand != null)
                                    {
                                        foreach (var x in hand)
                                        {
                                            x.MapObject?.Destroy();
                                        }

                                        hand.Clear();

                                        table.NPC.Ped.ResetData(key);
                                    }
                                }
                            }
                        }
                        else if (str[0] == 'R')
                        {
                            updateFunc("Раздача карт...");

                            if (CurrentTable == table && Data.Minigames.Casino.Casino.CurrentType == Minigames.Casino.Casino.Types.Blackjack)
                            {
                                Data.Minigames.Casino.Casino.ShowBlackjackButton(2, false);

                                Data.Minigames.Casino.Casino.ShowBlackjackButton(0, false);
                                Data.Minigames.Casino.Casino.ShowBlackjackButton(1, false);
                            }

                            table.StartCardGiving(str, onLoad);
                        }
                        else if (str[0] == 'F')
                        {
                            updateFunc("Завершение игры...");

                            if (CurrentTable == table && Data.Minigames.Casino.Casino.CurrentType == Minigames.Casino.Casino.Types.Blackjack)
                            {
                                Data.Minigames.Casino.Casino.ShowBlackjackButton(2, false);

                                Data.Minigames.Casino.Casino.ShowBlackjackButton(0, false);
                                Data.Minigames.Casino.Casino.ShowBlackjackButton(1, false);
                            }

                            table.FinishGame(str, onLoad);
                        }
                        else if (str[0] == 'D')
                        {
                            var subData = str.Split('*');

                            var seatIdx = byte.Parse(subData[1]);
                            var time = long.Parse(subData[2]);

                            if (CurrentTable == table && CurrentSeatIdx == seatIdx)
                            {
                                var timer = new Additional.ExtraTimer(async (obj) =>
                                {
                                    await RAGE.Game.Invoker.WaitAsync(0);

                                    updateFunc($"Ваш ход! ({DateTimeOffset.FromUnixTimeSeconds(time).DateTime.Subtract(Sync.World.ServerTime).GetBeautyString()})");
                                }, null, 0, 1000);

                                table.TextLabel.SetData("StateTask", timer);

                                if (Data.Minigames.Casino.Casino.CurrentType == Minigames.Casino.Casino.Types.Blackjack)
                                {
                                    Data.Minigames.Casino.Casino.ShowBlackjackButton(2, false);

                                    Data.Minigames.Casino.Casino.ShowBlackjackButton(0, true);
                                    Data.Minigames.Casino.Casino.ShowBlackjackButton(1, true);
                                }
                            }
                            else
                            {
                                if (CurrentTable == table && Data.Minigames.Casino.Casino.CurrentType == Minigames.Casino.Casino.Types.Blackjack)
                                {
                                    Data.Minigames.Casino.Casino.ShowBlackjackButton(2, false);

                                    Data.Minigames.Casino.Casino.ShowBlackjackButton(0, false);
                                    Data.Minigames.Casino.Casino.ShowBlackjackButton(1, false);
                                }

                                var timer = new Additional.ExtraTimer(async (obj) =>
                                {
                                    await RAGE.Game.Invoker.WaitAsync(0);

                                    updateFunc($"Ход игрока #{seatIdx + 1} ({DateTimeOffset.FromUnixTimeSeconds(time).DateTime.Subtract(Sync.World.ServerTime).GetBeautyString()})");
                                }, null, 0, 1000);

                                table.TextLabel.SetData("StateTask", timer);
                            }

                            table.StartDecisionPlayer(str, onLoad, 0);
                        }
                        else if (str[0] == 'H')
                        {
                            var subData = str.Split('*');

                            var seatIdx = byte.Parse(subData[1]);

                            if (CurrentTable == table && CurrentSeatIdx == seatIdx)
                            {
                                if (Data.Minigames.Casino.Casino.CurrentType == Minigames.Casino.Casino.Types.Blackjack)
                                {
                                    Data.Minigames.Casino.Casino.ShowBlackjackButton(2, false);

                                    Data.Minigames.Casino.Casino.ShowBlackjackButton(0, false);
                                    Data.Minigames.Casino.Casino.ShowBlackjackButton(1, false);
                                }

                                updateFunc("Вы берёте еще одну карту...");
                            }
                            else
                            {
                                if (CurrentTable == table && Data.Minigames.Casino.Casino.CurrentType == Minigames.Casino.Casino.Types.Blackjack)
                                {
                                    Data.Minigames.Casino.Casino.ShowBlackjackButton(2, false);

                                    Data.Minigames.Casino.Casino.ShowBlackjackButton(0, false);
                                    Data.Minigames.Casino.Casino.ShowBlackjackButton(1, false);
                                }

                                updateFunc($"Игрок #{seatIdx + 1} берёт еще одну карту...");
                            }

                            table.StartDecisionPlayer(str, onLoad, 1);
                        }
                        else if (str[0] == 'L')
                        {
                            var subData = str.Split('*');

                            var seatIdx = byte.Parse(subData[1]);

                            if (CurrentTable == table && Data.Minigames.Casino.Casino.CurrentType == Minigames.Casino.Casino.Types.Blackjack)
                            {
                                Data.Minigames.Casino.Casino.ShowBlackjackButton(2, false);

                                Data.Minigames.Casino.Casino.ShowBlackjackButton(0, false);
                                Data.Minigames.Casino.Casino.ShowBlackjackButton(1, false);
                            }

                            if (CurrentTable == table && CurrentSeatIdx == seatIdx)
                            {
                                updateFunc("Вы выбываете из игры!");
                            }
                            else
                            {
                                updateFunc($"Игрок #{seatIdx + 1} выбывает из игры!");
                            }

                            var bets = table.NPC?.Ped?.GetData<List<BetData>>("Bets");

                            if (bets != null)
                            {
                                bets[seatIdx].MapObject?.Destroy();

                                bets[seatIdx].MapObject = null;
                                bets[seatIdx].Amount = 0;
                            }

                            table.StartDecisionPlayer(str, onLoad, 2);
                        }
                        else if (str[0] == 'S')
                        {
                            var time = long.Parse(str.Substring(1, str.IndexOf('*') - 1));

                            var timer = new Additional.ExtraTimer(async (obj) =>
                            {
                                await RAGE.Game.Invoker.WaitAsync(0);

                                updateFunc($"Игра начнётся через {DateTimeOffset.FromUnixTimeSeconds(time).DateTime.Subtract(Sync.World.ServerTime).GetBeautyString()}");
                            }, null, 0, 1000);

                            table.TextLabel.SetData("StateTask", timer);

                            var oBets = table.NPC.Ped.GetData<List<BetData>>("Bets");

                            if (oBets != null)
                            {
                                foreach (var x in oBets)
                                    x.MapObject?.Destroy();
                            }

                            var subData = str.Split('*');

                            var bets = subData.Skip(1).Select(x => new BetData() { Amount = decimal.Parse(x) }).ToList();

                            table.SpawnAllBets(bets);

                            table.NPC.Ped.SetData("Bets", bets);

                            if (CurrentTable == table && Data.Minigames.Casino.Casino.CurrentType == Minigames.Casino.Casino.Types.Blackjack)
                            {
                                Data.Minigames.Casino.Casino.ShowBlackjackButton(0, false);
                                Data.Minigames.Casino.Casino.ShowBlackjackButton(1, false);

                                Data.Minigames.Casino.Casino.ShowBlackjackButton(2, bets[CurrentSeatIdx].Amount <= 0);
                            }
                        }
                    }
                    else
                    {
                        updateFunc("");
                    }

                    void updateFunc(string str)
                    {
                        if (str == null)
                            str = "";

                        if (table.TextLabel != null)
                        {
                            table.TextLabel.Text = $"Мин. ставка: {Utils.ToStringWithWhitespace(table.MinBet.ToString())} фишек\nМакс. ставка: {Utils.ToStringWithWhitespace(table.MaxBet.ToString())} фишек\n\n{str}";
                        }

                        if (CurrentTable == table)
                        {
                            Data.Minigames.Casino.Casino.UpdateStatus(str);
                        }
                    }
                }

                public void StartGame(byte seatIdx)
                {
                    NPC.Ped.PlaySpeech("MINIGAME_DEALER_GREET", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);

                    CurrentTable = this;
                    CurrentSeatIdx = seatIdx;

                    if (TextLabel != null)
                    {
                        var color = TextLabel.Color;

                        color.Alpha = 0;

                        TextLabel.Color = color;

                        var tl = TextLabel.GetData<Additional.ExtraLabel>("Info");

                        if (tl != null)
                        {
                            color = tl.Color;

                            color.Alpha = 0;

                            tl.Color = color;
                        }
                    }

                    GameEvents.Render -= OnGameRender;
                    GameEvents.Render += OnGameRender;
                }

                public void StopGame()
                {
                    CurrentTable = null;
                    CurrentSeatIdx = 0;

                    if (TextLabel != null)
                    {
                        var color = TextLabel.Color;

                        color.Alpha = 255;

                        TextLabel.Color = color;

                        var tl = TextLabel.GetData<Additional.ExtraLabel>("Info");

                        if (tl != null)
                        {
                            color = tl.Color;

                            color.Alpha = 255;

                            tl.Color = color;
                        }
                    }

                    GameEvents.Render -= OnGameRender;
                }

                private async void FinishGame(string resStr, bool onLoad)
                {
                    await Utils.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_GENERAL", false, -1);

                    var npc = NPC?.Ped;

                    if (TableObject?.Exists != true || npc?.Exists != true)
                        return;

                    var strD = resStr.Split('*');

                    var dealerHand = strD[1].Split('!').Select(x => { var t = x.Split('-'); return new CardData() { CardType = (CardTypes)byte.Parse(t[0]), Value = byte.Parse(t[1]) }; }).ToList();

                    var playersHands = strD.Skip(2).Select(x => { return x.Length == 0 ? null : x.Split('!').Select(y => { var t = y.Split('-'); return new CardData() { CardType = (CardTypes)byte.Parse(t[0]), Value = byte.Parse(t[1]) }; }).ToList(); }).ToList();

                    var rDealerHand = npc.GetData<List<CardData>>("DHand");

                    var dealerNewCards = new List<int>();

                    dealerNewCards.AddRange(Enumerable.Range(0, dealerHand.Count));

                    if (rDealerHand != null)
                    {
                        for (int i = 0; i <  rDealerHand.Count; i++)
                        {
                            var x = rDealerHand[i];

                            x.MapObject?.Destroy();

                            x.MapObject = null;

                            dealerNewCards.Remove(i);
                        }

                        rDealerHand.Clear();
                    }

                    npc.SetData("DHand", dealerHand);

                    for (int i = 0; i < 4; i++)
                    {
                        var key = $"PHand{i}";

                        var rPlayerHand = npc.GetData<List<CardData>>(key);

                        if (rPlayerHand != null)
                        {
                            for (int j = 0; j < rPlayerHand.Count; j++)
                            {
                                var x = rPlayerHand[j];

                                x.MapObject?.Destroy();

                                x.MapObject = null;
                            }

                            rPlayerHand.Clear();
                        }

                        if (playersHands[i] != null)
                            npc.SetData(key, playersHands[i]);
                        else
                            npc.ResetData(key);
                    }

                    await SpawnAllCards(dealerHand, playersHands, byte.MaxValue);

                    var cardRot = dealerHand[1].MapObject.GetRotation(0);

                    dealerHand[1].MapObject.SetRotation(180f, cardRot.Y, cardRot.Z, 0, false);

                    dealerHand[1].MapObject.SetData("IsFlipped", true);

                    if (onLoad)
                        return;

                    foreach (var x in dealerNewCards)
                    {
                        if (dealerHand[x].MapObject?.Exists == true)
                        {
                            dealerHand[x].MapObject.SetAlpha(0, false);
                        }
                    }

                    npc.TaskPlayAnim("anim_casino_b@amb@casino@games@blackjack@dealer", "check_and_turn_card", 3f, 1f, -1, 2, 0f, false, false, false);

                    npc.PlayFacialAnim("check_and_turn_card", "anim_casino_b@amb@casino@games@blackjack@dealer");

                    await RAGE.Game.Invoker.WaitAsync(500);

                    dealerHand[1].MapObject.SetRotation(0f, cardRot.Y, cardRot.Z, 0, false);

                    dealerHand[1].MapObject.ResetData("IsFlipped");

                    foreach (var x in dealerNewCards)
                    {
                        if (dealerHand[x].MapObject?.Exists == true)
                        {
                            await DealerGiveSelfCard((byte)x, dealerHand[x].MapObject);
                        }
                    }

                    await RAGE.Game.Invoker.WaitAsync(2000); // anim_casino_b@amb@casino@games@shared@player@

                    npc.TaskPlayAnim("anim_casino_b@amb@casino@games@blackjack@dealer", "female_retrieve_all_cards", 3f, 1f, -1, 2, 0f, false, false, false);

                    npc.PlayFacialAnim("female_retrieve_all_cards", "anim_casino_b@amb@casino@games@blackjack@dealer");
                }

                private async void StartDecisionPlayer(string resStr, bool onLoad, byte type)
                {
                    await Utils.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_GENERAL", false, -1);

                    var npc = NPC?.Ped;

                    if (TableObject?.Exists != true || npc?.Exists != true)
                        return;

                    var strD = resStr.Split('*');

                    var seatIdx = byte.Parse(strD[1]);

                    if (type == 0)
                    {
                        DealerFocusTo(seatIdx, onLoad);
                    }
                    else if (type == 1)
                    {

                    }
                    else if (type == 2)
                    {
                        var anim = $"retrieve_cards_player_0{GetAnimSeatIdx(seatIdx)}";

                        npc.TaskPlayAnim("anim_casino_b@amb@casino@games@blackjack@dealer", anim, 3f, 1f, -1, 2, 0f, false, false, false);

                        npc.PlayFacialAnim(anim, "anim_casino_b@amb@casino@games@blackjack@dealer");
                    }

                    var dealerHand = strD[3].Split('!').Select(x => { var t = x.Split('-'); return new CardData() { CardType = (CardTypes)byte.Parse(t[0]), Value = byte.Parse(t[1]) }; }).ToList();

                    var playersHands = strD.Skip(4).Select(x => { return x.Length == 0 ? null : x.Split('!').Select(y => { var t = y.Split('-'); return new CardData() { CardType = (CardTypes)byte.Parse(t[0]), Value = byte.Parse(t[1]) }; }).ToList(); }).ToList();

                    var rDealerHand = npc.GetData<List<CardData>>("DHand");

                    var curPlayerNewCards = new List<int>();

                    if (playersHands[seatIdx] != null)
                        curPlayerNewCards.AddRange(Enumerable.Range(0, playersHands[seatIdx].Count));

                    if (rDealerHand != null)
                    {
                        foreach (var x in rDealerHand)
                        {
                            x.MapObject?.Destroy();

                            x.MapObject = null;
                        }

                        rDealerHand.Clear();
                    }

                    npc.SetData("DHand", dealerHand);

                    for (int i = 0; i < 4; i++)
                    {
                        var key = $"PHand{i}";

                        var rPlayerHand = npc.GetData<List<CardData>>(key);

                        if (rPlayerHand != null)
                        {
                            for (int j = 0; j < rPlayerHand.Count; j++)
                            {
                                var x = rPlayerHand[j];

                                x.MapObject?.Destroy();

                                x.MapObject = null;

                                if (i == seatIdx)
                                    curPlayerNewCards.Remove(j);
                            }

                            rPlayerHand.Clear();
                        }

                        if (playersHands[i] != null)
                            npc.SetData(key, playersHands[i]);
                        else
                            npc.ResetData(key);
                    }

                    await SpawnAllCards(dealerHand, playersHands, byte.MaxValue);

                    if (dealerHand[1].MapObject?.Exists == true)
                    {
                        var rot = dealerHand[1].MapObject.GetRotation(0);

                        dealerHand[1].MapObject.SetRotation(180f, rot.Y, rot.Z, 0, false);

                        dealerHand[1].MapObject.SetData("IsFlipped", true);
                    }

                    if (onLoad)
                        return;

                    if (type == 0)
                        npc.PlaySpeech("MINIGAME_BJACK_DEALER_ANOTHER_CARD", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);

                    var hand = playersHands[seatIdx];

                    if (hand != null)
                    {
                        foreach (var x in curPlayerNewCards)
                        {
                            if (hand[x].MapObject?.Exists == true)
                            {
                                hand[x].MapObject.SetAlpha(0, false);

                                await DealerGiveCard(seatIdx, hand[x].MapObject);
                            }
                        }
                    }
                }

                private async void StartCardGiving(string resStr, bool onLoad)
                {
                    await Utils.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_GENERAL", false, -1);

                    var npc = NPC?.Ped;

                    if (TableObject?.Exists != true || npc?.Exists != true)
                        return;

                    npc.PlaySpeech("MINIGAME_DEALER_CLOSED_BETS", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);

                    var strD = resStr.Split('*');

                    var dealerHand = strD[1].Split('!').Select(x => { var t = x.Split('-'); return new CardData() { CardType = (CardTypes)byte.Parse(t[0]), Value = byte.Parse(t[1]) }; }).ToList();

                    var playersHands = strD.Skip(2).Select(x => { return x.Length == 0 ? null : x.Split('!').Select(y => { var t = y.Split('-'); return new CardData() { CardType = (CardTypes)byte.Parse(t[0]), Value = byte.Parse(t[1]) }; }).ToList(); }).ToList();

                    npc.SetData("DHand", dealerHand);

                    for (int i = 0; i < playersHands.Count; i++)
                        npc.SetData($"PHand{i}", playersHands[i]);

                    await SpawnAllCards(dealerHand, playersHands, onLoad ? byte.MaxValue : byte.MinValue);

                    if (dealerHand[1].MapObject?.Exists == true)
                    {
                        var rot = dealerHand[1].MapObject.GetRotation(0);

                        dealerHand[1].MapObject.SetRotation(180f, rot.Y, rot.Z, 0, false);

                        dealerHand[1].MapObject.SetData("IsFlipped", true);
                    }

                    byte dealerHandSum = 0;

                    for (int i = 0; i < dealerHand.Count; i++)
                    {
                        dealerHandSum += dealerHand[i].Value;

                        if (onLoad)
                            continue;

                        if (i == 0)
                        {
                            await DealerGiveSelfCard(1, dealerHand[i].MapObject);

                            npc.PlaySpeech($"MINIGAME_BJACK_DEALER_{dealerHand[i].Value}", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);
                        }
                        else
                        {
                            await DealerGiveSelfCard(0, dealerHand[i].MapObject);
                        }
                    }

                    for (int j = 0; j < playersHands.Count; j++)
                    {
                        if (playersHands[j] == null)
                            continue;

                        byte handSum = 0;

                        for (int i = 0; i < playersHands[j].Count; i++)
                        {
                            handSum += playersHands[j][i].Value;

                            if (onLoad)
                                continue;

                            await DealerGiveCard((byte)j, playersHands[j][i].MapObject);
                        }

                        if (onLoad)
                            continue;

                        if (handSum > 0 && handSum < 21)
                        {
                            npc.PlaySpeech($"MINIGAME_BJACK_DEALER_{handSum}", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);
                        }
                        else if (handSum == 21)
                        {
                            npc.PlaySpeech("MINIGAME_BJACK_DEALER_BLACKJACK", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);
                        }
                    }

                    if (dealerHandSum == 21)
                    {
                        if (!onLoad)
                        {
                            npc.TaskPlayAnim("anim_casino_b@amb@casino@games@blackjack@dealer", "check_and_turn_card", 3f, 1f, -1, 2, 0f, false, false, false);

                            npc.PlayFacialAnim("check_and_turn_card", "anim_casino_b@amb@casino@games@blackjack@dealer");

                            await RAGE.Game.Invoker.WaitAsync(500);
                        }

                        var cardRot = dealerHand[1].MapObject.GetRotation(0);

                        dealerHand[1].MapObject.SetRotation(0f, cardRot.Y, cardRot.Z, 0, false);

                        dealerHand[1].MapObject.ResetData("IsFlipped");

                        npc.PlaySpeech("MINIGAME_BJACK_DEALER_BLACKJACK", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);
                    }
                    else if (dealerHand[0].Value == 10 || dealerHand[0].Value == 11)
                    {
                        npc.TaskPlayAnim("anim_casino_b@amb@casino@games@blackjack@dealer", "check_card", 3f, 1f, -1, 2, 0f, false, false, false);

                        npc.PlayFacialAnim("check_card", "anim_casino_b@amb@casino@games@blackjack@dealer");
                    }
                }

                public async void SpawnAllBets(List<BetData> bets)
                {
                    var npc = NPC?.Ped;

                    if (TableObject?.Exists != true || npc?.Exists != true)
                        return;

                    var tableHeading = TableObject.GetHeading();

                    for (int i = 0; i < bets.Count; i++)
                    {
                        var x = bets[i];

                        if (x.Amount <= 0)
                            continue;

                        var offsetInfo = BetOffsets[i][0];

                        var objModelStr = Casino.GetChipPropByAmount(x.Amount);

                        var objModelhash = RAGE.Util.Joaat.Hash(objModelStr);

                        await Utils.RequestModel(objModelhash);

                        var coords = TableObject.GetOffsetFromInWorldCoords(offsetInfo.X, offsetInfo.Y, offsetInfo.Z);

                        x.MapObject?.Destroy();

                        x.MapObject = new MapObject(RAGE.Game.Object.CreateObjectNoOffset(objModelhash, coords.X, coords.Y, coords.Z, false, false, false))
                        {
                            Dimension = uint.MaxValue,
                        };

                        x.MapObject.SetRotation(0f, 0f, tableHeading + offsetInfo.RotationZ, 0, false);
                    }
                }

                public async System.Threading.Tasks.Task SpawnAllCards(List<CardData> dealerHand, List<List<CardData>> playersHands, byte alpha = 255)
                {
                    var npc = NPC?.Ped;

                    if (TableObject?.Exists != true || npc?.Exists != true)
                        return;

                    var tableHeading = TableObject.GetHeading();

                    for (int i = 0; i < dealerHand.Count; i++)
                    {
                        var offsetInfo = CardOffsets[0][i == 0 ? 1 : i == 1 ? 0 : i];

                        var x = dealerHand[i];

                        var objModelStr = GetCardModelByType(x.CardType == CardTypes.None ? CardTypes.Club_Ace : x.CardType);

                        var objModelhash = RAGE.Util.Joaat.Hash(objModelStr);

                        await Utils.RequestModel(objModelhash);

                        var coords = TableObject.GetOffsetFromInWorldCoords(offsetInfo.X, offsetInfo.Y, offsetInfo.Z);

                        x.MapObject?.Destroy();

                        x.MapObject = new MapObject(RAGE.Game.Object.CreateObjectNoOffset(objModelhash, coords.X, coords.Y, coords.Z, false, false, false))
                        {
                            Dimension = uint.MaxValue,
                        };

                        x.MapObject.SetAlpha(alpha, false);

                        x.MapObject.SetRotation(0f, 0f, tableHeading + offsetInfo.RotationZ, 0, false);
                    }

                    for (int i = 0; i < playersHands.Count; i++)
                    {
                        var x = playersHands[i];

                        if (x == null)
                            continue;

                        for (int j = 0; j < x.Count; j++)
                        {
                            var y = x[j];

                            var offsetInfo = CardOffsets[i + 1][j];

                            var objModelStr = GetCardModelByType(y.CardType);

                            var objModelhash = RAGE.Util.Joaat.Hash(objModelStr);

                            await Utils.RequestModel(objModelhash);

                            var coords = TableObject.GetOffsetFromInWorldCoords(offsetInfo.X, offsetInfo.Y, offsetInfo.Z);

                            y.MapObject?.Destroy();

                            y.MapObject = new MapObject(RAGE.Game.Object.CreateObjectNoOffset(objModelhash, coords.X, coords.Y, coords.Z, false, false, false))
                            {
                                Dimension = uint.MaxValue,
                            };

                            y.MapObject.SetAlpha(alpha, false);

                            y.MapObject.SetRotation(0f, 0f, tableHeading + offsetInfo.RotationZ, 0, false);
                        }
                    }
                }

                public async System.Threading.Tasks.Task DealerGiveSelfCard(byte idx, MapObject cardObject)
                {
                    var npc = NPC?.Ped;

                    if (npc?.Exists != true)
                        return;

                    var cardAnim = "deal_card_self_card_10";

                    if (idx == 0)
                        cardAnim = "deal_card_self";
                    else if (idx == 1)
                        cardAnim = "deal_card_self_second_card";
                    else if (idx == 2)
                        cardAnim = "deal_card_self_card_06";

                    npc.TaskPlayAnim("anim_casino_b@amb@casino@games@blackjack@dealer", cardAnim, 3f, 1f, -1, 2, 0f, false, false, false);

                    npc.PlayFacialAnim($"{cardAnim}_facial", "anim_casino_b@amb@casino@games@blackjack@dealer");

                    await RAGE.Game.Invoker.WaitAsync(1000);

                    if (cardObject?.Exists == true)
                        cardObject.SetAlpha(255, false);

                    await RAGE.Game.Invoker.WaitAsync(250);
                }

                public async System.Threading.Tasks.Task DealerGiveCard(byte seatIdx, MapObject cardObject)
                {
                    var npc = NPC?.Ped;

                    if (npc?.Exists != true)
                        return;

                    var rSeatIdx = GetAnimSeatIdx(seatIdx);

                    var cardAnim = $"deal_card_player_0{rSeatIdx}";

                    npc.TaskPlayAnim("anim_casino_b@amb@casino@games@blackjack@dealer", cardAnim, 3f, 1f, -1, 2, 0f, false, false, false);

                    npc.PlayFacialAnim($"{cardAnim}_facial", "anim_casino_b@amb@casino@games@blackjack@dealer");

                    await RAGE.Game.Invoker.WaitAsync(1250);

                    if (cardObject?.Exists == true)
                        cardObject.SetAlpha(255, false);

                    await RAGE.Game.Invoker.WaitAsync(250);
                }

                public async System.Threading.Tasks.Task DealerHitCard(byte seatIdx, MapObject cardObject)
                {
                    var npc = NPC?.Ped;

                    if (npc?.Exists != true)
                        return;

                    var rSeatIdx = GetAnimSeatIdx(seatIdx);

                    var cardAnim = $"hit_card_player_0{rSeatIdx}";

                    npc.TaskPlayAnim("anim_casino_b@amb@casino@games@blackjack@dealer", cardAnim, 3f, 1f, -1, 2, 0f, false, false, false);

                    npc.PlayFacialAnim($"{cardAnim}_facial", "anim_casino_b@amb@casino@games@blackjack@dealer");

                    await RAGE.Game.Invoker.WaitAsync(1250);

                    if (cardObject?.Exists == true)
                        cardObject.SetAlpha(255, false);

                    await RAGE.Game.Invoker.WaitAsync(250);
                }

                public async void DealerFocusTo(byte seatIdx, bool onLoad)
                {
                    var npc = NPC?.Ped;

                    if (npc?.Exists != true)
                        return;

                    var rSeatIdx = GetAnimSeatIdx(seatIdx);

                    var anim = $"dealer_focus_player_0{rSeatIdx}_idle";

                    if (!onLoad)
                    {
                        var introAnim = $"dealer_focus_player_0{rSeatIdx}_idle_intro";

                        if (!npc.IsPlayingAnim("anim_casino_b@amb@casino@games@blackjack@dealer", anim, 3))
                        {
                            npc.TaskPlayAnim("anim_casino_b@amb@casino@games@blackjack@dealer", introAnim, 3f, 1f, -1, 2, 0f, false, false, false);

                            npc.PlayFacialAnim($"{introAnim}_facial", "anim_casino_b@amb@casino@games@blackjack@dealer");

                            await RAGE.Game.Invoker.WaitAsync(1500);
                        }
                    }

                    npc.TaskPlayAnim("anim_casino_b@amb@casino@games@blackjack@dealer", anim, 3f, 1f, -1, 2, 0f, false, false, false);

                    npc.PlayFacialAnim($"{anim}_facial", "anim_casino_b@amb@casino@games@blackjack@dealer");
                }

                public static string GetCardModelByType(CardTypes cType) => $"vw_prop_cas_card_{cType.ToString()}";

                public static byte GetAnimSeatIdx(byte seatIdx) => (byte)(4 - seatIdx);

                private void OnGameRender()
                {
                    var npc = NPC?.Ped;

                    if (npc?.Exists != true)
                        return;

                    var dealerHand = npc.GetData<List<CardData>>("DHand");

                    var pHand = npc.GetData<List<CardData>>($"PHand{CurrentSeatIdx}");

                    var playerSum = pHand == null ? 0 : pHand.Where(x => x.MapObject?.Exists == true && !x.MapObject.GetData<bool>("IsFlipped") && x.MapObject.GetAlpha() == 255).Select(x => (int)x.Value).Sum();

                    float x = 0f, y = 0f;

                    var dealerSum = 0;

                    if (dealerHand != null && dealerHand.Count > 0)
                    {
                        if (dealerHand[0].MapObject?.Exists == true)
                        {
                            var pos = dealerHand[0].MapObject.GetCoords(false);

                            if (Utils.GetScreenCoordFromWorldCoord(pos, ref x, ref y))
                            {
                                dealerSum = dealerHand.Where(x => x.MapObject?.Exists == true && !x.MapObject.GetData<bool>("IsFlipped") && x.MapObject.GetAlpha() == 255).Select(x => (int)x.Value).Sum();

                                if (dealerSum > 0)
                                {
                                    var text = $"{dealerSum}";

                                    if (dealerSum > 21 || (playerSum > dealerSum && dealerSum >= 17))
                                        Utils.DrawText(text, x, y, 255, 0, 0, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
                                    else if (dealerSum == 21)
                                        Utils.DrawText(text, x, y, 255, 215, 0, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
                                    else if (dealerSum > playerSum)
                                        Utils.DrawText(text, x, y, 0, 255, 0, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
                                    else
                                        Utils.DrawText(text, x, y, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
                                }
                            }
                        }
                    }

                    if (playerSum > 0)
                    {
                        if (pHand[0].MapObject?.Exists == true)
                        {
                            var pos = pHand[0].MapObject.GetCoords(false);

                            if (Utils.GetScreenCoordFromWorldCoord(pos, ref x, ref y))
                            {
                                var text = $"{playerSum}";

                                if (dealerSum <= 21)
                                {
                                    if (playerSum > 21 || (dealerSum > playerSum && dealerSum >= 17 && dealerSum <= 21))
                                        Utils.DrawText(text, x, y, 255, 0, 0, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
                                    else if (playerSum == 21)
                                        Utils.DrawText(text, x, y, 255, 215, 0, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
                                    else if (playerSum > dealerSum)
                                        Utils.DrawText(text, x, y, 0, 255, 0, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
                                    else
                                        Utils.DrawText(text, x, y, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
                                }
                                else
                                {
                                    Utils.DrawText(text, x, y, 0, 255, 0, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
                                }
                            }
                        }
                    }

                    var bets = npc.GetData<List<BetData>>("Bets");

                    if (bets != null)
                    {
                        var myBet = bets[CurrentSeatIdx];

                        if (myBet.Amount > 0 && myBet.MapObject?.Exists == true)
                        {
                            var pos = myBet.MapObject.GetCoords(false);

                            if (Utils.GetScreenCoordFromWorldCoord(pos, ref x, ref y))
                            {
                                Utils.DrawText($"{Utils.ToStringWithWhitespace(myBet.Amount.ToString())} фишек", x, y, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
                            }
                        }
                    }
                }
            }
        }
    }
}
