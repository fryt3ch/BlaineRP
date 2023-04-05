using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCRPServer;

namespace BCRPServer.Events.NPC
{
    internal class Misc
    {
        [NPC.Action("vpound_d", "vpound_w_0")]
        private static object VehiclePoundGetData(PlayerData pData, string npcId, string[] data)
        {
            var vehsOnPound = pData.Info.VehiclesOnPound.ToList();

            if (vehsOnPound.Count == 0)
                return null;

            return $"{Settings.VEHICLEPOUND_PAY_PRICE}_{string.Join('_', vehsOnPound.Select(x => x.VID))}";
        }

        [NPC.Action("vpound_p", "vpound_w_0")]
        private static object VehiclePoundPay(PlayerData pData, string npcId, string[] data)
        {
            if (data.Length < 1)
                return false;

            var vPoundData = Sync.Vehicles.GetVehiclePoundData(npcId);

            if (vPoundData == null)
                return false;

            uint vid;

            if (!uint.TryParse(data[0], out vid))
                return false;

            var vInfo = pData.Info.VehiclesOnPound.Where(x => x.VID == vid).FirstOrDefault();

            if (vInfo == null)
                return false;

            ulong newCash;

            if (!pData.TryRemoveCash(Settings.VEHICLEPOUND_PAY_PRICE, out newCash, true))
                return false;

            pData.SetCash(newCash);

            vInfo.LastData.GarageSlot = -1;

            var newPos = new Utils.Vector4(vPoundData.GetNextVehicleSpawnPosition());

            vInfo.LastData.Position = newPos.Position;
            vInfo.LastData.Heading = newPos.RotationZ;

            vInfo.LastData.Dimension = Utils.Dimensions.Main;

            vInfo.LastData.GarageSlot = int.MinValue;

            vInfo.Spawn();

            MySQL.VehicleDeletionUpdate(vInfo);

            pData.Player.CreateGPSBlip(newPos.Position, Utils.Dimensions.Main, true);

            return true;
        }

        [NPC.Action("vrent_s_d", "vrent_s_0")]
        private static object VehicleRentSGetData(PlayerData pData, string npcId, string[] data)
        {
            return Settings.VEHICLERENT_S_PAY_PRICE;
        }

        [NPC.Action("vrent_s_p", "vrent_s_0")]
        private static object VehicleRentSPay(PlayerData pData, string npcId, string[] data)
        {
            var curRentedVeh = pData.RentedVehicle;

            if (curRentedVeh != null)
            {
                pData.Player.Notify("Vehicle::AHRV");

                return false;
            }

            var vSpawnData = Sync.Vehicles.GetVehicleRentSData(npcId);

            if (vSpawnData == null)
                return false;

            var vSpawnPos = vSpawnData.GetNextVehicleSpawnPosition();

            ulong newCash;

            if (!pData.TryRemoveCash(Settings.VEHICLERENT_S_PAY_PRICE, out newCash, true))
                return false;

            pData.SetCash(newCash);

            var vTypeData = Game.Data.Vehicles.GetData("faggio");

            var vData = VehicleData.NewRent(pData, vTypeData, new Utils.Colour(255, 0, 0, 255), new Utils.Colour(255, 0, 0, 255), vSpawnPos.Position, vSpawnPos.RotationZ, Utils.Dimensions.Main);

            return true;
        }
    }
}
