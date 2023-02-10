using RAGE;
using System;
using static BCRPClient.CEF.Phone;

namespace BCRPClient.CEF.PhoneApps
{
    public class SettingsApp : Events.Script
    {
        public SettingsApp()
        {
            Events.Add("Phone::UpdateWallpaper", (args) =>
            {
                if (LastSent.IsSpam(500, false, false))
                    return;

                var wpNum = (int)args[0];

                if (Settings.Other.PhoneWallpaperNum == wpNum)
                {
                    return;
                }

                if (CurrentApp == AppTypes.Settings)
                {
                    if (CurrentAppTab == "wallpaper".GetHashCode())
                    {
                        LastSent = DateTime.Now;

                        Settings.Other.PhoneWallpaperNum = wpNum;

                        CEF.Browser.Window.ExecuteCachedJs("Phone.updateWallpaper", wpNum);
                    }
                }
            });
        }

        public static void Show()
        {
            if (Phone.CurrentApp == Phone.AppTypes.None)
                Phone.SwitchMenu(false);

            Phone.CurrentApp = Phone.AppTypes.Settings;

            Phone.CurrentAppTab = -1;

            CEF.Browser.Window.ExecuteCachedJs("Phone.drawSettingsApp();");
        }
    }
}