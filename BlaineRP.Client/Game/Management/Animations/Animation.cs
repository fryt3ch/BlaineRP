using System;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management.Animations
{
    /// <summary>Класс анимации</summary>
    public class Animation
    {
        public Animation(string Dict,
                         string Name,
                         float BlendInSpeed = 8f,
                         float BlendOutSpeed = 1f,
                         int Duration = -1,
                         int Flag = 0,
                         float StartOffset = 0f,
                         bool BlockX = false,
                         bool BlockY = false,
                         bool BlockZ = false)
        {
            this.Dict = Dict;
            this.Name = Name;
            this.BlendInSpeed = BlendInSpeed;
            this.BlendOutSpeed = BlendOutSpeed;
            this.Duration = Duration;
            this.Flag = Flag;
            this.StartOffset = StartOffset;
            this.BlockX = BlockX;
            this.BlockY = BlockY;
            this.BlockZ = BlockZ;
        }

        /// <summary>Словарь</summary>
        public string Dict { get; private set; }

        /// <summary>Название</summary>
        public string Name { get; private set; }

        /// <summary>Скорость входа в анимацию</summary>
        public float BlendInSpeed { get; private set; }

        /// <summary>Скорость выхода из анимации</summary>
        public float BlendOutSpeed { get; private set; }

        /// <summary>Продолжительность</summary>
        public int Duration { get; private set; }

        /// <summary>Флаг</summary>
        public int Flag { get; private set; }

        /// <summary>Смещение начала</summary>
        public float StartOffset { get; private set; }

        public bool BlockX { get; private set; }

        public bool BlockY { get; private set; }

        public bool BlockZ { get; private set; }

        /// <summary>Название для первого лица</summary>
        public string NameFP { get; set; }

        public Action<Entity, Animation> StartAction { get; set; }
        public Action<Entity, Animation> StopAction { get; set; }
    }
}