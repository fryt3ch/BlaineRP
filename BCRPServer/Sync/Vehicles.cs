using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Sync
{
    public class Vehicles
    {
        public static void SetFixed(Vehicle veh)
        {
            if (veh.Controller is Player controller)
            {
                controller.TriggerEvent("Vehicles::Fix", veh);
            }
            else
            {
                veh.Repair();
            }
        }

        public static void SetVisualFixed(Vehicle veh)
        {
            if (veh.Controller is Player controller)
            {
                controller.TriggerEvent("Vehicles::FixV", veh);
            }
        }
    }
}
