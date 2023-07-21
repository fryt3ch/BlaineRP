using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.CEF
{
    [Script(int.MaxValue)]
    public class HUD 
    {
        #region HUD Menu
        [Script(int.MaxValue)]
        public class Menu 
        {
            public static bool IsActive { get => CEF.Browser.IsActive(CEF.Browser.IntTypes.HUD_Menu); }

            /// <summary>Все возможные типы выбора в меню</summary>
            /// <remarks>Порядок влияет на порядок отображения!</remarks>
            public enum Types
            {
                /// <summary>Меню</summary>
                Menu = 0,
                /// <summary>Документы</summary>
                Documents,
                /// <summary>Инвентарь</summary>
                Inventory,
                /// <summary>Телефон</summary>
                Phone,
                /// <summary>Служебный планшет полиции</summary>
                Fraction_Police_TabletPC,
                /// <summary>Меню фракции</summary>
                Fraction_Menu,
                /// <summary>Меню работы</summary>
                Job_Menu,
                /// <summary>Меню дома</summary>
                Menu_House,
                /// <summary>Меню квартиры</summary>
                Menu_Apartments,
                /// <summary>Анимации</summary>
                Animations,
                /// <summary>Меню локальных меток</summary>
                BlipsMenu,
                /// <summary>Меню раскрасок для оружия</summary>
                WeaponSkinsMenu,
            }

            private static Dictionary<Types, Action> Actions = new Dictionary<Types, Action>()
            {
                { Types.Menu, () => CEF.Menu.Show(CEF.Menu.SectionTypes.Last) },

                { Types.Documents, () => CEF.Documents.Show() },

                { Types.BlipsMenu, () => CEF.BlipsMenu.Show() },

                { Types.Menu_House, () => CEF.HouseMenu.ShowRequest() },

                { Types.Menu_Apartments, () => CEF.HouseMenu.ShowRequest() },

                { Types.WeaponSkinsMenu, () => Sync.Players.TryShowWeaponSkinsMenu() },

                { Types.Job_Menu, () => Data.Jobs.Job.ShowJobMenu() },

                { Types.Fraction_Menu, () => Data.Fractions.Fraction.ShowFractionMenu() },

                { Types.Fraction_Police_TabletPC, () => Data.Fractions.Police.ShowPoliceTabletPc() },
            };

            public static List<Types> CurrentTypes { get; private set; } = new List<Types>();

            private static List<int> TempBinds { get; set; } = new List<int>();

            public Menu()
            {
                Events.Add("HUD::Menu::Action", (object[] args) =>
                {
                    Switch(false);

                    Types type = (Types)args[0];

                    var kbAction = KeyBinds.HudMenuBinds.Where(x => x.Value == type).Select(x => KeyBinds.Get(x.Key).Action).FirstOrDefault();

                    if (kbAction != null)
                    {
                        kbAction.Invoke();

                        return;
                    }

                    Actions.GetValueOrDefault(type)?.Invoke();
                });
            }

            public static void UpdateCurrentTypes(bool enable, params Types[] types)
            {
                if (enable)
                {
                    foreach (var x in types)
                    {
                        if (!CurrentTypes.Contains(x))
                            CurrentTypes.Add(x);
                    }
                }
                else
                {
                    foreach (var x in types)
                        CurrentTypes.Remove(x);
                }

                CurrentTypes = CurrentTypes.OrderBy(x => x).ToList();
            }

            public static void SetCurrentTypes(params Types[] types) { CurrentTypes.Clear(); UpdateCurrentTypes(true, types); }

            public static void Switch(bool state, List<Types> types = null)
            {
                if (state)
                {
                    if (IsActive || Utils.IsAnyCefActive())
                        return;

                    if (types == null)
                        types = CurrentTypes;
                    else
                        types = types.OrderBy(x => x).ToList();

                    if ((types?.Count ?? 0) == 0)
                        return;

                    TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Switch(false)));

                    CEF.Browser.Switch(CEF.Browser.IntTypes.HUD_Menu, true);

                    Cursor.Show(true, true);

                    KeyBinds.Get(KeyBinds.Types.Menu).Disable();

                    CEF.Browser.Window.ExecuteJs("Hud.drawMenu", new object[] { types.Select(x => new object[] { x, Locale.HudMenu.Names.GetValueOrDefault(x) ?? "null" }) });

                    var cPos = RAGE.Ui.Cursor.Position;

                    if (cPos.X + 50 > GameEvents.ScreenResolution.X || cPos.Y + 50 > GameEvents.ScreenResolution.Y)
                    {
                        cPos.X = GameEvents.ScreenResolution.X / 2;
                        cPos.Y = GameEvents.ScreenResolution.Y / 2;
                    }

                    CEF.Browser.Window.ExecuteJs("Hud.positionMenu", cPos.X, cPos.Y);
                }
                else
                {
                    if (!IsActive)
                        return;

                    foreach (var x in TempBinds.ToList())
                        KeyBinds.Unbind(x);

                    TempBinds.Clear();

                    CEF.Browser.Switch(CEF.Browser.IntTypes.HUD_Menu, false);

                    Cursor.Show(false, false);

                    KeyBinds.Get(KeyBinds.Types.Menu).Enable();
                }
            }
        }
        #endregion

        public static bool IsActive { get => Browser.IsActiveOr(Browser.IntTypes.HUD_Top, Browser.IntTypes.HUD_Left); }

        public static bool SpeedometerEnabled => Browser.IsActive(Browser.IntTypes.HUD_Speedometer);
        public static bool SpeedometerMustBeEnabled { get; private set; }

        private static bool BeltOffSoundOn = false;

        public static int LastAmmo { get; private set; }
        public static int LastSpeed { get; private set; }

        public static Action InteractionAction = null;
        private static int InteractionBind = -1;

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
            Engine
        }

        public HUD()
        {
            SpeedometerMustBeEnabled = false;
            LastAmmo = int.MinValue;
            LastSpeed = -1;
        }

        public static void ShowHUD(bool value)
        {
            if (!Sync.Players.CharacterLoaded)
                return;

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (value)
            {
                UpdateLeftHUDPos();

                Browser.Switch(Browser.IntTypes.HUD_Top, true);
                Browser.Switch(Browser.IntTypes.HUD_Left, true);

                if (!CEF.Phone.IsActive)
                    Browser.Switch(Browser.IntTypes.HUD_Help, !Settings.User.Interface.HideHints);

                if (!Settings.User.Interface.HideQuest && Sync.Quest.ActualQuest != null)
                {
                    EnableQuest(true);
                }

                if (SpeedometerMustBeEnabled && !CEF.Phone.IsActive)
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
            var time = Settings.User.Interface.UseServerTime ? Sync.World.ServerTime : Sync.World.LocalTime;

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
                Browser.Window.ExecuteJs("Hud.drawInteract", text, KeyBinds.ExtraBind.GetKeyString(key));

            Browser.Switch(Browser.IntTypes.HUD_Interact, state);

            if (state)
            {
                //RAGE.Game.Audio.PlaySoundFrontend(-1, "Enter_Area", "DLC_Lowrider_Relay_Race_Sounds", true);

                if (InteractionBind != -1)
                    KeyBinds.Unbind(InteractionBind);

                InteractionBind = KeyBinds.Bind(key, true, () =>
                {
                    if (KeyBinds.Get(KeyBinds.Types.Interaction)?.IsDisabled == true)
                        return;

                    InteractionAction?.Invoke();
                });
            }
            else
            {
                if (InteractionBind != -1)
                {
                    KeyBinds.Unbind(InteractionBind);

                    InteractionBind = -1;
                }
            }
        }

        public static void SetQuestParams(string questGiver, string questName, string goal, Sync.Quest.QuestData.ColourTypes cType) => CEF.Browser.Window.ExecuteJs("Hud.drawQuest", new object[] { new object[] { questName, questGiver, goal, (int)cType } });

        public static void EnableQuest(bool state)
        {
            if (!Sync.Players.CharacterLoaded)
                return;

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (state)
            {
                if (Sync.Quest.ActualQuest != null)
                {
                    CEF.Browser.Switch(Browser.IntTypes.HUD_Quest, true);

                    Sync.Quest.ActualQuest.UpdateHudQuest();
                }
            }
            else
            {
                CEF.Browser.Switch(Browser.IntTypes.HUD_Quest, false);
            }
        }

        #endregion

        #region HUD Stuff
        public static void UpdateHUD()
        {
            var pos = Player.LocalPlayer.Position;

            Browser.Window.ExecuteJs("Hud.setLocation", ZoneNames.GetValueOrDefault(RAGE.Game.Zone.GetNameOfZone(pos.X, pos.Y, pos.Z).ToUpper()) ?? "null", Utils.GetStreetName(pos));
            Browser.Window.ExecuteJs("Hud.setOnline", Entities.Players.Count);
        }

        public static void UpdateLeftHUDPos()
        {
            float sfX = 1f / 20f;
            float sfY = 1f / 20f;

            float safezone = RAGE.Game.Graphics.GetSafeZoneSize();
            float aspectratio = RAGE.Game.Graphics.GetAspectRatio(false);

            float scaleX = 1f / GameEvents.ScreenResolution.X, scaleY = 1f / GameEvents.ScreenResolution.Y;

            float minimapWidth = scaleX * (GameEvents.ScreenResolution.X / (4 * aspectratio)) * (Minimap.MinimapZoomState == 2 ? 1.6f : 1f);

            float minimapRigthX = minimapWidth + (scaleX * (GameEvents.ScreenResolution.X * (sfX * (Math.Abs(safezone - 1f) * 10f))));
            float minimapBottomY = 1f - scaleY * (GameEvents.ScreenResolution.Y * (sfY * (Math.Abs(safezone - 1f) * 10f)));

            /*        width: scaleX * (resolution.x / (4 * aspectRatio)),
                    height: scaleY * (resolution.y / 5.674),
                    scaleX: scaleX,
                    scaleY: scaleY,
                    leftX: scaleX * (resolution.x * (sfX * (Math.abs(safeZone - 1.0) * 10))),
                    bottomY: 1.0 - scaleY * (resolution.y * (sfY * (Math.abs(safeZone - 1.0) * 10))),*/

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

            var veh = Player.LocalPlayer.Vehicle;

            if (veh?.Exists != true)
                return;

            var data = Sync.Vehicles.GetData(veh);

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
                SwitchBeltIcon(Sync.Players.GetData(Player.LocalPlayer).BeltOn);

                StartUpdateSpeedometerInfo();
            }

            SpeedometerMustBeEnabled = true;

            Browser.Window.ExecuteJs("Hud.updateSpeedometer", Math.Floor(RAGE.Game.Vehicle.GetVehicleModelMaxSpeed(veh.Model) * 3.6f) + 25);
            Browser.Window.ExecuteJs("Hud.updateSpeed", 0);


            if (!CEF.Phone.IsActive)
                Browser.Switch(Browser.IntTypes.HUD_Speedometer, CEF.Browser.IsActive(Browser.IntTypes.HUD_Left));

            StartUpdateSpeedometerSpeed();
        }

        private static async void StartUpdateSpeedometerSpeed()
        {
            while (Player.LocalPlayer.Vehicle != null)
            {
                var spd = (int)(Player.LocalPlayer.Vehicle.GetSpeedKm());

                if (spd != LastSpeed)
                    Browser.Window.ExecuteJs("Hud.updateSpeed", LastSpeed = spd);

                await RAGE.Game.Invoker.WaitAsync(Settings.App.Static.SPEEDOMETER_UPDATE_SPEED);
            }

            //HUD.SwitchSpeedometer(false);
        }

        private static async void StartUpdateSpeedometerInfo()
        {
            var data = Sync.Vehicles.GetData(Player.LocalPlayer.Vehicle);

            while (Player.LocalPlayer.Vehicle != null)
            {
                Browser.Window.ExecuteJs("Hud.setFuel", Utils.ToInt32(data.FuelLevel));
                Browser.Window.ExecuteJs("Hud.setMileage", Utils.ToInt32(data.Mileage) / 1000);

                Player driver = Utils.GetPlayerByHandle(Player.LocalPlayer.Vehicle.GetPedInSeat(-1, 0), true);

                bool isCruiseControlOn = driver != null ? data.ForcedSpeed >= 8.3f : false;

                float currentSpeed = Player.LocalPlayer.Vehicle.GetSpeedKm();

                if (data.Data.HasCruiseControl && currentSpeed > 30 && Player.LocalPlayer.Vehicle.GetSpeedVector(true).Y > 0)
                    SwitchCruiseControlIcon(isCruiseControlOn);
                else
                    SwitchCruiseControlIcon(null);

                if (data.EngineOn)
                {
                    var indState = data.IndicatorsState;

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
                        if (currentSpeed >= 100 && !Sync.Players.GetData(Player.LocalPlayer).BeltOn)
                            PlayBeltOffSound(true);
                    }
                    else if (currentSpeed < 100)
                        PlayBeltOffSound(false);

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

        #region ZoneNames
        public static Dictionary<string, string> ZoneNames = new Dictionary<string, string>()
        {
            {"GALLI", "Галли"},
            {"OBSERV", "Обсерватория"},

            {"AIRP", "Аэропорт"},
            {"ALAMO", "Аламо-си"},
            {"ALTA", "Альта"},
            {"ARMYB", "Форт Занкудо"},
            {"BANHAMC", "Каньон Бэнхам"},
            {"BANNING", "Бэннинг"},
            {"BEACH", "Пляж Веспуччи"},
            {"BHAMCA", "Каньон Бэнхам"},
            {"BRADP", "Брэддок"},
            {"BRADT", "Туннель Брэддок"},
            {"BURTON", "Бёртон"},
            {"CALAFB", "Мост Калафия"},
            {"CANNY", "Каньон Рэйтон"},
            {"CCREAK", "Ручей Кэссиди"},
            {"CHAMH", "Чемберлейн-Хиллс"},
            {"CHIL", "Вайнвуд-Хиллс"},
            {"CHU", "Чумаш"},
            {"CMSW", "Окрестности Чилиад"},
            {"CYPRE", "Сайпресс-Флэтс" },
            {"DAVIS", "Дэвис"},
            {"DELBE", "Пляж Дель-Перро"},
            {"DELPE", "Дель-Перро"},
            {"DELSOL", "Ла Пуэрта"},
            {"DESRT", "Пустыня Гранд-Сенора"},
            {"DOWNT", "Даунтаун"},
            {"DTVINE", "Даунтаун-Вайнвуд"},
            {"EAST_V", "Восточный Вайнвуд"},
            {"EBURO", "Эль Бурро"},
            {"ELGORL", "Маяк Эль Гордо"},
            {"ELYSIAN", "Остров Элизиан"},
            {"GALFISH", "Галилея"},
            {"GOLF", "Гольф"},
            {"GRAPES", "Грэйпсид"},
            {"GREATC", "Грейт-Чаперрэл"},
            {"HARMO", "Гармони"},
            {"HAWICK", "Хавик"},
            {"HORS", "Трасса Вайнвуд"},
            {"HUMLAB", "Лаборатория"},
            {"JAIL", "Тюрьма Болингброук"},
            {"KOREAT", "Маленький Сеул"},
            {"LACT", "Водохранилище"},
            {"LAGO", "Болото Занкудо"},
            {"LDAM", "Плотина Лэнд-Экт"},
            {"LEGSQU", "Легион-Сквер"},
            {"LMESA", "Ла Меса"},
            {"LOSPUER", "Ла Пуэрта"},
            {"MIRR", "Миррор Парк"},
            {"MORN", "Морнинвуд"},
            {"MOVIE", "Ричардс-Маджестик"},
            {"MTCHIL", "Гора Чилиад"},
            {"MTGORDO", "Гора Гордо"},
            {"MTJOSE", "Гора Джосайя"},
            {"MURRI", "Муррьета"},
            {"NCHU", "Северный Чумаш"},
            {"NOOSE", "N.O.O.S.E"},
            {"OCEANA", "Тихий Океан"},
            {"PALCOV", "Бухта Палето"},
            {"PALETO", "Палето-Бэй"},
            {"PALFOR", "Лес Палето"},
            {"PALHIGH", "Нагорье Паломино"},
            {"PALMPOW", "Энергостанция"},
            {"PBLUFF", "Пасифик-Блаффс"},
            {"PBOX", "Пиллбокс-Хилл"},
            {"PROCOB", "Пляж Прокопио"},
            {"PROL", "Пролог"},
            {"RANCHO", "Ранчо"},
            {"RGLEN", "Ричман-Глен"},
            {"RICHM", "Ричман"},
            {"ROCKF", "Рокфорд-Хиллс"},
            {"RTRAK", "Рэдвуд"},
            {"SANAND", "Сан-Андрэас"},
            {"SANCHIA", "Сан-Шаньский Хребет"},
            {"SANDY", "Сэнди-Шорс"},
            {"SKID", "Мишн-Роу"},
            {"SLAB", "Трэйлер-парк"},
            {"STAD", "Арена Мэйз-Банк"},
            {"STRAW", "Строуберри"},
            {"TATAMO", "Татавиамские горы"},
            {"TERMINA", "Терминал"},
            {"TEXTI", "Текстиль-Сити"},
            {"TONGVAH", "Тонгва-Хиллс"},
            {"TONGVAV", "Долина Тонгва"},
            {"VCANA", "Каналы Веспуччи"},
            {"VESP", "Веспуччи"},
            {"VINE", "Вайнвуд"},
            {"WINDF", "Ветряные мельницы"},
            {"WVINE", "Западный Вайнвуд"},
            {"ZANCUDO", "Река Занкудо"},
            {"ZP_ORT", "Порт Лос-Сантоса"},
            {"ZQ_UAR", "Карьер Дэвис-Кварц"}
        };
        #endregion
    }
}
