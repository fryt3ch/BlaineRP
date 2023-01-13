using BCRPClient.Sync;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;
using static BCRPClient.Additional.Camera.State;

namespace BCRPClient.Additional
{
    class Camera : Events.Script
    {
        public class State
        {
            public enum RenderTypes
            {
                None = -1,
                Heading,
                Position,
                Both,
            }

            public enum BehaviourTypes
            {
                None = -1,
                FrontOf,
                PointAt,
                PointBone,
            }

            public Vector3 Position { get; set; }

            public Vector3 Rotation { get; set; }

            public float Fov { get; set; }

            public float MinFov { get; set; }

            public float MaxFov { get; set; }

            public Vector3 TargetPosition { get; set; }

            public int TransitionTime { get; set; }

            public RenderTypes SourceRenderType { get; set; }
            public RenderTypes TargetRenderType { get; set; }

            public BehaviourTypes SourceBehaviourType { get; set; }
            public BehaviourTypes TargetBehaviourType { get; set; }

            public object SourceParams { get; set; }
            public object TargetParams { get; set; }

            public float ShakeAmplitude { get; set; }

            public Action<object[]> OnAction { get; set; }

            public Action<object[]> OffAction { get; set; }

            /// <summary>Состояние камеры</summary>
            /// <param name="Position">Позиция камеры (если задана основная сущность, то этот параметр - смещение, а не сама позиция)</param>
            /// <param name="Rotation">Поворот камеры (если null, то будет использоваться 0 0 0)</param>
            /// <param name="Fov">Поле обзора</param>
            /// <param name="TargetPosition">Целевая позиция (если задана целевая сущность, то этот параметр - смещение (от сущности/кости), а не сама позиция)</param>
            /// <param name="TransitionTime">Время перехода от прошлой камеры к текущей</param>
            public State(Vector3 Position = null, Vector3 Rotation = null, float Fov = 50, Vector3 TargetPosition = null, int TransitionTime = 0, RenderTypes SourceRenderType = RenderTypes.None, RenderTypes TargetRenderType = RenderTypes.None)
            {
                this.Position = Position;
                this.Rotation = Rotation;
                this.Fov = Fov;
                this.TargetPosition = TargetPosition;

                this.TransitionTime = TransitionTime;

                this.SourceRenderType = SourceRenderType;
                this.TargetRenderType = TargetRenderType;

                this.SourceBehaviourType = BehaviourTypes.None;
                this.TargetBehaviourType = BehaviourTypes.None;

                this.SourceParams = null;
                this.TargetParams = null;

                this.MinFov = Fov;
                this.MaxFov = Fov;

                ShakeAmplitude = 0.5f;
            }
        }

        public enum StateTypes
        {
            Head = 0,
            Body,
            Legs,
            Foots,
            RightHand,
            LeftHand,
            WholePed,

            WholeVehicle,
            WholeVehicleOpen,
            FrontVehicle,
            FrontVehicleOpenHood,
            BackVehicle,
            BackVehicleOpenTrunk,
            RightVehicle,
            LeftVehicle,
            TopVehicle,
            BackVehicleUpAngle,

            NpcTalk,
        }

