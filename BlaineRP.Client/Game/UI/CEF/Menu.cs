using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Enums;
using BlaineRP.Client.Game.Fractions;
using BlaineRP.Client.Game.Fractions.Enums;
using BlaineRP.Client.Game.World;
using BlaineRP.Client.Input.Enums;
using BlaineRP.Client.Quests.Enums;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;
using Core = BlaineRP.Client.Input.Core;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class Menu
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
            Money,
            CasinoChips,
        }

        public enum GiftSourceTypes
        {
            /// <summary>Сервер</summary>
            Server = 0,
            /// <summary>Магазин</summary>
            Shop,
            Achievement,
            Casino,
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

        private static TimeSpan _TimePlayed;
        public static TimeSpan TimePlayed { get => _TimePlayed; set { _TimePlayed = value; Browser.Window.ExecuteJs("Menu.setPlayed", value.TotalHours.ToString("0.0")); } }

        public static DateTime CreationDate { get; set; }
        public static DateTime BirthDate { get; set; }

        public static List<(uint VID, Data.Vehicles.Vehicle Data)> OwnedVehicles;
        public static Dictionary<uint, (string Reason, string Name)> Gifts;

        private static int TempBindEsc;

        public Menu()
        {
            TempBindEsc = -1;

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
                        Settings.User.Interface.UseServerTime = (bool)args[1];
                        break;

                    case "sett-help":
                        Settings.User.Interface.HideHints = (bool)args[1];
                        break;

                    case "sett-names":
                        Settings.User.Interface.HideNames = (bool)args[1];
                        break;

                    case "sett-cid":
                        Settings.User.Interface.HideCID = (bool)args[1];
                        break;

                    case "sett-hud":
                        Settings.User.Interface.HideHUD = (bool)args[1];
                        break;

                    case "sett-quest":
                        Settings.User.Interface.HideQuest = (bool)args[1];
                        break;

                    case "sett-interact":
                        Settings.User.Interface.HideInteractionBtn = (bool)args[1];
                        break;

                    case "sett-items":
                        Settings.User.Interface.HideIOGNames = (bool)args[1];
                        break;

                    case "sett-reload":
                        Settings.User.Interface.AutoReload = (bool)args[1];
                        break;

                    case "sett-finger":
                        Settings.User.Interface.FingerOn = (bool)args[1];
                        break;

                    case "sett-filter":
                        Settings.User.Chat.UseFilter = (bool)args[1];
                        break;

                    case "sett-timestamp":
                        Settings.User.Chat.ShowTime = (bool)args[1];
                        break;

                    case "sett-chat":
                        Settings.User.Chat.Height = int.Parse((string)args[1]);
                        break;

                    case "sett-font":
                        Settings.User.Chat.FontSize = int.Parse((string)args[1]);
                        break;

                    case "sett-speak":
                        Settings.User.Audio.VoiceVolume = int.Parse((string)args[1]);
                        break;

                    case "sett-3D":
                        Settings.User.Audio.SoundVolume = int.Parse((string)args[1]);
                        break;

                    case "sett-special":
                        if (LastSent.IsSpam(1000, false, false))
                            return;

                        Events.CallRemote("Players::SetIsInvalid", (bool)args[1]);

                        LastSent = World.Core.ServerTime;
                        break;

                    case "sett-aimType":
                        Settings.User.Aim.Type = (Settings.User.Aim.Types)int.Parse((string)args[1]);
                        break;

                    case "sett-aimScale":
                        Settings.User.Aim.Scale = args[1] is float ? (float)args[1] : (int)args[1];
                        break;
                }
            });

            Events.Add("Menu::UpdateAimColor", (object[] args) =>
            {
                Settings.User.Aim.Color = new Utils.Colour((string)args[0]);
                Settings.User.Aim.Alpha = args[1] is float ? (float)args[1] : (int)args[1];
            });
            #endregion

            #region Update Keybinds
            Events.Add("Menu::UpdateKeyBind", (object[] args) =>
            {
                BindTypes id = Enum.Parse<BindTypes>((string)args[0]);
                int mKey = (int)args[1];
                int key = (int)args[2];

                List<RAGE.Ui.VirtualKeys> list = new List<RAGE.Ui.VirtualKeys>();

                if (mKey != -1)
                    list.Add((RAGE.Ui.VirtualKeys)mKey);

                if (key != -1)
                    list.Add((RAGE.Ui.VirtualKeys)key);

                Core.Get(id)?.ChangeKeys(list.ToArray());
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

                    Settings.User.Initialization.DefaultAll();

                    UpdateSettingsData();
                }
                else if (type == "menu-controls")
                {
                    Core.DefaultAll();

                    UpdateKeyBindsData();
                }
            });
            #endregion

            Events.Add("Menu::GetGift", (object[] args) =>
            {
                var id = Utils.Convert.ToUInt32(((string)args[0]).Replace("-gift-btn", ""));

                if (!LastSent.IsSpam(1000, false, false))
                {
                    Events.CallRemote("Gift::Collect", id);

                    return;
                }
            });

            Events.Add("Menu::Gifts::Update", (object[] args) =>
            {
                var add = (bool)args[0];

                var id = Utils.Convert.ToUInt32(args[1]);

                if (add)
                {
                    bool notify = (bool)args[2];

                    int type = (int)args[3];
                    string gid = (string)args[4];
                    int amount = (int)args[5];
                    GiftSourceTypes reason = (GiftSourceTypes)(int)args[6];

                    var name = GetGiftName((GiftTypes)type, gid, amount);

                    Gifts.Add(id, (Locale.Notifications.Gifts.SourceNames[reason], GetGiftName((GiftTypes)type, gid, amount)));

                    CEF.Notification.Show(CEF.Notification.Types.Gift, Locale.Notifications.Gifts.Header, string.Format(Locale.Notifications.Gifts.Added, name, Core.Get(BindTypes.Menu).GetKeyString()));
                }
                else
                {
                    Gifts.Remove(id);
                }

                Browser.Window.ExecuteJs("Menu.drawGifts", new object[] { Gifts.Select(x => new object[] { x.Key, x.Value.Reason, x.Value.Name }) });
            });

            Events.Add("Menu::Quests::Locate", (args) =>
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var quests = pData.Quests;

                var quest = quests.Where(x => x.Type == (QuestTypes)Enum.Parse(typeof(QuestTypes), (string)args[0])).FirstOrDefault();

                if (quest == null)
                    return;

                quest.MenuIconFunc();
            });

            Events.Add("Menu::Report::Send", (args) => ReportSend((string)args[0]));

            Events.Add("Menu::Report::C", (args) =>
            {

            });

            Events.Add("Menu::Close", (args) => Close());
            #endregion
        }

        public static void Preload()
        {
            CEF.Browser.Window.ExecuteJs("Menu.selectOption", "menu-char");

            CEF.Browser.Window.ExecuteJs("Menu.drawSkills", new object[] { Settings.App.Static.PlayerMaxSkills.Select(x => new object[] { x.Key, x.Value }) });


            var mainDict = new Dictionary<string, string>()
            {
                { "time", Locale.Get("SETTING_USESERVERTIME") },
                { "help", Locale.Get("SETTING_HIDEHINTS") },
                { "names", Locale.Get("SETTING_HIDENAMES") },
                { "cid", Locale.Get("SETTING_HIDECIDS") },
                { "hud", Locale.Get("SETTING_HIDEHUD") },
                { "quest", Locale.Get("SETTING_HIDEQUEST") },
            }.ToDictionary(x => x.Key, x => Utils.Misc.ReplaceNewLineHtml(x.Value));

            var extraDict = new Dictionary<string, string>()
            {
                { "interact", Locale.Get("SETTING_HIDEINTERACT") },
                { "items", Locale.Get("SETTING_HIDENAMES_ITEMS") },
                { "reload", Locale.Get("SETTING_AUTORELOAD") },
                { "finger", Locale.Get("SETTING_FINGERPOINT") },
            }.ToDictionary(x => x.Key, x => Utils.Misc.ReplaceNewLineHtml(x.Value));

            CEF.Browser.Window.ExecuteJs("Menu.createManyToggles", "main", mainDict.Select(x => new object[] { x.Key, x.Value }));

            CEF.Browser.Window.ExecuteJs("Menu.createManyToggles", "extra", extraDict.Select(x => new object[] { x.Key, x.Value }));
        }

        public static void Show(SectionTypes sType = SectionTypes.Last)
        {
            if (IsActive || Utils.Misc.IsAnyCefActive())
                return;

            if (LastSwitched.IsSpam(1000, false, false))
                return;

            if (TempBindEsc != -1)
                Core.Unbind(TempBindEsc);

            TempBindEsc = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());

            if (sType != SectionTypes.Last)
                Browser.Window.ExecuteJs("Menu.selectOption", Sections[sType]);

            Browser.Window.ExecuteJs("Menu.setName", Player.LocalPlayer.Name);
            Browser.Window.ExecuteJs("Menu.setCharname", Player.LocalPlayer.Name);

            Browser.Switch(Browser.IntTypes.Menu, true);

            //Browser.Window.ExecuteJs("switchEsc", !Settings.User.Interface.HideHints);

            LastSwitched = World.Core.ServerTime;

            Cursor.Show(true, true);
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            Core.Unbind(TempBindEsc);
            TempBindEsc = -1;

            Browser.Switch(Browser.IntTypes.Menu, false);

            Cursor.Show(false, false);
        }

        #region Updaters
        public static void UpdateSettingsData()
        {
            UpdateInput("sett-chat", Settings.User.Chat.Height);
            UpdateInput("sett-font", Settings.User.Chat.FontSize);
            UpdateInput("sett-speak", Settings.User.Audio.VoiceVolume);
            UpdateInput("sett-3D", Settings.User.Audio.SoundVolume);

            Browser.Window.ExecuteJs("Menu.setAim", Settings.User.Aim.Type);
            Browser.Window.ExecuteJs("Menu.setAimSize", Settings.User.Aim.Scale);
            Browser.Window.ExecuteJs($"Menu.setColor", Settings.User.Aim.Color.HEX, Settings.User.Aim.Alpha);
        }

        public static void UpdateKeyBindsData()
        {
            var list = Core.Binds.Where(x => x.Value.Changeable && !x.Value.InvOnly).Aggregate(new List<object[]>(), (x, y) =>
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
                return Client.Data.Items.GetName(gid) + (amount > 1 ? $" x{amount}" : "");

            if (type == GiftTypes.Vehicle)
                return Data.Vehicles.Core.GetById(gid)?.Name ?? "null";

            if (type == GiftTypes.Money)
                return $"${amount}";

            if (type == GiftTypes.CasinoChips)
                return $"{amount} фишек";

            return "null";
        }

        public static void SetCash(ulong value) => Browser.Window.ExecuteJs("Menu.setCash", value);

        public static void SetBank(ulong value) => Browser.Window.ExecuteJs("Menu.setBank", value);

        public static void SetSex(bool state) => Browser.Window.ExecuteJs("Menu.setSex", state);

        public static void SetName(string value) => Browser.Window.ExecuteJs("Menu.setName", value);

        public static void SetCID(uint value) => Browser.Window.ExecuteJs("Menu.setCID", value);

        public static void SetFraction(FractionTypes type) => Browser.Window.ExecuteJs("Menu.setFraction", Fraction.Get(type)?.Name ?? Fraction.NoFractionStr);

        public static void SetOrganisation(string name) => Browser.Window.ExecuteJs("Menu.setOrganisation", name ?? "null");

        public static void UpdateSkill(SkillTypes type, int current) => Browser.Window.ExecuteJs("Menu.setSkill", type, current);

        public static void UpdateAchievement(AchievementTypes aType, int current, int max) => Browser.Window.ExecuteJs("Menu.updateAchProgress", aType.ToString(), current, max);

        public static void AddAchievement(AchievementTypes aType, int current, int max, string name, string desc) => Browser.Window.ExecuteJs("Menu.newAchievement", new object[] { new object[] { aType.ToString(), name, desc, current, max } });

        public static void UpdateQuestProgress() => Browser.Window.ExecuteJs("Menu.updateQuestProgress"); // id, new_progress

        public static void Load(PlayerData pData, TimeSpan timePlayed, DateTime creationDate, DateTime birthDate, Dictionary<uint, (int Type, string GID, int Amount, int Reason)> gifts)
        {
            Browser.Window.ExecuteJs("Menu.setOrganisation", "none"); // temp

            TimePlayed = timePlayed;

            BirthDate = birthDate;
            CreationDate = creationDate;

            Browser.Window.ExecuteJs("Menu.setRegDate", CreationDate.ToString("dd.MM.yyyy"));

            if (gifts.Count > 0)
            {
                foreach (var x in gifts)
                    Gifts.Add(x.Key, (Locale.Notifications.Gifts.SourceNames.GetValueOrDefault((GiftSourceTypes)x.Value.Reason) ?? "null", GetGiftName((GiftTypes)x.Value.Type, x.Value.GID, x.Value.Amount)));

                Browser.Window.ExecuteJs("Menu.drawGifts", new object[] { Gifts.Select(x => new object[] { x.Key, x.Value.Reason, x.Value.Name }) });
            }

            UpdateProperties(pData);

            UpdateQuests(pData);
        }

        public static void UpdateProperties(PlayerData pData = null)
        {
            if (pData == null)
                pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            Browser.Window.ExecuteJs("Menu.clearPropertyTable", "veh");
            Browser.Window.ExecuteJs("Menu.clearPropertyTable", "est");

            var properties = new List<object[]>();

            properties.AddRange(pData.OwnedVehicles.Select(x => new object[] { "veh", x.Data.Type.ToString(), x.Data.BrandName, x.Data.SubName, x.Data.Class.ToString(), x.Data.GovPrice }));

            properties.AddRange(pData.OwnedBusinesses.Select(x => new object[] { "est", PropertyTypes.Business.ToString(), x.Name, Utils.Game.Misc.GetStreetName(x.InfoColshape.Position), Locale.General.PropertyBusinessClass, x.Price, x.SubId }));

            properties.AddRange(pData.OwnedHouses.Select(x => new object[] { "est", PropertyTypes.House.ToString(), Locale.General.PropertyHouseString, Utils.Game.Misc.GetStreetName(x.Position), x.Class.ToString(), x.Price, x.Id }));

            properties.AddRange(pData.OwnedApartments.Select(x => new object[] { "est", PropertyTypes.Apartments.ToString(), Locale.General.PropertyApartmentsString, Client.Data.Locations.ApartmentsRoot.All[x.RootId].Name, x.Class.ToString(), x.Price, x.NumberInRoot + 1 }));

            properties.AddRange(pData.OwnedGarages.Select(x => new object[] { "est", PropertyTypes.Garage.ToString(), Locale.General.PropertyGarageString, Client.Data.Locations.GarageRoot.All[x.RootId].Name, x.ClassType.ToString(), x.Price, x.NumberInRoot + 1 }));

            Browser.Window.ExecuteJs("Menu.fillProperties", new object[] { properties });
        }

        public static void UpdateQuests(PlayerData pData = null)
        {
            if (pData == null)
                pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var quests = pData.Quests;

            Browser.Window.ExecuteJs("Menu.drawQuests", new object[] { quests.Select(x => new object[] { x.Type.ToString(), x.Data.Name, x.Data.GiverName, x.GoalWithProgress ?? "null", (int)x.Data.ColourType }) });
        }

        public static void ClearReportChatHistory()
        {
            Browser.Window.ExecuteJs("Menu.clearHelpMessages();");
        }

        public static void AddMessageToChatHistory(Player player, DateTime time, string text)
        {
            if (player == Player.LocalPlayer)
            {
                Browser.Window.ExecuteJs("Menu.newHelpMessage", false, time.ToString("HH:mm"), "Вы", text);
            }
            else
            {
                Browser.Window.ExecuteJs("Menu.newHelpMessage", true, time.ToString("HH:mm"), $"{player.Name} [#{Utils.Convert.ToDecimal(player.GetSharedData<object>("CID", 0))}]", text);
            }
        }

        public static void UpdatePlayerReportInput(string text)
        {
            Browser.Window.ExecuteJs("Menu.updateHelpMessage", text);
        }

        private static DateTime ReportSendAntiSpam;

        public static async void ReportSend(string text)
        {
            if (!text.IsTextLengthValid(10, 150, true))
            {
                if (text != null)
                    UpdatePlayerReportInput(text);

                return;
            }

            if (ReportSendAntiSpam.IsSpam(2500, false, true))
                return;

            ReportSendAntiSpam = World.Core.ServerTime;

            if ((bool)await Events.CallRemoteProc("Report::Send", text))
            {
                UpdatePlayerReportInput("");

                AddMessageToChatHistory(Player.LocalPlayer, World.Core.ServerTime, text);
            }
        }
    }
}
