using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.UI.CEF;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management.Commands
{
    partial class Core
    {
        [Command("teleportmarker", true, "Телепорт по метке", "tpmarker", "tpm")]
        public static void TeleportMarker()
        {
            Vector3 position = Main.WaypointPosition;

            if (position == null)
            {
                Notification.Show(Notification.Types.Error, Locale.Notifications.Commands.Header, Locale.Notifications.Commands.Teleport.NoWaypoint);

                return;
            }

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_tpp", Player.LocalPlayer.RemoteId, position.X, position.Y, position.Z, true);

            LastSent = World.Core.ServerTime;
        }

        [Command("auto_tpm", true, "Автоматически телепортироваться по метке", "autotpm")]
        public static void AutoTeleportMarker(bool? state = null)
        {
            if (state == null)
                Settings.User.Other.AutoTeleportMarker = !Settings.User.Other.AutoTeleportMarker;
            else
                Settings.User.Other.AutoTeleportMarker = (bool)state;

            Notification.Show(Notification.Types.Success,
                Locale.Notifications.Commands.Header,
                string.Format(Settings.User.Other.AutoTeleportMarker ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "AutoTPMarker")
            );
        }

        [Command("teleportpos", true, "Телепорт по координатам", "tppos", "tpp")]
        public static void TeleportPosition(float x, float y, float z, bool toGround = false)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_tpp", Player.LocalPlayer.RemoteId, x, y, z, toGround);

            LastSent = World.Core.ServerTime;
        }

        [Command("tpto", true, "Телепорт к игроку", "goto", "tpp")]
        public static void TeleportToPlayer(uint pid)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_tppl", pid, false);

            LastSent = World.Core.ServerTime;
        }

        [Command("tphere", true, "Телепорт игрока к себе", "gethere", "thp")]
        public static void GetHerePlayer(uint pid)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_tppl", pid, true);

            LastSent = World.Core.ServerTime;
        }

        [Command("tptoveh", true, "Телепорт к транспорту", "gotoveh", "tpv")]
        public static void TeleportToVehicle(uint vid)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_tpveh", vid, false);

            LastSent = World.Core.ServerTime;
        }

        [Command("tphereveh", true, "Телепорт транспорта к себе", "gethereveh", "thv")]
        public static void GetHereVehicle(uint vid)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_tpveh", vid, true);

            LastSent = World.Core.ServerTime;
        }

        [Command("setdim", true, "Смена измерения", "sdim")]
        public static void SetDimension(uint pid, uint dimension)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_sdim", pid, dimension);

            LastSent = World.Core.ServerTime;
        }
    }
}