        public static Dictionary<StateTypes, State> States = new Dictionary<StateTypes, State>()
        {
            { StateTypes.Foots, new State(new Vector3(0f, 0f, -0.5f), new Vector3(0f, 0f, 0f), 40, new Vector3(0f, 0f, -0.75f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = BehaviourTypes.PointBone, TargetParams = 23553, MinFov = 10 } },

            { StateTypes.Legs, new State(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 55, new Vector3(0f, 0f, -0.25f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = BehaviourTypes.PointBone, TargetParams = 23553, MinFov = 10 } },

            { StateTypes.Body, new State(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 60, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = BehaviourTypes.PointBone, TargetParams = 23553, MinFov = 10 } },

            { StateTypes.Head, new State(new Vector3(0, 0, 1f), new Vector3(0f, 0f, 0f), 30, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = BehaviourTypes.PointBone, TargetParams = 31086, MinFov = 10 } },

            { StateTypes.LeftHand, new State(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 30, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = BehaviourTypes.PointBone, TargetParams = 36029, MinFov = 10 } },

            { StateTypes.RightHand, new State(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 30, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = BehaviourTypes.PointBone, TargetParams = 57005, MinFov = 10 } },

            { StateTypes.WholePed, new State(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 80, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.5f }, TargetBehaviourType = BehaviourTypes.PointBone, TargetParams = 23553, MinFov = 10 } },

            { StateTypes.NpcTalk, new State(new Vector3(0f, 0f, 1f), null, 30, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.Both, State.RenderTypes.None) { SourceBehaviourType = BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = BehaviourTypes.PointBone, TargetParams = 31086, MinFov = 10 } },

            { StateTypes.WholeVehicle, new State(new Vector3(0f, 0f, 1.35f), null, 60, new Vector3(0f, 0f, 0f), 750, RenderTypes.None, RenderTypes.None) { SourceBehaviourType = BehaviourTypes.FrontOf, SourceParams = new float[] { 45f, 5.5f }, TargetBehaviourType = BehaviourTypes.PointAt, MinFov = 10 } },

            { StateTypes.WholeVehicleOpen, new State(new Vector3(0f, 0f, 1.35f), new Vector3(0f, 0f, 0f), 60, new Vector3(0f, 0f, 0f), 750, RenderTypes.None, RenderTypes.None) { SourceBehaviourType = BehaviourTypes.FrontOf, SourceParams = new float[] { 45f, 5.5f }, TargetBehaviourType = BehaviourTypes.PointAt, MinFov = 10,

                OnAction = (args) =>
                {
                    if (SourceEntity is Vehicle veh)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            if (veh.DoesHaveDoor(i) > 0)
                                veh.SetDoorOpen(i, false, false);
                        }
                    }
                },

                OffAction = (args) =>
                {
                    if (SourceEntity is Vehicle veh)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            if (veh.DoesHaveDoor(i) > 0)
                                veh.SetDoorShut(i, false);
                        }
                    }
                }
            } },

            { StateTypes.FrontVehicle, new State(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 70, new Vector3(0f, 0f, 0f), 750, RenderTypes.None, RenderTypes.None) { SourceBehaviourType = BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 5f }, TargetBehaviourType = BehaviourTypes.PointAt, MinFov = 10 } },

            { StateTypes.FrontVehicleOpenHood, new State(new Vector3(0, 0, 1f), new Vector3(0f, 0f, 0f), 70, new Vector3(0f, 0f, 0f), 750, RenderTypes.None, RenderTypes.None) { SourceBehaviourType = BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 5f }, TargetBehaviourType = BehaviourTypes.PointAt, MinFov = 10,

                OnAction = (args) =>
                {
                    if (SourceEntity is Vehicle veh)
                    {
                        if (veh.DoesHaveDoor(4) > 0)
                            veh.SetDoorOpen(4, false, false);
                    }
                },

                OffAction = (args) =>
                {
                    if (SourceEntity is Vehicle veh)
                    {
                        if (veh.DoesHaveDoor(4) > 0)
                            veh.SetDoorShut(4, false);
                    }
                }
            } },

            { StateTypes.BackVehicleOpenTrunk, new State(new Vector3(0, 0, 1f), new Vector3(0f, 0f, 0f), 70, new Vector3(0f, 0f, 0f), 750, RenderTypes.None, RenderTypes.None) { SourceBehaviourType = BehaviourTypes.FrontOf, SourceParams = new float[] { -180f, 5f }, TargetBehaviourType = BehaviourTypes.PointAt, MinFov = 10,

                OnAction = (args) =>
                {
                    if (SourceEntity is Vehicle veh)
                    {
                        if (veh.DoesHaveDoor(5) > 0)
                            veh.SetDoorOpen(5, false, false);
                    }
                },

                OffAction = (args) =>
                {
                    if (SourceEntity is Vehicle veh)
                    {
                        if (veh.DoesHaveDoor(5) > 0)
                            veh.SetDoorShut(5, false);
                    }
                }
            } },

            { StateTypes.BackVehicle, new State(new Vector3(0, 0, 0), new Vector3(0f, 0f, 0f), 70, new Vector3(0f, 0f, 0f), 750, RenderTypes.None, RenderTypes.None) { SourceBehaviourType = BehaviourTypes.FrontOf, SourceParams = new float[] { -180f, 5f }, TargetBehaviourType = BehaviourTypes.PointAt, MinFov = 10 } },

            { StateTypes.BackVehicleUpAngle, new State(new Vector3(0, 0, 1.35f), new Vector3(0f, 0f, 0f), 60, new Vector3(0f, 0f, 0f), 750, RenderTypes.None, RenderTypes.None) { SourceBehaviourType = BehaviourTypes.FrontOf, SourceParams = new float[] { 210f, 5f }, TargetBehaviourType = BehaviourTypes.PointAt, MinFov = 10 } },

            { StateTypes.RightVehicle, new State(new Vector3(0, 0, 0), new Vector3(0f, 0f, 0f), 80, new Vector3(0f, 0f, 0f), 750, RenderTypes.None, RenderTypes.None) { SourceBehaviourType = BehaviourTypes.FrontOf, SourceParams = new float[] { 90f, 3.5f }, TargetBehaviourType = BehaviourTypes.PointAt, MinFov = 10 } },

            { StateTypes.LeftVehicle, new State(new Vector3(0, 0, 0), new Vector3(0f, 0f, 0f), 80, new Vector3(0f, 0f, 0f), 750, RenderTypes.None, RenderTypes.None) { SourceBehaviourType = BehaviourTypes.FrontOf, SourceParams = new float[] { -90f, 3.5f }, TargetBehaviourType = BehaviourTypes.PointAt, MinFov = 10 } },

            { StateTypes.TopVehicle, new State(new Vector3(0, 0, 4f), new Vector3(0f, 0f, 0f), 70, new Vector3(0f, 0f, 0f), 750, RenderTypes.None, RenderTypes.None) { SourceBehaviourType = BehaviourTypes.PointAt, TargetBehaviourType = BehaviourTypes.PointAt, MinFov = 10 } },
        };

        /// <summary>Минимально возможный FOV</summary>
        private static float MinFov { get; set; }

        /// <summary>Максимально возможный FOV</summary>
        private static float MaxFov { get; set; }

        /// <summary>ID текущей камеры</summary>
        private static int ID { get; set; }

        /// <summary>Позиция текущей камеры</summary>
        public static Vector3 Position { get => RAGE.Game.Cam.GetCamCoord(ID); set { RAGE.Game.Cam.SetCamCoord(ID, value.X, value.Y, value.Z); } }

        /// <summary>Поворот текущей камеры</summary>
        public static Vector3 Rotation { get => RAGE.Game.Cam.GetCamRot(ID, 5); set { RAGE.Game.Cam.SetCamRot(ID, value.X, value.Y, value.Z, 5); } }

        /// <summary>Поле обзора</summary>
        public static float Fov { get => RAGE.Game.Cam.GetCamFov(ID); set { if (value > MaxFov) return; if (value < MinFov) return; RAGE.Game.Cam.SetCamFov(ID, value); } }

        /// <summary>Активна ли камера?</summary>
        public static bool IsActive { get; private set; }

        /// <summary>Использованные камеры, подлежат удалению</summary>
        private static List<int> UsedCams { get; set; }

        /// <summary>Хэш стандартной камеры</summary>
        private static uint DefaultScriptedCameraHash = RAGE.Game.Misc.GetHashKey("DEFAULT_SCRIPTED_CAMERA");

        private static Entity SourceEntity { get; set; }

        private static AsyncTask SourceTask { get; set; }
        private static AsyncTask TargetTask { get; set; }
        private static AsyncTask ExecuteTasksSchedule { get; set; }

        /// <summary>Получить новую стандартную камеру</summary>
        private static int DefaultCamera { get => RAGE.Game.Cam.CreateCameraWithParams(DefaultScriptedCameraHash, 0f, 0f, 0f, 0f, 0f, 0f, 0f, true, 2); }

        /// <summary>Текущий StateType</summary>
        private static StateTypes? CurrentState { get; set; }

        public Camera()
        {
            UsedCams = new List<int>();

            ID = -1;
            IsActive = false;
        }

        public static void Enable(StateTypes startType, Entity sourceEntity = null, Entity targetEntity = null, int transitionTime = 0, object sourceParams = null, object targetParams = null, Vector3 sourcePos = null)
        {
            if (IsActive)
            {
                Disable();
            }

            IsActive = true;

            ExecuteTasksSchedule?.Cancel();
            SourceTask?.Cancel();
            TargetTask?.Cancel();

            for (int i = 0; i < UsedCams.Count; i++)
                RAGE.Game.Cam.DestroyCam(UsedCams[i], false);

            UsedCams.Clear();

            ID = DefaultCamera;

            CurrentState = startType;

            var state = States[startType];

            if (transitionTime < 0)
                transitionTime = state.TransitionTime;

            ApplyState(state, sourceEntity, targetEntity, transitionTime, sourceParams, targetParams, sourcePos);

            RAGE.Game.Cam.SetCamActive(ID, true);

            if (transitionTime <= 0)
            {
                RAGE.Game.Cam.RenderScriptCams(true, false, 0, true, false, 0);
            }
            else
            {
                RAGE.Game.Cam.RenderScriptCams(true, true, transitionTime, true, false, 0);
            }
        }

        public static void Disable(int transitionTime = 0)
        {
            if (!IsActive)
                return;

            ExecuteTasksSchedule?.Cancel();
            SourceTask?.Cancel();
            TargetTask?.Cancel();

            RAGE.Game.Cam.SetCamActive(ID, false);

            RAGE.Game.Cam.RenderScriptCams(false, true, transitionTime, true, false, 0);

            UsedCams.Add(ID);

            IsActive = false;

            CurrentState = null;

            SourceEntity = null;
        }

        public static void FromState(StateTypes sType, Entity sourceEntity = null, Entity targetEntity = null, int transitionTime = 0, object sourceParams = null, object targetParams = null, Vector3 sourcePos = null)
        {
            if (!IsActive)
            {
                Enable(sType, sourceEntity, targetEntity, 0, sourceParams, targetParams);

                return;
            }

            if (CurrentState != null)
            {
                var curState = States[(StateTypes)CurrentState];

                curState.OffAction?.Invoke(null);
            }

            CurrentState = sType;

            ExecuteTasksSchedule?.Cancel();
            SourceTask?.Cancel();
            TargetTask?.Cancel();

            RAGE.Game.Cam.SetCamActive(ID, true);

            for (int i = 0; i < UsedCams.Count; i++)
                RAGE.Game.Cam.DestroyCam(UsedCams[i], false);

            UsedCams.Clear();

            var state = States[sType];

            if (transitionTime < 0)
                transitionTime = state.TransitionTime;

            if (transitionTime > 0)
            {
                var oldCam = ID;
                ID = DefaultCamera;

                UsedCams.Add(oldCam);

                ApplyState(state, sourceEntity, targetEntity, transitionTime, sourceParams, targetParams, sourcePos);

                RAGE.Game.Cam.SetCamActiveWithInterp(ID, oldCam, transitionTime, 4, 1);
            }
            else
            {
                ApplyState(state, sourceEntity, targetEntity, transitionTime, sourceParams, targetParams, sourcePos);

                RAGE.Game.Cam.SetCamActive(ID, true);
            }
        }

        private static void ApplyState(State state, Entity sourceEntity = null, Entity targetEntity = null, int transitionTime = 0, object sourceParams = null, object targetParams = null, Vector3 sourcePos = null)
        {
            SourceEntity = sourceEntity;

            state.OnAction?.Invoke(null);

            var sEntity = Utils.GetGameEntity(sourceEntity);
            var tEntity = Utils.GetGameEntity(targetEntity);

            MinFov = state.MinFov;
            MaxFov = state.MaxFov;

            Fov = state.Fov;

            Rotation = state.Rotation ?? new Vector3(0f, 0f, 0f);

            if (transitionTime > 0)
            {
                ExecuteTask(true, sEntity, RenderTypes.None, state.SourceBehaviourType, sourceParams ?? state.SourceParams, sourcePos ?? state.Position);
                ExecuteTask(false, tEntity, RenderTypes.None, state.TargetBehaviourType, targetParams ?? state.TargetParams, state.TargetPosition);

                ExecuteTasksSchedule = new AsyncTask(() =>
                {
                    if (state.SourceRenderType != RenderTypes.None)
                        ExecuteTask(true, sEntity, state.SourceRenderType, state.SourceBehaviourType, sourceParams ?? state.SourceParams, sourcePos ?? state.Position);

                    if (state.TargetRenderType != RenderTypes.None)
                        ExecuteTask(false, tEntity, state.TargetRenderType, state.TargetBehaviourType, targetParams ?? state.TargetParams, state.TargetPosition);

                    if (state.ShakeAmplitude > 0f)
                        RAGE.Game.Cam.ShakeCam(ID, "HAND_SHAKE", state.ShakeAmplitude);
                }, transitionTime, false, 0);

                ExecuteTasksSchedule.Run();
            }
            else
            {
                ExecuteTask(true, sEntity, state.SourceRenderType, state.SourceBehaviourType, sourceParams ?? state.SourceParams, sourcePos ?? state.Position);
                ExecuteTask(false, tEntity, state.TargetRenderType, state.TargetBehaviourType, targetParams ?? state.TargetParams, state.TargetPosition);

                if (state.ShakeAmplitude > 0f)
                    RAGE.Game.Cam.ShakeCam(ID, "HAND_SHAKE", state.ShakeAmplitude);
            }
        }

        private static void ExecuteTask(bool isSource, GameEntity entity, RenderTypes type, BehaviourTypes bType, object args, Vector3 position)
        {
            if (type == RenderTypes.None)
            {
                Vector3 pos = position;

                if (entity != null)
                {
                    if (bType == BehaviourTypes.FrontOf)
                    {
                        if (args is float[] arr)
                        {
                            pos = GetFrontOf(RAGE.Game.Entity.GetEntityCoords(entity.Handle, false), RAGE.Game.Entity.GetEntityHeading(entity.Handle) + arr[0], arr[1]) + position;
                        }
                    }
                    else if (bType == BehaviourTypes.PointAt)
                    {
                        pos = RAGE.Game.Entity.GetEntityCoords(entity.Handle, false) + position;
                    }
                    else if (bType == BehaviourTypes.PointBone)
                    {
                        pos = (Utils.GetBonePositionOfEntity(entity, args) ?? RAGE.Game.Entity.GetEntityCoords(entity.Handle, false)) + position;
                    }
                }

                if (isSource)
                    Position = pos;
                else
                    PointAtPos(pos);
            }
            else
            {
                Vector3 LastPosition = null;
                float LastHeading = entity == null ? 0f : RAGE.Game.Entity.GetEntityHeading(entity.Handle);

                if (args is float[] arr)
                    LastHeading += arr[0];

                var task = new AsyncTask(() =>
                {
                    if (!IsActive)
                        return;

                    if (entity != null)
                    {
                        if (bType == BehaviourTypes.FrontOf)
                        {
                            Vector3 pos = RAGE.Game.Entity.GetEntityCoords(entity.Handle, false);

                            float heading = 0f;
                            float dist = 0f;

                            if (args is float[] arr)
                            {
                                heading = RAGE.Game.Entity.GetEntityHeading(entity.Handle) + arr[0];
                                dist = arr[1];
                            }

                            if (type == RenderTypes.Both)
                            {
                                LastPosition = GetFrontOf(pos, heading, dist) + position;
                                LastHeading = heading;
                            }
                            else if (type == RenderTypes.Position)
                            {
                                if (LastHeading == heading)
                                {
                                    LastPosition = GetFrontOf(pos, heading, dist) + position;
                                }
                            }
                        }
                        else if (bType == BehaviourTypes.PointAt)
                        {
                            LastPosition = RAGE.Game.Entity.GetEntityCoords(entity.Handle, false) + position;
                        }
                        else if (bType == BehaviourTypes.PointBone)
                        {
                            LastPosition = (Utils.GetBonePositionOfEntity(entity, args) ?? RAGE.Game.Entity.GetEntityCoords(entity.Handle, false)) + position;
                        }

                        if (LastPosition != null)
                        {
                            if (isSource)
                            {
                                Position = LastPosition;
                            }
                            else
                            {
                                PointAtPos(LastPosition);
                            }
                        }
                    }

                }, 0, true, 0);

                if (isSource)
                    SourceTask = task;
                else
                    TargetTask = task;

                task.Run();
            }
        }

        #region Stuff

        public static void PointAtPos(Vector3 pos) => RAGE.Game.Cam.PointCamAtCoord(ID, pos.X, pos.Y, pos.Z);

        public static Vector3 GetFrontOf(Vector3 pos, float angle, float distance = 1.2f)
        {
            var radians = -angle * Math.PI / 180;

            var nX = (float)(pos.X + (distance * Math.Sin(radians)));
            var nY = (float)(pos.Y + (distance * Math.Cos(radians)));

            return new Vector3(nX, nY, pos.Z);
        }

        #endregion
    }
}
