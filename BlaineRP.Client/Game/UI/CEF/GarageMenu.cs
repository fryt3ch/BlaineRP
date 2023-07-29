using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Game.Estates;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class GarageMenu
    {
        private static DateTime LastSent;

        public GarageMenu()
        {
            LastSent = DateTime.MinValue;

            TempBinds = new List<int>();

            Events.Add("MenuGar::Close", (args) => Close());

            Events.Add("MenuGar::Action",
                async (args) =>
                {
                    var aId = (string)args[0];

                    var gId = (int)args[1];

                    Garage garage = CurrentGarageRoot?.AllGarages.ElementAt(gId - 1);

                    if (garage == null)
                        return;

                    if (LastSent.IsSpam(500, false, true))
                        return;

                    LastSent = World.Core.ServerTime;

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

                        if (Notification.HasApproveTimedOut(approveContext, World.Core.ServerTime, approveTime))
                        {
                            Notification.SetCurrentApproveContext(approveContext, World.Core.ServerTime);

                            Notification.Show(Notification.Types.Question,
                                Locale.Get("NOTIFICATION_HEADER_APPROVE"),
                                string.Format(Locale.Notifications.Money.AdmitToSellGov1, Locale.Get("GEN_MONEY_0", Utils.Misc.GetGovSellPrice(garage.Price))),
                                approveTime
                            );
                        }
                        else
                        {
                            Notification.ClearAll();

                            Notification.SetCurrentApproveContext(null, DateTime.MinValue);

                            if ((bool)await Events.CallRemoteProc("Garage::STG", garage.Id))
                                Close();
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
                }
            );
        }

        public static bool IsActive => Browser.IsActive(Browser.IntTypes.MenuGarage);

        private static GarageRoot CurrentGarageRoot { get; set; }

        private static List<int> TempBinds { get; set; }

        private static ExtraColshape CloseColshape { get; set; }

        public static async System.Threading.Tasks.Task Show(GarageRoot gRoot)
        {
            if (IsActive)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            IEnumerable<uint> ownedGarages = pData.OwnedGarages.Select(x => x.Id);

            var isOwnedGarageClosed = false;

            Garage ownedGarage = pData.OwnedGarages.Where(x => x.RootId == gRoot.Id).FirstOrDefault();

            if (ownedGarage != null)
                isOwnedGarageClosed = (bool?)await Events.CallRemoteProc("Garage::GetIsLocked", ownedGarage.Id) ?? false;

            await Browser.Render(Browser.IntTypes.MenuGarage, true, true);

            CloseColshape = new Sphere(Player.LocalPlayer.Position, 2.5f, false, Utils.Misc.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close();
                },
            };

            CurrentGarageRoot = gRoot;

            var gData = new List<object>();

            var counter = 0;

            int? ownedNum = null;

            foreach (Garage x in gRoot.AllGarages)
            {
                counter++;

                string oName = x.OwnerName;

                if (ownedNum == null && ownedGarages.Contains(x.Id))
                {
                    ownedNum = counter;

                    gData.Add(new object[]
                        {
                            counter,
                            oName,
                            (int)x.Type,
                            x.Price,
                            x.Tax,
                            isOwnedGarageClosed,
                        }
                    );
                }
                else
                {
                    gData.Add(new object[]
                        {
                            counter,
                            oName,
                            (int)x.Type,
                            x.Price,
                            x.Tax,
                            oName == null ? null : (bool?)true,
                        }
                    );
                }
            }

            Browser.Window.ExecuteJs("MenuGar.draw",
                new object[]
                {
                    new object[]
                    {
                        gRoot.Id + 1,
                        gData,
                        ownedNum,
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

            Browser.Render(Browser.IntTypes.MenuGarage, false);

            CurrentGarageRoot = null;

            foreach (int x in TempBinds)
            {
                Input.Core.Unbind(x);
            }

            Cursor.Show(false, false);

            TempBinds.Clear();
        }

        private static void SetIsLocked(int gNum, bool state)
        {
            Browser.Window.ExecuteJs("MenuGar.changeLocked", gNum, state);
        }
    }
}