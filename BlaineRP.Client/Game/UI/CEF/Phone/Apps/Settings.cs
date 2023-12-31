﻿using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.World;
using RAGE;

namespace BlaineRP.Client.Game.UI.CEF.Phone.Apps
{
    [Script(int.MaxValue)]
    public class Settings
    {
        public Settings()
        {
            Events.Add("Phone::UpdateWallpaper",
                (args) =>
                {
                    if (CEF.Phone.Phone.LastSent.IsSpam(250, false, false))
                        return;

                    var wpNum = (int)args[0];

                    if (Client.Settings.User.Other.PhoneWallpaperNum == wpNum)
                        return;

                    if (CEF.Phone.Phone.CurrentApp == AppType.Settings)
                        if (CEF.Phone.Phone.CurrentAppTab == "wallpaper".GetHashCode())
                        {
                            CEF.Phone.Phone.LastSent = Core.ServerTime;

                            Client.Settings.User.Other.PhoneWallpaperNum = wpNum;

                            Browser.Window.ExecuteCachedJs("Phone.updateWallpaper", wpNum);
                        }
                }
            );
        }

        public static void Show()
        {
            if (CEF.Phone.Phone.CurrentApp == AppType.None)
                CEF.Phone.Phone.SwitchMenu(false);

            CEF.Phone.Phone.CurrentApp = AppType.Settings;

            CEF.Phone.Phone.CurrentAppTab = -1;

            Browser.Window.ExecuteCachedJs("Phone.drawSettingsApp();");
        }
    }
}