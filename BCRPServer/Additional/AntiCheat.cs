using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Additional
{
    class AntiCheat : Script
    {
        public AntiCheat()
        {

        }

        #region Legalization Methods
        public static void SetVehiclePos(Vehicle veh, Vector3 pos, uint? dimension = null)
        {
            veh.Position = pos;

            if (dimension is uint dim)
            {
                veh.Dimension = dim;

                foreach (var x in veh.Occupants)
                    if (x is Player player)
                        player.TriggerEvent("AC::State::TP::IV", pos, dim);
            }
            else
            {
                foreach (var x in veh.Occupants)
                    if (x is Player player)
                        player.TriggerEvent("AC::State::TP::IV", pos);
            }
        }

        /// <summary>Установить позицию игрока</summary>
        /// <remarks>Также, можно установить измерение игрока</remarks>
        /// <param name="player">Сущность игрока</param>
        /// <param name="pos">Новая позиция</param>
        /// <param name="toGround">Привязывать ли к земле?</param>
        /// <param name="dimension">Новое измерение</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static void SetPlayerPos(Player player, Vector3 pos, bool toGround, uint? dimension = null)
        {
            if (player?.Exists != true)
                return;

            if (dimension == null)
            {
                player.TriggerEvent("AC::State::TP", pos, toGround);
            }
            else
            {
                player.TriggerEvent("AC::State::TP", pos, toGround, dimension);

                player.Dimension = (uint)dimension;
            }

            if (pos == null)
            {
                pos = player.Position;
            }
            else
            {
                player.Position = pos;
            }

            var pData = player.GetMainData();

            if (pData != null)
            {
                pData.Respawn(pos, player.Heading, Utils.RespawnTypes.Teleport);
            }
            else
                NAPI.Player.SpawnPlayer(player, pos, player.Heading);
        }

        /// <summary>Установить здоровье игрока</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="value">Новое здоровье</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static void SetPlayerHealth(Player player, int value)
        {
            if (player?.Exists != true)
                return;

            player.TriggerEvent("AC::State::HP", value);

            //player.Health = value;
        }

        /// <summary>Установить бессмертность игрока</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="state">true - бессмертен, false - смертен</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static void SetPlayerInvincible(Player player, bool state)
        {
            if (player?.Exists != true)
                return;

            player.TriggerEvent("AC::State::Invincible", state);
        }

        /// <summary>Установить броню игрока</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="value">Новая броня</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static void SetPlayerArmour(Player player, int value)
        {
            if (player?.Exists != true)
                return;

            player.TriggerEvent("AC::State::Arm", value);

            //player.Armor = value;
        }

        /// <summary>Установить прозрачность игрока</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="value">От 0 до 255 (255 - полностью виден)</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static void SetPlayerAlpha(Player player, int value)
        {
            if (player?.Exists != true)
                return;

            player.TriggerEvent("AC::State::Alpha", value);

            player.Transparency = value;
        }

        /// <summary>Установить оружие игрока</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="hash">Хэш оружия</param>
        /// <param name="ammo">Кол-во патронов</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static void SetPlayerWeapon(Player player, uint hash, int ammo = 0)
        {
            if (player?.Exists != true)
                return;

            //ammo++;

            player.RemoveAllWeapons();

            player.TriggerEvent("AC::State::Weapon", ammo, hash);

            NAPI.Player.GivePlayerWeapon(player, hash, ammo);
        }

        /// <summary>Установить патроны игрока</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="amount">Кол-во патронов</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static void SetPlayerAmmo(Player player, int amount)
        {
            if (player?.Exists != true)
                return;

            //amount++;

            player.TriggerEvent("AC::State::Weapon", amount);

            //NAPI.Player.SetPlayerCurrentWeaponAmmo(player, amount);
        }
        #endregion

        #region Remote Events
        [RemoteEvent("AC::Detect::TP")]
        public static void DetectTP(Player sender, float dist)
        {
            var data = sender.GetMainData();

            if (data?.AdminLevel > 0)
                return;

/*            if (sender.CheckSpamAttack(false, 10000).IsSpammer)
                return;*/

            //Utils.MsgToAdmins(string.Format(Locale.Chat.Admin.TeleportWarning, sender.Name + $"({sender.Id}) | #{data?.CID.ToString() ?? "null"}"));

            NAPI.Util.ConsoleOutput($"BAC: Обнаружен TP | Дистанция: {dist} м. Игрок: {sender.Name} ({sender.Id}) | #{data?.CID.ToString() ?? "null"}");
        }
        #endregion
        }
}
