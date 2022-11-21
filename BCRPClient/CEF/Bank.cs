using RAGE;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.CEF
{
    class Bank : Events.Script
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.MenuBank);

        public Bank()
        {
            Events.Add("MenuBank::Close", (object[] args) =>
            {
                Close(false);
            });

            Events.Add("MenuBank::Cash2Debet", (object[] args) =>
            {
                bool state = (bool)args[0];
            });

            Events.Add("MenuBank::Action", (object[] args) =>
            {
                var id = (string)args[0]; // savings or debit

                var aId = (string)args[1]; // deposit/withdraw/transfer

                var amount = (int)args[2];

                if (aId == "transfer")
                {
                    var cid = (int)args[3];
                }
                else
                {

                }
            });

            Events.Add("MenuBank::BuyTariff", (object[] args) =>
            {

            });
        }

        public static async System.Threading.Tasks.Task Show()
        {
            if (IsActive)
                return;

            if (Utils.IsAnyCefActive(true))
                return;

            await CEF.Browser.Render(Browser.IntTypes.MenuBank, true, true);

            CEF.Browser.Window.ExecuteJs("todo");

            CEF.Cursor.Show(true, true);
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            CEF.Browser.Render(Browser.IntTypes.MenuBank, false, false);

            CEF.Cursor.Show(false, false);
        }
    }
}
