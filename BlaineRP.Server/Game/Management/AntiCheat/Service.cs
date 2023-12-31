﻿using System.Collections.Generic;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Vehicles.Static;
using BlaineRP.Server.Game.Offers;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Management.AntiCheat
{
    public static partial class Service
    {
        public static void TeleportVehicle(GTANetworkAPI.Vehicle veh, Vector3 pos, uint? dimension = null, float? heading = null, bool fade = false, VehicleTeleportType tpType = VehicleTeleportType.Default, bool toGround = false)
        {
            veh.DetachAllEntities();

            veh.GetEntityIsAttachedTo()?.DetachEntity(veh);

            var vData = veh.GetMainData();

            if (vData != null)
            {
                if (vData.IsFrozen)
                    vData.IsFrozen = false;

                if (vData.ForcedSpeed != 0f)
                    vData.ForcedSpeed = 0f;

                if (vData.Info.LastData.GarageSlot >= 0)
                {
                    vData.IsInvincible = false;

                    vData.Info.LastData.GarageSlot = -1;
                }
            }

            var lastDim = veh.Dimension;

            if (tpType == VehicleTeleportType.Default)
            {
                if (dimension is uint dim)
                {
                    veh.Dimension = dim;

                    veh.Occupants.ForEach(x =>
                    {
                        if (x is Entity entity)
                        {
                            entity.Position = entity.Position;

                            entity.Dimension = lastDim;
                        }
                    });
                }

                var wasOnTrailer = veh.DetachObject(AttachmentType.TrailerObjOnBoat);

                veh.Position = pos;

                if (heading is float headingF)
                    veh.SetHeading(headingF);

                if (wasOnTrailer)
                    veh.AttachObject(EntitiesData.Vehicles.Static.Service.GetData("boattrailer").Model, AttachmentType.TrailerObjOnBoat, -1, null);
            }
            else if (tpType == VehicleTeleportType.OnlyDriver)
            {
                if (dimension is uint dim)
                    veh.Dimension = dim;

                Player driver = null;

                veh.Occupants.ForEach(x =>
                {
                    if (x is Player player)
                    {
                        if (driver == null && player.VehicleSeat == 0)
                        {
                            TeleportPlayers(pos, toGround, dimension, heading, fade, true, lastDim, player);

                            driver = player;

                            return;
                        }
                    }

                    x.Position = x.Position;

                    if (x.Dimension != lastDim)
                        x.Dimension = lastDim;
                });

                if (driver == null)
                {
                    var wasOnTrailer = veh.DetachObject(AttachmentType.TrailerObjOnBoat);

                    veh.Position = pos;

                    if (heading is float headingF)
                        veh.SetHeading(headingF);

                    if (wasOnTrailer)
                        veh.AttachObject(EntitiesData.Vehicles.Static.Service.GetData("boattrailer").Model, AttachmentType.TrailerObjOnBoat, -1, null);
                }
            }
            else if (tpType == VehicleTeleportType.All)
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
                    var wasOnTrailer = veh.DetachObject(AttachmentType.TrailerObjOnBoat);

                    veh.Position = pos;

                    if (heading is float headingF)
                        veh.SetHeading(headingF);

                    if (wasOnTrailer)
                        veh.AttachObject(EntitiesData.Vehicles.Static.Service.GetData("boattrailer").Model, AttachmentType.TrailerObjOnBoat, -1, null);
                }
                else
                {
                    TeleportPlayers(pos, toGround, dimension, heading, fade, true, lastDim, occupants.ToArray());
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
                        if (pData.CurrentBusiness != null)
                        {
                            Sync.Players.ExitFromBusiness(pData, false);
                        }

                        var pDim = lastDim is uint lDim ? lDim : player.Dimension;

                        if (pDim != dim)
                        {
                            if (pDim >= Properties.Settings.Profile.Current.Game.HouseDimensionBaseOffset)
                            {
                                if (pDim < Properties.Settings.Profile.Current.Game.ApartmentsRootDimensionBaseOffset)
                                {
                                    Utils.GetHouseBaseByDimension(pDim)?.SetPlayersOutside(false, player);
                                }
                                else if (pDim < Properties.Settings.Profile.Current.Game.GarageDimensionBaseOffset)
                                {
                                    Utils.GetApartmentsRootByDimension(pDim)?.SetPlayersOutside(false, player);
                                }
                                else
                                {
                                    Utils.GetGarageByDimension(pDim)?.SetPlayersOutside(false, player);
                                }
                            }
                        }

                        pData.ActiveOffer?.Cancel(false, false, ReplyTypes.AutoCancel, false);
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
                        pData.ActiveOffer?.Cancel(false, false, ReplyTypes.AutoCancel, false);
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
            if (dim < Properties.Settings.Profile.Current.Game.HouseDimensionBaseOffset)
                return;

            if (dim < Properties.Settings.Profile.Current.Game.ApartmentsRootDimensionBaseOffset)
            {
                Utils.GetHouseBaseByDimension(dim)?.SetPlayersInside(false, players);

                return;
            }

            if (dim < Properties.Settings.Profile.Current.Game.GarageDimensionBaseOffset)
            {
                Utils.GetApartmentsRootByDimension(dim)?.SetPlayersInside(false, players);

                return;
            }

            Utils.GetGarageByDimension(dim)?.SetPlayersInside(false, players);
        }

        public static void TeleportPeds(Vector3 pos, bool toGround, uint? dimension = null, float? heading = null, uint? lastDim = null, params Ped[] peds)
        {
            for (int i = 0; i < peds.Length; i++)
            {
                var ped = peds[i];

                if (ped.Controller is Player controller)
                {
                    if (dimension is uint dim)
                    {
                        ped.Dimension = dim;
                    }

                    if (pos != null)
                    {
                        ped.Position = pos;
                    }

                    if (heading is float headingF)
                    {
                        ped.Rotation = new Vector3(0f, 0f, headingF);
                    }

                    //controller.TriggerEvent("AC::Ped::TP", ped.Id, pos, heading);
                }
                else
                {
                    if (dimension is uint dim)
                    {
                        ped.Dimension = dim;
                    }

                    if (pos != null)
                    {
                        ped.Position = pos;
                    }

                    if (heading is float headingF)
                    {
                        ped.Rotation = new Vector3(0f, 0f, headingF);
                    }
                }
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
    }
}
