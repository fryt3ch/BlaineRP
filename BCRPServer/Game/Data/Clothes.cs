using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Data
{
    public class Clothes
    {
        public static Dictionary<string, Data> AllClothes = new Dictionary<string, Data>()
        {
            #region Tops Male
            { "top_m_0", new Top(true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14) },
            { "top_m_1", new Top(true, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14) },
            { "top_m_3", new Top(true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14) },
            { "top_m_4", new Top(true, 37, new int[] { 0, 1, 2 }, 14) },
            { "top_m_5", new Top(true, 57, new int[] { 0 }, 4) },
            { "top_m_6", new Top(true, 105, new int[] { 0 }, 0) },
            { "top_m_7", new Top(true, 81, new int[] { 0, 1, 2 }, 0) },
            { "top_m_8", new Top(true, 123, new int[] { 0, 1, 2 }, 0) },
            { "top_m_9", new Top(true, 128, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 0) },
            { "top_m_10", new Top(true, 83, new int[] { 0, 1, 2, 3, 4 }, 0) },
            { "top_m_11", new Top(true, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0) },
            { "top_m_12", new Top(true, 84, new int[] { 0, 1, 2, 3, 4, 5 }, 4) },
            { "top_m_13", new Top(true, 61, new int[] { 0, 1, 2, 3 }, 14) },
            { "top_m_14", new Top(true, 62, new int[] { 0 }, 14) },
            { "top_m_15", new Top(true, 64, new int[] { 0 }, 14) },
            { "top_m_16", new Top(true, 141, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 6) },
            { "top_m_17", new Top(true, 69, new int[] { 0, 1, 2, 3, 4, 5 }, 14, new Data.ExtraData(68, 14)) },
            { "top_m_18", new Top(true, 79, new int[] { 0 }, 6) },
            { "top_m_19", new Top(true, 235, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 0, new Data.ExtraData(236, 0)) },
            { "top_m_20", new Top(true, 86, new int[] { 0, 1, 2, 3, 4 }, 4) },
            { "top_m_22", new Top(true, 88, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, new Data.ExtraData(87, 14)) },
            { "top_m_23", new Top(true, 106, new int[] { 0 }, 14) },
            { "top_m_24", new Top(true, 234, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 11) },
            { "top_m_25", new Top(true, 107, new int[] { 0, 1, 2, 3, 4 }, 4) },
            { "top_m_26", new Top(true, 110, new int[] { 0 }, 4) },
            { "top_m_27", new Top(true, 113, new int[] { 0, 1, 2, 3 }, 6) },
            { "top_m_28", new Top(true, 114, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 14) },
            { "top_m_29", new Top(true, 118, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14) },
            { "top_m_30", new Top(true, 121, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4) },
            { "top_m_31", new Top(true, 122, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 14) },
            { "top_m_32", new Top(true, 126, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 4, new Data.ExtraData(127, 14)) },
            { "top_m_33", new Top(true, 130, new int[] { 0 }, 14) },
            { "top_m_34", new Top(true, 164, new int[] { 0, 1, 2 }, 0) },
            { "top_m_35", new Top(true, 136, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 14) },
            { "top_m_36", new Top(true, 137, new int[] { 0, 1, 2 }, 15) },
            { "top_m_37", new Top(true, 143, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 4) },
            { "top_m_38", new Top(true, 147, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 4) },
            { "top_m_39", new Top(true, 148, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4) },
            { "top_m_40", new Top(true, 149, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14) },
            { "top_m_41", new Top(true, 150, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4) },
            { "top_m_42", new Top(true, 151, new int[] { 0, 1, 2, 3, 4, 5 }, 14) },
            { "top_m_43", new Top(true, 153, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4) },
            { "top_m_44", new Top(true, 154, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 12) },
            { "top_m_45", new Top(true, 157, new int[] { 0, 1, 2, 3 }, 112) },
            { "top_m_46", new Top(true, 160, new int[] { 0, 1 }, 112) },
            { "top_m_47", new Top(true, 162, new int[] { 0, 1, 2, 3 }, 114) },
            { "top_m_48", new Top(true, 163, new int[] { 0 }, 14) },
            { "top_m_49", new Top(true, 166, new int[] { 0, 1, 2, 3, 4, 5 }, 14) },
            { "top_m_50", new Top(true, 167, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14) },
            { "top_m_51", new Top(true, 169, new int[] { 0, 1, 2, 3 }, 14) },
            { "top_m_52", new Top(true, 170, new int[] { 0, 1, 2, 3 }, 112) },
            { "top_m_53", new Top(true, 172, new int[] { 0, 1, 2, 3 }, 14) },
            { "top_m_54", new Top(true, 173, new int[] { 0, 1, 2, 3 }, 112) },
            { "top_m_55", new Top(true, 174, new int[] { 0, 1, 2, 3 }, 6) },
            { "top_m_56", new Top(true, 175, new int[] { 0, 1, 2, 3 }, 114) },
            { "top_m_57", new Top(true, 184, new int[] { 0, 1, 2, 3 }, 6, new Data.ExtraData(185, 14)) },
            { "top_m_58", new Top(true, 187, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 6, new Data.ExtraData(204, 6)) },
            { "top_m_59", new Top(true, 200, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new Data.ExtraData(203, 4)) },
            { "top_m_60", new Top(true, 205, new int[] { 0, 1, 2, 3, 4 }, 114, new Data.ExtraData(202, 114)) },
            { "top_m_61", new Top(true, 230, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, new Data.ExtraData(229, 14)) },
            { "top_m_62", new Top(true, 251, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new Data.ExtraData(253, 4)) },
            { "top_m_63", new Top(true, 193, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 0) },
            { "top_m_64", new Top(true, 258, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 6) },
            { "top_m_65", new Top(true, 261, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14) },
            { "top_m_66", new Top(true, 255, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4) },
            { "top_m_67", new Top(true, 257, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 6) },
            { "top_m_68", new Top(true, 259, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 6) },
            { "top_m_69", new Top(true, 262, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 4, new Data.ExtraData(263, 4)) },
            { "top_m_70", new Top(true, 264, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 6) },
            { "top_m_71", new Top(true, 271, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 0) },
            { "top_m_72", new Top(true, 273, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 }, 0) },
            { "top_m_73", new Top(true, 279, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 4, new Data.ExtraData(280, 4)) },
            { "top_m_74", new Top(true, 281, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 6) },
            { "top_m_75", new Top(true, 282, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0) },
            { "top_m_76", new Top(true, 296, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new Data.ExtraData(297, 4)) },
            { "top_m_77", new Top(true, 308, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4) },
            { "top_m_78", new Top(true, 313, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 0) },
            { "top_m_79", new Top(true, 325, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 0) },
            { "top_m_80", new Top(true, 323, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 0) },
            { "top_m_81", new Top(true, 50, new int[] { 0, 1, 2, 3, 4 }, 4) },
            { "top_m_82", new Top(true, 59, new int[] { 0, 1, 2, 3 }, 14) },
            { "top_m_83", new Top(true, 67, new int[] { 0, 1, 2, 3 }, 4) },
            { "top_m_84", new Top(true, 85, new int[] { 0 }, 1) },
            { "top_m_85", new Top(true, 89, new int[] { 0, 1, 2, 3 }, 6) },
            { "top_m_86", new Top(true, 109, new int[] { 0 }, 15) },
            { "top_m_87", new Top(true, 111, new int[] { 0, 1, 2, 3, 4, 5 }, 4) },
            { "top_m_88", new Top(true, 124, new int[] { 0 }, 14) },
            { "top_m_89", new Top(true, 125, new int[] { 0 }, 4) },
            { "top_m_90", new Top(true, 131, new int[] { 0 }, 0, new Data.ExtraData(132, 0)) },
            { "top_m_92", new Top(true, 134, new int[] { 0, 1, 2 }, 4) },
            { "top_m_93", new Top(true, 135, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 0) },
            { "top_m_94", new Top(true, 138, new int[] { 0, 1, 2 }, 4) },
            { "top_m_97", new Top(true, 155, new int[] { 0, 1, 2, 3 }, 14) },
            { "top_m_98", new Top(true, 158, new int[] { 0, 1, 2 }, 113) },
            { "top_m_99", new Top(true, 159, new int[] { 0, 1 }, 114) },
            { "top_m_100", new Top(true, 165, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 6) },
            { "top_m_101", new Top(true, 168, new int[] { 0, 1, 2 }, 12) },
            { "top_m_102", new Top(true, 171, new int[] { 0, 1 }, 4) },
            { "top_m_103", new Top(true, 176, new int[] { 0 }, 114) },
            { "top_m_104", new Top(true, 177, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 2) },
            { "top_m_105", new Top(true, 181, new int[] { 0, 1, 2, 3, 4, 5 }, 14) },
            { "top_m_106", new Top(true, 189, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 14, new Data.ExtraData(188, 14)) },
            { "top_m_107", new Top(true, 190, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 6) },
            { "top_m_108", new Top(true, 223, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 2) },
            { "top_m_109", new Top(true, 224, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 12) },
            { "top_m_110", new Top(true, 225, new int[] { 0, 1 }, 8) },
            { "top_m_112", new Top(true, 237, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 5) },
            { "top_m_113", new Top(true, 238, new int[] { 0, 1, 2, 3, 4, 5 }, 2) },
            { "top_m_114", new Top(true, 241, new int[] { 0, 1, 2, 3, 4, 5 }, 0, new Data.ExtraData(242, 0)) },
            { "top_m_116", new Top(true, 329, new int[] { 0 }, 4) },
            { "top_m_117", new Top(true, 330, new int[] { 0 }, 4, new Data.ExtraData(331, 4)) },
            { "top_m_118", new Top(true, 332, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4) },
            { "top_m_119", new Top(true, 334, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 0) },
            { "top_m_120", new Top(true, 335, new int[] { 0, 1, 2, 3, 4, 5 }, 8) },
            { "top_m_121", new Top(true, 340, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 14, new Data.ExtraData(341, 1)) },
            { "top_m_122", new Top(true, 342, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 4) },
            { "top_m_123", new Top(true, 344, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 14, new Data.ExtraData(343, 14)) },
            { "top_m_124", new Top(true, 350, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 0) },
            { "top_m_126", new Top(true, 217, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 6) },
            { "top_m_127", new Top(true, 233, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14, new Data.ExtraData(232, 14)) },
            { "top_m_128", new Top(true, 351, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 0) },
            { "top_m_129", new Top(true, 352, new int[] { 0, 1, 2 }, 4, new Data.ExtraData(353, 4)) },
            { "top_m_130", new Top(true, 357, new int[] { 0, 1 }, 2) },
            { "top_m_132", new Top(true, 382, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0, new Data.ExtraData(383, 0)) },
            { "top_m_133", new Top(true, 384, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new Data.ExtraData(385, 4)) },
            { "top_m_134", new Top(true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14, new Data.ExtraData(10, 14)) },
            { "top_m_135", new Top(true, 119, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14) },
            { "top_m_136", new Top(true, 21, new int[] { 0, 1, 2, 3 }, 15) },
            { "top_m_137", new Top(true, 25, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 15) },
            { "top_m_138", new Top(true, 24, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 14) },
            { "top_m_139", new Top(true, 27, new int[] { 0, 1, 2 }, 14) },
            { "top_m_140", new Top(true, 28, new int[] { 0, 1, 2 }, 14) },
            { "top_m_141", new Top(true, 35, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 14) },
            { "top_m_142", new Top(true, 45, new int[] { 0, 1, 2 }, 15) },
            { "top_m_143", new Top(true, 46, new int[] { 0, 1, 2 }, 14) },
            { "top_m_144", new Top(true, 58, new int[] { 0 }, 14) },
            { "top_m_145", new Top(true, 70, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14) },
            { "top_m_146", new Top(true, 72, new int[] { 0, 1, 2, 3 }, 14) },
            { "top_m_147", new Top(true, 74, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 14, new Data.ExtraData(75, 14)) },
            { "top_m_148", new Top(true, 76, new int[] { 0, 1, 2, 3, 4 }, 14) },
            { "top_m_149", new Top(true, 77, new int[] { 0, 1, 2, 3 }, 14) },
            { "top_m_150", new Top(true, 99, new int[] { 0, 1, 2, 3, 4 }, 14, new Data.ExtraData(100, 14)) },
            { "top_m_151", new Top(true, 108, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 14) },
            { "top_m_152", new Top(true, 142, new int[] { 0, 1, 2 }, 14) },
            { "top_m_153", new Top(true, 140, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 14) },
            { "top_m_154", new Top(true, 145, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 14) },
            { "top_m_155", new Top(true, 183, new int[] { 0, 1, 2, 3, 4, 5 }, 14) },
            { "top_m_156", new Top(true, 191, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14) },
            { "top_m_157", new Top(true, 192, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14) },
            { "top_m_158", new Top(true, 269, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14) },
            { "top_m_159", new Top(true, 266, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 14, new Data.ExtraData(265, 14)) },
            { "top_m_160", new Top(true, 260, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 11) },
            { "top_m_161", new Top(true, 298, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, 4) },
            { "top_m_162", new Top(true, 299, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 0) },
            { "top_m_163", new Top(true, 303, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14, new Data.ExtraData(300, 6)) },
            { "top_m_164", new Top(true, 301, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14, new Data.ExtraData(302, 14)) },
            { "top_m_165", new Top(true, 304, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14) },
            { "top_m_166", new Top(true, 305, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new Data.ExtraData(306, 4)) },
            { "top_m_167", new Top(true, 307, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 6) },
            { "top_m_168", new Top(true, 309, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 14) },
            { "top_m_170", new Top(true, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 15) },
            { "top_m_171", new Top(true, 20, new int[] { 0, 1, 2, 3 }, 14) },
            { "top_m_172", new Top(true, 23, new int[] { 0, 1, 2, 3 }, 14) },
            { "top_m_173", new Top(true, 40, new int[] { 0, 1 }, 15) },
            { "top_m_174", new Top(true, 103, new int[] { 0 }, 14) },
            { "top_m_175", new Top(true, 112, new int[] { 0 }, 14) },
            { "top_m_176", new Top(true, 115, new int[] { 0 }, 14) },
            { "top_m_177", new Top(true, 120, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 15) },
            { "top_m_178", new Top(true, 161, new int[] { 0, 1, 2, 3 }, 6) },
            { "top_m_179", new Top(true, 240, new int[] { 0, 1, 2, 3, 4, 5 }, 14) },
            { "top_m_180", new Top(true, 310, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 14) },
            { "top_m_181", new Top(true, 338, new int[] { 0, 1, 2, 3, 4, 5 }, 14) },
            { "top_m_182", new Top(true, 348, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, 1, new Data.ExtraData(349, 1)) },
            { "top_m_183", new Top(true, 78, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 6) },
            { "top_m_184", new Top(true, 355, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 184, new Data.ExtraData(354, 184)) },
            { "top_m_185", new Top(true, 358, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 6) },
            { "top_m_186", new Top(true, 360, new int[] { 0 }, 14, new Data.ExtraData(359, 14)) },
            { "top_m_187", new Top(true, 361, new int[] { 0 }, 4) },
            { "top_m_188", new Top(true, 369, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 2) },
            { "top_m_189", new Top(true, 370, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 12) },
            { "top_m_190", new Top(true, 371, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 4) },
            { "top_m_191", new Top(true, 374, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 4, new Data.ExtraData(373, 4)) },
            { "top_m_192", new Top(true, 376, new int[] { 0, 1, 2 }, 14, new Data.ExtraData(375, 14)) },
            { "top_m_193", new Top(true, 377, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 0) },
            { "top_m_194", new Top(true, 378, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 12) },
            { "top_m_195", new Top(true, 381, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, new Data.ExtraData(379, 14)) },
            { "top_m_196", new Top(true, 387, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 14, new Data.ExtraData(386, 14)) },
            { "top_m_197", new Top(true, 390, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14, new Data.ExtraData(388, 14)) },
            { "top_m_198", new Top(true, 391, new int[] { 0, 1, 2 }, 14, new Data.ExtraData(389, 14)) },

            { "top_m_199", new Top(true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 14, new Data.ExtraData(30, 14)) },
            { "top_m_200", new Top(true, 31, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 14, new Data.ExtraData(32, 14)) },
            #endregion

            #region Tops Female
            { "top_f_1", new Top(false, 161, new int[] { 0, 1, 2 }, 9) },
            { "top_f_2", new Top(false, 16, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 4) },
            { "top_f_3", new Top(false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5) },
            { "top_f_4", new Top(false, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5) },
            { "top_f_5", new Top(false, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 7) },
            { "top_f_6", new Top(false, 17, new int[] { 0 }, 9) },
            { "top_f_8", new Top(false, 31, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 5) },
            { "top_f_10", new Top(false, 33, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 4) },
            { "top_f_11", new Top(false, 37, new int[] { 0, 1, 2, 3, 4, 5 }, 4) },
            { "top_f_12", new Top(false, 43, new int[] { 0, 1, 2, 3, 4 }, 3) },
            { "top_f_13", new Top(false, 50, new int[] { 0 }, 3) },
            { "top_f_14", new Top(false, 54, new int[] { 0, 1, 2, 3 }, 3) },
            { "top_f_15", new Top(false, 55, new int[] { 0 }, 3) },
            { "top_f_16", new Top(false, 63, new int[] { 0, 1, 2, 3, 4, 5 }, 3) },
            { "top_f_17", new Top(false, 69, new int[] { 0 }, 5) },
            { "top_f_18", new Top(false, 71, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 1) },
            { "top_f_19", new Top(false, 72, new int[] { 0 }, 1) },
            { "top_f_20", new Top(false, 76, new int[] { 0, 1, 2, 3, 4 }, 9) },
            { "top_f_21", new Top(false, 78, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 3) },
            { "top_f_22", new Top(false, 79, new int[] { 0, 1, 2, 3 }, 1) },
            { "top_f_23", new Top(false, 81, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3) },
            { "top_f_24", new Top(false, 86, new int[] { 0, 1, 2 }, 9) },
            { "top_f_25", new Top(false, 96, new int[] { 0 }, 9) },
            { "top_f_26", new Top(false, 97, new int[] { 0 }, 6) },
            { "top_f_27", new Top(false, 98, new int[] { 0, 1, 2, 3, 4 }, 3) },
            { "top_f_28", new Top(false, 121, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, 3) },
            { "top_f_29", new Top(false, 100, new int[] { 0 }, 6) },
            { "top_f_30", new Top(false, 105, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0) },
            { "top_f_31", new Top(false, 106, new int[] { 0, 1, 2, 3 }, 6) },
            { "top_f_32", new Top(false, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0) },
            { "top_f_33", new Top(false, 110, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3) },
            { "top_f_34", new Top(false, 116, new int[] { 0, 1, 2 }, 11) },
            { "top_f_35", new Top(false, 119, new int[] { 0, 1, 2 }, 14) },
            { "top_f_36", new Top(false, 125, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14) },
            { "top_f_37", new Top(false, 127, new int[] { 0 }, 3) },
            { "top_f_38", new Top(false, 132, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 9) },
            { "top_f_39", new Top(false, 133, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 6) },
            { "top_f_40", new Top(false, 138, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 6) },
            { "top_f_41", new Top(false, 140, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3) },
            { "top_f_42", new Top(false, 144, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3) },
            { "top_f_43", new Top(false, 123, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3) },
            { "top_f_44", new Top(false, 145, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3) },
            { "top_f_45", new Top(false, 146, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 7) },
            { "top_f_46", new Top(false, 147, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3) },
            { "top_f_47", new Top(false, 150, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_48", new Top(false, 151, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 1) },
            { "top_f_49", new Top(false, 154, new int[] { 0, 1, 2, 3 }, 129) },
            { "top_f_50", new Top(false, 157, new int[] { 0, 1 }, 132) },
            { "top_f_51", new Top(false, 159, new int[] { 0, 1, 2, 3 }, 131) },
            { "top_f_52", new Top(false, 160, new int[] { 0 }, 5) },
            { "top_f_53", new Top(false, 163, new int[] { 0, 1, 2, 3, 4, 5 }, 5) },
            { "top_f_54", new Top(false, 164, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5) },
            { "top_f_55", new Top(false, 166, new int[] { 0, 1, 2, 3 }, 5) },
            { "top_f_56", new Top(false, 167, new int[] { 0, 1, 2, 3 }, 129) },
            { "top_f_57", new Top(false, 171, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 153) },
            { "top_f_58", new Top(false, 174, new int[] { 0, 1, 2, 3 }, 5) },
            { "top_f_59", new Top(false, 175, new int[] { 0, 1, 2, 3 }, 129) },
            { "top_f_60", new Top(false, 158, new int[] { 0, 1, 2, 3 }, 7) },
            { "top_f_61", new Top(false, 176, new int[] { 0, 1, 2, 3 }, 7) },
            { "top_f_62", new Top(false, 177, new int[] { 0, 1, 2, 3 }, 131) },
            { "top_f_63", new Top(false, 186, new int[] { 0, 1, 2, 3 }, 3) },
            { "top_f_64", new Top(false, 192, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 1) },
            { "top_f_65", new Top(false, 195, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 153) },
            { "top_f_66", new Top(false, 202, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_68", new Top(false, 233, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 11) },
            { "top_f_69", new Top(false, 234, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 6) },
            { "top_f_70", new Top(false, 251, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_71", new Top(false, 239, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3) },
            { "top_f_72", new Top(false, 259, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_73", new Top(false, 262, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 7) },
            { "top_f_74", new Top(false, 270, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5) },
            { "top_f_75", new Top(false, 267, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 1) },
            { "top_f_76", new Top(false, 266, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 6) },
            { "top_f_77", new Top(false, 268, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 1) },
            { "top_f_78", new Top(false, 271, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 3) },
            { "top_f_79", new Top(false, 280, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 14) },
            { "top_f_80", new Top(false, 286, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 }, 14) },
            { "top_f_81", new Top(false, 292, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 3) },
            { "top_f_82", new Top(false, 294, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 1) },
            { "top_f_83", new Top(false, 295, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14) },
            { "top_f_84", new Top(false, 319, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3) },
            { "top_f_85", new Top(false, 321, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0) },
            { "top_f_86", new Top(false, 324, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 14) },
            { "top_f_87", new Top(false, 338, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14) },
            { "top_f_88", new Top(false, 337, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 14) },
            { "top_f_89", new Top(false, 335, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14) },
            { "top_f_90", new Top(false, 273, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5) },
            { "top_f_91", new Top(false, 284, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 15) },
            { "top_f_92", new Top(false, 264, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_93", new Top(false, 21, new int[] { 0, 1, 2, 3, 4, 5 }, 16) },
            { "top_f_94", new Top(false, 74, new int[] { 0, 1, 2 }, 15) },
            { "top_f_95", new Top(false, 77, new int[] { 0 }, 6) },
            { "top_f_97", new Top(false, 112, new int[] { 0, 1, 2 }, 11) },
            { "top_f_98", new Top(false, 113, new int[] { 0, 1, 2 }, 11) },
            { "top_f_99", new Top(false, 114, new int[] { 0, 1, 2 }, 11) },
            { "top_f_100", new Top(false, 126, new int[] { 0, 1, 2 }, 14) },
            { "top_f_101", new Top(false, 131, new int[] { 0, 1, 2 }, 3) },
            { "top_f_102", new Top(false, 128, new int[] { 0 }, 14) },
            { "top_f_103", new Top(false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5) },
            { "top_f_104", new Top(false, 148, new int[] { 0, 1, 2, 3, 4, 5 }, 3) },
            { "top_f_105", new Top(false, 155, new int[] { 0, 1, 2 }, 130) },
            { "top_f_106", new Top(false, 156, new int[] { 0, 1 }, 131) },
            { "top_f_107", new Top(false, 165, new int[] { 0, 1, 2 }, 0) },
            { "top_f_108", new Top(false, 168, new int[] { 0, 1, 2, 3, 4, 5 }, 161) },
            { "top_f_109", new Top(false, 169, new int[] { 0, 1, 2, 3, 4, 5 }, 153) },
            { "top_f_110", new Top(false, 170, new int[] { 0, 1, 2, 3, 4, 5 }, 15) },
            { "top_f_111", new Top(false, 172, new int[] { 0, 1 }, 3) },
            { "top_f_112", new Top(false, 178, new int[] { 0 }, 131) },
            { "top_f_113", new Top(false, 190, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 3) },
            { "top_f_114", new Top(false, 189, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 7) },
            { "top_f_115", new Top(false, 207, new int[] { 0, 1, 2, 3, 4 }, 131) },
            { "top_f_117", new Top(false, 227, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 3) },
            { "top_f_118", new Top(false, 235, new int[] { 0, 1 }, 9) },
            { "top_f_119", new Top(false, 244, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 9) },
            { "top_f_120", new Top(false, 246, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14) },
            { "top_f_121", new Top(false, 247, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4) },
            { "top_f_122", new Top(false, 249, new int[] { 0, 1, 2, 3, 4, 5 }, 14) },
            { "top_f_123", new Top(false, 347, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_124", new Top(false, 349, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 14) },
            { "top_f_125", new Top(false, 350, new int[] { 0, 1, 2, 3, 4, 5 }, 9) },
            { "top_f_126", new Top(false, 354, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, 5) },
            { "top_f_127", new Top(false, 356, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 3) },
            { "top_f_128", new Top(false, 361, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 3) },
            { "top_f_129", new Top(false, 363, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 5) },
            { "top_f_130", new Top(false, 344, new int[] { 0 }, 3) },
            { "top_f_131", new Top(false, 345, new int[] { 0 }, 3) },
            { "top_f_132", new Top(false, 368, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14) },
            { "top_f_133", new Top(false, 369, new int[] { 0, 1, 2, 3, 4 }, 14) },
            { "top_f_134", new Top(false, 370, new int[] { 0, 1, 2 }, 3) },
            { "top_f_135", new Top(false, 377, new int[] { 0, 1, 2, 3, 4, 5 }, 14) },
            { "top_f_137", new Top(false, 400, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 14) },
            { "top_f_138", new Top(false, 407, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_139", new Top(false, 406, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 7) },
            { "top_f_140", new Top(false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5) },
            { "top_f_141", new Top(false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 6) },
            { "top_f_142", new Top(false, 25, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 6) },
            { "top_f_143", new Top(false, 18, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 15) },
            { "top_f_144", new Top(false, 279, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 15) },
            { "top_f_145", new Top(false, 24, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5) },
            { "top_f_146", new Top(false, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0) },
            { "top_f_147", new Top(false, 35, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5) },
            { "top_f_148", new Top(false, 39, new int[] { 0 }, 5) },
            { "top_f_149", new Top(false, 51, new int[] { 0 }, 3) },
            { "top_f_150", new Top(false, 52, new int[] { 0, 1, 2, 3 }, 3) },
            { "top_f_151", new Top(false, 64, new int[] { 0, 1, 2, 3, 4 }, 5) },
            { "top_f_152", new Top(false, 65, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5) },
            { "top_f_153", new Top(false, 66, new int[] { 0, 1, 2, 3 }, 5) },
            { "top_f_154", new Top(false, 70, new int[] { 0, 1, 2, 3, 4 }, 5) },
            { "top_f_155", new Top(false, 58, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 3) },
            { "top_f_156", new Top(false, 83, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 0) },
            { "top_f_157", new Top(false, 90, new int[] { 0, 1, 2, 3, 4 }, 3) },
            { "top_f_158", new Top(false, 99, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 7) },
            { "top_f_159", new Top(false, 173, new int[] { 0 }, 4) },
            { "top_f_160", new Top(false, 101, new int[] { 0, 1, 2, 3, 4, 5 }, 15) },
            { "top_f_161", new Top(false, 185, new int[] { 0, 1, 2, 3, 4, 5 }, 3) },
            { "top_f_162", new Top(false, 193, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 5) },
            { "top_f_163", new Top(false, 194, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 6) },
            { "top_f_164", new Top(false, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 9) },
            { "top_f_165", new Top(false, 34, new int[] { 0 }, 6) },
            { "top_f_166", new Top(false, 52, new int[] { 0, 1, 2, 3 }, 3) },
            { "top_f_167", new Top(false, 57, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 3) },
            { "top_f_168", new Top(false, 102, new int[] { 0 }, 3) },
            { "top_f_169", new Top(false, 137, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 7) },
            { "top_f_170", new Top(false, 139, new int[] { 0, 1, 2 }, 6) },
            { "top_f_171", new Top(false, 142, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 0) },
            { "top_f_172", new Top(false, 143, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 7) },
            { "top_f_173", new Top(false, 65, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5) },
            { "top_f_174", new Top(false, 305, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3) },
            { "top_f_175", new Top(false, 307, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_176", new Top(false, 318, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 1) },
            { "top_f_177", new Top(false, 353, new int[] { 0, 1, 2, 3, 4, 5 }, 3) },
            { "top_f_178", new Top(false, 366, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, 3) },
            { "top_f_179", new Top(false, 278, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5) },
            { "top_f_180", new Top(false, 274, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 3) },
            { "top_f_181", new Top(false, 269, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 9) },
            { "top_f_182", new Top(false, 309, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, 3) },
            { "top_f_183", new Top(false, 310, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 9) },
            { "top_f_184", new Top(false, 311, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_185", new Top(false, 314, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 5) },
            { "top_f_186", new Top(false, 315, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 5) },
            { "top_f_187", new Top(false, 316, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_188", new Top(false, 320, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 5) },
            { "top_f_189", new Top(false, 332, new int[] { 0 }, 3) },
            { "top_f_191", new Top(false, 322, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 11) },
            { "top_f_192", new Top(false, 323, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 11) },
            { "top_f_193", new Top(false, 339, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 3) },
            { "top_f_194", new Top(false, 373, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 229) },
            { "top_f_195", new Top(false, 376, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 1) },
            { "top_f_196", new Top(false, 379, new int[] { 0 }, 5) },
            { "top_f_197", new Top(false, 380, new int[] { 0 }, 3) },
            { "top_f_198", new Top(false, 388, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 11) },
            { "top_f_199", new Top(false, 389, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 6) },
            { "top_f_200", new Top(false, 390, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 3) },
            { "top_f_201", new Top(false, 392, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 3) },
            { "top_f_202", new Top(false, 394, new int[] { 0, 1, 2 }, 3) },
            { "top_f_203", new Top(false, 395, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 14) },
            { "top_f_204", new Top(false, 396, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 1) },
            { "top_f_205", new Top(false, 399, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5) },
            { "top_f_206", new Top(false, 403, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 5) },
            { "top_f_207", new Top(false, 411, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 5) },
            { "top_f_208", new Top(false, 412, new int[] { 0, 1, 2 }, 5) },
            { "top_f_209", new Top(false, 404, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 12) },
            { "top_f_210", new Top(false, 405, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 12) },
            #endregion

            #region Unders Male
            { "under_m_0", new Under(true, 0, new int[] { 0, 1, 2, 3, 4, 5, 7, 8, 11 }, new Top(true, 0, new int[] { 0, 1, 2, 3, 4, 5, 7, 8, 11 }, 0), 4, new Data.ExtraData(2, 4)) },
            { "under_m_1", new Under(true, 5, new int[] { 0, 1, 2, 7 }, new Top(true, 5, new int[] { 0, 1, 2, 7 }, 5), 6) },
            { "under_m_2", new Under(true, 1, new int[] { 0, 1, 3, 4, 5, 6, 7, 8, 11, 12, 14 }, new Top(true, 1, new int[] { 0, 1, 3, 4, 5, 6, 7, 8, 11, 12, 14 }, 0), 1, new Data.ExtraData(14, 1)) },
            { "under_m_3", new Under(true, 8, new int[] { 0, 10, 13, 14 }, new Top(true, 8, new int[] { 0, 10, 13, 14 }, 11), 4) },
            { "under_m_4", new Under(true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top(true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0), 1) },
            { "under_m_5", new Under(true, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Top(true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 1), 1, new Data.ExtraData(64, 1)) },
            { "under_m_6", new Under(true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top(true, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 1), 4, new Data.ExtraData(30, 4)) },
            { "under_m_7", new Under(true, 17, new int[] { 0, 1, 2, 3, 4, 5 }, new Top(true, 17, new int[] { 0, 1, 2, 3, 4, 5 }, 5), 6) },
            { "under_m_8", new Under(true, 41, new int[] { 0, 1, 2, 3, 4 }, new Top(true, 38, new int[] { 0, 1, 2, 3, 4 }, 8), 4) },
            { "under_m_9", new Under(true, 42, new int[] { 0, 1 }, new Top(true, 39, new int[] { 0, 1 }, 0), 1) },
            { "under_m_10", new Under(true, 43, new int[] { 0, 1, 2, 3 }, new Top(true, 41, new int[] { 0, 1, 2, 3 }, 12), 1) },
            { "under_m_11", new Under(true, 45, new int[] { 0 }, new Top(true, 42, new int[] { 0 }, 11, new Data.ExtraData(43, 11)), 4, new Data.ExtraData(46, 4)) },
            { "under_m_12", new Under(true, 53, new int[] { 0, 1, 2, 3 }, new Top(true, 47, new int[] { 0, 1, 2, 3 }, 0), 4, new Data.ExtraData(54, 4)) },
            { "under_m_13", new Under(true, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null, 11, new Data.ExtraData(7, 11)) },
            { "under_m_14", new Under(true, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new Top(true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 11), 4) },
            { "under_m_15", new Under(true, 67, new int[] { 0 }, new Top(true, 71, new int[] { 0 }, 0), 4) },
            { "under_m_16", new Under(true, 65, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }, new Top(true, 73, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }, 0), 4) },
            { "under_m_17", new Under(true, 4, new int[] { 0, 1, 2 }, null, 4, new Data.ExtraData(3, 4)) }, // -
            { "under_m_18", new Under(true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null, 4, new Data.ExtraData(11, 4)) }, // - 
            { "under_m_19", new Under(true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, null, 4, new Data.ExtraData(25, 4)) }, // -
            { "under_m_20", new Under(true, 33, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null, 4, new Data.ExtraData(34, 4)) }, // -
            { "under_m_21", new Under(true, 52, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, null, 4, new Data.ExtraData(51, 4)) },
            { "under_m_22", new Under(true, 69, new int[] { 0, 1, 2, 3, 4 }, null, 14) }, // -
            { "under_m_24", new Under(true, 22, new int[] { 0, 1, 2, 3, 4 }, null, 4) }, // -
            { "under_m_25", new Under(true, 93, new int[] { 0, 1 }, null, 11) }, // -
            { "under_m_26", new Under(true, 158, new int[] { 0 }, new Top(true, 322, new int[] { 0 }, 1, new Data.ExtraData(321, 1)), 4, new Data.ExtraData(157, 4)) },

            { "under_m_27", new Under(true, 79, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top(true, 152, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 1), 4, new Data.ExtraData(80, 4)) },
            { "under_m_28", new Under(true, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Top(true, 235, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 0), 1, new Data.ExtraData(110, 1)) },
            { "under_m_29", new Under(true, 187, new int[] { 0, 1, 2, 3, 4 }, new Top(true, 392, new int[] { 0, 1, 2, 3, 4 }, 0), 4, new Data.ExtraData(188, 4)) },
            { "under_m_30", new Under(true, 168, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, new Top(true, 345, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0), 4, new Data.ExtraData(169, 4)) },
            { "under_m_31", new Under(true, 47, new int[] { 0, 1, 2, 3 }, new Top(true, 44, new int[] { 0, 1, 2, 3 }, 0), 4, new Data.ExtraData(48, 4)) },
            { "under_m_32", new Under(true, 16, new int[] { 0, 1, 2 }, new Top(true, 16, new int[] { 0, 1, 2 }, 0), 1, new Data.ExtraData(18, 1)) },
            { "under_m_33", new Under(true, 72, new int[] { 0, 1, 2, 3, 4, 5 }, new Top(true, 139, new int[] { 0, 1, 2, 3, 4, 5 }, 4), 4, new Data.ExtraData(71, 4)) },
            { "under_m_34", new Under(true, 76, new int[] { 0, 1, 2, 3, 4, 5, 7, 8 }, new Top(true, 146, new int[] { 0, 1, 2, 3, 4, 5, 7, 8 }, 0), 4, new Data.ExtraData(77, 4)) },
            { "under_m_35", new Under(true, 178, new int[] { 0, 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, null, 4, new Data.ExtraData(179, 4)) },
            #endregion

            #region Unders Female
            { "under_f_0", new Under(false, 23, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, new Top(false, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 12), 0) }, // 26
            { "under_f_1", new Under(false, 26, new int[] { 0, 1, 2 }, new Top(false, 30, new int[] { 0, 1, 2 }, 2), 0) }, // 30
            { "under_f_2", new Under(false, 27, new int[] { 0, 1, 2 }, new Top(false, 32, new int[] { 0, 1, 2 }, 4), 0) }, // 32
            { "under_f_3", new Under(false, 71, new int[] { 0, 1, 2 }, new Top(false, 73, new int[] { 0, 1, 2 }, 14), 0) }, // 73
            { "under_f_4", new Under(false, 31, new int[] { 0, 1 }, new Top(false, 40, new int[] { 0, 1 }, 2), 0) }, // 40
            { "under_f_5", new Under(false, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top(false, 149, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 1), 0) }, // 149
            { "under_f_6", new Under(false, 61, new int[] { 0, 1, 2, 3 }, new Top(false, 75, new int[] { 0, 1, 2, 3 }, 9), 0) }, // 75
            { "under_f_7", new Under(false, 67, new int[] { 0, 1, 2, 3, 4, 5 }, new Top(false, 103, new int[] { 0, 1, 2, 3, 4, 5 }, 3), 0) }, // 103
            { "under_f_8", new Under(false, 78, new int[] { 0, 1, 2, 3, 4, 5 }, new Top(false, 141, new int[] { 0, 1, 2, 3, 4, 5 }, 14), 0) }, // 141
            { "under_f_9", new Under(false, 147, new int[] { 0 }, new Top(false, 236, new int[] { 0 }, 14), 0) }, // 236
            { "under_f_10", new Under(false, 22, new int[] { 0, 1, 2, 3, 4 }, new Top(false, 22, new int[] { 0, 1, 2, 3, 4 }, 4), 0) }, // 22
            { "under_f_11", new Under(false, 29, new int[] { 0, 1, 2, 3, 4 }, new Top(false, 36, new int[] { 0, 1, 2, 3, 4 }, 11), 0) }, // 36
            { "under_f_12", new Under(false, 45, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, new Top(false, 68, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, 14), 0) }, // 68
            { "under_f_13", new Under(false, 49, new int[] { 0 }, new Top(false, 67, new int[] { 0 }, 2), 0) }, // 67
            { "under_f_14", new Under(false, 117, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, new Top(false, 209, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, 12), 0) }, // 209
            { "under_f_15", new Under(false, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top(false, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 4), 0) }, // 13
            { "under_f_16", new Under(false, 38, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null, 0) }, // -
            { "under_f_17", new Under(false, 40, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, 0) },
            { "under_f_18", new Under(false, 68, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Top(false, 111, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4), 0) }, // 111
            { "under_f_19", new Under(false, 176, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Top(false, 283, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 12), 0) }, // 283

            { "under_f_20", new Under(false, 11, new int[] { 0, 1, 2, 10, 11, 15 }, new Top(false, 11, new int[] { 0, 1, 2, 10, 11, 15 }, 11), 0) }, // 11
            { "under_f_21", new Under(false, 24, new int[] { 0, 1, 2, 3, 4, 5 }, new Top(false, 27, new int[] { 0, 1, 2, 3, 4, 5 }, 0), 0) }, // 27
            { "under_f_22", new Under(false, 30, new int[] { 0, 1, 2, 3 }, new Top(false, 38, new int[] { 0, 1, 2, 3 }, 2), 0) }, // 38
            { "under_f_23", new Under(false, 227, new int[] { 0, 1, 2, 3, 4 }, new Top(false, 413, new int[] { 0, 1, 2, 3, 4  }, 14), 0) }, // 413
            { "under_f_24", new Under(false, 217, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, null, 0) }, // -
            #endregion

            #region Pants Male
            { "pants_m_0", new Pants(true, 0, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_1", new Pants(true, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_2", new Pants(true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_3", new Pants(true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_4", new Pants(true, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_5", new Pants(true, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_6", new Pants(true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_7", new Pants(true, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_8", new Pants(true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_9", new Pants(true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_10", new Pants(true, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_11", new Pants(true, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_12", new Pants(true, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_m_13", new Pants(true, 17, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "pants_m_14", new Pants(true, 18, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_m_15", new Pants(true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_m_16", new Pants(true, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_m_17", new Pants(true, 29, new int[] { 0, 1, 2 }) },
            { "pants_m_18", new Pants(true, 32, new int[] { 0, 1, 2, 3 }) },
            { "pants_m_19", new Pants(true, 42, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_m_20", new Pants(true, 43, new int[] { 0, 1 }) },
            { "pants_m_21", new Pants(true, 64, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "pants_m_22", new Pants(true, 58, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_23", new Pants(true, 62, new int[] { 0, 1, 2, 3 }) },
            { "pants_m_24", new Pants(true, 68, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "pants_m_25", new Pants(true, 69, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }) },
            { "pants_m_26", new Pants(true, 71, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "pants_m_27", new Pants(true, 73, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "pants_m_28", new Pants(true, 76, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_m_29", new Pants(true, 78, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_m_30", new Pants(true, 83, new int[] { 0, 1, 2, 3 }) },
            { "pants_m_31", new Pants(true, 80, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_m_32", new Pants(true, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "pants_m_33", new Pants(true, 90, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "pants_m_34", new Pants(true, 98, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_m_35", new Pants(true, 100, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "pants_m_36", new Pants(true, 117, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "pants_m_37", new Pants(true, 55, new int[] { 0, 1, 2, 3 }) },
            { "pants_m_38", new Pants(true, 56, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_m_39", new Pants(true, 59, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "pants_m_40", new Pants(true, 63, new int[] { 0 }) },
            { "pants_m_41", new Pants(true, 75, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_m_42", new Pants(true, 81, new int[] { 0, 1, 2 }) },
            { "pants_m_43", new Pants(true, 83, new int[] { 0, 1, 2, 3 }) },
            { "pants_m_44", new Pants(true, 132, new int[] { 0, 1, 2 }) },
            { "pants_m_45", new Pants(true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_46", new Pants(true, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_47", new Pants(true, 20, new int[] { 0, 1, 2, 3 }) },
            { "pants_m_48", new Pants(true, 22, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }) },
            { "pants_m_49", new Pants(true, 23, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }) },
            { "pants_m_50", new Pants(true, 24, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "pants_m_51", new Pants(true, 25, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "pants_m_52", new Pants(true, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_53", new Pants(true, 35, new int[] { 0 }) },
            { "pants_m_54", new Pants(true, 37, new int[] { 0, 1, 2, 3 }) },
            { "pants_m_55", new Pants(true, 44, new int[] { 0 }) },
            { "pants_m_56", new Pants(true, 49, new int[] { 0, 1, 2, 3, 4 }) },
            { "pants_m_57", new Pants(true, 54, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "pants_m_58", new Pants(true, 60, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_m_59", new Pants(true, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "pants_m_60", new Pants(true, 103, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }) },
            { "pants_m_61", new Pants(true, 116, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "pants_m_62", new Pants(true, 19, new int[] { 0, 1 }) },
            { "pants_m_63", new Pants(true, 48, new int[] { 0, 1, 2, 3, 4 }) },
            { "pants_m_64", new Pants(true, 51, new int[] { 0 }) },
            { "pants_m_65", new Pants(true, 52, new int[] { 0, 1, 2, 3 }) },
            { "pants_m_66", new Pants(true, 53, new int[] { 0 }) },
            { "pants_m_67", new Pants(true, 118, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_m_68", new Pants(true, 119, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "pants_m_69", new Pants(true, 131, new int[] { 0 }) },
            { "pants_m_70", new Pants(true, 138, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_m_71", new Pants(true, 139, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_m_72", new Pants(true, 140, new int[] { 0, 1, 2 }) },
            { "pants_m_73", new Pants(true, 141, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_m_74", new Pants(true, 142, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 }) },
            { "pants_m_75", new Pants(true, 143, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            #endregion

            #region Pants Female
            { "pants_f_0", new Pants(false, 0, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_1", new Pants(false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_2", new Pants(false, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_3", new Pants(false, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_4", new Pants(false, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_5", new Pants(false, 18, new int[] { 0, 1 }) },
            { "pants_f_6", new Pants(false, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_7", new Pants(false, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_8", new Pants(false, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_9", new Pants(false, 22, new int[] { 0, 1, 2 }) },
            { "pants_f_10", new Pants(false, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_11", new Pants(false, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_12", new Pants(false, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_13", new Pants(false, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_f_14", new Pants(false, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_15", new Pants(false, 57, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_f_16", new Pants(false, 66, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "pants_f_17", new Pants(false, 60, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_18", new Pants(false, 67, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "pants_f_19", new Pants(false, 43, new int[] { 0, 1, 2, 3, 4 }) },
            { "pants_f_20", new Pants(false, 71, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }) },
            { "pants_f_21", new Pants(false, 73, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "pants_f_22", new Pants(false, 74, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "pants_f_23", new Pants(false, 84, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "pants_f_24", new Pants(false, 87, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_25", new Pants(false, 102, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }) },
            { "pants_f_26", new Pants(false, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }) },
            { "pants_f_27", new Pants(false, 68, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "pants_f_28", new Pants(false, 80, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_f_29", new Pants(false, 81, new int[] { 0, 1, 2 }) },
            { "pants_f_30", new Pants(false, 92, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_f_31", new Pants(false, 20, new int[] { 0, 1, 2 }) },
            { "pants_f_32", new Pants(false, 26, new int[] { 0 }) },
            { "pants_f_33", new Pants(false, 28, new int[] { 0 }) },
            { "pants_f_34", new Pants(false, 30, new int[] { 0, 1, 2, 3, 4 }) },
            { "pants_f_35", new Pants(false, 36, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_36", new Pants(false, 44, new int[] { 0, 1, 2, 3, 4 }) },
            { "pants_f_37", new Pants(false, 45, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_38", new Pants(false, 52, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_39", new Pants(false, 54, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_40", new Pants(false, 58, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_41", new Pants(false, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "pants_f_42", new Pants(false, 64, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_43", new Pants(false, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_f_44", new Pants(false, 83, new int[] { 0, 1, 2 }) },
            { "pants_f_45", new Pants(false, 85, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_46", new Pants(false, 93, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "pants_f_47", new Pants(false, 106, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_f_48", new Pants(false, 139, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_49", new Pants(false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_50", new Pants(false, 19, new int[] { 0, 1, 2, 3, 4 }) },
            { "pants_f_51", new Pants(false, 25, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }) },
            { "pants_f_52", new Pants(false, 24, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }) },
            { "pants_f_53", new Pants(false, 23, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }) },
            { "pants_f_54", new Pants(false, 51, new int[] { 0, 1, 2, 3, 4 }) },
            { "pants_f_55", new Pants(false, 50, new int[] { 0, 1, 2, 3, 4 }) },
            { "pants_f_56", new Pants(false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_57", new Pants(false, 56, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "pants_f_58", new Pants(false, 62, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_f_59", new Pants(false, 37, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "pants_f_60", new Pants(false, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_f_61", new Pants(false, 75, new int[] { 0, 1, 2 }) },
            { "pants_f_62", new Pants(false, 108, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_63", new Pants(false, 78, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_64", new Pants(false, 104, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "pants_f_65", new Pants(false, 112, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_f_66", new Pants(false, 124, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_f_67", new Pants(false, 125, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "pants_f_68", new Pants(false, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_69", new Pants(false, 34, new int[] { 0 }) },
            { "pants_f_70", new Pants(false, 41, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_71", new Pants(false, 49, new int[] { 0, 1 }) },
            { "pants_f_72", new Pants(false, 76, new int[] { 0, 1, 2 }) },
            { "pants_f_73", new Pants(false, 77, new int[] { 0, 1, 2 }) },
            { "pants_f_74", new Pants(false, 107, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_f_75", new Pants(false, 123, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "pants_f_76", new Pants(false, 53, new int[] { 0 }) },
            { "pants_f_77", new Pants(false, 55, new int[] { 0 }) },
            { "pants_f_78", new Pants(false, 138, new int[] { 0 }) },
            { "pants_f_79", new Pants(false, 145, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_f_80", new Pants(false, 146, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_f_81", new Pants(false, 147, new int[] { 0, 1, 2 }) },
            { "pants_f_82", new Pants(false, 148, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_f_83", new Pants(false, 149, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 }) },
            { "pants_f_84", new Pants(false, 150, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            #endregion

            #region Shoes Male
            { "shoes_m_0", new Shoes(true, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_1", new Shoes(true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_2", new Shoes(true, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_3", new Shoes(true, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_4", new Shoes(true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_5", new Shoes(true, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_6", new Shoes(true, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_7", new Shoes(true, 22, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_m_8", new Shoes(true, 24, new int[] { 0 }) },
            { "shoes_m_9", new Shoes(true, 25, new int[] { 0 }) },
            { "shoes_m_10", new Shoes(true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_11", new Shoes(true, 28, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "shoes_m_12", new Shoes(true, 31, new int[] { 0, 1, 2, 3, 4 }) },
            { "shoes_m_13", new Shoes(true, 32, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_14", new Shoes(true, 44, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "shoes_m_15", new Shoes(true, 42, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "shoes_m_16", new Shoes(true, 46, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "shoes_m_17", new Shoes(true, 50, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "shoes_m_18", new Shoes(true, 53, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "shoes_m_19", new Shoes(true, 57, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_m_20", new Shoes(true, 58, new int[] { 0, 1, 2 }) },
            { "shoes_m_21", new Shoes(true, 64, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "shoes_m_22", new Shoes(true, 70, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_m_23", new Shoes(true, 79, new int[] { 0, 1 }) },
            { "shoes_m_24", new Shoes(true, 80, new int[] { 0, 1 }) },
            { "shoes_m_25", new Shoes(true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_26", new Shoes(true, 27, new int[] { 0 }) },
            { "shoes_m_27", new Shoes(true, 85, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_28", new Shoes(true, 86, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_29", new Shoes(true, 83, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }) },
            { "shoes_m_30", new Shoes(true, 37, new int[] { 0, 1, 2, 3, 4 }) },
            { "shoes_m_31", new Shoes(true, 38, new int[] { 0, 1, 2, 3, 4 }) },
            { "shoes_m_32", new Shoes(true, 43, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_m_33", new Shoes(true, 60, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_m_34", new Shoes(true, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_m_35", new Shoes(true, 62, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_m_36", new Shoes(true, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_m_37", new Shoes(true, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_38", new Shoes(true, 35, new int[] { 0, 1 }) },
            { "shoes_m_39", new Shoes(true, 52, new int[] { 0, 1 }) },
            { "shoes_m_40", new Shoes(true, 65, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "shoes_m_41", new Shoes(true, 66, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "shoes_m_42", new Shoes(true, 72, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_m_43", new Shoes(true, 73, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_m_44", new Shoes(true, 74, new int[] { 0, 1 }) },
            { "shoes_m_45", new Shoes(true, 81, new int[] { 0, 1, 2 }) },
            { "shoes_m_46", new Shoes(true, 82, new int[] { 0, 1, 2 }) },
            { "shoes_m_47", new Shoes(true, 88, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_m_48", new Shoes(true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_49", new Shoes(true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_50", new Shoes(true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_51", new Shoes(true, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_m_52", new Shoes(true, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_m_53", new Shoes(true, 23, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_54", new Shoes(true, 36, new int[] { 0, 1, 2, 3 }) },
            { "shoes_m_55", new Shoes(true, 55, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "shoes_m_56", new Shoes(true, 75, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_m_57", new Shoes(true, 76, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_m_58", new Shoes(true, 93, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }) },
            { "shoes_m_59", new Shoes(true, 77, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_m_60", new Shoes(true, 94, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }) },
            { "shoes_m_61", new Shoes(true, 30, new int[] { 0, 1 }) },
            { "shoes_m_62", new Shoes(true, 18, new int[] { 0, 1 }) },
            { "shoes_m_63", new Shoes(true, 40, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_m_64", new Shoes(true, 19, new int[] { 0 }) },
            { "shoes_m_65", new Shoes(true, 29, new int[] { 0 }) },
            { "shoes_m_66", new Shoes(true, 41, new int[] { 0 }) },
            { "shoes_m_67", new Shoes(true, 69, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_m_68", new Shoes(true, 87, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }) },
            { "shoes_m_69", new Shoes(true, 95, new int[] { 0 }) },
            { "shoes_m_70", new Shoes(true, 99, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }) },
            #endregion

            #region Shoes Female
            { "shoes_f_0", new Shoes(false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_1", new Shoes(false, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_2", new Shoes(false, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_3", new Shoes(false, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_4", new Shoes(false, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_5", new Shoes(false, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_6", new Shoes(false, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_7", new Shoes(false, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_8", new Shoes(false, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_9", new Shoes(false, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_10", new Shoes(false, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "shoes_f_11", new Shoes(false, 22, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_12", new Shoes(false, 30, new int[] { 0 }) },
            { "shoes_f_13", new Shoes(false, 33, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_f_14", new Shoes(false, 29, new int[] { 0, 1, 2 }) },
            { "shoes_f_15", new Shoes(false, 32, new int[] { 0, 1, 2, 3, 4 }) },
            { "shoes_f_16", new Shoes(false, 44, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_f_17", new Shoes(false, 60, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_f_18", new Shoes(false, 61, new int[] { 0, 1, 2 }) },
            { "shoes_f_19", new Shoes(false, 67, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "shoes_f_20", new Shoes(false, 77, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }) },
            { "shoes_f_21", new Shoes(false, 85, new int[] { 0, 1, 2 }) },
            { "shoes_f_22", new Shoes(false, 26, new int[] { 0 }) },
            { "shoes_f_23", new Shoes(false, 38, new int[] { 0, 1, 2, 3, 4 }) },
            { "shoes_f_24", new Shoes(false, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_25", new Shoes(false, 45, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "shoes_f_26", new Shoes(false, 39, new int[] { 0, 1, 2, 3, 4 }) },
            { "shoes_f_27", new Shoes(false, 46, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "shoes_f_28", new Shoes(false, 51, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "shoes_f_29", new Shoes(false, 52, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "shoes_f_30", new Shoes(false, 54, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "shoes_f_31", new Shoes(false, 55, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "shoes_f_32", new Shoes(false, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_f_33", new Shoes(false, 64, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_f_34", new Shoes(false, 65, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_f_35", new Shoes(false, 66, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_f_36", new Shoes(false, 83, new int[] { 0, 1 }) },
            { "shoes_f_37", new Shoes(false, 84, new int[] { 0, 1 }) },
            { "shoes_f_38", new Shoes(false, 27, new int[] { 0 }) },
            { "shoes_f_39", new Shoes(false, 28, new int[] { 0 }) },
            { "shoes_f_40", new Shoes(false, 53, new int[] { 0, 1 }) },
            { "shoes_f_41", new Shoes(false, 59, new int[] { 0, 1 }) },
            { "shoes_f_42", new Shoes(false, 87, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }) },
            { "shoes_f_43", new Shoes(false, 89, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_44", new Shoes(false, 90, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_45", new Shoes(false, 75, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_f_46", new Shoes(false, 76, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_f_47", new Shoes(false, 0, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_48", new Shoes(false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_49", new Shoes(false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_50", new Shoes(false, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_51", new Shoes(false, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_52", new Shoes(false, 19, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_f_53", new Shoes(false, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_f_54", new Shoes(false, 68, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "shoes_f_55", new Shoes(false, 37, new int[] { 0, 1, 2, 3 }) },
            { "shoes_f_56", new Shoes(false, 42, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_f_57", new Shoes(false, 31, new int[] { 0 }) },
            { "shoes_f_58", new Shoes(false, 58, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "shoes_f_59", new Shoes(false, 79, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_f_60", new Shoes(false, 80, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_f_61", new Shoes(false, 81, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_f_62", new Shoes(false, 96, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }) },
            { "shoes_f_63", new Shoes(false, 97, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }) },
            { "shoes_f_64", new Shoes(false, 73, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_f_65", new Shoes(false, 74, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_f_66", new Shoes(false, 18, new int[] { 0, 1, 2 }) },
            { "shoes_f_67", new Shoes(false, 56, new int[] { 0, 1, 2 }) },
            { "shoes_f_68", new Shoes(false, 57, new int[] { 0, 1, 2 }) },
            { "shoes_f_69", new Shoes(false, 24, new int[] { 0 }) },
            { "shoes_f_70", new Shoes(false, 25, new int[] { 0 }) },
            { "shoes_f_71", new Shoes(false, 36, new int[] { 0, 1 }) },
            { "shoes_f_72", new Shoes(false, 78, new int[] { 0, 1 }) },
            { "shoes_f_73", new Shoes(false, 47, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "shoes_f_74", new Shoes(false, 91, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }) },
            { "shoes_f_75", new Shoes(false, 92, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_f_76", new Shoes(false, 98, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_f_77", new Shoes(false, 99, new int[] { 0 }) },
            { "shoes_f_78", new Shoes(false, 23, new int[] { 0, 1, 2 }) },
            { "shoes_f_79", new Shoes(false, 103, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }) },
            #endregion

            #region Hats Male
            { "hat_m_0", new Hat(true, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_1", new Hat(true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_2", new Hat(true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_3", new Hat(true, 28, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "hat_m_4", new Hat(true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_5", new Hat(true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_6", new Hat(true, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_7", new Hat(true, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_8", new Hat(true, 20, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "hat_m_9", new Hat(true, 31, new int[] { 0 }) },
            { "hat_m_10", new Hat(true, 32, new int[] { 0 }) },
            { "hat_m_11", new Hat(true, 34, new int[] { 0 }) },
            { "hat_m_12", new Hat(true, 37, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "hat_m_13", new Hat(true, 54, new int[] { 0, 1 }) },
            { "hat_m_14", new Hat(true, 56, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_m_15", new Hat(true, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_m_16", new Hat(true, 65, new int[] { 0 }) },
            { "hat_m_17", new Hat(true, 83, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "hat_m_18", new Hat(true, 94, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_m_19", new Hat(true, 120, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "hat_m_20", new Hat(true, 135, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }) },
            { "hat_m_21", new Hat(true, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_22", new Hat(true, 50, new int[] { 0 }) },
            { "hat_m_23", new Hat(true, 51, new int[] { 0 }) },
            { "hat_m_24", new Hat(true, 73, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }) },
            { "hat_m_25", new Hat(true, 78, new int[] { 0, 1, 2, 3, 4 }) },
            { "hat_m_26", new Hat(true, 80, new int[] { 0, 1, 2, 3 }) },
            { "hat_m_27", new Hat(true, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }) },
            { "hat_m_28", new Hat(true, 85, new int[] { 0 }) },
            { "hat_m_29", new Hat(true, 86, new int[] { 0 }) },
            { "hat_m_30", new Hat(true, 87, new int[] { 0 }) },
            { "hat_m_31", new Hat(true, 88, new int[] { 0 }) },
            { "hat_m_32", new Hat(true, 76, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }) },
            { "hat_m_33", new Hat(true, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "hat_m_34", new Hat(true, 130, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }) },
            { "hat_m_35", new Hat(true, 139, new int[] { 0, 1, 2 }) },
            { "hat_m_36", new Hat(true, 142, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "hat_m_37", new Hat(true, 151, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_38", new Hat(true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_39", new Hat(true, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_40", new Hat(true, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_41", new Hat(true, 25, new int[] { 0, 1, 2 }) },
            { "hat_m_42", new Hat(true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "hat_m_43", new Hat(true, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "hat_m_44", new Hat(true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_45", new Hat(true, 30, new int[] { 0, 1 }) },
            { "hat_m_46", new Hat(true, 33, new int[] { 0, 1 }) },
            { "hat_m_47", new Hat(true, 55, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "hat_m_48", new Hat(true, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_m_49", new Hat(true, 64, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "hat_m_50", new Hat(true, 84, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_m_51", new Hat(true, 89, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_m_52", new Hat(true, 90, new int[] { 0 }) },
            { "hat_m_53", new Hat(true, 96, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "hat_m_54", new Hat(true, 154, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            #endregion

            #region Hats Female
            { "hat_f_0", new Hat(false, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_1", new Hat(false, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_2", new Hat(false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_3", new Hat(false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_4", new Hat(false, 20, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "hat_f_5", new Hat(false, 21, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "hat_f_6", new Hat(false, 22, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "hat_f_7", new Hat(false, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_8", new Hat(false, 30, new int[] { 0 }) },
            { "hat_f_9", new Hat(false, 31, new int[] { 0 }) },
            { "hat_f_10", new Hat(false, 33, new int[] { 0 }) },
            { "hat_f_11", new Hat(false, 36, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "hat_f_12", new Hat(false, 53, new int[] { 0, 1 }) },
            { "hat_f_13", new Hat(false, 56, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_f_14", new Hat(false, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_f_15", new Hat(false, 64, new int[] { 0 }) },
            { "hat_f_16", new Hat(false, 82, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "hat_f_17", new Hat(false, 131, new int[] { 0, 1, 2, 3 }) },
            { "hat_f_18", new Hat(false, 55, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "hat_f_19", new Hat(false, 134, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }) },
            { "hat_f_20", new Hat(false, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_21", new Hat(false, 50, new int[] { 0 }) },
            { "hat_f_22", new Hat(false, 49, new int[] { 0 }) },
            { "hat_f_23", new Hat(false, 72, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }) },
            { "hat_f_24", new Hat(false, 77, new int[] { 0, 1, 2, 3, 4 }) },
            { "hat_f_25", new Hat(false, 79, new int[] { 0, 1, 2, 3 }) },
            { "hat_f_26", new Hat(false, 81, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }) },
            { "hat_f_27", new Hat(false, 84, new int[] { 0 }) },
            { "hat_f_28", new Hat(false, 85, new int[] { 0 }) },
            { "hat_f_29", new Hat(false, 86, new int[] { 0 }) },
            { "hat_f_30", new Hat(false, 87, new int[] { 0 }) },
            { "hat_f_31", new Hat(false, 75, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }) },
            { "hat_f_32", new Hat(false, 108, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "hat_f_33", new Hat(false, 129, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }) },
            { "hat_f_34", new Hat(false, 141, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "hat_f_35", new Hat(false, 93, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_f_36", new Hat(false, 150, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_37", new Hat(false, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_38", new Hat(false, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_39", new Hat(false, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_40", new Hat(false, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "hat_f_41", new Hat(false, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "hat_f_42", new Hat(false, 54, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_43", new Hat(false, 32, new int[] { 0, 1 }) },
            { "hat_f_44", new Hat(false, 95, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "hat_f_45", new Hat(false, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_f_46", new Hat(false, 83, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_f_47", new Hat(false, 88, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_f_48", new Hat(false, 89, new int[] { 0 }) },
            { "hat_f_49", new Hat(false, 153, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            #endregion

            #region Accessories Male
            { "accs_m_0", new Accessory(true, 16, new int[] { 0, 1, 2 }) },
            { "accs_m_1", new Accessory(true, 17, new int[] { 0, 1, 2 }) },
            { "accs_m_2", new Accessory(true, 30, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "accs_m_3", new Accessory(true, 44, new int[] { 0 }) },
            { "accs_m_4", new Accessory(true, 74, new int[] { 0, 1 }) },
            { "accs_m_5", new Accessory(true, 85, new int[] { 0, 1 }) },
            { "accs_m_6", new Accessory(true, 87, new int[] { 0, 1 }) },
            { "accs_m_7", new Accessory(true, 110, new int[] { 0, 1 }) },
            { "accs_m_8", new Accessory(true, 112, new int[] { 0, 1, 2 }) },
            { "accs_m_9", new Accessory(true, 114, new int[] { 0 }) },
            { "accs_m_10", new Accessory(true, 119, new int[] { 0, 1 }) },
            { "accs_m_11", new Accessory(true, 124, new int[] { 0, 1 }) },
            { "accs_m_12", new Accessory(true, 151, new int[] { 0 }) },
            { "accs_m_13", new Accessory(true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_m_14", new Accessory(true, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_m_15", new Accessory(true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_m_16", new Accessory(true, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_m_17", new Accessory(true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_m_18", new Accessory(true, 32, new int[] { 0, 1, 2 }) },
            { "accs_m_19", new Accessory(true, 37, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_m_20", new Accessory(true, 38, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_m_21", new Accessory(true, 39, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_m_22", new Accessory(true, 118, new int[] { 0 }) },
            #endregion

            #region Accessories Female
            { "accs_f_0", new Accessory(false, 83, new int[] { 0, 1, 2 }) },
            { "accs_f_1", new Accessory(false, 9, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "accs_f_2", new Accessory(false, 15, new int[] { 0, 1, 2, 3, 4 }) },
            { "accs_f_3", new Accessory(false, 85, new int[] { 0 }) },
            { "accs_f_4", new Accessory(false, 94, new int[] { 0, 1 }) },
            { "accs_f_5", new Accessory(false, 120, new int[] { 0 }) },
            { "accs_f_6", new Accessory(false, 12, new int[] { 0, 1, 2 }) },
            { "accs_f_7", new Accessory(false, 13, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "accs_f_8", new Accessory(false, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_f_9", new Accessory(false, 21, new int[] { 0, 1, 2 }) },
            { "accs_f_10", new Accessory(false, 22, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_f_11", new Accessory(false, 23, new int[] { 0, 1, 2 }) },
            #endregion

            #region Watches Male
            { "watches_m_0", new Watches(true, 1, new int[] { 0, 1, 2, 3, 4 }) },
            { "watches_m_1", new Watches(true, 3, new int[] { 0, 1, 2, 3, 4 }) },
            { "watches_m_2", new Watches(true, 5, new int[] { 0, 1, 2, 3 }) },
            { "watches_m_3", new Watches(true, 7, new int[] { 0, 1, 2 }) },
            { "watches_m_4", new Watches(true, 10, new int[] { 0, 1, 2 }) },
            { "watches_m_5", new Watches(true, 12, new int[] { 0, 1, 2 }) },
            { "watches_m_6", new Watches(true, 13, new int[] { 0, 1, 2 }) },
            { "watches_m_7", new Watches(true, 14, new int[] { 0, 1, 2 }) },
            { "watches_m_8", new Watches(true, 15, new int[] { 0, 1, 2 }) },
            { "watches_m_9", new Watches(true, 20, new int[] { 0, 1, 2 }) },
            { "watches_m_10", new Watches(true, 21, new int[] { 0, 1, 2 }) },
            { "watches_m_11", new Watches(true, 36, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_m_12", new Watches(true, 0, new int[] { 0, 1, 2, 3, 4 }) },
            { "watches_m_13", new Watches(true, 6, new int[] { 0, 1, 2 }) },
            { "watches_m_14", new Watches(true, 8, new int[] { 0, 1, 2 }) },
            { "watches_m_15", new Watches(true, 9, new int[] { 0, 1, 2 }) },
            { "watches_m_16", new Watches(true, 11, new int[] { 0, 1, 2 }) },
            { "watches_m_17", new Watches(true, 16, new int[] { 0, 1, 2 }) },
            { "watches_m_18", new Watches(true, 17, new int[] { 0, 1, 2 }) },
            { "watches_m_19", new Watches(true, 18, new int[] { 0, 1, 2 }) },
            { "watches_m_20", new Watches(true, 19, new int[] { 0, 1, 2 }) },
            { "watches_m_21", new Watches(true, 30, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_m_22", new Watches(true, 31, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_m_23", new Watches(true, 32, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "watches_m_24", new Watches(true, 34, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_m_25", new Watches(true, 35, new int[] { 0, 1, 2, 3, 4, 5 }) },
            #endregion

            #region Watches Female
            { "watches_f_0", new Watches(false, 3, new int[] { 0, 1, 2 }) },
            { "watches_f_1", new Watches(false, 4, new int[] { 0, 1, 2 }) },
            { "watches_f_2", new Watches(false, 5, new int[] { 0, 1, 2 }) },
            { "watches_f_3", new Watches(false, 6, new int[] { 0, 1, 2 }) },
            { "watches_f_4", new Watches(false, 8, new int[] { 0, 1, 2 }) },
            { "watches_f_5", new Watches(false, 24, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_f_6", new Watches(false, 25, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_f_7", new Watches(false, 26, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_f_8", new Watches(false, 2, new int[] { 0, 1, 2, 3 }) },
            { "watches_f_9", new Watches(false, 7, new int[] { 0, 1, 2 }) },
            { "watches_f_10", new Watches(false, 9, new int[] { 0, 1, 2 }) },
            { "watches_f_11", new Watches(false, 19, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_f_12", new Watches(false, 20, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_f_13", new Watches(false, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "watches_f_14", new Watches(false, 23, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_f_15", new Watches(false, 24, new int[] { 0, 1, 2, 3, 4, 5 }) },
            #endregion

            #region Glasses Male
            { "glasses_m_0", new Glasses(true, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_1", new Glasses(true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_2", new Glasses(true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_3", new Glasses(true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_4", new Glasses(true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_5", new Glasses(true, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_6", new Glasses(true, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "glasses_m_7", new Glasses(true, 18, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_8", new Glasses(true, 21, new int[] { 0 }) },
            { "glasses_m_9", new Glasses(true, 22, new int[] { 0 }) },
            { "glasses_m_10", new Glasses(true, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_11", new Glasses(true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_12", new Glasses(true, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_13", new Glasses(true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_14", new Glasses(true, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_15", new Glasses(true, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_16", new Glasses(true, 17, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_17", new Glasses(true, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_18", new Glasses(true, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "glasses_m_19", new Glasses(true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }) },
            { "glasses_m_20", new Glasses(true, 30, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "glasses_m_21", new Glasses(true, 31, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "glasses_m_22", new Glasses(true, 32, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "glasses_m_23", new Glasses(true, 33, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            #endregion

            #region Glasses Female
            { "glasses_f_0", new Glasses(false, 0, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_1", new Glasses(false, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_2", new Glasses(false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_3", new Glasses(false, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_4", new Glasses(false, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_5", new Glasses(false, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_6", new Glasses(false, 22, new int[] { 0 }) },
            { "glasses_f_7", new Glasses(false, 23, new int[] { 0 }) },
            { "glasses_f_8", new Glasses(false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_9", new Glasses(false, 18, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_10", new Glasses(false, 19, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_11", new Glasses(false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_12", new Glasses(false, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "glasses_f_13", new Glasses(false, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "glasses_f_14", new Glasses(false, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_15", new Glasses(false, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "glasses_f_16", new Glasses(false, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "glasses_f_17", new Glasses(false, 24, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_18", new Glasses(false, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_19", new Glasses(false, 17, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "glasses_f_20", new Glasses(false, 30, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "glasses_f_21", new Glasses(false, 31, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }) },
            { "glasses_f_22", new Glasses(false, 32, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "glasses_f_23", new Glasses(false, 33, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "glasses_f_24", new Glasses(false, 34, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "glasses_f_25", new Glasses(false, 35, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            #endregion

            #region Gloves Male
            { "gloves_m_0", new Gloves(true, 51, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 51 }, { 14, 50 }, { 12, 49 }, { 11, 48 }, { 8, 47 }, { 6, 46 }, { 5, 45 }, { 4, 44 }, { 2, 43 }, { 1, 42 }, { 0, 41 }, { 184, 187 }, { 112, 117 }, { 113, 124 }, { 114, 131 }
                }, "gloves_f_0")
            },
            { "gloves_m_1", new Gloves(true, 62, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 62 }, { 14, 61 }, { 12, 60 }, { 11, 59 }, { 8, 58 }, { 6, 57 }, { 5, 56 }, { 4, 55 }, { 2, 54 }, { 1, 53 }, { 0, 52 }, { 184, 188 }, { 112, 118 }, { 113, 125 }, { 114, 132 }
                }, "gloves_f_1")
            },
            { "gloves_m_2", new Gloves(true, 73, new int[] { 0 }, new Dictionary<int, int>()
                {
                    { 15, 73 }, { 14, 72 }, { 12, 71 }, { 11, 70 }, { 8, 69 }, { 6, 68 }, { 5, 67 }, { 4, 66 }, { 2, 65 }, { 1, 64 }, { 0, 63 }, { 184, 189 }, { 112, 119 }, { 113, 126 }, { 114, 133 }
                }, "gloves_f_2")
            },
            { "gloves_m_3", new Gloves(true, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new Dictionary<int, int>()
                {
                    { 15, 109 }, { 14, 108 }, { 12, 107 }, { 11, 106 }, { 8, 105 }, { 6, 104 }, { 5, 103 }, { 4, 102 }, { 2, 101 }, { 1, 100 }, { 0, 99 }
                }, "gloves_f_3")
            },
            { "gloves_m_4", new Gloves(true, 95, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 95 }, { 14, 94 }, { 12, 93 }, { 11, 92 }, { 8, 91 }, { 6, 90 }, { 5, 89 }, { 4, 88 }, { 2, 87 }, { 1, 86 }, { 0, 85 }, { 184, 191 }, { 112, 121 }, { 113, 128 }, { 114, 135 }
                }, "gloves_f_4")
            },
            { "gloves_m_5", new Gloves(true, 29, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 29 }, { 14, 28 }, { 12, 27 }, { 11, 26 }, { 8, 25 }, { 6, 24 }, { 5, 23 }, { 4, 22 }, { 2, 21 }, { 1, 20 }, { 0, 19 }, { 184, 185 }, { 112, 115 }, { 113, 122 }, { 114, 129 }
                }, "gloves_f_5")
            },
            { "gloves_m_6", new Gloves(true, 40, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 40 }, { 14, 39 }, { 12, 38 }, { 11, 37 }, { 8, 36 }, { 6, 35 }, { 5, 34 }, { 4, 33 }, { 2, 32 }, { 1, 31 }, { 0, 30 }, { 184, 186 }, { 112, 116 }, { 113, 123 }, { 114, 130 }
                }, "gloves_f_6")
            },
            { "gloves_m_7", new Gloves(true, 84, new int[] { 0 }, new Dictionary<int, int>()
                {
                    { 15, 84 }, { 14, 83 }, { 12, 82 }, { 11, 81 }, { 8, 80 }, { 6, 79 }, { 5, 78 }, { 4, 77 }, { 2, 76 }, { 1, 75 }, { 0, 74 }, { 184, 190 }, { 112, 120 }, { 113, 127 }, { 114, 134 }
                }, "gloves_f_7")
            },
            { "gloves_m_8", new Gloves(true, 170, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, new Dictionary<int, int>()
                {
                    { 15, 170 }, { 14, 180 }, { 12, 179 }, { 11, 178 }, { 8, 177 }, { 6, 176 }, { 5, 175 }, { 4, 174 }, { 2, 173 }, { 1, 172 }, { 0, 171 }, { 184, 194 }, { 112, 181 }, { 113, 182 }, { 114, 183 }
                }, "gloves_f_8")
            },
            #endregion

            #region Gloves Female
            { "gloves_f_0", new Gloves(false, 58, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 58 }, { 14, 57 }, { 12, 56 }, { 11, 55 }, { 9, 54 }, { 7, 53 }, { 6, 52 }, { 5, 51 }, { 4, 50 }, { 3, 49 }, { 2, 48 }, { 1, 47 }, { 0, 46 }, { 129, 134 }, { 130, 141 }, { 131, 148 }, { 153, 156 }, { 161, 164 }, { 229, 232 }
                }, "gloves_m_0")
            },
            { "gloves_f_1", new Gloves(false, 71, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 71 }, { 14, 70 }, { 12, 69 }, { 11, 68 }, { 9, 67 }, { 7, 66 }, { 6, 65 }, { 5, 64 }, { 4, 63 }, { 3, 62 }, { 2, 61 }, { 1, 60 }, { 0, 59 }, { 129, 135 }, { 130, 142 }, { 131, 149 }, { 153, 157 }, { 161, 165 }, { 229, 233 }
                }, "gloves_m_1")
            },
            { "gloves_f_2", new Gloves(false, 84, new int[] { 0 }, new Dictionary<int, int>()
                {
                    { 15, 84 }, { 14, 83 }, { 12, 82 }, { 11, 81 }, { 9, 80 }, { 7, 79 }, { 6, 78 }, { 5, 77 }, { 4, 76 }, { 3, 75 }, { 2, 74 }, { 1, 73 }, { 0, 72 }, { 129, 136 }, { 130, 143 }, { 131, 150 }, { 153, 158 }, { 161, 166 }, { 229, 234 }
                }, "gloves_m_2")
            },
            { "gloves_f_3", new Gloves(false, 126, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new Dictionary<int, int>()
                {
                    { 15, 126 }, { 14, 125 }, { 12, 124 }, { 11, 123 }, { 9, 122 }, { 7, 121 }, { 6, 120 }, { 5, 119 }, { 4, 118 }, { 3, 117 }, { 2, 116 }, { 1, 115 }, { 0, 114 }
                }, "gloves_m_3")
            },
            { "gloves_f_4", new Gloves(false, 110, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 110 }, { 14, 109 }, { 12, 108 }, { 11, 107 }, { 9, 106 }, { 7, 105 }, { 6, 104 }, { 5, 103 }, { 4, 102 }, { 3, 101 }, { 2, 100 }, { 1, 99 }, { 0, 98 }, { 129, 138 }, { 130, 145 }, { 131, 152 }, { 153, 160 }, { 161, 168 }, { 229, 236 }
                }, "gloves_m_4")
            },
            { "gloves_f_5", new Gloves(false, 32, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 32 }, { 14, 31 }, { 12, 30 }, { 11, 29 }, { 9, 28 }, { 7, 27 }, { 6, 26 }, { 5, 25 }, { 4, 24 }, { 3, 23 }, { 2, 22 }, { 1, 21 }, { 0, 20 }, { 129, 132 }, { 130, 139 }, { 131, 146 }, { 153, 154 }, { 161, 162 }, { 229, 230 }
                }, "gloves_m_5")
            },
            { "gloves_f_6", new Gloves(false, 45, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 45 }, { 14, 44 }, { 12, 43 }, { 11, 42 }, { 9, 41 }, { 7, 40 }, { 6, 39 }, { 5, 38 }, { 4, 37 }, { 3, 36 }, { 2, 35 }, { 1, 34 }, { 0, 33 }, { 129, 133 }, { 130, 140 }, { 131, 147 }, { 153, 155 }, { 161, 163 }, { 229, 231 }
                }, "gloves_m_6")
            },
            { "gloves_f_7", new Gloves(false, 97, new int[] { 0 }, new Dictionary<int, int>()
                {
                    { 15, 97 }, { 14, 96 }, { 12, 95 }, { 11, 94 }, { 9, 93 }, { 7, 92 }, { 6, 91 }, { 5, 90 }, { 4, 89 }, { 3, 88 }, { 2, 87 }, { 1, 86 }, { 0, 85 }, { 129, 137 }, { 130, 144 }, { 131, 151 }, { 153, 159 }, { 161, 167 }, { 229, 235 }
                }, "gloves_m_7")
            },
            { "gloves_f_8", new Gloves(false, 211, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, new Dictionary<int, int>()
                {
                    { 15, 211 }, { 14, 223 }, { 12, 222 }, { 11, 221 }, { 9, 220 }, { 7, 219 }, { 6, 218 }, { 5, 217 }, { 4, 216 }, { 3, 215 }, { 2, 214 }, { 1, 213 }, { 0, 212 }, { 129, 224 }, { 130, 225 }, { 131, 226 }, { 153, 227 }, { 161, 228 }, { 229, 239 }
                }, "gloves_m_8")
            },
            #endregion

            #region Bracelets Male
            { "bracelet_m_0", new Bracelet(true, 0, new int[] { 0 }, "bracelet_f_7") },
            { "bracelet_m_1", new Bracelet(true, 1, new int[] { 0 }, "bracelet_f_8") },
            { "bracelet_m_2", new Bracelet(true, 2, new int[] { 0 }, "bracelet_f_9") },
            { "bracelet_m_3", new Bracelet(true, 3, new int[] { 0 }, "bracelet_f_10") },
            { "bracelet_m_4", new Bracelet(true, 4, new int[] { 0 }, "bracelet_f_11") },
            { "bracelet_m_5", new Bracelet(true, 5, new int[] { 0 }, "bracelet_f_12") },
            { "bracelet_m_6", new Bracelet(true, 6, new int[] { 0 }, "bracelet_f_13") },
            { "bracelet_m_7", new Bracelet(true, 7, new int[] { 0, 1, 2, 3 }, "bracelet_f_14") },
            { "bracelet_m_8", new Bracelet(true, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, "bracelet_f_15") },
            #endregion

            #region Bracelets Female
            { "bracelet_f_0", new Bracelet(false, 0, new int[] { 0 }) },
            { "bracelet_f_1", new Bracelet(false, 1, new int[] { 0 }) },
            { "bracelet_f_2", new Bracelet(false, 2, new int[] { 0 }) },
            { "bracelet_f_3", new Bracelet(false, 3, new int[] { 0 }) },
            { "bracelet_f_4", new Bracelet(false, 4, new int[] { 0 }) },
            { "bracelet_f_5", new Bracelet(false, 5, new int[] { 0 }) },
            { "bracelet_f_6", new Bracelet(false, 6, new int[] { 0 }) },
            { "bracelet_f_7", new Bracelet(false, 7, new int[] { 0 }, "bracelet_m_0") },
            { "bracelet_f_8", new Bracelet(false, 8, new int[] { 0 }, "bracelet_m_1") },
            { "bracelet_f_9", new Bracelet(false, 9, new int[] { 0 }, "bracelet_m_2") },
            { "bracelet_f_10", new Bracelet(false, 10, new int[] { 0 }, "bracelet_m_3") },
            { "bracelet_f_11", new Bracelet(false, 11, new int[] { 0 }, "bracelet_m_4") },
            { "bracelet_f_12", new Bracelet(false, 12, new int[] { 0 }, "bracelet_m_5") },
            { "bracelet_f_13", new Bracelet(false, 13, new int[] { 0 }, "bracelet_m_6") },
            { "bracelet_f_14", new Bracelet(false, 14, new int[] { 0, 1, 2, 3 }, "bracelet_m_7") },
            { "bracelet_f_15", new Bracelet(false, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, "bracelet_m_8") },
            #endregion

            #region Ears Male
            { "ears_m_0", new Ears(true, 3, new int[] { 0, 1, 2 }) },
            { "ears_m_1", new Ears(true, 4, new int[] { 0, 1, 2 }) },
            { "ears_m_2", new Ears(true, 5, new int[] { 0, 1, 2 }) },
            { "ears_m_3", new Ears(true, 6, new int[] { 0, 1 }) },
            { "ears_m_4", new Ears(true, 7, new int[] { 0, 1 }) },
            { "ears_m_5", new Ears(true, 8, new int[] { 0, 1 }) },
            { "ears_m_6", new Ears(true, 9, new int[] { 0, 1, 2 }) },
            { "ears_m_7", new Ears(true, 10, new int[] { 0, 1, 2 }) },
            { "ears_m_8", new Ears(true, 11, new int[] { 0, 1, 2 }) },
            { "ears_m_9", new Ears(true, 12, new int[] { 0, 1, 2 }) },
            { "ears_m_10", new Ears(true, 13, new int[] { 0, 1, 2 }) },
            { "ears_m_11", new Ears(true, 14, new int[] { 0, 1, 2 }) },
            { "ears_m_12", new Ears(true, 15, new int[] { 0, 1, 2 }) },
            { "ears_m_13", new Ears(true, 16, new int[] { 0, 1, 2 }) },
            { "ears_m_14", new Ears(true, 17, new int[] { 0, 1, 2 }) },
            { "ears_m_15", new Ears(true, 18, new int[] { 0, 1, 2, 3, 4 }) },
            { "ears_m_16", new Ears(true, 19, new int[] { 0, 1, 2, 3, 4 }) },
            { "ears_m_17", new Ears(true, 20, new int[] { 0, 1, 2, 3, 4 }) },
            { "ears_m_18", new Ears(true, 21, new int[] { 0, 1 }) },
            { "ears_m_19", new Ears(true, 22, new int[] { 0, 1 }) },
            { "ears_m_20", new Ears(true, 23, new int[] { 0, 1 }) },
            { "ears_m_21", new Ears(true, 24, new int[] { 0, 1, 2, 3 }) },
            { "ears_m_22", new Ears(true, 25, new int[] { 0, 1, 2, 3 }) },
            { "ears_m_23", new Ears(true, 26, new int[] { 0, 1, 2, 3 }) },
            { "ears_m_24", new Ears(true, 27, new int[] { 0, 1 }) },
            { "ears_m_25", new Ears(true, 28, new int[] { 0, 1 }) },
            { "ears_m_26", new Ears(true, 29, new int[] { 0, 1 }) },
            { "ears_m_27", new Ears(true, 30, new int[] { 0, 1, 2 }) },
            { "ears_m_28", new Ears(true, 21, new int[] { 0, 1 }) },
            { "ears_m_29", new Ears(true, 32, new int[] { 0, 1, 2 }) },
            { "ears_m_30", new Ears(true, 33, new int[] { 0 }) },
            { "ears_m_31", new Ears(true, 34, new int[] { 0, 1 }) },
            { "ears_m_32", new Ears(true, 35, new int[] { 0, 1 }) },
            { "ears_m_33", new Ears(true, 37, new int[] { 0, 1 }, "ears_f_14") },
            { "ears_m_34", new Ears(true, 38, new int[] { 0, 1, 2, 3 }, "ears_f_15") },
            { "ears_m_35", new Ears(true, 39, new int[] { 0, 1, 2, 3 }, "ears_f_16") },
            { "ears_m_36", new Ears(true, 40, new int[] { 0, 1, 2, 3 }, "ears_f_17") },
            #endregion

            #region Ears Female
            { "ears_f_0", new Ears(false, 3, new int[] { 0 }) },
            { "ears_f_1", new Ears(false, 4, new int[] { 0 }) },
            { "ears_f_2", new Ears(false, 5, new int[] { 0 }) },
            { "ears_f_3", new Ears(false, 6, new int[] { 0, 1, 2 }) },
            { "ears_f_4", new Ears(false, 7, new int[] { 0, 1, 2 }) },
            { "ears_f_5", new Ears(false, 8, new int[] { 0, 1, 2 }) },
            { "ears_f_6", new Ears(false, 9, new int[] { 0, 1, 2 }) },
            { "ears_f_7", new Ears(false, 10, new int[] { 0, 1, 2 }) },
            { "ears_f_8", new Ears(false, 11, new int[] { 0, 1, 2 }) },
            { "ears_f_9", new Ears(false, 12, new int[] { 0, 1, 2 }) },
            { "ears_f_10", new Ears(false, 13, new int[] { 0 }) },
            { "ears_f_11", new Ears(false, 14, new int[] { 0 }) },
            { "ears_f_12", new Ears(false, 15, new int[] { 0 }) },
            { "ears_f_13", new Ears(false, 16, new int[] { 0 }) },
            { "ears_f_14", new Ears(false, 18, new int[] { 0, 1 }, "ears_m_33") },
            { "ears_f_15", new Ears(false, 19, new int[] { 0, 1, 2, 3 }, "ears_m_34") },
            { "ears_f_16", new Ears(false, 20, new int[] { 0, 1, 2, 3 }, "ears_m_35") },
            { "ears_f_17", new Ears(false, 21, new int[] { 0, 1, 2, 3 }, "ears_m_36") },
            #endregion
        };

        #region Classes
        public abstract class Data
        {
            public class ExtraData
            {
                public int Drawable { get; set; }
                public int BestTorso { get; set; }

                public ExtraData(int Drawable, int BestTorso)
                {
                    this.Drawable = Drawable;
                    this.BestTorso = BestTorso;
                }
            }

            public Game.Items.Item.Types ItemType { get; set;}

            public bool Sex { get; set; }
            public int Drawable { get; set; }
            public int[] Textures { get; set; }
            public string SexAlternativeID { get; set; }

            public bool IsProp { get; set; }

            public Data(Items.Item.Types ItemType, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null)
            {
                this.Drawable = Drawable;
                this.Textures = Textures;

                this.ItemType = ItemType;

                this.Sex = Sex;

                this.SexAlternativeID = SexAlternativeID;

                this.IsProp = ItemType == Game.Items.Item.Types.Hat || ItemType == Game.Items.Item.Types.Glasses || ItemType == Game.Items.Item.Types.Ears || ItemType == Game.Items.Item.Types.Watches || ItemType == Game.Items.Item.Types.Bracelet;
            }
        }

        public class Top : Data
        {
            public int BestTorso { get; set; }
            public ExtraData ExtraData { get; set; }

            public Top(bool Sex, int Drawable, int[] Textures, int BestTorso, ExtraData ExtraData = null, string SexAlternative = null) : base(Items.Item.Types.Top, Sex, Drawable, Textures, SexAlternative)
            {
                this.BestTorso = BestTorso;
                this.ExtraData = ExtraData;
            }
        }

        public class Under : Data
        {
            public Top BestTop { get; set; }
            public int BestTorso { get; set; }
            public ExtraData ExtraData { get; set; }

            public Under(bool Sex, int Drawable, int[] Textures, Top BestTop, int BestTorso, ExtraData ExtraData = null, string SexAlternative = null) : base(Items.Item.Types.Under, Sex, Drawable, Textures, SexAlternative)
            {
                this.BestTop = BestTop;
                this.ExtraData = ExtraData;

                this.BestTorso = BestTorso;
            }
        }

        public class Gloves : Data
        {
            public Dictionary<int, int> BestTorsos { get; set; }

            public Gloves(bool Sex, int Drawable, int[] Textures, Dictionary<int, int> BestTorsos, string SexAlternative = null) : base(Items.Item.Types.Gloves, Sex, Drawable, Textures, SexAlternative)
            {
                this.BestTorsos = BestTorsos;
            }
        }

        public class Pants : Data
        {
            public Pants(bool Sex, int Drawable, int[] Textures, string SexAlternative = null) : base(Items.Item.Types.Pants, Sex, Drawable, Textures, SexAlternative)
            {

            }
        }

        public class Shoes : Data
        {
            public Shoes(bool Sex, int Drawable, int[] Textures, string SexAlternative = null) : base(Items.Item.Types.Shoes, Sex, Drawable, Textures, SexAlternative)
            {

            }
        }

        public class Hat : Data
        {
            public ExtraData ExtraData { get; set; }

            public Hat(bool Sex, int Drawable, int[] Textures, ExtraData ExtraData = null, string SexAlternative = null) : base(Items.Item.Types.Hat, Sex, Drawable, Textures, SexAlternative)
            {
                this.ExtraData = ExtraData;
            }
        }

        public class Accessory : Data
        {
            public Accessory(bool Sex, int Drawable, int[] Textures, string SexAlternative = null) : base(Items.Item.Types.Accessory, Sex, Drawable, Textures, SexAlternative)
            {

            }
        }

        public class Glasses : Data
        {
            public Glasses(bool Sex, int Drawable, int[] Textures, string SexAlternative = null) : base(Items.Item.Types.Glasses, Sex, Drawable, Textures, SexAlternative)
            {

            }
        }

        public class Watches : Data
        {
            public Watches(bool Sex, int Drawable, int[] Textures, string SexAlternative = null) : base(Items.Item.Types.Watches, Sex, Drawable, Textures, SexAlternative)
            {

            }
        }

        public class Bracelet : Data
        {
            public Bracelet(bool Sex, int Drawable, int[] Textures, string SexAlternative = null) : base(Items.Item.Types.Bracelet, Sex, Drawable, Textures, SexAlternative)
            {

            }
        }

        public class Ears : Data
        {
            public Ears(bool Sex, int Drawable, int[] Textures, string SexAlternative = null) : base(Items.Item.Types.Ears, Sex, Drawable, Textures, SexAlternative)
            {

            }
        }

        #endregion

        private static Dictionary<Items.Item.Types, int> Slots = new Dictionary<Items.Item.Types, int>()
        {
            // SetClothes
            { Items.Item.Types.Top, 11 }, { Items.Item.Types.Under, 8 },  { Items.Item.Types.Pants, 4 },  { Items.Item.Types.Shoes, 6 },
            { Items.Item.Types.Gloves, 3 },  { Items.Item.Types.Mask, 1 },  { Items.Item.Types.Accessory, 7 },  { Items.Item.Types.Bag, 0 },
            // SetProp
            { Items.Item.Types.Hat, 0 }, { Items.Item.Types.Glasses, 1 }, { Items.Item.Types.Ears, 2 }, { Items.Item.Types.Watches, 6 }, { Items.Item.Types.Bracelet, 7 },
        };

        private static Dictionary<bool, Dictionary<Items.Item.Types, int>> NudeDefault = new Dictionary<bool, Dictionary<Items.Item.Types, int>>()
        {
            { true, new Dictionary<Items.Item.Types, int>() { { Items.Item.Types.Top, 15 }, { Items.Item.Types.Under, 15 }, { Items.Item.Types.Gloves, 15 }, { Items.Item.Types.Pants, 21 }, { Items.Item.Types.Shoes, 34 }, { Items.Item.Types.Accessory, 0 }, { Items.Item.Types.Mask, 0 }, { Items.Item.Types.Bag, 0 }  } },
            { false, new Dictionary<Items.Item.Types, int>() { { Items.Item.Types.Top, 15 }, { Items.Item.Types.Under, 15 }, { Items.Item.Types.Gloves, 15 }, { Items.Item.Types.Pants, 15 }, { Items.Item.Types.Shoes, 35 }, { Items.Item.Types.Accessory, 0 }, { Items.Item.Types.Mask, 0 }, { Items.Item.Types.Bag, 0 } } },
        };

        #region Stuff
        public static int GetSlot(Items.Item.Types type)
        {
            if (!Slots.ContainsKey(type))
                return -1;

            return Slots[type];
        }

        public static Data GetData(string id)
        {
            if (!AllClothes.ContainsKey(id))
                return null;

            return AllClothes[id];
        }

        public static Data GetSexAlternative(string id)
        {
            if (!AllClothes.ContainsKey(id))
                return null;

            var clothes = AllClothes[id];

            if (clothes.SexAlternativeID == null)
                return null;

            if (AllClothes.ContainsKey(clothes.SexAlternativeID))
                return AllClothes[clothes.SexAlternativeID];

            return null;
        }

        public static int GetNudeDrawable(Items.Item.Types type, bool sex)
        {
            if (!NudeDefault[sex].ContainsKey(type))
                return 0;

            return NudeDefault[sex][type];
        }
        #endregion
    }
}
