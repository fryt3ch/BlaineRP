﻿using System.Collections.Generic;
using BlaineRP.Client.Animations.Enums;
using BlaineRP.Client.Sync;

namespace BlaineRP.Client
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

                public static string JustStopText = "Нажмите {0}, чтобы остановиться";

                public static string CancelTextInTrunk = "Нажмите {0}, чтобы вылезти из багажника";

                public static string CancelTextPushVehicle = "Нажмите W/A/S/D, чтобы перестать толкать";

                public static string TextDoPuffSmoke = "Нажмите ЛКМ, чтобы сделать затяжку [{0}]";
                public static string TextToMouthSmoke = "Нажмите ALT, чтобы зажать зубами";
                public static string TextToHandSmoke = "Нажмите ALT, чтобы взять в руку";
                public static string CancelTextSmoke = "Нажмите {0}, чтобы перестать курить";

                public static Dictionary<CEF.Animations.AnimSectionTypes, (string SectionName, Dictionary<OtherTypes, string> Names)> Anims = new Dictionary<CEF.Animations.AnimSectionTypes, (string SectionName, Dictionary<OtherTypes, string> Names)>
                {
                    {
                        CEF.Animations.AnimSectionTypes.Social,

                        (
                            "Социальные",

                            new Dictionary<OtherTypes, string>()
                            {
                                { OtherTypes.Busted, "Поднять руки за голову" },
                                { OtherTypes.Busted2, "Руки за голову на коленях" },
                                { OtherTypes.Hysterics, "Испугаться и сидеть в истерике" },
                                { OtherTypes.GetScared, "Сесть испуганно" },
                                { OtherTypes.MonologueEmotional, "Эмоционально говорить что-либо" },
                                { OtherTypes.GiveUp, "Сдаться" },
                                { OtherTypes.GiveUp2, "Сдаться #2" },
                                { OtherTypes.Joy, "Радоваться #1" },
                                { OtherTypes.Joy2, "Радоваться #2" },
                                { OtherTypes.Cheer, "Подбадривать #1" },
                                { OtherTypes.Cheer2, "Подбадривать #2" },
                                { OtherTypes.Attention, "Махать руками, привлекая внимание" },
                                { OtherTypes.Respect, "Респект" },
                                { OtherTypes.Respect2, "Респект #2" },
                                { OtherTypes.Clap, "Хлопать в ладоши" },
                                { OtherTypes.Clap2, "Хлопать в ладоши #2" },
                                { OtherTypes.Clap3, "Аплодировать" },
                                { OtherTypes.Salute, "Отдать честь" },
                                { OtherTypes.Salute2, "Отдать честь #2" },
                                { OtherTypes.Explain, "Объяснять что-то" },
                                { OtherTypes.WagFinger, "Грозить пальцем" },
                                { OtherTypes.Facepalm, "Facepalm [рукалицо]" },
                                { OtherTypes.KeepChest, "Держаться за грудь" },
                                { OtherTypes.Goat, "Рок-н-ролл" },
                                { OtherTypes.UCrazy, "Крутить пальцем у виска" },
                                { OtherTypes.AirKiss, "Воздушный поцелуй" },
                                { OtherTypes.AirKiss2, "Воздушный поцелуй #2" },
                                { OtherTypes.AirKiss3, "Воздушный поцелуй #3" },
                                { OtherTypes.Heartbreak, "Держаться за сердце" },
                                { OtherTypes.Peace, "Peace [пис/мир]" },
                                { OtherTypes.Like, "Лайк [большой палец вверх]" },
                                { OtherTypes.Vertigo, "Головокружение" },
                                { OtherTypes.FlirtOnCar, "Флиртовать, облокотившись о что-либо" },
                                { OtherTypes.Cry, "Плакать" },
                                { OtherTypes.ThreatToKill, "Угрожать убить" },
                                { OtherTypes.FingerShot, "Выстрел из пальца" },
                                { OtherTypes.NumberMe, "На созвоне" },
                                { OtherTypes.WannaFight, "Вызывать на бой" },
                                { OtherTypes.GuardStop, "Остановить человека [как охранник]" },
                                { OtherTypes.Muscles, "Хвастаться мускулами" },
                                { OtherTypes.Muscles2, "Хвастаться мускулами #2" },
                                { OtherTypes.TakeFlirt, "Заигрывающе поднять что-то" },
                                { OtherTypes.CoquettishlyStand, "Стоять кокетливо" },
                                { OtherTypes.CoquettishlyWave, "Махать кокетливо" },
                                { OtherTypes.KeepCalm, "Сохраняйте спокойствие" },
                                { OtherTypes.CheckMouthOdor, "Проверять запах изо рта" },
                                { OtherTypes.ShakeHandsFear, "Испуганно трясти руками" },
                                { OtherTypes.LowWave, "Слабо махать рукой" },
                                { OtherTypes.CoverFace, "Прикрывать лицо рукой" },
                            }
                        )
                    },

                    {
                        CEF.Animations.AnimSectionTypes.Dialogs,

                        (
                            "Для диалогов",

                            new Dictionary<OtherTypes, string>()
                            {
                                { OtherTypes.Argue, "Спорить, скрестив руки" },
                                { OtherTypes.Nod, "Кивать, скрестив руки" },
                                { OtherTypes.Agree, "Соглашаться после раздумий" },
                                { OtherTypes.WhatAPeople, "Радостное приветствие" },
                                { OtherTypes.Rebellion, "Возмущенно раскидывать руками" },
                                { OtherTypes.Listen, "Слушать" },
                                { OtherTypes.Listen1, "Слушать, одобрительно кивая" },
                                { OtherTypes.Listen2, "Слушать, расставив ноги" },
                                { OtherTypes.Worry, "Смотреть, беспокоясь" },
                                { OtherTypes.MonologueEmotional2, "Рассказывать что-то, жестикулируя" },
                                { OtherTypes.Listen3, "Слушать, скрестив руки" },
                                { OtherTypes.Waiting, "Говорить, ожидая кого-либо" },
                                { OtherTypes.SpeakAgree, "Говорить одобрительно" },
                                { OtherTypes.ListenBro, "Слушать #2" },
                                { OtherTypes.Listen4, "Слушать скромно" },
                                { OtherTypes.Explain2, "Объяснять доходчиво" },
                                { OtherTypes.Goodbye, "Прощаться" },
                                { OtherTypes.Speak, "Стоять и говорить" },
                                { OtherTypes.GoodbyeBow, "Откланяться на прощание" },
                            }
                        )
                    },

                    {
                        CEF.Animations.AnimSectionTypes.Reactions,

                        (
                            "Реакции",

                            new Dictionary<OtherTypes, string>()
                            {
                                { OtherTypes.Agree2, "Соглашаться" },
                                { OtherTypes.Disagree, "Отказываться" },
                                { OtherTypes.Disagree2, "Отказываться, жестикуляруя" },
                                { OtherTypes.Disagree3, "Отрицательно махать головой" },
                                { OtherTypes.IDontKnow, "Пожимать плечами" },
                                { OtherTypes.Heartbreak2, "Схватиться за сердце" },
                                { OtherTypes.Agree3, "Одобрение" },
                                { OtherTypes.BadSmell, "Почувствовать плохой запах" },
                                { OtherTypes.IWatchU, "Я слежу за вами" },
                                { OtherTypes.FuckU, "Выкуси" },
                                { OtherTypes.ThatsHowUDoThat, "Вот как это делается" },
                                { OtherTypes.Enough, "Хватит" },
                                { OtherTypes.Sad, "Расстроиться" },
                                { OtherTypes.BrainExplosion, "Взрыв мозга" },
                                { OtherTypes.Agree4, "Одобрительно покивать" },
                                { OtherTypes.Disagree4, "Неодобрительно покивать" },
                                { OtherTypes.Bewilderment, "Недоумение" },
                                { OtherTypes.Agree5, "Недовольно согласиться" },
                                { OtherTypes.IDontKnow2, "Разводить руками" },
                                { OtherTypes.Surprised, "Удивленно посмотреть вниз" },
                                { OtherTypes.Surprised2, "Удивленно трясти руками" },
                            }
                        )
                    },

                    {
                        CEF.Animations.AnimSectionTypes.SeatLie,

                        (
                            "Сидеть/лежать",

                            new Dictionary<OtherTypes, string>()
                            {
                                { OtherTypes.Lie, "Лежать" },
                                { OtherTypes.Lie2, "Лежать #2" },
                                { OtherTypes.Lie3, "Лежать, держась за голову" },
                                { OtherTypes.Lie4, "Лежать на животе" },
                                { OtherTypes.Lie5, "Лежать на животе #2" },
                                { OtherTypes.Lie6, "Лежать [как раненый]" },
                                { OtherTypes.Lie7, "Лежать на спине" },
                                { OtherTypes.Lie8, "Лежать, закрыв лицо" },
                                { OtherTypes.Seat, "Сидеть у ног" },
                                { OtherTypes.Seat2, "Сидеть, сложа руки" },
                                { OtherTypes.Lie9, "Лежать в конвульсиях" },
                                { OtherTypes.Seat3, "Присесть на колено" },
                                { OtherTypes.Seat4, "Сидеть на чем-либо" },
                                { OtherTypes.Seat5, "Сидеть на чем-либо #2" },
                                { OtherTypes.Seat6, "Сидеть на чем-либо #3" },
                                { OtherTypes.Seat7, "Сидеть на чем-либо #4" },
                                { OtherTypes.Seat8, "Сидеть на капоте" },
                                { OtherTypes.Seat9, "Сидеть полулежа" },
                                { OtherTypes.Seat10, "Сидеть как йог" },
                                { OtherTypes.Seat11, "Сидеть, облакотившись" },
                                { OtherTypes.Seat12, "Сидеть и грустить" },
                                { OtherTypes.Seat13, "Сидеть и держаться за голову " },
                                { OtherTypes.Seat14, "Сидеть на корточках" },
                                { OtherTypes.Seat15, "Сидеть, оперевшись" },
                                { OtherTypes.Seat16, "Сидеть, оперевшись #2" },
                                { OtherTypes.Seat17, "Оглядываться, сидя на корточках" },
                                { OtherTypes.Seat18, "Сидеть на земле меланхолично" },
                                { OtherTypes.Seat19, "Сидеть за барной стойкой" },
                                { OtherTypes.Seat20, "Сидеть за барной стойкой #2" },
                                { OtherTypes.Seat21, "Сидеть, скучая" },
                                { OtherTypes.Seat22, "Сидеть на ступенях" },
                                { OtherTypes.Seat23, "Сидеть в испуге" },
                            }
                        )
                    },

                    {
                        CEF.Animations.AnimSectionTypes.Sport,

                        (
                            "Спортивные",

                            new Dictionary<OtherTypes, string>()
                            {
                                { OtherTypes.Press, "Пресс" },
                                { OtherTypes.PushUps, "Отжимания" },
                                { OtherTypes.PushUps2, "Глубокие отжимания" },
                                { OtherTypes.Backflip, "Сальто назад" },
                                { OtherTypes.Fists, "Разминать кулаки" },
                                { OtherTypes.Yoga, "Заниматься йогой" },
                                { OtherTypes.Yoga2, "Заниматься йогой #2" },
                                { OtherTypes.Yoga3, "Заниматься йогой #3" },
                                { OtherTypes.Yoga4, "Элемент йоги" },
                                { OtherTypes.Yoga5, "Элемент йоги #2" },
                                { OtherTypes.Yoga6, "Элемент йоги #3" },
                                { OtherTypes.Run, "Бег на месте" },
                                { OtherTypes.Pose, "Позировать" },
                                { OtherTypes.Swallow, "Сделать ласточку" },
                                { OtherTypes.Meditation, "Медитация" },
                                { OtherTypes.RunFemale, "Пробежка на месте [Ж]" },
                                { OtherTypes.RunMale, "Пробежка на месте [М]" },
                                { OtherTypes.Karate, "Карате" },
                                { OtherTypes.Box, "Бокс с тенью" },
                            }
                        )
                    },

                    {
                        CEF.Animations.AnimSectionTypes.Indecent,

                        (
                            "Непристойные",

                            new Dictionary<OtherTypes, string>()
                            {
                                { OtherTypes.FuckU2, "Показывать фак [средний палец]" },
                                { OtherTypes.FuckU3, "Показывать фак #2" },
                                { OtherTypes.FuckU4, "Показывать фак всем вокруг" },
                                { OtherTypes.FuckU5, "Показывать фак эмоционально" },
                                { OtherTypes.Jerk, "Дергать рукой" },
                                { OtherTypes.Chicken, "Изображать курицу" },
                                { OtherTypes.Ass, "Повернуться задницей" },
                                { OtherTypes.FuckFingers, "Засовывать пальцы в кулак" },
                                { OtherTypes.PickNose, "Ковыряться в носу" },
                                { OtherTypes.Dumb, "Кривляться" },
                                { OtherTypes.Tease, "Дразнить" },
                                { OtherTypes.Dumb2, "Дурачиться" },
                                { OtherTypes.Dumb3, "Дурачиться #2" },
                                { OtherTypes.Dumb4, "Дурачиться #3" },
                                { OtherTypes.IGotSmthForU, "У меня для тебя кое-что есть" },
                                { OtherTypes.SnotShot, "Стрелять козявкой" },
                                { OtherTypes.Scratch, "Чесаться" },
                                { OtherTypes.ScratchAss, "Почесать попу" },
                                { OtherTypes.ShakeBoobs, "Трясти грудью" },
                                { OtherTypes.KeepCock, "Держаться за пах" },
                                { OtherTypes.IndecentJoy, "Неприлично радоваться" },
                                { OtherTypes.IndecentJoy2, "Неприлично радоваться #2" },
                                { OtherTypes.SexMale, "Секс [активная роль]" },
                                { OtherTypes.SexFemale, "Секс [пассивная роль]" },
                                { OtherTypes.SexMale2, "Секс [активная роль] #2" },
                                { OtherTypes.SexFemale2, "Секс [пассивная роль] #2" },
                                { OtherTypes.SexMale3, "Секс [активная роль] #3" },
                            }
                        )
                    },

                    {
                        CEF.Animations.AnimSectionTypes.StandPoses,

                        (
                            "Стойки",

                            new Dictionary<OtherTypes, string>()
                            {
                                { OtherTypes.GuardStand, "Стойка охранника" },
                                { OtherTypes.GuardStand2, "Стойка охранника #2" },
                                { OtherTypes.Stand, "Стоять задумчиво" },
                                { OtherTypes.Stand2, "Стоять с рукой на поясе" },
                                { OtherTypes.Stand3, "Стоять, сложа руки" },
                                { OtherTypes.Stand4, "Стоять, сложа руки #2" },
                                { OtherTypes.Stand5, "Стоять, сложа руки #3" },
                                { OtherTypes.Stand6, "Стоять, сложа руки #4" },
                                { OtherTypes.Stand7, "Стоять, облоктившись" },
                                { OtherTypes.Stand8, "Стоять, облокотившись #2" },
                                { OtherTypes.Stand9, "Стоять, облокотившись #3" },
                                { OtherTypes.Stand10, "Стоять, облокотившись #4" },
                                { OtherTypes.Stand11, "Стоять с руками за спиной" },
                                { OtherTypes.Stand12, "Стоять как супергерой" },
                                { OtherTypes.Stand13, "Стоять как супергерой #2" },
                                { OtherTypes.Stand14, "Стоять с руками на поясе" },
                                { OtherTypes.Stand15, "Стоять застенчиво" },
                                { OtherTypes.Stand16, "Стоять застенчиво #2" },
                                { OtherTypes.Stand17, "Стоять пьяным" },
                                { OtherTypes.Stand18, "Стоять, осматриваясь по сторонам" },
                                { OtherTypes.Stand19, "Стоять с ногой на возвышении" },
                                { OtherTypes.Stand20, "Стоять устало" },
                                { OtherTypes.Stand21, "Стоять неуверенно" },
                                { OtherTypes.Stand22, "Стоять, скрестив руки" },
                                { OtherTypes.Stand23, "Стоять, неодобрительно оглядываясь" },
                                { OtherTypes.Stand24, "Стоять, недоуменно оглядываясь" },
                                { OtherTypes.Stand25, "Стоять, переминаясь" },
                                { OtherTypes.Stand26, "Стоять, переминаясь #2" },
                                { OtherTypes.Stand27, "Стоять, держа себя за руку" },
                                { OtherTypes.Stand28, "Стоять, облокотившись к стене" },
                                { OtherTypes.Stand29, "Стоять, перебирая руками" },
                                { OtherTypes.Stand30, "Стоять задумчиво, оперевшись на что-либо" },
                                { OtherTypes.Stand31, "Стоять, скучая облокотившись" },
                                { OtherTypes.Stand32, "Стоять, облокотившись и скрестив руки" },
                                { OtherTypes.Stand33, "Стоять, смотря по сторонам" },
                                { OtherTypes.Stand34, "Стоять, облокотившись и скрестив ноги" },
                                { OtherTypes.Stand35, "Стоять, оглядываясь и переминаясь" },
                                { OtherTypes.Stand36, "Стоять, ожидая и почесываясь" },
                                { OtherTypes.Stand37, "Слушать, переминаясь" },
                                { OtherTypes.Stand38, "Стоять, облокотившись вбок" },
                                { OtherTypes.Stand39, "Стоять печально" },
                                { OtherTypes.Stand40, "Качаться пьяным" },
                                { OtherTypes.Stand41, "Стоять, засыпая" },
                                { OtherTypes.Stand42, "Стоять надменно" },
                            }
                        )
                    },

                    {
                        CEF.Animations.AnimSectionTypes.Dances,

                        (
                            "Танцы",

                            new Dictionary<OtherTypes, string>()
                            {
                                { OtherTypes.Dance, "Лезгинка" },
                                { OtherTypes.Dance2, "Танец руками" },
                                { OtherTypes.Dance3, "Флексить" },
                                { OtherTypes.Dance4, "Стриптиз" },
                                { OtherTypes.Dance5, "Стриптиз #2" },
                                { OtherTypes.Dance6, "Cтриптиз #3" },
                                { OtherTypes.Dance7, "Cтриптиз #4" },
                                { OtherTypes.Dance8, "Стриптиз #5" },
                                { OtherTypes.Dance9, "Лэп-дэнс" },
                                { OtherTypes.Dance10, "Пьяный стриптиз" },
                                { OtherTypes.Dance11, "Эротический танец" },
                                { OtherTypes.Dance12, "Сексуально завлекать" },
                                { OtherTypes.Dance13, "Сексуально пританцовывать" },
                                { OtherTypes.Dance14, "Завлекающий танец" },
                                { OtherTypes.Dance15, "Двигать тазом" },
                                { OtherTypes.Dance16, "Слушать музыку" },
                                { OtherTypes.Dance17, "Скромный танец" },
                                { OtherTypes.Dance18, "Кивать под музыку" },
                                { OtherTypes.Dance19, "Пританцовывать под музыку" },
                                { OtherTypes.Dance20, "Танец с похлопованием попы" },
                                { OtherTypes.Dance21, "Танец с оборотами" },
                                { OtherTypes.Dance22, "Танец робота" },
                                { OtherTypes.Dance23, "Танец робота #2" },
                                { OtherTypes.Dance24, "Танцевать как паучок" },
                                { OtherTypes.Dance25, "Произвольный танец" },
                                { OtherTypes.Dance26, "Легкий танец" },
                                { OtherTypes.Dance27, "Танец мачо" },
                                { OtherTypes.Dance28, "Танец мачо #2" },
                                { OtherTypes.Dance29, "Танец пингвина" },
                                { OtherTypes.Dance30, "Танец диджея" },
                                { OtherTypes.Dance31, "Танец диджея #2" },
                                { OtherTypes.Dance32, "Танцевать как курочка" },
                                { OtherTypes.Dance33, "Флексить как рэпер" },
                                { OtherTypes.Dance34, "Аккуратный танец" },
                                { OtherTypes.Dance35, "Современный танец" },
                                { OtherTypes.Dance36, "Танец забвения" },
                                { OtherTypes.Dance37, "Танец на месте" },
                                { OtherTypes.Dance38, "Танец на месте #2" },
                                { OtherTypes.Dance39, "Танец на месте #3" },
                                { OtherTypes.Dance40, "Танец зумбы" },
                                { OtherTypes.Dance41, "Танец зумбы #2" },
                                { OtherTypes.Dance42, "Танец зумбы #3" },
                                { OtherTypes.Dance43, "Клубный танец" },
                                { OtherTypes.Dance44, "Клубный танец #2" },
                                { OtherTypes.Dance45, "Клубный танец #3" },
                                { OtherTypes.Dance46, "Клубный танец #4" },
                                { OtherTypes.Dance47, "Клубный танец #5" },
                                { OtherTypes.Dance48, "Клубный танец #6" },
                                { OtherTypes.Dance49, "Клубный танец #7" },
                                { OtherTypes.Dance50, "Клубный танец #8" },
                                { OtherTypes.Dance51, "Танец лепестка" },
                                { OtherTypes.Dance52, "Танец пожилого человека" },
                                { OtherTypes.Dance53, "Танец заводной" },
                                { OtherTypes.Dance54, "Танец диско" },
                                { OtherTypes.Dance55, "Танец бедрами" },
                                { OtherTypes.Dance56, "Танец индийский" },
                                { OtherTypes.Dance57, "Танец счастливый" },
                                { OtherTypes.Dance58, "Танец шафл руками" },
                                { OtherTypes.Dance59, "Танец Skibidi" },
                                { OtherTypes.Dance60, "Танец c хлопаками" },
                                { OtherTypes.Dance61, "Танец улетный" },
                                { OtherTypes.Dance62, "Танец расслабленный" },
                                { OtherTypes.Dance63, "Танец лейла" },
                                { OtherTypes.Dance64, "Танец диджея #3" },
                                { OtherTypes.Dance65, "Танец электро" },
                                { OtherTypes.Dance66, "Танец загадочный" },
                                { OtherTypes.Dance67, "Танец игривый" },
                                { OtherTypes.Dance68, "Танец игривый #2" },
                                { OtherTypes.Dance69, "Танец Руки Вверх" },
                                { OtherTypes.Dance70, "Танец Руки Вверх #2" },
                                { OtherTypes.Dance71, "Танец Лапули" },
                                { OtherTypes.Dance72, "Танец Зазывающий" },
                                { OtherTypes.Dance73, "Танец манящий" },
                                { OtherTypes.Dance74, "Танец раскрепощенный" },
                                { OtherTypes.Dance75, "Танец Зайки" },
                                { OtherTypes.Dance76, "Танец вальяжный " },
                                { OtherTypes.Dance77, "Танец игривый" },
                                { OtherTypes.Dance78, "Танец с наклоном" },
                                { OtherTypes.Dance79, "Танец кокетки" },
                                { OtherTypes.Dance80, "Танец динамичный" },
                                { OtherTypes.Dance81, "Танец лапули #2" },
                                { OtherTypes.Dance82, "Танец Цыганочка" },
                                { OtherTypes.Dance83, "Танец Шейк" },
                                { OtherTypes.Dance84, "Танец мачо #3" },
                                { OtherTypes.Dance85, "Развязный танец" },
                                { OtherTypes.Dance86, "Танец извивающийся" },
                                { OtherTypes.Dance87, "Милый танец" },
                                { OtherTypes.Dance88, "Уличный танец" },
                                { OtherTypes.Dance89, "Танец кокетки #2" },
                                { OtherTypes.Dance90, "Танец заигрывающий" },
                                { OtherTypes.Dance91, "Танец с оборотами #2" },
                                { OtherTypes.Dance92, "Танец удачи" },
                                { OtherTypes.Dance93, "Бодрый танец" },
                                { OtherTypes.Dance94, "Танец с вилянием бедер" },
                                { OtherTypes.Dance95, "Танец кулачками легкий" },
                                { OtherTypes.Dance96, "Танцевать локтями" },
                                { OtherTypes.Dance97, "Расслабленный танец" },
                                { OtherTypes.Dance98, "Танец качающий" },
                                { OtherTypes.Dance99, "Стучать пальцами о пальцы" },
                                { OtherTypes.Dance100, "Танец сумасшедшего" },
                                { OtherTypes.Dance101, "Танец жизнерадостный" },
                                { OtherTypes.Dance102, "Танец активный" },
                                { OtherTypes.Dance103, "Победный танец" },
                                { OtherTypes.Dance104, "Танец волна" },
                            }
                        )
                    },

                    {
                        CEF.Animations.AnimSectionTypes.Situative,

                        (
                            "Ситуативные",

                            new Dictionary<OtherTypes, string>()
                            {
                                { OtherTypes.LookAtSmth, "Разглядывать что-то в руках" },
                                { OtherTypes.KnockDoor, "Стучать в дверь" },
                                { OtherTypes.CleanTable, "Протирать стол" },
                                { OtherTypes.WashSmth, "Мыть что-то" },
                                { OtherTypes.DJ, "Диджей" },
                                { OtherTypes.Guitar, "Воображаемая гитара" },
                                { OtherTypes.Drums, "Воображаемые барабаны" },
                                { OtherTypes.TossCoin, "Подбрасывать монетку" },
                                { OtherTypes.LookAround, "Осматриваться, сидя на колене" },
                                { OtherTypes.LookAtSmth2, "Разглядывать что-либо, приседая" },
                                { OtherTypes.PointAtSmth, "Указывать на что-либо" },
                                { OtherTypes.LookAtSmthGround, "Осматривать что-либо на земле" },
                                { OtherTypes.DropSig, "Эмоционально выкинуть сигарету" },
                            }
                        )
                    },

                    {
                        CEF.Animations.AnimSectionTypes.WithWeapon,

                        (
                            "Для оружия в руках",

                            new Dictionary<OtherTypes, string>()
                            {
                                { OtherTypes.AimLie, "Целиться лежа с оружием" },
                                { OtherTypes.AimCrouch, "Целиться на корточках с оружием" },
                                { OtherTypes.HurryUp, "Подгонять с оружием в руках" },
                                { OtherTypes.LookAroundWeapon, "Осматриваться с оружием" },
                                { OtherTypes.LookFromCoverWeapon, "Выглядывать из-за стены с оружием" },
                                { OtherTypes.CoverWeapon, "Стоять за стеной с оружием" },
                                { OtherTypes.CoverWeapon2, "Стоять за укрытием" },
                            }
                        )
                    },
                };

                public static Dictionary<EmotionTypes, string> Emotions = new Dictionary<EmotionTypes, string>()
                {
                    { EmotionTypes.None, "Обычный" },

                    { EmotionTypes.Happy, "Счастливый" },
                    { EmotionTypes.Joyful, "Радостный" },
                    { EmotionTypes.Smug, "Самодовольный" },

                    { EmotionTypes.Speculative, "Настороженный" },
                    { EmotionTypes.Mouthbreather, "Удивленный" },

                    { EmotionTypes.Sulking, "Грустный" },

                    { EmotionTypes.Grumpy, "Сердитый" },
                    { EmotionTypes.Grumpy2, "Сердитый #2" },
                    { EmotionTypes.Grumpy3, "Сердитый #3" },

                    { EmotionTypes.Angry, "Злой" },
                    { EmotionTypes.Stressed, "Яростный" },

                    { EmotionTypes.Shocked, "Шокированный" },
                    { EmotionTypes.Shocked2, "Шокированный #2" },

                    { EmotionTypes.NeverBlink, "Не моргающий" },
                    { EmotionTypes.OneEye, "Прицеливающийся" },

                    { EmotionTypes.Sleeping, "Спящий" },
                    { EmotionTypes.Sleeping2, "Спящий #2" },
                    { EmotionTypes.Sleeping3, "Спящий #3" },

                    { EmotionTypes.Weird, "Странный" },
                    { EmotionTypes.Weird2, "Странный #2" },
                    { EmotionTypes.Electrocuted, "Эпилепсик" },
                };

                public static Dictionary<WalkstyleTypes, string> Walkstyles = new Dictionary<WalkstyleTypes, string>()
                {
                    { WalkstyleTypes.None, "Обычный" },

                    { WalkstyleTypes.Wide, "Широкий" },
                    { WalkstyleTypes.Confident, "Уверенный" },
                    { WalkstyleTypes.Brave, "Храбрый" },
                    { WalkstyleTypes.Arrogant, "Высокомерный" },
                    { WalkstyleTypes.Posh, "Пафосный" },
                    { WalkstyleTypes.Posh2, "Пафосный #2" },

                    { WalkstyleTypes.Sassy, "Дерзкий" },
                    { WalkstyleTypes.Sassy2, "Дерзкий #2" },

                    { WalkstyleTypes.Swagger, "Развязный" },
                    { WalkstyleTypes.Shady, "Развязный #2" },
                    { WalkstyleTypes.Tough, "Крутой" },
                    { WalkstyleTypes.Tough2, "Крутой #2" },

                    { WalkstyleTypes.Sad, "Грустный" },
                    { WalkstyleTypes.Scared, "Испуганный" },
                    { WalkstyleTypes.Slow, "Медленный" },

                    { WalkstyleTypes.Hurry, "Спешащий" },
                    { WalkstyleTypes.Quick, "Спешащий #2" },

                    { WalkstyleTypes.Flee, "Парящий" },

                    { WalkstyleTypes.Hipster, "Хипстер" },
                    { WalkstyleTypes.Hobo, "Бродяга" },
                    { WalkstyleTypes.Muscle, "Силач" },

                    { WalkstyleTypes.Lester, "Храмой" },
                    { WalkstyleTypes.Lester2, "Храмой #2" },

                    { WalkstyleTypes.Sexy, "Сексуальный" },
                    { WalkstyleTypes.Chichi, "Манерный" },
                    { WalkstyleTypes.Femme, "Феминный" },
                    { WalkstyleTypes.Heels, "На каблуках" },
                    { WalkstyleTypes.Heels2, "На каблуках #2" },

                    { WalkstyleTypes.Cop, "Коп" },
                    { WalkstyleTypes.Cop2, "Коп #2" },
                    { WalkstyleTypes.Cop3, "Коп #3" },

                    { WalkstyleTypes.Drunk, "Пьяный" },
                    { WalkstyleTypes.Drunk2, "Пьяный #2" },
                    { WalkstyleTypes.Drunk3, "Пьяный #3" },
                    { WalkstyleTypes.Drunk4, "Пьяный #4" },

                    { WalkstyleTypes.Gangster, "Гангстер" },
                    { WalkstyleTypes.Gangster2, "Гангстер #2" },
                    { WalkstyleTypes.Gangster3, "Гангстер #3" },
                    { WalkstyleTypes.Gangster4, "Гангстер #4" },
                    { WalkstyleTypes.Gangster5, "Гангстер #5" },

                    { WalkstyleTypes.Guard, "Охранник" },
                };
            }
        }
    }
}
