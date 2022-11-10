using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Sync
{
    public class Crouch : Events.Script
    {
        private static DateTime LastSwitchTime;

        private const string MovementClipSet = "move_ped_crouched";
        private const string StrafeClipSet = "move_ped_crouched_strafing";
        public const float ClipSetSwitchTime = 0.25f;

        public static bool Toggled { get; private set; }

        private static Utils.Actions[] ActionsToCheck = new Utils.Actions[]
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

        public Crouch()
        {
            LastSwitchTime = DateTime.Now;
        }

        public static void Toggle()
        {
            if (LastSwitchTime.IsSpam(1000, false, false) || !Utils.CanDoSomething(ActionsToCheck))
                return;

            if (!Toggled)
            {
                On();
            }
            else
            {
                Off();
            }

            LastSwitchTime = DateTime.Now;
        }

        public static async void On(bool ready = false, Player player = null)
        {
            if (!ready)
            {
                if (Toggled)
                    return;

                Crawl.Off();
                PushVehicle.Off();

                Events.CallRemote("Players::ToggleCrouchingSync", true);
            }
            else
            {
                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    GameEvents.Update -= OnTick;
                    GameEvents.Update += OnTick;

                    Toggled = true;
                }

                player.ResetMovementClipset(ClipSetSwitchTime);

                await Utils.RequestClipSet(MovementClipSet);
                await Utils.RequestClipSet(StrafeClipSet);

                player.SetStealthMovement(false, "DEFAULT_ACTION");

                player.SetMovementClipset(MovementClipSet, ClipSetSwitchTime);
                player.SetStrafeClipset(StrafeClipSet);
            }
        }

        public static void Off(bool ready = false, Player player = null)
        {
            if (!ready)
            {
                if (!Toggled)
                    return;

                GameEvents.Update -= OnTick;

                Events.CallRemote("Players::ToggleCrouchingSync", false);
            }
            else
            {
                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    Toggled = false;
                }

                player.ResetMovementClipset(ClipSetSwitchTime);
                player.ResetStrafeClipset();

                var pData = Sync.Players.GetData(player);

                if (pData == null)
                    return;

                if (pData.Walkstyle != Animations.WalkstyleTypes.None)
                    Sync.Animations.Set(player, pData.Walkstyle);
            }
        }

        private static void OnTick()
        {
            if (!Utils.CanDoSomething(ActionsToCheck))
                Off();

            if (Utils.IsFirstPersonActive())
                RAGE.Game.Cam.SetFollowPedCamViewMode(0);
        }
    }
}
