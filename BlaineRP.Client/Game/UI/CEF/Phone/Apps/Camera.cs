using System;
using System.Collections.Generic;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Animations;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Game.Helpers;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Input.Enums;
using BlaineRP.Client.Game.Scripts.Misc;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF.Phone.Apps
{
    [Script(int.MaxValue)]
    public class Camera
    {
        private static Scaleform _scaleform;

        private static DateTime _lastSwitched;

        private static uint _renderTicks;

        private static int _currentFilter;
        private static int _currentAnimationIdx;

        private static string _currentAnimationDict;
        private static string _currentAnimationName;

        private static float _currentHorizontalOffset;
        private static float _currentVerticalOffset;
        private static float _currentRoll;
        private static float _currentHeadPitch;
        private static float _currentHeadRoll;
        private static float _currentHeadHeight;

        private static byte _photoStartCounter;

        private static bool _isPlayingAnimFlag;

        private static EmotionType _currentCameraEmotion;

        private static string[] _cameraFilters = new string[]
        {
            "NG_filmic01",
            "NG_filmic02",
            "NG_filmic03",
            "NG_filmic04",
            "NG_filmic05",
            "NG_filmic06",
            "NG_filmic07",
            "NG_filmic08",
            "NG_filmic09",
            "NG_filmic10",
            "NG_filmic11",
            "NG_filmic12",
            "NG_filmic13",
            "NG_filmic14",
            "NG_filmic15",
            "NG_filmic16",
            "NG_filmic17",
            "NG_filmic18",
            "NG_filmic19",
            "NG_filmic20",
            "NG_filmic21",
            "NG_filmic22",
            "NG_filmic23",
            "NG_filmic24",
            "NG_filmic25",
        };

        private static (string Dict, string Name, string LocaleName)[] _cameraAnims = new (string, string, string)[]
        {
            ("cellphone@self@franklin@", "chest_bump", "Кулак у груди"),
            ("cellphone@self@franklin@", "peace", "Мир"),
            ("cellphone@self@franklin@", "west_coast", "Распальцовка"),
            ("cellphone@self@michael@", "finger_point", "Палец в камеру"),
            ("cellphone@self@michael@", "run_chin", "Поправлять лицо"),
            ("cellphone@self@michael@", "stretch_neck", "Разминать шею"),
            ("cellphone@self@trevor@", "aggressive_finger", "Средний палец"),
            ("cellphone@self@trevor@", "proud_finger", "Средний палец #2"),
            ("anim@mp_player_intselfiethe_bird", null, "Средний палец #3"),
            ("cellphone@self@trevor@", "throat_slit", "Перерезать глотку"),
            ("anim@mp_player_intselfieblow_kiss", null, "Воздушный поцелуй"),
            ("anim@mp_player_intselfiedock", null, "ОК"),
            ("anim@mp_player_intselfiejazz_hands", null, "Махать руками"),
            ("anim@mp_player_intselfiethumbs_up", null, "Большой палец вверх"),
            ("anim@mp_player_intselfiewank", null, "Дергать рукой"),
        };

        public Camera()
        {
        }

        public static bool IsActive { get; private set; }

        public static bool FrontCamIsActive { get; private set; }

        public static bool BokehModeIsActive { get; private set; }

        public static void Show()
        {
            if (IsActive)
                return;

            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            Crouch.Off(false, Player.LocalPlayer);

            _photoStartCounter = 0;

            _currentFilter = -1;
            _currentAnimationIdx = -1;
            _currentCameraEmotion = pData.Emotion;

            _currentAnimationDict = "";
            _currentAnimationName = "";

            _currentHorizontalOffset = 0f;
            _currentVerticalOffset = 1f;

            _currentRoll = 0f;
            _currentHeadPitch = 0f;
            _currentHeadRoll = 0f;
            _currentHeadHeight = 0f;

            _isPlayingAnimFlag = false;

            _renderTicks = 0;

            _lastSwitched = World.Core.ServerTime;

            Input.Core.DisableAll(BindTypes.MicrophoneOn, BindTypes.MicrophoneOff);

            CEF.Phone.Phone.Close();

            HUD.ShowHUD(false);

            Chat.Show(false);

            IsActive = true;

            Game.Scripts.Misc.Phone.CreateLocalPhone();

            RAGE.Game.Mobile.CellCamActivate(true, true);

            ToggleFrontCam(FrontCamIsActive);
            ToggleBokehMode(BokehModeIsActive = false);

            Player.LocalPlayer.SetConfigFlag(242, true);
            Player.LocalPlayer.SetConfigFlag(243, true);
            Player.LocalPlayer.SetConfigFlag(244, false);

            _scaleform = new Scaleform("phone_camera_instbtns", "instructional_buttons");

            UpdateInstructionButtons();

            Main.Render -= OnRender;
            Main.Render += OnRender;
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            Main.Render -= OnRender;

            Input.Core.EnableAll();

            if (!BlaineRP.Client.Settings.User.Interface.HideHUD)
                HUD.ShowHUD(true);

            Chat.Show(true);

            if (_scaleform != null)
            {
                _scaleform.Destroy();

                _scaleform = null;
            }

            RAGE.Game.Mobile.CellCamActivate(false, false);

            Player.LocalPlayer.SetConfigFlag(242, false);
            Player.LocalPlayer.SetConfigFlag(243, false);
            Player.LocalPlayer.SetConfigFlag(244, true);

            RAGE.Game.Graphics.ClearTimecycleModifier();

            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData != null)
                Service.Set(Player.LocalPlayer, pData.Emotion);

            if (_currentAnimationDict.Length > 0)
            {
            }

            if (Game.Scripts.Misc.Phone.Toggled)
                CEF.Phone.Phone.Show();

            IsActive = false;
        }

        private static async void OnRender()
        {
            Cursor.OnTickCursor();

            RAGE.Game.Pad.DisableControlAction(32, 44, true); // disable Q

            ExtraColshape.InteractionColshapesDisabledThisFrame = true;

            if (_renderTicks == uint.MaxValue)
                _renderTicks = 0;
            else
                _renderTicks++;

            if (RAGE.Game.Pad.IsDisabledControlJustPressed(32, 200))
            {
                Close();

                return;
            }

            if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.Return) && !_lastSwitched.IsSpam(750, false, false) && _photoStartCounter == 0)
            {
                _photoStartCounter = 1;

                _lastSwitched = World.Core.ServerTime;

                return;
            }

            if (_photoStartCounter == 0)
            {
                _scaleform?.Render2D();
            }
            else
            {
                _photoStartCounter++;

                if (_photoStartCounter == 5)
                {
                    SavePicture(true, true, true);

                    _photoStartCounter = 0;

                    return;
                }

                return;
            }

            if (RAGE.Game.Pad.IsControlJustPressed(32, 0))
            {
                if (_lastSwitched.IsSpam(500, false, false))
                    return;

                ToggleFrontCam(FrontCamIsActive = !FrontCamIsActive);

                UpdateInstructionButtons();

                _lastSwitched = World.Core.ServerTime;

                return;
            }

            if (FrontCamIsActive)
            {
                if (true)
                {
                    Vector3 cameraPos = RAGE.Game.Cam.GetGameplayCamCoord();
                    Vector3 playerPos = Player.LocalPlayer.Position;

                    Entities.Players.Streamed.ForEach(x =>
                        {
                            if (x == Player.LocalPlayer)
                                return;

                            if (!x.IsOnScreen())
                                return;

                            x.TaskLookAtCoord2(cameraPos.X, cameraPos.Y, cameraPos.Z, 2000, 2048, 3);
                        }
                    );
                }

                if (RAGE.Game.Pad.IsDisabledControlPressed(32, 69) || RAGE.Game.Pad.IsDisabledControlPressed(32, 68))
                {
                    RAGE.Game.Pad.DisableControlAction(32, 1, true);
                    RAGE.Game.Pad.DisableControlAction(32, 2, true);

                    float normalA = RAGE.Game.Pad.GetDisabledControlNormal(32, 1) / 20f;
                    float normalB = -RAGE.Game.Pad.GetDisabledControlNormal(32, 2) / 20f;
                    float normalC = RAGE.Game.Pad.GetDisabledControlNormal(32, 30) / 12f;

                    if (RAGE.Game.Pad.IsDisabledControlPressed(32, 69))
                    {
                        _currentHorizontalOffset = Utils.Math.GetLimitedValue(_currentHorizontalOffset + normalA, 0f, 1f);
                        _currentVerticalOffset = Utils.Math.GetLimitedValue(_currentVerticalOffset + normalB, 0f, 2f);
                        _currentRoll = Utils.Math.GetLimitedValue(_currentRoll + normalC, -1f, 2f);
                    }
                    else
                    {
                        _currentHeadPitch = Utils.Math.GetLimitedValue(_currentHeadPitch + normalA, -1f, 1f);
                        _currentHeadRoll = Utils.Math.GetLimitedValue(_currentHeadRoll + normalC, -1f, 1f);
                        _currentHeadHeight = Utils.Math.GetLimitedValue(_currentHeadHeight + normalB, -1f, 1f);
                    }
                }

                RAGE.Game.Invoker.Invoke(0x1B0B4AEED5B9B41C, _currentHorizontalOffset); // CellCamSetHorizontalOffset
                RAGE.Game.Invoker.Invoke(0x3117D84EFA60F77B, _currentVerticalOffset); // CellCamSetVerticalOffset
                RAGE.Game.Invoker.Invoke(0x15E69E2802C24B8D, _currentRoll); // CellCamSetRoll
                RAGE.Game.Invoker.Invoke(0xD6ADE981781FCA09, _currentHeadPitch); // CellCamSetHeadPitch
                RAGE.Game.Invoker.Invoke(0xF1E22DC13F5EEBAD, _currentHeadRoll); // CellCamSetHeadRoll
                RAGE.Game.Invoker.Invoke(0x466DA42C89865553, _currentHeadHeight); // CellCamSetHeadHeight

                bool animAction = RAGE.Game.Pad.IsControlPressed(32, 61);

                if (animAction && !_isPlayingAnimFlag)
                {
                    if (_currentAnimationIdx >= 0)
                    {
                        (string Dict, string Name, string LocaleName) anim = _cameraAnims[_currentAnimationIdx];

                        _isPlayingAnimFlag = true;

                        await Streaming.RequestAnimDict(anim.Dict);

                        if (!IsActive)
                            return;

                        if (anim.Name != null)
                        {
                            Player.LocalPlayer.TaskPlayAnim(anim.Dict, anim.Name, 4f, 4f, -1, 128, -1f, false, false, false);

                            _currentAnimationDict = anim.Dict;
                            _currentAnimationName = anim.Name;
                        }
                        else
                        {
                            Player.LocalPlayer.TaskPlayAnim(anim.Dict, "enter", 4f, 4f, -1, 128, -1f, false, false, false);

                            await RAGE.Game.Invoker.WaitAsync((int)Math.Floor(1000 * RAGE.Game.Entity.GetAnimDuration(anim.Dict, anim.Name)));

                            if (!IsActive)
                                return;

                            Player.LocalPlayer.TaskPlayAnim(anim.Dict, "idle_a", 8f, 4f, -1, 129, -1f, false, false, false);

                            _currentAnimationDict = anim.Dict;
                            _currentAnimationName = "";
                        }
                    }
                }
                else if (_isPlayingAnimFlag && !animAction)
                {
                    _isPlayingAnimFlag = false;

                    if (_currentAnimationDict.Length > 0 && _currentAnimationName.Length == 0)
                    {
                        Player.LocalPlayer.TaskPlayAnim(_currentAnimationDict, "exit", 4f, 4f, -1, 128, -1f, false, false, false);

                        await RAGE.Game.Invoker.WaitAsync((int)Math.Floor(1000 * RAGE.Game.Entity.GetAnimDuration(_currentAnimationDict, "exit")));

                        if (!IsActive)
                            return;

                        Player.LocalPlayer.TaskPlayAnim("", "", 4f, 4f, -1, 128, -1f, false, false, false);
                    }
                    else
                    {
                        Player.LocalPlayer.StopAnimTask(_currentAnimationDict, _currentAnimationName, 3f);

                        Player.LocalPlayer.TaskPlayAnim("", "", 4f, 4f, -1, 128, -1f, false, false, false);
                    }
                }

                if (RAGE.Game.Pad.IsControlJustPressed(32, 73))
                {
                    ToggleBokehMode(BokehModeIsActive = !BokehModeIsActive);

                    UpdateInstructionButtons();

                    return;
                }

                if (RAGE.Game.Pad.IsControlJustPressed(32, 89) || RAGE.Game.Pad.IsControlJustPressed(32, 90))
                {
                    int curCamEmotionNum = (int)_currentCameraEmotion + (RAGE.Game.Pad.IsControlJustPressed(32, 89) ? -1 : 1);

                    if (!Enum.IsDefined(typeof(EmotionType), curCamEmotionNum))
                    {
                        if (curCamEmotionNum < 0)
                            curCamEmotionNum = (int)EmotionType.Electrocuted;
                        else
                            curCamEmotionNum = -1;
                    }

                    _currentCameraEmotion = (EmotionType)curCamEmotionNum;

                    Service.Set(Player.LocalPlayer, _currentCameraEmotion);

                    UpdateInstructionButtons();

                    return;
                }

                if (RAGE.Game.Pad.IsControlJustPressed(32, 187) || RAGE.Game.Pad.IsControlJustPressed(32, 188))
                {
                    _currentAnimationIdx += RAGE.Game.Pad.IsControlJustPressed(32, 187) ? -1 : 1;

                    if (_currentAnimationIdx < -1)
                        _currentAnimationIdx = _cameraAnims.Length - 1;
                    else if (_currentAnimationIdx >= _cameraAnims.Length)
                        _currentAnimationIdx = -1;

                    UpdateInstructionButtons();
                }
            }

            if (RAGE.Game.Pad.IsControlJustPressed(32, 189) || RAGE.Game.Pad.IsControlJustPressed(32, 190))
            {
                _currentFilter += RAGE.Game.Pad.IsControlJustPressed(32, 189) ? -1 : 1;

                if (_currentFilter < -1)
                    _currentFilter = _cameraFilters.Length - 1;
                else if (_currentFilter >= _cameraFilters.Length)
                    _currentFilter = -1;

                if (_currentFilter < 0)
                {
                    RAGE.Game.Graphics.ClearTimecycleModifier();
                }
                else
                {
                    RAGE.Game.Graphics.SetTimecycleModifierStrength(1f);
                    RAGE.Game.Graphics.SetTimecycleModifier(_cameraFilters[_currentFilter]);
                }

                UpdateInstructionButtons();

                return;
            }
        }

        private static void ToggleBokehMode(bool state)
        {
            RAGE.Game.Invoker.Invoke(0xA2CCBE62CD4C91A4, RAGE.Game.Invoker.Invoke<int>(0x375A706A5C2FD084, state)); // N_0xa2ccbe62cd4c91a4, SetMobilePhoneUnk
        }

        private static void ToggleFrontCam(bool state)
        {
            RAGE.Game.Invoker.Invoke(0x015C49A93E3E086E, state); // CellCamDisableThisFrame
        }

        public static void SavePicture(bool isCam, bool sound, bool notify)
        {
            var curDateStr = World.Core.ServerTime.ToString("dd_MM_yyyy_HH_mm_ss_ff");

            string fileName = isCam ? $"CAM_{curDateStr}.png" : $"{curDateStr}.png";

            RAGE.Input.TakeScreenshot(fileName, 1, 100f, 0f);

            if (sound)
                RAGE.Game.Audio.PlaySoundFrontend(-1, "Camera_Shoot", "Phone_SoundSet_Michael", true);

            if (notify)
                Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), string.Format(Locale.Notifications.General.ScreenshotSaved, fileName));
        }

        private static void UpdateInstructionButtons()
        {
            if (_scaleform == null)
                return;

            _scaleform.CallFunction("CLEAR_ALL");
            _scaleform.CallFunction("TOGGLE_MOUSE_BUTTONS", 0);
            _scaleform.CallFunction("CREATE_CONTAINER");
            _scaleform.CallFunction("SET_CLEAR_SPACE", 200);

            var btnsCount = 0;

            if (FrontCamIsActive)
            {
                _scaleform.CallFunction("SET_DATA_SLOT",
                    btnsCount++,
                    RAGE.Game.Pad.GetControlInstructionalButton(32, 73, true),
                    $"{Locale.Get("PHONE_CAMERA_BOKEH_0")} [{(BokehModeIsActive ? Locale.Get("GEN_ON_1") : Locale.Get("GEN_OFF_1"))}]"
                );
                _scaleform.CallFunction("SET_DATA_SLOT", btnsCount++, RAGE.Game.Pad.GetControlInstructionalButton(32, 69, true), Locale.Get("PHONE_CAMERA_CAM_OFFSET_0"));
                _scaleform.CallFunction("SET_DATA_SLOT", btnsCount++, RAGE.Game.Pad.GetControlInstructionalButton(32, 68, true), Locale.Get("PHONE_CAMERA_HEAD_OFFSET_0"));

                if (_currentAnimationIdx >= 0)
                    _scaleform.CallFunction("SET_DATA_SLOT",
                        btnsCount++,
                        RAGE.Game.Pad.GetControlInstructionalButton(0, 61, true),
                        $"{_cameraAnims[_currentAnimationIdx].LocaleName}"
                    );

                _scaleform.CallFunction("SET_DATA_SLOT",
                    btnsCount++,
                    $"{RAGE.Game.Pad.GetControlInstructionalButton(32, 188, true)}%{RAGE.Game.Pad.GetControlInstructionalButton(32, 187, true)}",
                    Locale.Get("PHONE_CAMERA_ANIMATION_0")
                );
                _scaleform.CallFunction("SET_DATA_SLOT",
                    btnsCount++,
                    $"{RAGE.Game.Pad.GetControlInstructionalButton(32, 90, true)}%{RAGE.Game.Pad.GetControlInstructionalButton(32, 89, true)}",
                    $"{Locale.Get("PHONE_CAMERA_EMOTION_0")} [{Locale.General.Animations.Emotions.GetValueOrDefault(_currentCameraEmotion) ?? "null"}]"
                );
            }
            else
            {
                _scaleform.CallFunction("SET_DATA_SLOT",
                    btnsCount++,
                    $"{RAGE.Game.Pad.GetControlInstructionalButton(32, 181, true)}%{RAGE.Game.Pad.GetControlInstructionalButton(32, 180, true)}",
                    Locale.Get("PHONE_CAMERA_ZOOM_0")
                );
            }

            _scaleform.CallFunction("SET_DATA_SLOT",
                btnsCount++,
                $"{RAGE.Game.Pad.GetControlInstructionalButton(32, 190, true)}%{RAGE.Game.Pad.GetControlInstructionalButton(32, 189, true)}",
                $"{Locale.Get("PHONE_CAMERA_FILTER_0")} [{(_currentFilter < 0 ? Locale.Get("GEN_OFF_1") : (_currentFilter + 1).ToString())}]"
            );

            _scaleform.CallFunction("SET_DATA_SLOT",
                btnsCount++,
                RAGE.Game.Pad.GetControlInstructionalButton(32, 0, true),
                FrontCamIsActive ? Locale.Get("PHONE_CAMERA_CAM_BACK_0") : Locale.Get("PHONE_CAMERA_CAM_FRONT_0")
            );
            _scaleform.CallFunction("SET_DATA_SLOT", btnsCount++, "w_Enter", Locale.Get("PHONE_CAMERA_SHOOT_0"));
            _scaleform.CallFunction("SET_DATA_SLOT", btnsCount++, RAGE.Game.Pad.GetControlInstructionalButton(32, 200, true), Locale.Get("PHONE_CAMERA_EXIT_0"));

            _scaleform.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1);
        }
    }
}