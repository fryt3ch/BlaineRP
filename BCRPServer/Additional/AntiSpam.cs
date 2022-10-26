using GTANetworkAPI;
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
            PlayerData pData;

            if (player?.Exists != true || ((pData = player.GetMainData()) == null))
            {
                Utils.KickSilent(player, "Подозрение в спам-атаке");

                return (true, null);
            }

            if (decreaseDelay <= 0)
                return (false, pData);

            var curSpams = player.GetData<int>($"Spam::Counter") + 1;

            player.SetData($"Spam::Counter", curSpams);

            Console.WriteLine($"SpamCounter: {curSpams}");

            if (curSpams > Settings.ANTISPAM_MAX_COUNT)
            {
                Utils.KickSilent(player, "Подозрение в спам-атаке");

                return (true, null);
            }
            else if (curSpams >= Settings.ANTISPAM_WARNING_COUNT)
                player.Notify("Spam::Warning", curSpams, Settings.ANTISPAM_MAX_COUNT);

            if (decreaseDelay != -1)
                NAPI.Task.Run(() =>
                {
                    if (player?.Exists != true)
                        return;

                    curSpams = player.GetData<int>("Spam::Counter");

                    player.SetData($"Spam::Counter", --curSpams);

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
            TempData tData;

            if (player?.Exists != true || ((tData = TempData.Get(player)) == null))
            {
                Utils.KickSilent(player, "Подозрение в спам-атаке");

                return (true, null);
            }

            if (decreaseDelay <= 0)
                return (false, tData);

            var curSpams = player.GetData<int>($"Spam::Counter") + 1;

            player.SetData($"Spam::Counter", curSpams);

            Console.WriteLine($"SpamCounter: {curSpams}");

            if (curSpams > Settings.ANTISPAM_MAX_COUNT)
            {
                Utils.KickSilent(player, "Подозрение в спам-атаке");

                return (true, null);
            }
            else if (curSpams >= Settings.ANTISPAM_WARNING_COUNT)
                player.Notify("Spam::Warning", curSpams, Settings.ANTISPAM_MAX_COUNT);

            if (decreaseDelay != -1)
                NAPI.Task.Run(() =>
                {
                    if (player?.Exists != true)
                        return;

                    curSpams = player.GetData<int>("Spam::Counter");

                    player.SetData($"Spam::Counter", --curSpams);

                    Console.WriteLine($"SpamCounter: {curSpams}");
                }, decreaseDelay);

            return (false, tData);
        }
    }
}
