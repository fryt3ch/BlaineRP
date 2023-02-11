using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

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

                    LastSent = DateTime.Now;
                }
            });


            Events.Add("GasStation::Show", async (object[] args) =>
            {
                var margin = args[0].ToDecimal();

                await Show(margin);
            });

            Events.Add("CarMaint::Close", async (object[] args) =>
            {
                Close();

                CEF.Numberplates.Close();
            });
        }

        public static void RequestShow(Vehicle vehicle)
        {
            if (IsActive)
                return;

            var gasStationId = StationID;

            if (gasStationId < 0)
            {
                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.Header, Locale.Notifications.Vehicles.NotAtGasStationError);

                return;
            }

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var vData = Sync.Vehicles.GetData(vehicle);

            if (vData == null || vData.Data == null)
                return;

            if (Player.LocalPlayer.Vehicle != null)
            {
                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.Header, Locale.Notifications.Vehicles.InVehicleError);

                return;
            }

            if (vData.FuelLevel == vData.Data.Tank)
            {
                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.Header, vData.Data.FuelType == Data.Vehicles.Vehicle.FuelTypes.Electricity ? Locale.Notifications.Vehicles.FullOfGasElectrical : Locale.Notifications.Vehicles.FullOfGasDef);

                return;
            }

            if (vData.Data.FuelType == Data.Vehicles.Vehicle.FuelTypes.None)
                return;

            TargetVehicle = vehicle;

            if (LastSent.IsSpam(1000, false, false))
                return;

            Events.CallRemote("GasStation::Enter", vehicle, gasStationId);

            LastSent = DateTime.Now;
        }

        public static async System.Threading.Tasks.Task Show(decimal margin)
        {
            if (IsActive || TargetVehicle == null)
                return;

            var vData = Sync.Vehicles.GetData(TargetVehicle);

            if (vData == null || vData.Data == null)
                return;

            int maxFuel = (int)Math.Ceiling(vData.Data.Tank - vData.FuelLevel);

            await CEF.Browser.Render(Browser.IntTypes.VehicleMisc, true, true);

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

            if (!ignoreTimeout && LastSent.IsSpam(1000))
            {
                return;
            }

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
