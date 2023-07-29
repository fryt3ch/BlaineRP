using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.Businesses;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using BlaineRP.Server.Game.Estates;
using BlaineRP.Server.Game.Inventory;

namespace BlaineRP.Server.Game.Offers
{
    public partial class Trade
    {
        public TradeItem[] SenderItems { get; set; }
        public TradeItem[] ReceiverItems { get; set; }

        public List<VehicleInfo> SenderVehicles { get; set; }
        public List<VehicleInfo> ReceiverVehicles { get; set; }

        public List<Businesses.Business> SenderBusinesses { get; set; }
        public List<Businesses.Business> ReceiverBusinesses { get; set; }

        public List<Estates.HouseBase> SenderHouseBases { get; set; }
        public List<Estates.HouseBase> ReceiverHouseBases { get; set; }

        public List<Estates.Garage> SenderGarages { get; set; }
        public List<Estates.Garage> ReceiverGarages { get; set; }

        public ulong SenderMoney { get; set; }
        public ulong ReceiverMoney { get; set; }

        public bool SenderReady { get; set; }
        public bool ReceiverReady { get; set; }

        public (Inventory.Service.ResultTypes Result, PlayerData PlayerError) Execute(PlayerData pData, PlayerData tData)
        {
            if (pData.Cash < SenderMoney)
                return (Inventory.Service.ResultTypes.NotEnoughMoney, pData);

            if (tData.Cash < ReceiverMoney)
                return (Inventory.Service.ResultTypes.NotEnoughMoney, tData);

            int senderFreeSlots = pData.Items.Where(x => x == null).Count();

            var senderItems = SenderItems.Where(x => x != null && x.ItemRoot != null).ToList();
            var receiverItems = ReceiverItems.Where(x => x != null && x.ItemRoot != null).ToList();

            int senderRemoveSlots = senderItems.Where(x => ((x.ItemRoot as Items.IStackable)?.Amount ?? 1) - x.Amount == 0).Count();
            int receiverRemoveSlots = receiverItems.Where(x => ((x.ItemRoot as Items.IStackable)?.Amount ?? 1) - x.Amount == 0).Count();

            if (senderFreeSlots + senderRemoveSlots < receiverItems.Count)
                return (Inventory.Service.ResultTypes.NoSpace, pData);

            int receiverFreeSlots = tData.Items.Where(x => x == null).Count();

            if (receiverFreeSlots + receiverRemoveSlots < senderItems.Count)
                return (Inventory.Service.ResultTypes.NoSpace, pData);

            float senderCurrentWeight = pData.Items.Sum(x => x?.Weight ?? 0f);

            float senderRemoveWeight = senderItems.Sum(x => x.Amount * x.ItemRoot.BaseWeight);
            float receiverRemoveWeight = receiverItems.Sum(x => x.Amount * x.ItemRoot.BaseWeight);

            if (senderCurrentWeight - senderRemoveWeight + receiverRemoveWeight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
                return (Inventory.Service.ResultTypes.NoSpace, pData);

            float receiverCurrentWeight = tData.Items.Sum(x => x?.Weight ?? 0f);

            if (receiverCurrentWeight - receiverRemoveWeight + senderRemoveWeight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
                return (Inventory.Service.ResultTypes.NoSpace, tData);

            foreach (VehicleInfo x in SenderVehicles)
            {
                if (x.OwnerID != pData.CID)
                    return (Inventory.Service.ResultTypes.Error, null);
            }

            foreach (VehicleInfo x in ReceiverVehicles)
            {
                if (x.OwnerID != tData.CID)
                    return (Inventory.Service.ResultTypes.Error, null);
            }

            foreach (Business x in SenderBusinesses)
            {
                if (x.Owner != pData.Info)
                    return (Inventory.Service.ResultTypes.Error, null);
            }

            foreach (Business x in ReceiverBusinesses)
            {
                if (x.Owner != pData.Info)
                    return (Inventory.Service.ResultTypes.Error, null);
            }

            var sHCount = 0;

            foreach (HouseBase x in SenderHouseBases)
            {
                if (x.Owner != pData.Info)
                    return (Inventory.Service.ResultTypes.Error, null);

                if (x.Type == Estates.HouseBase.Types.House)
                    sHCount++;
            }

            var rHCount = 0;

            foreach (HouseBase x in ReceiverHouseBases)
            {
                if (x.Owner != pData.Info)
                    return (Inventory.Service.ResultTypes.Error, null);

                if (x.Type == Estates.HouseBase.Types.House)
                    rHCount++;
            }

            foreach (Garage x in SenderGarages)
            {
                if (x.Owner != pData.Info)
                    return (Inventory.Service.ResultTypes.Error, null);
            }

            foreach (Garage x in ReceiverGarages)
            {
                if (x.Owner != pData.Info)
                    return (Inventory.Service.ResultTypes.Error, null);
            }

            int pFreeVehSlots = pData.FreeVehicleSlots;

            if (pFreeVehSlots < 0)
                pFreeVehSlots = 0;

            if (ReceiverVehicles.Count > 0 && pFreeVehSlots + SenderVehicles.Count - ReceiverVehicles.Count < 0)
            {
                pData.Player.Notify("Trade::MVOW", pData.OwnedVehicles.Count);
                tData.Player.Notify("Trade::EOP");

                return (Inventory.Service.ResultTypes.NotEnoughVehicleSlots, pData);
            }

            int tFreeVehSlots = tData.FreeVehicleSlots;

            if (tFreeVehSlots < 0)
                tFreeVehSlots = 0;

            if (SenderVehicles.Count > 0 && tFreeVehSlots + ReceiverVehicles.Count - SenderVehicles.Count < 0)
            {
                tData.Player.Notify("Trade::MVOW", tData.OwnedVehicles.Count);
                pData.Player.Notify("Trade::EOP");

                return (Inventory.Service.ResultTypes.NotEnoughVehicleSlots, tData);
            }

            if (ReceiverBusinesses.Count > 0)
            {
                if (!pData.HasLicense(LicenseType.Business, true))
                {
                    tData.Player.Notify("Trade::EOP");

                    return (Inventory.Service.ResultTypes.NoBusinessLicense, pData);
                }

                if (pData.FreeBusinessesSlots + SenderBusinesses.Count - ReceiverBusinesses.Count < 0)
                {
                    pData.Player.Notify("Trade::MBOW", pData.OwnedBusinesses.Count);
                    tData.Player.Notify("Trade::EOP");

                    return (Inventory.Service.ResultTypes.NotEnoughBusinessSlots, pData);
                }
            }

            if (SenderBusinesses.Count > 0)
            {
                if (!tData.HasLicense(LicenseType.Business, true))
                {
                    pData.Player.Notify("Trade::EOP");

                    return (Inventory.Service.ResultTypes.NoBusinessLicense, tData);
                }

                if (tData.FreeBusinessesSlots + ReceiverBusinesses.Count - SenderBusinesses.Count < 0)
                {
                    tData.Player.Notify("Trade::MBOW", tData.OwnedBusinesses.Count);
                    pData.Player.Notify("Trade::EOP");

                    return (Inventory.Service.ResultTypes.NotEnoughBusinessSlots, tData);
                }
            }

            if (ReceiverGarages.Count > 0 && pData.FreeGaragesSlots + SenderGarages.Count - ReceiverGarages.Count < 0)
            {
                pData.Player.Notify("Trade::MGOW", pData.OwnedGarages.Count);
                tData.Player.Notify("Trade::EOP");

                return (Inventory.Service.ResultTypes.NotEnoughGarageSlots, pData);
            }

            if (SenderGarages.Count > 0 && tData.FreeGaragesSlots + ReceiverGarages.Count - SenderGarages.Count < 0)
            {
                tData.Player.Notify("Trade::MGOW", tData.OwnedGarages.Count);
                pData.Player.Notify("Trade::EOP");

                return (Inventory.Service.ResultTypes.NotEnoughGarageSlots, tData);
            }

            if (rHCount > 0)
            {
                if (pData.SettledHouseBase?.Type == Estates.HouseBase.Types.House)
                {
                    pData.Player.Notify("Trade::ASH");
                    tData.Player.Notify("Trade::EOP");

                    return (Inventory.Service.ResultTypes.SettledToHouse, pData);
                }

                if (pData.FreeHouseSlots + sHCount - rHCount < 0)
                {
                    pData.Player.Notify("Trade::MHOW", pData.OwnedHouses.Count);
                    tData.Player.Notify("Trade::EOP");

                    return (Inventory.Service.ResultTypes.NotEnoughHouseSlots, pData);
                }
            }

            if (sHCount > 0)
            {
                if (pData.SettledHouseBase?.Type == Estates.HouseBase.Types.House)
                {
                    tData.Player.Notify("Trade::ASH");
                    pData.Player.Notify("Trade::EOP");

                    return (Inventory.Service.ResultTypes.SettledToHouse, tData);
                }

                if (tData.FreeHouseSlots + rHCount - sHCount < 0)
                {
                    tData.Player.Notify("Trade::MHOW", tData.OwnedHouses.Count);
                    pData.Player.Notify("Trade::EOP");

                    return (Inventory.Service.ResultTypes.NotEnoughHouseSlots, tData);
                }
            }

            rHCount = ReceiverHouseBases.Count - rHCount;
            sHCount = SenderHouseBases.Count - sHCount;

            if (rHCount > 0)
            {
                if (pData.SettledHouseBase?.Type == Estates.HouseBase.Types.Apartments)
                {
                    pData.Player.Notify("Trade::ASA");
                    tData.Player.Notify("Trade::EOP");

                    return (Inventory.Service.ResultTypes.SettledToApartments, pData);
                }

                if (pData.FreeApartmentsSlots + sHCount - rHCount < 0)
                {
                    pData.Player.Notify("Trade::MAOW", pData.OwnedApartments.Count);
                    tData.Player.Notify("Trade::EOP");

                    return (Inventory.Service.ResultTypes.NotEnoughApartmentsSlots, pData);
                }
            }

            if (sHCount > 0)
            {
                if (tData.SettledHouseBase?.Type == Estates.HouseBase.Types.Apartments)
                {
                    tData.Player.Notify("Trade::ASA");
                    pData.Player.Notify("Trade::EOP");

                    return (Inventory.Service.ResultTypes.SettledToApartments, tData);
                }

                if (tData.FreeApartmentsSlots + rHCount - sHCount < 0)
                {
                    tData.Player.Notify("Trade::MAOW", tData.OwnedApartments.Count);
                    pData.Player.Notify("Trade::EOP");

                    return (Inventory.Service.ResultTypes.NotEnoughApartmentsSlots, tData);
                }
            }

            foreach (VehicleInfo x in SenderVehicles)
            {
                x.ChangeOwner(tData.Info);
            }

            foreach (VehicleInfo x in ReceiverVehicles)
            {
                x.ChangeOwner(pData.Info);
            }

            foreach (Business x in SenderBusinesses)
            {
                x.ChangeOwner(tData.Info);
            }

            foreach (Business x in ReceiverBusinesses)
            {
                x.ChangeOwner(pData.Info);
            }

            foreach (HouseBase x in SenderHouseBases)
            {
                x.ChangeOwner(tData.Info);
            }

            foreach (HouseBase x in ReceiverHouseBases)
            {
                x.ChangeOwner(pData.Info);
            }

            foreach (Garage x in SenderGarages)
            {
                x.ChangeOwner(tData.Info);
            }

            foreach (Garage x in ReceiverGarages)
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

            for (var i = 0; i < senderItems.Count; i++)
            {
                if (senderItems[i].ItemRoot is Items.IStackable senderItemS && senderItems[i].Amount < senderItemS.Amount)
                {
                    for (var j = 0; j < pData.Items.Length; j++)
                    {
                        if (pData.Items[j] == senderItems[i].ItemRoot)
                        {
                            senderItemS.Amount -= senderItems[i].Amount;

                            senderSlotsToUpdate.Add((j, Items.Item.ToClientJson(pData.Items[j], GroupTypes.Items)));

                            break;
                        }
                    }

                    senderItems[i].ItemRoot = Items.Stuff.CreateItem(senderItems[i].ItemRoot.ID, 0, senderItems[i].Amount, false);

                    if (senderItems[i].ItemRoot == null)
                        return (Inventory.Service.ResultTypes.Error, null);
                }
                else
                {
                    for (var j = 0; j < pData.Items.Length; j++)
                    {
                        if (pData.Items[j] == senderItems[i].ItemRoot)
                        {
                            pData.Items[j] = null;

                            senderSlotsToUpdate.Add((j, Items.Item.ToClientJson(pData.Items[j], GroupTypes.Items)));

                            break;
                        }
                    }
                }
            }

            for (var i = 0; i < receiverItems.Count; i++)
            {
                if (receiverItems[i].ItemRoot is Items.IStackable receiverItemS && receiverItems[i].Amount < receiverItemS.Amount)
                {
                    for (var j = 0; j < tData.Items.Length; j++)
                    {
                        if (tData.Items[j] == receiverItems[i].ItemRoot)
                        {
                            receiverItemS.Amount -= receiverItems[i].Amount;

                            receiverSlotsToUpdate.Add((j, Items.Item.ToClientJson(tData.Items[j], GroupTypes.Items)));

                            break;
                        }
                    }

                    receiverItems[i].ItemRoot = Items.Stuff.CreateItem(receiverItems[i].ItemRoot.ID, 0, receiverItems[i].Amount, false);

                    if (receiverItems[i].ItemRoot == null)
                        return (Inventory.Service.ResultTypes.Error, null);
                }
                else
                {
                    for (var j = 0; j < tData.Items.Length; j++)
                    {
                        if (tData.Items[j] == receiverItems[i].ItemRoot)
                        {
                            tData.Items[j] = null;

                            receiverSlotsToUpdate.Add((j, Items.Item.ToClientJson(tData.Items[j], GroupTypes.Items)));

                            break;
                        }
                    }
                }
            }

            for (var i = 0; i < senderItems.Count; i++)
            {
                for (var j = 0; j < tData.Items.Length; j++)
                {
                    if (tData.Items[j] == null)
                    {
                        tData.Items[j] = senderItems[i].ItemRoot;

                        receiverSlotsToUpdate.Add((j, Items.Item.ToClientJson(tData.Items[j], GroupTypes.Items)));

                        break;
                    }
                }
            }

            for (var i = 0; i < receiverItems.Count; i++)
            {
                for (var j = 0; j < pData.Items.Length; j++)
                {
                    if (pData.Items[j] == null)
                    {
                        pData.Items[j] = receiverItems[i].ItemRoot;

                        senderSlotsToUpdate.Add((j, Items.Item.ToClientJson(pData.Items[j], GroupTypes.Items)));

                        break;
                    }
                }
            }

            MySQL.CharacterItemsUpdate(pData.Info);
            MySQL.CharacterItemsUpdate(tData.Info);

            if (senderSlotsToUpdate.Count % 2 != 0)
            {
                pData.Player.InventoryUpdate(GroupTypes.Items, senderSlotsToUpdate[0].Item1, senderSlotsToUpdate[0].Item2);

                for (var i = 1; i < senderSlotsToUpdate.Count; i += 2)
                {
                    pData.Player.InventoryUpdate(GroupTypes.Items,
                        senderSlotsToUpdate[i].Item1,
                        senderSlotsToUpdate[i].Item2,
                        GroupTypes.Items,
                        senderSlotsToUpdate[i + 1].Item1,
                        senderSlotsToUpdate[i + 1].Item2
                    );
                }
            }
            else
            {
                for (var i = 0; i < senderSlotsToUpdate.Count; i += 2)
                {
                    pData.Player.InventoryUpdate(GroupTypes.Items,
                        senderSlotsToUpdate[i].Item1,
                        senderSlotsToUpdate[i].Item2,
                        GroupTypes.Items,
                        senderSlotsToUpdate[i + 1].Item1,
                        senderSlotsToUpdate[i + 1].Item2
                    );
                }
            }

            if (receiverSlotsToUpdate.Count % 2 != 0)
            {
                tData.Player.InventoryUpdate(GroupTypes.Items, receiverSlotsToUpdate[0].Item1, receiverSlotsToUpdate[0].Item2);

                for (var i = 1; i < receiverSlotsToUpdate.Count; i += 2)
                {
                    tData.Player.InventoryUpdate(GroupTypes.Items,
                        receiverSlotsToUpdate[i].Item1,
                        receiverSlotsToUpdate[i].Item2,
                        GroupTypes.Items,
                        receiverSlotsToUpdate[i + 1].Item1,
                        receiverSlotsToUpdate[i + 1].Item2
                    );
                }
            }
            else
            {
                for (var i = 0; i < receiverSlotsToUpdate.Count; i += 2)
                {
                    tData.Player.InventoryUpdate(GroupTypes.Items,
                        receiverSlotsToUpdate[i].Item1,
                        receiverSlotsToUpdate[i].Item2,
                        GroupTypes.Items,
                        receiverSlotsToUpdate[i + 1].Item1,
                        receiverSlotsToUpdate[i + 1].Item2
                    );
                }
            }

            return (Inventory.Service.ResultTypes.Success, null);
        }

        public Trade()
        {
            SenderItems = new TradeItem[5];
            ReceiverItems = new TradeItem[5];

            SenderVehicles = new List<VehicleInfo>();
            ReceiverVehicles = new List<VehicleInfo>();

            SenderBusinesses = new List<Businesses.Business>();
            ReceiverBusinesses = new List<Businesses.Business>();

            SenderHouseBases = new List<Estates.HouseBase>();
            ReceiverHouseBases = new List<Estates.HouseBase>();

            SenderGarages = new List<Estates.Garage>();
            ReceiverGarages = new List<Estates.Garage>();

            SenderMoney = 0;
            ReceiverMoney = 0;

            SenderReady = false;
            ReceiverReady = false;
        }
    }
}