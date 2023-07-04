using System.Collections.Generic;

namespace BCRPServer.Sync
{
    public class Animations
    {
        /// <summary>Типы быстрых анимаций</summary>
        public enum FastTypes
        {
            /// <summary>Никакая анимация не проигрывается</summary>
            None = -1,

            /// <summary>Блокировка транспорта</summary>
            VehLocking = 0,
            /// <summary>Рукопожатие</summary>
            Handshake,
            /// <summary>Подбирание предмета</summary>
            Pickup,
            /// <summary>Выбрасывание предмета</summary>
            Putdown,
            /// <summary>Свист</summary>
            Whistle,

            SmokePuffCig,
            SmokeTransitionCig,

            ItemBurger,
            ItemChips,
            ItemHotdog,
            ItemChocolate,
            ItemPizza,
            ItemCola,
            ItemBeer,
            ItemVodka,
            ItemRum,
            ItemVegSmoothie,
            ItemSmoothie,
            ItemMilkshake,
            ItemMilk,

            ItemBandage,
            ItemMedKit,

            FakeAnim,
        }

        public enum GeneralTypes
        {
            None = -1,

            Knocked = 0,
            PushingVehicle,

            RagdollElectrocute,

            CarryA, CarryB,
            PiggyBackA, PiggyBackB,
            HostageA, HostageB,

            LieInTrunk,

            FishingIdle0, FishingProcess0,

            ShovelProcess0,

            MetalDetectorProcess0,

            CuffedStatic0,

            FarmPlantSmallShovelProcess0,

            BoxCarry0,

            WateringCan0,

            TreeCollect0,

            BucketCarryOneHand0,

            MilkCow0,

            PoliceEscort0,

            BedLie0,

            CasinoSlotMachineIdle0,
            CasinoBlackjackIdle0,
        }

        public enum OtherTypes
        {
            None = -1,

            // Social
            Busted, Busted2,
            Hysterics,
            GetScared,
            MonologueEmotional,
            GiveUp, GiveUp2,
            Joy, Joy2,
            Cheer, Cheer2,
            Attention,
            Respect, Respect2,
            Clap, Clap2, Clap3,
            Salute, Salute2,
            Explain,
            WagFinger,
            Facepalm,
            KeepChest,
            Goat,
            UCrazy,
            AirKiss, AirKiss2, AirKiss3,
            Heartbreak,
            Peace,
            Like,
            Vertigo,
            FlirtOnCar,
            Cry,
            ThreatToKill,
            FingerShot,
            NumberMe,
            WannaFight,
            GuardStop,
            Muscles, Muscles2,
            TakeFlirt,
            CoquettishlyStand,
            CoquettishlyWave,
            KeepCalm,
            CheckMouthOdor,
            ShakeHandsFear,
            LowWave,
            CoverFace,

            // Dialogs
            Argue,
            Nod,
            Agree,
            WhatAPeople,
            Rebellion,
            Listen, Listen1, Listen2,
            Worry,
            MonologueEmotional2,
            Listen3,
            Waiting,
            SpeakAgree,
            ListenBro,
            Listen4,
            Explain2,
            Goodbye,
            Speak,
            GoodbyeBow,

            // Reactions
            Agree2,
            Disagree, Disagree2, Disagree3,
            IDontKnow,
            Heartbreak2,
            Agree3,
            BadSmell,
            IWatchU,
            FuckU,
            ThatsHowUDoThat,
            Enough,
            Sad,
            BrainExplosion,
            Agree4,
            Disagree4,
            Bewilderment,
            Agree5,
            IDontKnow2,
            Surprised, Surprised2,

            // Seat/lie
            Lie, Lie2, Lie3, Lie4, Lie5, Lie6, Lie7, Lie8,
            Seat, Seat2,
            Lie9,
            Seat3, Seat4, Seat5, Seat6, Seat7, Seat8, Seat9, Seat10, Seat11, Seat12, Seat13, Seat14, Seat15, Seat16, Seat17, Seat18, Seat19, Seat20, Seat21, Seat22, Seat23,

            // Sport
            Press,
            PushUps, PushUps2,
            Backflip,
            Fists,
            Yoga, Yoga2, Yoga3, Yoga4, Yoga5, Yoga6,
            Run,
            Pose,
            Swallow,
            Meditation,
            RunFemale, RunMale,
            Karate,
            Box,

