using System.Collections.Generic;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management.Camera
{
    partial class Core
    {
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

        private static void ApplyState(CameraState state, Entity sourceEntity = null, Entity targetEntity = null, int transitionTime = 0, object sourceParams = null, object targetParams = null, Vector3 sourcePos = null)
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
                ExecuteTask(true, sEntity, CameraState.RenderTypes.None, state.SourceBehaviourType, sourceParams ?? state.SourceParams, sourcePos ?? state.Position);
                ExecuteTask(false, tEntity, CameraState.RenderTypes.None, state.TargetBehaviourType, targetParams ?? state.TargetParams, state.TargetPosition);

                ExecuteTasksSchedule = new AsyncTask(() =>
                {
                    if (state.SourceRenderType != CameraState.RenderTypes.None)
                        ExecuteTask(true, sEntity, state.SourceRenderType, state.SourceBehaviourType, sourceParams ?? state.SourceParams, sourcePos ?? state.Position);

                    if (state.TargetRenderType != CameraState.RenderTypes.None)
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

        private static void ExecuteTask(bool isSource, GameEntity entity, CameraState.RenderTypes type, CameraState.BehaviourTypes bType, object args, Vector3 position)
        {
            if (type == CameraState.RenderTypes.None)
            {
                var pos = position;

                if (entity != null)
                {
                    if (bType == CameraState.BehaviourTypes.FrontOf)
                    {
                        if (args is float[] arr)
                        {
                            pos = GetFrontOf(RAGE.Game.Entity.GetEntityCoords(entity.Handle, false), RAGE.Game.Entity.GetEntityHeading(entity.Handle) + arr[0], arr[1]) + position;
                        }
                    }
                    else if (bType == CameraState.BehaviourTypes.PointAt)
                    {
                        pos = RAGE.Game.Entity.GetEntityCoords(entity.Handle, false) + position;
                    }
                    else if (bType == CameraState.BehaviourTypes.PointBone)
                    {
                        pos = (Utils.Game.Misc.GetBonePositionOfEntity(entity, args) ?? RAGE.Game.Entity.GetEntityCoords(entity.Handle, false)) + position;
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
                        if (bType == CameraState.BehaviourTypes.FrontOf)
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

                            if (type == CameraState.RenderTypes.Both)
                            {
                                heading = RAGE.Game.Entity.GetEntityHeading(entity.Handle) + headingDiff;

                                LastPosition = GetFrontOf(pos, heading, dist) + position;
                                LastHeading = heading;
                            }
                            else if (type == CameraState.RenderTypes.Position)
                            {
                                LastPosition = GetFrontOf(pos, LastHeading + headingDiff, dist) + position;
                            }
                        }
                        else if (bType == CameraState.BehaviourTypes.PointAt)
                        {
                            LastPosition = RAGE.Game.Entity.GetEntityCoords(entity.Handle, false) + position;
                        }
                        else if (bType == CameraState.BehaviourTypes.PointBone)
                        {
                            LastPosition = (Utils.Game.Misc.GetBonePositionOfEntity(entity, args) ?? RAGE.Game.Entity.GetEntityCoords(entity.Handle, false)) + position;
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
        
        public static void PointAtPos(Vector3 pos) => RAGE.Game.Cam.PointCamAtCoord(Id, pos.X, pos.Y, pos.Z);

        public static Vector3 GetFrontOf(Vector3 pos, float angle, float distance = 1.2f)
        {
            var radians = -angle * System.Math.PI / 180;

            var nX = (float)(pos.X + (distance * System.Math.Sin(radians)));
            var nY = (float)(pos.Y + (distance * System.Math.Cos(radians)));

            return new Vector3(nX, nY, pos.Z);
        }
    }
}
