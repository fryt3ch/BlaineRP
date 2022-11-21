using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BCRPClient.CEF
{
    class ATM : Events.Script
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.ATM);

        public static Data.Locations.ATM CurrentATM => Player.LocalPlayer.HasData("CurrentATM") ? Player.LocalPlayer.GetData<Data.Locations.ATM>("CurrentATM") : null;

        private static DateTime LastSent;

        private static List<int> TempBinds { get; set; }

        public ATM()
        {
            TempBinds = new List<int>();

            LastSent = DateTime.Now;

            Events.Add("ATM::Action", (object[] args) =>
            {
                string id = (string)args[0];

                int amount = (int)args[1];

                if (LastSent.IsSpam(500, false, false))
                    return;

                if (id == "deposit")
                {

                }
                else if (id == "withdraw")
                {

                }

                LastSent = DateTime.Now;
            });
        }

        public static async System.Threading.Tasks.Task Show(float fee)
        {
            if (IsActive)
                return;

            if (Utils.IsAnyCefActive(true))
                return;

            var data = Sync.Players.GetData(Player.LocalPlayer);

            if (data == null)
                return;

            var curAtm = CurrentATM;

            if (curAtm == null)
                return;

            await CEF.Browser.Render(Browser.IntTypes.ATM, true, true);

            CEF.Browser.Window.ExecuteJs("ATM.draw", new object[] { new object[] { data.BankBalance, fee } });

            CEF.Cursor.Show(true, true);

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false)));
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            if (!ignoreTimeout && LastSent.IsSpam(500, false, false))
                return;

            CEF.Browser.Render(Browser.IntTypes.ATM, false, false);

            CEF.Cursor.Show(false, false);

            foreach (var x in TempBinds)
                RAGE.Input.Unbind(x);

            TempBinds.Clear();
        }

        public static void UpdateMoney(int value)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("ATM.updateMoney", value);
        }
    }
}
