using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Input;
using BlaineRP.Client.Sync;
using Core = BlaineRP.Client.Input.Core;

namespace BlaineRP.Client.CEF
{
    [Script(int.MaxValue)]
    public class GarageMenu
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.MenuGarage);

        private static DateTime LastSent;

        private static Data.Locations.GarageRoot CurrentGarageRoot { get; set; }

        private static List<int> TempBinds { get; set; }

        private static Additional.ExtraColshape CloseColshape { get; set; }

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

                if (LastSent.IsSpam(500, false, true))
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
                    var approveContext = "GarageMenuSellGov}";
                    var approveTime = 5_000;

                    if (CEF.Notification.HasApproveTimedOut(approveContext, Sync.World.ServerTime, approveTime))
                    {
                        CEF.Notification.SetCurrentApproveContext(approveContext, Sync.World.ServerTime);

                        CEF.Notification.Show(CEF.Notification.Types.Question, Locale.Get("NOTIFICATION_HEADER_APPROVE"), string.Format(Locale.Notifications.Money.AdmitToSellGov1, Locale.Get("GEN_MONEY_0", Misc.GetGovSellPrice(garage.Price))), approveTime);
                    }
                    else
                    {
                        CEF.Notification.ClearAll();

                        CEF.Notification.SetCurrentApproveContext(null, DateTime.MinValue);

                        if ((bool)await Events.CallRemoteProc("Garage::STG", garage.Id))
                        {
                            Close();
                        }
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

            if (Misc.IsAnyCefActive(true))
                return;

            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var ownedGarages = pData.OwnedGarages.Select(x => x.Id);

            var isOwnedGarageClosed = false;

            var ownedGarage = pData.OwnedGarages.Where(x => x.RootId == gRoot.Id).FirstOrDefault();

            if (ownedGarage != null)
                isOwnedGarageClosed = (bool?)await Events.CallRemoteProc("Garage::GetIsLocked", ownedGarage.Id) ?? false;

            await CEF.Browser.Render(Browser.IntTypes.MenuGarage, true, true);

            CloseColshape = new Additional.Sphere(Player.LocalPlayer.Position, 2.5f, false, Misc.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close();
                }
            };

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

            CEF.Browser.Window.ExecuteJs("MenuGar.draw", new object[] { new object[] { gRoot.Id + 1, gData, ownedNum } });

            CEF.Cursor.Show(true, true);

            TempBinds.Add(Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close()));
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CloseColshape?.Destroy();

            CloseColshape = null;

            CEF.Browser.Render(Browser.IntTypes.MenuGarage, false);

            CurrentGarageRoot = null;

            foreach (var x in TempBinds)
                Core.Unbind(x);

            CEF.Cursor.Show(false, false);

            TempBinds.Clear();
        }

        private static void SetIsLocked(int gNum, bool state) => CEF.Browser.Window.ExecuteJs("MenuGar.changeLocked", gNum, state);
    }
}
