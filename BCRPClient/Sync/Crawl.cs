using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Sync
{
    class Crawl : Events.Script
    {
        private static DateTime LastSwitchTime;

        private const string AnimDict = "move_crawlprone2crawlfront";
        private const string MoveAnimDict = "move_crawl";

        private static string CurrentMoveAnim = null;

        public static bool Toggled { get; private set; }

        public Crawl()
        {
            LastSwitchTime = DateTime.Now;
        }

        public static void Toggle()
        {
            if (LastSwitchTime.IsSpam(1000, false, false))
                return;

            if (!Toggled)
            {
                if (!Utils.CanDoSomething(false, Utils.Actions.Knocked, Utils.Actions.Frozen, Utils.Actions.Cuffed, Utils.Actions.Animation, Utils.Actions.Scenario, Utils.Actions.FastAnimation, Utils.Actions.InVehicle, Utils.Actions.Shooting, Utils.Actions.Reloading, Utils.Actions.Climbing, Utils.Actions.Falling, Utils.Actions.Ragdoll, Utils.Actions.Jumping, Utils.Actions.NotOnFoot, Utils.Actions.IsSwimming, Utils.Actions.HasItemInHands, Utils.Actions.IsAttachedTo))
                    return;

                On();
            }
            else
            {
                Off();
            }

            LastSwitchTime = DateTime.Now;
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
                await Utils.RequestAnimDict(AnimDict);
                await Utils.RequestAnimDict(MoveAnimDict);

                CurrentMoveAnim = null;

                GameEvents.Render -= OnTick;
                GameEvents.Render += OnTick;

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

                GameEvents.Render -= OnTick;
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
            if (!Utils.CanDoSomething(false, Utils.Actions.Knocked, Utils.Actions.Frozen, Utils.Actions.Cuffed, Utils.Actions.Animation, Utils.Actions.Scenario, Utils.Actions.FastAnimation, Utils.Actions.InVehicle, Utils.Actions.Shooting, Utils.Actions.Reloading, Utils.Actions.Climbing, Utils.Actions.Falling, Utils.Actions.Ragdoll, Utils.Actions.Jumping, Utils.Actions.NotOnFoot, Utils.Actions.IsSwimming, Utils.Actions.HasItemInHands, Utils.Actions.IsAttachedTo))
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

                    AsyncTask.RunSlim(() =>
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

                    AsyncTask.RunSlim(() =>
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
