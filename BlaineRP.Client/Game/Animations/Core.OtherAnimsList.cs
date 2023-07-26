using System.Collections.Generic;

namespace BlaineRP.Client.Game.Animations
{
    public partial class Core
    {
        public static Dictionary<OtherTypes, Animation> OtherAnimsList { get; private set; } =
            new Dictionary<OtherTypes, Animation>()
            {
                // Social
                {
                    OtherTypes.Busted,
                    new Animation("random@arrests@busted", "idle_c", 8, 1, -1, 49, 0, false, false, false)
                },
                { OtherTypes.Busted2, new Animation("busted", "idle_2_hands_up", 8, 1, -1, 2, 0, false, false, false) },
                {
                    OtherTypes.Hysterics,
                    new Animation("amb@code_human_cower@female@idle_a", "idle_c", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.GetScared,
                    new Animation("anim@heists@fleeca_bank@hostages@intro",
                        "intro_ped_e",
                        8,
                        1,
                        -1,
                        2,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.MonologueEmotional,
                    new Animation("special_ped@clinton@monologue_6@monologue_6d",
                        "war_crimes_3",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.GiveUp,
                    new Animation("anim@mp_player_intuppersurrender", "enter", 8, 1, -1, 48, 0, false, false, false)
                },
                {
                    OtherTypes.GiveUp2,
                    new Animation("mp_pol_bust_out", "guard_handsup_intro", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Joy,
                    new Animation("rcmfanatic1celebrate", "celebrate", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Joy2,
                    new Animation("amb@world_human_cheering@female_a", "base", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Cheer,
                    new Animation("random@street_race", "_streetracer_accepted", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Cheer2,
                    new Animation("amb@world_human_cheering@male_b", "base", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Attention,
                    new Animation("random@prisoner_lift", "arms_waving", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Respect,
                    new Animation("anim@mp_player_intcelebrationfemale@bro_love",
                        "bro_love",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Respect2,
                    new Animation("anim@mp_player_intcelebrationmale@respect",
                        "respect",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Clap,
                    new Animation("anim@mp_player_intupperslow_clap", "idle_a", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.Clap2,
                    new Animation("amb@world_human_cheering@female_d", "base", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.Clap3,
                    new Animation("amb@world_human_cheering@male_a", "base", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Salute,
                    new Animation("mp_player_int_uppersalute",
                        "mp_player_int_salute_enter",
                        8,
                        1,
                        -1,
                        50,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Salute2,
                    new Animation("mp_player_intsalute", "mp_player_int_salute", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Explain,
                    new Animation("misscarsteal4@actor", "actor_berating_loop", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.WagFinger,
                    new Animation("anim@mp_player_intincarno_waybodhi@ps@",
                        "idle_a_fp",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Facepalm,
                    new Animation("anim@mp_player_intcelebrationfemale@face_palm",
                        "face_palm",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.KeepChest,
                    new Animation("amb@code_human_in_car_mp_actions@tit_squeeze@std@ps@base",
                        "idle_a",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Goat,
                    new Animation("amb@code_human_in_car_mp_actions@rock@bodhi@rps@base",
                        "idle_a",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.UCrazy,
                    new Animation("anim@mp_player_intincaryou_locobodhi@ds@",
                        "idle_a_fp",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.AirKiss,
                    new Animation("anim@mp_player_intselfieblow_kiss", "exit", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.AirKiss2,
                    new Animation("anim@mp_player_intcelebrationmale@blow_kiss",
                        "blow_kiss",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.AirKiss3,
                    new Animation("anim@mp_player_intcelebrationfemale@finger_kiss",
                        "finger_kiss",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Heartbreak,
                    new Animation("misscarsteal4@director_grip",
                        "end_loop_director",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Peace,
                    new Animation("anim@mp_player_intupperpeace", "idle_a", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.Like,
                    new Animation("anim@mp_player_intupperthumbs_up", "idle_a_fp", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.Vertigo,
                    new Animation("anim@mp_player_intupperyou_loco", "idle_a", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.FlirtOnCar,
                    new Animation("random@street_race", "_car_a_flirt_girl", 8, 1, -1, 1, 0, false, false, false)
                },
                { OtherTypes.Cry, new Animation("random@robbery", "f_cower_01", 8, 1, -1, 49, 0, false, false, false) },
                {
                    OtherTypes.ThreatToKill,
                    new Animation("anim@mp_player_intcelebrationmale@cut_throat",
                        "cut_throat",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.FingerShot,
                    new Animation("anim@mp_player_intcelebrationmale@bang_bang",
                        "bang_bang",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.NumberMe,
                    new Animation("anim@mp_player_intcelebrationmale@call_me",
                        "call_me",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.WannaFight,
                    new Animation("switch@franklin@gang_taunt_p3",
                        "gang_taunt_with_lamar_loop_g2",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.GuardStop,
                    new Animation("anim@amb@casino@peds@",
                        "mini_strip_club_idles_bouncer_stop_stop",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Muscles,
                    new Animation("amb@world_human_muscle_flex@arms_in_front@idle_a",
                        "idle_b",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Muscles2,
                    new Animation("amb@world_human_muscle_flex@arms_in_front@idle_a",
                        "idle_c",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.TakeFlirt,
                    new Animation("amb@world_human_prostitute@cokehead@idle_a",
                        "idle_a",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.CoquettishlyStand,
                    new Animation("mini@strip_club@idles@stripper",
                        "stripper_idle_01",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.CoquettishlyWave,
                    new Animation("mini@strip_club@idles@stripper",
                        "stripper_idle_02",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.KeepCalm,
                    new Animation("amb@code_human_police_crowd_control@idle_a",
                        "idle_c",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.CheckMouthOdor,
                    new Animation("mp_move@prostitute@m@cokehead", "idle", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.ShakeHandsFear,
                    new Animation("anim@mp_player_intupperjazz_hands", "idle_a", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.LowWave,
                    new Animation("anim@mp_player_intupperwave", "idle_a", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.CoverFace,
                    new Animation("anim@mp_player_intupperface_palm", "enter_fp", 8, 1, -1, 49, 0, false, false, false)
                },

                // Dialogs
                {
                    OtherTypes.Argue,
                    new Animation("amb@world_human_hang_out_street@female_arms_crossed@idle_a",
                        "idle_c",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Nod,
                    new Animation("amb@world_human_hang_out_street@male_c@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Agree,
                    new Animation("amb@world_human_hang_out_street@male_b@idle_a",
                        "idle_d",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.WhatAPeople,
                    new Animation("special_ped@jessie@trevor_1@trevor_1j",
                        "dadwhatthefuck_9",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Rebellion,
                    new Animation("timetable@lamar@ig_4",
                        "nothing_to_see_here_stretch",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Listen,
                    new Animation("timetable@lamar@ig_4", "hey_one_time_stretch", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Listen1,
                    new Animation("mpcas2_int-8", "csb_agatha_dual-8", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Listen2,
                    new Animation("mpcas4_int-0", "mp_m_freemode_01^3_dual-0", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Worry,
                    new Animation("anim@amb@casino@brawl@reacts@standing@",
                        "f_standing_01_gawk_loop_01",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.MonologueEmotional2,
                    new Animation("mpsui_int-6", "csb_agatha_dual-6", 8, 1, -1, 17, 0, false, false, false)
                },
                {
                    OtherTypes.Listen3,
                    new Animation("anim@amb@casino@peds@",
                        "amb_world_human_hang_out_street_male_c_base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Waiting,
                    new Animation("anim@heists@ornate_bank@chat_manager", "charm", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.SpeakAgree,
                    new Animation("anim@heists@ornate_bank@chat_manager",
                        "nice_car",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.ListenBro,
                    new Animation("low_fun_mcs1-3", "mp_m_g_vagfun_01^6_dual-3", 8, 1, -1, 17, 0, false, false, false)
                },
                {
                    OtherTypes.Listen4,
                    new Animation("anim@miss@low@fin@vagos@", "idle_ped05", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Explain2,
                    new Animation("special_ped@jerome@monologue_6@monologue_6g",
                        "youthinkyourhappy_6",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Goodbye,
                    new Animation("random@shop_tattoo", "_greeting", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Speak,
                    new Animation("anim_heist@arcade_combined@",
                        "world_human_hang_out_street@_male_a@_idle_a_idle_c",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.GoodbyeBow,
                    new Animation("anim@arena@celeb@podium@no_prop@",
                        "regal_a_1st",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },

                // Reactions
                {
                    OtherTypes.Agree2,
                    new Animation("gestures@m@sitting@generic@casual",
                        "gesture_pleased",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Disagree,
                    new Animation("gestures@m@sitting@generic@casual",
                        "gesture_head_no",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Disagree2,
                    new Animation("gestures@m@sitting@generic@casual",
                        "gesture_no_way",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Disagree3,
                    new Animation("mp_player_int_upper_nod",
                        "mp_player_int_nod_no",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.IDontKnow,
                    new Animation("gestures@m@sitting@generic@casual",
                        "gesture_shrug_hard",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Heartbreak2,
                    new Animation("rcmfanatic1out_of_breath", "p_zero_tired_01", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.Agree3,
                    new Animation("anim@mp_player_intcelebrationmale@finger_kiss",
                        "finger_kiss",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.BadSmell,
                    new Animation("anim@mp_player_intcelebrationmale@stinker",
                        "stinker",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.IWatchU,
                    new Animation("anim@mp_player_intupperv_sign", "idle_a", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.FuckU,
                    new Animation("mp_player_int_upperv_sign",
                        "mp_player_int_v_sign",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.ThatsHowUDoThat,
                    new Animation("mp_player_introck", "mp_player_int_rock", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Enough,
                    new Animation("anim@heists@ornate_bank@chat_manager", "fail", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Sad,
                    new Animation("friends@frl@ig_1", "idle_a_lamar", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.BrainExplosion,
                    new Animation("anim@mp_player_intcelebrationmale@mind_blown",
                        "mind_blown",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Agree4,
                    new Animation("mp_cp_welcome_tutgreet", "greet", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Disagree4,
                    new Animation("anim@arena@celeb@podium@no_prop@",
                        "dance_b_3rd",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Bewilderment,
                    new Animation("mpcas6_int-18", "mp_m_freemode_01^3_dual-18", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Agree5,
                    new Animation("missheistpaletoscoresetup", "trevor_arrival_2", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.IDontKnow2,
                    new Animation("anim@mp_celebration@draw@male",
                        "draw_react_male_a",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Surprised,
                    new Animation("hs3f_int1-0", "hc_driver_dual-0", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Surprised2,
                    new Animation("random@arrests", "thanks_male_05", 8, 1, -1, 1, 0, false, false, false)
                },

                // Seat/Lie
                {
                    OtherTypes.Lie,
                    new Animation("amb@world_human_bum_slumped@male@laying_on_left_side@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Lie2,
                    new Animation("amb@lo_res_idles@", "lying_face_up_lo_res_base", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Lie3,
                    new Animation("rcmtmom_2leadinout", "tmom_2_leadout_loop", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Lie4,
                    new Animation("amb@lo_res_idles@",
                        "lying_face_down_lo_res_base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Lie5,
                    new Animation("amb@world_human_sunbathe@female@front@idle_a",
                        "idle_c",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Lie6,
                    new Animation("combat@damage@writhe", "writhe_loop", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Lie7, new Animation("timetable@denice@ig_1", "base", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Lie8,
                    new Animation("amb@world_human_sunbathe@female@back@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Seat,
                    new Animation("timetable@tracy@ig_7@base", "base", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Seat2,
                    new Animation("anim@heists@fleeca_bank@ig_7_jetski_owner",
                        "owner_idle",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Lie9,
                    new Animation("misstrevor3_beatup",
                        "guard_beatup_kickidle_dockworker",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Seat3,
                    new Animation("amb@medic@standing@kneel@base", "base", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Seat4,
                    new Animation("missfam2leadinoutmcs3",
                        "onboat_leadin_pornguy_a",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Seat5,
                    new Animation("random@robbery", "sit_down_idle_01", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Seat6,
                    new Animation("random@robbery", "sit_down_idle_01", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Seat7,
                    new Animation("misstrevor2", "gang_chatting_base_a", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Seat8,
                    new Animation("switch@michael@sitting_on_car_premiere",
                        "sitting_on_car_premiere_loop_player",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Seat9,
                    new Animation("amb@world_human_picnic@female@base", "base", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Seat10,
                    new Animation("switch@michael@tv_w_kids",
                        "001520_02_mics3_14_tv_w_kids_idle_trc",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Seat11,
                    new Animation("amb@world_human_picnic@male@base", "base", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Seat12,
                    new Animation("amb@world_human_stupor@male@base", "base", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Seat13,
                    new Animation("anim@amb@nightclub@lazlow@lo_alone@",
                        "lowalone_dlg_longrant_laz",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Seat14,
                    new Animation("missheistdockssetup1ig_10@base",
                        "talk_pipe_base_worker1",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Seat15,
                    new Animation("missfbi4leadinoutfbi_4_int",
                        "fbi_4_int_trv_idle_andreas",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Seat16,
                    new Animation("missfbi4leadinoutfbi_4_int",
                        "fbi_4_int_trv_idle_dave",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Seat17,
                    new Animation("anim@miss@low@fin@lamar@", "idle", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Seat18,
                    new Animation("anim@amb@casino@out_of_money@ped_male@02b@idles",
                        "idle_a",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Seat19,
                    new Animation(
                        "anim@amb@casino@brawl@reacts@hr_blackjack@bg_blackjack_breakout_t02@bg_blackjack_breakout_t02_s01_s03@",
                        "playing_loop_female_01",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Seat20,
                    new Animation(
                        "anim@amb@casino@brawl@reacts@hr_blackjack@bg_blackjack_breakout_t02@bg_blackjack_breakout_t02_s01_s03@",
                        "playing_loop_female_02",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Seat21,
                    new Animation("anim@amb@business@cfid@cfid_desk_no_work_bgen_chair_no_work@",
                        "leg_smacking_lazyworker",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Seat22, new Animation("rcmjosh3", "sit_stairs_idle", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Seat23,
                    new Animation("amb@code_human_cower@male@react_cowering",
                        "base_back_left",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },

                // Sport
                {
                    OtherTypes.Press,
                    new Animation("amb@world_human_sit_ups@male@base", "base", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.PushUps,
                    new Animation("amb@world_human_push_ups@male@base", "base", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.PushUps2,
                    new Animation("switch@franklin@press_ups", "pressups_loop", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Backflip,
                    new Animation("anim@arena@celeb@flat@solo@no_props@",
                        "flip_a_player_a",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Fists,
                    new Animation("anim@mp_player_intupperknuckle_crunch",
                        "idle_a",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Yoga,
                    new Animation("amb@world_human_yoga@female@base", "base_b", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Yoga2,
                    new Animation("amb@world_human_yoga@female@base", "base_c", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Yoga3,
                    new Animation("missfam5_yoga", "f_yogapose_b", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Yoga4,
                    new Animation("missfam5_yoga", "f_yogapose_b", 8, 1, -1, 1, 0, false, false, false)
                },
                { OtherTypes.Yoga5, new Animation("missfam5_yoga", "a3_pose", 8, 1, -1, 1, 0, false, false, false) },
                { OtherTypes.Yoga6, new Animation("missfam5_yoga", "a2_pose", 8, 1, -1, 1, 0, false, false, false) },
                {
                    OtherTypes.Run, new Animation("rcmfanatic1", "jogging_on_spot", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Pose,
                    new Animation("amb@world_human_muscle_flex@arms_in_front@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Swallow,
                    new Animation("rcmfanatic1maryann_stretchidle_b", "idle_e", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Meditation,
                    new Animation("rcmcollect_paperleadinout@", "meditiate_idle", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.RunFemale,
                    new Animation("amb@world_human_jog_standing@female@idle_a",
                        "idle_a",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.RunMale,
                    new Animation("amb@world_human_jog_standing@male@fitidle_a",
                        "idle_a",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Karate,
                    new Animation("anim@mp_player_intcelebrationmale@karate_chops",
                        "karate_chops",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Box,
                    new Animation("anim@mp_player_intcelebrationmale@shadow_boxing",
                        "shadow_boxing",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },

                // Indecent
                {
                    OtherTypes.FuckU2,
                    new Animation("veh@driveby@first_person@driver@unarmed",
                        "intro_0",
                        8,
                        1,
                        -1,
                        50,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.FuckU3,
                    new Animation("anim@mp_player_intcelebrationmale@finger",
                        "finger",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.FuckU4,
                    new Animation("anim@mp_player_intupperfinger", "idle_a", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.FuckU5,
                    new Animation("mp_player_intfinger", "mp_player_int_finger", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Jerk,
                    new Animation("mp_player_int_upperwank",
                        "mp_player_int_wank_01",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Chicken,
                    new Animation("anim@mp_player_intcelebrationfemale@chicken_taunt",
                        "chicken_taunt",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Ass,
                    new Animation("switch@trevor@mocks_lapdance",
                        "001443_01_trvs_28_idle_stripper",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.FuckFingers,
                    new Animation("anim@mp_player_intincardockbodhi@rds@",
                        "idle_a_fp",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.PickNose,
                    new Animation("anim@mp_player_intuppernose_pick", "idle_a", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.Dumb,
                    new Animation("anim@mp_player_intupperthumb_on_ears",
                        "idle_a",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Tease,
                    new Animation("anim@mp_player_intcelebrationmale@cry_baby",
                        "cry_baby",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dumb2,
                    new Animation("anim@mp_player_intcelebrationfemale@jazz_hands",
                        "jazz_hands",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dumb3,
                    new Animation("anim@mp_player_intcelebrationfemale@thumb_on_ears",
                        "thumb_on_ears",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dumb4,
                    new Animation("anim@mp_player_intcelebrationmale@jazz_hands",
                        "jazz_hands",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.IGotSmthForU,
                    new Animation("anim@arena@celeb@podium@no_prop@",
                        "flip_off_b_1st",
                        8,
                        1,
                        -1,
                        17,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.SnotShot,
                    new Animation("anim@mp_player_intcelebrationmale@nose_pick",
                        "nose_pick",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Scratch,
                    new Animation("friends@frl@ig_1", "idle_c_lamar", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.ScratchAss,
                    new Animation("anim@mp_corona_idles@male_a@idle_a", "idle_e", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.ShakeBoobs,
                    new Animation("mini@strip_club@backroom@",
                        "stripper_b_backroom_idle_b",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.KeepCock,
                    new Animation("mp_player_int_uppergrab_crotch",
                        "mp_player_int_grab_crotch",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.IndecentJoy,
                    new Animation("anim@mp_player_intcelebrationfemale@air_shagging",
                        "air_shagging",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.IndecentJoy2,
                    new Animation("anim@mp_player_intcelebrationmale@air_shagging",
                        "air_shagging",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.SexMale,
                    new Animation("rcmpaparazzo_2", "shag_action_a", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.SexFemale,
                    new Animation("rcmpaparazzo_2", "shag_action_poppy", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.SexMale2,
                    new Animation("rcmpaparazzo_2", "shag_loop_a", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.SexFemale2,
                    new Animation("rcmpaparazzo_2", "shag_loop_poppy", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.SexMale3,
                    new Animation("timetable@trevor@skull_loving_bear",
                        "skull_loving_bear",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },

                // Stand Poses
                {
                    OtherTypes.GuardStand,
                    new Animation("amb@world_human_stand_guard@male@base", "base", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.GuardStand2,
                    new Animation("amb@world_human_stand_impatient@female@no_sign@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand,
                    new Animation("anim@amb@casino@hangout@ped_male@stand@02b@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand2,
                    new Animation("amb@world_human_prostitute@hooker@base", "base", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Stand3,
                    new Animation("switch@franklin@lamar_tagging_wall",
                        "lamar_tagging_wall_loop_franklin",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand4,
                    new Animation("amb@world_human_hang_out_street@female_arms_crossed@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand5,
                    new Animation("mp_corona@single_team",
                        "single_team_intro_boss",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand6,
                    new Animation("mp_corona@single_team", "single_team_loop_boss", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Stand7,
                    new Animation("amb@world_human_leaning@male@wall@back@legs_crossed@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand8,
                    new Animation("anim@amb@nightclub@gt_idle@", "base", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Stand9,
                    new Animation("amb@world_human_leaning@male@wall@back@foot_up@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand10,
                    new Animation("mp_cp_welcome_tutleaning", "idle_a", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Stand11,
                    new Animation("anim@miss@low@fin@vagos@", "idle_ped06", 8, 1, -1, 49, 0, false, false, false)
                },
                { OtherTypes.Stand12, new Animation("missfam5_yoga", "c1_pose", 8, 1, -1, 49, 0, false, false, false) },
                { OtherTypes.Stand13, new Animation("rcmbarry", "base", 8, 1, -1, 1, 0, false, false, false) },
                {
                    OtherTypes.Stand14,
                    new Animation("timetable@amanda@ig_2", "ig_2_base_amanda", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.Stand15,
                    new Animation("amb@world_human_hang_out_street@female_hold_arm@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand16,
                    new Animation("amb@world_human_prostitute@cokehead@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand17,
                    new Animation("amb@world_human_prostitute@crackhooker@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand18,
                    new Animation("amb@world_human_guard_patrol@male@base", "base", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Stand19,
                    new Animation("missfbi4leadinoutfbi_4_int",
                        "agents_idle_b_andreas",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand20,
                    new Animation("amb@world_human_hang_out_street@female_arm_side@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand21,
                    new Animation("switch@michael@parkbench_smoke_ranger",
                        "ranger_nervous_loop",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand22,
                    new Animation("oddjobs@assassinate@guard", "unarmed_fold_arms", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Stand23,
                    new Animation("friends@frl@ig_1", "look_lamar", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Stand24,
                    new Animation("missarmenian1ig_13", "lamar_idle_01", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Stand25,
                    new Animation("mpcas6_int-18", "mp_m_freemode_01_dual-18", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Stand26,
                    new Animation("anim@amb@casino@hangout@ped_female@stand@01a@idles",
                        "idle_d",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand27,
                    new Animation("anim@amb@casino@hangout@ped_female@stand@02a@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand28,
                    new Animation("amb@world_human_leaning@male@wall@back@foot_up@idle_a",
                        "idle_a",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand29,
                    new Animation("mpcas6_ext-14", "csb_agatha_dual-14", 8, 1, -1, 17, 0, false, false, false)
                },
                {
                    OtherTypes.Stand30,
                    new Animation("anim@amb@business@bgen@bgen_no_work@",
                        "stand_phone_phoneputdown_sleeping_nowork",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand31,
                    new Animation("anim@amb@business@bgen@bgen_no_work@",
                        "stand_phone_phoneputdown_stretching-noworkfemale",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand32,
                    new Animation("anim@amb@business@bgen@bgen_no_work@",
                        "stand_phone_phoneputdown_idle_nowork",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand33,
                    new Animation("random@street_race", "_streetracer_start_loop", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Stand34,
                    new Animation("amb@world_human_leaning@female@wall@back@holding_elbow@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand35,
                    new Animation("anim@arena@celeb@podium@no_prop@",
                        "cocky_a_2nd",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand36,
                    new Animation("friends@frt@ig_1", "trevor_impatient_wait_4", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Stand37,
                    new Animation("missheistdockssetup1ig_2_p1@end_idle",
                        "supervisor_exitdoor_endidle_supervisor",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand38,
                    new Animation("missheistdockssetup1ig_10@idle_d",
                        "talk_pipe_d_worker2",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand39,
                    new Animation("amb@world_human_bum_standing@depressed@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand40,
                    new Animation("amb@world_human_bum_standing@drunk@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Stand41,
                    new Animation("mini@hookers_spcrackhead", "idle_reject_loop_c", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Stand42,
                    new Animation("mp_move@prostitute@m@french", "idle", 8, 1, -1, 1, 0, false, false, false)
                },

                // Dances
                {
                    OtherTypes.Dance,
                    new Animation("special_ped@mountain_dancer@monologue_3@monologue_3a",
                        "mnt_dnc_buttwag",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance2,
                    new Animation("misschinese2_crystalmazemcs1_ig",
                        "dance_loop_tao",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance3,
                    new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_a@",
                        "high_center",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance4,
                    new Animation("mini@strip_club@private_dance@part2",
                        "priv_dance_p2",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance5,
                    new Animation("mp_safehouse", "lap_dance_girl", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Dance6,
                    new Animation("mini@strip_Club@private_dance@part3",
                        "priv_dance_p3",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance7,
                    new Animation("mini@strip_club@lap_dance_2g@ld_2g_p2",
                        "ld_2g_p2_s1",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance8,
                    new Animation("mini@strip_club@lap_dance@ld_girl_a_song_a_p1",
                        "ld_girl_a_song_a_p1_f",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance9,
                    new Animation("mini@strip_club@lap_dance@ld_girl_a_song_a_p2",
                        "ld_girl_a_song_a_p2_f",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance10,
                    new Animation("amb@world_human_prostitute@crackhooker@idle_a",
                        "idle_c",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance11,
                    new Animation("mini@strip_club@idles@stripper",
                        "stripper_idle_04",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance12,
                    new Animation("mini@strip_club@idles@stripper",
                        "stripper_idle_05",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance13,
                    new Animation("mini@strip_club@idles@stripper",
                        "stripper_idle_06",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance14,
                    new Animation("mini@strip_club@lap_dance_2g@ld_2g_p2",
                        "ld_2g_p2_s2",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance15,
                    new Animation("mini@strip_club@private_dance@idle",
                        "priv_dance_idle",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance16,
                    new Animation("amb@world_human_strip_watch_stand@male_a@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance17,
                    new Animation("amb@world_human_strip_watch_stand@male_a@idle_a",
                        "idle_c",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance18,
                    new Animation("amb@world_human_strip_watch_stand@male_b@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance19,
                    new Animation("amb@world_human_strip_watch_stand@male_c@base",
                        "base",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance20,
                    new Animation("anim@amb@nightclub@lazlow@hi_podium@",
                        "danceidle_hi_11_buttwiggle_f_laz",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance21,
                    new Animation("anim@amb@nightclub@lazlow@hi_podium@",
                        "danceidle_hi_11_turnaround_laz",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance22,
                    new Animation("anim@amb@nightclub@lazlow@hi_podium@",
                        "danceidle_hi_15_crazyrobot_laz",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance23,
                    new Animation("anim@amb@nightclub@lazlow@hi_podium@",
                        "danceidle_mi_15_robot_laz",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance24,
                    new Animation("anim@amb@nightclub@lazlow@hi_podium@",
                        "danceidle_hi_17_spiderman_laz",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance25,
                    new Animation("anim@amb@nightclub@lazlow@hi_podium@",
                        "danceidle_hi_17_smackthat_laz",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance26,
                    new Animation("anim@amb@nightclub@lazlow@hi_podium@",
                        "danceidle_li_17_ethereal_laz",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance27,
                    new Animation("anim@amb@nightclub@lazlow@hi_podium@",
                        "danceidle_mi_17_crotchgrab_laz",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance28,
                    new Animation("anim@amb@nightclub@lazlow@hi_railing@",
                        "ambclub_10_mi_hi_crotchhold_laz",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance29,
                    new Animation("move_clown@p_m_two_idles@",
                        "fidget_short_dance",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance30,
                    new Animation("mini@strip_club@idles@dj@idle_04", "idle_04", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Dance31,
                    new Animation("anim@mp_player_intcelebrationmale@dj", "dj", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Dance32,
                    new Animation("anim@mp_player_intupperchicken_taunt",
                        "idle_a",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance33,
                    new Animation("missfbi3_sniping", "dance_m_default", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Dance34,
                    new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_a@",
                        "low_center_up",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance35,
                    new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_b@",
                        "high_center",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance36,
                    new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_b@",
                        "med_center_down",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance37,
                    new Animation("anim@amb@nightclub@dancers@black_madonna_entourage@",
                        "hi_dance_facedj_09_v2_male^5",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance38,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj@",
                        "hi_dance_facedj_09_v1_female^6",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance39,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj@",
                        "hi_dance_facedj_09_v1_male^2",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance40,
                    new Animation("timetable@tracy@ig_5@idle_a", "idle_a", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Dance41,
                    new Animation("timetable@tracy@ig_5@idle_a", "idle_b", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Dance42,
                    new Animation("timetable@tracy@ig_5@idle_a", "idle_c", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Dance43,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj@",
                        "hi_dance_facedj_15_v2_male^6",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance44,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj@",
                        "hi_dance_facedj_17_v2_male^6",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance45,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj@",
                        "mi_dance_facedj_15_v2_female^6",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance46,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj@hi_intensity",
                        "hi_dance_facedj_09_v1_male^1",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance47,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj@hi_intensity",
                        "hi_dance_facedj_15_v1_female^6",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance48,
                    new Animation("anim@amb@nightclub@mini@dance@dance_solo@female@var_a@",
                        "high_center_down",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance49,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@",
                        "trans_dance_facedj_mi_to_hi_08_v1_male^1",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance50,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "hi_dance_crowd_17_v1_female^6",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance51,
                    new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_a@",
                        "med_center_up",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance52,
                    new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_b@",
                        "high_right_down",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance53,
                    new Animation("anim@amb@nightclub@lazlow@hi_podium@",
                        "danceidle_hi_13_flyingv_laz",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance54,
                    new Animation("anim@amb@nightclub@lazlow@hi_podium@",
                        "danceidle_mi_11_pointthrust_laz",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance55,
                    new Animation("anim@amb@nightclub@lazlow@hi_podium@",
                        "danceidle_mi_15_shimmy_laz",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance56,
                    new Animation("anim@amb@nightclub@lazlow@hi_podium@",
                        "danceidle_mi_17_teapotthrust_laz",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance57,
                    new Animation("anim@amb@nightclub@mini@dance@dance_solo@female@var_a@",
                        "med_center_up",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance58,
                    new Animation("anim@amb@nightclub@mini@dance@dance_solo@male@var_b@",
                        "high_center_down",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance59,
                    new Animation("anim@amb@nightclub@mini@dance@dance_solo@male@var_b@",
                        "high_center",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance60,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@",
                        "trans_dance_facedj_li_to_mi_11_v1_male^4",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance61,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj@",
                        "hi_dance_facedj_15_v2_male^2",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance62,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj@",
                        "hi_dance_facedj_17_v1_male^3",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance63,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj@",
                        "hi_dance_facedj_17_v2_female^3",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance64,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj@hi_intensity",
                        "hi_dance_facedj_17_v2_male^2",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance65,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@",
                        "trans_dance_facedj_hi_to_mi_09_v1_male^4",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance66,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@",
                        "trans_dance_facedj_li_to_mi_11_v1_female^3",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance67,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@",
                        "trans_dance_facedj_mi_to_hi_09_v1_female^3",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance68,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@from_hi_intensity",
                        "trans_dance_facedj_hi_to_li_09_v1_female^3",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance69,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@from_low_intensity",
                        "trans_dance_facedj_li_to_hi_09_v1_female^2",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance70,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@from_med_intensity",
                        "trans_dance_facedj_mi_to_hi_08_v1_female^3",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance71,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@",
                        "trans_dance_facedj_mi_to_li_09_v1_female^3",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance72,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "hi_dance_crowd_09_v1_female^3",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance73,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "hi_dance_crowd_09_v2_female^1",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance74,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "hi_dance_crowd_09_v2_female^3",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance75,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "hi_dance_crowd_09_v2_female^5",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance76,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "hi_dance_crowd_11_v1_female^3",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance77,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "hi_dance_crowd_11_v1_female^1",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance78,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "hi_dance_crowd_11_v1_male^4",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance79,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "hi_dance_crowd_13_v2_female^1",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance80,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "hi_dance_crowd_15_v2_female^1",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance81,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "hi_dance_crowd_15_v2_female^3",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance82,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "hi_dance_crowd_17_v1_female^2",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance83,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "hi_dance_crowd_17_v2_female^2",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance84,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "hi_dance_crowd_17_v2_male^4",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance85,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "li_dance_crowd_09_v2_female^3",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance86,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "mi_dance_crowd_13_v2_female^1",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance87,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "mi_dance_crowd_13_v2_female^5",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance88,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "mi_dance_crowd_10_v2_female^5",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance89,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "mi_dance_crowd_17_v2_female^1",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance90,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@",
                        "mi_dance_crowd_17_v2_female^6",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance91,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@hi_intensity",
                        "hi_dance_crowd_09_v2_female^3",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance92,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@hi_intensity",
                        "hi_dance_crowd_11_v1_female^1",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance93,
                    new Animation("anim@amb@nightclub@dancers@crowddance_groups@hi_intensity",
                        "hi_dance_crowd_17_v2_female^2",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance94,
                    new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@",
                        "trans_dance_facedj_hi_to_mi_09_v1_female^1",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance95,
                    new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_a@",
                        "low_center",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance96,
                    new Animation("anim@mp_player_intupperuncle_disco", "idle_a", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.Dance97,
                    new Animation("anim@mp_player_intuppersalsa_roll", "idle_a", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.Dance98,
                    new Animation("anim@mp_player_intupperraise_the_roof",
                        "idle_a",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance99,
                    new Animation("anim@mp_player_intupperoh_snap", "idle_a", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.Dance100,
                    new Animation("anim@mp_player_intuppercats_cradle", "idle_a", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.Dance101,
                    new Animation("anim@mp_player_intupperbanging_tunes",
                        "idle_a",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance102,
                    new Animation("anim@mp_player_intcelebrationmale@heart_pumping",
                        "heart_pumping",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance103,
                    new Animation("anim@mp_player_intcelebrationmale@the_woogie",
                        "the_woogie",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Dance104,
                    new Animation("anim@mp_player_intupperfind_the_fish",
                        "idle_a",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },

                // Situative
                {
                    OtherTypes.LookAtSmth,
                    new Animation("missfbi1", "ext_t14_leaning_loop", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.KnockDoor,
                    new Animation("timetable@jimmy@doorknock@", "knockdoor_idle", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.CleanTable,
                    new Animation("timetable@floyd@clean_kitchen@idle_a", "idle_b", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.WashSmth,
                    new Animation("amb@world_human_maid_clean@idle_a", "idle_a", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.DJ,
                    new Animation("anim@mp_player_intupperdj", "idle_a", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.Guitar,
                    new Animation("anim@mp_player_intupperair_guitar",
                        "idle_a_fp",
                        8,
                        1,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.Drums,
                    new Animation("anim@mp_player_intcelebrationmale@air_drums",
                        "air_drums",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.TossCoin,
                    new Animation("anim@mp_player_intcelebrationmale@coin_roll_and_toss",
                        "coin_roll_and_toss",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.LookAround,
                    new Animation("oddjobs@hunter", "idle_a", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.LookAtSmth2,
                    new Animation("amb@world_human_guard_patrol@male@idle_b",
                        "idle_e",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.PointAtSmth,
                    new Animation("gestures@m@sitting@generic@casual",
                        "gesture_point",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.LookAtSmthGround,
                    new Animation("amb@medic@standing@kneel@idle_a", "idle_a", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.DropSig,
                    new Animation("anim@mp_player_intcelebrationmale@smoke_flick",
                        "smoke_flick",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },

                // With weapon
                {
                    OtherTypes.AimLie,
                    new Animation("missfbi3_sniping", "prone_michael", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.AimCrouch,
                    new Animation("combat@chg_stance", "crouch", 8, 1, -1, 1, 0, false, false, false)
                },
                {
                    OtherTypes.HurryUp,
                    new Animation("combat@gestures@rifle@beckon", "0", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.LookAroundWeapon,
                    new Animation("combat@gestures@rifle@panic", "180", 8, 1, -1, 49, 0, false, false, false)
                },
                {
                    OtherTypes.LookFromCoverWeapon,
                    new Animation("missheistpaletoscore2ig_8_p2",
                        "start_loop_a_player2",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.CoverWeapon,
                    new Animation("missheistpaletoscore2mcs_2_pt1",
                        "start_loop_player2",
                        8,
                        1,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    OtherTypes.CoverWeapon2,
                    new Animation("cover@idles@ai@1h@high@_b", "idle_l_corner", 8, 1, -1, 1, 0, false, false, false)
                },
            };
    }
}