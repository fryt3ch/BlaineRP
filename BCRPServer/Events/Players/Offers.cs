using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var tData = target.GetMainData();

            if (tData?.Player?.Exists != true)
                return;

            object dataObj = null;

            var oType = (Types)type;

            ReturnTypes res = ((Func<ReturnTypes>)(() =>
            {
                if (!pData.Player.AreEntitiesNearby(tData.Player, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                    return ReturnTypes.Error;

                if (pData.IsBusy)
                    return ReturnTypes.SourceBusy;

                if (tData.IsBusy)
                    return ReturnTypes.TargetBusy;

                if (pData.ActiveOffer != null)
                    return ReturnTypes.SourceHasOffer;

                if (tData.ActiveOffer != null)
                    return ReturnTypes.TargetHasOffer;

                try
                {
                    dataObj = data.DeserializeFromJson<object>();

                    if (oType == Types.Cash)
                    {
                        var cash = Convert.ToInt32(dataObj);

                        dataObj = cash;

                        if (cash < 0 || cash == 0)
                            return ReturnTypes.Error;

                        if (pData.Cash < cash)
                            return ReturnTypes.NotEnoughMoneySource;
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

                        dataObj = new Offer.PropertySellData(vInfo, price);
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

                        dataObj = new Offer.PropertySellData(bInfo, price);
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
                    if (oType == Types.Cash)
                    {
                        target.TriggerEvent("Offer::Show", player.Handle, type, data);
                    }
                    else
                    {
                        target.TriggerEvent("Offer::Show", player.Handle, type);
                    }

                    player.TriggerEvent("Offer::Reply::Server", true, false, false);
                    player.Notify("Offer::Sent");
                    break;

                case ReturnTypes.SourceBusy:
                    player.TriggerEvent("Offer::Reply::Server", false, false, true);
                    player.Notify("Player::Busy");
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

                case ReturnTypes.NotEnoughMoneySource:
                    player.TriggerEvent("Offer::Reply::Server", false, false, true);
                    player.Notify("Trade::NotEnoughMoney");
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
                pData.Player.CloseAll();

                tData.Player.Notify("Trade::Success");
                tData.Player.CloseAll();
            }
            else
            {
                if (result.Result == Game.Items.Inventory.Results.Error)
                {
                    offer.Cancel(false, false, Sync.Offers.ReplyTypes.AutoCancel, false);

                    pData.Player.Notify("Trade::Error");
                    pData.Player.CloseAll();

                    tData.Player.Notify("Trade::Error");
                    tData.Player.CloseAll();
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

        [RemoteEvent("Trade::UpdateMoney")]
        private static void UpdateMoney(Player player, int amount)
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

            bool isSender = offer.Sender == pData;

            var tData = isSender ? offer.Receiver : offer.Sender;

            if (pData.Cash < amount)
                amount = pData.Cash;

            if (isSender)
            {
                if (offer.TradeData.SenderMoney == amount)
                    return;

                offer.TradeData.SenderMoney = amount;
            }
            else
            {
                if (offer.TradeData.ReceiverMoney == amount)
                    return;

                offer.TradeData.ReceiverMoney = amount;
            }

            pData.Player.TriggerEvent("Inventory::Update", 11, true, amount);
            tData.Player.TriggerEvent("Inventory::Update", 13, true, amount);
        }

        [RemoteProc("Trade::UpdateProperty")]
        private static bool UpdateProperty(Player player, int idx, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer || idx < 0)
                return false;

            var pData = sRes.Data;

            var offer = pData.ActiveOffer;

            if (offer == null || offer.Type != Types.Exchange || offer.TradeData == null)
                return false;

            if (offer.TradeData.SenderReady || offer.TradeData.ReceiverReady)
                return false;

            bool isSender = offer.Sender == pData;

            var tData = isSender ? offer.Receiver : offer.Sender;

            string data = null;

            if (idx < pData.OwnedVehicles.Count)
            {
                var veh = pData.OwnedVehicles[idx];

                if (isSender)
                {
                    if (state == offer.TradeData.SenderVehicles.Contains(veh))
                        return false;

                    if (state)
                    {
                        offer.TradeData.SenderVehicles.Add(veh);
                    }
                    else
                    {
                        offer.TradeData.SenderVehicles.Remove(veh);
                    }
                }
                else
                {
                    if (state == offer.TradeData.ReceiverVehicles.Contains(veh))
                        return false;

                    if (state)
                    {
                        offer.TradeData.ReceiverVehicles.Add(veh);
                    }
                    else
                    {
                        offer.TradeData.ReceiverVehicles.Remove(veh);
                    }
                }

                data = $"{veh.Data.Name} | #{veh.VID}";
            }
            else
            {
                return false;
            }

            pData.Player.TriggerEvent("Inventory::Update", 11, false, state, data);
            tData.Player.TriggerEvent("Inventory::Update", 13, false, state, data);

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

            bool isSender = offer.Sender == pData;

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

                    pData.Player.TriggerEvent("Inventory::Update", 10, -1, slotFrom, "null");
                    tData.Player.TriggerEvent("Inventory::Update", 12, slotFrom, "null");
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
