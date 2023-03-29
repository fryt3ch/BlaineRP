using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;

namespace BCRPClient.CEF.PhoneApps
{
    public class CameraApp : Events.Script
    {
        public static bool IsActive { get; private set; }

        public static bool FrontCamActive { get; private set; }

        public static bool BokehModeActive { get; private set; }

        public static Additional.Scaleform Scaleform { get; private set; }

        private static DateTime LastSwitched;

        private static uint RenderTicks { get; set; }

        private static int CurrentCameraFilter { get; set; }
        private static int CurrentCameraAnimation { get; set; }

        private static string CurrentCameraAnimationDict { get; set; }
        private static string CurrentCameraAnimationName { get; set; }

        private static float CurrentHorizontalOffset { get; set; }
        private static float CurrentVerticalOffset { get; set; }
        private static float CurrentRoll { get; set; }
        private static float CurrentHeadPitch { get; set; }
        private static float CurrentHeadRoll { get; set; }
        private static float CurrentHeadHeight { get; set; }

        private static byte PhotoStartCounter { get; set; }

        private static bool r { get; set; }

        private static Sync.Animations.EmotionTypes CurrentCameraEmotion { get; set; }

        private static string[] CameraFilters { get; set; } = new string[] { "NG_filmic01", "NG_filmic02", "NG_filmic03", "NG_filmic04", "NG_filmic05", "NG_filmic06", "NG_filmic07", "NG_filmic08", "NG_filmic09", "NG_filmic10", "NG_filmic11", "NG_filmic12", "NG_filmic13", "NG_filmic14", "NG_filmic15", "NG_filmic16", "NG_filmic17", "NG_filmic18", "NG_filmic19", "NG_filmic20", "NG_filmic21", "NG_filmic22", "NG_filmic23", "NG_filmic24", "NG_filmic25" };

        private static (string Dict, string Name, string LocaleName)[] CameraAnims = new (string, string, string)[]
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

        public CameraApp()
        {

        }

        public static void Show()
        {
            if (IsActive)
                return;

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            Sync.Crouch.Off(false, Player.LocalPlayer);

            PhotoStartCounter = 0;

            CurrentCameraFilter = -1;
            CurrentCameraAnimation = -1;
            CurrentCameraEmotion = pData.Emotion;

            CurrentCameraAnimationDict = "";
            CurrentCameraAnimationName = "";

            CurrentHorizontalOffset = 0f;
            CurrentVerticalOffset = 1f;

            CurrentRoll = 0f;
            CurrentHeadPitch = 0f;
            CurrentHeadRoll = 0f;
            CurrentHeadHeight = 0f;

            r = false;

            RenderTicks = 0;

            LastSwitched = Sync.World.ServerTime;

            KeyBinds.DisableAll(KeyBinds.Types.MicrophoneOn);

            CEF.Phone.Close();

            CEF.HUD.ShowHUD(false);

            CEF.Chat.Show(false);

            IsActive = true;

            Sync.Phone.CreateLocalPhone();

            RAGE.Game.Mobile.CellCamActivate(true, true);

            ToggleFrontCam(FrontCamActive);
            ToggleBokehMode(BokehModeActive = false);

            Player.LocalPlayer.SetConfigFlag(242, true);
            Player.LocalPlayer.SetConfigFlag(243, true);
            Player.LocalPlayer.SetConfigFlag(244, false);

            Scaleform = new Additional.Scaleform("phone_camera_instbtns", "instructional_buttons");

            UpdateInstructionButtons();

            GameEvents.Render -= OnRender;
            GameEvents.Render += OnRender;
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            GameEvents.Render -= OnRender;

            KeyBinds.EnableAll();

            if (!Settings.Interface.HideHUD)
                CEF.HUD.ShowHUD(true);

            CEF.Chat.Show(true);

            if (Scaleform != null)
            {
                Scaleform.Destroy();

                Scaleform = null;
            }

            RAGE.Game.Mobile.CellCamActivate(false, false);

            Player.LocalPlayer.SetConfigFlag(242, false);
            Player.LocalPlayer.SetConfigFlag(243, false);
            Player.LocalPlayer.SetConfigFlag(244, true);

            RAGE.Game.Graphics.ClearTimecycleModifier();

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData != null)
            {
                Sync.Animations.Set(Player.LocalPlayer, pData.Emotion);
            }

            if (CurrentCameraAnimationDict.Length > 0)
            {

            }

            if (Sync.Phone.Toggled)
                CEF.Phone.Show();

            IsActive = false;
        }

