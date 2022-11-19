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

            #region TEST
            #if DEBUGGING

            int xyz = 0;
            bool changePos = true;

            RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Left, true, () =>
            {
                changePos = !changePos;

                RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Info, $"Change: {(changePos ? "position" : "rotation")}");
            });

            RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Right, true, () =>
            {
                xyz += 1;

                if (xyz == 3)
                    xyz = 0;

                RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Info, $"Change: {(xyz == 0 ? "X" : xyz == 1 ? "Y" : "Z")}");
            });

            var pos = AttachSystem.Attachments[AttachSystem.Type.VehKey].PositionOffset;
            var rot = AttachSystem.Attachments[AttachSystem.Type.VehKey].Rotation;

            RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Up, true, () =>
            {
                var list = Player.LocalPlayer.GetData<List<AttachSystem.AttachmentObject>>(AttachSystem.AttachedObjectsKey);
                var prop = AttachSystem.Attachments[AttachSystem.Type.VehKey];

                if (changePos)
                {
                    if (xyz == 0)
                        pos.X += 0.01f;
                    else if (xyz == 1)
                        pos.Y += 0.01f;
                    else
                        pos.Z += 0.01f;
                }
                else
                {
                    if (xyz == 0)
                        rot.X += 5f;
                    else if (xyz == 1)
                        rot.Y += 5f;
                    else
                        rot.Z += 5f;
                }

                RAGE.Game.Entity.DetachEntity(list[0].Object.Handle, false, false);
                RAGE.Game.Entity.AttachEntityToEntity(list[0].Object.Handle, Player.LocalPlayer.Handle, RAGE.Game.Ped.GetPedBoneIndex(Player.LocalPlayer.Handle, prop.BoneID), pos.X, pos.Y, pos.Z, rot.X, rot.Y, rot.Z, false, false, false, false, 2, true);

                RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Info, $"Position: {pos.X}, {pos.Y}, {pos.Z}");
                RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Info, $"Rotation: {rot.X}, {rot.Y}, {rot.Z}");
            });

            RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Down, true, () =>
            {
                var list = Player.LocalPlayer.GetData<List<AttachSystem.AttachmentObject>>(AttachSystem.AttachedObjectsKey);
                var prop = AttachSystem.Attachments[AttachSystem.Type.VehKey];

                if (changePos)
                {
                    if (xyz == 0)
                        pos.X -= 0.01f;
                    else if (xyz == 1)
                        pos.Y -= 0.01f;
                    else
                        pos.Z -= 0.01f;
                }
                else
                {
                    if (xyz == 0)
                        rot.X -= 5f;
                    else if (xyz == 1)
                        rot.Y -= 5f;
                    else
                        rot.Z -= 5f;
                }

                RAGE.Game.Entity.DetachEntity(list[0].Object.Handle, false, false);
                RAGE.Game.Entity.AttachEntityToEntity(list[0].Object.Handle, Player.LocalPlayer.Handle, RAGE.Game.Ped.GetPedBoneIndex(Player.LocalPlayer.Handle, prop.BoneID), pos.X, pos.Y, pos.Z, rot.X, rot.Y, rot.Z, false, false, false, false, 2, true);

                RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Info, $"Position: {pos.X}, {pos.Y}, {pos.Z}");
                RAGE.Ui.Console.LogLine(RAGE.Ui.ConsoleVerbosity.Info, $"Rotation: {rot.X}, {rot.Y}, {rot.Z}");
            });
#endif
            #endregion
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
