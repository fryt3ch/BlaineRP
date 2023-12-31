﻿using GTANetworkAPI;

namespace BlaineRP.Server.Game.Management.Misc
{
    public static partial class SkyCamera
    {
        /// <summary>Метод для взаимодействия с небесной камерой</summary>
        /// <param name="player">Игрок</param>
        /// <param name="fade">Затемнить изображение?</param>
        /// <param name="switchType">Тип переключения</param>
        /// <param name="eventOnFinish">Выполняемое событие на клиенте по завершении переключения</param>
        /// <param name="args">Аргументы для события</param>
        public static void Move(Player player, SwitchType switchType, bool fade, string eventOnFinish = null, params object[] args)
        {
            if (eventOnFinish == null)
            {
                player.TriggerEvent("SkyCamera::Move", switchType, fade);
            }
            else
            {
                if (args.Length > 0)
                {
                    player.TriggerEvent("SkyCamera::Move", switchType, fade, eventOnFinish, args);
                }
                else
                {
                    player.TriggerEvent("SkyCamera::Move", switchType, fade, eventOnFinish);
                }
            }
        }
    }
}
