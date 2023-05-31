using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.CEF
{
    public class PlayerMarket
    {
        public static bool IsActive => CurrentContext != null && CEF.Browser.IsActive(Browser.IntTypes.Retail);

        public static string CurrentContext { get; set; }

        public static async void Show(string context)
        {
            if (IsActive)
                return;

            await CEF.Browser.Render(Browser.IntTypes.Retail, true, true);

            CurrentContext = context;

            var d = context.Split('_');

            if (d[0] == "MARKETSTALL")
            {
                var items = new List<object>();

                for (int i = 0; i < CEF.Inventory.ItemsParams.Length; i++)
                {
                    var x = CEF.Inventory.ItemsParams[i];

                    if (x == null)
                        continue;

                    var y = (object[])CEF.Inventory.ItemsData[i][0];

                    items.Add(new object[] { new object[] { i, y[0] }, y[1], 0, y[3], y[4], false } );
                }

                CEF.Browser.Window.ExecuteJs("Retail.draw", "market", items, null, true);
            }
            else
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
