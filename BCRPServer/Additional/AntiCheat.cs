﻿using GTANetworkAPI;
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
        public static void TeleportVehicle(Vehicle veh, Vector3 pos, uint? dimension = null, float? heading = null, bool fade = false, VehicleTeleportTypes tpType = VehicleTeleportTypes.Default)
        {
            veh.DetachAllEntities();

            veh.GetEntityIsAttachedTo()?.DetachEntity(veh);

            var vData = veh.GetMainData();

            if (vData != null)
            {
                if (vData.IsFrozen)
                    vData.IsFrozen = false;
            }

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

                var wasOnTrailer = veh.DetachObject(Sync.AttachSystem.Types.TrailerObjOnBoat);

                veh.Position = pos;

                if (heading is float headingF)
                    veh.SetHeading(headingF);

                if (wasOnTrailer)
                    veh.AttachObject(Game.Data.Vehicles.GetData("boattrailer").Model, Sync.AttachSystem.Types.TrailerObjOnBoat, -1, null);
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
                            TeleportPlayers(pos, false, dimension, heading, fade, true, lastDim, player);

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
                    var wasOnTrailer = veh.DetachObject(Sync.AttachSystem.Types.TrailerObjOnBoat);

                    veh.Position = pos;

                    if (heading is float headingF)
                        veh.SetHeading(headingF);

                    if (wasOnTrailer)
                        veh.AttachObject(Game.Data.Vehicles.GetData("boattrailer").Model, Sync.AttachSystem.Types.TrailerObjOnBoat, -1, null);
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
                    var wasOnTrailer = veh.DetachObject(Sync.AttachSystem.Types.TrailerObjOnBoat);

                    veh.Position = pos;

                    if (heading is float headingF)
                        veh.SetHeading(headingF);

                    if (wasOnTrailer)
                        veh.AttachObject(Game.Data.Vehicles.GetData("boattrailer").Model, Sync.AttachSystem.Types.TrailerObjOnBoat, -1, null);
                }
                else
                {
                    TeleportPlayers(pos, false, dimension, heading, fade, true, lastDim, occupants.ToArray());
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
        public static void TeleportPlayers(Vector3 pos, bool toGround, uint? dimension = null, float? heading = null, bool fade = false, bool withVehicle = false, uint? lastDim = null, params Player[] players)
        {
            if (dimension is uint dim)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    var player = players[i];

                    player.DetachAllEntities();

                    player.GetEntityIsAttachedTo()?.DetachEntity(player);

                    var pData = player.GetMainData();

                    if (pData != null)
                    {
                        var ciiu = pData.CurrentItemInUse;

                        ciiu?.Item.StopUse(pData, Game.Items.Inventory.Groups.Items, ciiu.Value.Slot, true);

                        if (pData.CurrentBusiness != null)
                        {
                            Sync.Players.ExitFromBuiness(pData, false);
                        }

                        var pDim = lastDim is uint lDim ? lDim : player.Dimension;

                        if (pDim != dim)
                        {
                            if (pDim >= Utils.HouseDimBase)
                            {
                                if (dim < Utils.ApartmentsRootDimBase)
                                {
                                    Utils.GetHouseBaseByDimension(pDim)?.SetPlayersOutside(false, player);
                                }
                                else if (dim < Utils.GarageDimBase)
                                {
                                    Utils.GetApartmentsRootByDimension(pDim)?.SetPlayersOutside(false, player);
                                }
                                else
                                {
                                    Utils.GetGarageByDimension(pDim)?.SetPlayersOutside(false, player);
                                }
                            }
                        }

                        pData.ActiveOffer?.Cancel(false, false, Sync.Offers.ReplyTypes.AutoCancel, false);

                        foreach (var x in pData.ObjectsInHand)
                            player.DetachObject(x.Type);

                        pData.StopAnim();
                    }

                    if (!withVehicle)
                        player.Dimension = dim;
                }

                if (pos != null)
                {
                    if (withVehicle)
                        NAPI.ClientEvent.TriggerClientEventToPlayers(players, "AC::State::TP", dim, pos, toGround, heading, fade, true);
                    else
                        NAPI.ClientEvent.TriggerClientEventToPlayers(players, "AC::State::TP", dim, pos, toGround, heading, fade);
                }
                else
                    NAPI.ClientEvent.TriggerClientEventToPlayers(players, "AC::State::TP", dim);

                OnDimensionChange(dim, players);
            }
            else
            {
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
                }

                if (pos != null)
                {
                    if (withVehicle)
                        NAPI.ClientEvent.TriggerClientEventToPlayers(players, "AC::State::TP", -1, pos, toGround, heading, fade, true);
                    else
                        NAPI.ClientEvent.TriggerClientEventToPlayers(players, "AC::State::TP", -1, pos, toGround, heading, fade);
                }
            }
        }

        private static void OnDimensionChange(uint dim, params Player[] players)
        {
            if (dim < Utils.HouseDimBase)
                return;

            if (dim < Utils.ApartmentsRootDimBase)
            {
                Utils.GetHouseBaseByDimension(dim)?.SetPlayersInside(false, players);

                return;
            }

            if (dim < Utils.GarageDimBase)
            {
                Utils.GetApartmentsRootByDimension(dim)?.SetPlayersInside(false, players);

                return;
            }

            Utils.GetGarageByDimension(dim)?.SetPlayersInside(false, players);
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
