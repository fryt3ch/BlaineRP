using BCRPClient.CEF.PhoneApps;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Sync
{
    public class Phone : Events.Script
    {
        public enum PhoneStateTypes : byte
        {
            /// <summary>Телефон не используется</summary>
            Off = 0,
            /// <summary>Телефон используется без анимаций</summary>
            JustOn,
            /// <summary>Телефон используется c обычной анимацией</summary>
            Idle,
            /// <summary>Телефон используется с анимацией разговора</summary>
            Call,
            /// <summary>Телефон используется с анимацией камеры 0</summary>
            Camera,
        }

        private static DateTime LastSwitchTime;

        public static bool Toggled { get; private set; }

        #region Anims
        private static string AnimDict = "cellphone@str";
        private static string AnimDictSelf = "cellphone@self";

        private static string AnimTextReadBase = "cellphone_text_press_a";
        private static string AnimCallBase = "cellphone_call_listen_a";
        private static string AnimCameraSelfieBase = "selfie";

        #endregion

        private static AsyncTask CurrentTask { get; set; }

        public Phone()
        {
            LastSwitchTime = DateTime.Now;

            RAGE.Game.Mobile.DestroyMobilePhone();
        }

        public static bool CanUsePhoneAnim(bool notify = false) => Utils.CanDoSomething(notify, Utils.Actions.Crawl, Utils.Actions.IsSwimming, Utils.Actions.Falling, Utils.Actions.Animation, Utils.Actions.Reloading, Utils.Actions.Climbing, Utils.Actions.FastAnimation, Utils.Actions.HasItemInHands, Utils.Actions.HasWeapon, Utils.Actions.IsAttachedTo, Utils.Actions.OtherAnimation);

        public static void Toggle()
        {
            if (LastSwitchTime.IsSpam(2000, false, false))
                return;

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            LastSwitchTime = DateTime.Now;

            if (!Toggled)
            {
                if (Utils.IsAnyCefActive(true))
                    return;

                CallChangeState(pData, CanUsePhoneAnim() ? PhoneStateTypes.Idle : PhoneStateTypes.JustOn);
            }
            else
            {
                CallChangeState(pData, PhoneStateTypes.Off);
            }
        }

        public static void CallChangeState(Sync.Players.PlayerData pData, PhoneStateTypes stateType)
        {
            if (pData.PhoneStateType == stateType)
                return;

            Events.CallRemote("Players::SPST", stateType);
        }

        public static async void SetState(Player player, PhoneStateTypes stateType)
        {
            if (player.Handle == Player.LocalPlayer.Handle)
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (stateType == PhoneStateTypes.Off)
                {
                    player.TaskUseMobilePhone(0, 0);

                    DestroyLocalPhone();

                    if (Toggled)
                    {
                        RAGE.Game.Audio.PlaySound(-1, "Put_Away", "Phone_SoundSet_Michael", true, 0, true);

                        Toggled = false;

                        CEF.Phone.Close();

                        CEF.PhoneApps.CameraApp.Close();

                        if (CurrentTask != null)
                        {
                            CurrentTask.Cancel();

                            CurrentTask = null;
                        }
                    }
                }
                else
                {
                    if (stateType == PhoneStateTypes.JustOn)
                    {
                        player.TaskUseMobilePhone(0, 0);

                        DestroyLocalPhone();
                    }
                    else if (stateType == PhoneStateTypes.Idle)
                    {
                        player.TaskUseMobilePhone(0, 0);

                        CreateLocalPhone();
                    }
                    else if (stateType == PhoneStateTypes.Call)
                    {
                        CreateLocalPhone();

                        player.TaskUseMobilePhone(1, 1);
                    }
                    else if (stateType == PhoneStateTypes.Camera)
                    {

                    }

                    if (!Toggled)
                    {
                        RAGE.Game.Audio.PlaySound(-1, "Put_Away", "Phone_SoundSet_Michael", true, 0, true);

                        Toggled = true;

                        CEF.Phone.Show();

                        CurrentTask?.Cancel();

                        var lastSyncedType = stateType;

                        CurrentTask = new AsyncTask(() =>
                        {
                            if (!Toggled)
                                return;

                            if (CanUsePhoneAnim())
                            {
                                if (pData.ActiveCall?.Player != null)
                                {
                                    if (lastSyncedType != PhoneStateTypes.Call)
                                    {
                                        Events.CallRemote("Players::SPST", PhoneStateTypes.Call);

                                        lastSyncedType = PhoneStateTypes.Call;
                                    }
                                }
                                else if (CEF.PhoneApps.CameraApp.IsActive)
                                {
                                    if (lastSyncedType != PhoneStateTypes.Camera)
                                    {
                                        Events.CallRemote("Players::SPST", PhoneStateTypes.Camera);

                                        lastSyncedType = PhoneStateTypes.Camera;
                                    }
                                }
                                else
                                {

                                    LocalPhoneMoveFingerRandom();

                                    if (lastSyncedType != PhoneStateTypes.Idle)
                                    {
                                        Events.CallRemote("Players::SPST", PhoneStateTypes.Idle);

                                        lastSyncedType = PhoneStateTypes.Idle;
                                    }
                                }
                            }
                            else
                            {
                                CEF.PhoneApps.CameraApp.Close();

                                if (lastSyncedType != PhoneStateTypes.JustOn)
                                {
                                    Events.CallRemote("Players::SPST", PhoneStateTypes.JustOn);

                                    lastSyncedType = PhoneStateTypes.JustOn;
                                }

                            }
                        }, 100, true, 0);

                        CurrentTask.Run();
                    }
                }
            }
            else
            {
                if (stateType == PhoneStateTypes.Off || stateType == PhoneStateTypes.JustOn)
                {
                    player.StopAnimTask(AnimDict, AnimTextReadBase, 3f);
                    player.StopAnimTask(AnimDictSelf, AnimCameraSelfieBase, 3f);

                    player.StopAnimTask(AnimDict, AnimCallBase, 3f);
                }
                else if (stateType == PhoneStateTypes.Idle)
                {
                    await Utils.RequestAnimDict(AnimDict);

                    player.StopAnimTask(AnimDictSelf, AnimCameraSelfieBase, 3f);
                    player.StopAnimTask(AnimDict, AnimCallBase, 3f);

                    player.TaskPlayAnim(AnimDict, AnimTextReadBase, 8f, 0f, -1, 49, 0f, false, false, false);
                }
                else if (stateType == PhoneStateTypes.Call)
                {
                    player.StopAnimTask(AnimDict, AnimTextReadBase, 3f);
                    player.StopAnimTask(AnimDict, AnimCameraSelfieBase, 3f);

                    player.TaskPlayAnim(AnimDict, AnimCallBase, 8f, 0f, -1, 49, 0f, false, false, false);
                }
                else if (stateType == PhoneStateTypes.Camera)
                {
                    await Utils.RequestAnimDict(AnimDictSelf);

                    player.StopAnimTask(AnimDict, AnimTextReadBase, 3f);
                    player.StopAnimTask(AnimDict, AnimCallBase, 3f);

                    player.TaskPlayAnim(AnimDictSelf, AnimCameraSelfieBase, 4f, 4f, -1, 49, 0f, false, false, false);
                }
            }
        }

        public static void CreateLocalPhone()
        {
            if (IsLocalPhoneActive)
                return;

            RAGE.Game.Mobile.CreateMobilePhone(0); // default phone (iphone)
            RAGE.Game.Mobile.SetMobilePhoneScale(0f);
            RAGE.Game.Mobile.ScriptIsMovingMobilePhoneOffscreen(false);

            //RAGE.Game.Mobile.CellCamActivate(false, false);
        }

        public static void DestroyLocalPhone()
        {
            RAGE.Game.Mobile.DestroyMobilePhone();
        }

        public static void LocalPhoneMoveFingerRandom()
        {
            RAGE.Game.Mobile.MoveFinger(Utils.Random.Next(1, 6));
        }

        public static bool IsLocalPhoneActive => Player.LocalPlayer.GetSelectedWeapon() == Sync.WeaponSystem.MobileHash;
    }
}
