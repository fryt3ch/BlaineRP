﻿using GTANetworkAPI;
using System;
using System.Linq;
using static BCRPServer.Sync.Offers;

namespace BCRPServer.Events.Players
{
    class Offers : Script
    {
        [RemoteEvent("Offers::Send")]
        private static void Send(Player player, Player target, int type, string data)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            if (!Enum.IsDefined(typeof(Types), type))
                return;

            var pData = sRes.Data;

            if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                return;

            var tData = target.GetMainData();

            if (tData?.Player?.Exists != true || target == player)
                return;

            object dataObj = null;

            var oType = (Types)type;

            var res = ((Func<ReturnTypes>)(() =>
            {
                if (!pData.Player.AreEntitiesNearby(tData.Player, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                    return ReturnTypes.Error;

                if (tData.IsBusy)
                    return ReturnTypes.TargetBusy;

                if (pData.ActiveOffer != null)
                    return ReturnTypes.SourceHasOffer;

                if (tData.ActiveOffer != null)
                    return ReturnTypes.TargetHasOffer;

                try
                {
                    dataObj = data.DeserializeFromJson<object>();

                    if (oType == Types.Settle)
                    {
                        var curHouseBase = pData.CurrentHouseBase;

                        if (curHouseBase == null)
                            return ReturnTypes.Error;

                        if (curHouseBase.Owner != pData.Info)
                        {
                            player.Notify("House::NotAllowed");

                            return ReturnTypes.Error;
                        }

                        target.TriggerEvent("Offer::Show", player.Handle, type, curHouseBase.Type);
                    }
                    else if (oType == Types.Cash)
                    {
                        var cash = Convert.ToUInt32(dataObj);

                        dataObj = cash;

                        if (cash == 0)
                            return ReturnTypes.Error;

                        if (!pData.TryRemoveCash(cash, out _, true))
                            return ReturnTypes.Error;

                        target.TriggerEvent("Offer::Show", player.Handle, type, cash);
                    }
                    else if (oType == Types.WaypointShare)
                    {
                        var dataObjD = ((string)dataObj).Split('_');

                        dataObj = new Vector3(float.Parse(dataObjD[0]), float.Parse(dataObjD[1]), 0f);
                    }
                    else if (oType == Types.ShowVehiclePassport)
                    {
                        var vid = (uint)(long)dataObj;

                        var found = pData.OwnedVehicles.Where(x => x.VID == vid).FirstOrDefault();

                        if (found == null)
                            return ReturnTypes.Error;

                        dataObj = found;
                    }
                    else if (oType == Types.SellVehicle)
                    {
                        var dataObjD = ((string)dataObj).Split('_');

                        var price = int.Parse(dataObjD[1]);

                        if (price <= 0)
                            return ReturnTypes.Error;

                        var vid = uint.Parse(dataObjD[0]);

                        var vInfo = pData.OwnedVehicles.Where(x => x.VID == vid).FirstOrDefault();

                        if (vInfo == null)
                            return ReturnTypes.Error;

                        dataObj = new Offer.PropertySellData(vInfo, (ulong)price);
                    }
                    else if (oType == Types.SellBusiness)
                    {
                        var dataObjD = ((string)dataObj).Split('_');

                        var price = int.Parse(dataObjD[1]);

                        if (price <= 0)
                            return ReturnTypes.Error;

                        var businessId = int.Parse(dataObjD[0]);

                        var bInfo = pData.OwnedBusinesses.Where(x => x.ID == businessId).FirstOrDefault();

                        if (bInfo == null)
                            return ReturnTypes.Error;

                        dataObj = new Offer.PropertySellData(bInfo, (ulong)price);
                    }
                    else if (oType == Types.SellEstate)
                    {
                        var dataObjD = ((string)dataObj).Split('_');

                        var price = int.Parse(dataObjD[2]);

                        if (price <= 0)
                            return ReturnTypes.Error;

                        var estType = (PlayerData.PropertyTypes)int.Parse(dataObjD[0]);

                        var estId = uint.Parse(dataObjD[1]);

                        if (estType == PlayerData.PropertyTypes.House)
                        {
                            var house = pData.OwnedHouses.Where(x => x.Id == estId).FirstOrDefault();

                            if (house == null)
                                return ReturnTypes.Error;

                            dataObj = new Offer.PropertySellData(house, (ulong)price);
                        }
                        else if (estType == PlayerData.PropertyTypes.Apartments)
                        {
                            var aps = pData.OwnedApartments.Where(x => x.Id == estId).FirstOrDefault();

                            if (aps == null)
                                return ReturnTypes.Error;

                            dataObj = new Offer.PropertySellData(aps, (ulong)price);
                        }
                        else if (estType == PlayerData.PropertyTypes.Garage)
                        {
                            var garage = pData.OwnedGarages.Where(x => x.Id == estId).FirstOrDefault();

                            if (garage == null)
                                return ReturnTypes.Error;

                            dataObj = new Offer.PropertySellData(garage, (ulong)price);
                        }
                        else
                            return ReturnTypes.Error;
                    }
                    else if (oType == Types.PoliceFine)
                    {
                        var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                        if (fData == null)
                            return ReturnTypes.Error;

                        if (!fData.HasMemberPermission(pData.Info, 14, true))
                            return ReturnTypes.Error;

                        var dataObjD = ((string)dataObj).Split('_');

                        var amount = int.Parse(dataObjD[0]);

                        if (amount < Game.Fractions.Police.FINE_MIN_AMOUNT || amount > Game.Fractions.Police.FINE_MAX_AMOUNT)
                            return ReturnTypes.Error;

                        var reason = dataObjD[1].Trim();

                        if (!Game.Fractions.Police.FineReasonRegex.IsMatch(reason))
                            return ReturnTypes.Error;

                        dataObj = $"{amount}_{reason}";

                        target.TriggerEvent("Offer::Show", player.Handle, type, dataObj);
                    }
                    else if (oType == Types.InviteFraction)
                    {
                        if (tData.Fraction != Game.Fractions.Types.None)
                            return ReturnTypes.Error;

                        if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                            return ReturnTypes.Error;

                        var fData = Game.Fractions.Fraction.Get(pData.Fraction);

                        if (!fData.HasMemberPermission(pData.Info, 2, true))
                            return ReturnTypes.Error;

                        dataObj = fData.Type;

                        target.TriggerEvent("Offer::Show", player.Handle, type, dataObj);
                    }
                }
                catch (Exception ex)
                {
                    return ReturnTypes.Error;
                }

                Offer.Create(pData, tData, oType, -1, dataObj);

                return ReturnTypes.Success;
            })).Invoke();

            switch (res)
            {
                case ReturnTypes.Success:
                    if (oType != Types.Cash && oType != Types.Settle && oType != Types.PoliceFine)
                    {
                        target.TriggerEvent("Offer::Show", player.Handle, type);
                    }

                    player.TriggerEvent("Offer::Reply::Server", true, false, false);
                    player.Notify("Offer::Sent");
                    break;

                case ReturnTypes.TargetBusy:
                    player.TriggerEvent("Offer::Reply::Server", false, false, true);
                    player.Notify("Offer::TargetBusy");
                    break;

                case ReturnTypes.SourceHasOffer:
                    player.TriggerEvent("Offer::Reply::Server", false, false, true);
                    player.Notify("Offer::HasOffer");
                    break;

                case ReturnTypes.TargetHasOffer:
                    player.TriggerEvent("Offer::Reply::Server", false, false, true);
                    player.Notify("Offer::TargetHasOffer");
                    break;

                case ReturnTypes.Error:
                    player.TriggerEvent("Offer::Reply::Server", false, false, true);
                    break;
            }
        }

        [RemoteEvent("Offers::Reply")]
        private static void Reply(Player player, int rTypeNum)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(ReplyTypes), rTypeNum))
                return;

