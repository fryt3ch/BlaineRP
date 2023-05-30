using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.CEF
{
    class Gas : Events.Script
    {
        public static bool IsActive { get => CEF.Browser.IsActive(Browser.IntTypes.VehicleMisc); }

        private static DateTime LastSent;

        private static int StationID { get => Player.LocalPlayer.HasData("CurrentGasStation") ? Player.LocalPlayer.GetData<int>("CurrentGasStation") : -1; }

        private static Vehicle TargetVehicle { get; set; }

        public static Dictionary<Data.Vehicles.Vehicle.FuelTypes, string> GasIds { get; private set; } = new Dictionary<Data.Vehicles.Vehicle.FuelTypes, string>()
        {
            { Data.Vehicles.Vehicle.FuelTypes.Petrol, "gas_g_0" },

            { Data.Vehicles.Vehicle.FuelTypes.Electricity, "gas_e_0" },
        };

        private static Additional.ExtraColshape CloseColshape { get; set; }

        private static List<int> TempBinds { get; set; }

        public Gas()
        {
            LastSent = DateTime.MinValue;

            TempBinds = new List<int>();

            Events.Add("Gas::Buy", (object[] args) =>
            {
                bool byCash = (bool)args[0];
                int amount = (int)args[1];

                var stationId = StationID;

                if (stationId == -1)
                    return;

                var vData = Sync.Vehicles.GetData(TargetVehicle);

                if (vData == null || vData.Data == null)
                    return;

                if (!LastSent.IsSpam(500, false, false))
                {
                    Events.CallRemote("GasStation::Buy", TargetVehicle, (int)vData.Data.FuelType, amount, byCash);

                    LastSent = Sync.World.ServerTime;
                }
            });


            Events.Add("GasStation::Show", async (object[] args) =>
            {
                var margin = Utils.ToDecimal(args[0]);

                await Show(margin);
            });

            Events.Add("CarMaint::Close", async (object[] args) =>
            {
                Close();

                CEF.Numberplates.Close();
            });
        }

        public static async void RequestShow(Vehicle vehicle, bool showGasAnyway = false)
        {
            if (IsActive)
                return;

            var vData = Sync.Vehicles.GetData(vehicle);

            if (vData == null || vData.Data == null)
                return;

            var vDataData = vData.Data;

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (Player.LocalPlayer.Vehicle != null)
            {
                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.Header, Locale.Notifications.Vehicles.InVehicleError);

                return;
            }

            if (vDataData.FuelType == Data.Vehicles.Vehicle.FuelTypes.None)
                return;

            var vehIsPetrol = vDataData.FuelType == Data.Vehicles.Vehicle.FuelTypes.Petrol;

            var maxFuel = (int)Math.Floor(vDataData.Tank - vData.FuelLevel);

            if (maxFuel == 0)
            {
                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.Header, vehIsPetrol ? Locale.Notifications.Vehicles.FullOfGasDef : Locale.Notifications.Vehicles.FullOfGasElectrical);

                return;
            }

            var gasStationId = StationID;

            var allGasItems = new List<(int, string, int)>();

            if (!showGasAnyway)
            {
                for (int i = 0; i < CEF.Inventory.ItemsParams.Length; i++)
                {
                    if (CEF.Inventory.ItemsParams[i] == null)
                        continue;

                    if (Data.Items.GetType(CEF.Inventory.ItemsParams[i].Id, false) == typeof(Data.Items.VehicleJerrycan))
                    {
                        if ((Data.Items.GetData(CEF.Inventory.ItemsParams[i].Id, typeof(Data.Items.VehicleJerrycan)) as Data.Items.VehicleJerrycan.ItemData)?.IsPetrol == vehIsPetrol)
                        {
                            var name = ((string)(((object[])CEF.Inventory.ItemsData[i][0])[1]));

                            var nameT = name.Split(' ');

                            var fuelAmount = int.Parse(new string(nameT[nameT.Length - 1].Where(x => char.IsDigit(x)).ToArray()));

                            allGasItems.Add((i, name, fuelAmount));
                        }
                    }
                }
            }

            async void gasStationOrItemSelectActionBoxAction(CEF.ActionBox.ReplyTypes rType, decimal id)
            {
                CEF.ActionBox.Close(true);

                if (rType == CEF.ActionBox.ReplyTypes.OK)
                {
                    if (vehicle?.Exists == true)
                    {
                        if (id < 0)
                        {
                            CEF.Gas.RequestShow(vehicle, true);
                        }
                        else
                        {
                            var fuelAmount = allGasItems.Where(x => x.Item1 == id).Select(x => Math.Min(x.Item3, maxFuel)).FirstOrDefault();

                            if (fuelAmount > 0)
                            {
                                await CEF.ActionBox.ShowRange
                                (
                                    "GasItemRange", Locale.Actions.GasItemRangeHeader, 1, fuelAmount, fuelAmount, 1, CEF.ActionBox.RangeSubTypes.Default,

                                    CEF.ActionBox.DefaultBindAction,

                                    (rType, amountD) => gasItemRangeActionBoxAction(rType, amountD, (int)id),

                                    null
                                );
                            }
                        }
                    }
                }
            }

            void gasItemRangeActionBoxAction(CEF.ActionBox.ReplyTypes rType, decimal amountD, int itemIdx)
            {
                int amount;

                if (!amountD.IsNumberValid(1, int.MaxValue, out amount, true))
                    return;

                CEF.ActionBox.Close(true);

                if (rType == CEF.ActionBox.ReplyTypes.OK)
                {
                    Events.CallRemote("Vehicles::JerrycanUse", vehicle, itemIdx, amount);
                }
            }

            if (gasStationId < 0)
            {
                if (allGasItems.Count == 0)
                {
                    CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.Header, Locale.Notifications.Vehicles.NotAtGasStationError);
                }
                else if (allGasItems.Count == 1)
                {
                    var item = allGasItems[0];

                    var maxFuel1 = Math.Min(item.Item3, maxFuel);

                    await CEF.ActionBox.ShowRange
                    (
                        "GasItemRange", Locale.Actions.GasItemRangeHeader, 1, maxFuel1, maxFuel1, 1, ActionBox.RangeSubTypes.Default,

                        CEF.ActionBox.DefaultBindAction,

                        (rType, amountD) => gasItemRangeActionBoxAction(rType, amountD, item.Item1),

                        null
                    );
                }
                else
                {
                    await CEF.ActionBox.ShowSelect
                    (
                        "GasStationOrItemSelect", Locale.Actions.DefaultSelectHeader, allGasItems.Select(x => ((decimal)x.Item1, x.Item2)).ToArray(), null, null,

                        CEF.ActionBox.DefaultBindAction,

                        (rType, id) => gasStationOrItemSelectActionBoxAction(rType, id),

                        null
                    );
                }

                return;
            }
            else if (allGasItems.Count > 0)
            {
                allGasItems.Insert(0, (-1, Locale.Actions.GasStationText, 0));

                await CEF.ActionBox.ShowSelect
                (
                    "GasStationOrItemSelect", Locale.Actions.DefaultSelectHeader, allGasItems.Select(x => ((decimal)x.Item1, x.Item2)).ToArray(), null, null,

                    CEF.ActionBox.DefaultBindAction,

                    (rType, id) => gasStationOrItemSelectActionBoxAction(rType, id),

                    null
                );

                return;
            }

            TargetVehicle = vehicle;

            if (LastSent.IsSpam(1000, false, false))
                return;

            Events.CallRemote("GasStation::Enter", vehicle, gasStationId);

            LastSent = Sync.World.ServerTime;
        }

        public static async System.Threading.Tasks.Task Show(decimal margin)
        {
            if (IsActive || TargetVehicle == null)
                return;

            var vData = Sync.Vehicles.GetData(TargetVehicle);

            if (vData == null || vData.Data == null)
                return;

            var maxFuel = (int)Math.Ceiling(vData.Data.Tank - vData.FuelLevel);

            await CEF.Browser.Render(Browser.IntTypes.VehicleMisc, true, true);

            CloseColshape = new Additional.Sphere(Player.LocalPlayer.Position, 2.5f, false, Utils.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close(false);
                }
            };

            var prices = CEF.Shop.GetPrices(Shop.Types.GasStation);

            CEF.Browser.Window.ExecuteJs("CarMaint.drawGas", new object[] { vData.Data.FuelType == Data.Vehicles.Vehicle.FuelTypes.Petrol, new object[] { maxFuel, prices[GasIds[vData.Data.FuelType]] * margin } });

            CEF.Cursor.Show(true, true);

            GameEvents.Render -= Render;
            GameEvents.Render += Render;

            TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close()));
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            CloseColshape?.Destroy();

            CloseColshape = null;

            Events.CallRemote("GasStation::Exit");

            CEF.Cursor.Show(false, false);

            GameEvents.Render -= Render;

            CEF.Browser.Render(Browser.IntTypes.VehicleMisc, false);

            TargetVehicle = null;

            foreach (var x in TempBinds)
                KeyBinds.Unbind(x);

            TempBinds.Clear();
        }

        private static void Render()
        {
            if (TargetVehicle?.Exists != true || Vector3.Distance(Player.LocalPlayer.Position, TargetVehicle.Position) > Settings.ENTITY_INTERACTION_MAX_DISTANCE_RENDER)
            {
                Close();
            }
        }
    }
}
