using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;

namespace BCRPClient.CEF
{
    class ATM : Events.Script
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.ATM);

        private static DateTime LastSent;

        private static List<int> TempBinds { get; set; }

        public ATM()
        {
            TempBinds = new List<int>();

            LastSent = Sync.World.ServerTime;

            Events.Add("ATM::Action", (object[] args) =>
            {
                if (!IsActive)
                    return;

                string id = (string)args[0];

                int amount = (int)args[1];

                if (amount <= 0)
                    return;

                if (LastSent.IsSpam(1000, false, false))
                    return;

                Events.CallRemote("Bank::Debit::Operation", true, Player.LocalPlayer.GetData<int>("CurrentATM::Id"), id == "deposit", amount);

                LastSent = Sync.World.ServerTime;
            });

            Events.Add("ATM::Show", (object[] args) =>
            {
                Sync.Players.CloseAll(true);

                Player.LocalPlayer.SetData("CurrentATM::Id", (int)args[0]);

                Show(args[1].ToDecimal());
            });
        }

        public static async System.Threading.Tasks.Task Show(decimal fee)
        {
            if (IsActive)
                return;

            if (Utils.IsAnyCefActive(true))
                return;

            var data = Sync.Players.GetData(Player.LocalPlayer);

            if (data == null)
                return;

            await CEF.Browser.Render(Browser.IntTypes.ATM, true, true);

            CEF.Browser.Window.ExecuteJs("ATM.draw", new object[] { new object[] { data.BankBalance, fee * 100 } });

            CEF.Cursor.Show(true, true);

            TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false)));
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
                KeyBinds.Unbind(x);

            TempBinds.Clear();

            Player.LocalPlayer.ResetData("CurrentATM::Id");
        }

        public static void UpdateMoney(ulong value)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("ATM.updateBalance", value);
        }
    }
}
