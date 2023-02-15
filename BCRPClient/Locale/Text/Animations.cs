using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient
{
    public static partial class Locale
    {
        public static partial class General
        {
            public static class Animations
            {
                public static string CancelText = "Нажмите {0}, чтобы отменить текущую анимацию";

                public static string CancelTextCarryA = "Нажмите {0}, чтобы перестать нести человека";
                public static string CancelTextCarryB = "Нажмите {0}, чтобы слезть с человека";

                public static string CancelTextInTrunk = "Нажмите {0}, чтобы вылезти из багажника";

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

                    { Sync.Animations.EmotionTypes.Grumpy, "Сердитый" },
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
        }
    }
}
