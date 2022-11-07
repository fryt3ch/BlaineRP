using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;

namespace BCRPClient.Sync
{
    public class Animations : Events.Script
    {
        private static DateTime LastSent;

        #region Supported Anims

        /// <summary>Класс анимации</summary>
        public class Animation
        {
            /// <summary>Словарь</summary>
            public string Dict;
            /// <summary>Название</summary>
            public string Name;
            /// <summary>Скорость входа в анимацию</summary>
            public float BlendInSpeed;
            /// <summary>Скорость выхода из анимации</summary>
            public float BlendOutSpeed;
            /// <summary>Продолжительность</summary>
            public int Duration;
            /// <summary>Флаг</summary>
            public int Flag;
            /// <summary>Смещение начала</summary>
            public float StartOffset;
            public bool BlockX, BlockY, BlockZ;

            /// <summary>Название для первого лица</summary>
            public string NameFP;

            public Animation(string Dict, string Name, float BlendInSpeed = 8f, float BlendOutSpeed = 1f, int Duration = -1, int Flag = 0, float StartOffset = 0f, bool BlockX = false, bool BlockY = false, bool BlockZ = false)
            {
                this.Dict = Dict;
                this.Name = Name;
                this.BlendInSpeed = BlendInSpeed;
                this.BlendOutSpeed = BlendOutSpeed;
                this.Duration = Duration;
                this.Flag = Flag;
                this.StartOffset = StartOffset;
                this.BlockX = BlockX;
                this.BlockY = BlockY;
                this.BlockZ = BlockZ;

                this.NameFP = null;
            }
        }

        public enum FastTypes
        {
            None = -1,

            VehLocking = 0,
            Handshake,
            Pickup, Putdown,
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

            Angry,
            Drunk,
            Dumb,
            Electrocuted,
            Grumpy, Grumpy2, Grumpy3,
            Happy,
            Injured,
            Joyful,
            Mouthbreather,
            NeverBlink,
            OneEye,
            Shocked, Shocked2,
            Sleeping, Sleeping2, Sleeping3,
            Smug,
            Speculative,
            Stressed,
            Sulking,
            Weird, Weird2,
        }

        public enum ScenarioTypes
        {

        }

        private static Dictionary<FastTypes, Animation> FastAnimsList = new Dictionary<FastTypes, Animation>()
        {
            { FastTypes.VehLocking, new Animation("anim@mp_player_intmenu@key_fob@", "fob_click", 8f, 1f, 1500, 50, 0f, false, false, false) { NameFP = "fob_click_fp" } },
            { FastTypes.Pickup, new Animation("pickup_object", "pickup_low", 8f, 1f, 750, 48, 0f, false, false, false) },
            { FastTypes.Putdown, new Animation("pickup_object", "putdown_low", 8f, 1f, 750, 48, 0f, false, false, false) },
            { FastTypes.Handshake, new Animation("mp_ped_interaction", "handshake_guy_a", 8f, 1f, 4000, 16, 0f, false, false, false) },
            { FastTypes.Whistle, new Animation("rcmnigel1c", "hailing_whistle_waive_a", 2.7f, 2.7f, 2000, 49, 0f, false, false, false) },

            { FastTypes.SmokePuffCig, new Animation("amb@world_human_aa_smoke@male@idle_a", "idle_a", 8f, 1f, 2800, 49, 0f, false, false, false) },
            { FastTypes.SmokeTransitionCig, new Animation("move_p_m_two_idles@generic", "fidget_sniff_fingers", 8f, 1f, 1000, 49, 0f, false, false, false) },

            { FastTypes.ItemChips, new Animation("amb@code_human_wander_eating_donut@female@idle_a", "idle_c", 8f, 1f, 6000, 49, 0f, false, false, false) },
            { FastTypes.ItemBurger, new Animation("amb@code_human_wander_eating_donut@female@idle_a", "idle_b", 8f, 1f, 6000, 49, 0f, false, false, false) },
            { FastTypes.ItemHotdog, new Animation("mp_player_inteat@burger", "mp_player_int_eat_burger", 8f, 1f, 10000, 49, 0f, false, false, false) },
            { FastTypes.ItemChocolate, new Animation("amb@world_human_seat_wall_eating@male@both_hands@base", "idle_b", 8f, 1f, 6000, 49, 0f, false, false, false) },
            { FastTypes.ItemPizza, new Animation("amb@code_human_wander_eating_donut_fat@male@idle_a", "idle_a", 8f, 1f, 6000, 49, 0f, false, false, false) },
            { FastTypes.ItemCola, new Animation("amb@world_human_drinking@coffee@female@idle_a", "idle_a", 8f, 1f, 6000, 49, 0f, false, false, false) },
            { FastTypes.ItemBeer, new Animation("amb@world_human_drinking@beer@male@idle_a", "idle_b", 8f, 1f, 6000, 49, 0f, false, false, false) },

            { FastTypes.ItemBandage, new Animation("oddjobs@bailbond_hobotwitchy", "base", 8f, 1f, 4000, 49, 0f, false, false, false) },
            { FastTypes.ItemMedKit, new Animation("anim@amb@office@boardroom@crew@female@var_b@base@", "idle_a", 8f, 1f, 7000, 49, 0f, false, false, false) },
        };

        private static Dictionary<GeneralTypes, Animation> GeneralAnimsList = new Dictionary<GeneralTypes, Animation>()
        {
            { GeneralTypes.Knocked, new Animation("random@dealgonewrong", "idle_a", 1f, 1f, -1, 1, 0, false, false, false) },
            { GeneralTypes.PushingVehicle, new Animation("missfinale_c2ig_11", "pushcar_offcliff_m", 2f, -8f, -1, 35, 0, false, false, false) },

            { GeneralTypes.RagdollElectrocute, new Animation("ragdoll@human", "electrocute", 8f, 0f, -1, 39, 0, false, false, false) },

            { GeneralTypes.CarryA, new Animation("missfinale_c2mcs_1", "fin_c2_mcs_1_camman", 8f, -8f, -1, 48, 0, false, false, false) },
            { GeneralTypes.CarryB, new Animation("nm", "firemans_carry", 1f, 0f, -1, 1, 0, false, false, false) },

            { GeneralTypes.PiggyBackA, new Animation("anim@arena@celeb@flat@paired@no_props@", "piggyback_c_player_a", 8f, -8f, -1, 33, 0, false, false, false) },
            { GeneralTypes.PiggyBackB, new Animation("anim@arena@celeb@flat@paired@no_props@", "piggyback_c_player_b", 8f, -8f, -1, 49, 0, false, false, false) },

            { GeneralTypes.HostageA, new Animation("anim@gangops@hostage@", "perp_idle", 8f, -8f, -1, 49, 0, false, false, false) },
            { GeneralTypes.HostageB, new Animation("anim@gangops@hostage@", "victim_idle", 8f, -8f, -1, 49, 0, false, false, false) },

            { GeneralTypes.LieInTrunk, new Animation("timetable@floyd@cryingonbed@base", "base", 8f, -8f, -1, 1, 0, false, false, false) },
        };

