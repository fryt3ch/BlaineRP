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

        private static Dictionary<string, int> Prices { get; set; } = new Dictionary<string, int>()
        {
            { "gas_p", 10 }, // petrol

            { "gas_e", 5 }, // electricity
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

                if (!LastSent.IsSpam(500, false, false))
                {
                    Events.CallRemote("GasStation::Buy", TargetVehicle, "gas_p", amount, byCash);

                    LastSent = DateTime.Now;
                }
            });


            Events.Add("GasStation::Show", async (object[] args) =>
            {
                float margin = (float)args[0];

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
                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.Header, Locale.Notifications.Vehicles.NotAtGasStationError, 2500);

                return;
            }

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var vData = Sync.Vehicles.GetData(vehicle);

            if (vData == null)
                return;

            if (Player.LocalPlayer.Vehicle != null)
            {
                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.Vehicles.Header, Locale.Notifications.Vehicles.InVehicleError, 2500);

                return;
            }

            TargetVehicle = vehicle;

            if (LastSent.IsSpam(1000, false, false))
                return;

            Events.CallRemote("GasStation::Enter", vehicle, gasStationId);

            LastSent = DateTime.Now;
        }

        public static async System.Threading.Tasks.Task Show(float margin)
        {
            if (IsActive || TargetVehicle == null)
                return;

            await CEF.Browser.Render(Browser.IntTypes.VehicleMisc, true, true);

            CEF.Browser.Window.ExecuteJs("CarMaint.drawGas", new object[] { new object[] { 100, 10 * margin } });

            CEF.Cursor.Show(true, true);

            GameEvents.Render -= Render;
            GameEvents.Render += Render;

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close()));
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CEF.Cursor.Show(false, false);

            GameEvents.Render -= Render;

            CEF.Browser.Render(Browser.IntTypes.VehicleMisc, false);

            TargetVehicle = null;

            foreach (var x in TempBinds)
                RAGE.Input.Unbind(x);

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
