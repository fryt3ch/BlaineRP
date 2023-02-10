using RAGE;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.CEF.PhoneApps
{
    public class CameraApp : Events.Script
    {
        public CameraApp()
        {

        }

        public static bool CanShow()
        {
            return true;
        }

        public static void Show()
        {
            CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, "Приложение еще не готово :(");
        }

        public static void Close()
        {

        }

        public static void SavePicture(bool isCam, bool sound, bool notify)
        {
            var curDateStr = Utils.GetServerTime().ToString("dd_MM_yyyy_HH_mm_ss_ff");

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
    }
}
