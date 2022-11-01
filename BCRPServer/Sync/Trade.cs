using GTANetworkAPI;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;

namespace BCRPServer.Sync
{
    class Trade : Script
    {
        [RemoteEvent("Trade::Accept")]
        public static async Task Accept(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Offers.Semaphore.WaitAsync();

            await Task.Run(async () =>
            {
                var offer = Offers.Offer.Get(pData);

                if (offer == null || offer.Type != Offers.Types.Exchange || offer.TradeData == null)
                    return;

                bool isSender = offer.Sender == pData;

                var tData = isSender ? offer.Receiver : offer.Sender;

                if (!await tData.WaitAsync(1000))
                    return;

                if (isSender)
                {
                    if (!offer.TradeData.SenderReady)
                    {
                        tData.Release();

                        return;
                    }

                    if (!offer.TradeData.ReceiverReady)
                    {
                        await NAPI.Task.RunAsync(() =>
                        {
                            if (pData.Player?.Exists != true)
                                return;

                            pData.Player.Notify("Trade::PlayerNeedConfirm");
                        });

                        tData.Release();

                        return;
                    }
                }
                else
                {
                    if (!offer.TradeData.ReceiverReady)
                    {
                        tData.Release();

                        return;
                    }

                    if (!offer.TradeData.SenderReady)
                    {
                        await NAPI.Task.RunAsync(() =>
                        {
                            if (pData.Player?.Exists != true)
                                return;

                            pData.Player.Notify("Trade::PlayerNeedConfirm");
                        });

                        tData.Release();

                        return;
                    }
                }

                var result = await offer.TradeData.Execute(offer.Sender, offer.Receiver);

                if (result.Result == CEF.Inventory.Results.Success)
                {
                    await offer.Cancel(true, false, false);

                    await NAPI.Task.RunAsync(() =>
                    {
                        if (pData.Player?.Exists == true)
                        {
                            pData.Player.Notify("Trade::Success");
                            pData.Player.CloseAll();
                        }

                        if (tData.Player?.Exists ==  true)
                        {
                            tData.Player.Notify("Trade::Success");
                            tData.Player.CloseAll();
                        }
                    });
                }
                else
                {
                    if (result.Result == CEF.Inventory.Results.Error)
                    {
                        await offer.Cancel(false, false, false);

                        await NAPI.Task.RunAsync(() =>
                        {
                            if (pData.Player?.Exists == true)
                            {
                                pData.Player.Notify("Trade::Error");
                                pData.Player.CloseAll();
                            }

                            if (tData.Player?.Exists == true)
                            {
                                tData.Player.Notify("Trade::Error");
                                tData.Player.CloseAll();
                            }
                        });
                    }
                    else if (result.Result == CEF.Inventory.Results.NotEnoughMoney)
                    {
                        await NAPI.Task.RunAsync(() =>
                        {
                            if (pData.Player?.Exists == true)
                            {
                                pData.Player.Notify(pData == result.PlayerError ? "Trade::NotEnoughMoney" : "Trade::NotEnoughMoneyOther");
                            }

                            if (tData.Player?.Exists == true)
                            {
                                tData.Player.Notify(tData == result.PlayerError ? "Trade::NotEnoughMoney" : "Trade::NotEnoughMoneyOther");
                            }
                        });
                    }
                    else if (result.Result == CEF.Inventory.Results.NoSpace)
                    {
                        await NAPI.Task.RunAsync(() =>
                        {
                            if (pData.Player?.Exists == true)
                            {
                                pData.Player.Notify(pData == result.PlayerError ? "Inventory::NoSpace" : "Trade::NotEnoughSpaceOther");
                            }

                            if (tData.Player?.Exists == true)
                            {
                                tData.Player.Notify(tData == result.PlayerError ? "Inventory::NoSpace" : "Trade::NotEnoughSpaceOther");
                            }
                        });
                    }
                }

                tData.Release();
            });

            Offers.Semaphore.Release();

            pData.Release();
        }

