using System;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;
using Camera = BlaineRP.Client.Game.UI.CEF.Phone.Apps.Camera;
using Core = BlaineRP.Client.Game.World.Core;

namespace BlaineRP.Client.Game.Scripts.Misc
{
    [Script(int.MaxValue)]
    public class Phone
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

        private static DateTime _lastSwitchTime;

        public static bool Toggled { get; private set; }

        private const string AnimDict = "cellphone@str";
        private const string AnimDictSelf = "cellphone@self";

        private const string AnimTextReadBase = "cellphone_text_press_a";
        private const string AnimCallBase = "cellphone_call_listen_a";
        private const string AnimCameraSelfieBase = "selfie";

        private static AsyncTask _currentTask;

        public Phone()
        {
            RAGE.Game.Mobile.DestroyMobilePhone();
        }

        public static bool CanUsePhoneAnim(bool notify = false)
        {
            return !PlayerActions.IsAnyActionActive(notify, PlayerActions.Types.Crawl, PlayerActions.Types.IsSwimming, PlayerActions.Types.Falling, PlayerActions.Types.Animation, PlayerActions.Types.Reloading, PlayerActions.Types.Climbing, PlayerActions.Types.FastAnimation, PlayerActions.Types.HasItemInHands, PlayerActions.Types.HasWeapon, PlayerActions.Types.IsAttachedTo, PlayerActions.Types.OtherAnimation);
        }

        public static void Toggle()
        {
            if (_lastSwitchTime.IsSpam(1000, false, false))
                return;

            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            _lastSwitchTime = Core.ServerTime;

            if (!Toggled)
            {
                if (Utils.Misc.IsAnyCefActive(true))
                    return;

                CallChangeState(pData, CanUsePhoneAnim() ? PhoneStateTypes.Idle : PhoneStateTypes.JustOn);
            }
            else
            {
                CallChangeState(pData, PhoneStateTypes.Off);
            }
        }

        public static void CallChangeState(PlayerData pData, PhoneStateTypes stateType)
        {
            if (pData.PhoneStateType == stateType)
                return;

            Events.CallRemote("Players::SPST", stateType);
        }

        public static async void SetState(Player player, PhoneStateTypes stateType)
        {
            if (player.Handle == Player.LocalPlayer.Handle)
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

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

                        UI.CEF.Phone.Phone.Close();

                        Camera.Close();

                        if (_currentTask != null)
                        {
                            _currentTask.Cancel();

                            _currentTask = null;
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

                        UI.CEF.Phone.Phone.Show();

                        _currentTask?.Cancel();

                        var lastSyncedType = stateType;

                        _currentTask = new AsyncTask(() =>
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
                                else if (Camera.IsActive)
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
                                Camera.Close();

                                if (lastSyncedType != PhoneStateTypes.JustOn)
                                {
                                    Events.CallRemote("Players::SPST", PhoneStateTypes.JustOn);

                                    lastSyncedType = PhoneStateTypes.JustOn;
                                }

                            }
                        }, 100, true, 0);

                        _currentTask.Run();
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
                    await Streaming.RequestAnimDict(AnimDict);

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
                    await Streaming.RequestAnimDict(AnimDictSelf);

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
            RAGE.Game.Mobile.MoveFinger(Utils.Misc.Random.Next(1, 6));
        }

        public static bool IsLocalPhoneActive => Player.LocalPlayer.GetSelectedWeapon() == Game.Management.Weapons.Core.MobileHash;
    }
}
