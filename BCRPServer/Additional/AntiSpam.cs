﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.Additional
{
    class AntiSpam
    {
        /// <summary>Метод для проверки игрока на спам-аттаку (игрок должен иметь PlayerData)</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="decreaseDelay">Время в мс. для вычитания 1 из счётчика спама игрока (-1 - не вычитать)</param>
        /// <returns>true - игрок является спаммером и будет кикнут, false - все хорошо :)</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static (bool IsSpammer, PlayerData Data) CheckNormal(Player player, int decreaseDelay = 5000)
        {
            PlayerData pData = player.GetMainData();

            if (pData == null || !player.Exists)
            {
                Utils.KickSilent(player, "Подозрение в спам-атаке");

                return (true, null);
            }

            if (pData.BlockRemoteCalls)
                return (true, null);

            if (decreaseDelay <= 0)
                return (false, pData);

            var curSpams = ++pData.SpamCounter;

            Console.WriteLine($"SpamCounter: {curSpams}");

            if (curSpams > Settings.ANTISPAM_MAX_COUNT)
            {
                Utils.KickSilent(player, "Подозрение в спам-атаке");

                return (true, null);
            }
            else if (curSpams >= Settings.ANTISPAM_WARNING_COUNT)
            {
                player.Notify("Spam::Warning", curSpams, Settings.ANTISPAM_MAX_COUNT);
            }

            if (decreaseDelay != -1)
                NAPI.Task.Run(() =>
                {
                    if (player?.Exists != true)
                        return;

                    curSpams = --pData.SpamCounter;

                    pData.SpamCounter = curSpams;

                    Console.WriteLine($"SpamCounter: {curSpams}");
                }, decreaseDelay);

            return (false, pData);
        }

        /// <summary>Метод для проверки игрока на спам-аттаку (игрок должен иметь TempData)</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="decreaseDelay">Время в мс. для вычитания 1 из счётчика спама игрока (-1 - не вычитать)</param>
        /// <returns>true - игрок является спаммером и будет кикнут, false - все хорошо :)</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static (bool IsSpammer, TempData Data) CheckTemp(Player player, int decreaseDelay = 5000)
        {
            TempData tData = TempData.Get(player);

            if (tData == null || !player.Exists)
            {
                Utils.KickSilent(player, "Подозрение в спам-атаке");

                return (true, null);
            }

            if (tData.BlockRemoteCalls)
                return (true, null);

            if (decreaseDelay <= 0)
                return (false, tData);

            var curSpams = ++tData.SpamCounter;

            Console.WriteLine($"SpamCounter: {curSpams}");

            if (curSpams > Settings.ANTISPAM_MAX_COUNT)
            {
                Utils.KickSilent(player, "Подозрение в спам-атаке");

                return (true, null);
            }
            else if (curSpams >= Settings.ANTISPAM_WARNING_COUNT)
            {
                player.Notify("Spam::Warning", curSpams, Settings.ANTISPAM_MAX_COUNT);
            }

            if (decreaseDelay != -1)
                NAPI.Task.Run(() =>
                {
                    if (player?.Exists != true)
                        return;

                    curSpams = --tData.SpamCounter;

                    Console.WriteLine($"SpamCounter: {curSpams}");
                }, decreaseDelay);

            return (false, tData);
        }
    }
}
