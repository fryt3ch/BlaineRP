using System;
using BlaineRP.Server.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Management
{
    class AntiSpam
    {
        /// <summary>Метод для проверки игрока на спам-аттаку (игрок должен иметь PlayerData)</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="decreaseDelay">Время в мс. для вычитания 1 из счётчика спама игрока (-1 - не вычитать)</param>
        /// <returns>true - игрок является спаммером и будет кикнут, false - все хорошо :)</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static (bool IsSpammer, PlayerData Data) CheckNormal(Player player, int decreaseDelay = 5000, bool checkPlayerReady = true)
        {
            var pData = player.GetMainData();

            if (pData == null || !player.Exists)
            {
                Utils.Kick(player, "Подозрение в спам-атаке");

                return (true, null);
            }

            if (checkPlayerReady && player.HasData("CharacterNotReady"))
                return (true, null);

            if (pData.BlockRemoteCalls)
                return (true, null);

            if (decreaseDelay <= 0)
                return (false, pData);

            var curSpams = ++pData.SpamCounter;

            //Console.WriteLine($"SpamCounter: {curSpams}");

            if (curSpams > Properties.Settings.Static.ANTISPAM_MAX_COUNT)
            {
                Utils.Kick(player, "Подозрение в спам-атаке");

                return (true, null);
            }
            else if (curSpams >= Properties.Settings.Static.ANTISPAM_WARNING_COUNT)
            {
                player.Notify("Spam::Warning", curSpams, Properties.Settings.Static.ANTISPAM_MAX_COUNT);
            }

            if (decreaseDelay != -1)
                NAPI.Task.Run(() =>
                {
                    if (player?.Exists != true)
                        return;

                    curSpams = --pData.SpamCounter;

                    pData.SpamCounter = curSpams;

                    //Console.WriteLine($"SpamCounter: {curSpams}");
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
                Utils.Kick(player, "Подозрение в спам-атаке");

                return (true, null);
            }

            if (tData.BlockRemoteCalls)
                return (true, null);

            if (decreaseDelay <= 0)
                return (false, tData);

            var curSpams = ++tData.SpamCounter;

            Console.WriteLine($"SpamCounter: {curSpams}");

            if (curSpams > Properties.Settings.Static.ANTISPAM_MAX_COUNT)
            {
                Utils.Kick(player, "Подозрение в спам-атаке");

                return (true, null);
            }
            else if (curSpams >= Properties.Settings.Static.ANTISPAM_WARNING_COUNT)
            {
                player.Notify("Spam::Warning", curSpams, Properties.Settings.Static.ANTISPAM_MAX_COUNT);
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
