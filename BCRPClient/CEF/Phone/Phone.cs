using RAGE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.CEF
{
    public class Phone : Events.Script
    {
        private static bool _IsActive { get; set; }

        public static bool IsActive { get => _IsActive; private set => _IsActive = value; }

        public static AppTypes CurrentApp { get; set; }

        public static uint CurrentAppTab { get; set; }

        public static DateTime LastSent;

        private static List<int> TempBinds { get; set; } = new List<int>();

        public enum AppTypes
        {
            None = 0,

            Settings,
            Vehicles,
            Bank,
            BSim,
            Camera,
            Navigator,
            Music,
            Phone,
            Contacts,
            SMS,
            Browser,
        }

        private static Dictionary<AppTypes, string> AppsJsNames = new Dictionary<AppTypes, string>()
        {
            { AppTypes.Settings, "settings" },
            { AppTypes.Vehicles, "veh" },
            { AppTypes.Bank, "bank" },
            { AppTypes.BSim, "bsim" },
            { AppTypes.Camera, "camera" },
            { AppTypes.Navigator, "gps" },
            { AppTypes.Music, "music" },
            { AppTypes.Phone, "phone" },
            { AppTypes.Contacts, "contacts" },
            { AppTypes.SMS, "sms" },
            { AppTypes.Browser, "browser" },
        };

        public static AppTypes GetAppTypeByJsName(string jsName) => AppsJsNames.Where(x => x.Value == jsName).FirstOrDefault().Key;

        public Phone()
        {
            Events.Add("Phone::OpenApp", async (args) =>
            {
                if (LastSent.IsSpam(500, false, false))
                    return;

                var appIdStr = args[0] is string str ? str : null;

                var appType = appIdStr == null ? AppTypes.None : GetAppTypeByJsName(appIdStr.Replace("_app", ""));

                LastSent = DateTime.Now;

                if (appType == AppTypes.None)
                {
                    CurrentApp = AppTypes.None;

                    SwitchMenu(true);
                }
                else
                {
                    if (appType == AppTypes.BSim)
                    {
                        var res = ((string)await Events.CallRemoteProc("Phone::GPD"))?.Split('_');

                        if (res == null)
                            return;

                        PhoneApps.BSimApp.Show(res[0], uint.Parse(res[1]), uint.Parse(res[2]), uint.Parse(res[3]));
                    }
                    else if (appType == AppTypes.Phone)
                    {
                        PhoneApps.PhoneApp.ShowDefault(null);
                    }
                    else if (appType == AppTypes.Camera)
                    {

                    }
                    else if (appType == AppTypes.Settings)
                    {

                    }
                    else if (appType == AppTypes.Bank)
                    {

                    }
                    else if (appType == AppTypes.Vehicles)
                    {
                        
                    }
                    else if (appType == AppTypes.Navigator)
                    {

                    }
                    else if (appType == AppTypes.Contacts)
                    {

                    }
                    else if (appType == AppTypes.SMS)
                    {

                    }
                }
            });

            Events.Add("Phone::Transaction", async (args) =>
            {
                if (LastSent.IsSpam(500, false, false))
                    return;

                if (CurrentApp == AppTypes.BSim)
                {
                    var amountStr = args[0]?.ToString();

                    if (amountStr == null)
                        return;

                    decimal amountI;

                    if (!decimal.TryParse(amountStr, out amountI))
                        return;

                    int amount;

                    if (!amountI.IsNumberValid(1, int.MaxValue, out amount, true))
                        return;

                    LastSent = DateTime.Now;

                    var resObj = await Events.CallRemoteProc("Phone::AB", amount);

                    if (resObj == null)
                        return;

                    var res = Convert.ToDecimal(resObj).ToUInt32();

                    CEF.Browser.Window.ExecuteJs("Phone.updateInfoLine", "bsim-app-info", 1, res);
                }
            });

            Events.Add("Phone::Call", (args) =>
            {
                if (LastSent.IsSpam(500, false, false))
                    return;

                var number = args[0]?.ToString();

                if (number == null || number.Length == 0)
                    return;

                LastSent = DateTime.Now;

                PhoneApps.PhoneApp.Call(number);
            });

            Events.Add("Phone::CallAnswer", (args) =>
            {
                var ans = (bool)args[0];

                PhoneApps.PhoneApp.CallAnswer(ans);
            });
        }

        public static void Show()
        {
            IsActive = true;

            if (CEF.HUD.SpeedometerEnabled)
                CEF.Browser.Switch(Browser.IntTypes.HUD_Speedometer, false);

            CEF.Browser.Window.ExecuteCachedJs("Phone.showPhone", true);

            KeyBinds.CursorNotFreezeInput = true;

            CEF.Cursor.Show(false, true);

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () =>
            {
                if (CEF.Chat.InputVisible)
                    return;

                Sync.Phone.Toggle();
            }));
        }

        public static void Close()
        {
            IsActive = false;

            CEF.Browser.Window.ExecuteCachedJs("Phone.showPhone", false);

            if (CEF.HUD.SpeedometerMustBeEnabled)
                CEF.Browser.Switch(Browser.IntTypes.HUD_Speedometer, true);

            KeyBinds.CursorNotFreezeInput = false;

            CEF.Cursor.Show(false, false);
        }

        public static void Preload()
        {
            CEF.Browser.Switch(Browser.IntTypes.Phone, true);

            SetWallpaper(Settings.Other.PhoneWallpaperNum);
        }

        public static void SetWallpaper(int num)
        {
            CEF.Browser.Window.ExecuteJs("Phone.setWallpaper", num);
        }

        public static void SwitchMenu(bool state)
        {
            CurrentAppTab = 0;

            CEF.Browser.Window.ExecuteCachedJs("Phone.showMenu", state);
        }
    }
}
