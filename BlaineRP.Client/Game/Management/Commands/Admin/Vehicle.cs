using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.World;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management.Commands
{
    partial class Core
    {
        [Command("tempvehicle", true, "Выдать себе транспорт (временный)", "tveh", "tvehicle")]
        public static void TempVehicle(string id)
        {
            if (id == null)
                return;

            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            Commands.Core.CallRemote("veh_temp", Player.LocalPlayer.RemoteId, id);

            Commands.Core.LastSent = World.Core.ServerTime;
        }

        [Command("give_tempvehicle", true, "Выдать себе транспорт (временный)", "give_tveh", "give_tvehile")]
        public static void GiveTempVehicle(uint pid, string id)
        {
            if (id == null)
                return;

            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            Commands.Core.CallRemote("veh_temp", pid, id);

            Commands.Core.LastSent = World.Core.ServerTime;
        }

        [Command("vehicle", true, "Выдать себе транспорт", "veh")]
        public static void Vehicle(string id)
        {
            if (id == null)
                return;

            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            Commands.Core.CallRemote("veh_new", Player.LocalPlayer.RemoteId, id);

            Commands.Core.LastSent = World.Core.ServerTime;
        }

        [Command("give_vehicle", true, "Выдать транспорт игроку", "give_veh")]
        public static void GiveVehicle(uint pid, string id)
        {
            if (id == null)
                return;

            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            Commands.Core.CallRemote("veh_new", pid, id);

            Commands.Core.LastSent = World.Core.ServerTime;
        }

        [Command("respawnvehicle", true, "Респавн транспорта", "respawnveh")]
        public static void RespawnVehicle(uint vid)
        {
            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            Commands.Core.CallRemote("veh_rs", vid);

            Commands.Core.LastSent = World.Core.ServerTime;
        }

        [Command("deletevehicle", true, "Удалить транспорт (с сервера)", "delveh")]
        public static void DeleteVehicle(uint vid)
        {
            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            Commands.Core.CallRemote("veh_del", vid, false);

            Commands.Core.LastSent = World.Core.ServerTime;
        }

        [Command("deletevehicle_full", true, "Удалить транспорт (полностью!)", "delveh_full")]
        public static void DeleteVehicleFull(uint vid)
        {
            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            Commands.Core.CallRemote("veh_del", vid, true);

            Commands.Core.LastSent = World.Core.ServerTime;
        }
    }
}
