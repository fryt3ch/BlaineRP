using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Data
{
    public class ShootingRange : Events.Script
    {
        public enum Types
        {
            Shop = 0,
            Army,
        }

        public static Types? CurrentType { get; private set; }

        private static List<Target> Targets { get; set; }

        private static float CurrentRotSpeed { get; set; }

        private static float CurrentPosSpeed { get; set; }

        private static int CurrentMaxScore { get; set; }

        private static int CurrentScore { get; set; }
        private static int CurrentLooseScore { get; set; }
        private static int CurrentTotalShots { get; set; }

        private static DateTime LastTargetAdded { get; set; }
        private static DateTime LastShotRegistered { get; set; }

        private static int NewTargetDelay { get; set; }
        private static int OldTargetDelay { get; set; }

        private static float MinAccuracy { get; set; }

        private class Target
        {
            public enum StateTypes : byte
            {
                /// <summary>В процессе открытия</summary>
                Opening = 0,
                /// <summary>Открыта, бездействие</summary>
                Idle,
                /// <summary>В процессе закрытия</summary>
                Closing,
                /// <summary>Закрыта, бездействие</summary>
                Closed,
            }

            public MapObject Object { get; set; }

            public DateTime CreationTime { get; set; }

            public float RotationX { get; set; }

            public StateTypes CurrentState { get; set; }

            public float CurrentInterpolation { get; set; }

            public bool Direction { get; set; }

            public int Row { get; set; }

            public int Index { get; set; }

            private Target(Types ShootingRangeType, int Row, int Index, int PropVariant, bool Direction)
            {
                var rData = Ranges[ShootingRangeType];

                var rPos = rData.GetTargetPositionByIdx(Row, Index);

                this.Object = new MapObject(RAGE.Util.Joaat.Hash(PropVariant == 0 ? "prop_range_target_01" : "prop_range_target_03"), rPos, rData.TargetRotation, 75, Player.LocalPlayer.Dimension);

                this.Index = Index;
                this.Row = Row;

                this.CurrentInterpolation = rPos.DistanceTo(rData.GetLeftRowPosition(Row)) / rData.GetRowDistance(Row);

                this.Direction = Direction;

                this.CurrentState = StateTypes.Opening;

                CreationTime = DateTime.Now;
                RotationX = rData.TargetRotation.X;
            }

            public static Target TryCreateNew(Types srType)
            {
                var rand = new Random();

                var rData = Ranges[srType];

                int row = -1;
                int minRowCount = int.MaxValue;

                for (int i = 0; i < rData.TotalRows; i++)
                {
                    var count = Targets.Where(x => x.Row == i).Count();

                    if (count < minRowCount && count < rData.MaxTargetsInRow)
                    {
                        minRowCount = count;

                        row = i;
                    }
                }

                if (row < 0)
                    return null;

                for (int i = 0; i < rData.TargetPositions[row].Length; i++)
                {
                    var idx = rand.Next(0, rData.TargetPositions[row].Length);

                    if (!Targets.Where(x => x.Index == idx).Any())
                    {
                        var target = new Target(srType, row, idx, rand.Next(0, 2), rand.Next(0, 2) == 0);

                        Targets.Add(target);

                        return target;
                    }
                }

                return null;
            }

            public void Destroy()
            {
                Targets.Remove(this);

                Object?.Destroy();
            }
        }

        private static Dictionary<Types, Data> Ranges { get; set; } = new Dictionary<Types, Data>()
        {
            {
                Types.Shop,

                new Data(new Vector3(-90f, 0f, 160f), 6, new Vector3[][]
                {
                    new Vector3[]
                    {
                        new Vector3(10.96f, -1088.14f, 31.55f),
                        new Vector3(11.88f, -1088.48f, 31.55f),
                        new Vector3(12.82f, -1088.86f, 31.55f),
                        new Vector3(13.77f, -1089.18f, 31.55f),
                        new Vector3(14.69f, -1089.52f, 31.55f),
                        new Vector3(15.64f, -1089.85f, 31.55f),
                        new Vector3(16.58f, -1090.2f, 31.55f),
                        new Vector3(17.51f, -1090.54f, 31.55f),
                        new Vector3(18.48f, -1090.86f, 31.55f),
                        new Vector3(19.42f, -1091.24f, 31.55f),
                        new Vector3(20.33f, -1091.58f, 31.55f),
                        new Vector3(21.33f, -1091.94f, 31.55f),
                    },

                    new Vector3[]
                    {
                        new Vector3(14.1f, -1079.58f, 31.55f),
                        new Vector3(15.02f, -1079.95f, 31.55f),
                        new Vector3(15.96f, -1080.3f, 31.55f),
                        new Vector3(16.9f, -1080.59f, 31.55f),
                        new Vector3(17.83f, -1080.96f, 31.55f),
                        new Vector3(18.76f, -1081.31f, 31.55f),
                        new Vector3(19.71f, -1081.67f, 31.55f),
                        new Vector3(20.63f, -1082.05f, 31.55f),
                        new Vector3(21.59f, -1082.33f, 31.55f),
                        new Vector3(22.52f, -1082.71f, 31.55f),
                        new Vector3(23.47f, -1083.04f, 31.55f),
                        new Vector3(24.42f, -1083.38f, 31.55f),
                    },

                    new Vector3[]
                    {
                        new Vector3(17.84f, -1069.23f, 31.55f),
                        new Vector3(18.79f, -1069.56f, 31.55f),
                        new Vector3(19.73f, -1069.9f, 31.55f),
                        new Vector3(20.67f, -1070.25f, 31.55f),
                        new Vector3(21.61f, -1070.61f, 31.55f),
                        new Vector3(22.54f, -1070.95f, 31.55f),
                        new Vector3(23.48f, -1071.29f, 31.55f),
                        new Vector3(24.42f, -1071.64f, 31.55f),
                        new Vector3(25.37f, -1071.98f, 31.55f),
                        new Vector3(26.3f, -1072.33f, 31.55f),
                        new Vector3(27.24f, -1072.66f, 31.55f),
                        new Vector3(28.17f, -1073.03f, 31.55f),
                    },
                })
            }
        };

        public class Data
        {
            public Vector3[][] TargetPositions { get; private set; }

            public Vector3 TargetRotation { get; private set; }

            public int MaxTargetsInRow { get; private set; }

            public int TotalRows => TargetPositions.Length;

            private float[] RowsDistances { get; set; }

            public Data(Vector3 TargetRotation, int MaxTargetsInRow, Vector3[][] TargetPositions)
            {
                this.TargetRotation = TargetRotation;
                this.TargetPositions = TargetPositions;

                this.MaxTargetsInRow = MaxTargetsInRow;

                this.RowsDistances = new float[TargetPositions.Length];

                for (int i = 0; i < RowsDistances.Length; i++)
                {
                    RowsDistances[i] = GetLeftRowPosition(i).DistanceTo(GetRightRowPosition(i));
                }
            }

            public Vector3 GetTargetPositionByIdx(int row, int idx) => TargetPositions[row][idx];

            public Vector3 GetLeftRowPosition(int row) => TargetPositions[row][0];
            public Vector3 GetRightRowPosition(int row) => TargetPositions[row][TargetPositions[row].Length - 1];

            public float GetRowDistance(int row) => RowsDistances[row];
        }

        public ShootingRange()
        {
            Events.Add("SRange::Start", async (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var srType = (Types)(int)args[0];

                Utils.SetActionAsPending("ShootingRange", true);

                while (Additional.SkyCamera.IsFadedOut)
                    await RAGE.Game.Invoker.WaitAsync(25);

                if (!Utils.IsActionPending("ShootingRange"))
                    return;

                Utils.SetActionAsPending("ShootingRange", false);

                Start(srType, pData.Skills[Sync.Players.SkillTypes.Shooting]);
            });
        }

        public static void Start(Types type, int curSkill)
        {
            if (CurrentType != null)
                return;

            CurrentType = type;

            Sync.WeaponSystem.DisabledFiring = true;

            Targets = new List<Target>();

            LastShotRegistered = DateTime.MinValue;

            CurrentTotalShots = 0;

            CurrentScore = 0;
            CurrentLooseScore = 0;

            CurrentPosSpeed = 0.00095f + curSkill * 0.000045f;
            CurrentRotSpeed = 2f + curSkill * 0.045f;

            CurrentMaxScore = curSkill < 10 ? 10 : curSkill / 10 * 10;

            MinAccuracy = 25f + curSkill * 0.1f;

            NewTargetDelay = 2500 - curSkill * 10;
            OldTargetDelay = 5000 - curSkill * 20;

            LastTargetAdded = DateTime.MinValue;

            CEF.Notification.ShowHint(string.Format(Locale.Notifications.General.ShootingRangeHint1, Math.Round(MinAccuracy, 2)), false);

            AsyncTask task = null;

            task = new AsyncTask(async () =>
            {
                await Utils.RequestModel(RAGE.Util.Joaat.Hash("prop_range_target_01"));
                await Utils.RequestModel(RAGE.Util.Joaat.Hash("prop_range_target_03"));

                if (!Utils.IsTaskStillPending("SRange::Start::D", task))
                    return;

                var scaleformCounter = Additional.Scaleform.CreateCounter("srange_s_counter", Locale.Scaleform.ShootingRangeCountdownTitle, Locale.Scaleform.ShootingRangeCountdownText, 5, Additional.Scaleform.CounterSoundTypes.Deep);

                await RAGE.Game.Invoker.WaitAsync(5000);

                if (!Utils.IsTaskStillPending("SRange::Start::D", task))
                    return;

                Sync.WeaponSystem.DisabledFiring = false;

                Events.OnPlayerWeaponShot -= ShotHandler;
                Events.OnPlayerWeaponShot += ShotHandler;

                GameEvents.Render -= Render;
                GameEvents.Render += Render;

                Utils.CancelPendingTask("SRange::Start::D");
            }, 0, false, 0);

            Utils.SetTaskAsPending("SRange::Start::D", task);
        }

        public static void Stop()
        {
            if (CurrentType == null)
                return;

            GameEvents.Render -= Render;

            Events.OnPlayerWeaponShot -= ShotHandler;

            Utils.CancelPendingTask("SRange::Start::D");

            Additional.Scaleform.Get("srange_s_counter")?.Destroy();

            CurrentType = null;

            Sync.WeaponSystem.DisabledFiring = false;

            for (int i = 0; i < Targets.Count; i++)
                Targets[i--].Destroy();

            Targets.Clear();

            Targets = null;

            Utils.SetActionAsPending("ShootingRange", false);

            RAGE.Game.Audio.PlaySoundFrontend(-1, "SHOOTING_RANGE_ROUND_OVER", "HUD_AWARDS", true);
        }

        private static void Render()
        {
            //CurrentScore = CurrentMaxScore;

            var srType = (Types)CurrentType;

            var rData = Ranges[srType];

            Utils.DrawText(string.Format(Locale.Scaleform.ShootingRangeScoreText, CurrentScore, CurrentMaxScore), 0.5f, 0.925f, 255, 255, 255, 255, 0.45f, Utils.ScreenTextFontTypes.CharletComprimeColonge, false, true);

            var totalLooseScore = CurrentTotalShots + CurrentLooseScore;

            var totalAccuracy = Math.Round((CurrentScore == 0 ? 1 / ((float)totalLooseScore + 1f) : CurrentScore / (float)totalLooseScore) * 100f, 2);

            if (totalAccuracy < MinAccuracy)
            {
                Finish();

                return;
            }

            if (totalAccuracy < 50f)
            {
                Utils.DrawText(string.Format(Locale.Scaleform.ShootingRangeAccuracyText, totalAccuracy), 0.5f, 0.95f, 255, 0, 0, 255, 0.45f, Utils.ScreenTextFontTypes.CharletComprimeColonge, false, true);
            }
            else if (totalAccuracy < 75)
            {
                Utils.DrawText(string.Format(Locale.Scaleform.ShootingRangeAccuracyText, totalAccuracy), 0.5f, 0.95f, 255, 140, 0, 255, 0.45f, Utils.ScreenTextFontTypes.CharletComprimeColonge, false, true);
            }
            else if (totalAccuracy < 90)
            {
                Utils.DrawText(string.Format(Locale.Scaleform.ShootingRangeAccuracyText, totalAccuracy), 0.5f, 0.95f, 0, 255, 0, 255, 0.45f, Utils.ScreenTextFontTypes.CharletComprimeColonge, false, true);
            }
            else
            {
                Utils.DrawText(string.Format(Locale.Scaleform.ShootingRangeAccuracyText, totalAccuracy), 0.5f, 0.95f, 255, 215, 0, 255, 0.45f, Utils.ScreenTextFontTypes.CharletComprimeColonge, false, true);
            }

            if (Targets.Count <= 5 && DateTime.Now.Subtract(LastTargetAdded).TotalMilliseconds >= NewTargetDelay)
            {
                var target = Target.TryCreateNew(srType);

                if (target != null)
                {
                    LastTargetAdded = DateTime.Now;
                }
            }

            var fpsCoef = Utils.GetFpsCoef();

            for (int i = 0; i < Targets.Count; i++)
            {
                var x = Targets[i];

                var obj = x.Object;

                if (obj?.Exists != true)
                    continue;

                if (x.CurrentState == Target.StateTypes.Opening)
                {
                    if (x.RotationX >= 0f)
                    {
                        RAGE.Game.Audio.PlaySoundFromEntity(-1, "Pin_Good", obj.Handle, "DLC_HEIST_BIOLAB_PREP_HACKING_SOUNDS", false, 0);

                        x.CurrentState = Target.StateTypes.Idle;

                        obj.SetAlpha(255, false);
                    }
                    else
                    {
                        x.RotationX += CurrentRotSpeed * fpsCoef;

                        var dRot = rData.TargetRotation;

                        obj.SetRotation(x.RotationX, dRot.Y, dRot.Z, 2, true);
                    }

                    obj.ClearLastDamageEntity();
                }
                else if (x.CurrentState == Target.StateTypes.Closing)
                {
                    if (x.RotationX <= -90f)
                    {
                        x.CurrentState = Target.StateTypes.Closed;
                    }
                    else
                    {
                        x.RotationX -= CurrentRotSpeed * fpsCoef;

                        var dRot = rData.TargetRotation;

                        obj.SetRotation(x.RotationX, dRot.Y, dRot.Z, 2, true);
                    }
                }
                else if (x.CurrentState == Target.StateTypes.Closed)
                {
                    x.Destroy();

                    i--;
                }
                else if (x.CurrentState == Target.StateTypes.Idle)
                {
                    if (DateTime.Now.Subtract(x.CreationTime).TotalMilliseconds >= OldTargetDelay)
                    {
                        RAGE.Game.Audio.PlaySoundFromEntity(-1, "Pin_Bad", obj.Handle, "DLC_HEIST_BIOLAB_PREP_HACKING_SOUNDS", false, 0);

                        x.CurrentState = Target.StateTypes.Closing;

                        CurrentLooseScore++;
                    }
                    else
                    {
                        if (x.Object.HasBeenDamagedByAnyPed())
                        {
                            if (DateTime.Now.Subtract(LastShotRegistered).TotalMilliseconds < 100)
                            {
                                x.Object.ClearLastDamageEntity();

                                continue;
                            }

                            LastShotRegistered = DateTime.Now;

                            //RAGE.Game.Audio.PlaySoundFromEntity(-1, "Target_Hit_Head_Black", obj.Handle, "DLC_GR_Bunker_Shooting_Range_Sounds", false, 0);

                            x.CurrentState = Target.StateTypes.Closing;

                            CurrentScore++;

                            if (CurrentScore >= CurrentMaxScore)
                            {
                                Finish();

                                return;
                            }
                        }
                        else
                        {
                            if (Targets.Where(y => x.Index != y.Index && x.Row == y.Row && y.CurrentState == Target.StateTypes.Idle && Math.Abs(y.CurrentInterpolation - x.CurrentInterpolation) <= 0.075f).Any())
                            {
                                x.Direction = !x.Direction;
                            }
                            else
                            {
                                if (x.CurrentInterpolation >= 1f)
                                    x.Direction = false;
                                else if (x.CurrentInterpolation <= 0f)
                                    x.Direction = true;
                            }

                            x.CurrentInterpolation += (x.Direction ? CurrentPosSpeed : -CurrentPosSpeed) * fpsCoef;

                            var newPos = RAGE.Vector3.Lerp(rData.GetLeftRowPosition(x.Row), rData.GetRightRowPosition(x.Row), x.CurrentInterpolation);

                            obj.SetCoords(newPos.X, newPos.Y, newPos.Z, false, false, false, false);
                        }
                    }
                }
            }
        }

        private static void ShotHandler(Vector3 pos, Player target, RAGE.Events.CancelEventArgs cancel) => CurrentTotalShots++;

        private static void Finish()
        {
            Stop();

            var accuracy = Math.Round((CurrentScore / (float)(CurrentLooseScore + CurrentTotalShots)) * 100f, 2);

            Events.CallRemote("SRange::Exit::Shop", CurrentScore, CurrentMaxScore, accuracy);
        }
    }
}
