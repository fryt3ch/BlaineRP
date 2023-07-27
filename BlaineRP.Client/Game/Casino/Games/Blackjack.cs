using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Helpers;
using BlaineRP.Client.Game.Management.Animations;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Casino.Games
{
    public partial class Blackjack
    {
        public Blackjack(int CasinoId, int Id, string Model, float PosX, float PosY, float PosZ, float Heading)
        {
            TableObject = new MapObject(RAGE.Util.Joaat.Hash(Model), new Vector3(PosX, PosY, PosZ), new Vector3(0f, 0f, Heading), 255, Settings.App.Static.MainDimension)
            {
                NotifyStreaming = true, // h4_prop_casino_blckjack_01e
            };

            NPC = new NPCs.NPC($"Casino@Blackjack_{CasinoId}_{Id}",
                "",
                NPCs.NPC.Types.Static,
                "S_M_Y_Casino_01",
                RAGE.Game.Object.GetObjectOffsetFromCoords(PosX, PosY, PosZ, Heading, 0f, 0.7f, 1f),
                Heading + 180f,
                Settings.App.Static.MainDimension
            )
            {
                SubName = "NPC_SUBNAME_CASINO_BLACKJACK_WORKER",
            };
            NPC.Ped.StreamInCustomActionsAdd(OnPedStreamIn);
        }

        public static Blackjack CurrentTable { get; set; }

        public static byte CurrentSeatIdx { get; set; }

        public MapObject TableObject { get; set; }

        public NPCs.NPC NPC { get; set; }

        public ExtraLabel TextLabel { get; set; }

        public uint MinBet { get; set; }

        public uint MaxBet { get; set; }

        public string CurrentStateData { get; set; }

        public static Vector4 GetCardOffset(byte seatIdx, byte type, byte idx)
        {
            if (type == 0) // basic row
            {
                int maxNum = seatIdx == 0 ? CardOffsets[seatIdx].Length : 7;

                if (idx > maxNum)
                    return new Vector4(CardOffsets[seatIdx][maxNum].X,
                        CardOffsets[seatIdx][maxNum].Y,
                        CardOffsets[seatIdx][maxNum].Z + (idx - maxNum) * 0.0007f,
                        CardOffsets[seatIdx][maxNum].RotationZ
                    );

                return CardOffsets[seatIdx][idx];
            }
            else if (type == 1) // split row
            {
                if (seatIdx == 0)
                    return CardOffsets[seatIdx][0];

                int maxNum = CardOffsets[seatIdx].Length;

                idx += 7;

                if (idx > maxNum)
                    return new Vector4(CardOffsets[seatIdx][maxNum].X,
                        CardOffsets[seatIdx][maxNum].Y,
                        CardOffsets[seatIdx][maxNum].Z + (idx - maxNum) * 0.0007f,
                        CardOffsets[seatIdx][maxNum].RotationZ
                    );

                return CardOffsets[seatIdx][idx];
            }

            return new Vector4(0f, 0f, 0f, 0f);
        }

        private static void OnPedStreamIn(Entity entity)
        {
            var ped = entity as Ped;

            if (ped == null)
                return;

            int randomClothesNumber = Utils.Misc.Random.Next(0, 7);

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

            Core.Play(ped, new Animation("anim_casino_b@amb@casino@games@blackjack@dealer", "idle", 8f, 0f, -1, 0, 0f, true, true, true), -1);
        }

        public string GetCurrestStateString()
        {
            if (TextLabel == null || TextLabel.Text == null)
                return null;

            int indexStr = TextLabel.Text.IndexOf("\n\n");

            if (indexStr < 0)
                return null;

            return TextLabel.Text.Substring(indexStr + 2);
        }

        public static void OnCurrentStateDataUpdated(int casinoId, int tableId, string stateData, bool onLoad)
        {
            var casino = CasinoEntity.GetById(casinoId);

            if (!onLoad && (!casino.MainColshape.IsInside || AsyncTask.Methods.IsTaskStillPending("CASINO_TASK", null)))
                return;

            Blackjack table = casino.GetBlackjackById(tableId);

            table.CurrentStateData = stateData;

            if (table.TextLabel == null)
                return;

            AsyncTask stateTask = table.TextLabel.GetData<AsyncTask>("StateTask");

            if (stateTask != null)
            {
                stateTask.Cancel();

                table.TextLabel.ResetData("StateTask");
            }

            if (stateData is string str)
            {
                if (str[0] == 'I')
                {
                    var defText = "Ожидание первой ставки...";

                    updateFunc(defText);

                    if (CurrentTable == table)
                        if (CasinoMinigames.CurrentType == CasinoMinigames.Types.Blackjack)
                        {
                            CasinoMinigames.ShowBlackjackButton(0, false);
                            CasinoMinigames.ShowBlackjackButton(1, false);

                            CasinoMinigames.ShowBlackjackButton(2, true);
                        }

                    if (!onLoad)
                    {
                        List<BetData> bets = table.NPC?.Ped?.GetData<List<BetData>>("Bets");

                        if (bets != null)
                        {
                            foreach (BetData x in bets)
                            {
                                x.MapObject?.Destroy();
                            }

                            table.NPC.Ped.ResetData("Bets");
                        }

                        List<CardData> dealerHand = table.NPC?.Ped?.GetData<List<CardData>>("DHand");

                        if (dealerHand != null)
                        {
                            foreach (CardData x in dealerHand)
                            {
                                x.MapObject?.Destroy();
                            }

                            table.NPC.Ped.ResetData("DHand");
                        }

                        for (var i = 0; i < 4; i++)
                        {
                            var key = $"PHand{i}";

                            List<CardData> hand = table.NPC?.Ped?.GetData<List<CardData>>(key);

                            if (hand != null)
                            {
                                foreach (CardData x in hand)
                                {
                                    x.MapObject?.Destroy();
                                }

                                table.NPC.Ped.ResetData(key);
                            }
                        }
                    }
                }
                else if (str[0] == 'R')
                {
                    updateFunc("Раздача карт...");

                    if (CurrentTable == table && CasinoMinigames.CurrentType == CasinoMinigames.Types.Blackjack)
                    {
                        CasinoMinigames.ShowBlackjackButton(2, false);

                        CasinoMinigames.ShowBlackjackButton(0, false);
                        CasinoMinigames.ShowBlackjackButton(1, false);
                    }

                    table.StartCardGiving(casinoId, tableId, str, onLoad);
                }
                else if (str[0] == 'F')
                {
                    updateFunc("Завершение игры...");

                    if (CurrentTable == table && CasinoMinigames.CurrentType == CasinoMinigames.Types.Blackjack)
                    {
                        CasinoMinigames.ShowBlackjackButton(2, false);

                        CasinoMinigames.ShowBlackjackButton(0, false);
                        CasinoMinigames.ShowBlackjackButton(1, false);
                    }

                    table.FinishGame(casinoId, tableId, str, onLoad);
                }
                else if (str[0] == 'D')
                {
                    string[] subData = str.Split('*');

                    var seatIdx = byte.Parse(subData[1]);
                    var time = long.Parse(subData[2]);

                    if (CurrentTable == table && CurrentSeatIdx == seatIdx)
                    {
                        var task = new AsyncTask(() =>
                            {
                                updateFunc($"Ваш ход! ({DateTimeOffset.FromUnixTimeSeconds(time).DateTime.Subtract(World.Core.ServerTime).GetBeautyString()})");
                            },
                            1_000,
                            true,
                            0
                        );

                        task.Run();

                        table.TextLabel.SetData("StateTask", task);

                        if (CasinoMinigames.CurrentType == CasinoMinigames.Types.Blackjack)
                        {
                            CasinoMinigames.ShowBlackjackButton(2, false);

                            CasinoMinigames.ShowBlackjackButton(0, true);
                            CasinoMinigames.ShowBlackjackButton(1, true);
                        }
                    }
                    else
                    {
                        if (CurrentTable == table && CasinoMinigames.CurrentType == CasinoMinigames.Types.Blackjack)
                        {
                            CasinoMinigames.ShowBlackjackButton(2, false);

                            CasinoMinigames.ShowBlackjackButton(0, false);
                            CasinoMinigames.ShowBlackjackButton(1, false);
                        }

                        var task = new AsyncTask(() =>
                            {
                                updateFunc($"Ход игрока #{seatIdx + 1} ({DateTimeOffset.FromUnixTimeSeconds(time).DateTime.Subtract(World.Core.ServerTime).GetBeautyString()})");
                            },
                            1_000,
                            true,
                            0
                        );

                        task.Run();

                        table.TextLabel.SetData("StateTask", task);
                    }

                    table.NextStagePlayer(casinoId, tableId, str, onLoad, 0);
                }
                else if (str[0] == 'H')
                {
                    string[] subData = str.Split('*');

                    var seatIdx = byte.Parse(subData[1]);

                    if (CurrentTable == table && CurrentSeatIdx == seatIdx)
                    {
                        if (CasinoMinigames.CurrentType == CasinoMinigames.Types.Blackjack)
                        {
                            CasinoMinigames.ShowBlackjackButton(2, false);

                            CasinoMinigames.ShowBlackjackButton(0, false);
                            CasinoMinigames.ShowBlackjackButton(1, false);
                        }

                        updateFunc("Вы берёте еще одну карту...");
                    }
                    else
                    {
                        if (CurrentTable == table && CasinoMinigames.CurrentType == CasinoMinigames.Types.Blackjack)
                        {
                            CasinoMinigames.ShowBlackjackButton(2, false);

                            CasinoMinigames.ShowBlackjackButton(0, false);
                            CasinoMinigames.ShowBlackjackButton(1, false);
                        }

                        updateFunc($"Игрок #{seatIdx + 1} берёт еще одну карту...");
                    }

                    table.NextStagePlayer(casinoId, tableId, str, onLoad, 1);
                }
                else if (str[0] == 'L')
                {
                    string[] subData = str.Split('*');

                    var seatIdx = byte.Parse(subData[1]);

                    if (CurrentTable == table && CasinoMinigames.CurrentType == CasinoMinigames.Types.Blackjack)
                    {
                        CasinoMinigames.ShowBlackjackButton(2, false);

                        CasinoMinigames.ShowBlackjackButton(0, false);
                        CasinoMinigames.ShowBlackjackButton(1, false);
                    }

                    if (CurrentTable == table && CurrentSeatIdx == seatIdx)
                        updateFunc("Вы выбываете из игры!");
                    else
                        updateFunc($"Игрок #{seatIdx + 1} выбывает из игры!");

                    List<BetData> bets = table.NPC?.Ped?.GetData<List<BetData>>("Bets");

                    if (bets != null)
                    {
                        bets[seatIdx].MapObject?.Destroy();

                        bets[seatIdx].MapObject = null;
                        bets[seatIdx].Amount = 0;
                    }

                    table.NextStagePlayer(casinoId, tableId, str, onLoad, 2);
                }
                else if (str[0] == 'S')
                {
                    var time = long.Parse(str.Substring(1, str.IndexOf('*') - 1));

                    var task = new AsyncTask(() =>
                        {
                            updateFunc($"Игра начнётся через {DateTimeOffset.FromUnixTimeSeconds(time).DateTime.Subtract(World.Core.ServerTime).GetBeautyString()}");
                        },
                        1_000,
                        true,
                        0
                    );

                    task.Run();

                    table.TextLabel.SetData("StateTask", task);

                    if (CurrentTable == table && CasinoMinigames.CurrentType == CasinoMinigames.Types.Blackjack)
                    {
                        List<BetData> oBets = table.NPC.Ped.GetData<List<BetData>>("Bets");

                        CasinoMinigames.ShowBlackjackButton(0, false);
                        CasinoMinigames.ShowBlackjackButton(1, false);

                        CasinoMinigames.ShowBlackjackButton(2, (oBets?[CurrentSeatIdx]?.Amount ?? 0) <= 0);
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
                    table.TextLabel.Text = $"Мин. ставка: {Locale.Get("GEN_CHIPS_0", table.MinBet)}\nМакс. ставка: {Locale.Get("GEN_CHIPS_0", table.MaxBet)}\n\n{str}";

                if (CurrentTable == table)
                    CasinoMinigames.UpdateStatus(str);
            }
        }

        public void StartGame(byte seatIdx)
        {
            NPC.Ped.PlaySpeech("MINIGAME_DEALER_GREET", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);

            CurrentTable = this;
            CurrentSeatIdx = seatIdx;

            if (TextLabel != null)
            {
                RGBA color = TextLabel.Color;

                color.Alpha = 0;

                TextLabel.Color = color;

                ExtraLabel tl = TextLabel.GetData<ExtraLabel>("Info");

                if (tl != null)
                {
                    color = tl.Color;

                    color.Alpha = 0;

                    tl.Color = color;
                }
            }

            Main.Render -= OnGameRender;
            Main.Render += OnGameRender;
        }

        public void StopGame()
        {
            CurrentTable = null;
            CurrentSeatIdx = 0;

            if (TextLabel != null)
            {
                RGBA color = TextLabel.Color;

                color.Alpha = 255;

                TextLabel.Color = color;

                ExtraLabel tl = TextLabel.GetData<ExtraLabel>("Info");

                if (tl != null)
                {
                    color = tl.Color;

                    color.Alpha = 255;

                    tl.Color = color;
                }
            }

            Main.Render -= OnGameRender;
        }

        private void FinishGame(int casinoId, int id, string resStr, bool onLoad)
        {
            var taskKey = $"CASINO_BLJ_F_{casinoId}_{id}";

            AsyncTask task = null;

            task = new AsyncTask(async () =>
                {
                    await Utils.Game.Audio.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_GENERAL", false, -1);

                    Ped npc = NPC?.Ped;

                    if (TableObject?.Exists != true || npc?.Exists != true || !AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                        return;

                    string[] strD = resStr.Split('*');

                    var dealerHand = strD[1]
                                    .Split('!')
                                    .Select(x =>
                                         {
                                             string[] t = x.Split('-');
                                             return new CardData()
                                             {
                                                 CardType = (CardType)byte.Parse(t[0]),
                                                 Value = byte.Parse(t[1]),
                                             };
                                         }
                                     )
                                    .ToList();

                    var playersHands = strD.Skip(2)
                                           .Select(x =>
                                                {
                                                    return x.Length == 0
                                                        ? null
                                                        : x.Split('!')
                                                           .Select(y =>
                                                                {
                                                                    string[] t = y.Split('-');
                                                                    return new CardData()
                                                                    {
                                                                        CardType = (CardType)byte.Parse(t[0]),
                                                                        Value = byte.Parse(t[1]),
                                                                    };
                                                                }
                                                            )
                                                           .ToList();
                                                }
                                            )
                                           .ToList();

                    List<CardData> rDealerHand = npc.GetData<List<CardData>>("DHand");

                    var dealerNewCards = new List<int>();

                    dealerNewCards.AddRange(Enumerable.Range(0, dealerHand.Count));

                    if (rDealerHand != null)
                        for (var i = 0; i < rDealerHand.Count; i++)
                        {
                            CardData x = rDealerHand[i];

                            x.MapObject?.Destroy();

                            x.MapObject = null;

                            dealerNewCards.Remove(i);
                        }

                    npc.SetData("DHand", dealerHand);

                    for (var i = 0; i < 4; i++)
                    {
                        var key = $"PHand{i}";

                        List<CardData> rPlayerHand = npc.GetData<List<CardData>>(key);

                        if (rPlayerHand != null)
                            for (var j = 0; j < rPlayerHand.Count; j++)
                            {
                                CardData x = rPlayerHand[j];

                                x.MapObject?.Destroy();

                                x.MapObject = null;
                            }

                        if (playersHands[i] != null)
                            npc.SetData(key, playersHands[i]);
                        else
                            npc.ResetData(key);
                    }

                    await SpawnAllCards(taskKey, task, dealerHand, playersHands, byte.MaxValue);

                    if (TableObject?.Exists != true || npc?.Exists != true || !AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                        return;

                    Vector3 cardRot = dealerHand[1].MapObject.GetRotation(0);

                    dealerHand[1].MapObject.SetRotation(180f, cardRot.Y, cardRot.Z, 0, false);

                    dealerHand[1].MapObject.SetData("IsFlipped", true);

                    if (onLoad)
                        return;

                    foreach (int x in dealerNewCards)
                    {
                        if (dealerHand[x].MapObject?.Exists == true)
                            dealerHand[x].MapObject.SetAlpha(0, false);
                    }

                    npc.TaskPlayAnim("anim_casino_b@amb@casino@games@blackjack@dealer", "check_and_turn_card", 3f, 1f, -1, 2, 0f, false, false, false);

                    npc.PlayFacialAnim("check_and_turn_card", "anim_casino_b@amb@casino@games@blackjack@dealer");

                    await RAGE.Game.Invoker.WaitAsync(500);

                    if (TableObject?.Exists != true || npc?.Exists != true || !AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                        return;

                    dealerHand[1].MapObject.SetRotation(0f, cardRot.Y, cardRot.Z, 0, false);

                    dealerHand[1].MapObject.ResetData("IsFlipped");

                    foreach (int x in dealerNewCards)
                    {
                        if (dealerHand[x].MapObject?.Exists == true)
                        {
                            await DealerGiveSelfCard((byte)x, dealerHand[x].MapObject);

                            if (TableObject?.Exists != true || npc?.Exists != true || !AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                                return;
                        }
                    }

                    await RAGE.Game.Invoker.WaitAsync(2000);

                    if (TableObject?.Exists != true || npc?.Exists != true || !AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                        return;

                    npc.TaskPlayAnim("anim_casino_b@amb@casino@games@blackjack@dealer", "female_retrieve_all_cards", 3f, 1f, -1, 2, 0f, false, false, false);

                    npc.PlayFacialAnim("female_retrieve_all_cards", "anim_casino_b@amb@casino@games@blackjack@dealer");

                    AsyncTask.Methods.CancelPendingTask(taskKey);
                },
                0,
                false,
                0
            );

            AsyncTask.Methods.SetAsPending(task, taskKey);
        }

        private void NextStagePlayer(int casinoId, int id, string resStr, bool onLoad, byte type)
        {
            var taskKey = $"CASINO_BLJ_P_{casinoId}_{id}";

            AsyncTask task = null;

            task = new AsyncTask(async () =>
                {
                    await Utils.Game.Audio.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_GENERAL", false, -1);

                    Ped npc = NPC?.Ped;

                    if (TableObject?.Exists != true || npc?.Exists != true || !AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                        return;

                    string[] strD = resStr.Split('*');

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

                    var dealerHand = strD[3]
                                    .Split('!')
                                    .Select(x =>
                                         {
                                             string[] t = x.Split('-');
                                             return new CardData()
                                             {
                                                 CardType = (CardType)byte.Parse(t[0]),
                                                 Value = byte.Parse(t[1]),
                                             };
                                         }
                                     )
                                    .ToList();

                    var playersHands = strD.Skip(4)
                                           .Select(x =>
                                                {
                                                    return x.Length == 0
                                                        ? null
                                                        : x.Split('!')
                                                           .Select(y =>
                                                                {
                                                                    string[] t = y.Split('-');
                                                                    return new CardData()
                                                                    {
                                                                        CardType = (CardType)byte.Parse(t[0]),
                                                                        Value = byte.Parse(t[1]),
                                                                    };
                                                                }
                                                            )
                                                           .ToList();
                                                }
                                            )
                                           .ToList();

                    List<CardData> rDealerHand = npc.GetData<List<CardData>>("DHand");

                    var curPlayerNewCards = new List<int>();

                    if (playersHands[seatIdx] != null)
                        curPlayerNewCards.AddRange(Enumerable.Range(0, playersHands[seatIdx].Count));

                    if (rDealerHand != null)
                        foreach (CardData x in rDealerHand)
                        {
                            x.MapObject?.Destroy();

                            x.MapObject = null;
                        }

                    npc.SetData("DHand", dealerHand);

                    for (var i = 0; i < 4; i++)
                    {
                        var key = $"PHand{i}";

                        List<CardData> rPlayerHand = npc.GetData<List<CardData>>(key);

                        if (rPlayerHand != null)
                            for (var j = 0; j < rPlayerHand.Count; j++)
                            {
                                CardData x = rPlayerHand[j];

                                x.MapObject?.Destroy();

                                x.MapObject = null;

                                if (i == seatIdx)
                                    curPlayerNewCards.Remove(j);
                            }

                        if (playersHands[i] != null)
                            npc.SetData(key, playersHands[i]);
                        else
                            npc.ResetData(key);
                    }

                    await SpawnAllCards(taskKey, task, dealerHand, playersHands, byte.MaxValue);

                    if (TableObject?.Exists != true || npc?.Exists != true || !AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                        return;

                    if (dealerHand[1].MapObject?.Exists == true)
                    {
                        Vector3 rot = dealerHand[1].MapObject.GetRotation(0);

                        dealerHand[1].MapObject.SetRotation(180f, rot.Y, rot.Z, 0, false);

                        dealerHand[1].MapObject.SetData("IsFlipped", true);
                    }

                    if (onLoad)
                        return;

                    if (type == 0)
                        npc.PlaySpeech("MINIGAME_BJACK_DEALER_ANOTHER_CARD", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);

                    List<CardData> hand = playersHands[seatIdx];

                    if (hand != null)
                        foreach (int x in curPlayerNewCards)
                        {
                            if (hand[x].MapObject?.Exists == true)
                            {
                                hand[x].MapObject.SetAlpha(0, false);

                                await DealerGiveCard(seatIdx, hand[x].MapObject);

                                if (TableObject?.Exists != true || npc?.Exists != true || !AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                                    return;
                            }
                        }

                    AsyncTask.Methods.CancelPendingTask(taskKey);
                },
                0,
                false,
                0
            );

            AsyncTask.Methods.SetAsPending(task, taskKey);
        }

        private void StartCardGiving(int casinoId, int id, string resStr, bool onLoad)
        {
            var taskKey = $"CASINO_BLJ_S_{casinoId}_{id}";

            AsyncTask task = null;

            task = new AsyncTask(async () =>
                {
                    await Utils.Game.Audio.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_GENERAL", false, -1);

                    Ped npc = NPC?.Ped;

                    if (TableObject?.Exists != true || npc?.Exists != true || !AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                        return;

                    npc.PlaySpeech("MINIGAME_DEALER_CLOSED_BETS", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);

                    string[] strD = resStr.Split('*');

                    var dealerHand = strD[1]
                                    .Split('!')
                                    .Select(x =>
                                         {
                                             string[] t = x.Split('-');
                                             return new CardData()
                                             {
                                                 CardType = (CardType)byte.Parse(t[0]),
                                                 Value = byte.Parse(t[1]),
                                             };
                                         }
                                     )
                                    .ToList();

                    var playersHands = strD.Skip(2)
                                           .Select(x =>
                                                {
                                                    return x.Length == 0
                                                        ? null
                                                        : x.Split('!')
                                                           .Select(y =>
                                                                {
                                                                    string[] t = y.Split('-');
                                                                    return new CardData()
                                                                    {
                                                                        CardType = (CardType)byte.Parse(t[0]),
                                                                        Value = byte.Parse(t[1]),
                                                                    };
                                                                }
                                                            )
                                                           .ToList();
                                                }
                                            )
                                           .ToList();

                    npc.SetData("DHand", dealerHand);

                    for (var i = 0; i < playersHands.Count; i++)
                    {
                        npc.SetData($"PHand{i}", playersHands[i]);
                    }

                    await SpawnAllCards(taskKey, task, dealerHand, playersHands, onLoad ? byte.MaxValue : byte.MinValue);

                    if (TableObject?.Exists != true || npc?.Exists != true || !AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                        return;

                    if (dealerHand[1].MapObject?.Exists == true)
                    {
                        Vector3 rot = dealerHand[1].MapObject.GetRotation(0);

                        dealerHand[1].MapObject.SetRotation(180f, rot.Y, rot.Z, 0, false);

                        dealerHand[1].MapObject.SetData("IsFlipped", true);
                    }

                    byte dealerHandSum = 0;

                    for (var i = 0; i < dealerHand.Count; i++)
                    {
                        dealerHandSum += dealerHand[i].Value;

                        if (onLoad)
                            continue;

                        if (i == 0)
                        {
                            await DealerGiveSelfCard(1, dealerHand[i].MapObject);

                            if (TableObject?.Exists != true || npc?.Exists != true || !AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                                return;

                            npc.PlaySpeech($"MINIGAME_BJACK_DEALER_{dealerHand[i].Value}", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);
                        }
                        else
                        {
                            await DealerGiveSelfCard(0, dealerHand[i].MapObject);

                            if (TableObject?.Exists != true || npc?.Exists != true || !AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                                return;
                        }
                    }

                    for (var j = 0; j < playersHands.Count; j++)
                    {
                        if (playersHands[j] == null)
                            continue;

                        byte handSum = 0;

                        for (var i = 0; i < playersHands[j].Count; i++)
                        {
                            handSum += playersHands[j][i].Value;

                            if (onLoad)
                                continue;

                            await DealerGiveCard((byte)j, playersHands[j][i].MapObject);

                            if (TableObject?.Exists != true || npc?.Exists != true || !AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                                return;
                        }

                        if (onLoad)
                            continue;

                        if (handSum > 0 && handSum < BLACKJACK_ON)
                            npc.PlaySpeech($"MINIGAME_BJACK_DEALER_{handSum}", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);
                        else if (handSum == BLACKJACK_ON)
                            npc.PlaySpeech("MINIGAME_BJACK_DEALER_BLACKJACK", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);
                    }

                    if (dealerHandSum == BLACKJACK_ON)
                    {
                        if (!onLoad)
                        {
                            npc.TaskPlayAnim("anim_casino_b@amb@casino@games@blackjack@dealer", "check_and_turn_card", 3f, 1f, -1, 2, 0f, false, false, false);

                            npc.PlayFacialAnim("check_and_turn_card", "anim_casino_b@amb@casino@games@blackjack@dealer");

                            await RAGE.Game.Invoker.WaitAsync(500);

                            if (TableObject?.Exists != true || npc?.Exists != true || !AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                                return;
                        }

                        Vector3 cardRot = dealerHand[1].MapObject.GetRotation(0);

                        dealerHand[1].MapObject.SetRotation(0f, cardRot.Y, cardRot.Z, 0, false);

                        dealerHand[1].MapObject.ResetData("IsFlipped");

                        if (!onLoad)
                        {
                            npc.PlaySpeech("MINIGAME_BJACK_DEALER_BLACKJACK", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);

                            await RAGE.Game.Invoker.WaitAsync(750);

                            if (TableObject?.Exists != true || npc?.Exists != true || !AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                                return;

                            npc.TaskPlayAnim("anim_casino_b@amb@casino@games@blackjack@dealer", "female_retrieve_all_cards", 3f, 1f, -1, 2, 0f, false, false, false);

                            npc.PlayFacialAnim("female_retrieve_all_cards", "anim_casino_b@amb@casino@games@blackjack@dealer");
                        }
                    }
                    else if (dealerHand[0].Value == 10 || dealerHand[0].Value == 11)
                    {
                        if (!onLoad)
                        {
                            npc.TaskPlayAnim("anim_casino_b@amb@casino@games@blackjack@dealer", "check_card", 3f, 1f, -1, 2, 0f, false, false, false);

                            npc.PlayFacialAnim("check_card", "anim_casino_b@amb@casino@games@blackjack@dealer");
                        }
                    }

                    AsyncTask.Methods.CancelPendingTask(taskKey);
                },
                0,
                false,
                0
            );

            AsyncTask.Methods.SetAsPending(task, taskKey);
        }

        public async System.Threading.Tasks.Task SpawnAllCards(string taskKey, AsyncTask task, List<CardData> dealerHand, List<List<CardData>> playersHands, byte alpha = 255)
        {
            Ped npc = NPC?.Ped;

            if (TableObject?.Exists != true || npc?.Exists != true)
                return;

            float tableHeading = TableObject.GetHeading();

            for (var i = 0; i < dealerHand.Count; i++)
            {
                Vector4 offsetInfo = GetCardOffset(0, 0, (byte)(i == 0 ? 1 : i == 1 ? 0 : i));

                CardData x = dealerHand[i];

                string objModelStr = GetCardModelByType(x.CardType == CardType.None ? CardType.Club_Ace : x.CardType);

                uint objModelhash = RAGE.Util.Joaat.Hash(objModelStr);

                await Streaming.RequestModel(objModelhash);

                if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                    return;

                Vector3 coords = TableObject.GetOffsetFromInWorldCoords(offsetInfo.X, offsetInfo.Y, offsetInfo.Z);

                x.MapObject?.Destroy();

                x.MapObject = new MapObject(RAGE.Game.Object.CreateObjectNoOffset(objModelhash, coords.X, coords.Y, coords.Z, false, false, false))
                {
                    Dimension = uint.MaxValue,
                };

                x.MapObject.SetAlpha(alpha, false);

                x.MapObject.SetRotation(0f, 0f, tableHeading + offsetInfo.RotationZ, 0, false);
            }

            for (var i = 0; i < playersHands.Count; i++)
            {
                List<CardData> x = playersHands[i];

                if (x == null)
                    continue;

                for (var j = 0; j < x.Count; j++)
                {
                    CardData y = x[j];

                    Vector4 offsetInfo = GetCardOffset((byte)(i + 1), 0, (byte)j);

                    string objModelStr = GetCardModelByType(y.CardType);

                    uint objModelhash = RAGE.Util.Joaat.Hash(objModelStr);

                    await Streaming.RequestModel(objModelhash);

                    if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                        return;

                    Vector3 coords = TableObject.GetOffsetFromInWorldCoords(offsetInfo.X, offsetInfo.Y, offsetInfo.Z);

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
            Ped npc = NPC?.Ped;

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
            Ped npc = NPC?.Ped;

            if (npc?.Exists != true)
                return;

            byte rSeatIdx = GetAnimSeatIdx(seatIdx);

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
            Ped npc = NPC?.Ped;

            if (npc?.Exists != true)
                return;

            byte rSeatIdx = GetAnimSeatIdx(seatIdx);

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
            Ped npc = NPC?.Ped;

            if (npc?.Exists != true)
                return;

            byte rSeatIdx = GetAnimSeatIdx(seatIdx);

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

        public static string GetCardModelByType(CardType cType)
        {
            return $"vw_prop_cas_card_{cType.ToString()}";
        }

        public static byte GetAnimSeatIdx(byte seatIdx)
        {
            return (byte)(4 - seatIdx);
        }

        private void OnGameRender()
        {
            Ped npc = NPC?.Ped;

            if (npc?.Exists != true)
                return;

            List<CardData> dealerHand = npc.GetData<List<CardData>>("DHand");

            List<CardData> pHand = npc.GetData<List<CardData>>($"PHand{CurrentSeatIdx}");

            int playerSum = pHand == null
                ? 0
                : pHand.Where(x => x.MapObject?.Exists == true && !x.MapObject.GetData<bool>("IsFlipped") && x.MapObject.GetAlpha() == 255).Select(x => (int)x.Value).Sum();

            float x = 0f, y = 0f;

            var dealerSum = 0;

            if (dealerHand != null && dealerHand.Count > 0)
                if (dealerHand[0].MapObject?.Exists == true)
                {
                    Vector3 pos = dealerHand[0].MapObject.GetCoords(false);

                    if (Graphics.GetScreenCoordFromWorldCoord(pos, ref x, ref y))
                    {
                        dealerSum = dealerHand.Where(x => x.MapObject?.Exists == true && !x.MapObject.GetData<bool>("IsFlipped") && x.MapObject.GetAlpha() == 255)
                                              .Select(x => (int)x.Value)
                                              .Sum();

                        if (dealerSum > 0)
                        {
                            var text = $"{dealerSum}";

                            if (dealerSum > LOOSE_AFTER || playerSum > dealerSum && dealerSum >= DEALER_STOPS_ON)
                                Graphics.DrawText(text, x, y, 255, 0, 0, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                            else if (dealerSum == BLACKJACK_ON)
                                Graphics.DrawText(text, x, y, 255, 215, 0, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                            else if (dealerSum > playerSum)
                                Graphics.DrawText(text, x, y, 0, 255, 0, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                            else
                                Graphics.DrawText(text, x, y, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                        }
                    }
                }

            if (playerSum > 0)
                if (pHand[0].MapObject?.Exists == true)
                {
                    Vector3 pos = pHand[0].MapObject.GetCoords(false);

                    if (Graphics.GetScreenCoordFromWorldCoord(pos, ref x, ref y))
                    {
                        var text = $"{playerSum}";

                        if (dealerSum <= LOOSE_AFTER)
                        {
                            if (playerSum > LOOSE_AFTER || dealerSum > playerSum && dealerSum >= DEALER_STOPS_ON && dealerSum <= LOOSE_AFTER)
                                Graphics.DrawText(text, x, y, 255, 0, 0, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                            else if (playerSum == BLACKJACK_ON)
                                Graphics.DrawText(text, x, y, 255, 215, 0, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                            else if (playerSum > dealerSum)
                                Graphics.DrawText(text, x, y, 0, 255, 0, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                            else
                                Graphics.DrawText(text, x, y, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                        }
                        else
                        {
                            Graphics.DrawText(text, x, y, 0, 255, 0, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                        }
                    }
                }

            List<BetData> bets = npc.GetData<List<BetData>>("Bets");

            if (bets != null)
            {
                BetData myBet = bets[CurrentSeatIdx];

                if (myBet.Amount > 0 && myBet.MapObject?.Exists == true)
                {
                    Vector3 pos = myBet.MapObject.GetCoords(false);

                    if (Graphics.GetScreenCoordFromWorldCoord(pos, ref x, ref y))
                        Graphics.DrawText($"{Locale.Get("GEN_CHIPS_0", myBet.Amount)}", x, y, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                }
            }
        }

        public static async System.Threading.Tasks.Task LoadAllRequired()
        {
            await Utils.Game.Audio.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_GENERAL", false, -1);

            await Streaming.RequestAnimDict("anim_casino_b@amb@casino@games@blackjack@dealer");

            var allCards = (CardType[])Enum.GetValues(typeof(CardType));

            for (var i = 0; i < allCards.Length; i++)
            {
                await Streaming.RequestModel(RAGE.Util.Joaat.Hash(GetCardModelByType(allCards[i])));
            }
        }
    }
}