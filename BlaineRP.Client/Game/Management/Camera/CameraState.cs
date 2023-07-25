using System;
using RAGE;

namespace BlaineRP.Client.Game.Management.Camera
{
    internal class CameraState
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
        /// <param name="position">Позиция камеры (если задана основная сущность, то этот параметр - смещение, а не сама позиция)</param>
        /// <param name="rotation">Поворот камеры (если null, то будет использоваться 0 0 0)</param>
        /// <param name="fov">Поле обзора</param>
        /// <param name="targetPosition">Целевая позиция (если задана целевая сущность, то этот параметр - смещение (от сущности/кости), а не сама позиция)</param>
        /// <param name="transitionTime">Время перехода от прошлой камеры к текущей</param>
        public CameraState(Vector3 position = null, Vector3 rotation = null, float fov = 50, Vector3 targetPosition = null, int transitionTime = 0, RenderTypes sourceRenderType = RenderTypes.None, RenderTypes targetRenderType = RenderTypes.None)
        {
            Position = position;
            Rotation = rotation;
            Fov = fov;
            TargetPosition = targetPosition;

            TransitionTime = transitionTime;

            SourceRenderType = sourceRenderType;
            TargetRenderType = targetRenderType;

            SourceBehaviourType = BehaviourTypes.None;
            TargetBehaviourType = BehaviourTypes.None;

            SourceParams = null;
            TargetParams = null;

            MinFov = fov;
            MaxFov = fov;

            ShakeAmplitude = 0.5f;
        }
    }
}