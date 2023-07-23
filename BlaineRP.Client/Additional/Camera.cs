﻿using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using static BlaineRP.Client.Additional.Camera.State;

namespace BlaineRP.Client.Additional
{
    class Camera
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

            BodyUpper,
            BodyBackUpper,
            RightHandUpper,
            LeftHandUpper,
            LeftLeg,
            RightLeg,

            BodyBack,

            RightHandFingers,
            LeftHandFingers,

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

            CasinoRouletteGame,

            Empty,

            WholeFurniture,
            FrontFurniture,
            TopFurniture,
        }

        public static Dictionary<StateTypes, State> States = new Dictionary<StateTypes, State>()
        {
            { StateTypes.Foots, new State(new Vector3(0f, 0f, -0.5f), new Vector3(0f, 0f, 0f), 40, new Vector3(0f, 0f, -0.75f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = State.BehaviourTypes.PointBone, TargetParams = 23553, MinFov = 10 } },

            { StateTypes.Legs, new State(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 55, new Vector3(0f, 0f, -0.25f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = State.BehaviourTypes.PointBone, TargetParams = 23553, MinFov = 10 } },

            { StateTypes.LeftLeg, new State(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 55, new Vector3(0f, 0f, -0.25f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = State.BehaviourTypes.PointBone, TargetParams = 46078, MinFov = 10 } },
            { StateTypes.RightLeg, new State(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 55, new Vector3(0f, 0f, -0.25f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = State.BehaviourTypes.PointBone, TargetParams = 16335, MinFov = 10 } },

            { StateTypes.Body, new State(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 60, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = State.BehaviourTypes.PointBone, TargetParams = 23553, MinFov = 10 } },

            { StateTypes.BodyUpper, new State(new Vector3(0f, 0f, 0.5f), new Vector3(0f, 0f, 0f), 60, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = State.BehaviourTypes.PointBone, TargetParams = 23553, MinFov = 10 } },

            { StateTypes.BodyBack, new State(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 60, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 180f, 1.2f }, TargetBehaviourType = State.BehaviourTypes.PointBone, TargetParams = 23553, MinFov = 10 } },

            { StateTypes.BodyBackUpper, new State(new Vector3(0f, 0f, 0.5f), new Vector3(0f, 0f, 0f), 60, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 180f, 1.2f }, TargetBehaviourType = State.BehaviourTypes.PointBone, TargetParams = 23553, MinFov = 10 } },

            { StateTypes.Head, new State(new Vector3(0, 0, 1f), new Vector3(0f, 0f, 0f), 30, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.Position, State.RenderTypes.Position) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = State.BehaviourTypes.PointBone, TargetParams = 31086, MinFov = 10 } },

            { StateTypes.LeftHand, new State(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 30, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = State.BehaviourTypes.PointBone, TargetParams = 36029, MinFov = 10 } },

            { StateTypes.LeftHandFingers, new State(new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, 0f), 25, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 45f, 1.2f }, TargetBehaviourType = State.BehaviourTypes.PointBone, TargetParams = 4137, MinFov = 10 } },

            { StateTypes.LeftHandUpper, new State(new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 0f), 30, new Vector3(0f, 0f, 0.2f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = State.BehaviourTypes.PointBone, TargetParams = 36029, MinFov = 10 } },

            { StateTypes.RightHand, new State(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 30, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = State.BehaviourTypes.PointBone, TargetParams = 57005, MinFov = 10 } },

            { StateTypes.RightHandUpper, new State(new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 0f), 30, new Vector3(0f, 0f, 0.2f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = State.BehaviourTypes.PointBone, TargetParams = 57005, MinFov = 10 } },

            { StateTypes.RightHandFingers, new State(new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, 0f), 25, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { -45f, 1.2f }, TargetBehaviourType = State.BehaviourTypes.PointBone, TargetParams = 64064, MinFov = 10 } },

            { StateTypes.WholePed, new State(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 80, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.Position, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.5f }, TargetBehaviourType = State.BehaviourTypes.PointBone, TargetParams = 23553, MinFov = 10 } },

            { StateTypes.NpcTalk, new State(new Vector3(0f, 0f, 1f), null, 30, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.Both, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 1.2f }, TargetBehaviourType = State.BehaviourTypes.PointBone, TargetParams = 31086, MinFov = 10 } },

            { StateTypes.WholeVehicle, new State(new Vector3(0f, 0f, 1.35f), null, 60, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.None, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 45f, 5.5f }, TargetBehaviourType = State.BehaviourTypes.PointAt, MinFov = 10 } },

            { StateTypes.WholeVehicleOpen, new State(new Vector3(0f, 0f, 1.35f), new Vector3(0f, 0f, 0f), 60, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.None, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 45f, 5.5f }, TargetBehaviourType = State.BehaviourTypes.PointAt, MinFov = 10,

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

            { StateTypes.FrontVehicle, new State(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 70, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.None, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 5f }, TargetBehaviourType = State.BehaviourTypes.PointAt, MinFov = 10 } },

            { StateTypes.FrontVehicleOpenHood, new State(new Vector3(0, 0, 1f), new Vector3(0f, 0f, 0f), 70, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.None, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 0f, 5f }, TargetBehaviourType = State.BehaviourTypes.PointAt, MinFov = 10,

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

            { StateTypes.BackVehicleOpenTrunk, new State(new Vector3(0, 0, 1f), new Vector3(0f, 0f, 0f), 70, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.None, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { -180f, 5f }, TargetBehaviourType = State.BehaviourTypes.PointAt, MinFov = 10,

                OnAction = (args) =>
                {
                    if (SourceEntity is Vehicle veh)
                    {
                        RAGE.Game.Entity.SetEntityHeading(veh.Handle, RAGE.Game.Entity.GetEntityHeading(veh.Handle) - 180f);

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

            { StateTypes.BackVehicle, new State(new Vector3(0, 0, 0), new Vector3(0f, 0f, 0f), 70, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.None, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { -180, 5f }, TargetBehaviourType = State.BehaviourTypes.PointAt, MinFov = 10,

                OnAction = (args) =>
                {
                    if (SourceEntity is GameEntity gEntity)
                    {
                        RAGE.Game.Entity.SetEntityHeading(gEntity.Handle, RAGE.Game.Entity.GetEntityHeading(gEntity.Handle) - 180f);
                    }
                }
            } },

            { StateTypes.BackVehicleUpAngle, new State(new Vector3(0, 0, 1.35f), new Vector3(0f, 0f, 0f), 60, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.None, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 210f, 5f }, TargetBehaviourType = State.BehaviourTypes.PointAt, MinFov = 10,

                OnAction = (args) =>
                {
                    if (SourceEntity is GameEntity gEntity)
                    {
                        RAGE.Game.Entity.SetEntityHeading(gEntity.Handle, RAGE.Game.Entity.GetEntityHeading(gEntity.Handle) + 210f);
                    }
                }
            } },

            { StateTypes.RightVehicle, new State(new Vector3(0, 0, 0), new Vector3(0f, 0f, 0f), 80, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.None, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 90f, 3.5f }, TargetBehaviourType = State.BehaviourTypes.PointAt, MinFov = 10 } },

            { StateTypes.LeftVehicle, new State(new Vector3(0, 0, 0), new Vector3(0f, 0f, 0f), 80, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.None, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { -90f, 3.5f }, TargetBehaviourType = State.BehaviourTypes.PointAt, MinFov = 10 } },

            { StateTypes.TopVehicle, new State(new Vector3(0, 0, 4f), new Vector3(0f, 0f, 0f), 70, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.None, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.PointAt, TargetBehaviourType = State.BehaviourTypes.PointAt, MinFov = 10 } },

            { StateTypes.CasinoRouletteGame, new State(new Vector3(0f, 0f, 2f), new Vector3(0f, 0f, 0f), 80, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.None, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.PointAt, TargetBehaviourType = State.BehaviourTypes.None, ShakeAmplitude = 0f, } },

            { StateTypes.Empty, new State(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), 0, new Vector3(0f, 0f, 0f), 0, State.RenderTypes.None, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.None, TargetBehaviourType = State.BehaviourTypes.None, ShakeAmplitude = 0f, MinFov = 0f, MaxFov = 180f, } },

            { StateTypes.WholeFurniture, new State(new Vector3(0f, 0f, 1.35f), null, 60, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.None, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 180f + 35f, 5.5f }, TargetBehaviourType = State.BehaviourTypes.PointAt, MinFov = 5 } },
            { StateTypes.FrontFurniture, new State(new Vector3(0f, 0f, 0f), null, 60, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.None, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.FrontOf, SourceParams = new float[] { 180f, 5f }, TargetBehaviourType = State.BehaviourTypes.PointAt, MinFov = 5 } },
            { StateTypes.TopFurniture, new State(new Vector3(0f, 0f, 3f), null, 70, new Vector3(0f, 0f, 0f), 750, State.RenderTypes.None, State.RenderTypes.None) { SourceBehaviourType = State.BehaviourTypes.PointAt, TargetBehaviourType = State.BehaviourTypes.PointAt, MinFov = 5 } },
        };

        /// <summary>Минимально возможный FOV</summary>
        private static float MinFov { get; set; }

        /// <summary>Максимально возможный FOV</summary>
        private static float MaxFov { get; set; }

        /// <summary>ID текущей камеры</summary>
        private static int Id { get; set; } = -1;

        /// <summary>Позиция текущей камеры</summary>
        public static Vector3 Position { get => RAGE.Game.Cam.GetCamCoord(Id); set { RAGE.Game.Cam.SetCamCoord(Id, value.X, value.Y, value.Z); } }

        /// <summary>Поворот текущей камеры</summary>
        public static Vector3 Rotation { get => RAGE.Game.Cam.GetCamRot(Id, 2); set { RAGE.Game.Cam.SetCamRot(Id, value.X, value.Y, value.Z, 2); } }

        /// <summary>Поле обзора</summary>
        public static float Fov { get => RAGE.Game.Cam.GetCamFov(Id); set { if (value > MaxFov) return; if (value < MinFov) return; RAGE.Game.Cam.SetCamFov(Id, value); } }

        /// <summary>Активна ли камера?</summary>
        public static bool IsActive { get; private set; }

        /// <summary>Использованные камеры, подлежат удалению</summary>
        private static List<int> UsedCams { get; } = new List<int>();

        /// <summary>Хэш стандартной камеры</summary>
        private static uint DefaultScriptedCameraHash { get; } = RAGE.Game.Misc.GetHashKey("DEFAULT_SCRIPTED_CAMERA");

        private static Entity SourceEntity { get; set; }

        private static AsyncTask SourceTask { get; set; }
        private static AsyncTask TargetTask { get; set; }
        private static AsyncTask ExecuteTasksSchedule { get; set; }

        /// <summary>Получить новую стандартную камеру</summary>
        private static int DefaultCamera => RAGE.Game.Cam.CreateCameraWithParams(DefaultScriptedCameraHash, 0f, 0f, 0f, 0f, 0f, 0f, 0f, true, 2);

        /// <summary>Текущий StateType</summary>
        private static StateTypes? CurrentState { get; set; }

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

            UsedCams.ForEach(x => RAGE.Game.Cam.DestroyCam(x, false));

            UsedCams.Clear();

            Id = DefaultCamera;

            CurrentState = startType;

            var state = States[startType];

            if (transitionTime < 0)
                transitionTime = state.TransitionTime;

            ApplyState(state, sourceEntity, targetEntity, transitionTime, sourceParams, targetParams, sourcePos);

            RAGE.Game.Cam.SetCamActive(Id, true);

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

            RAGE.Game.Cam.SetCamActive(Id, false);

            RAGE.Game.Cam.RenderScriptCams(false, true, transitionTime, true, false, 0);

            UsedCams.Add(Id);

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

            RAGE.Game.Cam.SetCamActive(Id, true);

            for (int i = 0; i < UsedCams.Count; i++)
                RAGE.Game.Cam.DestroyCam(UsedCams[i], false);

            UsedCams.Clear();

            var state = States[sType];

            if (transitionTime < 0)
                transitionTime = state.TransitionTime;

            if (transitionTime > 0)
            {
                var oldCam = Id;
                Id = DefaultCamera;

                UsedCams.Add(oldCam);

                ApplyState(state, sourceEntity, targetEntity, transitionTime, sourceParams, targetParams, sourcePos);

                RAGE.Game.Cam.SetCamActiveWithInterp(Id, oldCam, transitionTime, 4, 1);
            }
            else
            {
                ApplyState(state, sourceEntity, targetEntity, transitionTime, sourceParams, targetParams, sourcePos);

                RAGE.Game.Cam.SetCamActive(Id, true);
            }
        }

        private static void ApplyState(State state, Entity sourceEntity = null, Entity targetEntity = null, int transitionTime = 0, object sourceParams = null, object targetParams = null, Vector3 sourcePos = null)
        {
            SourceEntity = sourceEntity;

            state.OnAction?.Invoke(null);

            var sEntity = (GameEntity)sourceEntity;
            var tEntity = (GameEntity)targetEntity;

            MinFov = state.MinFov;
            MaxFov = state.MaxFov;

            Fov = state.Fov;

            Rotation = state.Rotation ?? new Vector3(0f, 0f, 0f);

            if (transitionTime > 0)
            {
                ExecuteTask(true, sEntity, State.RenderTypes.None, state.SourceBehaviourType, sourceParams ?? state.SourceParams, sourcePos ?? state.Position);
                ExecuteTask(false, tEntity, State.RenderTypes.None, state.TargetBehaviourType, targetParams ?? state.TargetParams, state.TargetPosition);

                ExecuteTasksSchedule = new AsyncTask(() =>
                {
                    if (state.SourceRenderType != State.RenderTypes.None)
                        ExecuteTask(true, sEntity, state.SourceRenderType, state.SourceBehaviourType, sourceParams ?? state.SourceParams, sourcePos ?? state.Position);

                    if (state.TargetRenderType != State.RenderTypes.None)
                        ExecuteTask(false, tEntity, state.TargetRenderType, state.TargetBehaviourType, targetParams ?? state.TargetParams, state.TargetPosition);

                    if (state.ShakeAmplitude > 0f)
                        RAGE.Game.Cam.ShakeCam(Id, "HAND_SHAKE", state.ShakeAmplitude);
                }, transitionTime, false, 0);

                ExecuteTasksSchedule.Run();
            }
            else
            {
                ExecuteTask(true, sEntity, state.SourceRenderType, state.SourceBehaviourType, sourceParams ?? state.SourceParams, sourcePos ?? state.Position);
                ExecuteTask(false, tEntity, state.TargetRenderType, state.TargetBehaviourType, targetParams ?? state.TargetParams, state.TargetPosition);

                if (state.ShakeAmplitude > 0f)
                    RAGE.Game.Cam.ShakeCam(Id, "HAND_SHAKE", state.ShakeAmplitude);
            }
        }

        private static void ExecuteTask(bool isSource, GameEntity entity, State.RenderTypes type, State.BehaviourTypes bType, object args, Vector3 position)
        {
            if (type == State.RenderTypes.None)
            {
                var pos = position;

                if (entity != null)
                {
                    if (bType == State.BehaviourTypes.FrontOf)
                    {
                        if (args is float[] arr)
                        {
                            pos = GetFrontOf(RAGE.Game.Entity.GetEntityCoords(entity.Handle, false), RAGE.Game.Entity.GetEntityHeading(entity.Handle) + arr[0], arr[1]) + position;
                        }
                    }
                    else if (bType == State.BehaviourTypes.PointAt)
                    {
                        pos = RAGE.Game.Entity.GetEntityCoords(entity.Handle, false) + position;
                    }
                    else if (bType == State.BehaviourTypes.PointBone)
                    {
                        pos = (Utils.GetBonePositionOfEntity(entity, args) ?? RAGE.Game.Entity.GetEntityCoords(entity.Handle, false)) + position;
                    }

                    if (isSource)
                        Position = pos;
                    else
                        PointAtPos(pos);
                }
            }
            else
            {
                Vector3 LastPosition = null;
                var LastHeading = RAGE.Game.Entity.GetEntityHeading(entity.Handle);

                var task = new AsyncTask(() =>
                {
                    if (!IsActive)
                        return;

                    if (entity != null)
                    {
                        if (bType == State.BehaviourTypes.FrontOf)
                        {
                            Vector3 pos = RAGE.Game.Entity.GetEntityCoords(entity.Handle, false);

                            var heading = LastHeading;
                            var dist = 0f;
                            var headingDiff = 0f;

                            if (args is float[] arr)
                            {
                                headingDiff = arr[0];
                                dist = arr[1];
                            }

                            if (type == State.RenderTypes.Both)
                            {
                                heading = RAGE.Game.Entity.GetEntityHeading(entity.Handle) + headingDiff;

                                LastPosition = GetFrontOf(pos, heading, dist) + position;
                                LastHeading = heading;
                            }
                            else if (type == State.RenderTypes.Position)
                            {
                                LastPosition = GetFrontOf(pos, LastHeading + headingDiff, dist) + position;
                            }
                        }
                        else if (bType == State.BehaviourTypes.PointAt)
                        {
                            LastPosition = RAGE.Game.Entity.GetEntityCoords(entity.Handle, false) + position;
                        }
                        else if (bType == State.BehaviourTypes.PointBone)
                        {
                            LastPosition = (Utils.GetBonePositionOfEntity(entity, args) ?? RAGE.Game.Entity.GetEntityCoords(entity.Handle, false)) + position;
                        }

                        if (LastPosition != null)
                        {
                            //Utils.ConsoleOutputLimited(RAGE.Util.Json.Serialize(LastPosition) + $" - {isSource}");

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

        public static void PointAtPos(Vector3 pos) => RAGE.Game.Cam.PointCamAtCoord(Id, pos.X, pos.Y, pos.Z);

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