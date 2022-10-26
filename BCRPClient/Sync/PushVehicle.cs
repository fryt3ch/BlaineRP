using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Sync
{
    class PushVehicle : Events.Script
    {
        private static DateTime LastSwitchTime;

        public static bool Toggled { get; private set; }

        public static Utils.Actions[] ActionsToCheck = new Utils.Actions[]
        {
            Utils.Actions.Knocked,
            Utils.Actions.Frozen,
            Utils.Actions.Cuffed,

            Utils.Actions.Finger,

            Utils.Actions.Animation,
            Utils.Actions.FastAnimation,
            Utils.Actions.Scenario,

            Utils.Actions.InVehicle,
            Utils.Actions.InWater,
            Utils.Actions.Shooting, Utils.Actions.Reloading,
            Utils.Actions.Climbing, Utils.Actions.Falling, Utils.Actions.Ragdoll, Utils.Actions.Jumping, Utils.Actions.OnFoot,
        };

        public static Utils.Actions[] ActionsToCheckLoop = new Utils.Actions[]
        {
            Utils.Actions.Knocked,
            Utils.Actions.Frozen,
            Utils.Actions.Cuffed,

            Utils.Actions.Finger,

            //Utils.Actions.Animation,
            Utils.Actions.FastAnimation,
            Utils.Actions.Scenario,

            Utils.Actions.InVehicle,
            Utils.Actions.InWater,
            Utils.Actions.Shooting, Utils.Actions.Reloading,
            Utils.Actions.Climbing, Utils.Actions.Falling, Utils.Actions.Ragdoll, Utils.Actions.Jumping, Utils.Actions.OnFoot,
        };

        public PushVehicle()
        {
            LastSwitchTime = DateTime.Now;

            Toggled = false;
        }

        public static void Toggle(Vehicle vehicle = null)
        {
            if (LastSwitchTime.IsSpam(2000, false, false) || Utils.IsAnyCefActive() || !Utils.CanDoSomething(ActionsToCheck))
                return;

            if (!Toggled)
            {
                On(false, vehicle);
            }
            else
            {
                Off();
            }

            LastSwitchTime = DateTime.Now;
        }

        public static void On(bool ready = false, Vehicle vehicle = null)
        {
            if (!ready)
            {
                if (Toggled)
                    return;

                Phone.Off();

                if (!vehicle.IsOnAllWheels())
                    return;

                var data = Vehicles.GetData(vehicle);

                if (data == null)
                    return;

                if (data.EngineOn)
                {
                    CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.DefHeader, Locale.Notifications.Vehicles.Push.EngineOn);

                    return;
                }

                if (data.ForcedSpeed > 0f)
                {
                    return;
                }

                Crouch.Off();
                Crawl.Off();

                Events.CallRemote("Players::StartPushingVehicleSync", vehicle, Utils.PlayerInFrontOfVehicle(vehicle, 2f));
            }
            else
            {
                Interaction.Enabled = false;

                CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Notifications.DefHeader, Locale.Notifications.Vehicles.Push.Started);

                Toggled = true;
            }
        }

        public static void Off(bool ready = false)
        {
            if (!ready)
            {
                if (!Toggled)
                    return;

                Events.CallRemote("Players::StopPushingVehicleSync");
            }
            else
            {
                Interaction.Enabled = true;

                Toggled = false;
            }
        }
    }
}
