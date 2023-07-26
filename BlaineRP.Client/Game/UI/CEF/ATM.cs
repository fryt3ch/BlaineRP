using System;
using System.Collections.Generic;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.Scripts.Sync;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class ATM
    {
        private static DateTime LastSent;

        public ATM()
        {
            TempBinds = new List<int>();

            LastSent = World.Core.ServerTime;

            Events.Add("ATM::Action",
                (object[] args) =>
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

                    LastSent = World.Core.ServerTime;
                }
            );

            Events.Add("ATM::Show",
                (object[] args) =>
                {
                    Players.CloseAll(true);

                    Player.LocalPlayer.SetData("CurrentATM::Id", (int)args[0]);

                    Show(Utils.Convert.ToDecimal(args[1]));
                }
            );
        }

        public static bool IsActive => Browser.IsActive(Browser.IntTypes.ATM);

        private static List<int> TempBinds { get; set; }

        private static ExtraColshape CloseColshape { get; set; }

        public static async System.Threading.Tasks.Task Show(decimal fee)
        {
            if (IsActive)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            var data = PlayerData.GetData(Player.LocalPlayer);

            if (data == null)
                return;

            await Browser.Render(Browser.IntTypes.ATM, true, true);

            CloseColshape = new Sphere(Player.LocalPlayer.Position, 2.5f, false, Utils.Misc.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close();
                },
            };

            Browser.Window.ExecuteJs("ATM.draw",
                new object[]
                {
                    new object[]
                    {
                        data.BankBalance,
                        fee * 100,
                    },
                }
            );

            Cursor.Show(true, true);

            TempBinds.Add(Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close()));
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CloseColshape?.Destroy();

            CloseColshape = null;

            Browser.Render(Browser.IntTypes.ATM, false, false);

            Cursor.Show(false, false);

            foreach (int x in TempBinds)
            {
                Input.Core.Unbind(x);
            }

            TempBinds.Clear();

            Player.LocalPlayer.ResetData("CurrentATM::Id");
        }

        public static void UpdateMoney(ulong value)
        {
            if (!IsActive)
                return;

            Browser.Window.ExecuteJs("ATM.updateBalance", value);
        }
    }
}