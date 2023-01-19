//#define DEBUGGING

using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Sync
{
    class Phone : Events.Script
    {
        private static DateTime LastSwitchTime;

        public static bool Toggled { get; private set; }

        private static Utils.Actions[] ActionsToCheck = new Utils.Actions[]
        {
            Utils.Actions.Knocked,
            Utils.Actions.Frozen,
            Utils.Actions.Cuffed,

            Utils.Actions.Crawl,
            Utils.Actions.Finger,

            Utils.Actions.Animation,
            Utils.Actions.FastAnimation,
            Utils.Actions.Scenario,

            Utils.Actions.InWater,
            Utils.Actions.Shooting, Utils.Actions.Reloading,
            Utils.Actions.Climbing, Utils.Actions.Falling, Utils.Actions.Ragdoll, Utils.Actions.Jumping, //Utils.Actions.OnFoot,
        };

        #region Anims
        private static string AnimDict = "cellphone@";
        private static string AnimDictVehicle = "cellphone@in_car@ds";

        private static string AnimTextReadBase = "cellphone_text_read_base";
        private static string AnimTextToCall = "cellphone_text_to_call";
        private static string AnimCallToText = "cellphone_call_to_text";
        private static string AnimCallOut = "cellphone_call_out";
        private static string AnimTextOut = "cellphone_text_out";
        private static string AnimSwipeScreen = "cellphone_swipe_screen";
        private static string AnimRight = "cellphone_right";
        private static string AnimLeft = "cellphone_left";
        private static string AnimUp = "cellphone_up";
        private static string AnimDown = "cellphone_down";
        #endregion

        public Phone()
        {
            LastSwitchTime = DateTime.Now;

            RAGE.Game.Mobile.DestroyMobilePhone();
        }

        public static void Toggle()
        {
            if (LastSwitchTime.IsSpam(2000, false, false) || Utils.IsAnyCefActive() || !Utils.CanDoSomething(ActionsToCheck))
                return;

            LastSwitchTime = DateTime.Now;

            if (!Toggled)
            {
                On();
            }
            else
            {
                Off();
            }
        }

        public static async void On(bool ready = false, Player player = null)
        {
            if (!ready)
            {
                if (Toggled)
                    return;

                PushVehicle.Off();

                Events.CallRemote("Players::TogglePhone", true);
            }
            else
            {
                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    RAGE.Game.Mobile.CreateMobilePhone(0); // default phone (iphone)
                    RAGE.Game.Mobile.SetMobilePhoneScale(0f);
                    RAGE.Game.Mobile.ScriptIsMovingMobilePhoneOffscreen(false);

                    RAGE.Game.Audio.PlaySound(-1, "Put_Away", "Phone_SoundSet_Michael", true, 0, true);

                    Toggled = true;

                    return;
                }

                await Utils.RequestAnimDict(AnimDict);
                await Utils.RequestAnimDict(AnimDictVehicle);

                player.TaskPlayAnim(player.IsInAnyVehicle(false) ? AnimDictVehicle : AnimDict, AnimTextReadBase, 8f, 1f, -1, 50, 0f, false, false, false);
            }
        }

        public static void Off(bool ready = false, Player player = null)
        {
            if (!ready)
            {
                if (!Toggled)
                    return;

                Events.CallRemote("Players::TogglePhone", false);
            }
            else
            {
                if (player.Handle == Player.LocalPlayer.Handle)
                {
                    RAGE.Game.Mobile.DestroyMobilePhone();

                    RAGE.Game.Audio.PlaySound(-1, "Put_Away", "Phone_SoundSet_Michael", true, 0, true);

                    Toggled = false;

                    return;
                }

                player.StopAnimTask(AnimDict, AnimTextReadBase, 2f);
                player.StopAnimTask(AnimDict, AnimTextToCall, 2f);

                player.StopAnimTask(AnimDictVehicle, AnimTextReadBase, 2f);
                player.StopAnimTask(AnimDictVehicle, AnimTextToCall, 2f);
            }
        }

        public static void TurnVehiclePhone(Player player)
        {
            if (player == null || player.Handle == Player.LocalPlayer.Handle)
                return;

            var data = Sync.Players.GetData(player);

            if (data == null || !data.PhoneOn)
                return;

            Off(true, player);
            On(true, player);
        }
    }
}
