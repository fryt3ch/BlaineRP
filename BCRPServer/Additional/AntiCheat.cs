using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Additional
{
    public class AntiCheat
    {
        public enum VehicleTeleportTypes
        {
            /// <summary>Без игроков</summary>
            Default = 0,
            /// <summary>Вместе с водителем</summary>
            OnlyDriver,
            /// <summary>Вместе со всеми пассажирами</summary>
            All,
        }

        #region Legalization Methods
        public static void SetVehiclePos(Vehicle veh, Vector3 pos, uint? dimension = null, float? heading = null, bool fade = false, VehicleTeleportTypes tpType = VehicleTeleportTypes.Default)
        {
            veh.DetachAllEntities();

            veh.GetEntityIsAttachedTo()?.DetachEntity(veh);

            var lastDim = veh.Dimension;

            if (tpType == VehicleTeleportTypes.Default)
            {
                if (dimension is uint dim)
                {
                    veh.Dimension = dim;

                    veh.Occupants.ForEach(x =>
                    {
                        if (x is Player player)
                        {
                            player.WarpOutOfVehicle();

                            player.Dimension = lastDim;
                        }
                    });
                }

                veh.Position = pos;

                if (heading is float headingF)
                    veh.SetHeading(headingF);
            }
            else if (tpType == VehicleTeleportTypes.OnlyDriver)
            {
                if (dimension is uint dim)
                    veh.Dimension = dim;

                var wasDriver = false;

                veh.Occupants.ForEach(x =>
                {
                    if (x is Player player)
                    {
                        if (player.VehicleSeat == 0)
                        {
                            SetPlayerPos(pos, false, veh.Dimension, heading, false, true, player);

                            wasDriver = true;
                        }
                        else
                        {
                            player.WarpOutOfVehicle();

                            if (player.Dimension != lastDim)
                                player.Dimension = lastDim;
                        }
                    }
                });

                if (!wasDriver)
                {
                    veh.Position = pos;

                    if (heading is float headingF)
                        veh.SetHeading(headingF);
                }
            }
            else if (tpType == VehicleTeleportTypes.All)
            {
                if (dimension is uint dim)
                    veh.Dimension = dim;

                var occupants = new List<Player>();

                veh.Occupants.ForEach(x =>
                {
                    if (x is Player player)
                        occupants.Add(player);
                });

                if (occupants.Count == 0)
                {
                    veh.Position = pos;

                    if (heading is float headingF)
                        veh.SetHeading(headingF);
                }
                else
                {
                    SetPlayerPos(pos, false, null, heading, false, true, occupants.ToArray());
                }
            }
        }

        /// <summary>Установить позицию игрока</summary>
        /// <remarks>Также, можно установить измерение игрока</remarks>
        /// <param name="player">Сущность игрока</param>
        /// <param name="pos">Новая позиция</param>
        /// <param name="toGround">Привязывать ли к земле?</param>
        /// <param name="dimension">Новое измерение</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static void SetPlayerPos(Vector3 pos, bool toGround, uint? dimension = null, float? heading = null, bool fade = false, bool withVehicle = false, params Player[] players)
        {
            if (dimension is uint dim)
            {

            }

            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];

                player.DetachAllEntities();

                player.GetEntityIsAttachedTo()?.DetachEntity(player);

                var pData = player.GetMainData();

                if (pData != null)
                {
                    pData.ActiveOffer?.Cancel(false, false, Sync.Offers.ReplyTypes.AutoCancel, false);

                    foreach (var x in pData.ObjectsInHand)
                        player.DetachObject(x.Type);

                    pData.StopAnim();
                }

                if (dimension is uint cDim)
                {
                    player.Dimension = cDim;
                }
            }

            if (pos != null)
            {
                if (withVehicle)
                    NAPI.ClientEvent.TriggerClientEventToPlayers(players, "AC::State::TP", pos, toGround, heading, fade, true);
                else
                    NAPI.ClientEvent.TriggerClientEventToPlayers(players, "AC::State::TP", pos, toGround, heading, fade);
            }
        }

        private static void OnDimensionChange(uint oDim, uint dim, params Player[] players)
        {
            if (oDim == dim)
                return;

            Game.Houses.HouseBase oHouseBase = null;
            Game.Houses.Garage oGarage = null;
            Game.Houses.Apartments.ApartmentsRoot oApRoot = null;

            Game.Houses.HouseBase houseBase = null;
            Game.Houses.Garage garage = null;
            Game.Houses.Apartments.ApartmentsRoot apRoot = null;

            if (oDim != Utils.Dimensions.Main)
            {
                oHouseBase = Utils.GetHouseBaseByDimension(oDim);


            }


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
        }
        #endregion
    }
}
