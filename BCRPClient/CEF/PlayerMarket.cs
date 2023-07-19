using RAGE;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.CEF
{
    [Script(int.MaxValue)]
    public class PlayerMarket 
    {
        public static bool IsActive => CurrentContext != null && CEF.Browser.IsActive(Browser.IntTypes.Retail);

        public static string CurrentContext { get; set; }

        private static int EscBindIdx { get; set; } = -1;

        public PlayerMarket()
        {
            Events.Add("Shop::Confirm", async (args) =>
            {
                if (!IsActive)
                    return;

                var context = CurrentContext;

                var d = context.Split('_');

                if (d[0] == "MARKETSTALL@SELLER")
                {
                    var stallIdx = int.Parse(d[1]);

                    var itemsJson = (string)args[0];

                    if (itemsJson == null || itemsJson.Length == 0)
                        return;

                    var items1 = RAGE.Util.Json.Deserialize<List<List<object>>>(itemsJson);

                    var items = new List<string>();

                    for (int i = 0; i < items1.Count; i++)
                    {
                        var x = items1[i];

                        var idx = Utils.ToInt32(x[0]);

                        var amount = Utils.ToInt32(x[1]);
                        var priceD = Utils.ToDecimal(x[2]);

                        int price;

                        if (!priceD.IsNumberValid<int>(1, int.MaxValue, out price, false))
                        {
                            var y = (object[])CEF.Inventory.ItemsData[idx][0];

                            CEF.Notification.ShowError(Locale.Get("MARKETSTALL_MG_ITEMCH_0", y[1], 1, int.MaxValue));
    
                            return;
                        }

                        items.Add($"{idx}_{amount}_{price}");
                    }

                    if (CEF.Shop.LastSent.IsSpam(1500, false, true))
                        return;

                    CEF.Shop.LastSent = Sync.World.ServerTime;

                    var res = Utils.ToByte(await Events.CallRemoteProc("MarketStall::SI", stallIdx, RAGE.Util.Json.Serialize(items)));

                    if (res == 255)
                    {
                        Close();

                        CEF.Notification.ShowError(Locale.Get("MARKETSTALL_MG_ITEMCH_1"));

                        return;
                    }
                    else if (res == 1)
                    {
                        CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Get("MARKETSTALL_MG_ITEMCH_3"));
                    }
                }
            });

            Events.Add("Shop::Try", async (args) =>
            {
                if (!IsActive)
                    return;

                var context = CurrentContext;

                var d = context.Split('_');

                if (d[0] == "MARKETSTALL@BUYER")
                {
                    CEF.Notification.ShowError(Locale.Get("MARKETSTALL_TRY_ERROR_0"), -1);

                    return;

                    if (CEF.Shop.LastSent.IsSpam(1000, false, true))
                        return;

                    CEF.Shop.LastSent = Sync.World.ServerTime;

                    var uid = Utils.ToUInt32(args[0]);

                    var res = ((string)await Events.CallRemoteProc("MarketStall::Try", uid))?.Split('&');

                    if (res == null)
                        return;

                    var variation = int.Parse(res[0]);

                    //Data.Clothes.Wear();
                }
            });

            Events.Add("Shop::Buy", async (args) =>
            {
                if (!IsActive)
                    return;

                var context = CurrentContext;

                var d = context.Split('_');

                if (d[0] == "MARKETSTALL@BUYER")
                {
                    if (CEF.Shop.LastSent.IsSpam(250, false, true))
                        return;

                    CEF.Shop.LastSent = Sync.World.ServerTime;

                    var stallIdx = int.Parse(d[1]);

                    var useCash = (bool)args[0];

                    var itemUid = Utils.ToUInt32(args[1]);

                    var amount = Utils.ToInt32(args[2]);

                    var res = await Events.CallRemoteProc("MarketStall::Buy", stallIdx, itemUid, amount, useCash);
                }
            });
        }

        public static async void Show(string context, object[] addData)
        {
            if (IsActive)
                return;

            await CEF.Browser.Render(Browser.IntTypes.Retail, true, true);

            CurrentContext = context;

            var d = context.Split('_');

            if (d[0] == "MARKETSTALL@SELLER")
            {
                CEF.Browser.Window.ExecuteJs("Retail.draw", "market", new object[] { addData[0] }, null, true);

                CEF.Inventory.InventoryUpdated -= OnInventoryUpdatedMarketStallSeller;
                CEF.Inventory.InventoryUpdated += OnInventoryUpdatedMarketStallSeller;

                GameEvents.Render -= RenderMarketStallSeller;
                GameEvents.Render += RenderMarketStallSeller;

                EscBindIdx = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
            }
            else if (d[0] == "MARKETSTALL@BUYER")
            {
                CEF.Browser.Window.ExecuteJs("Retail.draw", "market", new object[] { addData[0] }, addData[1], false);

                EscBindIdx = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
            }

            CEF.Cursor.Show(true, true);
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            KeyBinds.Unbind(EscBindIdx);

            EscBindIdx = -1;

            CurrentContext = null;

            GameEvents.Render -= RenderMarketStallSeller;

            CEF.Inventory.InventoryUpdated -= OnInventoryUpdatedMarketStallSeller;

            CEF.Browser.Render(Browser.IntTypes.Retail, false);

            CEF.Cursor.Show(false, false);
        }

        private static void OnInventoryUpdatedMarketStallSeller(int typeId)
        {
            Close();

            CEF.Notification.ShowError(Locale.Get("MARKETSTALL_MG_ITEMCH_2"));
        }

        private static void RenderMarketStallSeller()
        {
            var text = Locale.Get("MARKETSTALL_MG_ITEMCH_H_0");

            Utils.DrawText(text, 0.5f, 0.920f, 255, 255, 255, 255, 0.5f, RAGE.Game.Font.ChaletComprimeCologne, true, true);

            text = Locale.Get("MARKETSTALL_MG_ITEMCH_H_1");

            Utils.DrawText(text, 0.5f, 0.950f, 255, 255, 255, 255, 0.5f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
        }
    }
}
