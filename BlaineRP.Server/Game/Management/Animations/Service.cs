using System;
using BlaineRP.Server.EntitiesData.Players;

namespace BlaineRP.Server.Game.Management.Animations
{
    public static class Service
    {
        /// <summary>Проиграть быструю анимацию на игроке</summary>
        /// <remarks>Быстрая анимация НЕ проигрывается у игроков, которые вошли в зону стрима данного игрока после того, как она была запущена</remarks>
        /// <param name="pData">Сущность игрока</param>
        /// <param name="type">Тип анимации</param>
        public static void Play(PlayerData pData, FastType type, TimeSpan time)
        {
            var player = pData.Player;

            if (pData.CrawlOn)
            {
                pData.CrawlOn = false;
            }
            else if (!pData.StopGeneralAnim())
            {
                pData.StopOtherAnim();
            }

            pData.FastAnim = type;

            var timeout = time.TotalMilliseconds;

            player.TriggerEventToStreamed("Players::PlayFastAnim", player.Handle, (int)type, timeout);
        }

        /// <summary>Проиграть обычную анимацию на игроке</summary>
        /// <param name="pData">Сущность игрока</param>
        /// <param name="type">Тип анимации</param>
        public static void Play(PlayerData pData, GeneralType type)
        {
            var player = pData.Player;

            if (pData.CrawlOn)
            {
                pData.CrawlOn = false;
            }
            else if (!pData.StopFastAnim())
            {
                pData.StopOtherAnim();
            }

            pData.GeneralAnim = type;
        }

        /// <summary>Проиграть другую анимацию на игроке</summary>
        /// <param name="pData">Сущность игрока</param>
        /// <param name="type">Тип анимации</param>
        public static void Play(PlayerData pData, OtherType type)
        {
            var player = pData.Player;

            if (pData.CrawlOn)
            {
                pData.CrawlOn = false;
            }
            else if (!pData.StopFastAnim())
            {
                pData.StopGeneralAnim();
            }

            pData.OtherAnim = type;
        }

        public static bool StopFastAnim(PlayerData pData)
        {
            if (pData.FastAnim != FastType.None)
            {
                pData.FastAnim = FastType.None;

                pData.Player.TriggerEventToStreamed("Players::StopFastAnim", pData.Player.Id);

                return true;
            }

            return false;
        }

        public static bool StopGeneralAnim(PlayerData pData)
        {
            if (pData.GeneralAnim != GeneralType.None)
            {
                pData.GeneralAnim = GeneralType.None;

                return true;
            }

            return false;
        }

        public static bool StopOtherAnim(PlayerData pData)
        {
            if (pData.OtherAnim != OtherType.None)
            {
                pData.OtherAnim = OtherType.None;

                return true;
            }

            return false;
        }

        public static void Set(PlayerData pData, WalkstyleType walkstyle)
        {
            var player = pData.Player;

            pData.Walkstyle = walkstyle;
        }

        public static void Set(PlayerData pData, EmotionType emotion)
        {
            var player = pData.Player;

            pData.Emotion = emotion;
        }

        public static void StopAll(PlayerData pData)
        {
            if (pData.CrawlOn)
            {
                pData.CrawlOn = false;

                return;
            }

            if (pData.StopGeneralAnim())
                return;

            if (pData.StopFastAnim())
                return;

            if (pData.StopOtherAnim())
                return;
        }
    }
}