        private static async void OnRender()
        {
            CEF.Cursor.OnTickCursor();

            RAGE.Game.Pad.DisableControlAction(32, 44, true); // disable Q

            Additional.ExtraColshape.InteractionColshapesDisabledThisFrame = true;

            if (RenderTicks == uint.MaxValue)
                RenderTicks = 0;
            else
                RenderTicks++;

            if (RAGE.Game.Pad.IsDisabledControlJustPressed(32, 200))
            {
                Close();

                return;
            }

            if (KeyBinds.IsDown(RAGE.Ui.VirtualKeys.Return) && !LastSwitched.IsSpam(750, false, false) && PhotoStartCounter == 0)
            {
                PhotoStartCounter = 1;

                LastSwitched = Sync.World.ServerTime;

                return;
            }

            if (PhotoStartCounter == 0)
            {
                Scaleform?.Render2D();
            }
            else
            {
                PhotoStartCounter++;

                if (PhotoStartCounter == 5)
                {
                    SavePicture(true, true, true);

                    PhotoStartCounter = 0;

                    return;
                }

                return;
            }

            if (RAGE.Game.Pad.IsControlJustPressed(32, 0))
            {
                if (LastSwitched.IsSpam(500, false, false))
                    return;

                ToggleFrontCam(FrontCamActive = !FrontCamActive);

                UpdateInstructionButtons();

                LastSwitched = Sync.World.ServerTime;

                return;
            }

            if (FrontCamActive)
            {
                if (true)
                {
                    var cameraPos = RAGE.Game.Cam.GetGameplayCamCoord();
                    var playerPos = Player.LocalPlayer.Position;

                    RAGE.Elements.Entities.Players.Streamed.ForEach(x =>
                    {
                        if (x == Player.LocalPlayer)
                            return;

                        if (!x.IsOnScreen())
                            return;

                        x.TaskLookAtCoord2(cameraPos.X, cameraPos.Y, cameraPos.Z, 2000, 2048, 3);
                    });
                }

                if (RAGE.Game.Pad.IsDisabledControlPressed(32, 69) || RAGE.Game.Pad.IsDisabledControlPressed(32, 68))
                {
                    RAGE.Game.Pad.DisableControlAction(32, 1, true);
                    RAGE.Game.Pad.DisableControlAction(32, 2, true);

                    var normalA = RAGE.Game.Pad.GetDisabledControlNormal(32, 1) / 20f;
                    var normalB = -RAGE.Game.Pad.GetDisabledControlNormal(32, 2) / 20f;
                    var normalC = RAGE.Game.Pad.GetDisabledControlNormal(32, 30) / 12f;

                    if (RAGE.Game.Pad.IsDisabledControlPressed(32, 69))
                    {
                        CurrentHorizontalOffset = Utils.GetLimitedValue(CurrentHorizontalOffset + normalA, 0f, 1f);
                        CurrentVerticalOffset = Utils.GetLimitedValue(CurrentVerticalOffset + normalB, 0f, 2f);
                        CurrentRoll = Utils.GetLimitedValue(CurrentRoll + normalC, -1f, 2f);
                    }
                    else
                    {
                        CurrentHeadPitch = Utils.GetLimitedValue(CurrentHeadPitch + normalA, -1f, 1f);
                        CurrentHeadRoll = Utils.GetLimitedValue(CurrentHeadRoll + normalC, -1f, 1f);
                        CurrentHeadHeight = Utils.GetLimitedValue(CurrentHeadHeight + normalB, -1f, 1f);
                    }
                }

                RAGE.Game.Invoker.Invoke(0x1B0B4AEED5B9B41C, CurrentHorizontalOffset); // CellCamSetHorizontalOffset
                RAGE.Game.Invoker.Invoke(0x3117D84EFA60F77B, CurrentVerticalOffset); // CellCamSetVerticalOffset
                RAGE.Game.Invoker.Invoke(0x15E69E2802C24B8D, CurrentRoll); // CellCamSetRoll
                RAGE.Game.Invoker.Invoke(0xD6ADE981781FCA09, CurrentHeadPitch); // CellCamSetHeadPitch
                RAGE.Game.Invoker.Invoke(0xF1E22DC13F5EEBAD, CurrentHeadRoll); // CellCamSetHeadRoll
                RAGE.Game.Invoker.Invoke(0x466DA42C89865553, CurrentHeadHeight); // CellCamSetHeadHeight

                var animAction = RAGE.Game.Pad.IsControlPressed(32, 61);

                if (animAction && !r)
                {
                    if (CurrentCameraAnimation >= 0)
                    {
                        var anim = CameraAnims[CurrentCameraAnimation];

                        r = true;

                        await Utils.RequestAnimDict(anim.Dict);

                        if (!IsActive)
                            return;

                        if (anim.Name != null)
                        {
                            Player.LocalPlayer.TaskPlayAnim(anim.Dict, anim.Name, 4f, 4f, -1, 128, -1f, false, false, false);

                            CurrentCameraAnimationDict = anim.Dict;
                            CurrentCameraAnimationName = anim.Name;
                        }
                        else
                        {
                            Player.LocalPlayer.TaskPlayAnim(anim.Dict, "enter", 4f, 4f, -1, 128, -1f, false, false, false);

                            await RAGE.Game.Invoker.WaitAsync((int)Math.Floor(1000 * RAGE.Game.Entity.GetAnimDuration(anim.Dict, anim.Name)));

                            if (!IsActive)
                                return;

                            Player.LocalPlayer.TaskPlayAnim(anim.Dict, "idle_a", 8f, 4f, -1, 129, -1f, false, false, false);

                            CurrentCameraAnimationDict = anim.Dict;
                            CurrentCameraAnimationName = "";
                        }
                    }
                }
                else if (r && !animAction)
                {
                    r = false;

                    if (CurrentCameraAnimationDict.Length > 0 && CurrentCameraAnimationName.Length == 0)
                    {
                        Player.LocalPlayer.TaskPlayAnim(CurrentCameraAnimationDict, "exit", 4f, 4f, -1, 128, -1f, false, false, false);

                        await RAGE.Game.Invoker.WaitAsync((int)Math.Floor(1000 * RAGE.Game.Entity.GetAnimDuration(CurrentCameraAnimationDict, "exit")));

                        if (!IsActive)
                            return;

                        Player.LocalPlayer.TaskPlayAnim("", "", 4f, 4f, -1, 128, -1f, false, false, false);
                    }
                    else
                    {
                        Player.LocalPlayer.StopAnimTask(CurrentCameraAnimationDict, CurrentCameraAnimationName, 3f);

                        Player.LocalPlayer.TaskPlayAnim("", "", 4f, 4f, -1, 128, -1f, false, false, false);
                    }
                }

                if (RAGE.Game.Pad.IsControlJustPressed(32, 73))
                {
                    ToggleBokehMode(BokehModeActive = !BokehModeActive);

                    UpdateInstructionButtons();

                    return;
                }

                if (RAGE.Game.Pad.IsControlJustPressed(32, 89) || RAGE.Game.Pad.IsControlJustPressed(32, 90))
                {
                    var curCamEmotionNum = (int)CurrentCameraEmotion + (RAGE.Game.Pad.IsControlJustPressed(32, 89) ? -1 : 1);

                    if (!Enum.IsDefined(typeof(Sync.Animations.EmotionTypes), curCamEmotionNum))
                    {
                        if (curCamEmotionNum < 0)
                            curCamEmotionNum = (int)Sync.Animations.EmotionTypes.Electrocuted;
                        else
                            curCamEmotionNum = -1;
                    }

                    CurrentCameraEmotion = (Sync.Animations.EmotionTypes)curCamEmotionNum;

                    Sync.Animations.Set(Player.LocalPlayer, CurrentCameraEmotion);

                    UpdateInstructionButtons();

                    return;
                }

                if (RAGE.Game.Pad.IsControlJustPressed(32, 187) || RAGE.Game.Pad.IsControlJustPressed(32, 188))
                {
                    CurrentCameraAnimation += RAGE.Game.Pad.IsControlJustPressed(32, 187) ? -1 : 1;

                    if (CurrentCameraAnimation < -1)
                        CurrentCameraAnimation = CameraAnims.Length - 1;
                    else if (CurrentCameraAnimation >= CameraAnims.Length)
                        CurrentCameraAnimation = -1;

                    UpdateInstructionButtons();
                }
            }

            if (RAGE.Game.Pad.IsControlJustPressed(32, 189) || RAGE.Game.Pad.IsControlJustPressed(32, 190))
            {
                CurrentCameraFilter += RAGE.Game.Pad.IsControlJustPressed(32, 189) ? -1 : 1;

                if (CurrentCameraFilter < -1)
                    CurrentCameraFilter = CameraFilters.Length - 1;
                else if (CurrentCameraFilter >= CameraFilters.Length)
                    CurrentCameraFilter = -1;

                if (CurrentCameraFilter < 0)
                {
                    RAGE.Game.Graphics.ClearTimecycleModifier();
                }
                else
                {
                    RAGE.Game.Graphics.SetTimecycleModifierStrength(1f);
                    RAGE.Game.Graphics.SetTimecycleModifier(CameraFilters[CurrentCameraFilter]);
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
            var curDateStr = Sync.World.ServerTime.ToString("dd_MM_yyyy_HH_mm_ss_ff");

            var fileName = isCam ? $"CAM_{curDateStr}.png" : $"{curDateStr}.png";

            RAGE.Input.TakeScreenshot(fileName, 1, 100f, 0f);

            if (sound)
            {
                RAGE.Game.Audio.PlaySoundFrontend(-1, "Camera_Shoot", "Phone_SoundSet_Michael", true);
            }

            if (notify)
            {
                CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Notifications.DefHeader, string.Format(Locale.Notifications.General.ScreenshotSaved, fileName));
            }
        }

        private static void UpdateInstructionButtons()
        {
            if (Scaleform == null)
                return;

            Scaleform.CallFunction("CLEAR_ALL");
            Scaleform.CallFunction("TOGGLE_MOUSE_BUTTONS", 0);
            Scaleform.CallFunction("CREATE_CONTAINER");
            Scaleform.CallFunction("SET_CLEAR_SPACE", 200);

            var btnsCount = 0;

            if (FrontCamActive)
            {
                Scaleform.CallFunction("SET_DATA_SLOT", btnsCount++, RAGE.Game.Pad.GetControlInstructionalButton(32, 73, true), $"{Locale.General.PhoneCamera.Bokeh} [{(BokehModeActive ? Locale.General.PhoneCamera.On : Locale.General.PhoneCamera.Off)}]");
                Scaleform.CallFunction("SET_DATA_SLOT", btnsCount++, RAGE.Game.Pad.GetControlInstructionalButton(32, 69, true), Locale.General.PhoneCamera.CamOffset);
                Scaleform.CallFunction("SET_DATA_SLOT", btnsCount++, RAGE.Game.Pad.GetControlInstructionalButton(32, 68, true), Locale.General.PhoneCamera.HeadOffset);

                if (CurrentCameraAnimation >= 0)
                    Scaleform.CallFunction("SET_DATA_SLOT", btnsCount++, RAGE.Game.Pad.GetControlInstructionalButton(0, 61, true), $"{CameraAnims[CurrentCameraAnimation].LocaleName}");

                Scaleform.CallFunction("SET_DATA_SLOT", btnsCount++, $"{RAGE.Game.Pad.GetControlInstructionalButton(32, 188, true)}%{RAGE.Game.Pad.GetControlInstructionalButton(32, 187, true)}", Locale.General.PhoneCamera.Animation);
                Scaleform.CallFunction("SET_DATA_SLOT", btnsCount++, $"{RAGE.Game.Pad.GetControlInstructionalButton(32, 90, true)}%{RAGE.Game.Pad.GetControlInstructionalButton(32, 89, true)}", $"{Locale.General.PhoneCamera.Emotion} [{Locale.General.Animations.Emotions.GetValueOrDefault(CurrentCameraEmotion) ?? "null"}]");
            }
            else
            {
                Scaleform.CallFunction("SET_DATA_SLOT", btnsCount++, $"{RAGE.Game.Pad.GetControlInstructionalButton(32, 181, true)}%{RAGE.Game.Pad.GetControlInstructionalButton(32, 180, true)}", Locale.General.PhoneCamera.Zoom);
            }

            Scaleform.CallFunction("SET_DATA_SLOT", btnsCount++, $"{RAGE.Game.Pad.GetControlInstructionalButton(32, 190, true)}%{RAGE.Game.Pad.GetControlInstructionalButton(32, 189, true)}", $"{Locale.General.PhoneCamera.Filter} [{(CurrentCameraFilter < 0 ? Locale.General.PhoneCamera.Off : (CurrentCameraFilter + 1).ToString())}]");

            Scaleform.CallFunction("SET_DATA_SLOT", btnsCount++, RAGE.Game.Pad.GetControlInstructionalButton(32, 0, true), FrontCamActive ? Locale.General.PhoneCamera.BackCam : Locale.General.PhoneCamera.FrontCam);
            Scaleform.CallFunction("SET_DATA_SLOT", btnsCount++, "w_Enter", Locale.General.PhoneCamera.Photo);
            Scaleform.CallFunction("SET_DATA_SLOT", btnsCount++, RAGE.Game.Pad.GetControlInstructionalButton(32, 200, true), Locale.General.PhoneCamera.Exit);

            Scaleform.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1);
        }
    }
}
