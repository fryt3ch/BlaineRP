using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;
using System;

namespace BlaineRP.Client.Sync
{
    public class Crouch
    {
        private static DateTime LastSwitchTime;

        private const string MovementClipSet = "move_ped_crouched";
        private const string StrafeClipSet = "move_ped_crouched_strafing";
        public const float ClipSetSwitchTime = 0.25f;

        public static bool Toggled { get; private set; }

        public static void Toggle()
        {
            if (LastSwitchTime.IsSpam(250, false, false))
                return;

            if (!Toggled)
            {
                if (PlayerActions.IsAnyActionActive(false, PlayerActions.Types.InVehicle, PlayerActions.Types.Knocked, PlayerActions.Types.Frozen, PlayerActions.Types.IsSwimming, PlayerActions.Types.Climbing, PlayerActions.Types.Falling, PlayerActions.Types.Ragdoll, PlayerActions.Types.Jumping, PlayerActions.Types.NotOnFoot))
                    return;

                On();
            }
            else
            {
                Off();
            }

            LastSwitchTime = Sync.World.ServerTime;
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

                await Streaming.RequestClipSet(MovementClipSet);
                await Streaming.RequestClipSet(StrafeClipSet);

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
            if (PlayerActions.IsAnyActionActive(false, PlayerActions.Types.InVehicle, PlayerActions.Types.Knocked, PlayerActions.Types.Frozen, PlayerActions.Types.IsSwimming, PlayerActions.Types.Climbing, PlayerActions.Types.Falling, PlayerActions.Types.Ragdoll, PlayerActions.Types.Jumping, PlayerActions.Types.NotOnFoot))
                Off();

            if (Utils.Game.Camera.IsFirstPersonActive())
                RAGE.Game.Cam.SetFollowPedCamViewMode(0);
        }
    }
}
