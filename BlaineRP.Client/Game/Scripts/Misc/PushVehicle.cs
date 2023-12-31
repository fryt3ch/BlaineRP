﻿using System;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Vehicles;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Game.World;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Scripts.Misc
{
    [Script(int.MaxValue)]
    public class PushVehicle
    {
        private static DateTime LastSwitchTime;

        private static readonly PlayerActions.Types[] ActionsToCheck = new[]
        {
            PlayerActions.Types.Knocked,
            PlayerActions.Types.Frozen,
            PlayerActions.Types.Cuffed,
            PlayerActions.Types.Finger,
            PlayerActions.Types.Animation,
            PlayerActions.Types.FastAnimation,
            PlayerActions.Types.Scenario,
            PlayerActions.Types.InVehicle,
            PlayerActions.Types.InWater,
            PlayerActions.Types.Shooting,
            PlayerActions.Types.Reloading,
            PlayerActions.Types.Climbing,
            PlayerActions.Types.Falling,
            PlayerActions.Types.Ragdoll,
            PlayerActions.Types.Jumping,
            PlayerActions.Types.NotOnFoot,
        };

        public PushVehicle()
        {
        }

        public static bool Toggled { get; private set; }

        public static void Toggle(Vehicle vehicle = null)
        {
            if (LastSwitchTime.IsSpam(2000, false, false) || Utils.Misc.IsAnyCefActive() || PlayerActions.IsAnyActionActive(true, ActionsToCheck))
                return;

            if (!Toggled)
                On(false, vehicle);
            else
                Off();

            LastSwitchTime = Core.ServerTime;
        }

        public static void On(bool ready = false, Vehicle vehicle = null)
        {
            if (!ready)
            {
                if (Toggled)
                    return;

                if (!vehicle.IsOnAllWheels())
                    return;

                var data = VehicleData.GetData(vehicle);

                if (data == null)
                    return;

                if (data.EngineOn)
                {
                    Notification.ShowError(Locale.Get("VEHICLE_ENGINE_ON_E_2"));

                    return;
                }

                if (data.ForcedSpeed > 0f)
                    return;

                Crouch.Off();
                Crawl.Off();

                Events.CallRemote("Players::StartPushingVehicleSync", vehicle, Utils.Game.Vehicles.PlayerInFrontOfVehicle(vehicle, 2f));
            }
            else
            {
                Management.Interaction.Enabled = false;

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
                Management.Interaction.Enabled = true;

                Toggled = false;
            }
        }
    }
}