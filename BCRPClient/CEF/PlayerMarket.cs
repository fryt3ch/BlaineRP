using RAGE;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.CEF
{
    public class PlayerMarket : Events.Script
    {
        public static bool IsActive => CurrentContext != null && CEF.Browser.IsActive(Browser.IntTypes.Retail);

        public static string CurrentContext { get; set; }

        public PlayerMarket()
        {
            Events.Add("Shop::Confirm", (args) =>
            {
                if (!IsActive)
                    return;

                var context = CurrentContext;

                var d = context.Split('_');

                if (d[0] == "MARKETSTALL@SELLER")
                {
                    var itemsJson = (string)args[0];

                    if (itemsJson == null || itemsJson.Length == 0)
                        return;

                    var items1 = RAGE.Util.Json.Deserialize<List<List<object>>>(itemsJson);

                    var items = new List<(uint, int, int)>();

                    for (int i = 0; i < items1.Count; i++)
                    {
                        var x = items1[i];

                        var uid = uint.Parse(x[0].ToString());

                        var amountD = decimal.Parse(x[1].ToString());
                        var priceD = decimal.Parse(x[2].ToString());

                        int price;

                        if (!priceD.IsNumberValid<int>(1, int.MaxValue, out price, false))
                        {
                            CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Get("NOTIFICATION_HEADER_ERROR"), Locale.Get("MARKETSTALL_MG_ITEMCH_0", 1, int.MaxValue));
    
                        return;
                        }
                    }
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
            }
            else if (d[0] == "MARKETSTALL@BUYER")
            {

            }
        }

        public static void Close()
        {
            if (!IsActive)
                return;
        }
    }
}