            var rType = (ReplyTypes)rTypeNum;

            var offer = pData.ActiveOffer;

            if (offer == null)
                return;

            if (pData == offer.Receiver)
            {
                if (rType == ReplyTypes.Accept)
                {
                    offer.Execute();
                }
                else
                {
                    offer.Cancel(false, false, rType, false);
                }
            }
            else
            {
                offer.Cancel(false, true, rType, false);
            }
        }

        [RemoteEvent("Trade::Accept")]
        private static void Accept(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var offer = pData.ActiveOffer;

            if (offer == null || offer.TradeData == null)
                return;

            bool isSender = offer.Sender == pData;

            if (offer.Type != Types.Exchange)
            {
                if (isSender)
                    return;

                offer.TradeData.ReceiverReady = true;
            }

            var tData = isSender ? offer.Receiver : offer.Sender;

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

            var result = offer.TradeData.Execute(offer.Sender, offer.Receiver);

            if (result.Result == Game.Items.Inventory.Results.Success)
            {
                offer.Cancel(true, false, Sync.Offers.ReplyTypes.AutoCancel, false);

                pData.Player.Notify("Trade::Success");
                pData.Player.CloseAll(true);

                tData.Player.Notify("Trade::Success");
                tData.Player.CloseAll(true);
            }
            else
            {
                if (result.Result == Game.Items.Inventory.Results.Error)
                {
                    offer.Cancel(false, false, Sync.Offers.ReplyTypes.AutoCancel, false);

                    pData.Player.Notify("Trade::Error");
                    pData.Player.CloseAll(true);

                    tData.Player.Notify("Trade::Error");
                    tData.Player.CloseAll(true);
                }
                else if (result.Result == Game.Items.Inventory.Results.NotEnoughMoney)
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
                else if (result.Result == Game.Items.Inventory.Results.NoSpace)
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
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var offer = pData.ActiveOffer;

            if (offer == null || offer.Type != Types.Exchange || offer.TradeData == null)
                return;

            bool isSender = offer.Sender == pData;

            var tData = isSender ? offer.Receiver : offer.Sender;

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
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (amountI < 0)
                return false;

            var amount = (ulong)amountI;

            var offer = pData.ActiveOffer;

            if (offer == null || offer.Type != Types.Exchange || offer.TradeData == null)
                return false;

            if (offer.TradeData.SenderReady || offer.TradeData.ReceiverReady)
                return false;

            var isSender = offer.Sender == pData;

            var tData = isSender ? offer.Receiver : offer.Sender;

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
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer || !Enum.IsDefined(typeof(PlayerData.PropertyTypes), pTypeNum))
                return false;

            var pData = sRes.Data;

            var offer = pData.ActiveOffer;

            if (offer == null || offer.Type != Types.Exchange || offer.TradeData == null)
                return false;

            if (offer.TradeData.SenderReady || offer.TradeData.ReceiverReady)
                return false;

            var isSender = offer.Sender == pData;

            var tData = isSender ? offer.Receiver : offer.Sender;

            var pType = (PlayerData.PropertyTypes)pTypeNum;

            if (pType == PlayerData.PropertyTypes.Vehicle)
            {
                var veh = pData.OwnedVehicles.Where(x => x.VID == pId).FirstOrDefault();

                if (veh == null)
                    return false;

                var tradeVehs = isSender ? offer.TradeData.SenderVehicles : offer.TradeData.ReceiverVehicles;

                if (state == tradeVehs.Contains(veh))
                    return false;

                if (state)
                {
                    if (tradeVehs.Count >= Settings.MAX_VEHICLES_IN_TRADE)
                    {
                        player.Notify("Trade::MVIT", Settings.MAX_VEHICLES_IN_TRADE);

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
            else if (pType == PlayerData.PropertyTypes.House || pType == PlayerData.PropertyTypes.Apartments)
            {
                var house = pType == PlayerData.PropertyTypes.House ? (Game.Estates.HouseBase)pData.OwnedHouses.Where(x => x.Id == pId).FirstOrDefault() : (Game.Estates.HouseBase)pData.OwnedApartments.Where(x => x.Id == pId).FirstOrDefault();

                if (house == null)
                    return false;

                var tradeHouses = isSender ? offer.TradeData.SenderHouseBases : offer.TradeData.ReceiverHouseBases;

                if (state == tradeHouses.Contains(house))
                    return false;

                if (state)
                {
                    if (tradeHouses.Count >= Settings.MAX_HOUSEBASES_IN_TRADE)
                    {
                        player.Notify("Trade::MHBIT", Settings.MAX_HOUSEBASES_IN_TRADE);

                        return false;
                    }

                    if (house.Balance < (ulong)(Settings.MIN_PAID_HOURS_HOUSE_APS * house.Tax))
                    {
                        player.Notify(house.Type == Game.Estates.HouseBase.Types.House ? "Trade::MHPH" : "Trade::MAPH", pId, Settings.MIN_PAID_HOURS_HOUSE_APS);

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
            else if (pType == PlayerData.PropertyTypes.Garage)
            {
                var garage = pData.OwnedGarages.Where(x => x.Id == pId).FirstOrDefault();

                if (garage == null)
                    return false;

                var tradeGarages = isSender ? offer.TradeData.SenderGarages : offer.TradeData.ReceiverGarages;

                if (state == tradeGarages.Contains(garage))
                    return false;

                if (state)
                {
                    if (tradeGarages.Count >= Settings.MAX_GARAGES_IN_TRADE)
                    {
                        player.Notify("Trade::MGIT", Settings.MAX_GARAGES_IN_TRADE);

                        return false;
                    }

                    if (garage.Balance < (ulong)(Settings.MIN_PAID_HOURS_GARAGE * garage.Tax))
                    {
                        player.Notify("Trade::MGPH", pId, Settings.MIN_PAID_HOURS_GARAGE);

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
            else if (pType == PlayerData.PropertyTypes.Business)
            {
                var biz = pData.OwnedBusinesses.Where(x => x.ID == pId).FirstOrDefault();

                if (biz == null)
                    return false;

                var tradeBusinesses = isSender ? offer.TradeData.SenderBusinesses : offer.TradeData.ReceiverBusinesses;

                if (state == tradeBusinesses.Contains(biz))
                    return false;

                if (state)
                {
                    if (tradeBusinesses.Count >= Settings.MAX_BUSINESS_IN_TRADE)
                    {
                        player.Notify("Trade::MBIT", Settings.MAX_BUSINESS_IN_TRADE);

                        return false;
                    }

                    if (biz.Bank < (ulong)(Settings.MIN_PAID_HOURS_BUSINESS * biz.Rent))
                    {
                        player.Notify("Trade::MBPH", pId, Settings.MIN_PAID_HOURS_BUSINESS);

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
                return false;

            return true;
        }

        [RemoteEvent("Trade::UpdateItem")]
        private static void UpdateItem(Player player, bool fromPockets, int slotTo, int slotFrom, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var offer = pData.ActiveOffer;

            if (offer == null || offer.Type != Types.Exchange || offer.TradeData == null)
                return;

            if (offer.TradeData.SenderReady || offer.TradeData.ReceiverReady)
                return;

            var isSender = offer.Sender == pData;

            var tData = isSender ? offer.Receiver : offer.Sender;

            if (fromPockets)
            {
                if (slotTo < 0 || slotTo >= offer.TradeData.SenderItems.Length)
                    return;

                if (slotFrom < 0 || slotFrom >= pData.Items.Length)
                    return;

                var item = pData.Items[slotFrom];

                if (item == null)
                    return;

                if (item.IsTemp)
                    return;

                if (item is Game.Items.IStackable stackable)
                {
                    if (amount <= 0 || amount > stackable.Amount)
                        amount = stackable.Amount;
                }
                else
                    amount = 1;

                if (isSender)
                {
                    for (int i = 0; i < offer.TradeData.SenderItems.Length; i++)
                    {
                        if (i == slotTo)
                            continue;

                        if (offer.TradeData.SenderItems[i]?.ItemRoot == item)
                            slotTo = i;
                    }
                }
                else
                {
                    for (int i = 0; i < offer.TradeData.ReceiverItems.Length; i++)
                    {
                        if (i == slotTo)
                            continue;

                        if (offer.TradeData.ReceiverItems[i]?.ItemRoot == item)
                            slotTo = i;
                    }
                }

                var iData = isSender ? offer.TradeData.SenderItems[slotTo] : offer.TradeData.ReceiverItems[slotTo];

                if (iData != null)
                {
                    if (iData.ItemRoot == item)
                    {
                        if (item is Game.Items.IStackable stackableR)
                        {
                            var expectedAmount = stackableR.Amount - iData.Amount;

                            if (amount >= expectedAmount)
                                amount = expectedAmount;
                        }
                        else
                            return;
                    }
                    else
                    {
                        iData.ItemRoot = item;
                        iData.Amount = amount;
                    }
                }
                else
                {
                    iData = new Offer.Trade.TradeItem(item, amount);

                    if (isSender)
                    {
                        offer.TradeData.SenderItems[slotTo] = iData;
                    }
                    else
                    {
                        offer.TradeData.ReceiverItems[slotTo] = iData;
                    }
                }

                var upd2 = iData.ToClientJson();

                pData.Player.TriggerEvent("Inventory::Update", 10, slotFrom, slotTo, upd2);
                tData.Player.TriggerEvent("Inventory::Update", 12, slotTo, upd2);
            }
            else
            {
                if (slotFrom < 0 || slotFrom >= offer.TradeData.SenderItems.Length)
                    return;

                var iData = isSender ? offer.TradeData.SenderItems[slotFrom] : offer.TradeData.ReceiverItems[slotFrom];

                if (iData == null || iData.ItemRoot == null)
                    return;

                if (amount <= 0 || amount > iData.Amount)
                    amount = iData.Amount;

                if (amount == iData.Amount)
                {
                    iData = null;

                    if (isSender)
                    {
                        offer.TradeData.SenderItems[slotFrom] = iData;
                    }
                    else
                    {
                        offer.TradeData.ReceiverItems[slotFrom] = iData;
                    }

                    pData.Player.TriggerEvent("Inventory::Update", 10, -1, slotFrom, Game.Items.Item.ToClientJson(null, Game.Items.Inventory.Groups.Items));
                    tData.Player.TriggerEvent("Inventory::Update", 12, slotFrom, Game.Items.Item.ToClientJson(null, Game.Items.Inventory.Groups.Items));
                }
                else
                {
                    iData.Amount -= amount;

                    var upd2 = iData.ToClientJson();

                    pData.Player.TriggerEvent("Inventory::Update", 10, -1, slotFrom, upd2);
                    tData.Player.TriggerEvent("Inventory::Update", 12, slotFrom, upd2);
                }
            }
        }
    }
}
