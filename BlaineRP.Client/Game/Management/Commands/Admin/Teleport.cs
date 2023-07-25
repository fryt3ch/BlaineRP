using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Game.World;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management.Commands
{
    partial class Core
    {
        [Command("teleportmarker", true, "Телепорт по метке", "tpmarker", "tpm")]
        public static void TeleportMarker()
        {
            var position = Main.WaypointPosition;

            if (position == null)
            {
                Notification.Show(Notification.Types.Error, Locale.Notifications.Commands.Header, Locale.Notifications.Commands.Teleport.NoWaypoint);

                return;
            }

            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            Commands.Core.CallRemote("p_tpp", Player.LocalPlayer.RemoteId, position.X, position.Y, position.Z, true);

            Commands.Core.LastSent = World.Core.ServerTime;
        }

        [Command("auto_tpm", true, "Автоматически телепортироваться по метке", "autotpm")]
        public static void AutoTeleportMarker(bool? state = null)
        {
            if (state == null)
                Settings.User.Other.AutoTeleportMarker = !Settings.User.Other.AutoTeleportMarker;
            else
                Settings.User.Other.AutoTeleportMarker = (bool)state;

            Notification.Show(Notification.Types.Success, Locale.Notifications.Commands.Header, string.Format(Settings.User.Other.AutoTeleportMarker ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "AutoTPMarker"));
        }

        [Command("teleportpos", true, "Телепорт по координатам", "tppos", "tpp")]
        public static void TeleportPosition(float x, float y, float z, bool toGround = false)
        {
            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            Commands.Core.CallRemote("p_tpp", Player.LocalPlayer.RemoteId, x, y, z, toGround);

            Commands.Core.LastSent = World.Core.ServerTime;
        }

        [Command("tpto", true, "Телепорт к игроку", "goto", "tpp")]
        public static void TeleportToPlayer(uint pid)
        {
            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            Commands.Core.CallRemote("p_tppl", pid, false);

            Commands.Core.LastSent = World.Core.ServerTime;
        }

        [Command("tphere", true, "Телепорт игрока к себе", "gethere", "thp")]
        public static void GetHerePlayer(uint pid)
        {
            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            Commands.Core.CallRemote("p_tppl", pid, true);

            Commands.Core.LastSent = World.Core.ServerTime;
        }

        [Command("tptoveh", true, "Телепорт к транспорту", "gotoveh", "tpv")]
        public static void TeleportToVehicle(uint vid)
        {
            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            Commands.Core.CallRemote("p_tpveh", vid, false);

            Commands.Core.LastSent = World.Core.ServerTime;
        }

        [Command("tphereveh", true, "Телепорт транспорта к себе", "gethereveh", "thv")]
        public static void GetHereVehicle(uint vid)
        {
            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            Commands.Core.CallRemote("p_tpveh", vid, true);

            Commands.Core.LastSent = World.Core.ServerTime;
        }

        [Command("setdim", true, "Смена измерения", "sdim")]
        public static void SetDimension(uint pid, uint dimension)
        {
            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            Commands.Core.CallRemote("p_sdim", pid, dimension);

            Commands.Core.LastSent = World.Core.ServerTime;
        }
    }
}
