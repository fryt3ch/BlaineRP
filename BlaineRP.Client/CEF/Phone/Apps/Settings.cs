using BlaineRP.Client.CEF.Phone.Enums;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using RAGE;

namespace BlaineRP.Client.CEF.Phone.Apps
{
    [Script(int.MaxValue)]
    public class Settings
    {
        public Settings()
        {
            Events.Add("Phone::UpdateWallpaper", (args) =>
            {
                if (CEF.Phone.Phone.LastSent.IsSpam(250, false, false))
                    return;

                var wpNum = (int)args[0];

                if (Client.Settings.User.Other.PhoneWallpaperNum == wpNum)
                {
                    return;
                }

                if (CEF.Phone.Phone.CurrentApp == AppTypes.Settings)
                {
                    if (CEF.Phone.Phone.CurrentAppTab == "wallpaper".GetHashCode())
                    {
                        CEF.Phone.Phone.LastSent = Sync.World.ServerTime;

                        Client.Settings.User.Other.PhoneWallpaperNum = wpNum;

                        Browser.Window.ExecuteCachedJs("Phone.updateWallpaper", wpNum);
                    }
                }
            });
        }

        public static void Show()
        {
            if (CEF.Phone.Phone.CurrentApp == AppTypes.None)
                CEF.Phone.Phone.SwitchMenu(false);

            CEF.Phone.Phone.CurrentApp = AppTypes.Settings;

            CEF.Phone.Phone.CurrentAppTab = -1;

            Browser.Window.ExecuteCachedJs("Phone.drawSettingsApp();");
        }
    }
}