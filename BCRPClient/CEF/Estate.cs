using RAGE;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.CEF
{
    class Estate : Events.Script
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.Estate);

        public static Types? CurrentType { get; set; }

        public enum Types
        {
            Info = 0,
            Offer,
            Sell,
        }

        public Estate()
        {
            Events.Add("Estate::Action", (object[] args) =>
            {
                var id = (string)args[0]; // enter/mail/buy/cancel/accept/decline
            });
        }

        private static async System.Threading.Tasks.Task<bool> Show(bool showCursor = true)
        {
            if (IsActive || CurrentType != null)
                return false;

            if (Utils.IsAnyCefActive(true))
                return false;

            await CEF.Browser.Render(Browser.IntTypes.Estate, true, true);

            if (showCursor)
                CEF.Cursor.Show(true, true);

            return true;
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            CEF.Browser.Render(Browser.IntTypes.Estate, false, false);

            CEF.Cursor.Show(false, false);
        }
    }
}
