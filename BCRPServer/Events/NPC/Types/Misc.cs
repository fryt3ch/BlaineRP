using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCRPServer;
using static BCRPServer.Game.Bank;

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

            vInfo.LastData.Dimension = Settings.MAIN_DIMENSION;

            vInfo.LastData.GarageSlot = int.MinValue;

            vInfo.Spawn();

            MySQL.VehicleDeletionUpdate(vInfo);

            pData.Player.CreateGPSBlip(newPos.Position, Settings.MAIN_DIMENSION, true);

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

            var vData = VehicleData.NewRent(pData, vTypeData, new Utils.Colour(255, 0, 0, 255), new Utils.Colour(255, 0, 0, 255), vSpawnPos.Position, vSpawnPos.RotationZ, Settings.MAIN_DIMENSION);

            return true;
        }

        [NPC.Action("fishbuyer_s", "fishbuyer_0")]
        private static object FishBuyerSell(PlayerData pData, string npcId, string[] data)
        {
            if (data.Length != 2)
                return 0;

            var fishId = data[0];

            var fishBuyer = Game.Misc.FishBuyer.Get(int.Parse(npcId.Split('_')[1]));

            if (fishBuyer == null)
                return 0;

            if (fishId == string.Empty)
            {
                var dict = new Dictionary<string, int>();

                var slotsToUpdate = new List<int>();

                for (int i = 0; i < pData.Items.Length; i++)
                {
                    if (pData.Items[i] != null && pData.Items[i] is Game.Items.Food fish && Game.Misc.FishBuyer.BasePrices.ContainsKey(fish.ID))
                    {
                        var fAmount = fish.Amount;

                        if (!dict.TryAdd(fish.ID, fAmount))
                            dict[fish.ID] += fAmount;

                        fish.Delete();

                        pData.Items[i] = null;

                        slotsToUpdate.Add(i);
                    }
                }

                if (dict.Count == 0)
                    return 1;

                ulong totalGet = 0;

                foreach (var x in dict)
                {
                    uint price;

                    if (fishBuyer.TryGetPrice(x.Key, out price))
                        totalGet += (ulong)(price * x.Value);
                }

                ulong newBalance;

                if (pData.TryAddCash(totalGet, out newBalance, true))
                    pData.SetCash(newBalance);

                foreach (var x in slotsToUpdate)
                    pData.Player.InventoryUpdate(Game.Items.Inventory.Groups.Items, x, Game.Items.Item.ToClientJson(pData.Items[x], Game.Items.Inventory.Groups.Items));

                return byte.MaxValue;
            }
            else
            {
                int amount;

                if (!int.TryParse(data[1], out amount))
                    return 0;

                if (amount <= 0)
                    return 0;

                uint price;

                if (!fishBuyer.TryGetPrice(fishId, out price))
                    return 0;

                var amountFound = 0;

                var slotsToUpdate = new List<int>();

                for (int i = 0; i < pData.Items.Length; i++)
                {
                    if (pData.Items[i]?.ID == fishId && pData.Items[i] is Game.Items.Food fish)
                    {
                        var fAmount = fish.Amount;

                        var amountNeed = amount - amountFound;

                        if (fAmount < amountNeed)
                            amountNeed = fAmount;

                        fish.Amount -= amountNeed;
                        amountFound += amountNeed;

                        if (fish.Amount <= 0)
                        {
                            fish.Delete();

                            pData.Items[i] = null;
                        }
                        else
                        {
                            fish.Update();
                        }

                        slotsToUpdate.Add(i);

                        if (amountFound >= amount)
                            break;
                    }
                }

                if (amountFound == 0)
                    return 1;

                ulong newBalance;

                var totalGet = (ulong)(price * amountFound);

                if (pData.TryAddCash(totalGet, out newBalance, true))
                    pData.SetCash(newBalance);

                foreach (var x in slotsToUpdate)
                    pData.Player.InventoryUpdate(Game.Items.Inventory.Groups.Items, x, Game.Items.Item.ToClientJson(pData.Items[x], Game.Items.Inventory.Groups.Items));

                return byte.MaxValue;
            }
        }
    }
}
