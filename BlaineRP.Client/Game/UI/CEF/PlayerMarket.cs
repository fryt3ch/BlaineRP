using System.Collections.Generic;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.World;
using BlaineRP.Client.Utils.Game;
using RAGE;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class PlayerMarket
    {
        public PlayerMarket()
        {
            Events.Add("Shop::Confirm",
                async (args) =>
                {
                    if (!IsActive)
                        return;

                    string context = CurrentContext;

                    string[] d = context.Split('_');

                    if (d[0] == "MARKETSTALL@SELLER")
                    {
                        var stallIdx = int.Parse(d[1]);

                        var itemsJson = (string)args[0];

                        if (itemsJson == null || itemsJson.Length == 0)
                            return;

                        List<List<object>> items1 = RAGE.Util.Json.Deserialize<List<List<object>>>(itemsJson);

                        var items = new List<string>();

                        for (var i = 0; i < items1.Count; i++)
                        {
                            List<object> x = items1[i];

                            var idx = Utils.Convert.ToInt32(x[0]);

                            var amount = Utils.Convert.ToInt32(x[1]);
                            var priceD = Utils.Convert.ToDecimal(x[2]);

                            int price;

                            if (!priceD.IsNumberValid<int>(1, int.MaxValue, out price, false))
                            {
                                var y = (object[])Inventory.ItemsData[idx][0];

                                Notification.ShowError(Locale.Get("MARKETSTALL_MG_ITEMCH_0", y[1], 1, int.MaxValue));

                                return;
                            }

                            items.Add($"{idx}_{amount}_{price}");
                        }

                        if (Shop.LastSent.IsSpam(1500, false, true))
                            return;

                        Shop.LastSent = Core.ServerTime;

                        var res = Utils.Convert.ToByte(await Events.CallRemoteProc("MarketStall::SI", stallIdx, RAGE.Util.Json.Serialize(items)));

                        if (res == 255)
                        {
                            Close();

                            Notification.ShowError(Locale.Get("MARKETSTALL_MG_ITEMCH_1"));

                            return;
                        }
                        else if (res == 1)
                        {
                            Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Get("MARKETSTALL_MG_ITEMCH_3"));
                        }
                    }
                }
            );

            Events.Add("Shop::Try",
                async (args) =>
                {
                    if (!IsActive)
                        return;

                    string context = CurrentContext;

                    string[] d = context.Split('_');

                    if (d[0] == "MARKETSTALL@BUYER")
                    {
                        Notification.ShowError(Locale.Get("MARKETSTALL_TRY_ERROR_0"), -1);

                        return;

                        if (Shop.LastSent.IsSpam(1000, false, true))
                            return;

                        Shop.LastSent = Core.ServerTime;

                        var uid = Utils.Convert.ToUInt32(args[0]);

                        string[] res = ((string)await Events.CallRemoteProc("MarketStall::Try", uid))?.Split('&');

                        if (res == null)
                            return;

                        var variation = int.Parse(res[0]);

                        //Data.Clothes.Wear();
                    }
                }
            );

            Events.Add("Shop::Buy",
                async (args) =>
                {
                    if (!IsActive)
                        return;

                    string context = CurrentContext;

                    string[] d = context.Split('_');

                    if (d[0] == "MARKETSTALL@BUYER")
                    {
                        if (Shop.LastSent.IsSpam(250, false, true))
                            return;

                        Shop.LastSent = Core.ServerTime;

                        var stallIdx = int.Parse(d[1]);

                        var useCash = (bool)args[0];

                        var itemUid = Utils.Convert.ToUInt32(args[1]);

                        var amount = Utils.Convert.ToInt32(args[2]);

                        object res = await Events.CallRemoteProc("MarketStall::Buy", stallIdx, itemUid, amount, useCash);
                    }
                }
            );
        }

        public static bool IsActive => CurrentContext != null && Browser.IsActive(Browser.IntTypes.Retail);

        public static string CurrentContext { get; set; }

        private static int EscBindIdx { get; set; } = -1;

        public static async void Show(string context, object[] addData)
        {
            if (IsActive)
                return;

            await Browser.Render(Browser.IntTypes.Retail, true, true);

            CurrentContext = context;

            string[] d = context.Split('_');

            if (d[0] == "MARKETSTALL@SELLER")
            {
                Browser.Window.ExecuteJs("Retail.draw",
                    "market",
                    new object[]
                    {
                        addData[0],
                    },
                    null,
                    true
                );

                Inventory.InventoryUpdated -= OnInventoryUpdatedMarketStallSeller;
                Inventory.InventoryUpdated += OnInventoryUpdatedMarketStallSeller;

                Main.Render -= RenderMarketStallSeller;
                Main.Render += RenderMarketStallSeller;

                EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
            }
            else if (d[0] == "MARKETSTALL@BUYER")
            {
                Browser.Window.ExecuteJs("Retail.draw",
                    "market",
                    new object[]
                    {
                        addData[0],
                    },
                    addData[1],
                    false
                );

                EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
            }

            Cursor.Show(true, true);
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            Input.Core.Unbind(EscBindIdx);

            EscBindIdx = -1;

            CurrentContext = null;

            Main.Render -= RenderMarketStallSeller;

            Inventory.InventoryUpdated -= OnInventoryUpdatedMarketStallSeller;

            Browser.Render(Browser.IntTypes.Retail, false);

            Cursor.Show(false, false);
        }

        private static void OnInventoryUpdatedMarketStallSeller(int typeId)
        {
            Close();

            Notification.ShowError(Locale.Get("MARKETSTALL_MG_ITEMCH_2"));
        }

        private static void RenderMarketStallSeller()
        {
            string text = Locale.Get("MARKETSTALL_MG_ITEMCH_H_0");

            Graphics.DrawText(text, 0.5f, 0.920f, 255, 255, 255, 255, 0.5f, RAGE.Game.Font.ChaletComprimeCologne, true, true);

            text = Locale.Get("MARKETSTALL_MG_ITEMCH_H_1");

            Graphics.DrawText(text, 0.5f, 0.950f, 255, 255, 255, 255, 0.5f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
        }
    }
}