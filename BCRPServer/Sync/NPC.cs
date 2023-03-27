using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Sync
{
    public static class NPC
    {
        private static Dictionary<string, Vector3> Positions = new Dictionary<string, Vector3>()
        {
            { "vpound_w_0", new Vector3(485.6506f, -54.18661f, 78.30058f) },

            { "vrent_s_0", new Vector3(-718.6724f, 5821.765f, 17.21804f) },

            { $"cop0_{(int)Game.Fractions.Types.PolicePaleto}", new Vector3(-448.2888f, 6012.634f, 31.71635f) },
        };

        private static Dictionary<List<string>, List<string>> AllowedActionsProcs = new Dictionary<List<string>, List<string>>()
        {
            {
                new List<string>()
                {
                    "vpound_w_0",
                },

                new List<string>()
                {
                    "vpound_d",
                    "vpound_p",
                }
            },

            {
                new List<string>()
                {
                    "vrent_s_0",
                },

                new List<string>()
                {
                    "vrent_s_d",
                    "vrent_s_p",
                }
            },
        };

        private static Dictionary<string, Action<PlayerData, string[]>> Actions = new Dictionary<string, Action<PlayerData, string[]>>()
        {

        };

        private static Dictionary<string, Func<PlayerData, string, string[], object>> Procedures = new Dictionary<string, Func<PlayerData, string, string[], object>>()
        {
            {
                "vpound_d",

                (pData, npcId, data) =>
                {
                    var vehsOnPound = pData.Info.VehiclesOnPound.ToList();

                    if (vehsOnPound.Count == 0)
                        return null;

                    return $"{Settings.VEHICLEPOUND_PAY_PRICE}_{string.Join('_', vehsOnPound.Select(x => x.VID))}";
                }
            },

            {
                "vpound_p",

                (pData, npcId, data) =>
                {
                    if (data.Length < 1)
                        return false;

                    var vPoundData = BCRPServer.Sync.Vehicles.GetVehiclePoundData(npcId);

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
            },

            {
                "vrent_s_d",

                (pData, npcId, data) =>
                {
                    return Settings.VEHICLERENT_S_PAY_PRICE;
                }
            },

            {
                "vrent_s_p",

                (pData, npcId, data) =>
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
            },
        };

        public static Vector3 GetPositionById(string npcId) => Positions.GetValueOrDefault(npcId);

        public static Action<PlayerData, string[]> GetActionById(string actionId) => Actions.GetValueOrDefault(actionId);

        public static Func<PlayerData, string, string[], object> GetProcById(string procId) => Procedures.GetValueOrDefault(procId);

        public static bool IsNpcAllowedTo(string npcId, string actionProcId) => AllowedActionsProcs.Where(x => x.Key.Contains(npcId) && x.Value.Contains(actionProcId)).Any();
    }
}
