using System;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.World;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Misc
{
    [Script(int.MaxValue)]
    public class Crawl
    {
        private static DateTime LastSwitchTime;

        private const string AnimDict = "move_crawlprone2crawlfront";
        private const string MoveAnimDict = "move_crawl";

        private static string CurrentMoveAnim = null;

        public static bool Toggled { get; private set; }

        public Crawl()
        {

        }

        public static void Toggle()
        {
            if (LastSwitchTime.IsSpam(250, false, false))
                return;

            if (!Toggled)
            {
                if (PlayerActions.IsAnyActionActive(false, PlayerActions.Types.Knocked, PlayerActions.Types.Frozen, PlayerActions.Types.Cuffed, PlayerActions.Types.Animation, PlayerActions.Types.Scenario, PlayerActions.Types.FastAnimation, PlayerActions.Types.InVehicle, PlayerActions.Types.Shooting, PlayerActions.Types.Reloading, PlayerActions.Types.Climbing, PlayerActions.Types.Falling, PlayerActions.Types.Ragdoll, PlayerActions.Types.Jumping, PlayerActions.Types.NotOnFoot, PlayerActions.Types.IsSwimming, PlayerActions.Types.HasItemInHands, PlayerActions.Types.IsAttachedTo))
                    return;

                On();
            }
            else
            {
                Off();
            }

            LastSwitchTime = Core.ServerTime;
        }

        public static async void On(bool ready = false)
        {
            if (!ready)
            {
                if (Toggled)
                    return;

                Crouch.Off();
                PushVehicle.Off();

                Events.CallRemote("Players::ToggleCrawlingSync", true);
            }
            else
            {
                await Streaming.RequestAnimDict(AnimDict);
                await Streaming.RequestAnimDict(MoveAnimDict);

                CurrentMoveAnim = null;

                Main.Render -= OnTick;
                Main.Render += OnTick;

                Player.LocalPlayer.TaskPlayAnim(AnimDict, "front", 8.0f, 1000, -1, 2, 0, false, false, false);

                Toggled = true;
            }
        }

        public static void Off(bool ready = false)
        {
            if (!ready)
            {
                if (!Toggled)
                    return;

                Events.CallRemote("Players::ToggleCrawlingSync", false);

                Main.Render -= OnTick;
            }
            else
            {
                Player.LocalPlayer.ClearTasks();
                Player.LocalPlayer.ClearSecondaryTask();

                Toggled = false;
            }
        }

        private static void OnTick()
        {
            if (PlayerActions.IsAnyActionActive(false, PlayerActions.Types.Knocked, PlayerActions.Types.Frozen, PlayerActions.Types.Cuffed, PlayerActions.Types.Animation, PlayerActions.Types.Scenario, PlayerActions.Types.FastAnimation, PlayerActions.Types.InVehicle, PlayerActions.Types.Shooting, PlayerActions.Types.Reloading, PlayerActions.Types.Climbing, PlayerActions.Types.Falling, PlayerActions.Types.Ragdoll, PlayerActions.Types.Jumping, PlayerActions.Types.NotOnFoot, PlayerActions.Types.IsSwimming, PlayerActions.Types.HasItemInHands, PlayerActions.Types.IsAttachedTo))
                Off();

            RAGE.Game.Pad.DisableControlAction(0, 32, true);
            RAGE.Game.Pad.DisableControlAction(0, 33, true);
            RAGE.Game.Pad.DisableControlAction(0, 34, true);
            RAGE.Game.Pad.DisableControlAction(0, 35, true);

            Vector3 rotation = Player.LocalPlayer.GetRotation(2);

            if (RAGE.Game.Pad.IsDisabledControlPressed(0, 32)) // forward
            {
                if (CurrentMoveAnim != "onfront_fwd" && CurrentMoveAnim != "onfront_bwd")
                {
                    CurrentMoveAnim = "onfront_fwd";

                    float duration = RAGE.Game.Entity.GetAnimDuration("move_crawl", CurrentMoveAnim);

                    Player.LocalPlayer.TaskPlayAnim(MoveAnimDict, CurrentMoveAnim, 8.0f, 1000, -1, 2, 0, false, false, false);

                    AsyncTask.Methods.Run(() =>
                    {
                        CurrentMoveAnim = null;
                    }, (int)((duration - 0.1f) * 1000f));
                }
            }

            if (RAGE.Game.Pad.IsDisabledControlPressed(0, 33)) // backward
            {
                if (CurrentMoveAnim != "onfront_fwd" && CurrentMoveAnim != "onfront_bwd")
                {
                    CurrentMoveAnim = "onfront_bwd";

                    float duration = RAGE.Game.Entity.GetAnimDuration("move_crawl", CurrentMoveAnim);

                    Player.LocalPlayer.TaskPlayAnim(MoveAnimDict, CurrentMoveAnim, 8.0f, 1000, -1, 2, 0, false, false, false);

                    AsyncTask.Methods.Run(() =>
                    {
                        CurrentMoveAnim = null;
                    }, (int)((duration - 0.1f) * 1000f));
                }
            }

            if (RAGE.Game.Pad.IsDisabledControlPressed(0, 34)) // left
            {
                Player.LocalPlayer.SetRotation(rotation.X, rotation.Y, rotation.Z + 0.2f, 2, true);
            }

            if (RAGE.Game.Pad.IsDisabledControlPressed(0, 35)) // right
            {
                Player.LocalPlayer.SetRotation(rotation.X, rotation.Y, rotation.Z - 0.2f, 2, true);
            }
        }
    }
}
