using BCRPClient.Sync;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace BCRPClient.CEF
{
    public class Menu : Events.Script
    {
        private static DateTime LastSwitched;
        private static DateTime LastSent;

        public static bool IsActive { get => Browser.IsActive(Browser.IntTypes.Menu); }

        public enum GiftTypes
        {
            /// <summary>Предмет</summary>
            Item = 0,
            /// <summary>Транспорт</summary>
            Vehicle,
            /// <summary>Деньги</summary>
            Money
        }

        public enum GiftSourceTypes
        {
            /// <summary>Сервер</summary>
            Server = 0,
            /// <summary>Магазин</summary>
            Shop,
        }

        public enum SectionTypes
        {
            Last = -1,

            Information = 0,
            Quests,
            Achievements,
            Gifts,
            Shop,
            Settings,
            Keybinds,
            Help,
        }

        private static Dictionary<SectionTypes, string> Sections = new Dictionary<SectionTypes, string>()
        {
            { SectionTypes.Information, "menu-char" },
            { SectionTypes.Quests, "menu-quests" },
            { SectionTypes.Achievements, "menu-achievements" },
            { SectionTypes.Gifts, "menu-gifts" },
            { SectionTypes.Shop, "menu-shop" },
            { SectionTypes.Settings, "menu-settings" },
            { SectionTypes.Keybinds, "menu-controls" },
            { SectionTypes.Help, "menu-help" },
        };

        private static int _TimePlayed;
        public static int TimePlayed { get => _TimePlayed; set { _TimePlayed = value; Browser.Window.ExecuteJs("Menu.setPlayed", (value / 60f).ToString("0.0")); } }

        public static DateTime CreationDate { get; set; }
        public static DateTime BirthDate { get; set; }

        public static List<Data.Vehicles.Vehicle> OwnedVehicles;
        public static Dictionary<int, (string Reason, string Name)> Gifts;

        private static int TempBindEsc;

        private static Utils.Actions[] ActionsToCheck = new Utils.Actions[]
        {
            Utils.Actions.Knocked,
            //Utils.Actions.Frozen,
            //Utils.Actions.Cuffed,

            //Utils.Actions.Crouch,
            //Utils.Actions.Crawl,
            //Utils.Actions.Finger,
            //Utils.Actions.PushingVehicle,

            //Utils.Actions.Animation,
            //Utils.Actions.CustomAnimation,
            //Utils.Actions.Scenario,

            //Utils.Actions.InVehicle,
            //Utils.Actions.InWater,
            Utils.Actions.Shooting, //Utils.Actions.Reloading, //Utils.Actions.HasWeapon,
                                    //Utils.Actions.Climbing, Utils.Actions.Falling, Utils.Actions.Ragdoll, Utils.Actions.Jumping, Utils.Actions.OnFoot,
        };

        public Menu()
        {
            TempBindEsc = -1;

            LastSwitched = DateTime.Now;
            LastSent = DateTime.Now;

            _TimePlayed = 0;

            OwnedVehicles = new List<Data.Vehicles.Vehicle>();
            Gifts = new Dictionary<int, (string Reason, string Name)>();

            #region Events
            #region Update Settings
            Events.Add("Menu::UpdateSetting", (object[] args) =>
            {
                string id = (string)args[0];

                switch (id)
                {
                    case "sett-time":
                        Settings.Interface.UseServerTime = (bool)args[1];
                    break;

                    case "sett-help":
                        Settings.Interface.HideHints = (bool)args[1];
                    break;

                    case "sett-names":
                        Settings.Interface.HideNames = (bool)args[1];
                    break;

                    case "sett-cid":
                        Settings.Interface.HideCID = (bool)args[1];
                    break;

                    case "sett-hud":
                        Settings.Interface.HideHUD = (bool)args[1];
                    break;

                    case "sett-quest":
                        Settings.Interface.HideQuest = (bool)args[1];
                    break;

                    case "sett-interact":
                        Settings.Interface.HideInteractionBtn = (bool)args[1];
                    break;

                    case "sett-items":
                        Settings.Interface.HideIOGNames = (bool)args[1];
                    break;

                    case "sett-reload":
                        Settings.Interface.AutoReload = (bool)args[1];
                    break;

                    case "sett-finger":
                        Settings.Interface.FingerOn = (bool)args[1];
                    break;

                    case "sett-filter":
                        Settings.Chat.UseFilter = (bool)args[1];
                    break;

                    case "sett-timestamp":
                        Settings.Chat.ShowTime = (bool)args[1];
                    break;

                    case "sett-chat":
                        Settings.Chat.Height = int.Parse((string)args[1]);
                    break;

                    case "sett-font":
                        Settings.Chat.FontSize = int.Parse((string)args[1]);
                    break;

                    case "sett-speak":
                        Settings.Audio.VoiceVolume = int.Parse((string)args[1]);
                    break;

                    case "sett-3D":
                        Settings.Audio.SoundVolume = int.Parse((string)args[1]);
                    break;

                    case "sett-special":
                        if (LastSent.IsSpam(1000, false, false))
                            return;

                        Events.CallRemote("Players::SetIsInvalid", (bool)args[1]);

                        LastSent = DateTime.Now;
                    break;

                    case "sett-aimType":
                        Settings.Aim.Type = (Settings.Aim.Types)int.Parse((string)args[1]);
                    break;

                    case "sett-aimScale":
                        Settings.Aim.Scale = args[1] is float ? (float)args[1] : (int)args[1];
                    break;
                }
            });

            Events.Add("Menu::UpdateAimColor", (object[] args) =>
            {
                Settings.Aim.Color = ((string)args[0]).ToColor();
                Settings.Aim.Alpha = args[1] is float ? (float)args[1] : (int)args[1];
            });
            #endregion

            #region Update Keybinds
            Events.Add("Menu::UpdateKeyBind", (object[] args) =>
            {
                KeyBinds.Types id = Enum.Parse<KeyBinds.Types>((string)args[0]);
                int mKey = (int)args[1];
                int key = (int)args[2];

                List<RAGE.Ui.VirtualKeys> list = new List<RAGE.Ui.VirtualKeys>();

                if (mKey != -1)
                    list.Add((RAGE.Ui.VirtualKeys)mKey);

                if (key != -1)
                    list.Add((RAGE.Ui.VirtualKeys)key);

                KeyBinds.Binds[id].ChangeKeys(list.ToArray());
            });
            #endregion

            #region Defaulers
            Events.Add("Menu::DefaultAll", (object[] args) =>
            {
                var type = (string)args[0];

                if (type == "menu-settings")
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    Settings.DefaultAll();

                    UpdateSettingsData();
                }
                else if (type == "menu-controls")
                {
                    KeyBinds.DefaultAll();

                    UpdateKeyBindsData();
                }
            });
            #endregion

            Events.Add("Menu::GetGift", (object[] args) =>
            {
                int id = (int)args[0];

                if (id < 0)
                    return;

                if (!LastSent.IsSpam(1000, false, false))
                {
                    Events.CallRemote("Gift::Collect", id);

                    return;
                }
            });

            Events.Add("Menu::Gifts::Update", (object[] args) =>
            {
                bool add = (bool)args[0];

                int id = (int)args[1];

                if (add)
                {
                    bool notify = (bool)args[2];
                    bool spaceHint = (bool)args[3];

                    int type = (int)args[4];
                    string gid = (string)args[5];
                    int amount = (int)args[6];
                    GiftSourceTypes reason = (GiftSourceTypes)(int)args[7];

                    var name = GetGiftName((GiftTypes)type, gid, amount);

                    Gifts.Add(id, (Locale.Notifications.Gifts.SourceNames[reason], GetGiftName((GiftTypes)type, gid, amount)));

                    CEF.Notification.Show(CEF.Notification.Types.Gift, Locale.Notifications.Gifts.Header, (string.Format(Locale.Notifications.Gifts.Added, name, KeyBinds.Get(KeyBinds.Types.Menu).GetKeyString())) + (spaceHint ? Locale.Notifications.Gifts.SpaceHint : ""), 5000);
                }
                else
                {
                    Gifts.Remove(id);
                }

                Browser.Window.ExecuteJs("Menu.drawGifts", new object[] { Gifts.Select(x => new object[] { x.Key, x.Value.Reason, x.Value.Name }) });
            });
            #endregion
        }

        public static void Show(SectionTypes sType = SectionTypes.Last)
        {
            if (IsActive || Utils.IsAnyCefActive() || !Utils.CanDoSomething(ActionsToCheck))
                return;

            if (LastSwitched.IsSpam(1000, false, false))
                return;

            if (TempBindEsc != -1)
                RAGE.Input.Unbind(TempBindEsc);

            TempBindEsc = RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());

            if (sType != SectionTypes.Last)
                Browser.Window.ExecuteJs("Menu.selectOption", Sections[sType]);

            Browser.Window.ExecuteJs("Menu.setName", Player.LocalPlayer.Name);
            Browser.Window.ExecuteJs("Menu.setCharname", Player.LocalPlayer.Name);

            Browser.Switch(Browser.IntTypes.Menu, true);

            //Browser.Window.ExecuteJs("switchEsc", !Settings.Interface.HideHints);

            LastSwitched = DateTime.Now;

            Cursor.Show(true, true);
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            RAGE.Input.Unbind(TempBindEsc);
            TempBindEsc = -1;

            Cursor.Show(false, false);

            Browser.Switch(Browser.IntTypes.Menu, false);
        }

        #region Updaters
        public static void UpdateSettingsData()
        {
            UpdateInput("sett-chat", Settings.Chat.Height);
            UpdateInput("sett-font", Settings.Chat.FontSize);
            UpdateInput("sett-speak", Settings.Audio.VoiceVolume);
            UpdateInput("sett-3D", Settings.Audio.SoundVolume);

            Browser.Window.ExecuteJs("Menu.setAim", Settings.Aim.Type);
            Browser.Window.ExecuteJs("Menu.setAimSize", Settings.Aim.Scale);
            Browser.Window.ExecuteJs($"Menu.setColor", Settings.Aim.Color.ToHEX(), Settings.Aim.Alpha);

            Utils.ConsoleOutput(Settings.Aim.Color.ToHEX());
        }

        public static void UpdateKeyBindsData()
        {
            var list = KeyBinds.Binds.Where(x => x.Value.Changeable && !x.Value.InvOnly).Aggregate(new List<object[]>(), (x, y) =>
            {
                x.Add(new object[] { y.Key.ToString(), y.Value.Description, y.Value.Keys });

                return x;
            });

            Browser.Window.ExecuteJs("Menu.drawControls", list);
        }
        #endregion

        public static void UpdateToggle(string id, bool value)
        {
            Browser.Window.ExecuteJs("Menu.setToggleState", id, value);
        }

        public static void UpdateInput(string id, object value)
        {
            Browser.Window.ExecuteJs("Menu.setInputValue", id, value);
        }

        public static string GetGiftName(GiftTypes type, string gid, int amount)
        {
            if (type == GiftTypes.Item)
                return Data.Items.GetName(gid) ?? "null" + (amount > 1 ? $" x{amount}" : "");

            if (type == GiftTypes.Vehicle)
                return Data.Vehicles.Get(gid)?.Name ?? "null";

            if (type == GiftTypes.Money)
                return Locale.Notifications.Money.Header + $" {amount}$";

            return "null";
        }

        public static void SetCash(int value) => Browser.Window.ExecuteJs("Menu.setCash", value);
        public static void SetBank(int value) => Browser.Window.ExecuteJs("Menu.setBank", value);
        public static void SetSex(bool state) => Browser.Window.ExecuteJs("Menu.setSex", state);
        public static void SetName(string value) => Browser.Window.ExecuteJs("Menu.setName", value);
        public static void SetCID(int value) => Browser.Window.ExecuteJs("Menu.setCID", value);
        public static void SetFraction(Players.FractionTypes type) => Browser.Window.ExecuteJs("Menu.setFraction", Locale.General.Players.FractionNames[type]);
        public static void SetOrganisation() => Browser.Window.ExecuteJs("Menu.setOrganisation");

        public static void UpdateSkill(Sync.Players.SkillTypes type, int current) => Browser.Window.ExecuteJs($"Menu.setSkill", type, current);

        public static void Load(params object[] args)
        {
            var info = RAGE.Util.Json.Deserialize<(int timePlayed, DateTime creationDate, DateTime birtDate)>((string)args[0]);
            var vehicles = RAGE.Util.Json.Deserialize<List<string>>((string)args[1]);
            var gifts = RAGE.Util.Json.Deserialize<List<(int id, int type, string gid, int amount, int reason)>>((string)args[2]);

            Browser.Window.ExecuteJs("Menu.setOrganisation", Locale.General.Players.FractionNames[Sync.Players.FractionTypes.None]);

            Browser.Window.ExecuteJs("Menu.setRegDate", info.creationDate.ToString("dd.MM.yyyy"));

            TimePlayed = info.timePlayed;

            BirthDate = info.birtDate;
            CreationDate = info.creationDate;

            if (gifts.Count > 0)
            {
                foreach (var x in gifts)
                    Gifts.Add(x.id, (Locale.Notifications.Gifts.SourceNames[(GiftSourceTypes)(int)x.reason], GetGiftName((GiftTypes)x.type, x.gid, x.amount)));

                Browser.Window.ExecuteJs("Menu.drawGifts", new object[] { Gifts.Select(x => new object[] { x.Key, x.Value.Reason, x.Value.Name }) });
            }

            foreach (var x in vehicles)
            {
                var vData = Data.Vehicles.Get(x);

                if (vData == null)
                    continue;

                OwnedVehicles.Add(vData);

                Browser.Window.ExecuteJs("Menu.newProperty", new object[] { new object[] { "veh", vData.Type.ToString(), vData.BrandName, vData.SubName, "Luxe", 100 } });
            }
        }
    }
}
