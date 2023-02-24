using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.CEF
{
    public class GarageMenu : Events.Script
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.MenuGarage);

        private static DateTime LastSent;

        private static Data.Locations.GarageRoot CurrentGarageRoot { get; set; }

        private static List<int> TempBinds { get; set; }

        public GarageMenu()
        {
            LastSent = DateTime.MinValue;

            TempBinds = new List<int>();

            Events.Add("MenuGar::Close", (args) => Close());

            Events.Add("MenuGar::Action", async (args) =>
            {
                var aId = (string)args[0];

                var gId = (int)args[1];

                var garage = CurrentGarageRoot?.AllGarages.ElementAt(gId - 1);

                if (garage == null)
                    return;

                if (LastSent.IsSpam(500, false, false))
                    return;

                LastSent = Sync.World.ServerTime;

                if (aId == "enter")
                {
                    Events.CallRemote("Garage::Enter", garage.Id);
                }
                else if (aId == "open")
                {
                    if ((bool)await Events.CallRemoteProc("Garage::ToggleLock", garage.Id, false))
                        SetIsLocked(gId, false);
                }
                else if (aId == "close")
                {
                    if ((bool)await Events.CallRemoteProc("Garage::ToggleLock", garage.Id, true))
                        SetIsLocked(gId, true);
                }
                else if (aId == "sell")
                {
                    if (!Player.LocalPlayer.HasData("GarageMenu::SellGov::ApproveTime") || Sync.World.ServerTime.Subtract(Player.LocalPlayer.GetData<DateTime>("GarageMenu::SellGov::ApproveTime")).TotalMilliseconds > 5000)
                    {
                        Player.LocalPlayer.SetData("GarageMenu::SellGov::ApproveTime", Sync.World.ServerTime);

                        CEF.Notification.Show(CEF.Notification.Types.Question, Locale.Notifications.ApproveHeader, string.Format(Locale.Notifications.Money.AdmitToSellGov1, Utils.GetPriceString(Utils.GetGovSellPrice(garage.Price))), 5000);
                    }
                    else
                    {
                        Events.CallRemote("Garage::SellGov", garage.Id);

                        Player.LocalPlayer.ResetData("GarageMenu::SellGov::ApproveTime");
                    }
                }
                else if (aId == "buy")
                {
                    if ((bool)await Events.CallRemoteProc("Garage::BuyGov", garage.Id))
                    {
                        Close();

                        return;
                    }
                }
            });
        }

        public static async System.Threading.Tasks.Task Show(Data.Locations.GarageRoot gRoot)
        {
            if (IsActive)
                return;

            if (Utils.IsAnyCefActive(true))
                return;

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var ownedGarages = pData.OwnedGarages.Select(x => x.Id);

            var isOwnedGarageClosed = false;

            var ownedGarage = pData.OwnedGarages.Where(x => x.RootType == gRoot.Type).FirstOrDefault();

            if (ownedGarage != null)
                isOwnedGarageClosed = (bool?)await Events.CallRemoteProc("Garage::GetIsLocked", ownedGarage.Id) ?? false;

            await CEF.Browser.Render(Browser.IntTypes.MenuGarage, true, true);

            CurrentGarageRoot = gRoot;

            List<object> gData = new List<object>();

            var counter = 0;

            int? ownedNum = null;

            foreach (var x in gRoot.AllGarages)
            {
                counter++;

                var oName = x.OwnerName;

                if (ownedNum == null && ownedGarages.Contains(x.Id))
                {
                    ownedNum = counter;

                    gData.Add(new object[] { counter, oName, (int)x.Type, x.Price, x.Tax, isOwnedGarageClosed });
                }
                else
                {
                    gData.Add(new object[] { counter, oName, (int)x.Type, x.Price, x.Tax, oName == null ? null : (bool?)true });
                }
            }

            CEF.Browser.Window.ExecuteJs("MenuGar.draw", new object[] { new object[] { (int)gRoot.Type + 1, gData, ownedNum } });

            CEF.Cursor.Show(true, true);

            TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close()));
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CEF.Browser.Render(Browser.IntTypes.MenuGarage, false);

            CurrentGarageRoot = null;

            foreach (var x in TempBinds)
                KeyBinds.Unbind(x);

            CEF.Cursor.Show(false, false);

            TempBinds.Clear();

            Player.LocalPlayer.ResetData("GarageMenu::SellGov::ApproveTime");
        }

        private static void SetIsLocked(int gNum, bool state) => CEF.Browser.Window.ExecuteJs("MenuGar.changeLocked", gNum, state);
    }
}
