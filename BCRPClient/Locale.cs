using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace BCRPClient
{
    public static class Locale
    {
        #region General
        public static class General
        {
            public static string PropertyHouseString = "Дом";
            public static string PropertyApartmentsString = "Квартира";
            public static string PropertyGarageString = "Гараж";
            public static string PropertyBusinessClass = "Business";

            public static class Blip
            {
                public static Dictionary<Additional.ExtraBlip.Types, string> TypesNames = new Dictionary<Additional.ExtraBlip.Types, string>()
                {
                    { Additional.ExtraBlip.Types.GPS, "GPS-отметка" },
                    { Additional.ExtraBlip.Types.Furniture, "Мебель" },
                    { Additional.ExtraBlip.Types.AutoPilot, "Цель автопилота" },
                };

                public static string ApartmentsOwnedBlip = "{0}, кв. {1}";
                public static string GarageOwnedBlip = "{0}, #{1}";
            }

            public static class Discord
            {
                public static string Header = "Играет на Blaine RP";

                public static Dictionary<Additional.Discord.Types, string> Statuses = new Dictionary<Additional.Discord.Types, string>()
                {
                    { Additional.Discord.Types.Default, "" },
                    { Additional.Discord.Types.Login, "Входит в аккаунт" },
                    { Additional.Discord.Types.Registration, "Проходит регистрацию" },
                    { Additional.Discord.Types.CharacterSelect, "Выбирает персонажа" },
                };
            }

            #region Players
            public static class Players
            {
                public static string MaleNameDefault = "Гражданин";
                public static string FemaleNameDefault = "Гражданка";

                public static string Id = "({0})";

                public static string AdminLabel = "Администратор";

                public static string PlayerQuitText = "Игрок вышел {0} в {1}\nCID: #{2} | ID: {3}";

                public static Dictionary<Sync.Players.FractionTypes, string> FractionNames = new Dictionary<Sync.Players.FractionTypes, string>()
                {
                    { Sync.Players.FractionTypes.None, "Отсутствует" },
                };

                public static Dictionary<Sync.Players.SkillTypes, string> SkillNames = new Dictionary<Sync.Players.SkillTypes, string>()
                {
                    { Sync.Players.SkillTypes.Shooting, "Стрельба" },
                    { Sync.Players.SkillTypes.Fishing, "Рыболовство" },
                    { Sync.Players.SkillTypes.Cooking, "Кулинария" },
                    { Sync.Players.SkillTypes.Strength, "Сила" },
                };

                public static Dictionary<Sync.Players.SkillTypes, string> SkillNamesGenitive = new Dictionary<Sync.Players.SkillTypes, string>()
                {
                    { Sync.Players.SkillTypes.Shooting, "стрельбы" },
                    { Sync.Players.SkillTypes.Fishing, "рыболовства" },
                    { Sync.Players.SkillTypes.Cooking, "кулинарии" },
                    { Sync.Players.SkillTypes.Strength, "силы" },
                };

                public static Dictionary<Sync.Players.AchievementTypes, (string Title, string Desc)> AchievementTexts = new Dictionary<Sync.Players.AchievementTypes, (string, string)>()
                {
                    { Sync.Players.AchievementTypes.SR1, ("В яблочко!", "Получите навык стрельбы 80 в тире") },
                    { Sync.Players.AchievementTypes.SR2, ("Концентрация", "Продержите точность 100% в тире при навыке стрельбы 100") }
                };
            }
            #endregion

            public static class Business
            {
                public static string InfoColshape = "{0} #{1}";

                public static string NothingItem = "Ничего";

                public static string TuningNeon = "Неон";
                public static string TuningColours = "Цвета покраски";
                public static string TuningPearl = "Перламутр";
                public static string TuningWheelColour = "Цвет покрышек";
                public static string TuningTyreSmokeColour = "Цвет дыма от колес";

                public static string ShootingRangeTitle = "Тир";
            }

            public static class NPC
            {
                public static string NotFamiliarMale = "Незнакомец";
                public static string NotFamiliarFemale = "Незнакомка";

                public static Dictionary<string, string> TypeNames = new Dictionary<string, string>()
                {
                    { "quest", "Квестодатель" },

                    { "seller", "Продавец" },
                    { "bank", "Работник банка" },
                    { "vpound", "Работник штрафстоянки" },
                };

                public static Dictionary<Data.Dialogue.TimeTypes, Dictionary<int, string>> TimeWords = new Dictionary<Data.Dialogue.TimeTypes, Dictionary<int, string>>()
                {
                    {
                        Data.Dialogue.TimeTypes.Morning,

                        new Dictionary<int, string>()
                        {
                            { 0, "утро" },
                        }
                    },

                    {
                        Data.Dialogue.TimeTypes.Day,

                        new Dictionary<int, string>()
                        {
                            { 0, "день" },
                        }
                    },

                    {
                        Data.Dialogue.TimeTypes.Evening,

                        new Dictionary<int, string>()
                        {
                            { 0, "вечер" },
                        }
                    },

                    {
                        Data.Dialogue.TimeTypes.Night,

                        new Dictionary<int, string>()
                        {
                            { 0, "ночь" },
                        }
                    },
                };
            }

            public static class Containers
            {
                public static Dictionary<CEF.Inventory.ContainerTypes, string> Names = new Dictionary<CEF.Inventory.ContainerTypes, string>()
                {
                    { CEF.Inventory.ContainerTypes.None, "null" },
                    { CEF.Inventory.ContainerTypes.Trunk, "Багажник" },
                    { CEF.Inventory.ContainerTypes.Locker, "Шкаф" },
                    { CEF.Inventory.ContainerTypes.Storage, "Склад" },
                    { CEF.Inventory.ContainerTypes.Crate, "Ящик" },
                    { CEF.Inventory.ContainerTypes.Fridge, "Холодильник" },
                    { CEF.Inventory.ContainerTypes.Wardrobe, "Гардероб" },
                };
            }

            public static class Inventory
            {
                public static class Actions
                {
                    public static string TakeOff = "Снять";
                    public static string TakeOn = "Надеть";

                    public static string ToHands = "В руки";
                    public static string FromHands = "Из рук";

                    public static string Load = "Зарядить";
                    public static string Unload = "Разрядить";

                    public static Dictionary<Sync.WeaponSystem.Weapon.ComponentTypes, string> WeaponComponentsTakeOffStrings = new Dictionary<Sync.WeaponSystem.Weapon.ComponentTypes, string>()
                    {
                        { Sync.WeaponSystem.Weapon.ComponentTypes.Suppressor, "Снять глушитель" },
                        { Sync.WeaponSystem.Weapon.ComponentTypes.Grip, "Снять рукоятку" },
                        { Sync.WeaponSystem.Weapon.ComponentTypes.Scope, "Снять прицел" },
                        { Sync.WeaponSystem.Weapon.ComponentTypes.Flashlight, "Снять фонарик" },
                    };

                    public static string Drop = "Выбросить";

                    public static string Reset = "Переключить";

                    public static string Split = "Разделить";
                    public static string Shift = "Переложить";
                    public static string ShiftTrade = "В обмен";
                    public static string ShiftOutOfTrade = "Убрать из обмена";

                    public static string Use = "Использовать";

                    public static string FindVehicle = "Найти транспорт";
                }
            }

            public static class Documents
            {
                public static string SexMale = "мужской";
                public static string SexFemale = "женский";

                public static string NotMarriedMale = "не женат";
                public static string NotMarriedFemale = "не замужем";

                public static string VehiclePassportNoPlate = "отсутствует";
            }

            public static class Animations
            {
                public static string CancelText = "Нажмите {0}, чтобы отменить текущую анимацию";

                public static string CancelTextCarryA = "Нажмите {0}, чтобы перестать нести человека";
                public static string CancelTextCarryB = "Нажмите {0}, чтобы слезть с человека";

                public static string CancelTextInTrunk= "Нажмите {0}, чтобы вылезти из багажника";

                public static string CancelTextPushVehicle = "Нажмите W/A/S/D, чтобы перестать толкать";

                public static string TextDoPuffSmoke = "Нажмите ЛКМ, чтобы сделать затяжку [{0}]";
                public static string TextToMouthSmoke = "Нажмите ALT, чтобы зажать зубами";
                public static string TextToHandSmoke = "Нажмите ALT, чтобы взять в руку";
                public static string CancelTextSmoke = "Нажмите {0}, чтобы перестать курить";

                public static Dictionary<CEF.Animations.AnimSectionTypes, (string SectionName, Dictionary<Sync.Animations.OtherTypes, string> Names)> Anims = new Dictionary<CEF.Animations.AnimSectionTypes, (string SectionName, Dictionary<Sync.Animations.OtherTypes, string> Names)>
                {
                    {
                        CEF.Animations.AnimSectionTypes.Social,

                        (
                            "Социальные",
                            
                            new Dictionary<Sync.Animations.OtherTypes, string>()
                            {
                                { Sync.Animations.OtherTypes.Busted, "Поднять руки за голову" },
                                { Sync.Animations.OtherTypes.Busted2, "Руки за голову на коленях" },
                                { Sync.Animations.OtherTypes.Hysterics, "Испугаться и сидеть в истерике" },
                                { Sync.Animations.OtherTypes.GetScared, "Сесть испуганно" },
                                { Sync.Animations.OtherTypes.MonologueEmotional, "Эмоционально говорить что-либо" },
                                { Sync.Animations.OtherTypes.GiveUp, "Сдаться" },
                                { Sync.Animations.OtherTypes.GiveUp2, "Сдаться #2" },
                                { Sync.Animations.OtherTypes.Joy, "Радоваться #1" },
                                { Sync.Animations.OtherTypes.Joy2, "Радоваться #2" },
                                { Sync.Animations.OtherTypes.Cheer, "Подбадривать #1" },
                                { Sync.Animations.OtherTypes.Cheer2, "Подбадривать #2" },
                                { Sync.Animations.OtherTypes.Attention, "Махать руками, привлекая внимание" },
                                { Sync.Animations.OtherTypes.Respect, "Респект" },
                                { Sync.Animations.OtherTypes.Respect2, "Респект #2" },
                                { Sync.Animations.OtherTypes.Clap, "Хлопать в ладоши" },
                                { Sync.Animations.OtherTypes.Clap2, "Хлопать в ладоши #2" },
                                { Sync.Animations.OtherTypes.Clap3, "Аплодировать" },
                                { Sync.Animations.OtherTypes.Salute, "Отдать честь" },
                                { Sync.Animations.OtherTypes.Salute2, "Отдать честь #2" },
                                { Sync.Animations.OtherTypes.Explain, "Объяснять что-то" },
                                { Sync.Animations.OtherTypes.WagFinger, "Грозить пальцем" },
                                { Sync.Animations.OtherTypes.Facepalm, "Facepalm [рукалицо]" },
                                { Sync.Animations.OtherTypes.KeepChest, "Держаться за грудь" },
                                { Sync.Animations.OtherTypes.Goat, "Рок-н-ролл" },
                                { Sync.Animations.OtherTypes.UCrazy, "Крутить пальцем у виска" },
                                { Sync.Animations.OtherTypes.AirKiss, "Воздушный поцелуй" },
                                { Sync.Animations.OtherTypes.AirKiss2, "Воздушный поцелуй #2" },
                                { Sync.Animations.OtherTypes.AirKiss3, "Воздушный поцелуй #3" },
                                { Sync.Animations.OtherTypes.Heartbreak, "Держаться за сердце" },
                                { Sync.Animations.OtherTypes.Peace, "Peace [пис/мир]" },
                                { Sync.Animations.OtherTypes.Like, "Лайк [большой палец вверх]" },
                                { Sync.Animations.OtherTypes.Vertigo, "Головокружение" },
                                { Sync.Animations.OtherTypes.FlirtOnCar, "Флиртовать, облокотившись о что-либо" },
                                { Sync.Animations.OtherTypes.Cry, "Плакать" },
                                { Sync.Animations.OtherTypes.ThreatToKill, "Угрожать убить" },
                                { Sync.Animations.OtherTypes.FingerShot, "Выстрел из пальца" },
                                { Sync.Animations.OtherTypes.NumberMe, "На созвоне" },
                                { Sync.Animations.OtherTypes.WannaFight, "Вызывать на бой" },
                                { Sync.Animations.OtherTypes.GuardStop, "Остановить человека [как охранник]" },
                                { Sync.Animations.OtherTypes.Muscles, "Хвастаться мускулами" },
                                { Sync.Animations.OtherTypes.Muscles2, "Хвастаться мускулами #2" },
                                { Sync.Animations.OtherTypes.TakeFlirt, "Заигрывающе поднять что-то" },
                                { Sync.Animations.OtherTypes.CoquettishlyStand, "Стоять кокетливо" },
                                { Sync.Animations.OtherTypes.CoquettishlyWave, "Махать кокетливо" },
                                { Sync.Animations.OtherTypes.KeepCalm, "Сохраняйте спокойствие" },
                                { Sync.Animations.OtherTypes.CheckMouthOdor, "Проверять запах изо рта" },
                                { Sync.Animations.OtherTypes.ShakeHandsFear, "Испуганно трясти руками" },
                                { Sync.Animations.OtherTypes.LowWave, "Слабо махать рукой" },
                                { Sync.Animations.OtherTypes.CoverFace, "Прикрывать лицо рукой" },
                            }
                        )
                    },

                    {
                        CEF.Animations.AnimSectionTypes.Dialogs,

                        (
                            "Для диалогов",

                            new Dictionary<Sync.Animations.OtherTypes, string>()
                            {
                                { Sync.Animations.OtherTypes.Argue, "Спорить, скрестив руки" },
                                { Sync.Animations.OtherTypes.Nod, "Кивать, скрестив руки" },
                                { Sync.Animations.OtherTypes.Agree, "Соглашаться после раздумий" },
                                { Sync.Animations.OtherTypes.WhatAPeople, "Радостное приветствие" },
                                { Sync.Animations.OtherTypes.Rebellion, "Возмущенно раскидывать руками" },
                                { Sync.Animations.OtherTypes.Listen, "Слушать" },
                                { Sync.Animations.OtherTypes.Listen1, "Слушать, одобрительно кивая" },
                                { Sync.Animations.OtherTypes.Listen2, "Слушать, расставив ноги" },
                                { Sync.Animations.OtherTypes.Worry, "Смотреть, беспокоясь" },
                                { Sync.Animations.OtherTypes.MonologueEmotional2, "Рассказывать что-то, жестикулируя" },
                                { Sync.Animations.OtherTypes.Listen3, "Слушать, скрестив руки" },
                                { Sync.Animations.OtherTypes.Waiting, "Говорить, ожидая кого-либо" },
                                { Sync.Animations.OtherTypes.SpeakAgree, "Говорить одобрительно" },
                                { Sync.Animations.OtherTypes.ListenBro, "Слушать #2" },
                                { Sync.Animations.OtherTypes.Listen4, "Слушать скромно" },
                                { Sync.Animations.OtherTypes.Explain2, "Объяснять доходчиво" },
                                { Sync.Animations.OtherTypes.Goodbye, "Прощаться" },
                                { Sync.Animations.OtherTypes.Speak, "Стоять и говорить" },
                                { Sync.Animations.OtherTypes.GoodbyeBow, "Откланяться на прощание" },
                            }
                        )
                    },

                    {
                        CEF.Animations.AnimSectionTypes.Reactions,

                        (
                            "Реакции",

                            new Dictionary<Sync.Animations.OtherTypes, string>()
                            {
                                { Sync.Animations.OtherTypes.Agree2, "Соглашаться" },
                                { Sync.Animations.OtherTypes.Disagree, "Отказываться" },
                                { Sync.Animations.OtherTypes.Disagree2, "Отказываться, жестикуляруя" },
                                { Sync.Animations.OtherTypes.Disagree3, "Отрицательно махать головой" },
                                { Sync.Animations.OtherTypes.IDontKnow, "Пожимать плечами" },
                                { Sync.Animations.OtherTypes.Heartbreak2, "Схватиться за сердце" },
                                { Sync.Animations.OtherTypes.Agree3, "Одобрение" },
                                { Sync.Animations.OtherTypes.BadSmell, "Почувствовать плохой запах" },
                                { Sync.Animations.OtherTypes.IWatchU, "Я слежу за вами" },
                                { Sync.Animations.OtherTypes.FuckU, "Выкуси" },
                                { Sync.Animations.OtherTypes.ThatsHowUDoThat, "Вот как это делается" },
                                { Sync.Animations.OtherTypes.Enough, "Хватит" },
                                { Sync.Animations.OtherTypes.Sad, "Расстроиться" },
                                { Sync.Animations.OtherTypes.BrainExplosion, "Взрыв мозга" },
                                { Sync.Animations.OtherTypes.Agree4, "Одобрительно покивать" },
                                { Sync.Animations.OtherTypes.Disagree4, "Неодобрительно покивать" },
                                { Sync.Animations.OtherTypes.Bewilderment, "Недоумение" },
                                { Sync.Animations.OtherTypes.Agree5, "Недовольно согласиться" },
                                { Sync.Animations.OtherTypes.IDontKnow2, "Разводить руками" },
                                { Sync.Animations.OtherTypes.Surprised, "Удивленно посмотреть вниз" },
                                { Sync.Animations.OtherTypes.Surprised2, "Удивленно трясти руками" },
                            }
                        )
                    },

                    {
                        CEF.Animations.AnimSectionTypes.SeatLie,

                        (
                            "Сидеть/лежать",

                            new Dictionary<Sync.Animations.OtherTypes, string>()
                            {
                                { Sync.Animations.OtherTypes.Lie, "Лежать" },
                                { Sync.Animations.OtherTypes.Lie2, "Лежать #2" },
                                { Sync.Animations.OtherTypes.Lie3, "Лежать, держась за голову" },
                                { Sync.Animations.OtherTypes.Lie4, "Лежать на животе" },
                                { Sync.Animations.OtherTypes.Lie5, "Лежать на животе #2" },
                                { Sync.Animations.OtherTypes.Lie6, "Лежать [как раненый]" },
                                { Sync.Animations.OtherTypes.Lie7, "Лежать на спине" },
                                { Sync.Animations.OtherTypes.Lie8, "Лежать, закрыв лицо" },
                                { Sync.Animations.OtherTypes.Seat, "Сидеть у ног" },
                                { Sync.Animations.OtherTypes.Seat2, "Сидеть, сложа руки" },
                                { Sync.Animations.OtherTypes.Lie9, "Лежать в конвульсиях" },
                                { Sync.Animations.OtherTypes.Seat3, "Присесть на колено" },
                                { Sync.Animations.OtherTypes.Seat4, "Сидеть на чем-либо" },
                                { Sync.Animations.OtherTypes.Seat5, "Сидеть на чем-либо #2" },
                                { Sync.Animations.OtherTypes.Seat6, "Сидеть на чем-либо #3" },
                                { Sync.Animations.OtherTypes.Seat7, "Сидеть на чем-либо #4" },
                                { Sync.Animations.OtherTypes.Seat8, "Сидеть на капоте" },
                                { Sync.Animations.OtherTypes.Seat9, "Сидеть полулежа" },
                                { Sync.Animations.OtherTypes.Seat10, "Сидеть как йог" },
                                { Sync.Animations.OtherTypes.Seat11, "Сидеть, облакотившись" },
                                { Sync.Animations.OtherTypes.Seat12, "Сидеть и грустить" },
                                { Sync.Animations.OtherTypes.Seat13, "Сидеть и держаться за голову " },
                                { Sync.Animations.OtherTypes.Seat14, "Сидеть на корточках" },
                                { Sync.Animations.OtherTypes.Seat15, "Сидеть, оперевшись" },
                                { Sync.Animations.OtherTypes.Seat16, "Сидеть, оперевшись #2" },
                                { Sync.Animations.OtherTypes.Seat17, "Оглядываться, сидя на корточках" },
                                { Sync.Animations.OtherTypes.Seat18, "Сидеть на земле меланхолично" },
                                { Sync.Animations.OtherTypes.Seat19, "Сидеть за барной стойкой" },
                                { Sync.Animations.OtherTypes.Seat20, "Сидеть за барной стойкой #2" },
                                { Sync.Animations.OtherTypes.Seat21, "Сидеть, скучая" },
                                { Sync.Animations.OtherTypes.Seat22, "Сидеть на ступенях" },
                                { Sync.Animations.OtherTypes.Seat23, "Сидеть в испуге" },
                            }
                        )
                    },

                    {
                        CEF.Animations.AnimSectionTypes.Sport,

                        (
                            "Спортивные",

                            new Dictionary<Sync.Animations.OtherTypes, string>()
                            {
                                { Sync.Animations.OtherTypes.Press, "Пресс" },
                                { Sync.Animations.OtherTypes.PushUps, "Отжимания" },
                                { Sync.Animations.OtherTypes.PushUps2, "Глубокие отжимания" },
                                { Sync.Animations.OtherTypes.Backflip, "Сальто назад" },
                                { Sync.Animations.OtherTypes.Fists, "Разминать кулаки" },
                                { Sync.Animations.OtherTypes.Yoga, "Заниматься йогой" },
                                { Sync.Animations.OtherTypes.Yoga2, "Заниматься йогой #2" },
                                { Sync.Animations.OtherTypes.Yoga3, "Заниматься йогой #3" },
                                { Sync.Animations.OtherTypes.Yoga4, "Элемент йоги" },
                                { Sync.Animations.OtherTypes.Yoga5, "Элемент йоги #2" },
                                { Sync.Animations.OtherTypes.Yoga6, "Элемент йоги #3" },
                                { Sync.Animations.OtherTypes.Run, "Бег на месте" },
                                { Sync.Animations.OtherTypes.Pose, "Позировать" },
                                { Sync.Animations.OtherTypes.Swallow, "Сделать ласточку" },
                                { Sync.Animations.OtherTypes.Meditation, "Медитация" },
                                { Sync.Animations.OtherTypes.RunFemale, "Пробежка на месте [Ж]" },
                                { Sync.Animations.OtherTypes.RunMale, "Пробежка на месте [М]" },
                                { Sync.Animations.OtherTypes.Karate, "Карате" },
                                { Sync.Animations.OtherTypes.Box, "Бокс с тенью" },
                            }
                        )
                    },

                    {
                        CEF.Animations.AnimSectionTypes.Indecent,

                        (
                            "Непристойные",

                            new Dictionary<Sync.Animations.OtherTypes, string>()
                            {
                                { Sync.Animations.OtherTypes.FuckU2, "Показывать фак [средний палец]" },
                                { Sync.Animations.OtherTypes.FuckU3, "Показывать фак #2" },
                                { Sync.Animations.OtherTypes.FuckU4, "Показывать фак всем вокруг" },
                                { Sync.Animations.OtherTypes.FuckU5, "Показывать фак эмоционально" },
                                { Sync.Animations.OtherTypes.Jerk, "Дергать рукой" },
                                { Sync.Animations.OtherTypes.Chicken, "Изображать курицу" },
                                { Sync.Animations.OtherTypes.Ass, "Повернуться задницей" },
                                { Sync.Animations.OtherTypes.FuckFingers, "Засовывать пальцы в кулак" },
                                { Sync.Animations.OtherTypes.PickNose, "Ковыряться в носу" },
                                { Sync.Animations.OtherTypes.Dumb, "Кривляться" },
                                { Sync.Animations.OtherTypes.Tease, "Дразнить" },
                                { Sync.Animations.OtherTypes.Dumb2, "Дурачиться" },
                                { Sync.Animations.OtherTypes.Dumb3, "Дурачиться #2" },
                                { Sync.Animations.OtherTypes.Dumb4, "Дурачиться #3" },
                                { Sync.Animations.OtherTypes.IGotSmthForU, "У меня для тебя кое-что есть" },
                                { Sync.Animations.OtherTypes.SnotShot, "Стрелять козявкой" },
                                { Sync.Animations.OtherTypes.Scratch, "Чесаться" },
                                { Sync.Animations.OtherTypes.ScratchAss, "Почесать попу" },
                                { Sync.Animations.OtherTypes.ShakeBoobs, "Трясти грудью" },
                                { Sync.Animations.OtherTypes.KeepCock, "Держаться за пах" },
                                { Sync.Animations.OtherTypes.IndecentJoy, "Неприлично радоваться" },
                                { Sync.Animations.OtherTypes.IndecentJoy2, "Неприлично радоваться #2" },
                                { Sync.Animations.OtherTypes.SexMale, "Секс [активная роль]" },
                                { Sync.Animations.OtherTypes.SexFemale, "Секс [пассивная роль]" },
                                { Sync.Animations.OtherTypes.SexMale2, "Секс [активная роль] #2" },
                                { Sync.Animations.OtherTypes.SexFemale2, "Секс [пассивная роль] #2" },
                                { Sync.Animations.OtherTypes.SexMale3, "Секс [активная роль] #3" },
                            }
                        )
                    },

                    {
                        CEF.Animations.AnimSectionTypes.StandPoses,

                        (
                            "Стойки",

                            new Dictionary<Sync.Animations.OtherTypes, string>()
                            {
                                { Sync.Animations.OtherTypes.GuardStand, "Стойка охранника" },
                                { Sync.Animations.OtherTypes.GuardStand2, "Стойка охранника #2" },
                                { Sync.Animations.OtherTypes.Stand, "Стоять задумчиво" },
                                { Sync.Animations.OtherTypes.Stand2, "Стоять с рукой на поясе" },
                                { Sync.Animations.OtherTypes.Stand3, "Стоять, сложа руки" },
                                { Sync.Animations.OtherTypes.Stand4, "Стоять, сложа руки #2" },
                                { Sync.Animations.OtherTypes.Stand5, "Стоять, сложа руки #3" },
                                { Sync.Animations.OtherTypes.Stand6, "Стоять, сложа руки #4" },
                                { Sync.Animations.OtherTypes.Stand7, "Стоять, облоктившись" },
                                { Sync.Animations.OtherTypes.Stand8, "Стоять, облокотившись #2" },
                                { Sync.Animations.OtherTypes.Stand9, "Стоять, облокотившись #3" },
                                { Sync.Animations.OtherTypes.Stand10, "Стоять, облокотившись #4" },
                                { Sync.Animations.OtherTypes.Stand11, "Стоять с руками за спиной" },
                                { Sync.Animations.OtherTypes.Stand12, "Стоять как супергерой" },
                                { Sync.Animations.OtherTypes.Stand13, "Стоять как супергерой #2" },
                                { Sync.Animations.OtherTypes.Stand14, "Стоять с руками на поясе" },
                                { Sync.Animations.OtherTypes.Stand15, "Стоять застенчиво" },
                                { Sync.Animations.OtherTypes.Stand16, "Стоять застенчиво #2" },
                                { Sync.Animations.OtherTypes.Stand17, "Стоять пьяным" },
                                { Sync.Animations.OtherTypes.Stand18, "Стоять, осматриваясь по сторонам" },
                                { Sync.Animations.OtherTypes.Stand19, "Стоять с ногой на возвышении" },
                                { Sync.Animations.OtherTypes.Stand20, "Стоять устало" },
                                { Sync.Animations.OtherTypes.Stand21, "Стоять неуверенно" },
                                { Sync.Animations.OtherTypes.Stand22, "Стоять, скрестив руки" },
                                { Sync.Animations.OtherTypes.Stand23, "Стоять, неодобрительно оглядываясь" },
                                { Sync.Animations.OtherTypes.Stand24, "Стоять, недоуменно оглядываясь" },
                                { Sync.Animations.OtherTypes.Stand25, "Стоять, переминаясь" },
                                { Sync.Animations.OtherTypes.Stand26, "Стоять, переминаясь #2" },
                                { Sync.Animations.OtherTypes.Stand27, "Стоять, держа себя за руку" },
                                { Sync.Animations.OtherTypes.Stand28, "Стоять, облокотившись к стене" },
                                { Sync.Animations.OtherTypes.Stand29, "Стоять, перебирая руками" },
                                { Sync.Animations.OtherTypes.Stand30, "Стоять задумчиво, оперевшись на что-либо" },
                                { Sync.Animations.OtherTypes.Stand31, "Стоять, скучая облокотившись" },
                                { Sync.Animations.OtherTypes.Stand32, "Стоять, облокотившись и скрестив руки" },
                                { Sync.Animations.OtherTypes.Stand33, "Стоять, смотря по сторонам" },
                                { Sync.Animations.OtherTypes.Stand34, "Стоять, облокотившись и скрестив ноги" },
                                { Sync.Animations.OtherTypes.Stand35, "Стоять, оглядываясь и переминаясь" },
                                { Sync.Animations.OtherTypes.Stand36, "Стоять, ожидая и почесываясь" },
                                { Sync.Animations.OtherTypes.Stand37, "Слушать, переминаясь" },
                                { Sync.Animations.OtherTypes.Stand38, "Стоять, облокотившись вбок" },
                                { Sync.Animations.OtherTypes.Stand39, "Стоять печально" },
                                { Sync.Animations.OtherTypes.Stand40, "Качаться пьяным" },
                                { Sync.Animations.OtherTypes.Stand41, "Стоять, засыпая" },
                                { Sync.Animations.OtherTypes.Stand42, "Стоять надменно" },
                            }
                        )
                    },

                    {
                        CEF.Animations.AnimSectionTypes.Dances,

                        (
                            "Танцы",

                            new Dictionary<Sync.Animations.OtherTypes, string>()
                            {
                                { Sync.Animations.OtherTypes.Dance, "Лезгинка" },
                                { Sync.Animations.OtherTypes.Dance2, "Танец руками" },
                                { Sync.Animations.OtherTypes.Dance3, "Флексить" },
                                { Sync.Animations.OtherTypes.Dance4, "Стриптиз" },
                                { Sync.Animations.OtherTypes.Dance5, "Стриптиз #2" },
                                { Sync.Animations.OtherTypes.Dance6, "Cтриптиз #3" },
                                { Sync.Animations.OtherTypes.Dance7, "Cтриптиз #4" },
                                { Sync.Animations.OtherTypes.Dance8, "Стриптиз #5" },
                                { Sync.Animations.OtherTypes.Dance9, "Лэп-дэнс" },
                                { Sync.Animations.OtherTypes.Dance10, "Пьяный стриптиз" },
                                { Sync.Animations.OtherTypes.Dance11, "Эротический танец" },
                                { Sync.Animations.OtherTypes.Dance12, "Сексуально завлекать" },
                                { Sync.Animations.OtherTypes.Dance13, "Сексуально пританцовывать" },
                                { Sync.Animations.OtherTypes.Dance14, "Завлекающий танец" },
                                { Sync.Animations.OtherTypes.Dance15, "Двигать тазом" },
                                { Sync.Animations.OtherTypes.Dance16, "Слушать музыку" },
                                { Sync.Animations.OtherTypes.Dance17, "Скромный танец" },
                                { Sync.Animations.OtherTypes.Dance18, "Кивать под музыку" },
                                { Sync.Animations.OtherTypes.Dance19, "Пританцовывать под музыку" },
                                { Sync.Animations.OtherTypes.Dance20, "Танец с похлопованием попы" },
                                { Sync.Animations.OtherTypes.Dance21, "Танец с оборотами" },
                                { Sync.Animations.OtherTypes.Dance22, "Танец робота" },
                                { Sync.Animations.OtherTypes.Dance23, "Танец робота #2" },
                                { Sync.Animations.OtherTypes.Dance24, "Танцевать как паучок" },
                                { Sync.Animations.OtherTypes.Dance25, "Произвольный танец" },
                                { Sync.Animations.OtherTypes.Dance26, "Легкий танец" },
                                { Sync.Animations.OtherTypes.Dance27, "Танец мачо" },
                                { Sync.Animations.OtherTypes.Dance28, "Танец мачо #2" },
                                { Sync.Animations.OtherTypes.Dance29, "Танец пингвина" },
                                { Sync.Animations.OtherTypes.Dance30, "Танец диджея" },
                                { Sync.Animations.OtherTypes.Dance31, "Танец диджея #2" },
                                { Sync.Animations.OtherTypes.Dance32, "Танцевать как курочка" },
                                { Sync.Animations.OtherTypes.Dance33, "Флексить как рэпер" },
                                { Sync.Animations.OtherTypes.Dance34, "Аккуратный танец" },
                                { Sync.Animations.OtherTypes.Dance35, "Современный танец" },
                                { Sync.Animations.OtherTypes.Dance36, "Танец забвения" },
                                { Sync.Animations.OtherTypes.Dance37, "Танец на месте" },
                                { Sync.Animations.OtherTypes.Dance38, "Танец на месте #2" },
                                { Sync.Animations.OtherTypes.Dance39, "Танец на месте #3" },
                                { Sync.Animations.OtherTypes.Dance40, "Танец зумбы" },
                                { Sync.Animations.OtherTypes.Dance41, "Танец зумбы #2" },
                                { Sync.Animations.OtherTypes.Dance42, "Танец зумбы #3" },
                                { Sync.Animations.OtherTypes.Dance43, "Клубный танец" },
                                { Sync.Animations.OtherTypes.Dance44, "Клубный танец #2" },
                                { Sync.Animations.OtherTypes.Dance45, "Клубный танец #3" },
                                { Sync.Animations.OtherTypes.Dance46, "Клубный танец #4" },
                                { Sync.Animations.OtherTypes.Dance47, "Клубный танец #5" },
                                { Sync.Animations.OtherTypes.Dance48, "Клубный танец #6" },
                                { Sync.Animations.OtherTypes.Dance49, "Клубный танец #7" },
                                { Sync.Animations.OtherTypes.Dance50, "Клубный танец #8" },
                                { Sync.Animations.OtherTypes.Dance51, "Танец лепестка" },
                                { Sync.Animations.OtherTypes.Dance52, "Танец пожилого человека" },
                                { Sync.Animations.OtherTypes.Dance53, "Танец заводной" },
                                { Sync.Animations.OtherTypes.Dance54, "Танец диско" },
                                { Sync.Animations.OtherTypes.Dance55, "Танец бедрами" },
                                { Sync.Animations.OtherTypes.Dance56, "Танец индийский" },
                                { Sync.Animations.OtherTypes.Dance57, "Танец счастливый" },
                                { Sync.Animations.OtherTypes.Dance58, "Танец шафл руками" },
                                { Sync.Animations.OtherTypes.Dance59, "Танец Skibidi" },
                                { Sync.Animations.OtherTypes.Dance60, "Танец c хлопаками" },
                                { Sync.Animations.OtherTypes.Dance61, "Танец улетный" },
                                { Sync.Animations.OtherTypes.Dance62, "Танец расслабленный" },
                                { Sync.Animations.OtherTypes.Dance63, "Танец лейла" },
                                { Sync.Animations.OtherTypes.Dance64, "Танец диджея #3" },
                                { Sync.Animations.OtherTypes.Dance65, "Танец электро" },
                                { Sync.Animations.OtherTypes.Dance66, "Танец загадочный" },
                                { Sync.Animations.OtherTypes.Dance67, "Танец игривый" },
                                { Sync.Animations.OtherTypes.Dance68, "Танец игривый #2" },
                                { Sync.Animations.OtherTypes.Dance69, "Танец Руки Вверх" },
                                { Sync.Animations.OtherTypes.Dance70, "Танец Руки Вверх #2" },
                                { Sync.Animations.OtherTypes.Dance71, "Танец Лапули" },
                                { Sync.Animations.OtherTypes.Dance72, "Танец Зазывающий" },
                                { Sync.Animations.OtherTypes.Dance73, "Танец манящий" },
                                { Sync.Animations.OtherTypes.Dance74, "Танец раскрепощенный" },
                                { Sync.Animations.OtherTypes.Dance75, "Танец Зайки" },
                                { Sync.Animations.OtherTypes.Dance76, "Танец вальяжный " },
                                { Sync.Animations.OtherTypes.Dance77, "Танец игривый" },
                                { Sync.Animations.OtherTypes.Dance78, "Танец с наклоном" },
                                { Sync.Animations.OtherTypes.Dance79, "Танец кокетки" },
                                { Sync.Animations.OtherTypes.Dance80, "Танец динамичный" },
                                { Sync.Animations.OtherTypes.Dance81, "Танец лапули #2" },
                                { Sync.Animations.OtherTypes.Dance82, "Танец Цыганочка" },
                                { Sync.Animations.OtherTypes.Dance83, "Танец Шейк" },
                                { Sync.Animations.OtherTypes.Dance84, "Танец мачо #3" },
                                { Sync.Animations.OtherTypes.Dance85, "Развязный танец" },
                                { Sync.Animations.OtherTypes.Dance86, "Танец извивающийся" },
                                { Sync.Animations.OtherTypes.Dance87, "Милый танец" },
                                { Sync.Animations.OtherTypes.Dance88, "Уличный танец" },
                                { Sync.Animations.OtherTypes.Dance89, "Танец кокетки #2" },
                                { Sync.Animations.OtherTypes.Dance90, "Танец заигрывающий" },
                                { Sync.Animations.OtherTypes.Dance91, "Танец с оборотами #2" },
                                { Sync.Animations.OtherTypes.Dance92, "Танец удачи" },
                                { Sync.Animations.OtherTypes.Dance93, "Бодрый танец" },
                                { Sync.Animations.OtherTypes.Dance94, "Танец с вилянием бедер" },
                                { Sync.Animations.OtherTypes.Dance95, "Танец кулачками легкий" },
                                { Sync.Animations.OtherTypes.Dance96, "Танцевать локтями" },
                                { Sync.Animations.OtherTypes.Dance97, "Расслабленный танец" },
                                { Sync.Animations.OtherTypes.Dance98, "Танец качающий" },
                                { Sync.Animations.OtherTypes.Dance99, "Стучать пальцами о пальцы" },
                                { Sync.Animations.OtherTypes.Dance100, "Танец сумасшедшего" },
                                { Sync.Animations.OtherTypes.Dance101, "Танец жизнерадостный" },
                                { Sync.Animations.OtherTypes.Dance102, "Танец активный" },
                                { Sync.Animations.OtherTypes.Dance103, "Победный танец" },
                                { Sync.Animations.OtherTypes.Dance104, "Танец волна" },
                            }
                        )
                    },

                    {
                        CEF.Animations.AnimSectionTypes.Situative,

                        (
                            "Ситуативные",

                            new Dictionary<Sync.Animations.OtherTypes, string>()
                            {
                                { Sync.Animations.OtherTypes.LookAtSmth, "Разглядывать что-то в руках" },
                                { Sync.Animations.OtherTypes.KnockDoor, "Стучать в дверь" },
                                { Sync.Animations.OtherTypes.CleanTable, "Протирать стол" },
                                { Sync.Animations.OtherTypes.WashSmth, "Мыть что-то" },
                                { Sync.Animations.OtherTypes.DJ, "Диджей" },
                                { Sync.Animations.OtherTypes.Guitar, "Воображаемая гитара" },
                                { Sync.Animations.OtherTypes.Drums, "Воображаемые барабаны" },
                                { Sync.Animations.OtherTypes.TossCoin, "Подбрасывать монетку" },
                                { Sync.Animations.OtherTypes.LookAround, "Осматриваться, сидя на колене" },
                                { Sync.Animations.OtherTypes.LookAtSmth2, "Разглядывать что-либо, приседая" },
                                { Sync.Animations.OtherTypes.PointAtSmth, "Указывать на что-либо" },
                                { Sync.Animations.OtherTypes.LookAtSmthGround, "Осматривать что-либо на земле" },
                                { Sync.Animations.OtherTypes.DropSig, "Эмоционально выкинуть сигарету" },
                            }
                        )
                    },

                    {
                        CEF.Animations.AnimSectionTypes.WithWeapon,

                        (
                            "Для оружия в руках",

                            new Dictionary<Sync.Animations.OtherTypes, string>()
                            {
                                { Sync.Animations.OtherTypes.AimLie, "Целиться лежа с оружием" },
                                { Sync.Animations.OtherTypes.AimCrouch, "Целиться на корточках с оружием" },
                                { Sync.Animations.OtherTypes.HurryUp, "Подгонять с оружием в руках" },
                                { Sync.Animations.OtherTypes.LookAroundWeapon, "Осматриваться с оружием" },
                                { Sync.Animations.OtherTypes.LookFromCoverWeapon, "Выглядывать из-за стены с оружием" },
                                { Sync.Animations.OtherTypes.CoverWeapon, "Стоять за стеной с оружием" },
                                { Sync.Animations.OtherTypes.CoverWeapon2, "Стоять за укрытием" },
                            }
                        )
                    },
                };

                public static Dictionary<Sync.Animations.EmotionTypes, string> Emotions = new Dictionary<Sync.Animations.EmotionTypes, string>()
                {
                    { Sync.Animations.EmotionTypes.None, "Обычный" },

                    { Sync.Animations.EmotionTypes.Happy, "Счастливый" },
                    { Sync.Animations.EmotionTypes.Joyful, "Радостный" },
                    { Sync.Animations.EmotionTypes.Smug, "Самодовольный" },

                    { Sync.Animations.EmotionTypes.Speculative, "Настороженный" },
                    { Sync.Animations.EmotionTypes.Mouthbreather, "Удивленный" },

                    { Sync.Animations.EmotionTypes.Sulking, "Грустный" },

                    { Sync.Animations.EmotionTypes.Grumpy,"Сердитый" },
                    { Sync.Animations.EmotionTypes.Grumpy2, "Сердитый #2" },
                    { Sync.Animations.EmotionTypes.Grumpy3, "Сердитый #3" },

                    { Sync.Animations.EmotionTypes.Angry, "Злой" },
                    { Sync.Animations.EmotionTypes.Stressed, "Яростный" },

                    { Sync.Animations.EmotionTypes.Shocked, "Шокированный" },
                    { Sync.Animations.EmotionTypes.Shocked2, "Шокированный #2" },

                    { Sync.Animations.EmotionTypes.NeverBlink, "Не моргающий" },
                    { Sync.Animations.EmotionTypes.OneEye, "Прицеливающийся" },

                    { Sync.Animations.EmotionTypes.Sleeping, "Спящий" },
                    { Sync.Animations.EmotionTypes.Sleeping2, "Спящий #2" },
                    { Sync.Animations.EmotionTypes.Sleeping3, "Спящий #3" },

                    { Sync.Animations.EmotionTypes.Weird, "Странный" },
                    { Sync.Animations.EmotionTypes.Weird2, "Странный #2" },
                    { Sync.Animations.EmotionTypes.Electrocuted, "Эпилепсик" },
                };

                public static Dictionary<Sync.Animations.WalkstyleTypes, string> Walkstyles = new Dictionary<Sync.Animations.WalkstyleTypes, string>()
                {
                    { Sync.Animations.WalkstyleTypes.None, "Обычный" },

                    { Sync.Animations.WalkstyleTypes.Wide, "Широкий" },
                    { Sync.Animations.WalkstyleTypes.Confident, "Уверенный" },
                    { Sync.Animations.WalkstyleTypes.Brave, "Храбрый" },
                    { Sync.Animations.WalkstyleTypes.Arrogant, "Высокомерный" },
                    { Sync.Animations.WalkstyleTypes.Posh, "Пафосный" },
                    { Sync.Animations.WalkstyleTypes.Posh2, "Пафосный #2" },

                    { Sync.Animations.WalkstyleTypes.Sassy, "Дерзкий" },
                    { Sync.Animations.WalkstyleTypes.Sassy2, "Дерзкий #2" },

                    { Sync.Animations.WalkstyleTypes.Swagger, "Развязный" },
                    { Sync.Animations.WalkstyleTypes.Shady, "Развязный #2" },
                    { Sync.Animations.WalkstyleTypes.Tough, "Крутой" },
                    { Sync.Animations.WalkstyleTypes.Tough2, "Крутой #2" },

                    { Sync.Animations.WalkstyleTypes.Sad, "Грустный" },
                    { Sync.Animations.WalkstyleTypes.Scared, "Испуганный" },
                    { Sync.Animations.WalkstyleTypes.Slow, "Медленный" },

                    { Sync.Animations.WalkstyleTypes.Hurry, "Спешащий" },
                    { Sync.Animations.WalkstyleTypes.Quick, "Спешащий #2" },

                    { Sync.Animations.WalkstyleTypes.Flee, "Парящий" },

                    { Sync.Animations.WalkstyleTypes.Hipster, "Хипстер" },
                    { Sync.Animations.WalkstyleTypes.Hobo, "Бродяга" },
                    { Sync.Animations.WalkstyleTypes.Muscle, "Силач" },

                    { Sync.Animations.WalkstyleTypes.Lester, "Храмой" },
                    { Sync.Animations.WalkstyleTypes.Lester2, "Храмой #2" },

                    { Sync.Animations.WalkstyleTypes.Sexy, "Сексуальный" },
                    { Sync.Animations.WalkstyleTypes.Chichi, "Манерный" },
                    { Sync.Animations.WalkstyleTypes.Femme, "Феминный" },
                    { Sync.Animations.WalkstyleTypes.Heels, "На каблуках" },
                    { Sync.Animations.WalkstyleTypes.Heels2, "На каблуках #2" },

                    { Sync.Animations.WalkstyleTypes.Cop, "Коп" },
                    { Sync.Animations.WalkstyleTypes.Cop2, "Коп #2" },
                    { Sync.Animations.WalkstyleTypes.Cop3, "Коп #3" },

                    { Sync.Animations.WalkstyleTypes.Drunk, "Пьяный" },
                    { Sync.Animations.WalkstyleTypes.Drunk2, "Пьяный #2" },
                    { Sync.Animations.WalkstyleTypes.Drunk3, "Пьяный #3" },
                    { Sync.Animations.WalkstyleTypes.Drunk4, "Пьяный #4" },

                    { Sync.Animations.WalkstyleTypes.Gangster, "Гангстер" },
                    { Sync.Animations.WalkstyleTypes.Gangster2, "Гангстер #2" },
                    { Sync.Animations.WalkstyleTypes.Gangster3, "Гангстер #3" },
                    { Sync.Animations.WalkstyleTypes.Gangster4, "Гангстер #4" },
                    { Sync.Animations.WalkstyleTypes.Gangster5, "Гангстер #5" },

                    { Sync.Animations.WalkstyleTypes.Guard, "Охранник" },
                };
            }

            public static class Game
            {

            }
        }
        #endregion

        #region Other
        public static class PauseMenu
        {
            public static string Money = "Наличные: {0}$ | Банк: {1}$";
        }

        public static class Scaleform
        {
            public static class Wasted
            {
                public static string Header = "Вы при смерти";

                public static string TextAttacker = "Атакующий: {0} | CID: #{1}";
                public static string TextSelf = "Несчастный случай";
            }

            public static string ShootingRangeCountdownTitle = "~g~Приготовьтесь!";
            public static string ShootingRangeCountdownText = "Начало через: {0}";

            public static string ShootingRangeScoreText = "Счёт: {0} / {1}";
            public static string ShootingRangeAccuracyText = "Точность: {0}%";
        }

        public static class Interaction
        {
            public static Dictionary<Additional.ExtraColshape.InteractionTypes, string> Names = new Dictionary<Additional.ExtraColshape.InteractionTypes, string>()
            {
                { Additional.ExtraColshape.InteractionTypes.HouseEnter, "для взаимодействия" },
                { Additional.ExtraColshape.InteractionTypes.HouseExit, "чтобы выйти" },
                { Additional.ExtraColshape.InteractionTypes.GarageExit, "чтобы выйти" },

                { Additional.ExtraColshape.InteractionTypes.Locker, "чтобы посмотреть шкаф" },
                { Additional.ExtraColshape.InteractionTypes.Wardrobe, "чтобы посмотреть гардероб" },
                { Additional.ExtraColshape.InteractionTypes.Fridge, "чтобы посмотреть холодильник" },

                { Additional.ExtraColshape.InteractionTypes.BusinessInfo, "для просмотра информации" },
                { Additional.ExtraColshape.InteractionTypes.BusinessEnter, "для взаимодействия" },

                { Additional.ExtraColshape.InteractionTypes.Interact, "для взаимодействия" },

                { Additional.ExtraColshape.InteractionTypes.NpcDialogue, "чтобы поговорить" },

                { Additional.ExtraColshape.InteractionTypes.ATM, "чтобы воспользоваться банкоматом" },

                { Additional.ExtraColshape.InteractionTypes.TuningEnter, "чтобы перейти к тюнингу" },

                { Additional.ExtraColshape.InteractionTypes.ShootingRangeEnter, "чтобы войти в тир [${0}]" },

                { Additional.ExtraColshape.InteractionTypes.ApartmentsRootEnter, "чтобы войти" },
                { Additional.ExtraColshape.InteractionTypes.ApartmentsRootExit, "чтобы выйти на улицу" },
                { Additional.ExtraColshape.InteractionTypes.ApartmentsRootElevator, "чтобы воспользоваться лифтом" },

                { Additional.ExtraColshape.InteractionTypes.GarageRootEnter, "для взаимодействия" },
            };
        }

        public static class Actions
        {
            public static string Drop = "Выбросить {0}?";
            public static string Split = "Разделить {0}?";
            public static string GetAmmo = "Достать патроны из {0}?";
            public static string LoadAmmo = "Зарядить {0}?";
            public static string Take = "Подобрать {0}?";

            public static string GiveCash = "Передать деньги {0}?";

            public static string HouseExitActionBoxHeader = "Выход";

            public static string HouseExitActionBoxOutside = "На улицу";
            public static string HouseExitActionBoxToGarage = "В гараж";
            public static string HouseExitActionBoxToHouse = "В дом";

            public static string GarageVehicleActionBoxHeader = "Загнать Т/С в гараж";

            public static string NumberplateSelectHeader = "Выбор номерного знака";

            public static string GarageVehicleSlotSelectHeader = "Выбор места в гараже";

            public static string VehiclePassportSelectHeader = "Выбор тех. паспорта";

            public static string VehiclePoundSelectHeader = "Выбор транспорта";

            public static string WeaponSkinsMenuSelectHeader = "Текущие раскраски оружия";

            public static Dictionary<Data.Items.WeaponSkin.ItemData.Types, string> WeaponSkinTypeNames = new Dictionary<Data.Items.WeaponSkin.ItemData.Types, string>()
            {
                { Data.Items.WeaponSkin.ItemData.Types.UniDef, "Универсальная (обыч.)" },
                { Data.Items.WeaponSkin.ItemData.Types.UniMk2, "Универсальная (Mk2)" },
            };
        }

        public static class HudMenu
        {
            public static Dictionary<CEF.HUD.Menu.Types, string> Names = new Dictionary<CEF.HUD.Menu.Types, string>()
            {
                { CEF.HUD.Menu.Types.Menu, "Меню" },
                { CEF.HUD.Menu.Types.Documents, "Документы" },
                { CEF.HUD.Menu.Types.Menu_House, "Меню дома" },
                { CEF.HUD.Menu.Types.Menu_Apartments, "Меню квартиры" },

                { CEF.HUD.Menu.Types.Inventory, "Инвентарь" },
                { CEF.HUD.Menu.Types.Phone, "Телефон" },
                { CEF.HUD.Menu.Types.Animations, "Меню анимаций" },

                { CEF.HUD.Menu.Types.BlipsMenu, "Меню меток" },

                { CEF.HUD.Menu.Types.WeaponSkinsMenu, "Раскраски оружия" },
            };
        }

        public static class TestDrive
        {
            public static string CloseText = "Нажмите ESC, чтобы закончить тест-драйв";
            public static string TuningText = "Нажмите F4, чтобы открыть меню тюнинга";
        }

        public static class Property
        {
            public static Dictionary<Data.Vehicles.Vehicle.Types, string> VehicleTypesNames = new Dictionary<Data.Vehicles.Vehicle.Types, string>()
            {
                { Data.Vehicles.Vehicle.Types.Car, "Автомобиль" },
                { Data.Vehicles.Vehicle.Types.Boat, "Лодка" },
                { Data.Vehicles.Vehicle.Types.Motorcycle, "Мотоцикл" },
                { Data.Vehicles.Vehicle.Types.Cycle, "Велосипед" },
                { Data.Vehicles.Vehicle.Types.Helicopter, "Вертолет" },
                { Data.Vehicles.Vehicle.Types.Plane, "Самолет" },
            };

            public static string VehicleTradeInfoStr = "{0} | {1} #{2}";
            public static string VehicleTradeInfoStr1 = "{0} #{1}";
            public static string HouseTradeInfoStr = "Дом #{0}";
            public static string ApartmentsTradeInfoStr = "{0}, кв. {1}";
            public static string GarageTradeInfoStr = "{0}, #{1}";
            public static string BusinessTradeInfoStr = "{0} #{1}";

            public static Dictionary<Data.Locations.Business.Types, string> BusinessNames = new Dictionary<Data.Locations.Business.Types, string>()
            {
                { Data.Locations.Business.Types.ClothesShop1, "Магазин спортивной одежды" },
                { Data.Locations.Business.Types.ClothesShop2, "Магазин премиальной одежды" },
                { Data.Locations.Business.Types.ClothesShop3, "Магазин брендовой одежды" },

                { Data.Locations.Business.Types.Market, "Магазин 24/7" },

                { Data.Locations.Business.Types.GasStation, "АЗС" },

                { Data.Locations.Business.Types.CarShop1, "Автосалон бюджетного сегмента" },

                { Data.Locations.Business.Types.BoatShop, "Лодочный салон" },

                { Data.Locations.Business.Types.AeroShop, "Салон воздушного транспорта" },

                { Data.Locations.Business.Types.TuningShop, "Тюнинг" },

                { Data.Locations.Business.Types.WeaponShop, "Оружейный магазин" },
            };

            public static Dictionary<Data.Locations.ApartmentsRoot.Types, string> ApartmentsRootNames = new Dictionary<Data.Locations.ApartmentsRoot.Types, string>()
            {
                { Data.Locations.ApartmentsRoot.Types.Cheap1, "ЖК Paleto" },
            };

            public static string NoOwner = "Государство";

            public static string BankNameDef = "Банковское отделение";
            public static string AtmNameDef = "Банковское отделение";

            public static string GarageRootNameDef = "Гаражный комплекс";
            public static string GarageRootName = "Гаражный комплекс #{0}";

            public static string ApartmentsRootTextLabel = "{0}\nЭтажей: {1}\nКвартир свободно: {2}/{3}";
            public static string ApartmentsTextLabel = "Квартира #{0}\nВладелец: {1}";
            public static string HouseTextLabel = "Дом #{0}\nВладелец: {1}";

            public static string ApartmentsRootElevatorTextLabel = "Лифт [{0} этаж]";

            public static string ApartmentsRootExitTextLabel = "Выход на улицу";
            public static string HouseExitTextLabel = "Выход";
        }

        public static class Shop
        {
            public static string ModDeletionTitle = "Удаление модификации";

            public static string ModDeletionText = "Вы собираетесь удалить {0} со своего транспорта.\n\nДанное действие необратимо!\nВыберите способ оплаты, чтобы продолжить.";
        }
        #endregion

        #region Notifications
        public static class Notifications
        {
            public static string DefHeader = "Уведомление";
            public static string ErrorHeader = "Ошибка";
            public static string ApproveHeader = "Подтверждение";

            public static class Blip
            {
                public static string Header = "GPS";

                public static string ReachedGPS = "Вы достигли точки маршрута!";

                public static Dictionary<Additional.ExtraBlip.Types, string> TypesText = new Dictionary<Additional.ExtraBlip.Types, string>()
                {
                    { Additional.ExtraBlip.Types.GPS, "Местоположение отмечено у вас на карте!" }
                };
            }

            public static class AntiSpam
            {
                public static string Header = "Анти-спам";

                public static string DontFlood = "Не так быстро!";
                public static string Warning = "Прекратите спаммить!\nВ случае продолжения, вы будете кикнуты!\nВаш лимит: {0}/{1}";

                public static string CooldownText1 = "Вы должны подождать некоторое время, прежде чем вновь сможете сделать это!";
                public static string CooldownText2 = "Вы устали, подождите некоторое время и возвращайтесь!";
                public static string CooldownText3 = "Подождите еще {0}, прежде чем сделать это!";

                public static string CooldownText4 = "Сейчас вы не можете сделать это, приходите завтра!";
            }

            public static class General
            {
                public static string Kick = "Вы были кикнуты!\nПричина: {0}";
                public static string TeleportBy = "Вы были телепортированы!\nАдминистратор: {0}";

                public static class PayDay
                {
                    public static string Header = "Зарплата";

                    public static string FailTime = "Вы отыграли {0} из {1} мин. на сервере за последний час, поэтому вы не получили зарплату!";
                    public static string Fail = "Сейчас вы не можете получить зарплату!";
                    public static string FailBank = "У вас нет банковского счёта!\nЗарплата не была начислена";
                }

                public static class Report
                {
                    public static string Header = "Администрация";

                    public static string Start = "{0} начал заниматься вашим вопросом!";
                    public static string Close = "{0} закрыл ваш вопрос!\nЕсли остались вы недовольны работой администратора - обратитесь на форум";

                    public static string Reply = "Администратор {0} ответил вам:\n{1}";
                }

                public static class Death
                {
                    public static string Header = "EMS";

                    public static string EMSNotified = "Службы скорой помощи получили ваш вызов!";
                }

                public static string SkillUp = "Навык {0} повышен! [+{1}]\nТекущий уровень: {2}/{3}]";
                public static string SkillDown = "Навык {0} понижен! [-{1}]\nТекущий уровень: {2}/{3}";

                public static string MaxAmountOfBusinesses = "Вы уже владеете максимальным количеством бизнесов!";
                public static string MaxAmountOfHouses = "Вы уже владеете максимальным количеством домов!";
                public static string MaxAmountOfApartments = "Вы уже владеете максимальным количеством квартир!";

                public static string BusinessAlreadyBought = "Этот бизнес уже кем-то приобретен!";

                public static string NoLicenseToBuy = "У вас нет необходимой лицензии для совершения покупки!";

                public static string TuningAlreadyHaveThisColour = "У вас уже установлен этот цвет!";
                public static string TuningAlreadyHaveThisColour2 = "У вас уже установлены эти цвета!";

                public static string TuningNotAllowed = "Этот транспорт нельзя тюнинговать!";

                public static string ShootingRangeHint1 = "Будьте внимательны, вы проиграете, если ваша меткость будет ниже {0}%!";

                public static string AchievementUnlockedText = "Достижение разблокировано!";

                public static string NoMedicalCard = "У вас нет мед. карты!";

                public static string NoOwnedBusiness = "Вы не владеете ни одним бизнесом!";
                public static string NoOwnedEstate = "Вы не владеете ни одним видом недвижимости!";

                public static string ElevatorCurrentFloor = "Вы и так находитесь на этом этаже!";
            }

            public static class House
            {
                public static string Header = "Дом";

                public static string NotInAnyHouse = "Вы не находитесь в доме!";
                public static string NotInAnyHouseOrApartments = "Вы не находитесь в доме/квартире!";

                public static string NotAllowed = "У вас недостаточно прав!";

                public static string NotNearGarage = "Вы не находитесь рядом с гаражом!";

                public static string LightColourChanged = "Цвет света лампы был изменен!";

                public static string IsLocked = "Дверь закрыта владельцем!";
                public static string ContainersLocked = "Контейнеры закрыты владельцем!";

                public static string ExpelledHouse = "{0} выписал вас из своего дома!";
                public static string SettledHouse = "{0} прописал вас в своем доме!";
                public static string ExpelledApartments = "{0} выписал вас из своей квартиры!";
                public static string SettledApartments = "{0} прописал вас в своей квартире!";

                public static string ExpelledHouseSelf = "Вы выписались из этого дома!";
                public static string ExpelledApartmentsSelf = "Вы выписались из этой квартиры!";

                public static string ExpelledHouseAuto = "Вы были выписаны из дома!";
                public static string SettledHouseAuto = "Вы были прописаны в доме!";

                public static string ExpelledApartmentsAuto = "Вы были выписаны из квартиры!";
                public static string SettledApartmentsAuto = "Вы были прописаны в квартире!";

                public static string AlreadySettledHere = "Вы и так здесь прописаны!";
                public static string AlreadySettledOtherHouse = "Вы уже прописаны в другом доме!";
                public static string AlreadySettledOtherApartments = "Вы уже прописаны в другой квартире!";

                public static string OwnsHouseSettle = "У вас уже есть свой дом, вы не можете прописаться!";
                public static string OwnsApartmentsSettle = "У вас уже есть своя квартира, вы не можете прописаться!";

                public static string NoVehiclePlacesInGarage = "В гараже нет свободных мест для транспорта!";
            }

            public static class Commands
            {
                public static string Header = "Команда";

                public static string NotFound = "Такой команды не существует!";
                public static string WrongUsing = "Неверные параметры! Используйте:\n{0}";

                public static string IdNotFound = "Такой ID не существует!";
                public static string TargetNotFound = "Цель не существует!";

                public static string Position = "Позиция: {0}, {1}, {2} | Поворот: {3}";

                public static string Enabled = "{0} - включено!";
                public static string Disabled = "{0} - выключено!";

                public static class Teleport
                {
                    public static string Header = "Телепортация";

                    public static string NoWaypoint = "Вы не поставили метку!";
                }

                public static class Chat
                {
                    public static string Header = "Чат";

                    public static string WrongValue = "Такое значение нельзя использовать!";
                }

                public static class Item
                {
                    public static string Header = "Предмет";

                    public static string Info = "ID: {0}\nНазвание: {1}\nТип: {2}\nПодтип: {3}\nИнтерфейсы: {4}";
                }
            }

            public static class Punishments
            {
                public static class Mute
                {
                    public static string Header = "Мут";
                }

                public static class Ban
                {
                    public static string HeaderCasual = "Блокировка";
                    public static string HeaderHard = "Hard-блокировка";
                }

                public static class Jail
                {
                    public static string Header = "NonRP-тюрьма";
                }

                public static class Warn
                {
                    public static string Header = "Предупреждение";

                    public static string Got = "Администратор: {0}\nПричина: {1}\nУ вас предупреждений: {2}/{3}";
                }

                public static class Kick
                {
                    public static string Header = "Кик";

                    public static string Got = "Администратор: {0}\nПричина: {1}";
                }

                public static string GotTimed = "Администратор: {0}\nВремя: {1} сек.\nПричина: {2}";
                public static string GotDated = "Администратор: {0}\nСрок: {1} мин.\nПричина: {2}";

                public static string TimeLeft = "До снятия: {0} сек.";

                public static string PlayerOnlineInfo = "Игрок в сети!\nCID: #{0} | ID: {1}\nИмя: {2}\n\nВведите команду еще раз, чтобы подтвердить!";
                public static string PlayerOfflineInfo = "Игрок НЕ в сети!\nCID: #{0} | ID: {1}\nИмя: {2}\n\nВведите команду еще раз, чтобы подтвердить!";
            }

            public static class Money
            {
                public static string Header = "Деньги";

                public static class Cash
                {
                    public static string AddHeader = "+${0}";
                    public static string LossHeader = "-${0}$";

                    public static string Balance = "Всего наличных: ${0}";

                    public static string NotEnough = "Недостаточно средств!\nУ вас наличных: ${0}";
                }

                public static class Bank
                {
                    public static string AddHeader = "+${0}";
                    public static string LossHeader = "-${0}";

                    public static string Balance = "Всего на счёте: ${0}";

                    public static string NotEnough = "Недостаточно средств!\nУ вас на счёте: ${0}";
                    public static string NoAccount = "У вас нет банковского счёта!";
                    public static string NoAccountTarget = "У получателя нет банковского счёта!";
                    public static string TargetNotFound = "Неверный идентификатор получателя!";

                    public static string DayLimitExceed = "Превышение максимальной суммы средств для отправки в день по тарифу!";
                    public static string SavingsDepositMaxExceed = "Превышение максимально возможного баланса сберегательного счёта по тарифу!";

                    public static string SendApprove = "Вы собираетесь отправить ${0} {1} {2}.\nНажмите еще раз для подтверждения";
                }

                public static string AdmitToBuy = "Вы уверены? Нажмите еще раз,\nчтобы совершить покупку";

                public static string AdmitToSellGov1 = "Вы уверены? Вы получите {0}\nНажмите еще раз для подтверждения";

                public static string NoMaterialsShop = "В этом магазине недостаточно материалов!";
            }

            public static class Hints
            {
                public static string Header = "Подсказка";

                public static string ClothesShopOrder = "Не вся нижняя одежда сочетается с верхней!\nЕсли хотите носить верхнюю одежду с нижней,\nто лучше сначала выберите верхнюю,\nа потом нижнюю";
                public static string ClothesShopUnderExtraNotNeedTop = "Данная нижняя одежда может изменять состояние только если на вас нет верхней одежды!";
                public static string ClothesShopUnderExtraNeedTop = "Данная нижняя одежда может изменять состояние только если на вас есть верхняя одежда!";

                public static string GasStationColshape = "В этой зоне вы можете заправить свой транспорт";

                public static string AuthCursor = "Не видно курсор? Нажми {0}";
            }

            public static class Container
            {
                public static string Header = "Контейнер";

                public static string Wait = "Подождите, пока кто-то прекратит пользоваться этим";
                public static string ReadOnly = "Вы можете только просматривать данный контейнер!";
            }

            public static class Inventory
            {
                public static string Header = "Инвентарь";

                public static string ActionRestricted = "В данный момент вы не можете делать это!";
                public static string PlaceRestricted = "Вы не можете положить данный предмет в это место!";
                public static string NoSpace = "Нет свободного места!";

                public static string AddedOne = "{0} у вас в инвентаре!";
                public static string Added = "{0} x{1} у вас в инвентаре!";

                public static string TempItem = "Этот предмет является временным!\nВы не можете выполнить данное действие!";
                public static string TempItemDeleted = "Этот предмет был временным и был удалён";

                public static string ArmourBroken = "Ваш бронежилет сломался!";

                public static string Wounded = "Нельзя использовать это сразу после ранения!";

                public static string NoSuchItem = "У вас нет необходимого предмета в инвентаре!";
                public static string NoSuchItemAmount = "У вас нет необходимого кол-ва нужного предмета в инвентаре!";

                public static string InventoryBlocked = "В данный момент вы не можете взаимодействовать с инвентарем!";

                public static string WeaponHasThisComponent = "На это оружие уже установлен этот компонент!";
                public static string WeaponWrongComponent = "На это оружие нельзя установить этот компонент!";

                public static string NoWeaponSkins = "У вас не активна ни одна раскраска на оружие!";
            }

            public static class Gifts
            {
                public static string Header = "Подарки";

                public static string Added = "{0} у вас в подарках!\n\nЗабрать: {1} - Меню - Подарки";

                public static Dictionary<CEF.Menu.GiftSourceTypes, string> SourceNames = new Dictionary<CEF.Menu.GiftSourceTypes, string>()
                {
                    { CEF.Menu.GiftSourceTypes.Server, "Сервер" },
                    { CEF.Menu.GiftSourceTypes.Shop, "Магазин" },
                    { CEF.Menu.GiftSourceTypes.Achievement, "Достижение" },
                };
            }

            public static class Offers
            {
                public static string Header = "Предложение";
                public static string HeaderTrade = "Обмен";

                public static string OfferSettleHouse = "в свой дом";
                public static string OfferSettleApartments = "в свою квартиру";

                public static Dictionary<Sync.Offers.Types, string> Types = new Dictionary<Sync.Offers.Types, string>()
                {
                    { Sync.Offers.Types.Handshake, "{0} предлагает вам поздороваться" },

                    { Sync.Offers.Types.HeadsOrTails, "{0} предлагает вам сыграть в орел и решку" },

                    { Sync.Offers.Types.Exchange, "{0} предлагает вам обменяться" },
                    { Sync.Offers.Types.SellEstate, "{0} предлагает вам продажу недвижимости" },
                    { Sync.Offers.Types.SellVehicle, "{0} предлагает вам продажу транспорта" },
                    { Sync.Offers.Types.SellBusiness, "{0} предлагает вам продажу бизнеса" },

                    { Sync.Offers.Types.Settle, "{0} предлагает вам подселиться {1}" },

                    { Sync.Offers.Types.Carry, "{0} предлагает понести вас" },

                    { Sync.Offers.Types.Cash, "{0} предлагает вам ${1}" },

                    { Sync.Offers.Types.WaypointShare, "{0} предлагает вам свою метку" },

                    { Sync.Offers.Types.ShowPassport, "{0} предлагает вам посмотреть паспорт" },
                    { Sync.Offers.Types.ShowMedicalCard, "{0} предлагает вам посмотреть мед. карту" },
                    { Sync.Offers.Types.ShowVehiclePassport, "{0} предлагает вам посмотреть тех. паспорт" },
                    { Sync.Offers.Types.ShowLicenses, "{0} предлагает вам посмотреть лицензии" },
                    { Sync.Offers.Types.ShowResume, "{0} предлагает вам посмотреть резюме" },

                    { Sync.Offers.Types.InviteFraction, "{0} предлагает вам вступить во фракцию {1}" },
                    { Sync.Offers.Types.InviteOrganisation, "{0} предлагает вам вступить в организацию {1}" },
                };

                public static string Cancel = "Предложение было отменено!";
                public static string CancelBy = "Игрок отменил предложение!";

                public static string Sent = "Предложение успешно отправлено!";

                public static string TargetBusy  = "Данный игрок сейчас занят!";
                public static string TargetHasOffer = "Данному игроку уже что-то предложили!";
                public static string PlayerHasOffer = "У вас уже есть активное предложение!";

                public static string PlayerNeedConfirm = "Другой игрок еще не подтвердил условия обмена. Подождите, пока он сделает это";
                public static string PlayerConfirmed = "Другой игрок подтвердил условия обмена. Чтобы согласиться и совершить обмен - сделайте то же самое";
                public static string PlayerConfirmedCancel = "Другой игрок отменил подтверждение условий обмена!";

                public static string TradeCompleted = "Обмен успешно завершен!";
                public static string TradeError = "Произошла ошибка! Попробуйте еще раз";

                public static string TradeNotEnoughMoney = "У вас недостаточно средств!";
                public static string TradeNotEnoughMoneyOther = "У другого игрока недостаточно средств!";
                public static string TradeNotEnoughSpaceOther = "У другого игрока недостаточно места в инвентаре!";
            }

            public static class CharacterCreation
            {
                public static string CtrlMovePed = "Удерживайте CTRL и двигайте мышкой,\nчтобы повернуть персонажа";
                public static string PressAgainToExit = "Нажмите еще раз,\n чтобы отменить создание персонажа";
                public static string PressAgainToCreate = "Вы уверены? Нажмите еще раз,\n чтобы создать персонажа";

                public static string WrongAge = "Укажите возраст от 18 до 99";
                public static string WrongName = "Имя должно содержать от 2 до X букв";
                public static string WrongSurname = "Фамилия должна содержать от 2 до X букв";
            }

            public static class Bind
            {
                public static string Header = "Привязка клавиш";
                public static string Hint = "Нажмите клавишу, чтобы назначить её на слот\nBackspace - удалить привязку";
                public static string HintCombo = "Нажмите комбинацию клавиш, чтобы назначить их на слот\nBackspace - удалить привязку";
                public static string Binded = "Действие назначено на клавишу {0}";
                public static string BindedCombo = "Действие назначено на комбинацию {0}";
            }

            public static class Interaction
            {
                public static string Header = "Взаимодействие";
                public static string DistanceTooLarge = "Цель слишком далеко!";
                public static string NotFound = "Цель не найдена!";
            }

            public static class Weapon
            {
                public static string Header = "Оружие";

                public static string InVehicleRestricted = "Данное оружие нельзя использовать в транспорте!";
            }

            public static class Auth
            {
                public static string MailNotFree = "Такая почта занята!\nПопробуйте другую";
                public static string LoginNotFree = "Такой логин занят!\nПопробуйте другой";

                public static string WrongPassword = "Неверный пароль!\nОсталось попыток: {0}";
                public static string WrongLogin = "Неверный логин!\nОсталось попыток: {0}";
                public static string WrongToken = "Неверный токен!\nОсталось попыток: {0}";

                public static string LoginTooShort = "Логин слишком короткий!";
                public static string LoginTooLong = "Логин слишком длинный!";
                public static string InvalidLogin = "Логин не соответствует правилу!";

                public static string PasswordTooShort = "Пароль слишком короткий!";
                public static string PasswordTooLong = "Пароль слишком длинный!";
                public static string InvalidPassword = "Пароль не соответствует правилу!";
                public static string PassNotMatch = "Введенные пароли не совпадают!";

                public static string InvalidMail = "Неверная почта!";

                public static string AlreadyOnline = "Этот персонаж уже в сети!";
            }

            public static class Players
            {
                public static class Microphone
                {
                    public static string Reloaded = "Голосовой чат был перезагружен!";
                }

                public static class Administrator
                {
                    public static string FreezedBy = "Вы были заморожены администратором #{0}";
                    public static string UnfreezedBy = "Вы были разморожены администратором #{0}";
                }

                public static class States
                {
                    public static string Header = "Состояние";

                    public static string Wounded = "Вы сильно ранены!\nВы будете терять здоровье, пока вам не окажут помощь";

                    public static string LowMood = "У вас плохое настроение!\nПоднимите его чем-нибудь приятным :)";
                    public static string LowSatiety = "Вы проголодались!\nЕсли вы не поедите в ближайшее время,\nто начнёте терять здоровье";
                }
            }

            public static class Vehicles
            {
                public static string Header = "Транспорт";

                public static class Push
                {
                    public static string EngineOn = "Двигатель не заглушен!";
                }

                public static class Additional
                {
                    public static string HeaderCruise = "Круиз-контроль";
                    public static string HeaderAutoPilot = "Автопилот";

                    public static string On = "Система активирована!";
                    public static string Off = "Система деактивирована!";

                    public static string Reverse = "Недоступно при движении задним ходом!";
                    public static string MinSpeed = "Минимальная скорость - {0} км/ч!";
                    public static string MaxSpeed = "Максимальная скорость - {0} км/ч!";
                    public static string Danger = "Обнаружен опасный манёвр";
                    public static string Collision = "Обнаружено столкновение!";
                    public static string Invtervention = "Обнаружено вмешательство в управление!";

                    public static string Unsupported = "В текущем транспорте нет этой системы!";
                }

                public static class SeatBelt
                {
                    public static string Header = "Ремень безопасности";

                    public static string TakeOffToLeave = "Вы пристегнуты!\nОтстегните ремень, чтобы выйти";
                    public static string TakeOffToSeat = "Вы пристегнуты!\nОтстегните ремень, чтобы пересесть";
                }

                public static class GPS
                {
                    public static string Header = "Навигатор";

                    public static string RouteReady = "Маршрут проложен!";
                    public static string RouteCancel = "Маршрут отменён!";
                }

                public static class Engine
                {
                    public static string On = "Двигатель запущен!";
                    public static string Off = "Двигатель заглушен!";
                    public static string OutOfFuel = "Закончилось топливо!";
                }

                public static class Doors
                {
                    public static string AlreadyLocked = "Двери уже заблокированы!";
                    public static string AlreadyUnlocked = "Двери уже разблокированы!";
                    public static string Locked = "Двери заблокированы!";
                    public static string Unlocked = "Двери разлокированы!";
                }

                public static class Trunk
                {
                    public static string AlreadyLocked = "Багажник уже закрыт!";
                    public static string AlreadyUnlocked = "Багажник уже открыт!";
                    public static string Locked = "Багажник закрыт!";
                    public static string Unlocked = "Багажник открыт!";

                    public static string NoTrunk = "В этом транспорте нет багажника!";
                    public static string NoPhysicalTrunk = "В этом транспорте нет физического багажника!";
                }

                public static class Hood
                {
                    public static string AlreadyLocked = "Капот уже закрыт!";
                    public static string AlreadyUnlocked = "Капот уже открыт!";
                    public static string Locked = "Капот закрыт!";
                    public static string Unlocked = "Капот открыт!";
                }

                public static class Passengers
                {
                    public static string None = "Нет ни одного пассажира!";
                    public static string SomeoneSeating = "Кто-то уже сидит на этом месте!";
                    public static string IsDriver = "Пересаживаться могут только пассажиры!";

                    public static string NotEnterable = "В этот транспорт нельзя садиться!";
                }

                public static class Park
                {
                    public static string Success = "Вы успешно припарковали свой транспорт!";
                    public static string WrongPlace = "Здесь нельзя парковаться!";
                    public static string NotAllowed = "Этот транспорт нельзя парковать!";

                    public static string Warning = "Ваше Т/С не припарковано!\nОно может оказаться на штрафстоянке";
                }

                public static string FullOfGasDef = "Бак вашего Т/С уже полон!";
                public static string FullOfGasElectrical = "Ваше Т/С уже полностью заряжено!";

                public static string NotAtGasStationError = "Вы не на заправке!";
                public static string InVehicleError = "Выйдите из транспорта, чтобы заправить его!";

                public static string NotAllowed = "У вас нет ключей от этого транспорта!";

                public static string NoPlate = "На этом транспорте не установлен номерной знак!";
                public static string PlateExists = "На этом транспорте уже установлен номерной знак!\nДля начала нужно его снять";
                public static string PlateInstalled = "Номерной знак [{0}] установлен!";

                public static string NoOwnedVehicles = "Вы не владеете ни одним транспортом!";

                public static string VehicleOnPound = "Этот транспорт находится на штрафстоянке!";
                public static string VehicleKeyError = "Этот ключ не работает!";

                public static string VehicleIsDeadFixError = "Этот транспорт слишком сильно поврежден, вызовите механика!";
                public static string VehicleIsNotDamagedFixError = "Этот транспорт не поврежден!";
            }
        }
        #endregion
    }
}
