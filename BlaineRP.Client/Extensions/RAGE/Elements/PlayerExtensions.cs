using BlaineRP.Client.Utils.Game;
using RAGE.Elements;
using System.Linq;

namespace BlaineRP.Client.Extensions.RAGE.Elements
{
    public static class PlayerExtensions
    {
        public static bool IsFamilliar(this Player player, bool fractionToo = true) => Players.IsPlayerFamiliar(player, fractionToo);
        public static string GetName(this Player player, bool familiarOnly = true, bool donNotMask = true, bool includeId = false) => Players.GetPlayerName(player, familiarOnly, donNotMask, includeId);

        /// <summary>Устанавливает настоящий уровень здоровья игроку</summary>
        /// <remarks>Метод прибавляет 100 к передаваемому значению</remarks>
        /// <param name="player">Игрок</param>
        /// <param name="value">Значение</param>
        public static void SetRealHealth(this Player player, int value)
        {
            if (value <= 0)
                player.SetHealth(0);
            else
                player.SetHealth(100 + value);
        }

        /// <summary>Проверка, держит ли игрок в руках оружие (игнорируется кулак и телефон)</summary>
        /// <param name="player"></param>
        public static bool HasWeapon(this Player player)
        {
            var weapon = player.GetSelectedWeapon();

            return weapon != Sync.WeaponSystem.UnarmedHash && weapon != Sync.WeaponSystem.MobileHash;
        }

        public static void GetNameSurname(this Player player, out string name, out string surname)
        {
            if (player == null || (player.Name?.Length ?? 0) < 2)
            {
                name = "null";
                surname = "null";

                return;
            }

            var nameArray = player.Name.Split(' ').ToArray();

            if (nameArray.Length >= 2)
            {
                name = nameArray[0];
                surname = nameArray[1];

                return;
            }

            name = "null";
            surname = "null";
        }

        /// <summary>Получить настоящий уровень здоровья игрока</summary>
        /// <remarks>Метод отнимает 100 от игрового уровня здоровья</remarks>
        /// <param name="player">Игрок</param>
        /// <returns>Значение от 0 до int.MaxValue - 100</returns>
        public static int GetRealHealth(this Player player)
        {
            var hp = player.GetHealth() - 100;

            return hp < 0 ? 0 : hp;
        }

        public static bool GetSex(this Player player) => player.Model == 0x705E61F2;
    }
}