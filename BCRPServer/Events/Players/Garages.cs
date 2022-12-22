using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Events.Players
{
    class Garages : Script
    {
        [RemoteProc("Garage::ToggleLock")]
        public static bool GarageToggleLock(Player player, uint id, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            var garage = Game.Houses.Garage.Get(id);

            if (garage == null)
                return false;

            if (garage.Owner != pData.Info)
                return false;

            if (!garage.Root.IsEntityNearEnter(player))
                return false;

            if (garage.IsLocked == state)
                return false;

            garage.IsLocked = state;

            return true;
        }

        [RemoteProc("Garage::GetIsLocked")]
        public static bool? GarageGetIsLocked(Player player, uint id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            var garage = Game.Houses.Garage.Get(id);

            if (garage == null)
                return null;

            if (!garage.Root.IsEntityNearEnter(player))
                return null;

            return garage.IsLocked;
        }

        [RemoteEvent("Garage::Buy")]
        public static void GarageBuy(Player player, uint id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var garage = Game.Houses.Garage.Get(id);

            if (garage == null)
                return;

            if (garage.Owner != null)
                return;

            if (!garage.Root.IsEntityNearEnter(player))
                return;
        }

        [RemoteEvent("Garage::SellGov")]
        public static void GarageSellGov(Player player, uint id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var garage = Game.Houses.Garage.Get(id);

            if (garage == null)
                return;

            if (garage.Owner != pData.Info)
                return;

            if (!garage.Root.IsEntityNearEnter(player))
                return;
        }

        [RemoteEvent("Garage::Enter")]
        public static void GarageEnter(Player player, uint id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (player.Vehicle != null)
                return;

            var garage = Game.Houses.Garage.Get(id);

            if (garage == null)
                return;

            if (!garage.Root.IsEntityNearEnter(player))
                return;

            player.Teleport(garage.StyleData.EnterPosition.Position, false, garage.Dimension, garage.StyleData.EnterPosition.RotationZ, true);

            player.TriggerEvent("Garage::Enter", id);
        }

        [RemoteEvent("Garage::Exit")]
        public static void GarageExit(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            if (player.Vehicle != null)
                return;

            var pData = sRes.Data;

            var garage = pData.CurrentGarage;

            if (garage == null)
                return;

            player.Teleport(garage.Root.EnterPosition.Position, false, Utils.Dimensions.Main, garage.Root.EnterPosition.RotationZ, true);

            player.TriggerEvent("Garage::Exit");
        }
    }
}
