using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Sync.Offers
{
    public partial class Offer
    {
        public class PropertySellData
        {
            public object Data { get; set; }

            public ulong Price { get; set; }

            public PropertySellData(object Data, ulong Price)
            {
                this.Data = Data;
                this.Price = Price;
            }
        }

        public class Trade
        {
            public class TradeItem
            {
                public Game.Items.Item ItemRoot { get; set; }
                public int Amount { get; set; }

                public TradeItem(Game.Items.Item ItemRoot, int Amount)
                {
                    this.ItemRoot = ItemRoot;
                    this.Amount = Amount;
                }

                public string ToClientJson() => ItemRoot == null ? "" : $"{ItemRoot.ID}&{Amount}&{(ItemRoot is Game.Items.IStackable ? ItemRoot.BaseWeight : ItemRoot.Weight)}&{Game.Items.Stuff.GetItemTag(ItemRoot)}";
            }

            public TradeItem[] SenderItems { get; set; }
            public TradeItem[] ReceiverItems { get; set; }

            public List<VehicleData.VehicleInfo> SenderVehicles { get; set; }
            public List<VehicleData.VehicleInfo> ReceiverVehicles { get; set; }

            public List<Game.Businesses.Business> SenderBusinesses { get; set; }
            public List<Game.Businesses.Business> ReceiverBusinesses { get; set; }

            public List<Game.Estates.HouseBase> SenderHouseBases { get; set; }
            public List<Game.Estates.HouseBase> ReceiverHouseBases { get; set; }

            public List<Game.Estates.Garage> SenderGarages { get; set; }
            public List<Game.Estates.Garage> ReceiverGarages { get; set; }

            public ulong SenderMoney { get; set; }
            public ulong ReceiverMoney { get; set; }

            public bool SenderReady { get; set; }
            public bool ReceiverReady { get; set; }

            public (Game.Items.Inventory.ResultTypes Result, PlayerData PlayerError) Execute(PlayerData pData, PlayerData tData)
            {
                if (pData.Cash < SenderMoney)
                    return (Game.Items.Inventory.ResultTypes.NotEnoughMoney, pData);

                if (tData.Cash < ReceiverMoney)
                    return (Game.Items.Inventory.ResultTypes.NotEnoughMoney, tData);

                var senderFreeSlots = pData.Items.Where(x => x == null).Count();

                var senderItems = SenderItems.Where(x => x != null && x.ItemRoot != null).ToList();
                var receiverItems = ReceiverItems.Where(x => x != null && x.ItemRoot != null).ToList();

                var senderRemoveSlots = senderItems.Where(x => (((x.ItemRoot as Game.Items.IStackable)?.Amount ?? 1) - x.Amount) == 0).Count();
                var receiverRemoveSlots = receiverItems.Where(x => (((x.ItemRoot as Game.Items.IStackable)?.Amount ?? 1) - x.Amount) == 0).Count();

                if (senderFreeSlots + senderRemoveSlots < receiverItems.Count)
                    return (Game.Items.Inventory.ResultTypes.NoSpace, pData);

                var receiverFreeSlots = tData.Items.Where(x => x == null).Count();

                if (receiverFreeSlots + receiverRemoveSlots < senderItems.Count)
                    return (Game.Items.Inventory.ResultTypes.NoSpace, pData);

                var senderCurrentWeight = pData.Items.Sum(x => x?.Weight ?? 0f);

                var senderRemoveWeight = senderItems.Sum(x => x.Amount * x.ItemRoot.BaseWeight);
                var receiverRemoveWeight = receiverItems.Sum(x => x.Amount * x.ItemRoot.BaseWeight);

                if (senderCurrentWeight - senderRemoveWeight + receiverRemoveWeight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
                    return (Game.Items.Inventory.ResultTypes.NoSpace, pData);

                var receiverCurrentWeight = tData.Items.Sum(x => x?.Weight ?? 0f);

                if (receiverCurrentWeight - receiverRemoveWeight + senderRemoveWeight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
                    return (Game.Items.Inventory.ResultTypes.NoSpace, tData);

                foreach (var x in SenderVehicles)
                {
                    if (x.OwnerID != pData.CID)
                        return (Game.Items.Inventory.ResultTypes.Error, null);
                }

                foreach (var x in ReceiverVehicles)
                {
                    if (x.OwnerID != tData.CID)
                        return (Game.Items.Inventory.ResultTypes.Error, null);
                }

                foreach (var x in SenderBusinesses)
                {
                    if (x.Owner != pData.Info)
                        return (Game.Items.Inventory.ResultTypes.Error, null);
                }

                foreach (var x in ReceiverBusinesses)
                {
                    if (x.Owner != pData.Info)
                        return (Game.Items.Inventory.ResultTypes.Error, null);
                }

                var sHCount = 0;

                foreach (var x in SenderHouseBases)
                {
                    if (x.Owner != pData.Info)
                        return (Game.Items.Inventory.ResultTypes.Error, null);

                    if (x.Type == Game.Estates.HouseBase.Types.House)
                        sHCount++;
                }

                var rHCount = 0;

                foreach (var x in ReceiverHouseBases)
                {
                    if (x.Owner != pData.Info)
                        return (Game.Items.Inventory.ResultTypes.Error, null);

                    if (x.Type == Game.Estates.HouseBase.Types.House)
                        rHCount++;
                }

                foreach (var x in SenderGarages)
                {
                    if (x.Owner != pData.Info)
                        return (Game.Items.Inventory.ResultTypes.Error, null);
                }

                foreach (var x in ReceiverGarages)
                {
                    if (x.Owner != pData.Info)
                        return (Game.Items.Inventory.ResultTypes.Error, null);
                }

                var pFreeVehSlots = pData.FreeVehicleSlots;

                if (pFreeVehSlots < 0)
                    pFreeVehSlots = 0;

                if (ReceiverVehicles.Count > 0 && (pFreeVehSlots + SenderVehicles.Count - ReceiverVehicles.Count) < 0)
                {
                    pData.Player.Notify("Trade::MVOW", pData.OwnedVehicles.Count);
                    tData.Player.Notify("Trade::EOP");

                    return (Game.Items.Inventory.ResultTypes.NotEnoughVehicleSlots, pData);
                }

                var tFreeVehSlots = tData.FreeVehicleSlots;

                if (tFreeVehSlots < 0)
                    tFreeVehSlots = 0;

                if (SenderVehicles.Count > 0 && (tFreeVehSlots + ReceiverVehicles.Count - SenderVehicles.Count) < 0)
                {
                    tData.Player.Notify("Trade::MVOW", tData.OwnedVehicles.Count);
                    pData.Player.Notify("Trade::EOP");

                    return (Game.Items.Inventory.ResultTypes.NotEnoughVehicleSlots, tData);
                }

                if (ReceiverBusinesses.Count > 0)
                {
                    if (!pData.HasLicense(PlayerData.LicenseTypes.Business, true))
                    {
                        tData.Player.Notify("Trade::EOP");

                        return (Game.Items.Inventory.ResultTypes.NoBusinessLicense, pData);
                    }

                    if ((pData.FreeBusinessesSlots + SenderBusinesses.Count - ReceiverBusinesses.Count) < 0)
                    {
                        pData.Player.Notify("Trade::MBOW", pData.OwnedBusinesses.Count);
                        tData.Player.Notify("Trade::EOP");

                        return (Game.Items.Inventory.ResultTypes.NotEnoughBusinessSlots, pData);
                    }
                }

                if (SenderBusinesses.Count > 0)
                {
                    if (!tData.HasLicense(PlayerData.LicenseTypes.Business, true))
                    {
                        pData.Player.Notify("Trade::EOP");

                        return (Game.Items.Inventory.ResultTypes.NoBusinessLicense, tData);
                    }

                    if ((tData.FreeBusinessesSlots + ReceiverBusinesses.Count - SenderBusinesses.Count) < 0)
                    {
                        tData.Player.Notify("Trade::MBOW", tData.OwnedBusinesses.Count);
                        pData.Player.Notify("Trade::EOP");

                        return (Game.Items.Inventory.ResultTypes.NotEnoughBusinessSlots, tData);
                    }
                }

                if (ReceiverGarages.Count > 0 && (pData.FreeGaragesSlots + SenderGarages.Count - ReceiverGarages.Count) < 0)
                {
                    pData.Player.Notify("Trade::MGOW", pData.OwnedGarages.Count);
                    tData.Player.Notify("Trade::EOP");

                    return (Game.Items.Inventory.ResultTypes.NotEnoughGarageSlots, pData);
                }

                if (SenderGarages.Count > 0 && (tData.FreeGaragesSlots + ReceiverGarages.Count - SenderGarages.Count) < 0)
                {
                    tData.Player.Notify("Trade::MGOW", tData.OwnedGarages.Count);
                    pData.Player.Notify("Trade::EOP");

                    return (Game.Items.Inventory.ResultTypes.NotEnoughGarageSlots, tData);
                }

                if (rHCount > 0)
                {
                    if (pData.SettledHouseBase?.Type == Game.Estates.HouseBase.Types.House)
                    {
                        pData.Player.Notify("Trade::ASH");
                        tData.Player.Notify("Trade::EOP");

                        return (Game.Items.Inventory.ResultTypes.SettledToHouse, pData);
                    }

                    if ((pData.FreeHouseSlots + sHCount - rHCount) < 0)
                    {
                        pData.Player.Notify("Trade::MHOW", pData.OwnedHouses.Count);
                        tData.Player.Notify("Trade::EOP");

                        return (Game.Items.Inventory.ResultTypes.NotEnoughHouseSlots, pData);
                    }
                }

                if (sHCount > 0)
                {
                    if (pData.SettledHouseBase?.Type == Game.Estates.HouseBase.Types.House)
                    {
                        tData.Player.Notify("Trade::ASH");
                        pData.Player.Notify("Trade::EOP");

                        return (Game.Items.Inventory.ResultTypes.SettledToHouse, tData);
                    }

                    if ((tData.FreeHouseSlots + rHCount - sHCount) < 0)
                    {
                        tData.Player.Notify("Trade::MHOW", tData.OwnedHouses.Count);
                        pData.Player.Notify("Trade::EOP");

                        return (Game.Items.Inventory.ResultTypes.NotEnoughHouseSlots, tData);
                    }
                }

                rHCount = ReceiverHouseBases.Count - rHCount;
                sHCount = SenderHouseBases.Count - sHCount;

                if (rHCount > 0)
                {
                    if (pData.SettledHouseBase?.Type == Game.Estates.HouseBase.Types.Apartments)
                    {
                        pData.Player.Notify("Trade::ASA");
                        tData.Player.Notify("Trade::EOP");

                        return (Game.Items.Inventory.ResultTypes.SettledToApartments, pData);
                    }

                    if ((pData.FreeApartmentsSlots + sHCount - rHCount) < 0)
                    {
                        pData.Player.Notify("Trade::MAOW", pData.OwnedApartments.Count);
                        tData.Player.Notify("Trade::EOP");

                        return (Game.Items.Inventory.ResultTypes.NotEnoughApartmentsSlots, pData);
                    }
                }

                if (sHCount > 0)
                {
                    if (tData.SettledHouseBase?.Type == Game.Estates.HouseBase.Types.Apartments)
                    {
                        tData.Player.Notify("Trade::ASA");
                        pData.Player.Notify("Trade::EOP");

                        return (Game.Items.Inventory.ResultTypes.SettledToApartments, tData);
                    }

                    if ((tData.FreeApartmentsSlots + rHCount - sHCount) < 0)
                    {
                        tData.Player.Notify("Trade::MAOW", tData.OwnedApartments.Count);
                        pData.Player.Notify("Trade::EOP");

                        return (Game.Items.Inventory.ResultTypes.NotEnoughApartmentsSlots, tData);
                    }
                }

                foreach (var x in SenderVehicles)
                {
                    x.ChangeOwner(tData.Info);
                }

                foreach (var x in ReceiverVehicles)
                {
                    x.ChangeOwner(pData.Info);
                }

                foreach (var x in SenderBusinesses)
                {
                    x.ChangeOwner(tData.Info);
                }

                foreach (var x in ReceiverBusinesses)
                {
                    x.ChangeOwner(pData.Info);
                }

                foreach (var x in SenderHouseBases)
                {
                    x.ChangeOwner(tData.Info);
                }

                foreach (var x in ReceiverHouseBases)
                {
                    x.ChangeOwner(pData.Info);
                }

                foreach (var x in SenderGarages)
                {
                    x.ChangeOwner(tData.Info);
                }

                foreach (var x in ReceiverGarages)
                {
                    x.ChangeOwner(pData.Info);
                }

                if (ReceiverMoney > 0)
                {
                    ulong pNewCash;

                    if (tData.TryRemoveCash(ReceiverMoney, out pNewCash, true, pData))
                    {
                        tData.SetCash(pNewCash);

                        if (pData.TryAddCash(ReceiverMoney, out pNewCash, true, tData))
                            pData.SetCash(pNewCash);
                    }
                }

                if (SenderMoney > 0)
                {
                    ulong pNewCash;

                    if (pData.TryRemoveCash(SenderMoney, out pNewCash, true, pData))
                    {
                        pData.SetCash(pNewCash);

                        if (tData.TryAddCash(SenderMoney, out pNewCash, true, tData))
                            tData.SetCash(pNewCash);
                    }
                }

                var senderSlotsToUpdate = new List<(int, string)>();
                var receiverSlotsToUpdate = new List<(int, string)>();

                for (int i = 0; i < senderItems.Count; i++)
                {
                    if (senderItems[i].ItemRoot is Game.Items.IStackable senderItemS && senderItems[i].Amount < senderItemS.Amount)
                    {
                        for (int j = 0; j < pData.Items.Length; j++)
                            if (pData.Items[j] == senderItems[i].ItemRoot)
                            {
                                senderItemS.Amount -= senderItems[i].Amount;

                                senderSlotsToUpdate.Add((j, Game.Items.Item.ToClientJson(pData.Items[j], Game.Items.Inventory.GroupTypes.Items)));

                                break;
                            }

                        senderItems[i].ItemRoot = Game.Items.Stuff.CreateItem(senderItems[i].ItemRoot.ID, 0, senderItems[i].Amount, false);

                        if (senderItems[i].ItemRoot == null)
                            return (Game.Items.Inventory.ResultTypes.Error, null);
                    }
                    else
                    {
                        for (int j = 0; j < pData.Items.Length; j++)
                            if (pData.Items[j] == senderItems[i].ItemRoot)
                            {
                                pData.Items[j] = null;

                                senderSlotsToUpdate.Add((j, Game.Items.Item.ToClientJson(pData.Items[j], Game.Items.Inventory.GroupTypes.Items)));

                                break;
                            }
                    }
                }

                for (int i = 0; i < receiverItems.Count; i++)
                {
                    if (receiverItems[i].ItemRoot is Game.Items.IStackable receiverItemS && receiverItems[i].Amount < receiverItemS.Amount)
                    {
                        for (int j = 0; j < tData.Items.Length; j++)
                            if (tData.Items[j] == receiverItems[i].ItemRoot)
                            {
                                receiverItemS.Amount -= receiverItems[i].Amount;

                                receiverSlotsToUpdate.Add((j, Game.Items.Item.ToClientJson(tData.Items[j], Game.Items.Inventory.GroupTypes.Items)));

                                break;
                            }

                        receiverItems[i].ItemRoot = Game.Items.Stuff.CreateItem(receiverItems[i].ItemRoot.ID, 0, receiverItems[i].Amount, false);

                        if (receiverItems[i].ItemRoot == null)
                            return (Game.Items.Inventory.ResultTypes.Error, null);
                    }
                    else
                    {
                        for (int j = 0; j < tData.Items.Length; j++)
                            if (tData.Items[j] == receiverItems[i].ItemRoot)
                            {
                                tData.Items[j] = null;

                                receiverSlotsToUpdate.Add((j, Game.Items.Item.ToClientJson(tData.Items[j], Game.Items.Inventory.GroupTypes.Items)));

                                break;
                            }
                    }
                }

                for (int i = 0; i < senderItems.Count; i++)
                {
                    for (int j = 0; j < tData.Items.Length; j++)
                    {
                        if (tData.Items[j] == null)
                        {
                            tData.Items[j] = senderItems[i].ItemRoot;

                            receiverSlotsToUpdate.Add((j, Game.Items.Item.ToClientJson(tData.Items[j], Game.Items.Inventory.GroupTypes.Items)));

                            break;
                        }
                    }
                }

                for (int i = 0; i < receiverItems.Count; i++)
                {
                    for (int j = 0; j < pData.Items.Length; j++)
                    {
                        if (pData.Items[j] == null)
                        {
                            pData.Items[j] = receiverItems[i].ItemRoot;

                            senderSlotsToUpdate.Add((j, Game.Items.Item.ToClientJson(pData.Items[j], Game.Items.Inventory.GroupTypes.Items)));

                            break;
                        }
                    }
                }

                MySQL.CharacterItemsUpdate(pData.Info);
                MySQL.CharacterItemsUpdate(tData.Info);

                if (senderSlotsToUpdate.Count % 2 != 0)
                {
                    pData.Player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, senderSlotsToUpdate[0].Item1, senderSlotsToUpdate[0].Item2);

                    for (int i = 1; i < senderSlotsToUpdate.Count; i += 2)
                        pData.Player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, senderSlotsToUpdate[i].Item1, senderSlotsToUpdate[i].Item2, Game.Items.Inventory.GroupTypes.Items, senderSlotsToUpdate[i + 1].Item1, senderSlotsToUpdate[i + 1].Item2);
                }
                else
                {
                    for (int i = 0; i < senderSlotsToUpdate.Count; i += 2)
                        pData.Player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, senderSlotsToUpdate[i].Item1, senderSlotsToUpdate[i].Item2, Game.Items.Inventory.GroupTypes.Items, senderSlotsToUpdate[i + 1].Item1, senderSlotsToUpdate[i + 1].Item2);
                }

                if (receiverSlotsToUpdate.Count % 2 != 0)
                {
                    tData.Player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, receiverSlotsToUpdate[0].Item1, receiverSlotsToUpdate[0].Item2);

                    for (int i = 1; i < receiverSlotsToUpdate.Count; i += 2)
                        tData.Player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, receiverSlotsToUpdate[i].Item1, receiverSlotsToUpdate[i].Item2, Game.Items.Inventory.GroupTypes.Items, receiverSlotsToUpdate[i + 1].Item1, receiverSlotsToUpdate[i + 1].Item2);
                }
                else
                {
                    for (int i = 0; i < receiverSlotsToUpdate.Count; i += 2)
                        tData.Player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, receiverSlotsToUpdate[i].Item1, receiverSlotsToUpdate[i].Item2, Game.Items.Inventory.GroupTypes.Items, receiverSlotsToUpdate[i + 1].Item1, receiverSlotsToUpdate[i + 1].Item2);
                }

                return (Game.Items.Inventory.ResultTypes.Success, null);
            }

            public Trade()
            {
                SenderItems = new TradeItem[5];
                ReceiverItems = new TradeItem[5];

                SenderVehicles = new List<VehicleData.VehicleInfo>();
                ReceiverVehicles = new List<VehicleData.VehicleInfo>();

                SenderBusinesses = new List<Game.Businesses.Business>();
                ReceiverBusinesses = new List<Game.Businesses.Business>();

                SenderHouseBases = new List<Game.Estates.HouseBase>();
                ReceiverHouseBases = new List<Game.Estates.HouseBase>();

                SenderGarages = new List<Game.Estates.Garage>();
                ReceiverGarages = new List<Game.Estates.Garage>();

                SenderMoney = 0;
                ReceiverMoney = 0;

                SenderReady = false;
                ReceiverReady = false;
            }
        }
    }
}
