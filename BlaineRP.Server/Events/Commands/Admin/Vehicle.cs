using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.EntitiesData.Vehicles;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Events.Commands
{
    partial class Commands
    {
        [Command("veh_temp", 1)]
        private static void TempVehicle(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            var id = args[1];

            uint pid;

            if (!uint.TryParse(args[0], out pid))
                return;

            var vType = Game.Data.Vehicles.GetData(id);

            if (vType == null)
            {
                pData.Player.Notify("Cmd::IdNotFound");

                return;
            }

            var tData = pData;

            if (pData.CID != pid && pData.Player.Id != pid)
            {
                tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    pData.Player.Notify("Cmd::TargetNotFound");

                    return;
                }
            }

            var vData = VehicleData.NewTemp(tData, vType, new Colour(0, 0, 0), new Colour(0, 0, 0), tData.Player.Position, tData.Player.Heading, tData.Player.Dimension);

            if (vData == null)
                return;
        }

        [Command("veh_new", 1)]
        private static void NewVehicle(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            var id = args[1];

            uint pid;

            if (!uint.TryParse(args[0], out pid))
                return;

            var vType = Game.Data.Vehicles.GetData(id);

            if (vType == null)
            {
                pData.Player.Notify("Cmd::IdNotFound");

                return;
            }

            var tData = pData;

            if (pData.CID != pid && pData.Player.Id != pid)
            {
                tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    pData.Player.Notify("Cmd::TargetNotFound");

                    return;
                }
            }

            var vData = VehicleData.New(tData, vType, new Colour(0, 0, 0), new Colour(0, 0, 0), tData.Player.Position, tData.Player.Heading, tData.Player.Dimension, true);

            if (vData == null)
                return;
        }

        [Command("veh_del", 1)]
        private static void DeleteVehicle(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            uint vid;
            bool completely;

            if (!uint.TryParse(args[0], out vid) || !bool.TryParse(args[1], out completely))
                return;

            var vData = Utils.FindVehicleOnline(vid);

            if (vData == null || vData.Vehicle?.Exists != true)
            {
                pData.Player.Notify("Cmd::TargetNotFound");

                return;
            }

            vData.Delete(completely);
        }

        [Command("veh_rs", 1)]
        private static void RespawnVehicle(PlayerData pData, params string[] args)
        {
            if (args.Length != 1)
                return;

            uint vid;

            if (!uint.TryParse(args[0], out vid))
                return;

            var vData = Utils.FindVehicleOnline(vid);

            if (vData == null || vData.Vehicle?.Exists != true)
            {
                pData.Player.Notify("Cmd::TargetNotFound");

                return;
            }

            //vData.Respawn();
        }
    }
}
