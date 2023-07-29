using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.EntitiesData.Vehicles;
using BlaineRP.Server.Game.Businesses;
using BlaineRP.Server.Game.Estates;
using BlaineRP.Server.Game.Inventory;
using BlaineRP.Server.Game.Items;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Management.Offers
{
    public partial class Trade
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("Trade::Accept")]
            private static void Accept(Player player)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                Offer offer = pData.ActiveOffer;

                if (offer == null || offer.TradeData == null)
                    return;

                bool isSender = offer.Sender == pData;

                if (offer.Type != OfferType.Exchange)
                {
                    if (isSender)
                        return;

                    offer.TradeData.ReceiverReady = true;
                }

                PlayerData tData = isSender ? offer.Receiver : offer.Sender;

                if (isSender)
                {
                    if (!offer.TradeData.SenderReady)
                        return;

                    if (!offer.TradeData.ReceiverReady)
                    {
                        pData.Player.Notify("Trade::PlayerNeedConfirm");

                        return;
                    }
                }
                else
                {
                    if (!offer.TradeData.ReceiverReady)
                        return;

                    if (!offer.TradeData.SenderReady)
                    {
                        pData.Player.Notify("Trade::PlayerNeedConfirm");

                        return;
                    }
                }

                (Inventory.Service.ResultTypes Result, PlayerData PlayerError) result = offer.TradeData.Execute(offer.Sender, offer.Receiver);

                if (result.Result == Inventory.Service.ResultTypes.Success)
                {
                    offer.Cancel(true, false, ReplyTypes.AutoCancel, false);

                    pData.Player.Notify("Trade::Success");
                    pData.Player.CloseAll(true);

                    tData.Player.Notify("Trade::Success");
                    tData.Player.CloseAll(true);
                }
                else
                {
                    if (result.Result == Inventory.Service.ResultTypes.Error)
                    {
                        offer.Cancel(false, false, ReplyTypes.AutoCancel, false);

                        pData.Player.Notify("Trade::Error");
                        pData.Player.CloseAll(true);

                        tData.Player.Notify("Trade::Error");
                        tData.Player.CloseAll(true);
                    }
                    else if (result.Result == Inventory.Service.ResultTypes.NotEnoughMoney)
                    {
                        if (pData == result.PlayerError)
                        {
                            pData.Player.Notify("Trade::NotEnoughMoney");
                            tData.Player.Notify("Trade::NotEnoughMoneyOther");
                        }
                        else
                        {
                            pData.Player.Notify("Trade::NotEnoughMoneyOther");
                            tData.Player.Notify("Trade::NotEnoughMoney");
                        }
                    }
                    else if (result.Result == Inventory.Service.ResultTypes.NoSpace)
                    {
                        if (pData == result.PlayerError)
                        {
                            pData.Player.Notify("Inventory::NoSpace");
                            tData.Player.Notify("Trade::NotEnoughSpaceOther");
                        }
                        else
                        {
                            pData.Player.Notify("Trade::NotEnoughSpaceOther");
                            tData.Player.Notify("Inventory::NoSpace");
                        }
                    }
                }
            }

            [RemoteEvent("Trade::Confirm")]
            private static void Confirm(Player player, bool state)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                Offer offer = pData.ActiveOffer;

                if (offer == null || offer.Type != OfferType.Exchange || offer.TradeData == null)
                    return;

                bool isSender = offer.Sender == pData;

                PlayerData tData = isSender ? offer.Receiver : offer.Sender;

                bool otherState;

                if (isSender)
                {
                    offer.TradeData.SenderReady = state;

                    otherState = offer.TradeData.ReceiverReady;
                }
                else
                {
                    offer.TradeData.ReceiverReady = state;

                    otherState = offer.TradeData.SenderReady;
                }

                pData.Player.TriggerEvent("Inventory::Update", 14, true, state, otherState);
                tData.Player.TriggerEvent("Inventory::Update", 14, false, state, otherState);
            }

            [RemoteProc("Trade::UpdateMoney")]
            private static bool UpdateMoney(Player player, int amountI)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                PlayerData pData = sRes.Data;

                if (amountI < 0)
                    return false;

                var amount = (ulong)amountI;

                Offer offer = pData.ActiveOffer;

                if (offer == null || offer.Type != OfferType.Exchange || offer.TradeData == null)
                    return false;

                if (offer.TradeData.SenderReady || offer.TradeData.ReceiverReady)
                    return false;

                bool isSender = offer.Sender == pData;

                PlayerData tData = isSender ? offer.Receiver : offer.Sender;

                if (pData.Cash < amount)
                    return false;

                if (isSender)
                {
                    if (offer.TradeData.SenderMoney == amount)
                        return false;

                    offer.TradeData.SenderMoney = amount;
                }
                else
                {
                    if (offer.TradeData.ReceiverMoney == amount)
                        return false;

                    offer.TradeData.ReceiverMoney = amount;
                }

                //pData.Player.TriggerEvent("Inventory::Update", 11, true, amount);
                tData.Player.TriggerEvent("Inventory::Update", 13, true, amount);

                return true;
            }

            [RemoteProc("Trade::UpdateProperty")]
            private static bool UpdateProperty(Player player, int pTypeNum, uint pId, bool state)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer || !Enum.IsDefined(typeof(PropertyTypes), pTypeNum))
                    return false;

                PlayerData pData = sRes.Data;

                Offer offer = pData.ActiveOffer;

                if (offer == null || offer.Type != OfferType.Exchange || offer.TradeData == null)
                    return false;

                if (offer.TradeData.SenderReady || offer.TradeData.ReceiverReady)
                    return false;

                bool isSender = offer.Sender == pData;

                PlayerData tData = isSender ? offer.Receiver : offer.Sender;

                var pType = (PropertyTypes)pTypeNum;

                if (pType == PropertyTypes.Vehicle)
                {
                    var veh = pData.OwnedVehicles.Where(x => x.VID == pId).FirstOrDefault();

                    if (veh == null)
                        return false;

                    List<VehicleInfo> tradeVehs = isSender ? offer.TradeData.SenderVehicles : offer.TradeData.ReceiverVehicles;

                    if (state == tradeVehs.Contains(veh))
                        return false;

                    if (state)
                    {
                        if (tradeVehs.Count >= Properties.Settings.Static.MAX_VEHICLES_IN_TRADE)
                        {
                            player.Notify("Trade::MVIT", Properties.Settings.Static.MAX_VEHICLES_IN_TRADE);

                            return false;
                        }

                        tradeVehs.Add(veh);
                    }
                    else
                    {
                        tradeVehs.Remove(veh);
                    }

                    pData.Player.TriggerEvent("Inventory::Update", 11, false, state, pTypeNum, pId, veh.ID);
                    tData.Player.TriggerEvent("Inventory::Update", 13, false, state, pTypeNum, pId, veh.ID);
                }
                else if (pType == PropertyTypes.House || pType == PropertyTypes.Apartments)
                {
                    HouseBase house = pType == PropertyTypes.House
                        ? (Estates.HouseBase)pData.OwnedHouses.Where(x => x.Id == pId).FirstOrDefault()
                        : (Estates.HouseBase)pData.OwnedApartments.Where(x => x.Id == pId).FirstOrDefault();

                    if (house == null)
                        return false;

                    List<HouseBase> tradeHouses = isSender ? offer.TradeData.SenderHouseBases : offer.TradeData.ReceiverHouseBases;

                    if (state == tradeHouses.Contains(house))
                        return false;

                    if (state)
                    {
                        if (tradeHouses.Count >= Properties.Settings.Static.MAX_HOUSEBASES_IN_TRADE)
                        {
                            player.Notify("Trade::MHBIT", Properties.Settings.Static.MAX_HOUSEBASES_IN_TRADE);

                            return false;
                        }

                        if (house.Balance < (ulong)(Properties.Settings.Static.MIN_PAID_HOURS_HOUSE_APS * house.Tax))
                        {
                            player.Notify(house.Type == Estates.HouseBase.Types.House ? "Trade::MHPH" : "Trade::MAPH", pId, Properties.Settings.Static.MIN_PAID_HOURS_HOUSE_APS);

                            return false;
                        }

                        tradeHouses.Add(house);
                    }
                    else
                    {
                        tradeHouses.Remove(house);
                    }

                    pData.Player.TriggerEvent("Inventory::Update", 11, false, state, pTypeNum, pId);
                    tData.Player.TriggerEvent("Inventory::Update", 13, false, state, pTypeNum, pId);
                }
                else if (pType == PropertyTypes.Garage)
                {
                    var garage = pData.OwnedGarages.Where(x => x.Id == pId).FirstOrDefault();

                    if (garage == null)
                        return false;

                    List<Garage> tradeGarages = isSender ? offer.TradeData.SenderGarages : offer.TradeData.ReceiverGarages;

                    if (state == tradeGarages.Contains(garage))
                        return false;

                    if (state)
                    {
                        if (tradeGarages.Count >= Properties.Settings.Static.MAX_GARAGES_IN_TRADE)
                        {
                            player.Notify("Trade::MGIT", Properties.Settings.Static.MAX_GARAGES_IN_TRADE);

                            return false;
                        }

                        if (garage.Balance < (ulong)(Properties.Settings.Static.MIN_PAID_HOURS_GARAGE * garage.Tax))
                        {
                            player.Notify("Trade::MGPH", pId, Properties.Settings.Static.MIN_PAID_HOURS_GARAGE);

                            return false;
                        }

                        tradeGarages.Add(garage);
                    }
                    else
                    {
                        tradeGarages.Remove(garage);
                    }

                    pData.Player.TriggerEvent("Inventory::Update", 11, false, state, pTypeNum, pId);
                    tData.Player.TriggerEvent("Inventory::Update", 13, false, state, pTypeNum, pId);
                }
                else if (pType == PropertyTypes.Business)
                {
                    var biz = pData.OwnedBusinesses.Where(x => x.ID == pId).FirstOrDefault();

                    if (biz == null)
                        return false;

                    List<Business> tradeBusinesses = isSender ? offer.TradeData.SenderBusinesses : offer.TradeData.ReceiverBusinesses;

                    if (state == tradeBusinesses.Contains(biz))
                        return false;

                    if (state)
                    {
                        if (tradeBusinesses.Count >= Properties.Settings.Static.MAX_BUSINESS_IN_TRADE)
                        {
                            player.Notify("Trade::MBIT", Properties.Settings.Static.MAX_BUSINESS_IN_TRADE);

                            return false;
                        }

                        if (biz.Bank < (ulong)(Properties.Settings.Static.MIN_PAID_HOURS_BUSINESS * biz.Rent))
                        {
                            player.Notify("Trade::MBPH", pId, Properties.Settings.Static.MIN_PAID_HOURS_BUSINESS);

                            return false;
                        }

                        tradeBusinesses.Add(biz);
                    }
                    else
                    {
                        tradeBusinesses.Remove(biz);
                    }

                    pData.Player.TriggerEvent("Inventory::Update", 11, false, state, pTypeNum, pId);
                    tData.Player.TriggerEvent("Inventory::Update", 13, false, state, pTypeNum, pId);
                }
                else
                {
                    return false;
                }

                return true;
            }

            [RemoteEvent("Trade::UpdateItem")]
            private static void UpdateItem(Player player, bool fromPockets, int slotTo, int slotFrom, int amount)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                Offer offer = pData.ActiveOffer;

                if (offer == null || offer.Type != OfferType.Exchange || offer.TradeData == null)
                    return;

                if (offer.TradeData.SenderReady || offer.TradeData.ReceiverReady)
                    return;

                bool isSender = offer.Sender == pData;

                PlayerData tData = isSender ? offer.Receiver : offer.Sender;

                if (fromPockets)
                {
                    if (slotTo < 0 || slotTo >= offer.TradeData.SenderItems.Length)
                        return;

                    if (slotFrom < 0 || slotFrom >= pData.Items.Length)
                        return;

                    Item item = pData.Items[slotFrom];

                    if (item == null)
                        return;

                    if (item.IsTemp)
                        return;

                    if (item is Items.IStackable stackable)
                    {
                        if (amount <= 0 || amount > stackable.Amount)
                            amount = stackable.Amount;
                    }
                    else
                    {
                        amount = 1;
                    }

                    if (isSender)
                        for (var i = 0; i < offer.TradeData.SenderItems.Length; i++)
                        {
                            if (i == slotTo)
                                continue;

                            if (offer.TradeData.SenderItems[i]?.ItemRoot == item)
                                slotTo = i;
                        }
                    else
                        for (var i = 0; i < offer.TradeData.ReceiverItems.Length; i++)
                        {
                            if (i == slotTo)
                                continue;

                            if (offer.TradeData.ReceiverItems[i]?.ItemRoot == item)
                                slotTo = i;
                        }

                    TradeItem iData = isSender ? offer.TradeData.SenderItems[slotTo] : offer.TradeData.ReceiverItems[slotTo];

                    if (iData != null)
                    {
                        if (iData.ItemRoot == item)
                        {
                            if (item is Items.IStackable stackableR)
                            {
                                int expectedAmount = stackableR.Amount - iData.Amount;

                                if (amount >= expectedAmount)
                                    amount = expectedAmount;
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            iData.ItemRoot = item;
                            iData.Amount = amount;
                        }
                    }
                    else
                    {
                        iData = new TradeItem(item, amount);

                        if (isSender)
                            offer.TradeData.SenderItems[slotTo] = iData;
                        else
                            offer.TradeData.ReceiverItems[slotTo] = iData;
                    }

                    string upd2 = iData.ToClientJson();

                    pData.Player.TriggerEvent("Inventory::Update", 10, slotFrom, slotTo, upd2);
                    tData.Player.TriggerEvent("Inventory::Update", 12, slotTo, upd2);
                }
                else
                {
                    if (slotFrom < 0 || slotFrom >= offer.TradeData.SenderItems.Length)
                        return;

                    TradeItem iData = isSender ? offer.TradeData.SenderItems[slotFrom] : offer.TradeData.ReceiverItems[slotFrom];

                    if (iData == null || iData.ItemRoot == null)
                        return;

                    if (amount <= 0 || amount > iData.Amount)
                        amount = iData.Amount;

                    if (amount == iData.Amount)
                    {
                        iData = null;

                        if (isSender)
                            offer.TradeData.SenderItems[slotFrom] = iData;
                        else
                            offer.TradeData.ReceiverItems[slotFrom] = iData;

                        pData.Player.TriggerEvent("Inventory::Update", 10, -1, slotFrom, Items.Item.ToClientJson(null, GroupTypes.Items));
                        tData.Player.TriggerEvent("Inventory::Update", 12, slotFrom, Items.Item.ToClientJson(null, GroupTypes.Items));
                    }
                    else
                    {
                        iData.Amount -= amount;

                        string upd2 = iData.ToClientJson();

                        pData.Player.TriggerEvent("Inventory::Update", 10, -1, slotFrom, upd2);
                        tData.Player.TriggerEvent("Inventory::Update", 12, slotFrom, upd2);
                    }
                }
            }
        }
    }
}