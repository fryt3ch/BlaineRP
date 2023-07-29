using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Data.Vehicles;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Game.EntitiesData.Vehicles;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.Items;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class Gas
    {
        private static DateTime LastSent;

        private static int EscBindIdx = -1;

        public Gas()
        {
            Events.Add("Gas::Buy",
                async (object[] args) =>
                {
                    var useCash = (bool)args[0];
                    var amount = (int)args[1];

                    int stationId = StationID;

                    if (stationId == -1)
                        return;

                    var vData = VehicleData.GetData(TargetVehicle);

                    if (vData == null || vData.Data == null)
                        return;

                    if (!LastSent.IsSpam(500, false, true))
                    {
                        LastSent = World.Core.ServerTime;

                        if ((bool)await Events.CallRemoteProc("Shop::Buy",
                                $"{GetGasBuyIdByFuelType(vData.Data.FuelType)}&{TargetVehicle.RemoteId}&{amount}&{(BuyByFraction ? 1 : 0)}",
                                useCash
                            ))
                        {
                        }
                    }
                }
            );

            Events.Add("CarMaint::Close",
                async (object[] args) =>
                {
                    Close();

                    Numberplates.Close();
                }
            );
        }

        public static bool IsActive => Browser.IsActive(Browser.IntTypes.VehicleMisc);

        private static int StationID => Player.LocalPlayer.HasData("CurrentGasStation") ? Player.LocalPlayer.GetData<int>("CurrentGasStation") : -1;

        private static RAGE.Elements.Vehicle TargetVehicle { get; set; }

        private static ExtraColshape CloseColshape { get; set; }

        public static bool BuyByFraction { get; set; }

        public static string GetGasBuyIdByFuelType(FuelTypes fType)
        {
            return fType == FuelTypes.Electricity ? "gas_e_0" : "gas_g_0";
        }

        public static async void RequestShow(RAGE.Elements.Vehicle vehicle, bool showGasAnyway = false)
        {
            if (IsActive)
                return;

            var vData = VehicleData.GetData(vehicle);

            if (vData == null || vData.Data == null)
                return;

            Data.Vehicles.Vehicle vDataData = vData.Data;

            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (Player.LocalPlayer.Vehicle != null)
            {
                Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.Header, Locale.Notifications.Vehicles.InVehicleError);

                return;
            }

            if (vDataData.FuelType == FuelTypes.None)
                return;

            bool vehIsPetrol = vDataData.FuelType == FuelTypes.Petrol;

            var maxFuel = (int)Math.Floor(vDataData.Tank - vData.FuelLevel);

            if (maxFuel == 0)
            {
                Notification.Show(Notification.Types.Error,
                    Locale.Notifications.Vehicles.Header,
                    vehIsPetrol ? Locale.Notifications.Vehicles.FullOfGasDef : Locale.Notifications.Vehicles.FullOfGasElectrical
                );

                return;
            }

            int gasStationId = StationID;

            var allGasItems = new List<(int, string, int)>();

            if (!showGasAnyway)
                for (var i = 0; i < Inventory.ItemsParams.Length; i++)
                {
                    if (Inventory.ItemsParams[i] == null)
                        continue;

                    if (Items.Core.GetType(Inventory.ItemsParams[i].Id, false) == typeof(VehicleJerrycan))
                        if ((Items.Core.GetData(Inventory.ItemsParams[i].Id, typeof(VehicleJerrycan)) as VehicleJerrycan.ItemData)?.IsPetrol == vehIsPetrol)
                        {
                            var name = (string)((object[])Inventory.ItemsData[i][0])[1];

                            string[] nameT = name.Split(' ');

                            var fuelAmount = int.Parse(new string(nameT[nameT.Length - 1].Where(x => char.IsDigit(x)).ToArray()));

                            allGasItems.Add((i, name, fuelAmount));
                        }
                }

            async void gasStationOrItemSelectActionBoxAction(ActionBox.ReplyTypes rType, decimal id)
            {
                ActionBox.Close(true);

                if (rType == ActionBox.ReplyTypes.OK)
                    if (vehicle?.Exists == true)
                    {
                        if (id < 0)
                        {
                            if (id == -2)
                                BuyByFraction = true;

                            RequestShow(vehicle, true);
                        }
                        else
                        {
                            int fuelAmount = allGasItems.Where(x => x.Item1 == id).Select(x => Math.Min(x.Item3, maxFuel)).FirstOrDefault();

                            if (fuelAmount > 0)
                                await ActionBox.ShowRange("GasItemRange",
                                    Locale.Actions.GasItemRangeHeader,
                                    1,
                                    fuelAmount,
                                    fuelAmount,
                                    1,
                                    ActionBox.RangeSubTypes.Default,
                                    ActionBox.DefaultBindAction,
                                    (rType, amountD) => gasItemRangeActionBoxAction(rType, amountD, (int)id),
                                    null
                                );
                        }
                    }
            }

            void gasItemRangeActionBoxAction(ActionBox.ReplyTypes rType, decimal amountD, int itemIdx)
            {
                int amount;

                if (!amountD.IsNumberValid(1, int.MaxValue, out amount, true))
                    return;

                ActionBox.Close(true);

                if (rType == ActionBox.ReplyTypes.OK)
                    Events.CallRemote("Vehicles::JerrycanUse", vehicle, itemIdx, amount);
            }

            if (gasStationId < 0)
            {
                if (allGasItems.Count == 0)
                {
                    Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.Header, Locale.Notifications.Vehicles.NotAtGasStationError);
                }
                else if (allGasItems.Count == 1)
                {
                    (int, string, int) item = allGasItems[0];

                    int maxFuel1 = Math.Min(item.Item3, maxFuel);

                    await ActionBox.ShowRange("GasItemRange",
                        Locale.Actions.GasItemRangeHeader,
                        1,
                        maxFuel1,
                        maxFuel1,
                        1,
                        ActionBox.RangeSubTypes.Default,
                        ActionBox.DefaultBindAction,
                        (rType, amountD) => gasItemRangeActionBoxAction(rType, amountD, item.Item1),
                        null
                    );
                }
                else
                {
                    await ActionBox.ShowSelect("GasStationOrItemSelect",
                        Locale.Actions.DefaultSelectHeader,
                        allGasItems.Select(x => ((decimal)x.Item1, x.Item2)).ToArray(),
                        null,
                        null,
                        ActionBox.DefaultBindAction,
                        (rType, id) => gasStationOrItemSelectActionBoxAction(rType, id),
                        null
                    );
                }

                return;
            }
            else if (allGasItems.Count > 0)
            {
                allGasItems.Insert(0, (-1, Locale.Get("SHOP_GAS_L_DEFAULT"), 0));

                if (pData.CurrentFraction != null)
                    allGasItems.Insert(1, (-2, Locale.Get("SHOP_GAS_L_FRACTON"), 0));

                await ActionBox.ShowSelect("GasStationOrItemSelect",
                    Locale.Actions.DefaultSelectHeader,
                    allGasItems.Select(x => ((decimal)x.Item1, x.Item2)).ToArray(),
                    null,
                    null,
                    ActionBox.DefaultBindAction,
                    (rType, id) => gasStationOrItemSelectActionBoxAction(rType, id),
                    null
                );

                return;
            }

            TargetVehicle = vehicle;

            if (LastSent.IsSpam(1000, false, true))
                return;

            LastSent = World.Core.ServerTime;

            object res = await Events.CallRemoteProc("GasStation::Enter", vehicle, gasStationId);

            if (res == null)
                return;

            Show(Utils.Convert.ToDecimal(res));
        }

        public static async System.Threading.Tasks.Task Show(decimal margin)
        {
            if (IsActive || TargetVehicle == null)
                return;

            var vData = VehicleData.GetData(TargetVehicle);

            if (vData == null || vData.Data == null)
                return;

            var maxFuel = (int)Math.Ceiling(vData.Data.Tank - vData.FuelLevel);

            await Browser.Render(Browser.IntTypes.VehicleMisc, true, true);

            if (BuyByFraction)
                Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Get("SHOP_GAS_FRACTION_INFO"));

            CloseColshape = new Sphere(Player.LocalPlayer.Position, 2.5f, false, Utils.Misc.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close(false);
                },
            };

            Dictionary<string, uint> prices = Shop.GetPrices(Game.Businesses.BusinessType.GasStation);

            Browser.Window.ExecuteJs("CarMaint.drawGas",
                new object[]
                {
                    vData.Data.FuelType == FuelTypes.Petrol,
                    new object[]
                    {
                        maxFuel,
                        prices[GetGasBuyIdByFuelType(vData.Data.FuelType)] * margin,
                    },
                }
            );

            Cursor.Show(true, true);

            Main.Render -= Render;
            Main.Render += Render;

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            CloseColshape?.Destroy();

            CloseColshape = null;

            Events.CallRemote("GasStation::Exit");

            Cursor.Show(false, false);

            Main.Render -= Render;

            Browser.Render(Browser.IntTypes.VehicleMisc, false);

            TargetVehicle = null;

            Input.Core.Unbind(EscBindIdx);

            EscBindIdx = -1;

            BuyByFraction = false;
        }

        private static void Render()
        {
            if (TargetVehicle?.Exists != true || Vector3.Distance(Player.LocalPlayer.Position, TargetVehicle.Position) > Settings.App.Static.EntityInteractionMaxDistance)
                Close();
        }
    }
}