            // Indecent
            FuckU2, FuckU3, FuckU4, FuckU5,
            Jerk,
            Chicken,
            Ass,
            FuckFingers,
            PickNose,
            Dumb,
            Tease,
            Dumb2, Dumb3, Dumb4,
            IGotSmthForU,
            SnotShot,
            Scratch, ScratchAss,
            ShakeBoobs,
            KeepCock,
            IndecentJoy, IndecentJoy2,
            SexMale, SexFemale,
            SexMale2, SexFemale2,
            SexMale3,

            // Stand Poses
            GuardStand, GuardStand2,
            Stand, Stand2, Stand3, Stand4, Stand5, Stand6, Stand7, Stand8, Stand9, Stand10, Stand11, Stand12, Stand13, Stand14, Stand15, Stand16, Stand17, Stand18, Stand19, Stand20,
            Stand21, Stand22, Stand23, Stand24, Stand25, Stand26, Stand27, Stand28, Stand29, Stand30, Stand31, Stand32, Stand33, Stand34, Stand35, Stand36, Stand37, Stand38, Stand39, Stand40,
            Stand41, Stand42,

            // Dances
            Dance, Dance2, Dance3, Dance4, Dance5, Dance6, Dance7, Dance8, Dance9, Dance10, Dance11, Dance12, Dance13, Dance14, Dance15, Dance16, Dance17, Dance18, Dance19, Dance20,
            Dance21, Dance22, Dance23, Dance24, Dance25, Dance26, Dance27, Dance28, Dance29, Dance30, Dance31, Dance32, Dance33, Dance34, Dance35, Dance36, Dance37, Dance38, Dance39, Dance40,
            Dance41, Dance42, Dance43, Dance44, Dance45, Dance46, Dance47, Dance48, Dance49, Dance50, Dance51, Dance52, Dance53, Dance54, Dance55, Dance56, Dance57, Dance58, Dance59, Dance60,
            Dance61, Dance62, Dance63, Dance64, Dance65, Dance66, Dance67, Dance68, Dance69, Dance70, Dance71, Dance72, Dance73, Dance74, Dance75, Dance76, Dance77, Dance78, Dance79, Dance80,
            Dance81, Dance82, Dance83, Dance84, Dance85, Dance86, Dance87, Dance88, Dance89, Dance90, Dance91, Dance92, Dance93, Dance94, Dance95, Dance96, Dance97, Dance98, Dance99, Dance100,
            Dance101, Dance102, Dance103, Dance104,

            // Situative
            LookAtSmth,
            KnockDoor,
            CleanTable,
            WashSmth,
            DJ,
            Guitar,
            Drums,
            TossCoin,
            LookAround,
            LookAtSmth2,
            PointAtSmth,
            LookAtSmthGround,
            DropSig,

            // With weapon
            AimLie,
            AimCrouch,
            HurryUp,
            LookAroundWeapon,
            LookFromCoverWeapon,
            CoverWeapon, CoverWeapon2,
        }

        public enum WalkstyleTypes
        {
            None = -1,

            Alien,
            Armored,
            Arrogant,
            Brave,
            Casual, Casual2, Casual3, Casual4, Casual5, Casual6,
            Chichi,
            Confident,
            Cop, Cop2, Cop3,
            DefaultMale, DefaultFemale,
            Drunk, Drunk2, Drunk3, Drunk4,
            Femme,
            Fire, Fire2, Fire3,
            Flee,
            Franklin,
            Gangster, Gangster2, Gangster3, Gangster4, Gangster5,
            Grooving,
            Guard,
            Handcuffs,
            Heels, Heels2,
            Hiking,
            Hipster,
            Hobo,
            Hurry,
            Janitor, Janitor2,
            Jog,
            Lemar,
            Lester, Lester2,
            Maneater,
            Michael,
            Money,
            Muscle,
            Posh, Posh2,
            Quick,
            Runner,
            Sad,
            Sassy, Sassy2,
            Scared,
            Sexy,
            Shady,
            Slow,
            Swagger,
            Tough, Tough2,
            Trash, Trash2,
            Trevor,
            Wide,
        }

        public enum EmotionTypes
        {
            None = -1,

