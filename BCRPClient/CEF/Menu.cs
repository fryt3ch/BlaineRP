using BCRPClient.Sync;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;

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
            Achievement,
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

        public static List<(uint VID, Data.Vehicles.Vehicle Data)> OwnedVehicles;
        public static Dictionary<uint, (string Reason, string Name)> Gifts;

        private static int TempBindEsc;

        public Menu()
        {
            TempBindEsc = -1;

            LastSwitched = DateTime.Now;
            LastSent = DateTime.Now;

            _TimePlayed = 0;

            OwnedVehicles = new List<(uint, Data.Vehicles.Vehicle)>();
            Gifts = new Dictionary<uint, (string Reason, string Name)>();

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
                Settings.Aim.Color = ((string)args[0]).ToColour();
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
                var id = (uint)(int)args[0];

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

                var id = (uint)(int)args[1];

                if (add)
                {
                    bool notify = (bool)args[2];

                    int type = (int)args[3];
                    string gid = (string)args[4];
                    int amount = (int)args[5];
                    GiftSourceTypes reason = (GiftSourceTypes)(int)args[6];

                    var name = GetGiftName((GiftTypes)type, gid, amount);

                    Gifts.Add(id, (Locale.Notifications.Gifts.SourceNames[reason], GetGiftName((GiftTypes)type, gid, amount)));

                    CEF.Notification.Show(CEF.Notification.Types.Gift, Locale.Notifications.Gifts.Header, string.Format(Locale.Notifications.Gifts.Added, name, KeyBinds.Get(KeyBinds.Types.Menu).GetKeyString()));
                }
                else
                {
                    Gifts.Remove(id);
                }

                Browser.Window.ExecuteJs("Menu.drawGifts", new object[] { Gifts.Select(x => new object[] { x.Key, x.Value.Reason, x.Value.Name }) });
            });

            Events.Add("Menu::Quests::Locate", (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var quests = pData.Quests;

                var quest = quests.Where(x => x.Type == (Sync.Quest.QuestData.Types)Enum.Parse(typeof(Sync.Quest.QuestData.Types), (string)args[0])).FirstOrDefault();

                if (quest == null)
                    return;

                quest.MenuIconFunc();
            });
            #endregion
        }

        public static void Preload()
        {
            CEF.Browser.Window.ExecuteJs("Menu.selectOption", "menu-char");

            CEF.Browser.Window.ExecuteJs("Menu.drawSkills", new object[] { Sync.Players.MaxSkills.Select(x => new object[] { x.Key, x.Value }) });


            var mainDict = new Dictionary<string, string>()
            {
                { "time", "Время сервера" },
                { "help", "Скрыть подсказки" },
                { "names", "Скрыть имена игроков" },
                { "cid", "Скрыть CID игроков" },
                { "hud", "Скрыть HUD" },
                { "quest", "Скрыть задание" },
            }.ToDictionary(x => x.Key, x => Utils.ReplaceNewLineHtml(x.Value));

            var extraDict = new Dictionary<string, string>()
            {
                { "interact", "Скрыть кнопку взаимодействия" },
                { "items", "Скрыть названия\nпредметов на земле" },
                { "reload", "Автоматическая перезарядка" },
                { "finger", "Включить указание\nпальцем на объекты" },
            }.ToDictionary(x => x.Key, x => Utils.ReplaceNewLineHtml(x.Value));

            CEF.Browser.Window.ExecuteJs("Menu.createManyToggles", "main", mainDict.Select(x => new object[] { x.Key, x.Value }));

            CEF.Browser.Window.ExecuteJs("Menu.createManyToggles", "extra", extraDict.Select(x => new object[] { x.Key, x.Value }));
        }

        public static void Show(SectionTypes sType = SectionTypes.Last)
        {
            if (IsActive || Utils.IsAnyCefActive())
                return;

            if (LastSwitched.IsSpam(1000, false, false))
                return;

            if (TempBindEsc != -1)
                KeyBinds.Unbind(TempBindEsc);

            TempBindEsc = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());

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

            KeyBinds.Unbind(TempBindEsc);
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
            Browser.Window.ExecuteJs($"Menu.setColor", Settings.Aim.Color.HEX, Settings.Aim.Alpha);
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
                return Data.Vehicles.GetById(gid)?.Name ?? "null";

            if (type == GiftTypes.Money)
                return Locale.Notifications.Money.Header + $" {amount}$";

            return "null";
        }

        public static void SetCash(ulong value) => Browser.Window.ExecuteJs("Menu.setCash", value);

        public static void SetBank(ulong value) => Browser.Window.ExecuteJs("Menu.setBank", value);

        public static void SetSex(bool state) => Browser.Window.ExecuteJs("Menu.setSex", state);

        public static void SetName(string value) => Browser.Window.ExecuteJs("Menu.setName", value);

        public static void SetCID(uint value) => Browser.Window.ExecuteJs("Menu.setCID", value);

        public static void SetFraction(Players.FractionTypes type) => Browser.Window.ExecuteJs("Menu.setFraction", Locale.General.Players.FractionNames[type]);

        public static void SetOrganisation(string name) => Browser.Window.ExecuteJs("Menu.setOrganisation", name ?? Locale.General.Players.FractionNames[Sync.Players.FractionTypes.None]);

        public static void UpdateSkill(Sync.Players.SkillTypes type, int current) => Browser.Window.ExecuteJs("Menu.setSkill", type, current);

        public static void UpdateAchievement(Sync.Players.AchievementTypes aType, int current, int max) => Browser.Window.ExecuteJs("Menu.updateAchProgress", aType.ToString(), current, max);

        public static void AddAchievement(Sync.Players.AchievementTypes aType, int current, int max, string name, string desc) => Browser.Window.ExecuteJs("Menu.newAchievement", new object[] { new object[] { aType.ToString(), name, desc, current, max } });

        public static void UpdateQuestProgress() => Browser.Window.ExecuteJs("Menu.updateQuestProgress"); // id, new_progress

        public static void Load(Sync.Players.PlayerData pData, int timePlayed, DateTime creationDate, DateTime birthDate, Dictionary<uint, (int Type, string GID, int Amount, int Reason)> gifts)
        {
            Browser.Window.ExecuteJs("Menu.setOrganisation", "none"); // temp

            TimePlayed = timePlayed;

            BirthDate = birthDate;
            CreationDate = creationDate;

            Browser.Window.ExecuteJs("Menu.setRegDate", CreationDate.ToString("dd.MM.yyyy"));

            if (gifts.Count > 0)
            {
                foreach (var x in gifts)
                    Gifts.Add(x.Key, (Locale.Notifications.Gifts.SourceNames[(GiftSourceTypes)(int)x.Value.Reason], GetGiftName((GiftTypes)x.Value.Type, x.Value.GID, x.Value.Amount)));

                Browser.Window.ExecuteJs("Menu.drawGifts", new object[] { Gifts.Select(x => new object[] { x.Key, x.Value.Reason, x.Value.Name }) });
            }

            UpdateProperties(pData);

            UpdateQuests(pData);
        }

        public static void UpdateProperties(Sync.Players.PlayerData pData = null)
        {
            if (pData == null)
                pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            Browser.Window.ExecuteJs("Menu.clearPropertyTable", "veh");
            Browser.Window.ExecuteJs("Menu.clearPropertyTable", "est");

            var properties = new List<object[]>();

            properties.AddRange(pData.OwnedVehicles.Select(x => new object[] { "veh", x.Data.Type.ToString(), x.Data.BrandName, x.Data.SubName, x.Data.Class.ToString(), x.Data.GovPrice }));

            properties.AddRange(pData.OwnedBusinesses.Select(x => new object[] { "est", Sync.Players.PropertyTypes.Business.ToString(), x.Name, Utils.GetStreetName(x.InfoColshape.Position), Locale.General.PropertyBusinessClass, x.Price, x.SubId }));

            properties.AddRange(pData.OwnedHouses.Select(x => new object[] { "est", Sync.Players.PropertyTypes.House.ToString(), Locale.General.PropertyHouseString, Utils.GetStreetName(x.Position), x.Class.ToString(), x.Price, x.Id }));

            properties.AddRange(pData.OwnedApartments.Select(x => new object[] { "est", Sync.Players.PropertyTypes.Apartments.ToString(), Locale.General.PropertyApartmentsString, Data.Locations.ApartmentsRoot.All[x.RootType].Name, x.Class.ToString(), x.Price, x.NumberInRoot + 1 }));

            properties.AddRange(pData.OwnedGarages.Select(x => new object[] { "est", Sync.Players.PropertyTypes.Garage.ToString(), Locale.General.PropertyGarageString, Data.Locations.GarageRoot.All[x.RootType].Name, x.ClassType.ToString(), x.Price, x.NumberInRoot + 1 }));

            Browser.Window.ExecuteJs("Menu.fillProperties", new object[] { properties });
        }

        public static void UpdateQuests(Sync.Players.PlayerData pData = null)
        {
            if (pData == null)
                pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var quests = pData.Quests;

            Browser.Window.ExecuteJs("Menu.drawQuests", new object[] { quests.Select(x => new object[] { x.Type.ToString(), x.Data.Name, x.Data.GiverName, x.GoalWithProgress ?? "null", (int)x.Data.ColourType }) });
        }
    }
}
