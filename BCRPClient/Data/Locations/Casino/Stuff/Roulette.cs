using RAGE.Elements;
using RAGE;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace BCRPClient.Data
{
    public partial class Locations
    {
        public partial class Casino
        {
            public class Roulette
            {
                public enum BetTypes : byte
                {
                    None = 0,

                    _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13, _14, _15, _16, _17, _18, _19, _20, _21, _22, _23, _24, _25, _26, _27, _28, _29, _30, _31, _32, _33, _34, _35, _36,
                    _0, _00,

                    Red = 100,
                    Black = 101,

                    Even = 110,
                    Odd = 111,

                    _1to18 = 120,
                    _19to36 = 121,


                    First_12 = 130,
                    Second_12 = 131,
                    Third_12 = 132,

                    _2to1_1 = 140,
                    _2to1_2 = 141,
                    _2to1_3 = 142,
                }

                public static Roulette CurrentRoulette { get; set; }

                public static Dictionary<BetTypes, HoverData> HoverDatas { get; set; }

                private static List<MapObject> HoverObjects { get; set; }

                public List<BetData> ActiveBets { get; set; }

                public static BetTypes HoveredBet { get; set; }

                public class BetData
                {
                    public BetTypes BetType { get; set; }

                    public uint Amount { get; set; }

                    public MapObject MapObject { get; set; }
                }

                public class HoverData
                {
                    public uint HoverModel { get; set; }

                    public byte[] HoverNumbers { get; set; }

                    public Vector3 HoverPosition { get; set; }
                    public Vector3 ObjectPosition { get; set; }
                    public Vector3 Position { get; set; }

                    public string DisplayName { get; set; }

                    public HoverData(string HoverModel = null)
                    {
                        if (HoverModel != null)
                            this.HoverModel = RAGE.Util.Joaat.Hash(HoverModel);
                    }
                }

                public int GetIdInCasino(int casino) => Casino.GetById(casino)?.Roulettes is Roulette[] t ? Array.IndexOf(t, this) : -1;

                public MapObject TableObject { get; set; }

                public MapObject BallObject { get; set; }

                public NPC NPC { get; set; }

                public uint MinBet { get; set; }
                public uint MaxBet { get; set; }

                public List<BetTypes> LastBets { get; set; }

                public Additional.ExtraLabel TextLabel { get; set; }

                public string CurrentStateData { get; set; }

                public Roulette(int CasinoId, int Id, string Model, float PosX, float PosY, float PosZ, float RotZ)
                {
                    TableObject = new MapObject(RAGE.Util.Joaat.Hash(Model), new Vector3(PosX, PosY, PosZ), new Vector3(0f, 0f, RotZ), 255, Settings.App.Static.MainDimension)
                    {
                        NotifyStreaming = true,
                    };

                    NPC = new NPC($"Casino@Roulette_{CasinoId}_{Id}", "", NPC.Types.Static, "S_F_Y_Casino_01", RAGE.Game.Object.GetObjectOffsetFromCoords(PosX, PosY, PosZ, RotZ, 0f, 0.7f, 1f), RotZ + 180f, Settings.App.Static.MainDimension)
                    {
                        SubName = "NPC_SUBNAME_CASINO_ROULETTE_WORKER",
                    };

                    NPC.Ped.StreamInCustomActionsAdd(OnPedStreamIn);
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

                public static void OnCurrentStateDataUpdated(int casinoId, int rouletteId, string stateData, bool onLoad)
                {
                    var casino = Casino.GetById(casinoId);

                    if (!onLoad && (!casino.MainColshape.IsInside || Utils.IsTaskStillPending("CASINO_TASK", null)))
                        return;

                    var roulette = casino.GetRouletteById(rouletteId);

                    roulette.CurrentStateData = stateData;

                    if (roulette.TextLabel == null)
                        return;

                    var stateTask = roulette.TextLabel.GetData<AsyncTask>("StateTask");

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

                                var betType = (BetTypes)lastBallRes;

                                if (!onLoad)
                                {
                                    updateFunc($"Выпало число {betType.ToString().Replace("_", "")}!");

                                    var task = new AsyncTask(() =>
                                    {
                                        updateFunc(defText);
                                    }, 2_500, false, 0);

                                    task.Run();

                                    roulette.TextLabel.SetData("StateTask", task);

                                    roulette.LastBets?.Add((BetTypes)lastBallRes);

                                    if (CurrentRoulette == roulette)
                                    {
                                        Data.Minigames.Casino.Casino.AddLastBet(betType);
                                    }

                                    if (roulette.ActiveBets != null)
                                    {
                                        foreach (var x in roulette.ActiveBets)
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
                            {
                                roulette.Spin(casinoId, rouletteId, ballRes);
                            }

                            updateFunc("Ожидание результата игры...");
                        }
                        else if (str[0] == 'S')
                        {
                            var time = long.Parse(str.Substring(1));

                            var task = new AsyncTask(() =>
                            {
                                updateFunc($"Игра начнётся через {DateTimeOffset.FromUnixTimeSeconds(time).DateTime.Subtract(Sync.World.ServerTime).GetBeautyString()}");
                            }, 1_000, true, 0);

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
                        {
                            roulette.TextLabel.Text = $"Мин. ставка: {Utils.SplitToNumberOf(roulette.MinBet.ToString())} фишек\nМакс. ставка: {Utils.SplitToNumberOf(roulette.MaxBet.ToString())} фишек\n\n{str}";
                        }

                        if (CurrentRoulette == roulette)
                        {
                            Data.Minigames.Casino.Casino.UpdateStatus(str);
                        }
                    }
                }

                private static void OnPedStreamIn(Entity entity)
                {
                    var ped = entity as Ped;

                    if (ped == null)
                        return;

                    ped.SetDefaultComponentVariation();

                    var randomClothesNumber = Utils.Random.Next(0, 7);

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

                    Sync.Animations.Play(ped, new Sync.Animations.Animation("anim_casino_b@amb@casino@games@roulette@dealer_female", "idle", 8f, 0f, -1, 0, 0f, true, true, true), -1);
                }

                public void Spin(int casinoId, int rouletteId, byte targetNumber)
                {
                    var taskKey = $"CASINO_ROULETTE_{casinoId}_{rouletteId}";

                    AsyncTask task = null;

                    task = new AsyncTask(async () =>
                    {
                        var ballHash = RAGE.Util.Joaat.Hash("vw_prop_roulette_ball");

                        await Utils.RequestModel(ballHash);

                        await Utils.RequestScriptAudioBank("DLC_VINEWOOD/CASINO_GENERAL", false, -1);

                        await Utils.RequestAnimDict("anim_casino_b@amb@casino@games@roulette@table");

                        if (!Utils.IsTaskStillPending(taskKey, task) || TableObject?.Exists != true || NPC.Ped?.Exists != true)
                            return;

                        NPC.Ped.PlaySpeech("MINIGAME_DEALER_CLOSED_BETS", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);

                        var wheelPos = TableObject.GetWorldPositionOfBone(TableObject.GetBoneIndexByName("Roulette_Wheel"));

                        Sync.Animations.Play(NPC.Ped, new Sync.Animations.Animation("anim_casino_b@amb@casino@games@roulette@dealer_female", "no_more_bets", 8f, 0f, -1, 0, 0f, true, true, true), -1);

                        await RAGE.Game.Invoker.WaitAsync(1_500);

                        BallObject?.Destroy();

                        Sync.Animations.Play(NPC.Ped, new Sync.Animations.Animation("anim_casino_b@amb@casino@games@roulette@dealer_female", "spin_wheel", 8f, 0f, -1, 0, 0f, true, true, true), -1);

                        await RAGE.Game.Invoker.WaitAsync(3_000);

                        if (!Utils.IsTaskStillPending(taskKey, task) || TableObject?.Exists != true || NPC.Ped?.Exists != true)
                            return;

                        var rouletteSoundId = RAGE.Game.Audio.GetSoundId();

                        RAGE.Game.Audio.PlaySoundFromEntity(rouletteSoundId, "DLC_VW_ROULETTE_BALL_LOOP", TableObject.Handle, "dlc_vw_table_games_sounds", true, 0);

                        var ballIdx = new Dictionary<byte, byte>()
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

                        if (!Utils.IsTaskStillPending(taskKey, task) || TableObject?.Exists != true || NPC.Ped?.Exists != true)
                            return;

                        NPC.Ped.PlaySpeech($"MINIGAME_ROULETTE_BALL_{(targetNumber == (byte)BetTypes._0 ? "0" : targetNumber == (byte)BetTypes._00 ? "00" : targetNumber.ToString())}", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);

                        Sync.Animations.Play(NPC.Ped, new Sync.Animations.Animation("anim_casino_b@amb@casino@games@roulette@dealer_female", "clear_chips_zone2", 8f, 0f, -1, 0, 0f, true, true, true), -1);

                        await RAGE.Game.Invoker.WaitAsync(1_500);

                        if (!Utils.IsTaskStillPending(taskKey, task) || TableObject?.Exists != true || NPC.Ped?.Exists != true)
                            return;

                        NPC.Ped.PlaySpeech("MINIGAME_DEALER_PLACE_BET_01", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);

                        Sync.Animations.Play(NPC.Ped, new Sync.Animations.Animation("anim_casino_b@amb@casino@games@roulette@dealer_female", "idle", 8f, 0f, -1, 0, 0f, true, true, true), -1);

                        Utils.CancelPendingTask(taskKey);
                    }, 0, false, 0);

                    Utils.SetTaskAsPending(taskKey, task);
                }

                public void StartGame()
                {
                    if (TextLabel != null)
                    {
                        var color = TextLabel.Color;

                        color.Alpha = 0;

                        TextLabel.Color = color;
                    }

                    CurrentRoulette = this;

                    NPC.Ped.PlaySpeech("MINIGAME_DEALER_GREET", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1);

                    var tableHeading = TableObject.GetHeading();

                    Additional.Camera.Enable(Additional.Camera.StateTypes.CasinoRouletteGame, TableObject, null, 500, null, null, null);

                    Additional.Camera.Rotation = new Vector3(270f, -90f, tableHeading + 270f);

                    HoverDatas = new Dictionary<BetTypes, HoverData>();

                    var counter = (byte)1;

                    for (byte i = 0; i < 12; i++)
                    {
                        for (byte j = 0; j < 3; j++)
                        {
                            HoverDatas.Add((BetTypes)counter, new HoverData("vw_prop_vw_marker_02a")
                            {
                                HoverPosition = TableObject.GetOffsetFromInWorldCoords(0.081f * i - 0.057f, 0.167f * j - 0.192f, 0.9448f),
                                Position = TableObject.GetOffsetFromInWorldCoords(0.081f * i - 0.057f, 0.167f * j - 0.192f, 0.9448f),
                                ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.081f * i - 0.057f, 0.167f * j - 0.192f, 0.9448f),

                                DisplayName = counter.ToString(),

                                HoverNumbers = new byte[] { counter, },
                            });

                            counter++;
                        }
                    }

                    HoverDatas.Add(BetTypes._0, new HoverData("vw_prop_vw_marker_01a")
                    {
                        HoverPosition = TableObject.GetOffsetFromInWorldCoords(-0.137f, -0.148f, 0.9448f),
                        Position = TableObject.GetOffsetFromInWorldCoords(-0.126f, -0.14f, 0.9448f),
                        ObjectPosition = TableObject.GetOffsetFromInWorldCoords(-0.126f, -0.14f, 0.9448f),

                        DisplayName = "Zero",

                        HoverNumbers = new byte[] { (byte)BetTypes._0, },
                    });

                    HoverDatas.Add(BetTypes._00, new HoverData("vw_prop_vw_marker_01a")
                    {
                        HoverPosition = TableObject.GetOffsetFromInWorldCoords(-0.137f, 0.107f, 0.9448f),
                        Position = TableObject.GetOffsetFromInWorldCoords(-0.13f, 0.11f, 0.9448f),
                        ObjectPosition = TableObject.GetOffsetFromInWorldCoords(-0.13f, 0.11f, 0.9448f),

                        DisplayName = "Double Zero",

                        HoverNumbers = new byte[] { (byte)BetTypes._00, },
                    });

                    HoverDatas.Add(BetTypes.Red, new HoverData()
                    {
                        Position = TableObject.GetOffsetFromInWorldCoords(0.295f, -0.38f, 0.9448f),
                        ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.295f, -0.38f, 0.9448f),

                        DisplayName = "Red",

                        HoverNumbers = new byte[] { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36, },
                    });

                    HoverDatas.Add(BetTypes.Black, new HoverData()
                    {
                        Position = TableObject.GetOffsetFromInWorldCoords(0.45f, -0.38f, 0.9448f),
                        ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.45f, -0.38f, 0.9448f),

                        DisplayName = "Black",

                        HoverNumbers = new byte[] { 2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35, },
                    });

                    HoverDatas.Add(BetTypes.Even, new HoverData()
                    {
                        Position = TableObject.GetOffsetFromInWorldCoords(0.13f, -0.38f, 0.9448f),
                        ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.13f, -0.38f, 0.9448f),

                        DisplayName = "Even",

                        HoverNumbers = new byte[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, },
                    });

                    HoverDatas.Add(BetTypes.Odd, new HoverData()
                    {
                        Position = TableObject.GetOffsetFromInWorldCoords(0.65f, -0.38f, 0.9448f),
                        ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.65f, -0.38f, 0.9448f),

                        DisplayName = "Odd",

                        HoverNumbers = new byte[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31, 33, 35, },
                    });

                    HoverDatas.Add(BetTypes._1to18, new HoverData()
                    {
                        Position = TableObject.GetOffsetFromInWorldCoords(-0.01f, -0.38f, 0.9448f),
                        ObjectPosition = TableObject.GetOffsetFromInWorldCoords(-0.01f, -0.38f, 0.9448f),

                        DisplayName = "1 to 18",

                        HoverNumbers = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, },
                    });

                    HoverDatas.Add(BetTypes._19to36, new HoverData()
                    {
                        Position = TableObject.GetOffsetFromInWorldCoords(0.77f, -0.38f, 0.9448f),
                        ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.77f, -0.38f, 0.9448f),

                        DisplayName = "19 to 36",

                        HoverNumbers = new byte[] { 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, },
                    });

                    HoverDatas.Add(BetTypes.First_12, new HoverData()
                    {
                        Position = TableObject.GetOffsetFromInWorldCoords(0.1f, -0.3f, 0.9448f),
                        ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.1f, -0.3f, 0.9448f),

                        DisplayName = "1st 12",

                        HoverNumbers = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, },
                    });

                    HoverDatas.Add(BetTypes.Second_12, new HoverData()
                    {
                        Position = TableObject.GetOffsetFromInWorldCoords(0.4f, -0.3f, 0.9448f),
                        ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.4f, -0.3f, 0.9448f),

                        DisplayName = "2nd 12",

                        HoverNumbers = new byte[] { 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, },
                    });

                    HoverDatas.Add(BetTypes.Third_12, new HoverData()
                    {
                        Position = TableObject.GetOffsetFromInWorldCoords(0.7f, -0.3f, 0.9448f),
                        ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.7f, -0.3f, 0.9448f),

                        DisplayName = "3rd 12",

                        HoverNumbers = new byte[] { 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, },
                    });

                    HoverDatas.Add(BetTypes._2to1_1, new HoverData()
                    {
                        Position = TableObject.GetOffsetFromInWorldCoords(0.92f, -0.2f, 0.9448f),
                        ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.92f, -0.2f, 0.9448f),

                        DisplayName = "2 to 1",

                        HoverNumbers = new byte[] { 1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34, },
                    });

                    HoverDatas.Add(BetTypes._2to1_2, new HoverData()
                    {
                        Position = TableObject.GetOffsetFromInWorldCoords(0.93f, -0.01f, 0.9448f),
                        ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.93f, -0.01f, 0.9448f),

                        DisplayName = "2 to 1",

                        HoverNumbers = new byte[] { 2, 5, 8, 11, 14, 17, 20, 23, 26, 29, 32, 35, },
                    });

                    HoverDatas.Add(BetTypes._2to1_3, new HoverData()
                    {
                        Position = TableObject.GetOffsetFromInWorldCoords(0.94f, 0.17f, 0.9448f),
                        ObjectPosition = TableObject.GetOffsetFromInWorldCoords(0.94f, 0.17f, 0.9448f),

                        DisplayName = "2 to 1",

                        HoverNumbers = new byte[] { 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, },
                    });

                    HoverObjects = new List<MapObject>();

                    UpdateActiveBets();

                    GameEvents.Render -= Render;
                    GameEvents.Render += Render;

                    GameEvents.MouseClicked -= OnMouseClick;
                    GameEvents.MouseClicked += OnMouseClick;
                }

                public void StopGame()
                {
                    if (TextLabel != null)
                    {
                        var color = TextLabel.Color;

                        color.Alpha = 255;

                        TextLabel.Color = color;
                    }

                    Additional.Camera.Disable(750);

                    CurrentRoulette = null;

                    HoveredBet = BetTypes.None;

                    GameEvents.Render -= Render;

                    GameEvents.MouseClicked -= OnMouseClick;

                    if (ActiveBets != null)
                    {
                        foreach (var x in ActiveBets)
                        {
                            if (x.MapObject != null)
                            {
                                x.MapObject.Destroy();

                                x.MapObject = null;
                            }
                        }
                    }

                    if (HoverObjects != null)
                    {
                        foreach (var x in HoverObjects)
                            x?.Destroy();

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

                    var tableHeading = TableObject.GetHeading();

                    foreach (var x in ActiveBets)
                    {
                        var data = HoverDatas[x.BetType];

                        x.MapObject?.Destroy();

                        var chipModel = RAGE.Util.Joaat.Hash(Casino.GetChipPropByAmount(x.Amount));

                        x.MapObject = new MapObject(RAGE.Game.Object.CreateObjectNoOffset(chipModel, data.ObjectPosition.X, data.ObjectPosition.Y, data.ObjectPosition.Z, false, false, false))
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

                    var betType = HoveredBet;

                    if (!CEF.Cursor.IsVisible || betType == BetTypes.None)
                        return;

                    var roulette = CurrentRoulette;

                    if (roulette == null)
                        return;

                    var casino = Casino.All.Where(x => x.Roulettes.Contains(roulette)).FirstOrDefault();

                    if (casino == null)
                        return;

                    var casinoIdx = casino.Id;

                    var rouletteIdx = CurrentRoulette.GetIdInCasino(casinoIdx);

                    var stateData = roulette.CurrentStateData;

                    if (stateData == null || !(stateData[0] == 'S' || stateData[0] == 'I'))
                    {
                        CEF.Notification.Show("Casino::CSB");

                        return;
                    }

                    var bet = Data.Minigames.Casino.Casino.CurrentBet;

                    if (roulette.ActiveBets != null)
                    {
                        var sameBet = roulette.ActiveBets.Where(x => x.BetType == betType).FirstOrDefault();

                        if (sameBet != null)
                        {
                            CEF.Notification.ShowError("Вы уже сделали ставку на этот сектор!", -1);

                            return;
                        }
                    }

                    if (bet < roulette.MinBet || bet > roulette.MaxBet)
                    {
                        CEF.Notification.ShowError($"На этом столе разрешены ставки от {Utils.SplitToNumberOf(CurrentRoulette.MinBet.ToString())} до {Utils.SplitToNumberOf(CurrentRoulette.MaxBet.ToString())} фишек!", -1);

                        return;
                    }

                    if (Casino.LastSent.IsSpam(500, false, false))
                        return;

                    Casino.LastSent = Sync.World.ServerTime;

                    var res = (bool)await Events.CallRemoteProc("Casino::RLTSB", casinoIdx, rouletteIdx, (byte)betType, bet);

                    if (!res)
                        return;

                    if (roulette.ActiveBets == null)
                        roulette.ActiveBets = new List<BetData>();

                    roulette.ActiveBets.Add(new BetData() { BetType = betType, Amount = bet, });

                    roulette.UpdateActiveBets();

                    roulette.NPC?.Ped?.PlaySpeech("MINIGAME_DEALER_PLACE_CHIPS", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", 1, true);
                }

                private static void Render()
                {
                    if (CurrentRoulette == null || CurrentRoulette.TableObject?.Exists != true)
                        return;

                    var screenRes = new RAGE.Ui.Cursor.Vector2(GameEvents.ScreenResolution.X, GameEvents.ScreenResolution.Y);

                    var cursorPos = RAGE.Ui.Cursor.Position;

                    var detectTolerance = 25f; var betType = BetTypes.None;

                    if (CurrentRoulette.ActiveBets != null)
                    {
                        foreach (var x in CurrentRoulette.ActiveBets)
                        {
                            var hData = HoverDatas[x.BetType];

                            var screenCoordPos = Utils.GetScreenCoordFromWorldCoord(hData.ObjectPosition);

                            if (screenCoordPos == null)
                                continue;

                            //Utils.DrawText(hData.DisplayName, screenCoordPos.X, screenCoordPos.Y, 255, 255, 255, 255, 0.25f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                            Utils.DrawText(Utils.SplitToNumberOf(x.Amount.ToString()), screenCoordPos.X, screenCoordPos.Y, 255, 255, 255, 255, 0.35f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                        }
                    }

                    if (CEF.Cursor.IsVisible)
                    {
                        foreach (var x in HoverDatas)
                        {
                            if (x.Value.Position == null)
                                continue;

                            var screenCoordPos = Utils.GetScreenCoordFromWorldCoord(x.Value.Position);

                            if (screenCoordPos == null)
                                continue;

                            var dist = (float)Math.Sqrt(Math.Pow(screenCoordPos.X * screenRes.X - cursorPos.X, 2f) + Math.Pow(screenCoordPos.Y * screenRes.Y - cursorPos.Y, 2f));

                            if (dist < detectTolerance)
                            {
                                detectTolerance = dist;
                                betType = x.Key;
                            }
                        }
                    }

                    if (HoveredBet == betType)
                        return;

                    HoveredBet = betType;

                    if (HoverObjects.Count > 0)
                    {
                        foreach (var x in HoverObjects)
                            x?.Destroy();

                        HoverObjects.Clear();
                    }

                    /*                    foreach (var x in HoverDatas)
                                        {
                                            if (x.Value.Position == null)
                                                continue;

                                            var mapObj = new MapObject(RAGE.Game.Object.CreateObjectNoOffset(RAGE.Util.Joaat.Hash("vw_prop_chip_100dollar_x1"), x.Value.Position.X, x.Value.Position.Y, x.Value.Position.Z, false, false, false));

                                            HoverObjects.Add(mapObj);
                                        }*/

                    if (HoveredBet == BetTypes.None)
                        return;

                    var data = HoverDatas.GetValueOrDefault(HoveredBet);

                    if (data == null || data.HoverNumbers == null)
                        return;

                    var tableHeading = CurrentRoulette.TableObject.GetHeading();

                    foreach (var x in data.HoverNumbers)
                    {
                        var t = HoverDatas.GetValueOrDefault((BetTypes)x);

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
    }
}
