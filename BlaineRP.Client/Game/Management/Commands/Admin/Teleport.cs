using BlaineRP.Client.Extensions.System;
using RAGE.Elements;

namespace BlaineRP.Client.Management.Commands
{
    partial class Core
    {
        [Command("teleportmarker", true, "Телепорт по метке", "tpmarker", "tpm")]
        public static void TeleportMarker()
        {
            var position = Main.WaypointPosition;

            if (position == null)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.Commands.Header, Locale.Notifications.Commands.Teleport.NoWaypoint);

                return;
            }

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_tpp", Player.LocalPlayer.RemoteId, position.X, position.Y, position.Z, true);

            LastSent = Sync.World.ServerTime;
        }

        [Command("auto_tpm", true, "Автоматически телепортироваться по метке", "autotpm")]
        public static void AutoTeleportMarker(bool? state = null)
        {
            if (state == null)
                Settings.User.Other.AutoTeleportMarker = !Settings.User.Other.AutoTeleportMarker;
            else
                Settings.User.Other.AutoTeleportMarker = (bool)state;

            CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Notifications.Commands.Header, string.Format(Settings.User.Other.AutoTeleportMarker ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "AutoTPMarker"));
        }

        [Command("teleportpos", true, "Телепорт по координатам", "tppos", "tpp")]
        public static void TeleportPosition(float x, float y, float z, bool toGround = false)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_tpp", Player.LocalPlayer.RemoteId, x, y, z, toGround);

            LastSent = Sync.World.ServerTime;
        }

        [Command("tpto", true, "Телепорт к игроку", "goto", "tpp")]
        public static void TeleportToPlayer(uint pid)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_tppl", pid, false);

            LastSent = Sync.World.ServerTime;
        }

        [Command("tphere", true, "Телепорт игрока к себе", "gethere", "thp")]
        public static void GetHerePlayer(uint pid)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_tppl", pid, true);

            LastSent = Sync.World.ServerTime;
        }

        [Command("tptoveh", true, "Телепорт к транспорту", "gotoveh", "tpv")]
        public static void TeleportToVehicle(uint vid)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_tpveh", vid, false);

            LastSent = Sync.World.ServerTime;
        }

        [Command("tphereveh", true, "Телепорт транспорта к себе", "gethereveh", "thv")]
        public static void GetHereVehicle(uint vid)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_tpveh", vid, true);

            LastSent = Sync.World.ServerTime;
        }

        [Command("setdim", true, "Смена измерения", "sdim")]
        public static void SetDimension(uint pid, uint dimension)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_sdim", pid, dimension);

            LastSent = Sync.World.ServerTime;
        }
    }
}
