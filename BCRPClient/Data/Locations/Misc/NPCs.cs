﻿using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Data
{
    public partial class Locations
    {
        public static void InitializeNPCs()
        {
            new NPC("vpound_w_0", "Джон", NPC.Types.Talkable, "ig_trafficwarden", new Vector3(485.6506f, -54.18661f, 78.30058f), 55.38f, Settings.MAIN_DIMENSION)
            {
                Blip = new Additional.ExtraBlip(832, new Vector3(485.6506f, -54.18661f, 78.30058f), "Штрафстоянка", 1f, 47, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION),

                DefaultDialogueId = "vpound_preprocess",
            };

            new NPC("vrent_s_0", "Джон", NPC.Types.Talkable, "s_m_m_trucker_01", new Vector3(-718.6724f, 5821.765f, 17.21804f), 106.9247f, Settings.MAIN_DIMENSION)
            {
                Blip = new Additional.ExtraBlip(76, new Vector3(-718.6724f, 5821.765f, 17.21804f), "Аренда мопедов", 0.85f, 47, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION),

                DefaultDialogueId = "vrent_s_preprocess",
            };

            new NPC($"cop0_{(int)Fractions.Types.COP_BLAINE}", "Майкл", NPC.Types.Talkable, "csb_cop", new Vector3(-448.2888f, 6012.634f, 31.71635f), 313.2359f, Settings.MAIN_DIMENSION)
            {
                DefaultDialogueId = "cop_0_g",
            };

            new NPC("priest_0", "Михаэль", NPC.Types.Talkable, "ig_priest", new Vector3(-1670.684f, -292.9667f, 52.62305f), 10.80344f, Settings.MAIN_DIMENSION)
            {

            };

            new NPC("priest_1", "Сантьяго", NPC.Types.Talkable, "ig_priest", new Vector3(-328.2661f, 2799.341f, 60.18345f), 289.3676f, Settings.MAIN_DIMENSION)
            {

            };

            new NPC("priest_2", "Роберт", NPC.Types.Talkable, "ig_priest", new Vector3(-311.6561f, 6142.962f, 33.04696f), 12.82745f, Settings.MAIN_DIMENSION)
            {

            };

            new NPC("boatm_0", "", NPC.Types.Talkable, "s_m_y_uscg_01", new Vector3(-769.2745f, -1491.8f, 3.709361f), 289.8319f, Settings.MAIN_DIMENSION)
            {
                Blip = new Additional.ExtraBlip(371, new Vector3(-769.2745f, -1491.8f, 0f), Locale.General.NPC.TypeNames.GetValueOrDefault("boatm", "null"), 0.75f, 3, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION, Additional.ExtraBlip.Types.Default),
            };

            new NPC("boatm_1", "", NPC.Types.Talkable, "s_m_y_uscg_01", new Vector3(968.3807f, 3669.91f, 31.20985f), 198.9756f, Settings.MAIN_DIMENSION)
            {
                Blip = new Additional.ExtraBlip(371, new Vector3(968.3807f, 3669.91f, 0f), Locale.General.NPC.TypeNames.GetValueOrDefault("boatm", "null"), 0.75f, 3, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION, Additional.ExtraBlip.Types.Default),
            };

            new NPC("boatm_2", "", NPC.Types.Talkable, "s_m_y_uscg_01", new Vector3(2833.404f, -673.732f, 1.150914f), 107.9428f, Settings.MAIN_DIMENSION)
            {
                Blip = new Additional.ExtraBlip(371, new Vector3(2833.404f, -673.732f, 0f), Locale.General.NPC.TypeNames.GetValueOrDefault("boatm", "null"), 0.75f, 3, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION, Additional.ExtraBlip.Types.Default),
            };

            new NPC("boatm_3", "", NPC.Types.Talkable, "s_m_y_uscg_01", new Vector3(1535.112f, 6633.743f, 2.363752f), 187.1163f, Settings.MAIN_DIMENSION)
            {
                Blip = new Additional.ExtraBlip(371, new Vector3(1535.112f, 6633.743f, 0f), Locale.General.NPC.TypeNames.GetValueOrDefault("boatm", "null"), 0.75f, 3, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION, Additional.ExtraBlip.Types.Default),
            };

            new NPC("airvm_0", "", NPC.Types.Talkable, "s_m_y_airworker", new Vector3(-700.7498f, -1401.783f, 5.495286f), 138.7849f, Settings.MAIN_DIMENSION)
            {
                Blip = new Additional.ExtraBlip(370, new Vector3(-700.7498f, -1401.783f, 0f), Locale.General.NPC.TypeNames.GetValueOrDefault("airvm", "null"), 0.75f, 3, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION, Additional.ExtraBlip.Types.Default),
            };

            new NPC("airvm_1", "", NPC.Types.Talkable, "s_m_y_airworker", new Vector3(1758.6f, 3297.691f, 41.14833f), 142.3068f, Settings.MAIN_DIMENSION)
            {
                Blip = new Additional.ExtraBlip(370, new Vector3(1758.6f, 3297.691f, 0f), Locale.General.NPC.TypeNames.GetValueOrDefault("airvm", "null"), 0.75f, 3, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION, Additional.ExtraBlip.Types.Default),
            };

            new NPC("airvm_2", "", NPC.Types.Talkable, "s_m_y_airworker", new Vector3(-1070.619f, -2868.29f, 13.95183f), 147.5721f, Settings.MAIN_DIMENSION)
            {
                Blip = new Additional.ExtraBlip(370, new Vector3(-1070.619f, -2868.29f, 0f), Locale.General.NPC.TypeNames.GetValueOrDefault("airvm", "null"), 0.75f, 3, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION, Additional.ExtraBlip.Types.Default),
            };

            new NPC("groundvm_0", "", NPC.Types.Talkable, "s_m_y_grip_01", new Vector3(1657.615f, 3799.935f, 35.12451f), 216.7175f, Settings.MAIN_DIMENSION)
            {
                Blip = new Additional.ExtraBlip(832, new Vector3(1657.615f, 3799.935f, 0f), Locale.General.NPC.TypeNames.GetValueOrDefault("groundvm", "null"), 0.75f, 3, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION, Additional.ExtraBlip.Types.Default),
            };

            new NPC("groundvm_1", "", NPC.Types.Talkable, "s_m_y_grip_01", new Vector3(190.1143f, 6624.087f, 31.67836f), 209.2979f, Settings.MAIN_DIMENSION)
            {
                Blip = new Additional.ExtraBlip(832, new Vector3(190.1143f, 6624.087f, 0f), Locale.General.NPC.TypeNames.GetValueOrDefault("groundvm", "null"), 0.75f, 3, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION, Additional.ExtraBlip.Types.Default),
            };

            new NPC("groundvm_2", "", NPC.Types.Talkable, "s_m_y_grip_01", new Vector3(-519.0234f, 44.49887f, 52.57992f), 167.8689f, Settings.MAIN_DIMENSION)
            {
                Blip = new Additional.ExtraBlip(832, new Vector3(-519.0234f, 44.49887f, 0f), Locale.General.NPC.TypeNames.GetValueOrDefault("groundvm", "null"), 0.75f, 3, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION, Additional.ExtraBlip.Types.Default),
            };

            new NPC("groundvm_3", "", NPC.Types.Talkable, "s_m_y_grip_01", new Vector3(-1743.313f, -728.6164f, 10.42183f), 286.1692f, Settings.MAIN_DIMENSION)
            {
                Blip = new Additional.ExtraBlip(832, new Vector3(-1743.313f, -728.6164f, 0f), Locale.General.NPC.TypeNames.GetValueOrDefault("groundvm", "null"), 0.75f, 3, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION, Additional.ExtraBlip.Types.Default),
            };
        }
    }
}
