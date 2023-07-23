﻿using RAGE;
using RAGE.Elements;
using System;

namespace BlaineRP.Client.Sync
{
    [Script(int.MaxValue)]
    public class PushVehicle 
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
            Utils.Actions.Climbing, Utils.Actions.Falling, Utils.Actions.Ragdoll, Utils.Actions.Jumping, Utils.Actions.NotOnFoot,
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
            Utils.Actions.Climbing, Utils.Actions.Falling, Utils.Actions.Ragdoll, Utils.Actions.Jumping, Utils.Actions.NotOnFoot,
        };

        public PushVehicle()
        {

        }

        public static void Toggle(Vehicle vehicle = null)
        {
            if (LastSwitchTime.IsSpam(2000, false, false) || Utils.IsAnyCefActive() || !Utils.CanDoSomething(true, ActionsToCheck))
                return;

            if (!Toggled)
            {
                On(false, vehicle);
            }
            else
            {
                Off();
            }

            LastSwitchTime = Sync.World.ServerTime;
        }

        public static void On(bool ready = false, Vehicle vehicle = null)
        {
            if (!ready)
            {
                if (Toggled)
                    return;

                if (!vehicle.IsOnAllWheels())
                    return;

                var data = Vehicles.GetData(vehicle);

                if (data == null)
                    return;

                if (data.EngineOn)
                {
                    CEF.Notification.ShowError(Locale.Get("VEHICLE_ENGINE_ON_E_2"));

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