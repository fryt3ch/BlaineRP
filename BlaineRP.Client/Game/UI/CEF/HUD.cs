﻿using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Game.Fractions;
using BlaineRP.Client.Game.Input.Enums;
using BlaineRP.Client.Game.Jobs;
using BlaineRP.Client.Game.Quests;
using BlaineRP.Client.Game.Scripts.Misc;
using BlaineRP.Client.Game.Scripts.Sync;
using BlaineRP.Client.Game.World;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class HUD
    {
        public enum SpeedometerTypes : byte
        {
            /// <summary>Левый поворотник</summary>
            LeftArrow = 0,

            /// <summary>Правый поворотник</summary>
            RightArrow,

            /// <summary>Блокировка дверей</summary>
            Doors,

            /// <summary>Фары</summary>
            Lights,

            /// <summary>Ремень</summary>
            Belt,

            /// <summary>Двигатель</summary>
            Engine,
        }

        public enum StatusTypes : byte
        {
            /// <summary>Голод</summary>
            Food = 0,

            /// <summary>Настроение</summary>
            Mood,

            /// <summary>Плохое самочувствие</summary>
            Sick,

            /// <summary>Ранение</summary>
            Wounded,

            /// <summary>Дайвинг</summary>
            Diving,

            /// <summary>Зеленая зона</summary>
            GreenZone,

            /// <summary>Зона для рыбалки</summary>
            FishingZone,
        }

        private static bool BeltOffSoundOn = false;

        public static Action InteractionAction = null;
        private static int InteractionBind = -1;

        #region ZoneNames

        public static Dictionary<string, string> ZoneNames = new Dictionary<string, string>()
        {
            { "GALLI", "Галли" },
            { "OBSERV", "Обсерватория" },
            { "AIRP", "Аэропорт" },
            { "ALAMO", "Аламо-си" },
            { "ALTA", "Альта" },
            { "ARMYB", "Форт Занкудо" },
            { "BANHAMC", "Каньон Бэнхам" },
            { "BANNING", "Бэннинг" },
            { "BEACH", "Пляж Веспуччи" },
            { "BHAMCA", "Каньон Бэнхам" },
            { "BRADP", "Брэддок" },
            { "BRADT", "Туннель Брэддок" },
            { "BURTON", "Бёртон" },
            { "CALAFB", "Мост Калафия" },
            { "CANNY", "Каньон Рэйтон" },
            { "CCREAK", "Ручей Кэссиди" },
            { "CHAMH", "Чемберлейн-Хиллс" },
            { "CHIL", "Вайнвуд-Хиллс" },
            { "CHU", "Чумаш" },
            { "CMSW", "Окрестности Чилиад" },
            { "CYPRE", "Сайпресс-Флэтс" },
            { "DAVIS", "Дэвис" },
            { "DELBE", "Пляж Дель-Перро" },
            { "DELPE", "Дель-Перро" },
            { "DELSOL", "Ла Пуэрта" },
            { "DESRT", "Пустыня Гранд-Сенора" },
            { "DOWNT", "Даунтаун" },
            { "DTVINE", "Даунтаун-Вайнвуд" },
            { "EAST_V", "Восточный Вайнвуд" },
            { "EBURO", "Эль Бурро" },
            { "ELGORL", "Маяк Эль Гордо" },
            { "ELYSIAN", "Остров Элизиан" },
            { "GALFISH", "Галилея" },
            { "GOLF", "Гольф" },
            { "GRAPES", "Грэйпсид" },
            { "GREATC", "Грейт-Чаперрэл" },
            { "HARMO", "Гармони" },
            { "HAWICK", "Хавик" },
            { "HORS", "Трасса Вайнвуд" },
            { "HUMLAB", "Лаборатория" },
            { "JAIL", "Тюрьма Болингброук" },
            { "KOREAT", "Маленький Сеул" },
            { "LACT", "Водохранилище" },
            { "LAGO", "Болото Занкудо" },
            { "LDAM", "Плотина Лэнд-Экт" },
            { "LEGSQU", "Легион-Сквер" },
            { "LMESA", "Ла Меса" },
            { "LOSPUER", "Ла Пуэрта" },
            { "MIRR", "Миррор Парк" },
            { "MORN", "Морнинвуд" },
            { "MOVIE", "Ричардс-Маджестик" },
            { "MTCHIL", "Гора Чилиад" },
            { "MTGORDO", "Гора Гордо" },
            { "MTJOSE", "Гора Джосайя" },
            { "MURRI", "Муррьета" },
            { "NCHU", "Северный Чумаш" },
            { "NOOSE", "N.O.O.S.E" },
            { "OCEANA", "Тихий Океан" },
            { "PALCOV", "Бухта Палето" },
            { "PALETO", "Палето-Бэй" },
            { "PALFOR", "Лес Палето" },
            { "PALHIGH", "Нагорье Паломино" },
            { "PALMPOW", "Энергостанция" },
            { "PBLUFF", "Пасифик-Блаффс" },
            { "PBOX", "Пиллбокс-Хилл" },
            { "PROCOB", "Пляж Прокопио" },
            { "PROL", "Пролог" },
            { "RANCHO", "Ранчо" },
            { "RGLEN", "Ричман-Глен" },
            { "RICHM", "Ричман" },
            { "ROCKF", "Рокфорд-Хиллс" },
            { "RTRAK", "Рэдвуд" },
            { "SANAND", "Сан-Андрэас" },
            { "SANCHIA", "Сан-Шаньский Хребет" },
            { "SANDY", "Сэнди-Шорс" },
            { "SKID", "Мишн-Роу" },
            { "SLAB", "Трэйлер-парк" },
            { "STAD", "Арена Мэйз-Банк" },
            { "STRAW", "Строуберри" },
            { "TATAMO", "Татавиамские горы" },
            { "TERMINA", "Терминал" },
            { "TEXTI", "Текстиль-Сити" },
            { "TONGVAH", "Тонгва-Хиллс" },
            { "TONGVAV", "Долина Тонгва" },
            { "VCANA", "Каналы Веспуччи" },
            { "VESP", "Веспуччи" },
            { "VINE", "Вайнвуд" },
            { "WINDF", "Ветряные мельницы" },
            { "WVINE", "Западный Вайнвуд" },
            { "ZANCUDO", "Река Занкудо" },
            { "ZP_ORT", "Порт Лос-Сантоса" },
            { "ZQ_UAR", "Карьер Дэвис-Кварц" },
        };

        #endregion

        public HUD()
        {
            SpeedometerMustBeEnabled = false;
            LastAmmo = int.MinValue;
            LastSpeed = -1;
        }

        public static bool IsActive => Browser.IsActiveOr(Browser.IntTypes.HUD_Top, Browser.IntTypes.HUD_Left);

        public static bool SpeedometerEnabled => Browser.IsActive(Browser.IntTypes.HUD_Speedometer);
        public static bool SpeedometerMustBeEnabled { get; private set; }

        public static int LastAmmo { get; private set; }
        public static int LastSpeed { get; private set; }

        public static void ShowHUD(bool value)
        {
            if (!Players.CharacterLoaded)
                return;

            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (value)
            {
                UpdateLeftHUDPos();

                Browser.Switch(Browser.IntTypes.HUD_Top, true);
                Browser.Switch(Browser.IntTypes.HUD_Left, true);

                if (!Phone.Phone.IsActive)
                    Browser.Switch(Browser.IntTypes.HUD_Help, !Settings.User.Interface.HideHints);

                if (!Settings.User.Interface.HideQuest && Quest.ActualQuest != null)
                    EnableQuest(true);

                if (SpeedometerMustBeEnabled && !Phone.Phone.IsActive)
                    Browser.Switch(Browser.IntTypes.HUD_Speedometer, true);
            }
            else
            {
                Browser.Switch(Browser.IntTypes.HUD_Top, false);
                Browser.Switch(Browser.IntTypes.HUD_Left, false);
                Browser.Switch(Browser.IntTypes.HUD_Help, false);
                Browser.Switch(Browser.IntTypes.HUD_Quest, false);

                Browser.Switch(Browser.IntTypes.HUD_Speedometer, false);
            }

            RAGE.Game.Ui.DisplayRadar(value);
        }

        #region HUD Menu

        [Script(int.MaxValue)]
        public class Menu
        {
            /// <summary>Все возможные типы выбора в меню</summary>
            /// <remarks>Порядок влияет на порядок отображения!</remarks>
            public enum Types
            {
                [Language.Localized("HUD_MENU_I_MENU_NAME_0", "MENU_I_NAME")]
                /// <summary>Меню</summary>
                Menu = 0,

                [Language.Localized("HUD_MENU_I_DOCUMENTS_NAME_0", "MENU_I_NAME")]
                /// <summary>Документы</summary>
                Documents,

                [Language.Localized("HUD_MENU_I_INVENTORY_NAME_0", "MENU_I_NAME")]
                /// <summary>Инвентарь</summary>
                Inventory,

                [Language.Localized("HUD_MENU_I_PHONE_NAME_0", "MENU_I_NAME")]
                /// <summary>Телефон</summary>
                Phone,

                [Language.Localized("HUD_MENU_I_TABLETPC_NAME_0", "MENU_I_NAME")]
                /// <summary>Служебный планшет полиции</summary>
                Fraction_Police_TabletPC,

                [Language.Localized("HUD_MENU_I_FRACMENU_NAME_0", "MENU_I_NAME")]
                /// <summary>Меню фракции</summary>
                Fraction_Menu,

                [Language.Localized("HUD_MENU_I_JOBMENU_NAME_0", "MENU_I_NAME")]
                /// <summary>Меню работы</summary>
                Job_Menu,

                [Language.Localized("HUD_MENU_I_HOUSEMENU_NAME_0", "MENU_I_NAME")]
                /// <summary>Меню дома</summary>
                Menu_House,

                [Language.Localized("HUD_MENU_I_APSMENU_NAME_0", "MENU_I_NAME")]
                /// <summary>Меню квартиры</summary>
                Menu_Apartments,

                [Language.Localized("HUD_MENU_I_ANIMMENU_NAME_0", "MENU_I_NAME")]
                /// <summary>Анимации</summary>
                Animations,

                [Language.Localized("HUD_MENU_I_BLIPSMENU_NAME_0", "MENU_I_NAME")]
                /// <summary>Меню локальных меток</summary>
                BlipsMenu,

                [Language.Localized("HUD_MENU_I_WSKINSMENU_NAME_0", "MENU_I_NAME")]
                /// <summary>Меню раскрасок для оружия</summary>
                WeaponSkinsMenu,
            }

            private static Dictionary<Types, Action> Actions = new Dictionary<Types, Action>()
            {
                { Types.Menu, () => CEF.Menu.Show(CEF.Menu.SectionTypes.Last) },
                { Types.Documents, () => Documents.Show() },
                { Types.BlipsMenu, () => BlipsMenu.Show() },
                { Types.Menu_House, () => HouseMenu.ShowRequest() },
                { Types.Menu_Apartments, () => HouseMenu.ShowRequest() },
                { Types.WeaponSkinsMenu, () => Players.TryShowWeaponSkinsMenu() },
                { Types.Job_Menu, () => Job.ShowJobMenu() },
                { Types.Fraction_Menu, () => Fraction.ShowFractionMenu() },
                { Types.Fraction_Police_TabletPC, () => Police.ShowPoliceTabletPc() },
            };

            public Menu()
            {
                Events.Add("HUD::Menu::Action",
                    (object[] args) =>
                    {
                        Switch(false);

                        var type = (Types)args[0];

                        Action kbAction = Input.Core.HudMenuBinds.Where(x => x.Value == type).Select(x => Input.Core.Get(x.Key).Action).FirstOrDefault();

                        if (kbAction != null)
                        {
                            kbAction.Invoke();

                            return;
                        }

                        Actions.GetValueOrDefault(type)?.Invoke();
                    }
                );
            }

            public static bool IsActive => Browser.IsActive(Browser.IntTypes.HUD_Menu);

            public static List<Types> CurrentTypes { get; private set; } = new List<Types>();

            private static List<int> TempBinds { get; set; } = new List<int>();

            public static void UpdateCurrentTypes(bool enable, params Types[] types)
            {
                if (enable)
                    foreach (Types x in types)
                    {
                        if (!CurrentTypes.Contains(x))
                            CurrentTypes.Add(x);
                    }
                else
                    foreach (Types x in types)
                    {
                        CurrentTypes.Remove(x);
                    }

                CurrentTypes = CurrentTypes.OrderBy(x => x).ToList();
            }

            public static void SetCurrentTypes(params Types[] types)
            {
                CurrentTypes.Clear();
                UpdateCurrentTypes(true, types);
            }

            public static void Switch(bool state, List<Types> types = null)
            {
                if (state)
                {
                    if (IsActive || Utils.Misc.IsAnyCefActive())
                        return;

                    if (types == null)
                        types = CurrentTypes;
                    else
                        types = types.OrderBy(x => x).ToList();

                    if ((types?.Count ?? 0) == 0)
                        return;

                    TempBinds.Add(Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Switch(false)));

                    Browser.Switch(Browser.IntTypes.HUD_Menu, true);

                    Cursor.Show(true, true);

                    Input.Core.Get(BindTypes.Menu).Disable();

                    Browser.Window.ExecuteJs("Hud.drawMenu",
                        new object[]
                        {
                            types.Select(x => new object[]
                                {
                                    x,
                                    Locale.Get(Language.Strings.GetKeyFromTypeByMemberName(x.GetType(), x.ToString(), "MENU_I_NAME") ?? "null"),
                                }
                            ),
                        }
                    );

                    RAGE.Ui.Cursor.Vector2 cPos = RAGE.Ui.Cursor.Position;

                    if (cPos.X + 50 > Main.ScreenResolution.X || cPos.Y + 50 > Main.ScreenResolution.Y)
                    {
                        cPos.X = Main.ScreenResolution.X / 2;
                        cPos.Y = Main.ScreenResolution.Y / 2;
                    }

                    Browser.Window.ExecuteJs("Hud.positionMenu", cPos.X, cPos.Y);
                }
                else
                {
                    if (!IsActive)
                        return;

                    foreach (int x in TempBinds.ToList())
                    {
                        Input.Core.Unbind(x);
                    }

                    TempBinds.Clear();

                    Browser.Switch(Browser.IntTypes.HUD_Menu, false);

                    Cursor.Show(false, false);

                    Input.Core.Get(BindTypes.Menu).Enable();
                }
            }
        }

        #endregion

        #region JS Stuff

        /// <summary>Установить кол-во наличных</summary>
        public static void SetCash(ulong value)
        {
            Browser.Window.ExecuteJs("Hud.setCash", value);
        }

        /// <summary>Установить кол-во денег в банке</summary>
        public static void SetBank(ulong value)
        {
            Browser.Window.ExecuteJs("Hud.setBank", value);
        }

        /// <summary>Переключить подсказки справа</summary>
        /// <param name="state">true - показать, false - скрыть</param>
        public static void ToggleHints(bool state)
        {
            Browser.Switch(Browser.IntTypes.HUD_Help, state);
        }

        public static void UpdateTime()
        {
            DateTime time = Settings.User.Interface.UseServerTime ? Core.ServerTime : Core.LocalTime;

            Browser.Window.ExecuteJs("Hud.setTime", Settings.User.Interface.UseServerTime, time.ToString("HH:mm"), time.ToString("dd.MM.yyyy"));
        }

        /// <summary>Переключить иконку микрофона</summary>
        /// <param name="value">true - включён, false - выключен, null - недоступен</param>
        public static void SwitchMicroIcon(bool? state)
        {
            Browser.Window.ExecuteJs("Hud.switchMicro", state);
        }

        public static void SwitchStatusIcon(StatusTypes type, bool state)
        {
            Browser.Window.ExecuteJs("Hud.setState", (int)type, state);
        }

        /// <summary>Переключить иконку патронов</summary>
        /// <param name="state">true - показать, false - скрыть</param>
        public static void SwitchAmmo(bool state)
        {
            Browser.Window.ExecuteJs("Hud.switchAmmo", state);
        }

        /// <summary>Задать кол-во патронов</summary>
        public static void SetAmmo(int value = 0)
        {
            if (value != LastAmmo)
            {
                LastAmmo = value;

                Browser.Window.ExecuteJs("Hud.setAmmo", LastAmmo < 0 ? "∞" : LastAmmo.ToString());
            }
        }

        private static void PlayBeltOffSound(bool value, bool isElectricCar = false)
        {
            //Browser.Window.ExecuteJs("Hud.playBeltOff", value, isElectricCar);
            BeltOffSoundOn = value;
        }

        public static void SwitchEngineIcon(bool value)
        {
            Browser.Window.ExecuteJs("Hud.setSpdmtrState", SpeedometerTypes.Engine, value);
        }

        public static void SwitchBeltIcon(bool value)
        {
            Browser.Window.ExecuteJs("Hud.setSpdmtrState", SpeedometerTypes.Belt, value);

            if (value && BeltOffSoundOn)
                PlayBeltOffSound(false);
        }

        public static void SwitchLightsIcon(bool value)
        {
            Browser.Window.ExecuteJs("Hud.setSpdmtrState", SpeedometerTypes.Lights, value);
        }

        public static void SwitchDoorsIcon(bool value)
        {
            Browser.Window.ExecuteJs("Hud.setSpdmtrState", SpeedometerTypes.Doors, value);
        }

        public static void SwitchArrowIcon(bool left, bool value)
        {
            Browser.Window.ExecuteJs("Hud.setSpdmtrState", left ? SpeedometerTypes.LeftArrow : SpeedometerTypes.RightArrow, value);
        }

        public static void SwitchCruiseControlIcon(bool? value)
        {
            Browser.Window.ExecuteJs("Hud.switchCruiseControl", value);
        }

        public static void SwitchInteractionText(bool state, string text = null, RAGE.Ui.VirtualKeys key = RAGE.Ui.VirtualKeys.E)
        {
            if (text != null)
                Browser.Window.ExecuteJs("Hud.drawInteract", text, Input.Core.GetKeyString(key));

            Browser.Switch(Browser.IntTypes.HUD_Interact, state);

            if (state)
            {
                //RAGE.Game.Audio.PlaySoundFrontend(-1, "Enter_Area", "DLC_Lowrider_Relay_Race_Sounds", true);

                if (InteractionBind != -1)
                    Input.Core.Unbind(InteractionBind);

                InteractionBind = Input.Core.Bind(key,
                    true,
                    () =>
                    {
                        if (Input.Core.Get(BindTypes.Interaction)?.IsDisabled == true)
                            return;

                        InteractionAction?.Invoke();
                    }
                );
            }
            else
            {
                if (InteractionBind != -1)
                {
                    Input.Core.Unbind(InteractionBind);

                    InteractionBind = -1;
                }
            }
        }

        public static void SetQuestParams(string questGiver, string questName, string goal, Quest.QuestData.ColourTypes cType)
        {
            Browser.Window.ExecuteJs("Hud.drawQuest",
                new object[]
                {
                    new object[]
                    {
                        questName,
                        questGiver,
                        goal,
                        (int)cType,
                    },
                }
            );
        }

        public static void EnableQuest(bool state)
        {
            if (!Players.CharacterLoaded)
                return;

            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (state)
            {
                if (Quest.ActualQuest != null)
                {
                    Browser.Switch(Browser.IntTypes.HUD_Quest, true);

                    Quest.ActualQuest.UpdateHudQuest();
                }
            }
            else
            {
                Browser.Switch(Browser.IntTypes.HUD_Quest, false);
            }
        }

        #endregion

        #region HUD Stuff

        public static void UpdateHUD()
        {
            Vector3 pos = Player.LocalPlayer.Position;

            Browser.Window.ExecuteJs("Hud.setLocation",
                ZoneNames.GetValueOrDefault(RAGE.Game.Zone.GetNameOfZone(pos.X, pos.Y, pos.Z).ToUpper()) ?? "null",
                Utils.Game.Misc.GetStreetName(pos)
            );
            Browser.Window.ExecuteJs("Hud.setOnline", Entities.Players.Count);
        }

        public static void UpdateLeftHUDPos()
        {
            float sfX = 1f / 20f;
            float sfY = 1f / 20f;

            float safezone = RAGE.Game.Graphics.GetSafeZoneSize();
            float aspectratio = RAGE.Game.Graphics.GetAspectRatio(false);

            float scaleX = 1f / Main.ScreenResolution.X, scaleY = 1f / Main.ScreenResolution.Y;

            float minimapWidth = scaleX * (Main.ScreenResolution.X / (4 * aspectratio)) * (MiniMap.ZoomState == 2 ? 1.6f : 1f);

            float minimapRigthX = minimapWidth + scaleX * (Main.ScreenResolution.X * (sfX * (Math.Abs(safezone - 1f) * 10f)));
            float minimapBottomY = 1f - scaleY * (Main.ScreenResolution.Y * (sfY * (Math.Abs(safezone - 1f) * 10f)));

            /*        width: scaleX * (resolution.x / (4 * aspectRatio)),
                    height: scaleY * (resolution.y / 5.674),
                    scaleX: scaleX,
                    scaleY: scaleY,
                    leftX: scaleX * (resolution.x * (sfX * (System.Math.abs(safeZone - 1.0) * 10))),
                    bottomY: 1.0 - scaleY * (resolution.y * (sfY * (System.Math.abs(safeZone - 1.0) * 10))),*/

            Browser.Window.ExecuteJs("Hud.changeLBHpos", minimapRigthX * 100f + 1f, (1f - minimapBottomY) * 100f);
        }

        #endregion

        #region Speedometer Stuff

        public static void SwitchSpeedometer(bool value)
        {
            if (SpeedometerMustBeEnabled == value)
                return;

            if (!value)
            {
                Browser.Switch(Browser.IntTypes.HUD_Speedometer, false);

                PlayBeltOffSound(false);

                SpeedometerMustBeEnabled = false;

                return;
            }

            Vehicle veh = Player.LocalPlayer.Vehicle;

            if (veh?.Exists != true)
                return;

            var data = EntitiesData.Vehicles.VehicleData.GetData(veh);

            if (data == null)
            {
                Browser.Window.ExecuteJs("Hud.setFuel", 0);
                Browser.Window.ExecuteJs("Hud.setMileage", 0);

                SwitchEngineIcon(true);
                SwitchLightsIcon(true);
                SwitchDoorsIcon(false);
                SwitchBeltIcon(false);

                SwitchArrowIcon(true, false);
                SwitchArrowIcon(false, false);
            }
            else
            {
                SwitchEngineIcon(data.EngineOn);
                SwitchDoorsIcon(data.DoorsLocked);
                SwitchBeltIcon(PlayerData.GetData(Player.LocalPlayer).BeltOn);

                StartUpdateSpeedometerInfo();
            }

            SpeedometerMustBeEnabled = true;

            Browser.Window.ExecuteJs("Hud.updateSpeedometer", Math.Floor(RAGE.Game.Vehicle.GetVehicleModelMaxSpeed(veh.Model) * 3.6f) + 25);
            Browser.Window.ExecuteJs("Hud.updateSpeed", 0);


            if (!Phone.Phone.IsActive)
                Browser.Switch(Browser.IntTypes.HUD_Speedometer, Browser.IsActive(Browser.IntTypes.HUD_Left));

            StartUpdateSpeedometerSpeed();
        }

        private static async void StartUpdateSpeedometerSpeed()
        {
            while (Player.LocalPlayer.Vehicle != null)
            {
                var spd = (int)Player.LocalPlayer.Vehicle.GetSpeedKm();

                if (spd != LastSpeed)
                    Browser.Window.ExecuteJs("Hud.updateSpeed", LastSpeed = spd);

                await RAGE.Game.Invoker.WaitAsync(Settings.App.Static.SPEEDOMETER_UPDATE_SPEED);
            }

            //HUD.SwitchSpeedometer(false);
        }

        private static async void StartUpdateSpeedometerInfo()
        {
            var data = EntitiesData.Vehicles.VehicleData.GetData(Player.LocalPlayer.Vehicle);

            while (Player.LocalPlayer.Vehicle != null)
            {
                Browser.Window.ExecuteJs("Hud.setFuel", Utils.Convert.ToInt32(data.FuelLevel));
                Browser.Window.ExecuteJs("Hud.setMileage", Utils.Convert.ToInt32(data.Mileage) / 1000);

                Player driver = Utils.Game.Misc.GetPlayerByHandle(Player.LocalPlayer.Vehicle.GetPedInSeat(-1, 0), true);

                bool isCruiseControlOn = driver != null ? data.ForcedSpeed >= 8.3f : false;

                float currentSpeed = Player.LocalPlayer.Vehicle.GetSpeedKm();

                if (data.Data.HasCruiseControl && currentSpeed > 30 && Player.LocalPlayer.Vehicle.GetSpeedVector(true).Y > 0)
                    SwitchCruiseControlIcon(isCruiseControlOn);
                else
                    SwitchCruiseControlIcon(null);

                if (data.EngineOn)
                {
                    byte indState = data.IndicatorsState;

                    if (indState == 0)
                    {
                        SwitchArrowIcon(false, false);
                        SwitchArrowIcon(true, false);
                    }
                    else if (indState == 1)
                    {
                        SwitchArrowIcon(false, true);
                        SwitchArrowIcon(true, false);
                    }
                    else if (indState == 2)
                    {
                        SwitchArrowIcon(false, false);
                        SwitchArrowIcon(true, true);
                    }
                    else if (indState == 3)
                    {
                        SwitchArrowIcon(false, true);
                        SwitchArrowIcon(true, true);
                    }

                    SwitchLightsIcon(data.LightsOn);

                    if (!BeltOffSoundOn)
                    {
                        if (currentSpeed >= 100 && !PlayerData.GetData(Player.LocalPlayer).BeltOn)
                            PlayBeltOffSound(true);
                    }
                    else if (currentSpeed < 100)
                    {
                        PlayBeltOffSound(false);
                    }

                    await RAGE.Game.Invoker.WaitAsync(500);

                    SwitchArrowIcon(true, false);
                    SwitchArrowIcon(false, false);

                    await RAGE.Game.Invoker.WaitAsync(500);
                }
                else
                {
                    PlayBeltOffSound(false);

                    SwitchArrowIcon(true, false);
                    SwitchArrowIcon(false, false);

                    SwitchLightsIcon(false);

                    await RAGE.Game.Invoker.WaitAsync(1000);
                }
            }
        }

        #endregion
    }
}