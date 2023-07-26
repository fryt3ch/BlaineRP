using BlaineRP.Client.Extensions.System;
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

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("veh_temp", Player.LocalPlayer.RemoteId, id);

            LastSent = World.Core.ServerTime;
        }

        [Command("give_tempvehicle", true, "Выдать себе транспорт (временный)", "give_tveh", "give_tvehile")]
        public static void GiveTempVehicle(uint pid, string id)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("veh_temp", pid, id);

            LastSent = World.Core.ServerTime;
        }

        [Command("vehicle", true, "Выдать себе транспорт", "veh")]
        public static void Vehicle(string id)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("veh_new", Player.LocalPlayer.RemoteId, id);

            LastSent = World.Core.ServerTime;
        }

        [Command("give_vehicle", true, "Выдать транспорт игроку", "give_veh")]
        public static void GiveVehicle(uint pid, string id)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("veh_new", pid, id);

            LastSent = World.Core.ServerTime;
        }

        [Command("respawnvehicle", true, "Респавн транспорта", "respawnveh")]
        public static void RespawnVehicle(uint vid)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("veh_rs", vid);

            LastSent = World.Core.ServerTime;
        }

        [Command("deletevehicle", true, "Удалить транспорт (с сервера)", "delveh")]
        public static void DeleteVehicle(uint vid)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("veh_del", vid, false);

            LastSent = World.Core.ServerTime;
        }

        [Command("deletevehicle_full", true, "Удалить транспорт (полностью!)", "delveh_full")]
        public static void DeleteVehicleFull(uint vid)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("veh_del", vid, true);

            LastSent = World.Core.ServerTime;
        }
    }
}