        public static Dictionary<OtherTypes, Animation> OtherAnimsList { get; private set; } = new Dictionary<OtherTypes, Animation>()
        {
            // Social
            { OtherTypes.Busted, new Animation("random@arrests@busted", "idle_c", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Busted2, new Animation("busted", "idle_2_hands_up", 8, 1, -1, 2, 0, false, false, false) },
            { OtherTypes.Hysterics, new Animation("amb@code_human_cower@female@idle_a", "idle_c", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.GetScared, new Animation("anim@heists@fleeca_bank@hostages@intro", "intro_ped_e", 8, 1, -1, 2, 0, false, false, false) },
            { OtherTypes.MonologueEmotional, new Animation("special_ped@clinton@monologue_6@monologue_6d", "war_crimes_3", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.GiveUp, new Animation("anim@mp_player_intuppersurrender", "enter", 8, 1, -1, 48, 0, false, false, false) },
            { OtherTypes.GiveUp2, new Animation("mp_pol_bust_out", "guard_handsup_intro", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Joy, new Animation("rcmfanatic1celebrate", "celebrate", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Joy2, new Animation("amb@world_human_cheering@female_a", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Cheer, new Animation("random@street_race", "_streetracer_accepted", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Cheer2, new Animation("amb@world_human_cheering@male_b", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Attention, new Animation("random@prisoner_lift", "arms_waving", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Respect, new Animation("anim@mp_player_intcelebrationfemale@bro_love", "bro_love", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Respect2, new Animation("anim@mp_player_intcelebrationmale@respect", "respect", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Clap, new Animation("anim@mp_player_intupperslow_clap", "idle_a", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Clap2, new Animation("amb@world_human_cheering@female_d", "base", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Clap3, new Animation("amb@world_human_cheering@male_a", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Salute, new Animation("mp_player_int_uppersalute", "mp_player_int_salute_enter", 8, 1, -1, 50, 0, false, false, false) },
            { OtherTypes.Salute2, new Animation("mp_player_intsalute", "mp_player_int_salute", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Explain, new Animation("misscarsteal4@actor", "actor_berating_loop", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.WagFinger, new Animation("anim@mp_player_intincarno_waybodhi@ps@", "idle_a_fp", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Facepalm, new Animation("anim@mp_player_intcelebrationfemale@face_palm", "face_palm", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.KeepChest, new Animation("amb@code_human_in_car_mp_actions@tit_squeeze@std@ps@base", "idle_a", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Goat, new Animation("amb@code_human_in_car_mp_actions@rock@bodhi@rps@base", "idle_a", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.UCrazy, new Animation("anim@mp_player_intincaryou_locobodhi@ds@", "idle_a_fp", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.AirKiss, new Animation("anim@mp_player_intselfieblow_kiss", "exit", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.AirKiss2, new Animation("anim@mp_player_intcelebrationmale@blow_kiss", "blow_kiss", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.AirKiss3, new Animation("anim@mp_player_intcelebrationfemale@finger_kiss", "finger_kiss", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Heartbreak, new Animation("misscarsteal4@director_grip", "end_loop_director", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Peace, new Animation("anim@mp_player_intupperpeace", "idle_a", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Like, new Animation("anim@mp_player_intupperthumbs_up", "idle_a_fp", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Vertigo, new Animation("anim@mp_player_intupperyou_loco", "idle_a", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.FlirtOnCar, new Animation("random@street_race", "_car_a_flirt_girl", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Cry, new Animation("random@robbery", "f_cower_01", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.ThreatToKill, new Animation("anim@mp_player_intcelebrationmale@cut_throat", "cut_throat", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.FingerShot, new Animation("anim@mp_player_intcelebrationmale@bang_bang", "bang_bang", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.NumberMe, new Animation("anim@mp_player_intcelebrationmale@call_me", "call_me", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.WannaFight, new Animation("switch@franklin@gang_taunt_p3", "gang_taunt_with_lamar_loop_g2", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.GuardStop, new Animation("anim@amb@casino@peds@", "mini_strip_club_idles_bouncer_stop_stop", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Muscles, new Animation("amb@world_human_muscle_flex@arms_in_front@idle_a", "idle_b", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Muscles2, new Animation("amb@world_human_muscle_flex@arms_in_front@idle_a", "idle_c", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.TakeFlirt, new Animation("amb@world_human_prostitute@cokehead@idle_a", "idle_a", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.CoquettishlyStand, new Animation("mini@strip_club@idles@stripper", "stripper_idle_01", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.CoquettishlyWave, new Animation("mini@strip_club@idles@stripper", "stripper_idle_02", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.KeepCalm, new Animation("amb@code_human_police_crowd_control@idle_a", "idle_c", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.CheckMouthOdor, new Animation("mp_move@prostitute@m@cokehead", "idle", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.ShakeHandsFear, new Animation("anim@mp_player_intupperjazz_hands", "idle_a", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.LowWave, new Animation("anim@mp_player_intupperwave", "idle_a", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.CoverFace, new Animation("anim@mp_player_intupperface_palm", "enter_fp", 8, 1, -1, 49, 0, false, false, false) },

            // Dialogs
            { OtherTypes.Argue, new Animation("amb@world_human_hang_out_street@female_arms_crossed@idle_a", "idle_c", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Nod, new Animation("amb@world_human_hang_out_street@male_c@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Agree, new Animation("amb@world_human_hang_out_street@male_b@idle_a", "idle_d", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.WhatAPeople, new Animation("special_ped@jessie@trevor_1@trevor_1j", "dadwhatthefuck_9", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Rebellion, new Animation("timetable@lamar@ig_4", "nothing_to_see_here_stretch", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Listen, new Animation("timetable@lamar@ig_4", "hey_one_time_stretch", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Listen1, new Animation("mpcas2_int-8", "csb_agatha_dual-8", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Listen2, new Animation("mpcas4_int-0", "mp_m_freemode_01^3_dual-0", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Worry, new Animation("anim@amb@casino@brawl@reacts@standing@", "f_standing_01_gawk_loop_01", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.MonologueEmotional2, new Animation("mpsui_int-6", "csb_agatha_dual-6", 8, 1, -1, 17, 0, false, false, false) },
            { OtherTypes.Listen3, new Animation("anim@amb@casino@peds@", "amb_world_human_hang_out_street_male_c_base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Waiting, new Animation("anim@heists@ornate_bank@chat_manager", "charm", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.SpeakAgree, new Animation("anim@heists@ornate_bank@chat_manager", "nice_car", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.ListenBro, new Animation("low_fun_mcs1-3", "mp_m_g_vagfun_01^6_dual-3", 8, 1, -1, 17, 0, false, false, false) },
            { OtherTypes.Listen4, new Animation("anim@miss@low@fin@vagos@", "idle_ped05", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Explain2, new Animation("special_ped@jerome@monologue_6@monologue_6g", "youthinkyourhappy_6", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Goodbye, new Animation("random@shop_tattoo", "_greeting", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Speak, new Animation("anim_heist@arcade_combined@", "world_human_hang_out_street@_male_a@_idle_a_idle_c", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.GoodbyeBow, new Animation("anim@arena@celeb@podium@no_prop@", "regal_a_1st", 8, 1, -1, 1, 0, false, false, false) },

            // Reactions
            { OtherTypes.Agree2, new Animation("gestures@m@sitting@generic@casual", "gesture_pleased", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Disagree, new Animation("gestures@m@sitting@generic@casual", "gesture_head_no", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Disagree2, new Animation("gestures@m@sitting@generic@casual", "gesture_no_way", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Disagree3, new Animation("mp_player_int_upper_nod", "mp_player_int_nod_no", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.IDontKnow, new Animation("gestures@m@sitting@generic@casual", "gesture_shrug_hard", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Heartbreak2, new Animation("rcmfanatic1out_of_breath", "p_zero_tired_01", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Agree3, new Animation("anim@mp_player_intcelebrationmale@finger_kiss", "finger_kiss", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.BadSmell, new Animation("anim@mp_player_intcelebrationmale@stinker", "stinker", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.IWatchU, new Animation("anim@mp_player_intupperv_sign", "idle_a", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.FuckU, new Animation("mp_player_int_upperv_sign", "mp_player_int_v_sign", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.ThatsHowUDoThat, new Animation("mp_player_introck", "mp_player_int_rock", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Enough, new Animation("anim@heists@ornate_bank@chat_manager", "fail", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Sad, new Animation("friends@frl@ig_1", "idle_a_lamar", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.BrainExplosion, new Animation("anim@mp_player_intcelebrationmale@mind_blown", "mind_blown", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Agree4, new Animation("mp_cp_welcome_tutgreet", "greet", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Disagree4, new Animation("anim@arena@celeb@podium@no_prop@", "dance_b_3rd", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Bewilderment, new Animation("mpcas6_int-18", "mp_m_freemode_01^3_dual-18", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Agree5, new Animation("missheistpaletoscoresetup", "trevor_arrival_2", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.IDontKnow2, new Animation("anim@mp_celebration@draw@male", "draw_react_male_a", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Surprised, new Animation("hs3f_int1-0", "hc_driver_dual-0", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Surprised2, new Animation("random@arrests", "thanks_male_05", 8, 1, -1, 1, 0, false, false, false) },

            // Seat/Lie
            { OtherTypes.Lie, new Animation("amb@world_human_bum_slumped@male@laying_on_left_side@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Lie2, new Animation("amb@lo_res_idles@", "lying_face_up_lo_res_base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Lie3, new Animation("rcmtmom_2leadinout", "tmom_2_leadout_loop", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Lie4, new Animation("amb@lo_res_idles@", "lying_face_down_lo_res_base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Lie5, new Animation("amb@world_human_sunbathe@female@front@idle_a", "idle_c", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Lie6, new Animation("combat@damage@writhe", "writhe_loop", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Lie7, new Animation("timetable@denice@ig_1", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Lie8, new Animation("amb@world_human_sunbathe@female@back@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat, new Animation("timetable@tracy@ig_7@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat2, new Animation("anim@heists@fleeca_bank@ig_7_jetski_owner", "owner_idle", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Lie9, new Animation("misstrevor3_beatup", "guard_beatup_kickidle_dockworker", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat3, new Animation("amb@medic@standing@kneel@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat4, new Animation("missfam2leadinoutmcs3", "onboat_leadin_pornguy_a", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat5, new Animation("random@robbery", "sit_down_idle_01", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat6, new Animation("random@robbery", "sit_down_idle_01", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat7, new Animation("misstrevor2", "gang_chatting_base_a", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat8, new Animation("switch@michael@sitting_on_car_premiere", "sitting_on_car_premiere_loop_player", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat9, new Animation("amb@world_human_picnic@female@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat10, new Animation("switch@michael@tv_w_kids", "001520_02_mics3_14_tv_w_kids_idle_trc", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat11, new Animation("amb@world_human_picnic@male@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat12, new Animation("amb@world_human_stupor@male@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat13, new Animation("anim@amb@nightclub@lazlow@lo_alone@", "lowalone_dlg_longrant_laz", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat14, new Animation("missheistdockssetup1ig_10@base", "talk_pipe_base_worker1", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat15, new Animation("missfbi4leadinoutfbi_4_int", "fbi_4_int_trv_idle_andreas", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat16, new Animation("missfbi4leadinoutfbi_4_int", "fbi_4_int_trv_idle_dave", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat17, new Animation("anim@miss@low@fin@lamar@", "idle", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat18, new Animation("anim@amb@casino@out_of_money@ped_male@02b@idles", "idle_a", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat19, new Animation("anim@amb@casino@brawl@reacts@hr_blackjack@bg_blackjack_breakout_t02@bg_blackjack_breakout_t02_s01_s03@", "playing_loop_female_01", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat20, new Animation("anim@amb@casino@brawl@reacts@hr_blackjack@bg_blackjack_breakout_t02@bg_blackjack_breakout_t02_s01_s03@", "playing_loop_female_02", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat21, new Animation("anim@amb@business@cfid@cfid_desk_no_work_bgen_chair_no_work@", "leg_smacking_lazyworker", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat22, new Animation("rcmjosh3", "sit_stairs_idle", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Seat23, new Animation("amb@code_human_cower@male@react_cowering", "base_back_left", 8, 1, -1, 1, 0, false, false, false) },

            // Sport
            { OtherTypes.Press, new Animation("amb@world_human_sit_ups@male@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.PushUps, new Animation("amb@world_human_push_ups@male@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.PushUps2, new Animation("switch@franklin@press_ups", "pressups_loop", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Backflip, new Animation("anim@arena@celeb@flat@solo@no_props@", "flip_a_player_a", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Fists, new Animation("anim@mp_player_intupperknuckle_crunch", "idle_a", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Yoga, new Animation("amb@world_human_yoga@female@base", "base_b", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Yoga2, new Animation("amb@world_human_yoga@female@base", "base_c", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Yoga3, new Animation("missfam5_yoga", "f_yogapose_b", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Yoga4, new Animation("missfam5_yoga", "f_yogapose_b", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Yoga5, new Animation("missfam5_yoga", "a3_pose", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Yoga6, new Animation("missfam5_yoga", "a2_pose", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Run, new Animation("rcmfanatic1", "jogging_on_spot", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Pose, new Animation("amb@world_human_muscle_flex@arms_in_front@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Swallow, new Animation("rcmfanatic1maryann_stretchidle_b", "idle_e", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Meditation, new Animation("rcmcollect_paperleadinout@", "meditiate_idle", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.RunFemale, new Animation("amb@world_human_jog_standing@female@idle_a", "idle_a", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.RunMale, new Animation("amb@world_human_jog_standing@male@fitidle_a", "idle_a", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Karate, new Animation("anim@mp_player_intcelebrationmale@karate_chops", "karate_chops", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Box, new Animation("anim@mp_player_intcelebrationmale@shadow_boxing", "shadow_boxing", 8, 1, -1, 1, 0, false, false, false) },

            // Indecent
            { OtherTypes.FuckU2, new Animation("veh@driveby@first_person@driver@unarmed", "intro_0", 8, 1, -1, 50, 0, false, false, false) },
            { OtherTypes.FuckU3, new Animation("anim@mp_player_intcelebrationmale@finger", "finger", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.FuckU4, new Animation("anim@mp_player_intupperfinger", "idle_a", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.FuckU5, new Animation("mp_player_intfinger", "mp_player_int_finger", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Jerk, new Animation("mp_player_int_upperwank", "mp_player_int_wank_01", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Chicken, new Animation("anim@mp_player_intcelebrationfemale@chicken_taunt", "chicken_taunt", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Ass, new Animation("switch@trevor@mocks_lapdance", "001443_01_trvs_28_idle_stripper", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.FuckFingers, new Animation("anim@mp_player_intincardockbodhi@rds@", "idle_a_fp", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.PickNose, new Animation("anim@mp_player_intuppernose_pick", "idle_a", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Dumb, new Animation("anim@mp_player_intupperthumb_on_ears", "idle_a", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Tease, new Animation("anim@mp_player_intcelebrationmale@cry_baby", "cry_baby", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dumb2, new Animation("anim@mp_player_intcelebrationfemale@jazz_hands", "jazz_hands", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dumb3, new Animation("anim@mp_player_intcelebrationfemale@thumb_on_ears", "thumb_on_ears", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dumb4, new Animation("anim@mp_player_intcelebrationmale@jazz_hands", "jazz_hands", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.IGotSmthForU, new Animation("anim@arena@celeb@podium@no_prop@", "flip_off_b_1st", 8, 1, -1, 17, 0, false, false, false) },
            { OtherTypes.SnotShot, new Animation("anim@mp_player_intcelebrationmale@nose_pick", "nose_pick", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Scratch, new Animation("friends@frl@ig_1", "idle_c_lamar", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.ScratchAss, new Animation("anim@mp_corona_idles@male_a@idle_a", "idle_e", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.ShakeBoobs, new Animation("mini@strip_club@backroom@", "stripper_b_backroom_idle_b", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.KeepCock, new Animation("mp_player_int_uppergrab_crotch", "mp_player_int_grab_crotch", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.IndecentJoy, new Animation("anim@mp_player_intcelebrationfemale@air_shagging", "air_shagging", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.IndecentJoy2, new Animation("anim@mp_player_intcelebrationmale@air_shagging", "air_shagging", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.SexMale, new Animation("rcmpaparazzo_2", "shag_action_a", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.SexFemale, new Animation("rcmpaparazzo_2", "shag_action_poppy", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.SexMale2, new Animation("rcmpaparazzo_2", "shag_loop_a", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.SexFemale2, new Animation("rcmpaparazzo_2", "shag_loop_poppy", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.SexMale3, new Animation("timetable@trevor@skull_loving_bear", "skull_loving_bear", 8, 1, -1, 1, 0, false, false, false) },

            // Stand Poses
            { OtherTypes.GuardStand, new Animation("amb@world_human_stand_guard@male@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.GuardStand2, new Animation("amb@world_human_stand_impatient@female@no_sign@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand, new Animation("anim@amb@casino@hangout@ped_male@stand@02b@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand2, new Animation("amb@world_human_prostitute@hooker@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand3, new Animation("switch@franklin@lamar_tagging_wall", "lamar_tagging_wall_loop_franklin", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Stand4, new Animation("amb@world_human_hang_out_street@female_arms_crossed@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand5, new Animation("mp_corona@single_team", "single_team_intro_boss", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand6, new Animation("mp_corona@single_team", "single_team_loop_boss", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand7, new Animation("amb@world_human_leaning@male@wall@back@legs_crossed@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand8, new Animation("anim@amb@nightclub@gt_idle@", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand9, new Animation("amb@world_human_leaning@male@wall@back@foot_up@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand10, new Animation("mp_cp_welcome_tutleaning", "idle_a", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand11, new Animation("anim@miss@low@fin@vagos@", "idle_ped06", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Stand12, new Animation("missfam5_yoga", "c1_pose", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Stand13, new Animation("rcmbarry", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand14, new Animation("timetable@amanda@ig_2", "ig_2_base_amanda", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Stand15, new Animation("amb@world_human_hang_out_street@female_hold_arm@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand16, new Animation("amb@world_human_prostitute@cokehead@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand17, new Animation("amb@world_human_prostitute@crackhooker@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand18, new Animation("amb@world_human_guard_patrol@male@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand19, new Animation("missfbi4leadinoutfbi_4_int", "agents_idle_b_andreas", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand20, new Animation("amb@world_human_hang_out_street@female_arm_side@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand21, new Animation("switch@michael@parkbench_smoke_ranger", "ranger_nervous_loop", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand22, new Animation("oddjobs@assassinate@guard", "unarmed_fold_arms", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand23, new Animation("friends@frl@ig_1", "look_lamar", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand24, new Animation("missarmenian1ig_13", "lamar_idle_01", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand25, new Animation("mpcas6_int-18", "mp_m_freemode_01_dual-18", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand26, new Animation("anim@amb@casino@hangout@ped_female@stand@01a@idles", "idle_d", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand27, new Animation("anim@amb@casino@hangout@ped_female@stand@02a@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand28, new Animation("amb@world_human_leaning@male@wall@back@foot_up@idle_a", "idle_a", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand29, new Animation("mpcas6_ext-14", "csb_agatha_dual-14", 8, 1, -1, 17, 0, false, false, false) },
            { OtherTypes.Stand30, new Animation("anim@amb@business@bgen@bgen_no_work@", "stand_phone_phoneputdown_sleeping_nowork", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand31, new Animation("anim@amb@business@bgen@bgen_no_work@", "stand_phone_phoneputdown_stretching-noworkfemale", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand32, new Animation("anim@amb@business@bgen@bgen_no_work@", "stand_phone_phoneputdown_idle_nowork", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand33, new Animation("random@street_race", "_streetracer_start_loop", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand34, new Animation("amb@world_human_leaning@female@wall@back@holding_elbow@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand35, new Animation("anim@arena@celeb@podium@no_prop@", "cocky_a_2nd", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand36, new Animation("friends@frt@ig_1", "trevor_impatient_wait_4", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand37, new Animation("missheistdockssetup1ig_2_p1@end_idle", "supervisor_exitdoor_endidle_supervisor", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand38, new Animation("missheistdockssetup1ig_10@idle_d", "talk_pipe_d_worker2", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand39, new Animation("amb@world_human_bum_standing@depressed@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand40, new Animation("amb@world_human_bum_standing@drunk@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand41, new Animation("mini@hookers_spcrackhead", "idle_reject_loop_c", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Stand42, new Animation("mp_move@prostitute@m@french", "idle", 8, 1, -1, 1, 0, false, false, false) },

            // Dances
            { OtherTypes.Dance, new Animation("special_ped@mountain_dancer@monologue_3@monologue_3a", "mnt_dnc_buttwag", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance2, new Animation("misschinese2_crystalmazemcs1_ig", "dance_loop_tao", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance3, new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_a@", "high_center", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance4, new Animation("mini@strip_club@private_dance@part2", "priv_dance_p2", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance5, new Animation("mp_safehouse", "lap_dance_girl", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance6, new Animation("mini@strip_Club@private_dance@part3", "priv_dance_p3", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance7, new Animation("mini@strip_club@lap_dance_2g@ld_2g_p2", "ld_2g_p2_s1", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance8, new Animation("mini@strip_club@lap_dance@ld_girl_a_song_a_p1", "ld_girl_a_song_a_p1_f", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance9, new Animation("mini@strip_club@lap_dance@ld_girl_a_song_a_p2", "ld_girl_a_song_a_p2_f", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance10, new Animation("amb@world_human_prostitute@crackhooker@idle_a", "idle_c", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance11, new Animation("mini@strip_club@idles@stripper", "stripper_idle_04", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance12, new Animation("mini@strip_club@idles@stripper", "stripper_idle_05", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance13, new Animation("mini@strip_club@idles@stripper", "stripper_idle_06", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance14, new Animation("mini@strip_club@lap_dance_2g@ld_2g_p2", "ld_2g_p2_s2", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance15, new Animation("mini@strip_club@private_dance@idle", "priv_dance_idle", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance16, new Animation("amb@world_human_strip_watch_stand@male_a@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance17, new Animation("amb@world_human_strip_watch_stand@male_a@idle_a", "idle_c", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance18, new Animation("amb@world_human_strip_watch_stand@male_b@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance19, new Animation("amb@world_human_strip_watch_stand@male_c@base", "base", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance20, new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_hi_11_buttwiggle_f_laz", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance21, new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_hi_11_turnaround_laz", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance22, new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_hi_15_crazyrobot_laz", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance23, new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_mi_15_robot_laz", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance24, new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_hi_17_spiderman_laz", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance25, new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_hi_17_smackthat_laz", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance26, new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_li_17_ethereal_laz", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance27, new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_mi_17_crotchgrab_laz", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance28, new Animation("anim@amb@nightclub@lazlow@hi_railing@", "ambclub_10_mi_hi_crotchhold_laz", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance29, new Animation("move_clown@p_m_two_idles@", "fidget_short_dance", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance30, new Animation("mini@strip_club@idles@dj@idle_04", "idle_04", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance31, new Animation("anim@mp_player_intcelebrationmale@dj", "dj", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance32, new Animation("anim@mp_player_intupperchicken_taunt", "idle_a", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Dance33, new Animation("missfbi3_sniping", "dance_m_default", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance34, new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_a@", "low_center_up", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance35, new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_b@", "high_center", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance36, new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_b@", "med_center_down", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance37, new Animation("anim@amb@nightclub@dancers@black_madonna_entourage@", "hi_dance_facedj_09_v2_male^5", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance38, new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "hi_dance_facedj_09_v1_female^6", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance39, new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "hi_dance_facedj_09_v1_male^2", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance40, new Animation("timetable@tracy@ig_5@idle_a", "idle_a", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance41, new Animation("timetable@tracy@ig_5@idle_a", "idle_b", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance42, new Animation("timetable@tracy@ig_5@idle_a", "idle_c", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance43, new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "hi_dance_facedj_15_v2_male^6", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance44, new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "hi_dance_facedj_17_v2_male^6", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance45, new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "mi_dance_facedj_15_v2_female^6", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance46, new Animation("anim@amb@nightclub@dancers@crowddance_facedj@hi_intensity", "hi_dance_facedj_09_v1_male^1", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance47, new Animation("anim@amb@nightclub@dancers@crowddance_facedj@hi_intensity", "hi_dance_facedj_15_v1_female^6", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance48, new Animation("anim@amb@nightclub@mini@dance@dance_solo@female@var_a@", "high_center_down", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance49, new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@", "trans_dance_facedj_mi_to_hi_08_v1_male^1", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance50, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_17_v1_female^6", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance51, new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_a@", "med_center_up", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance52, new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_b@", "high_right_down", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance53, new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_hi_13_flyingv_laz", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance54, new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_mi_11_pointthrust_laz", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance55, new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_mi_15_shimmy_laz", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance56, new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_mi_17_teapotthrust_laz", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance57, new Animation("anim@amb@nightclub@mini@dance@dance_solo@female@var_a@", "med_center_up", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance58, new Animation("anim@amb@nightclub@mini@dance@dance_solo@male@var_b@", "high_center_down", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance59, new Animation("anim@amb@nightclub@mini@dance@dance_solo@male@var_b@", "high_center", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Dance60, new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@", "trans_dance_facedj_li_to_mi_11_v1_male^4", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance61, new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "hi_dance_facedj_15_v2_male^2", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance62, new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "hi_dance_facedj_17_v1_male^3", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance63, new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "hi_dance_facedj_17_v2_female^3", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance64, new Animation("anim@amb@nightclub@dancers@crowddance_facedj@hi_intensity", "hi_dance_facedj_17_v2_male^2", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance65, new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@", "trans_dance_facedj_hi_to_mi_09_v1_male^4", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance66, new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@", "trans_dance_facedj_li_to_mi_11_v1_female^3", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance67, new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@", "trans_dance_facedj_mi_to_hi_09_v1_female^3", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance68, new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@from_hi_intensity", "trans_dance_facedj_hi_to_li_09_v1_female^3", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance69, new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@from_low_intensity", "trans_dance_facedj_li_to_hi_09_v1_female^2", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance70, new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@from_med_intensity", "trans_dance_facedj_mi_to_hi_08_v1_female^3", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance71, new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@", "trans_dance_facedj_mi_to_li_09_v1_female^3", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance72, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_09_v1_female^3", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance73, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_09_v2_female^1", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance74, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_09_v2_female^3", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance75, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_09_v2_female^5", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance76, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_11_v1_female^3", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance77, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_11_v1_female^1", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance78, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_11_v1_male^4", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance79, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_13_v2_female^1", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance80, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_15_v2_female^1", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance81, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_15_v2_female^3", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance82, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_17_v1_female^2", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance83, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_17_v2_female^2", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance84, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_17_v2_male^4", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance85, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "li_dance_crowd_09_v2_female^3", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance86, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "mi_dance_crowd_13_v2_female^1", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance87, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "mi_dance_crowd_13_v2_female^5", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance88, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "mi_dance_crowd_10_v2_female^5", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance89, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "mi_dance_crowd_17_v2_female^1", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance90, new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "mi_dance_crowd_17_v2_female^6", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance91, new Animation("anim@amb@nightclub@dancers@crowddance_groups@hi_intensity", "hi_dance_crowd_09_v2_female^3", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance92, new Animation("anim@amb@nightclub@dancers@crowddance_groups@hi_intensity", "hi_dance_crowd_11_v1_female^1", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance93, new Animation("anim@amb@nightclub@dancers@crowddance_groups@hi_intensity", "hi_dance_crowd_17_v2_female^2", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance94, new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@", "trans_dance_facedj_hi_to_mi_09_v1_female^1", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance95, new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_a@", "low_center", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance96, new Animation("anim@mp_player_intupperuncle_disco", "idle_a", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Dance97, new Animation("anim@mp_player_intuppersalsa_roll", "idle_a", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Dance98, new Animation("anim@mp_player_intupperraise_the_roof", "idle_a", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Dance99, new Animation("anim@mp_player_intupperoh_snap", "idle_a", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Dance100, new Animation("anim@mp_player_intuppercats_cradle", "idle_a", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Dance101, new Animation("anim@mp_player_intupperbanging_tunes", "idle_a", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Dance102, new Animation("anim@mp_player_intcelebrationmale@heart_pumping", "heart_pumping", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance103, new Animation("anim@mp_player_intcelebrationmale@the_woogie", "the_woogie", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Dance104, new Animation("anim@mp_player_intupperfind_the_fish", "idle_a", 8, 1, -1, 49, 0, false, false, false) },

            // Situative
            { OtherTypes.LookAtSmth, new Animation("missfbi1", "ext_t14_leaning_loop", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.KnockDoor, new Animation("timetable@jimmy@doorknock@", "knockdoor_idle", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.CleanTable, new Animation("timetable@floyd@clean_kitchen@idle_a", "idle_b", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.WashSmth, new Animation("amb@world_human_maid_clean@idle_a", "idle_a", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.DJ, new Animation("anim@mp_player_intupperdj", "idle_a", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.Guitar, new Animation("anim@mp_player_intupperair_guitar", "idle_a_fp", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.Drums, new Animation("anim@mp_player_intcelebrationmale@air_drums", "air_drums", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.TossCoin, new Animation("anim@mp_player_intcelebrationmale@coin_roll_and_toss", "coin_roll_and_toss", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.LookAround, new Animation("oddjobs@hunter", "idle_a", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.LookAtSmth2, new Animation("amb@world_human_guard_patrol@male@idle_b", "idle_e", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.PointAtSmth, new Animation("gestures@m@sitting@generic@casual", "gesture_point", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.LookAtSmthGround, new Animation("amb@medic@standing@kneel@idle_a", "idle_a", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.DropSig, new Animation("anim@mp_player_intcelebrationmale@smoke_flick", "smoke_flick", 8, 1, -1, 1, 0, false, false, false) },

            // With weapon
            { OtherTypes.AimLie, new Animation("missfbi3_sniping", "prone_michael", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.AimCrouch, new Animation("combat@chg_stance", "crouch", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.HurryUp, new Animation("combat@gestures@rifle@beckon", "0", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.LookAroundWeapon, new Animation("combat@gestures@rifle@panic", "180", 8, 1, -1, 49, 0, false, false, false) },
            { OtherTypes.LookFromCoverWeapon, new Animation("missheistpaletoscore2ig_8_p2", "start_loop_a_player2", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.CoverWeapon, new Animation("missheistpaletoscore2mcs_2_pt1", "start_loop_player2", 8, 1, -1, 1, 0, false, false, false) },
            { OtherTypes.CoverWeapon2, new Animation("cover@idles@ai@1h@high@_b", "idle_l_corner", 8, 1, -1, 1, 0, false, false, false) },
        };

        public static Dictionary<EmotionTypes, string> EmotionsList { get; private set; } = new Dictionary<EmotionTypes, string>()
        {
            { EmotionTypes.Angry, "mood_angry_1" },
            { EmotionTypes.Drunk, "mood_drunk_1" },
            { EmotionTypes.Dumb, "pose_injured_1" },
            { EmotionTypes.Electrocuted, "electrocuted_1" },
            { EmotionTypes.Grumpy,"effort_1" },
            { EmotionTypes.Grumpy2, "mood_drivefast_1" },
            { EmotionTypes.Grumpy3, "pose_angry_1" },
            { EmotionTypes.Happy, "mood_happy_1" },
            { EmotionTypes.Injured, "mood_injured_1" },
            { EmotionTypes.Joyful, "mood_dancing_low_1" },
            { EmotionTypes.Mouthbreather, "smoking_hold_1" },
            { EmotionTypes.NeverBlink, "pose_normal_1" },
            { EmotionTypes.OneEye, "pose_aiming_1" },
            { EmotionTypes.Shocked, "shocked_1" },
            { EmotionTypes.Shocked2, "shocked_2" },
            { EmotionTypes.Sleeping, "mood_sleeping_1" },
            { EmotionTypes.Sleeping2, "dead_1" },
            { EmotionTypes.Sleeping3, "dead_2" },
            { EmotionTypes.Smug, "mood_smug_1" },
            { EmotionTypes.Speculative, "mood_aiming_1" },
            { EmotionTypes.Stressed, "mood_stressed_1" },
            { EmotionTypes.Sulking, "mood_sulk_1" },
            { EmotionTypes.Weird, "effort_2" },
            { EmotionTypes.Weird2, "effort_3" },
        };

        public static Dictionary<WalkstyleTypes, string> WalkstylesList { get; private set; } = new Dictionary<WalkstyleTypes, string>()
        {
            { WalkstyleTypes.Alien, "move_m@alien" },
            { WalkstyleTypes.Armored, "anim_group_move_ballistic" },
            { WalkstyleTypes.Arrogant, "move_f@arrogant@a" },
            { WalkstyleTypes.Brave, "move_m@brave" },
            { WalkstyleTypes.Casual, "move_m@casual@a" },
            { WalkstyleTypes.Casual2, "move_m@casual@b" },
            { WalkstyleTypes.Casual3, "move_m@casual@c" },
            { WalkstyleTypes.Casual4, "move_m@casual@d" },
            { WalkstyleTypes.Casual5, "move_m@casual@e" },
            { WalkstyleTypes.Casual6, "move_m@casual@f" },
            { WalkstyleTypes.Chichi, "move_f@chichi" },
            { WalkstyleTypes.Confident, "move_m@confident" },
            { WalkstyleTypes.Cop, "move_m@business@a" },
            { WalkstyleTypes.Cop2, "move_m@business@b" },
            { WalkstyleTypes.Cop3, "move_m@business@c" },
            { WalkstyleTypes.DefaultMale, "move_m@multiplayer" },
            { WalkstyleTypes.DefaultFemale, "move_f@multiplayer" },
            { WalkstyleTypes.Drunk, "move_m@drunk@a" },
            { WalkstyleTypes.Drunk2, "move_m@drunk@slightlydrunk" },
            { WalkstyleTypes.Drunk3, "move_m@buzzed" },
            { WalkstyleTypes.Drunk4, "move_m@drunk@verydrunk" },
            { WalkstyleTypes.Femme, "move_f@femme@" },
            { WalkstyleTypes.Fire, "move_characters@franklin@fire" },
            { WalkstyleTypes.Fire2, "move_characters@michael@fire" },
            { WalkstyleTypes.Fire3, "move_m@fire" },
            { WalkstyleTypes.Flee, "move_f@flee@a" },
            { WalkstyleTypes.Franklin, "move_p_m_one" },
            { WalkstyleTypes.Gangster, "move_m@gangster@generic" },
            { WalkstyleTypes.Gangster2, "move_m@gangster@ng" },
            { WalkstyleTypes.Gangster3, "move_m@gangster@var_e" },
            { WalkstyleTypes.Gangster4, "move_m@gangster@var_f" },
            { WalkstyleTypes.Gangster5, "move_m@gangster@var_i" },
            { WalkstyleTypes.Grooving, "anim@move_m@grooving@" },
            { WalkstyleTypes.Guard, "move_m@prison_gaurd" },
            { WalkstyleTypes.Handcuffs, "move_m@prisoner_cuffed" },
            { WalkstyleTypes.Heels, "move_f@heels@c" },
            { WalkstyleTypes.Heels2, "move_f@heels@d" },
            { WalkstyleTypes.Hiking, "move_m@hiking" },
            { WalkstyleTypes.Hipster, "move_m@hipster@a" },
            { WalkstyleTypes.Hobo, "move_m@hobo@a" },
            { WalkstyleTypes.Hurry, "move_f@hurry@a" },
            { WalkstyleTypes.Janitor, "move_p_m_zero_janitor" },
            { WalkstyleTypes.Janitor2, "move_p_m_zero_slow" },
            { WalkstyleTypes.Jog, "move_m@jog@" },
            { WalkstyleTypes.Lemar, "anim_group_move_lemar_alley" },
            { WalkstyleTypes.Lester, "move_heist_lester" },
            { WalkstyleTypes.Lester2, "move_lester_caneup" },
            { WalkstyleTypes.Maneater, "move_f@maneater" },
            { WalkstyleTypes.Michael, "move_ped_bucket" },
            { WalkstyleTypes.Money, "move_m@money" },
            { WalkstyleTypes.Muscle, "move_m@muscle@a" },
            { WalkstyleTypes.Posh, "move_m@posh@" },
            { WalkstyleTypes.Posh2, "move_f@posh@" },
            { WalkstyleTypes.Quick, "move_m@quick" },
            { WalkstyleTypes.Runner, "female_fast_runner" },
            { WalkstyleTypes.Sad, "move_m@sad@a" },
            { WalkstyleTypes.Sassy, "move_m@sassy" },
            { WalkstyleTypes.Sassy2, "move_f@sassy" },
            { WalkstyleTypes.Scared, "move_f@scared" },
            { WalkstyleTypes.Sexy, "move_f@sexy@a" },
            { WalkstyleTypes.Shady, "move_m@shadyped@a" },
            { WalkstyleTypes.Slow, "move_characters@jimmy@slow@" },
            { WalkstyleTypes.Swagger, "move_m@swagger" },
            { WalkstyleTypes.Tough, "move_m@tough_guy@" },
            { WalkstyleTypes.Tough2, "move_f@tough_guy@" },
            { WalkstyleTypes.Trash, "clipset@move@trash_fast_turn" },
            { WalkstyleTypes.Trash2, "missfbi4prepp1_garbageman" },
            { WalkstyleTypes.Trevor, "move_p_m_two" },
            { WalkstyleTypes.Wide, "move_m@bag" },
        };
        #endregion

        public Animations()
        {
            LastSent = DateTime.Now;

            #region Events
            Events.Add("Players::PlayFastAnim", async (object[] args) =>
            {
                Player player = (Player)args[0];

                if (player == null)
                    return;

                FastTypes type = (FastTypes)(int)args[1];

                if (!FastAnimsList.ContainsKey(type))
                    return;

                Play(player, FastAnimsList[type]);
            });

            Events.Add("Players::StopAnim", async (object[] args) =>
            {
                Player player = (Player)args[0];

                if (player == null)
                    return;

                Stop(player);
            });
            #endregion
        }

        private static Utils.Actions[] ActionsToCheck = new Utils.Actions[]
        {
            Utils.Actions.Knocked,
            Utils.Actions.Frozen,
            Utils.Actions.Cuffed,

            //Utils.Actions.Crouch,
            Utils.Actions.Crawl,
            Utils.Actions.Finger,
            Utils.Actions.PushingVehicle,

            Utils.Actions.Animation,
            Utils.Actions.FastAnimation,
            Utils.Actions.Scenario,

            Utils.Actions.InVehicle,
            Utils.Actions.InWater,
            Utils.Actions.Shooting, Utils.Actions.Reloading,
            Utils.Actions.Climbing, Utils.Actions.Falling, Utils.Actions.Ragdoll, Utils.Actions.Jumping, Utils.Actions.OnFoot,
        };

        public static void Set(Player player, EmotionTypes emotion)
        {
            if (player == null)
                return;

            if (emotion == EmotionTypes.None)
            {
                player.ClearFacialIdleAnimOverride();

                return;
            }
            else
            {
                Utils.InvokeViaJs(RAGE.Game.Natives.SetFacialIdleAnimOverride, player.Handle, EmotionsList[emotion], 0);
            }
        }

        public static void Set(Player player, WalkstyleTypes walkstyle)
        {
            if (player == null)
                return;

            if (walkstyle == WalkstyleTypes.None)
            {
                player.ResetMovementClipset(0f);

                return;
            }
            else
            {
                Utils.RequestClipSet(WalkstylesList[walkstyle]);

                player.SetMovementClipset(WalkstylesList[walkstyle], Sync.Crouch.ClipSetSwitchTime);
            }
        }

        public static void Play(Player player, GeneralTypes type)
        {
            if (player == null)
                return;

            if (!GeneralAnimsList.ContainsKey(type))
                return;

            Play(player, GeneralAnimsList[type]);
        }

        public static void Play(Player player, OtherTypes type)
        {
            if (player == null)
                return;

            if (!OtherAnimsList.ContainsKey(type))
                return;

            Play(player, OtherAnimsList[type]);
        }

        private static void Play(Player player, Animation anim, int customTime = -1)
        {
            if (player == null)
                return;

            player.ClearTasks();

            Utils.RequestAnimDict(anim.Dict);

            if (player.Handle != Player.LocalPlayer.Handle)
                player.TaskPlayAnim(anim.Dict, anim.Name, anim.BlendInSpeed, anim.BlendOutSpeed, customTime == -1 ? anim.Duration : customTime, anim.Flag, anim.StartOffset, anim.BlockX, anim.BlockY, anim.BlockZ);
            else
                player.TaskPlayAnim(anim.Dict, Utils.IsFirstPersonActive() ? (anim.NameFP ?? anim.Name) : anim.Name, anim.BlendInSpeed, anim.BlendOutSpeed, customTime == -1 ? anim.Duration : customTime, anim.Flag, anim.StartOffset, anim.BlockX, anim.BlockY, anim.BlockZ);
        }

        public static void Stop(Player player)
        {
            if (player == null)
                return;

            player.ClearTasksImmediately();
        }

        public static void PlaySync(FastTypes fastType, int delay = 1000)
        {
            if (!Utils.CanDoSomething(ActionsToCheck))
                return;

            if (LastSent.IsSpam(delay, false, false))
                return;

            Events.CallRemote("Players::PlayAnim", true, (int)fastType);

            LastSent = DateTime.Now;
        }
    }
}