            Happy,
            Joyful,
            Smug,
            Speculative,
            Mouthbreather,
            Sulking,
            Grumpy, Grumpy2, Grumpy3,
            Angry,
            Stressed,
            Shocked, Shocked2,
            NeverBlink,
            OneEye,
            Sleeping, Sleeping2, Sleeping3,
            Weird, Weird2,
            Electrocuted,

            /*            Drunk,
                        Dumb,
                        Injured,*/
        }

        public static Dictionary<FastTypes, int> FastTimeouts = new Dictionary<FastTypes, int>()
        {
            { FastTypes.VehLocking, 1500 },
            { FastTypes.Handshake, 4000 },
            { FastTypes.Pickup, 1500 }, { FastTypes.Putdown, 1500 },
            { FastTypes.Whistle, 2500 },

            { FastTypes.ItemBurger, 6000 },

            { FastTypes.SmokePuffCig, 3000 },
            { FastTypes.SmokeTransitionCig, 1000 },

            { FastTypes.ItemBandage, 4000 },
            { FastTypes.ItemMedKit, 7000 },
        };

        /// <summary>Проиграть быструю анимацию на игроке</summary>
        /// <remarks>Быстрая анимация НЕ проигрывается у игроков, которые вошли в зону стрима данного игрока после того, как она была запущена</remarks>
        /// <param name="pData">Сущность игрока</param>
        /// <param name="type">Тип анимации</param>
        public static void Play(PlayerData pData, FastTypes type, int customTimeout = -1)
        {
            var player = pData.Player;

            if (pData.CrawlOn)
            {
                pData.CrawlOn = false;
            }
            else if (!pData.StopGeneralAnim())
            {
                pData.StopOtherAnim();
            }

            player.TriggerEventToStreamed("Players::PlayFastAnim", player.Handle, (int)type);

            pData.FastAnim = type;

            var timeout = customTimeout < 0 ? FastTimeouts.GetValueOrDefault(type) : customTimeout;

            if (timeout > 0)
            {
                player.TriggerEvent("Players::FAST", timeout);
            }
        }

        /// <summary>Проиграть обычную анимацию на игроке</summary>
        /// <param name="pData">Сущность игрока</param>
        /// <param name="type">Тип анимации</param>
        public static void Play(PlayerData pData, GeneralTypes type)
        {
            var player = pData.Player;

            if (pData.CrawlOn)
            {
                pData.CrawlOn = false;
            }
            else if (!pData.StopFastAnim())
            {
                pData.StopOtherAnim();
            }

            pData.GeneralAnim = type;
        }

        /// <summary>Проиграть другую анимацию на игроке</summary>
        /// <param name="pData">Сущность игрока</param>
        /// <param name="type">Тип анимации</param>
        public static void Play(PlayerData pData, OtherTypes type)
        {
            var player = pData.Player;

            if (pData.CrawlOn)
            {
                pData.CrawlOn = false;
            }
            else if (!pData.StopFastAnim())
            {
                pData.StopGeneralAnim();
            }

            pData.OtherAnim = type;
        }

        public static bool StopFastAnim(PlayerData pData)
        {
            if (pData.FastAnim != FastTypes.None)
            {
                pData.FastAnim = FastTypes.None;

                pData.Player.TriggerEventToStreamed("Players::StopAnim", pData.Player.Id);

                return true;
            }

            return false;
        }

        public static bool StopGeneralAnim(PlayerData pData)
        {
            if (pData.GeneralAnim != GeneralTypes.None)
            {
                pData.GeneralAnim = GeneralTypes.None;

                return true;
            }

            return false;
        }

        public static bool StopOtherAnim(PlayerData pData)
        {
            if (pData.OtherAnim != OtherTypes.None)
            {
                pData.OtherAnim = OtherTypes.None;

                return true;
            }

            return false;
        }

        public static void Set(PlayerData pData, WalkstyleTypes walkstyle)
        {
            var player = pData.Player;

            pData.Walkstyle = walkstyle;
        }

        public static void Set(PlayerData pData, EmotionTypes emotion)
        {
            var player = pData.Player;

            pData.Emotion = emotion;
        }

        public static void StopAll(PlayerData pData)
        {
            if (pData.CrawlOn)
            {
                pData.CrawlOn = false;

                return;
            }

            if (pData.StopGeneralAnim())
                return;

            if (pData.StopFastAnim())
                return;

            if (pData.StopOtherAnim())
                return;
        }
    }
}
