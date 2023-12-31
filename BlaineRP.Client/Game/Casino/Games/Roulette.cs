﻿using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Animations;
using BlaineRP.Client.Game.Helpers;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Casino.Games
{
    public partial class Roulette
    {
        public Roulette(int CasinoId, int Id, string Model, float PosX, float PosY, float PosZ, float RotZ)
        {
            TableObject = new MapObject(RAGE.Util.Joaat.Hash(Model), new Vector3(PosX, PosY, PosZ), new Vector3(0f, 0f, RotZ), 255, Settings.App.Static.MainDimension)
            {
                NotifyStreaming = true,
            };

            NPC = new NPCs.NPC($"Casino@Roulette_{CasinoId}_{Id}",
                "",
                NPCs.NPC.Types.Static,
                "S_F_Y_Casino_01",
                RAGE.Game.Object.GetObjectOffsetFromCoords(PosX, PosY, PosZ, RotZ, 0f, 0.7f, 1f),
                RotZ + 180f,
                Settings.App.Static.MainDimension
            )
            {
                SubName = "NPC_SUBNAME_CASINO_ROULETTE_WORKER",
            };
            NPC.Ped.StreamInCustomActionsAdd(OnPedStreamIn);
        }

        public static Roulette CurrentRoulette { get; set; }

        public static Dictionary<BetType, HoverData> HoverDatas { get; set; }

        private static List<MapObject> HoverObjects { get; set; }

        public List<BetData> ActiveBets { get; set; }

        public static BetType HoveredBet { get; set; }

        public MapObject TableObject { get; set; }

        public MapObject BallObject { get; set; }

        public NPCs.NPC NPC { get; set; }

        public uint MinBet { get; set; }
        public uint MaxBet { get; set; }

        public List<BetType> LastBets { get; set; }

        public ExtraLabel TextLabel { get; set; }

        public string CurrentStateData { get; set; }

        public int GetIdInCasino(int casino)
        {
            return CasinoEntity.GetById(casino)?.Roulettes is Roulette[] t ? Array.IndexOf(t, this) : -1;
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

        public static void OnCurrentStateDataUpdated(int casinoId, int rouletteId, string stateData, bool onLoad)
        {
            var casino = CasinoEntity.GetById(casinoId);

            if (!onLoad && (!casino.MainColshape.IsInside || AsyncTask.Methods.IsTaskStillPending("CASINO_TASK", null)))
                return;

            Roulette roulette = casino.GetRouletteById(rouletteId);

            roulette.CurrentStateData = stateData;

            if (roulette.TextLabel == null)
                return;

            AsyncTask stateTask = roulette.TextLabel.GetData<AsyncTask>("StateTask");

            if (stateTask != null)
            {
                stateTask.Cancel();

                roulette.TextLabel.ResetData("StateTask");
            }

            if (stateData is string str)
            {
                if (str[0] == 'I')
                {
                    var defText = "Ожидание первой ставки...";

                    if (str.Length > 1)
                    {
                        var lastBallRes = byte.Parse(str.Substring(1));

                        var betType = (BetType)lastBallRes;

                        if (!onLoad)
                        {
                            updateFunc($"Выпало число {betType.ToString().Replace("_", "")}!");

                            var task = new AsyncTask(() =>
                                {
                                    updateFunc(defText);
                                },
                                2_500,
                                false,
                                0
                            );

                            task.Run();

                            roulette.TextLabel.SetData("StateTask", task);

                            roulette.LastBets?.Add((BetType)lastBallRes);

                            if (CurrentRoulette == roulette)
                                CasinoMinigames.AddLastBet(betType);

                            if (roulette.ActiveBets != null)
                            {
                                foreach (BetData x in roulette.ActiveBets)
                                {
                                    x.MapObject?.Destroy();
                                }

                                roulette.ActiveBets.Clear();

                                roulette.ActiveBets = null;
                            }
                        }
                        else
                        {
                            updateFunc(defText);
                        }
                    }
                    else
                    {
                        updateFunc(defText);
                    }
                }

                if (str[0] == 'R')
                {
                    var ballRes = byte.Parse(str.Substring(1));

                    if (!onLoad)
                        roulette.Spin(casinoId, rouletteId, ballRes);

                    updateFunc("Ожидание результата игры...");
                }
                else if (str[0] == 'S')
                {
                    var time = long.Parse(str.Substring(1));

                    var task = new AsyncTask(() =>
                        {
                            updateFunc($"Игра начнётся через {DateTimeOffset.FromUnixTimeSeconds(time).DateTime.Subtract(World.Core.ServerTime).GetBeautyString()}");
                        },
                        1_000,
                        true,
                        0
                    );

                    task.Run();

                    roulette.TextLabel.SetData("StateTask", task);
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

                if (roulette.TextLabel != null)
                    roulette.TextLabel.Text = $"Мин. ставка: {Locale.Get("GEN_CHIPS_0", roulette.MinBet)}nМакс. ставка: {Locale.Get("GEN_CHIPS_0", roulette.MaxBet)}\n\n{str}";

                if (CurrentRoulette == roulette)
                    CasinoMinigames.UpdateStatus(str);
            }
        }

        private static void OnPedStreamIn(Entity entity)
        {
            var ped = entity as Ped;

            if (ped == null)
                return;

            ped.SetDefaultComponentVariation();

            int randomClothesNumber = Utils.Misc.Random.Next(0, 7);

            if (randomClothesNumber == 0)
            {
                ped.SetComponentVariation(0, 4, 0, 0);
                ped.SetComponentVariation(1, 0, 0, 0);
                ped.SetComponentVariation(2, 4, 0, 0);
                ped.SetComponentVariation(3, 2, 1, 0);
                ped.SetComponentVariation(4, 1, 0, 0);
                ped.SetComponentVariation(6, 1, 0, 0);
                ped.SetComponentVariation(7, 1, 0, 0);
                ped.SetComponentVariation(8, 2, 0, 0);
                ped.SetComponentVariation(10, 0, 0, 0);
                ped.SetComponentVariation(11, 0, 0, 0);

                ped.SetPropIndex(1, 0, 0, false);
            }
            else if (randomClothesNumber == 1)
            {
                ped.SetComponentVariation(0, 3, 1, 0);
                ped.SetComponentVariation(1, 0, 0, 0);
                ped.SetComponentVariation(2, 3, 1, 0);
                ped.SetComponentVariation(3, 1, 1, 0);
                ped.SetComponentVariation(4, 1, 0, 0);
                ped.SetComponentVariation(6, 1, 0, 0);
                ped.SetComponentVariation(7, 2, 0, 0);
                ped.SetComponentVariation(8, 1, 0, 0);
                ped.SetComponentVariation(10, 0, 0, 0);
                ped.SetComponentVariation(11, 0, 0, 0);
            }
            else if (randomClothesNumber == 2)
            {
                ped.SetComponentVariation(0, 3, 0, 0);
                ped.SetComponentVariation(1, 0, 0, 0);
                ped.SetComponentVariation(2, 3, 0, 0);
                ped.SetComponentVariation(3, 0, 1, 0);
                ped.SetComponentVariation(4, 1, 0, 0);
                ped.SetComponentVariation(6, 1, 0, 0);
                ped.SetComponentVariation(7, 1, 0, 0);
                ped.SetComponentVariation(8, 0, 0, 0);
                ped.SetComponentVariation(10, 0, 0, 0);
                ped.SetComponentVariation(11, 0, 0, 0);

                ped.SetPropIndex(1, 0, 0, false);
            }
            else if (randomClothesNumber == 3)
            {
                ped.SetComponentVariation(0, 2, 1, 0);
                ped.SetComponentVariation(1, 0, 0, 0);
                ped.SetComponentVariation(2, 2, 1, 0);
                ped.SetComponentVariation(3, 3, 3, 0);
                ped.SetComponentVariation(4, 1, 0, 0);
                ped.SetComponentVariation(6, 1, 0, 0);
                ped.SetComponentVariation(7, 2, 0, 0);
                ped.SetComponentVariation(8, 3, 0, 0);
                ped.SetComponentVariation(10, 0, 0, 0);
                ped.SetComponentVariation(11, 0, 0, 0);
            }
            else if (randomClothesNumber == 4)
            {
                ped.SetComponentVariation(0, 2, 0, 0);
                ped.SetComponentVariation(1, 0, 0, 0);
                ped.SetComponentVariation(2, 2, 0, 0);
                ped.SetComponentVariation(3, 2, 3, 0);
                ped.SetComponentVariation(4, 0, 0, 0);
                ped.SetComponentVariation(6, 0, 0, 0);
                ped.SetComponentVariation(7, 0, 0, 0);
                ped.SetComponentVariation(8, 2, 0, 0);
                ped.SetComponentVariation(10, 0, 0, 0);
                ped.SetComponentVariation(11, 0, 0, 0);
            }
            else if (randomClothesNumber == 5)
            {
                ped.SetComponentVariation(0, 1, 1, 0);
                ped.SetComponentVariation(1, 0, 0, 0);
                ped.SetComponentVariation(2, 1, 1, 0);
                ped.SetComponentVariation(3, 1, 3, 0);
                ped.SetComponentVariation(4, 0, 0, 0);
                ped.SetComponentVariation(6, 0, 0, 0);
                ped.SetComponentVariation(7, 2, 0, 0);
                ped.SetComponentVariation(8, 1, 0, 0);
                ped.SetComponentVariation(10, 0, 0, 0);
                ped.SetComponentVariation(11, 0, 0, 0);
            }
            else if (randomClothesNumber == 6)
            {
                ped.SetComponentVariation(0, 1, 1, 0);
                ped.SetComponentVariation(1, 0, 0, 0);
                ped.SetComponentVariation(2, 1, 0, 0);
                ped.SetComponentVariation(3, 0, 3, 0);
                ped.SetComponentVariation(4, 0, 0, 0);
                ped.SetComponentVariation(6, 0, 0, 0);
                ped.SetComponentVariation(7, 0, 0, 0);
                ped.SetComponentVariation(8, 0, 0, 0);
                ped.SetComponentVariation(10, 0, 0, 0);
                ped.SetComponentVariation(11, 0, 0, 0);
            }

            Service.Play(ped, new Animation("anim_casino_b@amb@casino@games@roulette@dealer_female", "idle", 8f, 0f, -1, 0, 0f, true, true, true), -1);
        }

        public void Spin(int casinoId, int rouletteId, byte targetNumber)
        {
            var taskKey = $"CASINO_ROULETTE_{casinoId}_{rouletteId}";

            AsyncTask task = null;

            task = new AsyncTask(async () =>
                {
                    uint ballHash = RAGE.Util.Joaat.Hash("vw_prop_roulette_ball");

                    await Streaming.RequestModel(ballHash);

                    await Utils.Game.Audio.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_GENERAL", false, -1);

                    await Streaming.RequestAnimDict("anim_casino_b@amb@casino@games@roulette@table");

                    if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task) || TableObject?.Exists != true || NPC.Ped?.Exists != true)
                        return;

                    NPC.Ped.PlaySpeech("MINIGAME_DEALER_CLOSED_BETS", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);

                    Vector3 wheelPos = TableObject.GetWorldPositionOfBone(TableObject.GetBoneIndexByName("Roulette_Wheel"));

                    Service.Play(NPC.Ped, new Animation("anim_casino_b@amb@casino@games@roulette@dealer_female", "no_more_bets", 8f, 0f, -1, 0, 0f, true, true, true), -1);

                    await RAGE.Game.Invoker.WaitAsync(1_500);

                    BallObject?.Destroy();

                    Service.Play(NPC.Ped, new Animation("anim_casino_b@amb@casino@games@roulette@dealer_female", "spin_wheel", 8f, 0f, -1, 0, 0f, true, true, true), -1);

                    await RAGE.Game.Invoker.WaitAsync(3_000);

                    if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task) || TableObject?.Exists != true || NPC.Ped?.Exists != true)
                        return;

                    int rouletteSoundId = RAGE.Game.Audio.GetSoundId();

                    RAGE.Game.Audio.PlaySoundFromEntity(rouletteSoundId, "DLC_VW_ROULETTE_BALL_LOOP", TableObject.Handle, "dlc_vw_table_games_sounds", true, 0);

                    byte ballIdx = new Dictionary<byte, byte>()
                    {
                        { 1, 38 },
                        { 2, 19 },
                        { 3, 34 },
                        { 4, 15 },
                        { 5, 30 },
                        { 6, 11 },
                        { 7, 26 },
                        { 8, 7 },
                        { 9, 22 },
                        { 10, 3 },
                        { 11, 25 },
                        { 12, 6 },
                        { 13, 37 },
                        { 14, 18 },
                        { 15, 33 },
                        { 16, 14 },
                        { 17, 29 },
                        { 18, 10 },
                        { 19, 8 },
                        { 20, 27 },
                        { 21, 12 },
                        { 22, 31 },
                        { 23, 16 },
                        { 24, 35 },
                        { 25, 4 },
                        { 26, 23 },
                        { 27, 2 },
                        { 28, 21 },
                        { 29, 5 },
                        { 30, 24 },
                        { 31, 9 },
                        { 32, 28 },
                        { 33, 13 },
                        { 34, 32 },
                        { 35, 17 },
                        { 36, 36 },
                        { 37, 20 },
                        { 38, 1 },
                    }.GetValueOrDefault(targetNumber);

                    BallObject = new MapObject(RAGE.Game.Object.CreateObjectNoOffset(ballHash, wheelPos.X, wheelPos.Y, wheelPos.Z, false, false, false))
                    {
                        Dimension = uint.MaxValue,
                    };

                    var ballRotation = new Vector3(0f, 0f, TableObject.GetHeading() + 90f);

                    BallObject.SetRotation(ballRotation.X, ballRotation.Y, ballRotation.Z, 2, false);

                    BallObject.PlayAnim("intro_ball", "anim_casino_b@amb@casino@games@roulette@table", 1000f, false, true, true, 0, 136704);
                    BallObject.PlayAnim("loop_ball", "anim_casino_b@amb@casino@games@roulette@table", 1000f, false, true, false, 0, 136704);

                    TableObject.PlayAnim("intro_wheel", "anim_casino_b@amb@casino@games@roulette@table", 1000f, false, true, true, 0, 136704);
                    TableObject.PlayAnim("loop_wheel", "anim_casino_b@amb@casino@games@roulette@table", 1000f, false, true, false, 0, 136704);

                    BallObject.PlayAnim($"exit_{ballIdx}_ball", "anim_casino_b@amb@casino@games@roulette@table", 1000f, false, true, false, 0, 136704);
                    TableObject.PlayAnim($"exit_{ballIdx}_wheel", "anim_casino_b@amb@casino@games@roulette@table", 1000f, false, true, false, 0f, 136704);

                    await RAGE.Game.Invoker.WaitAsync(7_000);

                    RAGE.Game.Audio.StopSound(rouletteSoundId);
                    RAGE.Game.Audio.ReleaseSoundId(rouletteSoundId);

                    await RAGE.Game.Invoker.WaitAsync(3_000);

                    if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task) || TableObject?.Exists != true || NPC.Ped?.Exists != true)
                        return;

                    NPC.Ped.PlaySpeech($"MINIGAME_ROULETTE_BALL_{(targetNumber == (byte)BetType._0 ? "0" : targetNumber == (byte)BetType._00 ? "00" : targetNumber.ToString())}",
                        "SPEECH_PARAMS_FORCE_NORMAL_CLEAR",
                        1
                    );

                    Service.Play(NPC.Ped, new Animation("anim_casino_b@amb@casino@games@roulette@dealer_female", "clear_chips_zone2", 8f, 0f, -1, 0, 0f, true, true, true), -1);

                    await RAGE.Game.Invoker.WaitAsync(1_500);

                    if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task) || TableObject?.Exists != true || NPC.Ped?.Exists != true)
                        return;

                    NPC.Ped.PlaySpeech("MINIGAME_DEALER_PLACE_BET_01", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);

                    Service.Play(NPC.Ped, new Animation("anim_casino_b@amb@casino@games@roulette@dealer_female", "idle", 8f, 0f, -1, 0, 0f, true, true, true), -1);

                    AsyncTask.Methods.CancelPendingTask(taskKey);
                },
                0,
                false,
                0
            );

            AsyncTask.Methods.SetAsPending(task, taskKey);
        }

        public void StartGame()
        {
            if (TextLabel != null)
            {
                RGBA color = TextLabel.Color;

                color.Alpha = 0;

                TextLabel.Color = color;
            }

            CurrentRoulette = this;

            NPC.Ped.PlaySpeech("MINIGAME_DEALER_GREET", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);

            float tableHeading = TableObject.GetHeading();

            Management.Camera.Service.Enable(Management.Camera.Service.StateTypes.CasinoRouletteGame, TableObject, null, 500, null, null, null);

            Management.Camera.Service.Rotation = new Vector3(270f, -90f, tableHeading + 270f);

            HoverDatas = new Dictionary<BetType, HoverData>();

            var counter = (byte)1;

            for (byte i = 0; i < 12; i++)
            {
                for (byte j = 0; j < 3; j++)
                {
                    HoverDatas.Add((BetType)counter,
                        new HoverData("vw_prop_vw_marker_02a")
                        {
                            HoverPosition = TableObject.GetOffsetFromInWorldCoords(0.081f * i - 0.057f, 0.167f * j - 0.192f, 0.9448f),
                            Position = TableObject.GetOffsetFromInWorldCoords(0.081f * i - 0.057f, 0.167f * j - 0.192f, 0.9448f),
                            ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.081f * i - 0.057f, 0.167f * j - 0.192f, 0.9448f),
                            DisplayName = counter.ToString(),
                            HoverNumbers = new byte[]
                            {
                                counter,
                            },
                        }
                    );

                    counter++;
                }
            }

            HoverDatas.Add(BetType._0,
                new HoverData("vw_prop_vw_marker_01a")
                {
                    HoverPosition = TableObject.GetOffsetFromInWorldCoords(-0.137f, -0.148f, 0.9448f),
                    Position = TableObject.GetOffsetFromInWorldCoords(-0.126f, -0.14f, 0.9448f),
                    ObjectPosition = TableObject.GetOffsetFromInWorldCoords(-0.126f, -0.14f, 0.9448f),
                    DisplayName = "Zero",
                    HoverNumbers = new byte[]
                    {
                        (byte)BetType._0,
                    },
                }
            );

            HoverDatas.Add(BetType._00,
                new HoverData("vw_prop_vw_marker_01a")
                {
                    HoverPosition = TableObject.GetOffsetFromInWorldCoords(-0.137f, 0.107f, 0.9448f),
                    Position = TableObject.GetOffsetFromInWorldCoords(-0.13f, 0.11f, 0.9448f),
                    ObjectPosition = TableObject.GetOffsetFromInWorldCoords(-0.13f, 0.11f, 0.9448f),
                    DisplayName = "Double Zero",
                    HoverNumbers = new byte[]
                    {
                        (byte)BetType._00,
                    },
                }
            );

            HoverDatas.Add(BetType.Red,
                new HoverData()
                {
                    Position = TableObject.GetOffsetFromInWorldCoords(0.295f, -0.38f, 0.9448f),
                    ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.295f, -0.38f, 0.9448f),
                    DisplayName = "Red",
                    HoverNumbers = new byte[]
                    {
                        1,
                        3,
                        5,
                        7,
                        9,
                        12,
                        14,
                        16,
                        18,
                        19,
                        21,
                        23,
                        25,
                        27,
                        30,
                        32,
                        34,
                        36,
                    },
                }
            );

            HoverDatas.Add(BetType.Black,
                new HoverData()
                {
                    Position = TableObject.GetOffsetFromInWorldCoords(0.45f, -0.38f, 0.9448f),
                    ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.45f, -0.38f, 0.9448f),
                    DisplayName = "Black",
                    HoverNumbers = new byte[]
                    {
                        2,
                        4,
                        6,
                        8,
                        10,
                        11,
                        13,
                        15,
                        17,
                        20,
                        22,
                        24,
                        26,
                        28,
                        29,
                        31,
                        33,
                        35,
                    },
                }
            );

            HoverDatas.Add(BetType.Even,
                new HoverData()
                {
                    Position = TableObject.GetOffsetFromInWorldCoords(0.13f, -0.38f, 0.9448f),
                    ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.13f, -0.38f, 0.9448f),
                    DisplayName = "Even",
                    HoverNumbers = new byte[]
                    {
                        2,
                        4,
                        6,
                        8,
                        10,
                        12,
                        14,
                        16,
                        18,
                        20,
                        22,
                        24,
                        26,
                        28,
                        30,
                        32,
                        34,
                        36,
                    },
                }
            );

            HoverDatas.Add(BetType.Odd,
                new HoverData()
                {
                    Position = TableObject.GetOffsetFromInWorldCoords(0.65f, -0.38f, 0.9448f),
                    ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.65f, -0.38f, 0.9448f),
                    DisplayName = "Odd",
                    HoverNumbers = new byte[]
                    {
                        1,
                        3,
                        5,
                        7,
                        9,
                        11,
                        13,
                        15,
                        17,
                        19,
                        21,
                        23,
                        25,
                        27,
                        29,
                        31,
                        33,
                        35,
                    },
                }
            );

            HoverDatas.Add(BetType._1to18,
                new HoverData()
                {
                    Position = TableObject.GetOffsetFromInWorldCoords(-0.01f, -0.38f, 0.9448f),
                    ObjectPosition = TableObject.GetOffsetFromInWorldCoords(-0.01f, -0.38f, 0.9448f),
                    DisplayName = "1 to 18",
                    HoverNumbers = new byte[]
                    {
                        1,
                        2,
                        3,
                        4,
                        5,
                        6,
                        7,
                        8,
                        9,
                        10,
                        11,
                        12,
                        13,
                        14,
                        15,
                        16,
                        17,
                        18,
                    },
                }
            );

            HoverDatas.Add(BetType._19to36,
                new HoverData()
                {
                    Position = TableObject.GetOffsetFromInWorldCoords(0.77f, -0.38f, 0.9448f),
                    ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.77f, -0.38f, 0.9448f),
                    DisplayName = "19 to 36",
                    HoverNumbers = new byte[]
                    {
                        19,
                        20,
                        21,
                        22,
                        23,
                        24,
                        25,
                        26,
                        27,
                        28,
                        29,
                        30,
                        31,
                        32,
                        33,
                        34,
                        35,
                        36,
                    },
                }
            );

            HoverDatas.Add(BetType.First_12,
                new HoverData()
                {
                    Position = TableObject.GetOffsetFromInWorldCoords(0.1f, -0.3f, 0.9448f),
                    ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.1f, -0.3f, 0.9448f),
                    DisplayName = "1st 12",
                    HoverNumbers = new byte[]
                    {
                        1,
                        2,
                        3,
                        4,
                        5,
                        6,
                        7,
                        8,
                        9,
                        10,
                        11,
                        12,
                    },
                }
            );

            HoverDatas.Add(BetType.Second_12,
                new HoverData()
                {
                    Position = TableObject.GetOffsetFromInWorldCoords(0.4f, -0.3f, 0.9448f),
                    ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.4f, -0.3f, 0.9448f),
                    DisplayName = "2nd 12",
                    HoverNumbers = new byte[]
                    {
                        13,
                        14,
                        15,
                        16,
                        17,
                        18,
                        19,
                        20,
                        21,
                        22,
                        23,
                        24,
                    },
                }
            );

            HoverDatas.Add(BetType.Third_12,
                new HoverData()
                {
                    Position = TableObject.GetOffsetFromInWorldCoords(0.7f, -0.3f, 0.9448f),
                    ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.7f, -0.3f, 0.9448f),
                    DisplayName = "3rd 12",
                    HoverNumbers = new byte[]
                    {
                        25,
                        26,
                        27,
                        28,
                        29,
                        30,
                        31,
                        32,
                        33,
                        34,
                        35,
                        36,
                    },
                }
            );

            HoverDatas.Add(BetType._2to1_1,
                new HoverData()
                {
                    Position = TableObject.GetOffsetFromInWorldCoords(0.92f, -0.2f, 0.9448f),
                    ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.92f, -0.2f, 0.9448f),
                    DisplayName = "2 to 1",
                    HoverNumbers = new byte[]
                    {
                        1,
                        4,
                        7,
                        10,
                        13,
                        16,
                        19,
                        22,
                        25,
                        28,
                        31,
                        34,
                    },
                }
            );

            HoverDatas.Add(BetType._2to1_2,
                new HoverData()
                {
                    Position = TableObject.GetOffsetFromInWorldCoords(0.93f, -0.01f, 0.9448f),
                    ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.93f, -0.01f, 0.9448f),
                    DisplayName = "2 to 1",
                    HoverNumbers = new byte[]
                    {
                        2,
                        5,
                        8,
                        11,
                        14,
                        17,
                        20,
                        23,
                        26,
                        29,
                        32,
                        35,
                    },
                }
            );

            HoverDatas.Add(BetType._2to1_3,
                new HoverData()
                {
                    Position = TableObject.GetOffsetFromInWorldCoords(0.94f, 0.17f, 0.9448f),
                    ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.94f, 0.17f, 0.9448f),
                    DisplayName = "2 to 1",
                    HoverNumbers = new byte[]
                    {
                        3,
                        6,
                        9,
                        12,
                        15,
                        18,
                        21,
                        24,
                        27,
                        30,
                        33,
                        36,
                    },
                }
            );

            HoverObjects = new List<MapObject>();

            UpdateActiveBets();

            Main.Render -= Render;
            Main.Render += Render;

            Main.MouseClicked -= OnMouseClick;
            Main.MouseClicked += OnMouseClick;
        }

        public void StopGame()
        {
            if (TextLabel != null)
            {
                RGBA color = TextLabel.Color;

                color.Alpha = 255;

                TextLabel.Color = color;
            }

            Management.Camera.Service.Disable(750);

            CurrentRoulette = null;

            HoveredBet = BetType.None;

            Main.Render -= Render;

            Main.MouseClicked -= OnMouseClick;

            if (ActiveBets != null)
                foreach (BetData x in ActiveBets)
                {
                    if (x.MapObject != null)
                    {
                        x.MapObject.Destroy();

                        x.MapObject = null;
                    }
                }

            if (HoverObjects != null)
            {
                foreach (MapObject x in HoverObjects)
                {
                    x?.Destroy();
                }

                HoverObjects.Clear();
                HoverObjects = null;
            }

            if (HoverDatas != null)
            {
                HoverDatas.Clear();
                HoverDatas = null;
            }
        }

        public void UpdateActiveBets()
        {
            if (ActiveBets == null || HoverDatas == null)
                return;

            float tableHeading = TableObject.GetHeading();

            foreach (BetData x in ActiveBets)
            {
                HoverData data = HoverDatas[x.BetType];

                x.MapObject?.Destroy();

                uint chipModel = RAGE.Util.Joaat.Hash(CasinoEntity.GetChipPropByAmount(x.Amount));

                x.MapObject = new MapObject(RAGE.Game.Object.CreateObjectNoOffset(chipModel,
                        data.ObjectPosition.X,
                        data.ObjectPosition.Y,
                        data.ObjectPosition.Z,
                        false,
                        false,
                        false
                    )
                )
                {
                    Dimension = uint.MaxValue,
                };

                x.MapObject.SetHeading(tableHeading);
            }
        }

        private static async void OnMouseClick(int x, int y, bool up, bool right)
        {
            if (!up || right)
                return;

            BetType betType = HoveredBet;

            if (!Cursor.IsVisible || betType == BetType.None)
                return;

            Roulette roulette = CurrentRoulette;

            if (roulette == null)
                return;

            CasinoEntity casino = CasinoEntity.All.Where(x => x.Roulettes.Contains(roulette)).FirstOrDefault();

            if (casino == null)
                return;

            int casinoIdx = casino.Id;

            int rouletteIdx = CurrentRoulette.GetIdInCasino(casinoIdx);

            string stateData = roulette.CurrentStateData;

            if (stateData == null || !(stateData[0] == 'S' || stateData[0] == 'I'))
            {
                Notification.Show("Casino::CSB");

                return;
            }

            uint bet = CasinoMinigames.CurrentBet;

            if (roulette.ActiveBets != null)
            {
                BetData sameBet = roulette.ActiveBets.Where(x => x.BetType == betType).FirstOrDefault();

                if (sameBet != null)
                {
                    Notification.ShowError("Вы уже сделали ставку на этот сектор!", -1);

                    return;
                }
            }

            if (bet < roulette.MinBet || bet > roulette.MaxBet)
            {
                Notification.ShowError(
                    $"На этом столе разрешены ставки от {Locale.Get("GEN_CHIPS_0", CurrentRoulette.MinBet)} до {Locale.Get("GEN_CHIPS_0", CurrentRoulette.MaxBet)}",
                    -1
                );

                return;
            }

            if (CasinoEntity.LastSent.IsSpam(500, false, false))
                return;

            CasinoEntity.LastSent = World.Core.ServerTime;

            var res = (bool)await Events.CallRemoteProc("Casino::RLTSB", casinoIdx, rouletteIdx, (byte)betType, bet);

            if (!res)
                return;

            if (roulette.ActiveBets == null)
                roulette.ActiveBets = new List<BetData>();

            roulette.ActiveBets.Add(new BetData()
                {
                    BetType = betType,
                    Amount = bet,
                }
            );

            roulette.UpdateActiveBets();

            roulette.NPC?.Ped?.PlaySpeech("MINIGAME_DEALER_PLACE_CHIPS", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1, true);
        }

        private static void Render()
        {
            if (CurrentRoulette == null || CurrentRoulette.TableObject?.Exists != true)
                return;

            var screenRes = new RAGE.Ui.Cursor.Vector2(Main.ScreenResolution.X, Main.ScreenResolution.Y);

            RAGE.Ui.Cursor.Vector2 cursorPos = RAGE.Ui.Cursor.Position;

            var detectTolerance = 25f;
            BetType betType = BetType.None;

            if (CurrentRoulette.ActiveBets != null)
                foreach (BetData x in CurrentRoulette.ActiveBets)
                {
                    HoverData hData = HoverDatas[x.BetType];

                    RAGE.Ui.Cursor.Vector2 screenCoordPos = Graphics.GetScreenCoordFromWorldCoord(hData.ObjectPosition);

                    if (screenCoordPos == null)
                        continue;

                    //UtilsT.GTA.Graphics.DrawText(hData.DisplayName, screenCoordPos.X, screenCoordPos.Y, 255, 255, 255, 255, 0.25f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                    Graphics.DrawText(Locale.Get("GEN_CHIPS_0", x.Amount),
                        screenCoordPos.X,
                        screenCoordPos.Y,
                        255,
                        255,
                        255,
                        255,
                        0.35f,
                        RAGE.Game.Font.ChaletComprimeCologne,
                        true,
                        true
                    );
                }

            if (Cursor.IsVisible)
                foreach (KeyValuePair<BetType, HoverData> x in HoverDatas)
                {
                    if (x.Value.Position == null)
                        continue;

                    RAGE.Ui.Cursor.Vector2 screenCoordPos = Graphics.GetScreenCoordFromWorldCoord(x.Value.Position);

                    if (screenCoordPos == null)
                        continue;

                    var dist = (float)System.Math.Sqrt(System.Math.Pow(screenCoordPos.X * screenRes.X - cursorPos.X, 2f) +
                                                       System.Math.Pow(screenCoordPos.Y * screenRes.Y - cursorPos.Y, 2f)
                    );

                    if (dist < detectTolerance)
                    {
                        detectTolerance = dist;
                        betType = x.Key;
                    }
                }

            if (HoveredBet == betType)
                return;

            HoveredBet = betType;

            if (HoverObjects.Count > 0)
            {
                foreach (MapObject x in HoverObjects)
                {
                    x?.Destroy();
                }

                HoverObjects.Clear();
            }

            /*                    foreach (var x in HoverDatas)
                                {
                                    if (x.Value.Position == null)
                                        continue;

                                    var mapObj = new MapObject(RAGE.Game.Object.CreateObjectNoOffset(RAGE.Util.Joaat.Hash("vw_prop_chip_100dollar_x1"), x.Value.Position.X, x.Value.Position.Y, x.Value.Position.Z, false, false, false));

                                    HoverObjects.Add(mapObj);
                                }*/

            if (HoveredBet == BetType.None)
                return;

            HoverData data = HoverDatas.GetValueOrDefault(HoveredBet);

            if (data == null || data.HoverNumbers == null)
                return;

            float tableHeading = CurrentRoulette.TableObject.GetHeading();

            foreach (byte x in data.HoverNumbers)
            {
                HoverData t = HoverDatas.GetValueOrDefault((BetType)x);

                if (t == null)
                    continue;

                var mapObj = new MapObject(RAGE.Game.Object.CreateObjectNoOffset(t.HoverModel, t.HoverPosition.X, t.HoverPosition.Y, t.HoverPosition.Z, false, false, false))
                {
                    Dimension = uint.MaxValue,
                };

                mapObj.SetHeading(tableHeading);

                HoverObjects.Add(mapObj);
            }
        }
    }
}