using RAGE;
using static BCRPClient.CEF.Phone;

namespace BCRPClient.CEF.PhoneApps
{
    [Script(int.MaxValue)]
    public class SettingsApp 
    {
        public SettingsApp()
        {
            Events.Add("Phone::UpdateWallpaper", (args) =>
            {
                if (LastSent.IsSpam(250, false, false))
                    return;

                var wpNum = (int)args[0];

                if (Settings.User.Other.PhoneWallpaperNum == wpNum)
                {
                    return;
                }

                if (CurrentApp == AppTypes.Settings)
                {
                    if (CurrentAppTab == "wallpaper".GetHashCode())
                    {
                        LastSent = Sync.World.ServerTime;

                        Settings.User.Other.PhoneWallpaperNum = wpNum;

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