        [RemoteEvent("Trade::Confirm")]
        public static async Task Confirm(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Offers.Semaphore.WaitAsync();

            await Task.Run(async () =>
            {
                var offer = Offers.Offer.Get(pData);

                if (offer == null || offer.Type != Offers.Types.Exchange || offer.TradeData == null)
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

                await NAPI.Task.RunAsync(() =>
                {
                    if (tData.Player?.Exists != true || pData.Player?.Exists != true)
                        return;

                    pData.Player.TriggerEvent("Inventory::Update", 14, true, state, otherState);

                    tData.Player.TriggerEvent("Inventory::Update", 14, false, state, otherState);
                });
            });

            Offers.Semaphore.Release();

            pData.Release();
        }

        [RemoteEvent("Trade::UpdateMoney")]
        public static async Task UpdateMoney(Player player, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Offers.Semaphore.WaitAsync();

            await Task.Run(async () =>
            {
                var offer = Offers.Offer.Get(pData);

                if (offer == null || offer.Type != Offers.Types.Exchange || offer.TradeData == null)
                    return;

                if (offer.TradeData.SenderReady || offer.TradeData.ReceiverReady)
                    return;

                bool isSender = offer.Sender == pData;

                var tData = isSender ? offer.Receiver : offer.Sender;

                amount = await NAPI.Task.RunAsync(() =>
                {
                    if (tData.Player?.Exists != true || pData.Player?.Exists != true)
                        return -1;

                    if (pData.Cash < amount)
                        amount = pData.Cash;

                    pData.Player.TriggerEvent("Inventory::Update", 11, true, amount);
                    tData.Player.TriggerEvent("Inventory::Update", 13, true, amount);

                    return amount;
                });

                if (amount == -1)
                    return;

                if (isSender)
                    offer.TradeData.SenderMoney = amount;
                else
                    offer.TradeData.ReceiverMoney = amount;
            });

            Offers.Semaphore.Release();

            pData.Release();
        }

        [RemoteEvent("Trade::UpdateItem")]
        public static async Task UpdateItem(Player player, bool fromPockets, int slotTo, int slotFrom, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Offers.Semaphore.WaitAsync();

            await Task.Run(async () =>
            {
                var offer = Offers.Offer.Get(pData);

                if (offer == null || offer.Type != Offers.Types.Exchange || offer.TradeData == null)
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

                    if (item is Game.Items.IStackable)
                    {
                        if (amount <= 0 || amount > (item as Game.Items.IStackable).Amount)
                            amount = (item as Game.Items.IStackable).Amount;
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
                            if (item is Game.Items.IStackable)
                            {
                                var expectedAmount = (item as Game.Items.IStackable).Amount - iData.Amount;

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
                        iData = new Offers.Offer.Trade.TradeItem(item, amount);

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

                    NAPI.Task.Run(() =>
                    {
                        if (tData.Player?.Exists != true || pData.Player?.Exists != true)
                            return;

                        pData.Player.TriggerEvent("Inventory::Update", 10, slotFrom, slotTo, upd2);
                        tData.Player.TriggerEvent("Inventory::Update", 12, slotTo, upd2);
                    });
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

                        NAPI.Task.Run(() =>
                        {
                            if (tData.Player?.Exists != true || pData.Player?.Exists != true)
                                return;

                            pData.Player.TriggerEvent("Inventory::Update", 10, -1, slotFrom, "null");
                            tData.Player.TriggerEvent("Inventory::Update", 12, slotFrom, "null");
                        });
                    }
                    else
                    {
                        iData.Amount -= amount;

                        var upd2 = iData.ToClientJson();

                        NAPI.Task.Run(() =>
                        {
                            if (tData.Player?.Exists != true || pData.Player?.Exists != true)
                                return;

                            pData.Player.TriggerEvent("Inventory::Update", 10, -1, slotFrom, upd2);
                            tData.Player.TriggerEvent("Inventory::Update", 12, slotFrom, upd2);
                        });
                    }
                }
            });

            Offers.Semaphore.Release();

            pData.Release();
        }
    }
}
