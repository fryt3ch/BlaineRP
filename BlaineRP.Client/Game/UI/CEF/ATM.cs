using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Input;
using BlaineRP.Client.Sync;
using Core = BlaineRP.Client.Input.Core;

namespace BlaineRP.Client.CEF
{
    [Script(int.MaxValue)]
    public class ATM
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.ATM);

        private static DateTime LastSent;

        private static List<int> TempBinds { get; set; }

        private static Additional.ExtraColshape CloseColshape { get; set; }

        public ATM()
        {
            TempBinds = new List<int>();

            LastSent = Sync.World.ServerTime;

            Events.Add("ATM::Action", (object[] args) =>
            {
                if (!IsActive)
                    return;

                if (args == null || args.Length != 2 || args[1] == null)
                    return;

                var id = (string)args[0];

                if (id == null)
                    return;

                int amount;

                if (!Utils.Convert.ToDecimal(args[1]).IsNumberValid(1, int.MaxValue, out amount, true))
                    return;

                if (LastSent.IsSpam(1000, false, false))
                    return;

                Events.CallRemote("Bank::Debit::Operation", true, Player.LocalPlayer.GetData<int>("CurrentATM::Id"), id == "deposit", amount);

                LastSent = Sync.World.ServerTime;
            });

            Events.Add("ATM::Show", (object[] args) =>
            {
                Players.CloseAll(true);

                Player.LocalPlayer.SetData("CurrentATM::Id", (int)args[0]);

                Show(Utils.Convert.ToDecimal(args[1]));
            });
        }

        public static async System.Threading.Tasks.Task Show(decimal fee)
        {
            if (IsActive)
                return;

            if (Misc.IsAnyCefActive(true))
                return;

            var data = PlayerData.GetData(Player.LocalPlayer);

            if (data == null)
                return;

            await CEF.Browser.Render(Browser.IntTypes.ATM, true, true);

            CloseColshape = new Additional.Sphere(Player.LocalPlayer.Position, 2.5f, false, Misc.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close();
                }
            };

            CEF.Browser.Window.ExecuteJs("ATM.draw", new object[] { new object[] { data.BankBalance, fee * 100 } });

            CEF.Cursor.Show(true, true);

            TempBinds.Add(Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close()));
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CloseColshape?.Destroy();

            CloseColshape = null;

            CEF.Browser.Render(Browser.IntTypes.ATM, false, false);

            CEF.Cursor.Show(false, false);

            foreach (var x in TempBinds)
                Core.Unbind(x);

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
