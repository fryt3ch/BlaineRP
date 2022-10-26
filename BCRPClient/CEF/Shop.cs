using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace BCRPClient.CEF
{
    class Shop : Events.Script
    {
        //        Стало : //items[i] = [id, 'name', cash, bank, variants || maxspeed, chageable(t|f) || [slots, weight] || maxtank, cruise, autopilot, maxtrunk, maxweight] 
        //(если магаз не транспортный последние 4 параметра можно либо не передавать вообще, либо передавать как null)
        //- добавлены разделы магазинов машин(6, 7, 8, 9)

        public static bool IsActive { get => Browser.IsActive(Browser.IntTypes.Shop); }

        private static Additional.Camera.StateTypes[] AllowedCameraStates;

        private static DateTime LastBuyRequested;
        private static DateTime LastSent;

        /// <summary>ID текущего предмета</summary>
        private static string CurrentItem;

        /// <summary>Номер вариации текущего предмета</summary>
        /// <remarks>Используется только для одежды, тюнинга, причесок, татуировок</remarks>
        private static int CurrentVariation;

        /// <summary>Номер навигации</summary>
        /// <remarks>Используется только для одежды, тюнинга, причесок, татуировок</remarks>
        private static int CurrentNavigation;

        /// <summary>Цвет текущего предмета</summary>
        /// <remarks>Используется только для транспорта</remarks>
        private static Color CurrentColor;

        /// <summary>Тип текущего магазина</summary>
        private static Types CurrentType;

        public enum Types
        {
            None = -1,
            ClothesShop1 = 0,
            ClothesShop2,
            ClothesShop3,
        }

        private static List<int> TempBinds;

        private static float DefaultHeading;
        private static RAGE.Ui.Cursor.Vector2 LastCursorPos;
        private static AsyncTask CursorTask;

        private static int CurrentCameraStateNum;

        private static Vehicle CurrentVehicle;

        private static Dictionary<int, (int, int)> RealClothes;
        private static Dictionary<int, (int, int)> RealAccessories;

        #region ClothesShop1 Prices
        private static Dictionary<string, int> ClothesShop1_Prices = new Dictionary<string, int>()
        {
            { "top_m_0", 100 },
            { "top_m_1", 100 },
            { "top_m_3", 100 },
            { "top_m_4", 100 },
            { "top_m_5", 100 },
            { "top_m_6", 100 },
            { "top_m_7", 100 },
            { "top_m_8", 100 },
            { "top_m_9", 100 },
            { "top_m_10", 100 },
            { "top_m_11", 100 },
            { "top_m_12", 100 },
            { "top_m_13", 100 },
            { "top_m_14", 100 },
            { "top_m_15", 100 },
            { "top_m_16", 100 },
            { "top_m_17", 100 },
            { "top_m_18", 100 },
            { "top_m_19", 100 },
            { "top_m_20", 100 },
            { "top_m_22", 100 },
            { "top_m_23", 100 },
            { "top_m_24", 100 },
            { "top_m_25", 100 },
            { "top_m_26", 100 },
            { "top_m_27", 100 },
            { "top_m_28", 100 },
            { "top_m_29", 100 },
            { "top_m_30", 100 },
            { "top_m_31", 100 },
            { "top_m_32", 100 },
            { "top_m_33", 100 },
            { "top_m_34", 100 },
            { "top_m_35", 100 },
            { "top_m_36", 100 },
            { "top_m_37", 100 },
            { "top_m_38", 100 },
            { "top_m_39", 100 },
            { "top_m_40", 100 },
            { "top_m_41", 100 },
            { "top_m_42", 100 },
            { "top_m_43", 100 },
            { "top_m_44", 100 },
            { "top_m_45", 100 },
            { "top_m_46", 100 },
            { "top_m_47", 100 },
            { "top_m_48", 100 },
            { "top_m_49", 100 },
            { "top_m_50", 100 },
            { "top_m_51", 100 },
            { "top_m_52", 100 },
            { "top_m_53", 100 },
            { "top_m_54", 100 },
            { "top_m_55", 100 },
            { "top_m_56", 100 },
            { "top_m_57", 100 },
            { "top_m_58", 100 },
            { "top_m_59", 100 },
            { "top_m_60", 100 },
            { "top_m_61", 100 },
            { "top_m_62", 100 },
            { "top_m_63", 100 },
            { "top_m_64", 100 },
            { "top_m_65", 100 },
            { "top_m_66", 100 },
            { "top_m_67", 100 },
            { "top_m_68", 100 },
            { "top_m_69", 100 },
            { "top_m_70", 100 },
            { "top_m_71", 100 },
            { "top_m_72", 100 },
            { "top_m_73", 100 },
            { "top_m_74", 100 },
            { "top_m_75", 100 },
            { "top_m_76", 100 },
            { "top_m_77", 100 },
            { "top_m_78", 100 },
            { "top_m_79", 100 },
            { "top_m_80", 100 },
            { "top_m_81", 100 },
            { "top_m_82", 100 },
            { "top_m_83", 100 },
            { "top_m_84", 100 },
            { "top_m_85", 100 },
            { "top_m_86", 100 },
            { "top_m_87", 100 },
            { "top_m_88", 100 },
            { "top_m_89", 100 },
            { "top_m_90", 100 },
            { "top_m_92", 100 },
            { "top_m_93", 100 },
            { "top_m_94", 100 },
            { "top_m_97", 100 },
            { "top_m_98", 100 },
            { "top_m_99", 100 },
            { "top_m_100", 100 },
            { "top_m_101", 100 },
            { "top_m_102", 100 },
            { "top_m_103", 100 },
            { "top_m_104", 100 },
            { "top_m_105", 100 },
            { "top_m_106", 100 },
            { "top_m_107", 100 },
            { "top_m_108", 100 },
            { "top_m_109", 100 },
            { "top_m_110", 100 },
            { "top_m_112", 100 },
            { "top_m_113", 100 },
            { "top_m_114", 100 },
            { "top_m_116", 100 },
            { "top_m_117", 100 },
            { "top_m_118", 100 },
            { "top_m_119", 100 },
            { "top_m_120", 100 },
            { "top_m_121", 100 },
            { "top_m_122", 100 },
            { "top_m_123", 100 },
            { "top_m_124", 100 },
            { "top_m_126", 100 },
            { "top_m_127", 100 },
            { "top_m_128", 100 },
            { "top_m_129", 100 },
            { "top_m_130", 100 },
            { "top_m_132", 100 },
            { "top_m_133", 100 },

            { "top_f_1", 100 },
            { "top_f_2", 100 },
            { "top_f_3", 100 },
            { "top_f_4", 100 },
            { "top_f_5", 100 },
            { "top_f_6", 100 },
            { "top_f_8", 100 },
            { "top_f_10", 100 },
            { "top_f_11", 100 },
            { "top_f_12", 100 },
            { "top_f_13", 100 },
            { "top_f_14", 100 },
            { "top_f_15", 100 },
            { "top_f_16", 100 },
            { "top_f_17", 100 },
            { "top_f_18", 100 },
            { "top_f_19", 100 },
            { "top_f_20", 100 },
            { "top_f_21", 100 },
            { "top_f_22", 100 },
            { "top_f_23", 100 },
            { "top_f_24", 100 },
            { "top_f_25", 100 },
            { "top_f_26", 100 },
            { "top_f_27", 100 },
            { "top_f_28", 100 },
            { "top_f_29", 100 },
            { "top_f_30", 100 },
            { "top_f_31", 100 },
            { "top_f_32", 100 },
            { "top_f_33", 100 },
            { "top_f_34", 100 },
            { "top_f_35", 100 },
            { "top_f_36", 100 },
            { "top_f_37", 100 },
            { "top_f_38", 100 },
            { "top_f_39", 100 },
            { "top_f_40", 100 },
            { "top_f_41", 100 },
            { "top_f_42", 100 },
            { "top_f_43", 100 },
            { "top_f_44", 100 },
            { "top_f_45", 100 },
            { "top_f_46", 100 },
            { "top_f_47", 100 },
            { "top_f_48", 100 },
            { "top_f_49", 100 },
            { "top_f_50", 100 },
            { "top_f_51", 100 },
            { "top_f_52", 100 },
            { "top_f_53", 100 },
            { "top_f_54", 100 },
            { "top_f_55", 100 },
            { "top_f_56", 100 },
            { "top_f_57", 100 },
            { "top_f_58", 100 },
            { "top_f_59", 100 },
            { "top_f_60", 100 },
            { "top_f_61", 100 },
            { "top_f_62", 100 },
            { "top_f_63", 100 },
            { "top_f_64", 100 },
            { "top_f_65", 100 },
            { "top_f_66", 100 },
            { "top_f_68", 100 },
            { "top_f_69", 100 },
            { "top_f_70", 100 },
            { "top_f_71", 100 },
            { "top_f_72", 100 },
            { "top_f_73", 100 },
            { "top_f_74", 100 },
            { "top_f_75", 100 },
            { "top_f_76", 100 },
            { "top_f_77", 100 },
            { "top_f_78", 100 },
            { "top_f_79", 100 },
            { "top_f_80", 100 },
            { "top_f_81", 100 },
            { "top_f_82", 100 },
            { "top_f_83", 100 },
            { "top_f_84", 100 },
            { "top_f_85", 100 },
            { "top_f_86", 100 },
            { "top_f_87", 100 },
            { "top_f_88", 100 },
            { "top_f_89", 100 },
            { "top_f_90", 100 },
            { "top_f_91", 100 },
            { "top_f_92", 100 },
            { "top_f_93", 100 },
            { "top_f_94", 100 },
            { "top_f_95", 100 },
            { "top_f_97", 100 },
            { "top_f_98", 100 },
            { "top_f_99", 100 },
            { "top_f_100", 100 },
            { "top_f_101", 100 },
            { "top_f_102", 100 },
            { "top_f_103", 100 },
            { "top_f_104", 100 },
            { "top_f_105", 100 },
            { "top_f_106", 100 },
            { "top_f_107", 100 },
            { "top_f_108", 100 },
            { "top_f_109", 100 },
            { "top_f_110", 100 },
            { "top_f_111", 100 },
            { "top_f_112", 100 },
            { "top_f_113", 100 },
            { "top_f_114", 100 },
            { "top_f_115", 100 },
            { "top_f_117", 100 },
            { "top_f_118", 100 },
            { "top_f_119", 100 },
            { "top_f_120", 100 },
            { "top_f_121", 100 },
            { "top_f_122", 100 },
            { "top_f_123", 100 },
            { "top_f_124", 100 },
            { "top_f_125", 100 },
            { "top_f_126", 100 },
            { "top_f_127", 100 },
            { "top_f_128", 100 },
            { "top_f_129", 100 },
            { "top_f_130", 100 },
            { "top_f_131", 100 },
            { "top_f_132", 100 },
            { "top_f_133", 100 },
            { "top_f_134", 100 },
            { "top_f_135", 100 },
            { "top_f_137", 100 },
            { "top_f_138", 100 },
            { "top_f_139", 100 },

            { "under_m_0", 100 },
            { "under_m_1", 100 },
            { "under_m_2", 100 },
            { "under_m_3", 100 },
            { "under_m_4", 100 },
            { "under_m_5", 100 },
            { "under_m_6", 100 },
            { "under_m_7", 100 },
            { "under_m_8", 100 },
            { "under_m_9", 100 },
            { "under_m_10", 100 },
            { "under_m_11", 100 },
            { "under_m_12", 100 },
            { "under_m_13", 100 },
            { "under_m_14", 100 },
            { "under_m_15", 100 },
            { "under_m_16", 100 },
            { "under_m_17", 100 },
            { "under_m_18", 100 },
            { "under_m_19", 100 },
            { "under_m_20", 100 },
            { "under_m_21", 100 },
            { "under_m_22", 100 },
            { "under_m_24", 100 },
            { "under_m_25", 100 },
            { "under_m_26", 100 },

            { "under_f_0", 100 },
            { "under_f_1", 100 },
            { "under_f_2", 100 },
            { "under_f_3", 100 },
            { "under_f_4", 100 },
            { "under_f_5", 100 },
            { "under_f_6", 100 },
            { "under_f_7", 100 },
            { "under_f_8", 100 },
            { "under_f_9", 100 },
            { "under_f_10", 100 },
            { "under_f_11", 100 },
            { "under_f_12", 100 },
            { "under_f_13", 100 },
            { "under_f_14", 100 },
            { "under_f_15", 100 },
            { "under_f_16", 100 },
            { "under_f_17", 100 },
            { "under_f_18", 100 },
            { "under_f_19", 100 },

            { "pants_m_0", 100 },
            { "pants_m_1", 100 },
            { "pants_m_2", 100 },
            { "pants_m_3", 100 },
            { "pants_m_4", 100 },
            { "pants_m_5", 100 },
            { "pants_m_6", 100 },
            { "pants_m_7", 100 },
            { "pants_m_8", 100 },
            { "pants_m_9", 100 },
            { "pants_m_10", 100 },
            { "pants_m_11", 100 },
            { "pants_m_12", 100 },
            { "pants_m_13", 100 },
            { "pants_m_14", 100 },
            { "pants_m_15", 100 },
            { "pants_m_16", 100 },
            { "pants_m_17", 100 },
            { "pants_m_18", 100 },
            { "pants_m_19", 100 },
            { "pants_m_20", 100 },
            { "pants_m_21", 100 },
            { "pants_m_22", 100 },
            { "pants_m_23", 100 },
            { "pants_m_24", 100 },
            { "pants_m_25", 100 },
            { "pants_m_26", 100 },
            { "pants_m_27", 100 },
            { "pants_m_28", 100 },
            { "pants_m_29", 100 },
            { "pants_m_30", 100 },
            { "pants_m_31", 100 },
            { "pants_m_32", 100 },
            { "pants_m_33", 100 },
            { "pants_m_34", 100 },
            { "pants_m_35", 100 },
            { "pants_m_36", 100 },
            { "pants_m_37", 100 },
            { "pants_m_38", 100 },
            { "pants_m_39", 100 },
            { "pants_m_40", 100 },
            { "pants_m_41", 100 },
            { "pants_m_42", 100 },
            { "pants_m_43", 100 },
            { "pants_m_44", 100 },

            { "pants_f_0", 100 },
            { "pants_f_1", 100 },
            { "pants_f_2", 100 },
            { "pants_f_3", 100 },
            { "pants_f_4", 100 },
            { "pants_f_5", 100 },
            { "pants_f_6", 100 },
            { "pants_f_7", 100 },
            { "pants_f_8", 100 },
            { "pants_f_9", 100 },
            { "pants_f_10", 100 },
            { "pants_f_11", 100 },
            { "pants_f_12", 100 },
            { "pants_f_13", 100 },
            { "pants_f_14", 100 },
            { "pants_f_15", 100 },
            { "pants_f_16", 100 },
            { "pants_f_17", 100 },
            { "pants_f_18", 100 },
            { "pants_f_19", 100 },
            { "pants_f_20", 100 },
            { "pants_f_21", 100 },
            { "pants_f_22", 100 },
            { "pants_f_23", 100 },
            { "pants_f_24", 100 },
            { "pants_f_25", 100 },
            { "pants_f_26", 100 },
            { "pants_f_27", 100 },
            { "pants_f_28", 100 },
            { "pants_f_29", 100 },
            { "pants_f_30", 100 },
            { "pants_f_31", 100 },
            { "pants_f_32", 100 },
            { "pants_f_33", 100 },
            { "pants_f_34", 100 },
            { "pants_f_35", 100 },
            { "pants_f_36", 100 },
            { "pants_f_37", 100 },
            { "pants_f_38", 100 },
            { "pants_f_39", 100 },
            { "pants_f_40", 100 },
            { "pants_f_41", 100 },
            { "pants_f_42", 100 },
            { "pants_f_43", 100 },
            { "pants_f_44", 100 },
            { "pants_f_45", 100 },
            { "pants_f_46", 100 },
            { "pants_f_47", 100 },
            { "pants_f_48", 100 },

            { "shoes_m_0", 100 },
            { "shoes_m_1", 100 },
            { "shoes_m_2", 100 },
            { "shoes_m_3", 100 },
            { "shoes_m_4", 100 },
            { "shoes_m_5", 100 },
            { "shoes_m_6", 100 },
            { "shoes_m_7", 100 },
            { "shoes_m_8", 100 },
            { "shoes_m_9", 100 },
            { "shoes_m_10", 100 },
            { "shoes_m_11", 100 },
            { "shoes_m_12", 100 },
            { "shoes_m_13", 100 },
            { "shoes_m_14", 100 },
            { "shoes_m_15", 100 },
            { "shoes_m_16", 100 },
            { "shoes_m_17", 100 },
            { "shoes_m_18", 100 },
            { "shoes_m_19", 100 },
            { "shoes_m_20", 100 },
            { "shoes_m_21", 100 },
            { "shoes_m_22", 100 },
            { "shoes_m_23", 100 },
            { "shoes_m_24", 100 },
            { "shoes_m_25", 100 },
            { "shoes_m_26", 100 },
            { "shoes_m_27", 100 },
            { "shoes_m_28", 100 },
            { "shoes_m_29", 100 },
            { "shoes_m_30", 100 },
            { "shoes_m_31", 100 },
            { "shoes_m_32", 100 },
            { "shoes_m_33", 100 },
            { "shoes_m_34", 100 },
            { "shoes_m_35", 100 },
            { "shoes_m_36", 100 },
            { "shoes_m_37", 100 },
            { "shoes_m_38", 100 },
            { "shoes_m_39", 100 },
            { "shoes_m_40", 100 },
            { "shoes_m_41", 100 },
            { "shoes_m_42", 100 },
            { "shoes_m_43", 100 },
            { "shoes_m_44", 100 },
            { "shoes_m_45", 100 },
            { "shoes_m_46", 100 },
            { "shoes_m_47", 100 },

            { "shoes_f_0", 100 },
            { "shoes_f_1", 100 },
            { "shoes_f_2", 100 },
            { "shoes_f_3", 100 },
            { "shoes_f_4", 100 },
            { "shoes_f_5", 100 },
            { "shoes_f_6", 100 },
            { "shoes_f_7", 100 },
            { "shoes_f_8", 100 },
            { "shoes_f_9", 100 },
            { "shoes_f_10", 100 },
            { "shoes_f_11", 100 },
            { "shoes_f_12", 100 },
            { "shoes_f_13", 100 },
            { "shoes_f_14", 100 },
            { "shoes_f_15", 100 },
            { "shoes_f_16", 100 },
            { "shoes_f_17", 100 },
            { "shoes_f_18", 100 },
            { "shoes_f_19", 100 },
            { "shoes_f_20", 100 },
            { "shoes_f_21", 100 },
            { "shoes_f_22", 100 },
            { "shoes_f_23", 100 },
            { "shoes_f_24", 100 },
            { "shoes_f_25", 100 },
            { "shoes_f_26", 100 },
            { "shoes_f_27", 100 },
            { "shoes_f_28", 100 },
            { "shoes_f_29", 100 },
            { "shoes_f_30", 100 },
            { "shoes_f_31", 100 },
            { "shoes_f_32", 100 },
            { "shoes_f_33", 100 },
            { "shoes_f_34", 100 },
            { "shoes_f_35", 100 },
            { "shoes_f_36", 100 },
            { "shoes_f_37", 100 },
            { "shoes_f_38", 100 },
            { "shoes_f_39", 100 },
            { "shoes_f_40", 100 },
            { "shoes_f_41", 100 },
            { "shoes_f_42", 100 },
            { "shoes_f_43", 100 },
            { "shoes_f_44", 100 },
            { "shoes_f_45", 100 },
            { "shoes_f_46", 100 },

            { "hat_m_0", 100 },
            { "hat_m_1", 100 },
            { "hat_m_2", 100 },
            { "hat_m_3", 100 },
            { "hat_m_4", 100 },
            { "hat_m_5", 100 },
            { "hat_m_6", 100 },
            { "hat_m_7", 100 },
            { "hat_m_8", 100 },
            { "hat_m_9", 100 },
            { "hat_m_10", 100 },
            { "hat_m_11", 100 },
            { "hat_m_12", 100 },
            { "hat_m_13", 100 },
            { "hat_m_14", 100 },
            { "hat_m_15", 100 },
            { "hat_m_16", 100 },
            { "hat_m_17", 100 },
            { "hat_m_18", 100 },
            { "hat_m_19", 100 },
            { "hat_m_20", 100 },
            { "hat_m_21", 100 },
            { "hat_m_22", 100 },
            { "hat_m_23", 100 },
            { "hat_m_24", 100 },
            { "hat_m_25", 100 },
            { "hat_m_26", 100 },
            { "hat_m_27", 100 },
            { "hat_m_28", 100 },
            { "hat_m_29", 100 },
            { "hat_m_30", 100 },
            { "hat_m_31", 100 },
            { "hat_m_32", 100 },
            { "hat_m_33", 100 },
            { "hat_m_34", 100 },
            { "hat_m_35", 100 },
            { "hat_m_36", 100 },
            { "hat_m_37", 100 },

            { "hat_f_0", 100 },
            { "hat_f_1", 100 },
            { "hat_f_2", 100 },
            { "hat_f_3", 100 },
            { "hat_f_4", 100 },
            { "hat_f_5", 100 },
            { "hat_f_6", 100 },
            { "hat_f_7", 100 },
            { "hat_f_8", 100 },
            { "hat_f_9", 100 },
            { "hat_f_10", 100 },
            { "hat_f_11", 100 },
            { "hat_f_12", 100 },
            { "hat_f_13", 100 },
            { "hat_f_14", 100 },
            { "hat_f_15", 100 },
            { "hat_f_16", 100 },
            { "hat_f_17", 100 },
            { "hat_f_18", 100 },
            { "hat_f_19", 100 },
            { "hat_f_20", 100 },
            { "hat_f_21", 100 },
            { "hat_f_22", 100 },
            { "hat_f_23", 100 },
            { "hat_f_24", 100 },
            { "hat_f_25", 100 },
            { "hat_f_26", 100 },
            { "hat_f_27", 100 },
            { "hat_f_28", 100 },
            { "hat_f_29", 100 },
            { "hat_f_30", 100 },
            { "hat_f_31", 100 },
            { "hat_f_32", 100 },
            { "hat_f_33", 100 },
            { "hat_f_34", 100 },
            { "hat_f_35", 100 },
            { "hat_f_36", 100 },

            { "accs_m_0", 100 },
            { "accs_m_1", 100 },
            { "accs_m_2", 100 },
            { "accs_m_3", 100 },
            { "accs_m_4", 100 },
            { "accs_m_5", 100 },
            { "accs_m_6", 100 },
            { "accs_m_7", 100 },
            { "accs_m_8", 100 },
            { "accs_m_9", 100 },
            { "accs_m_10", 100 },
            { "accs_m_11", 100 },
            { "accs_m_12", 100 },

            { "accs_f_0", 100 },
            { "accs_f_1", 100 },
            { "accs_f_2", 100 },
            { "accs_f_3", 100 },
            { "accs_f_4", 100 },
            { "accs_f_5", 100 },

            { "watches_m_0", 100 },
            { "watches_m_1", 100 },
            { "watches_m_2", 100 },
            { "watches_m_3", 100 },
            { "watches_m_4", 100 },
            { "watches_m_5", 100 },
            { "watches_m_6", 100 },
            { "watches_m_7", 100 },
            { "watches_m_8", 100 },
            { "watches_m_9", 100 },
            { "watches_m_10", 100 },
            { "watches_m_11", 100 },

            { "watches_f_0", 100 },
            { "watches_f_1", 100 },
            { "watches_f_2", 100 },
            { "watches_f_3", 100 },
            { "watches_f_4", 100 },
            { "watches_f_5", 100 },
            { "watches_f_6", 100 },
            { "watches_f_7", 100 },

            { "glasses_m_0", 100 },
            { "glasses_m_1", 100 },
            { "glasses_m_2", 100 },
            { "glasses_m_3", 100 },
            { "glasses_m_4", 100 },
            { "glasses_m_5", 100 },
            { "glasses_m_6", 100 },
            { "glasses_m_7", 100 },
            { "glasses_m_8", 100 },
            { "glasses_m_9", 100 },

            { "glasses_f_0", 100 },
            { "glasses_f_1", 100 },
            { "glasses_f_2", 100 },
            { "glasses_f_3", 100 },
            { "glasses_f_4", 100 },
            { "glasses_f_5", 100 },
            { "glasses_f_6", 100 },
            { "glasses_f_7", 100 },

            { "gloves_m_0", 100 },
            { "gloves_m_1", 100 },
            { "gloves_m_2", 100 },
            { "gloves_m_3", 100 },
            { "gloves_m_4", 100 },

            { "gloves_f_0", 100 },
            { "gloves_f_1", 100 },
            { "gloves_f_2", 100 },
            { "gloves_f_3", 100 },
            { "gloves_f_4", 100 },

            { "bracelet_m_6", 100 },
            { "bracelet_m_7", 100 },
            { "bracelet_m_8", 100 },

            { "bracelet_f_13", 100 },
            { "bracelet_f_14", 100 },
            { "bracelet_f_15", 100 },
        };
        #endregion

        #region ClothesShop2 Prices
        private static Dictionary<string, int> ClothesShop2_Prices = new Dictionary<string, int>()
        {
            { "top_m_134", 100},
            { "top_m_135", 100},
            { "top_m_136", 100},
            { "top_m_137", 100},
            { "top_m_138", 100},
            { "top_m_139", 100},
            { "top_m_140", 100},
            { "top_m_141", 100},
            { "top_m_142", 100},
            { "top_m_143", 100},
            { "top_m_144", 100},
            { "top_m_145", 100},
            { "top_m_146", 100},
            { "top_m_147", 100},
            { "top_m_148", 100},
            { "top_m_149", 100},
            { "top_m_150", 100},
            { "top_m_151", 100},
            { "top_m_152", 100},
            { "top_m_153", 100},
            { "top_m_154", 100},
            { "top_m_155", 100},
            { "top_m_156", 100},
            { "top_m_157", 100},
            { "top_m_158", 100},
            { "top_m_159", 100},
            { "top_m_160", 100},
            { "top_m_161", 100},
            { "top_m_162", 100},
            { "top_m_163", 100},
            { "top_m_164", 100},
            { "top_m_165", 100},
            { "top_m_166", 100},
            { "top_m_167", 100},
            { "top_m_168", 100},
            { "top_m_170", 100},
            { "top_m_171", 100},
            { "top_m_172", 100},
            { "top_m_173", 100},
            { "top_m_174", 100},
            { "top_m_175", 100},
            { "top_m_176", 100},
            { "top_m_177", 100},
            { "top_m_178", 100},
            { "top_m_179", 100},
            { "top_m_180", 100},
            { "top_m_181", 100},
            { "top_m_182", 100},
            { "top_m_183", 100},
            { "top_m_184", 100},
            { "top_m_185", 100},
            { "top_m_186", 100},
            { "top_m_187", 100},
            { "top_m_188", 100},
            { "top_m_189", 100},
            { "top_m_190", 100},
            { "top_m_191", 100},
            { "top_m_192", 100},
            { "top_m_193", 100},
            { "top_m_194", 100},
            { "top_m_195", 100},
            { "top_m_196", 100},
            { "top_m_197", 100},
            { "top_m_198", 100},

            { "top_f_140", 100 },
            { "top_f_141", 100 },
            { "top_f_142", 100 },
            { "top_f_143", 100 },
            { "top_f_144", 100 },
            { "top_f_145", 100 },
            { "top_f_146", 100 },
            { "top_f_147", 100 },
            { "top_f_148", 100 },
            { "top_f_149", 100 },
            { "top_f_150", 100 },
            { "top_f_151", 100 },
            { "top_f_152", 100 },
            { "top_f_153", 100 },
            { "top_f_154", 100 },
            { "top_f_155", 100 },
            { "top_f_156", 100 },
            { "top_f_157", 100 },
            { "top_f_158", 100 },
            { "top_f_159", 100 },
            { "top_f_160", 100 },
            { "top_f_161", 100 },
            { "top_f_162", 100 },
            { "top_f_163", 100 },
            { "top_f_164", 100 },
            { "top_f_165", 100 },
            { "top_f_166", 100 },
            { "top_f_167", 100 },
            { "top_f_168", 100 },
            { "top_f_169", 100 },
            { "top_f_170", 100 },
            { "top_f_171", 100 },
            { "top_f_172", 100 },
            { "top_f_173", 100 },
            { "top_f_174", 100 },
            { "top_f_175", 100 },
            { "top_f_176", 100 },
            { "top_f_177", 100 },
            { "top_f_178", 100 },
            { "top_f_179", 100 },
            { "top_f_180", 100 },
            { "top_f_181", 100 },
            { "top_f_182", 100 },
            { "top_f_183", 100 },
            { "top_f_184", 100 },
            { "top_f_185", 100 },
            { "top_f_186", 100 },
            { "top_f_187", 100 },
            { "top_f_188", 100 },
            { "top_f_189", 100 },
            { "top_f_191", 100 },
            { "top_f_192", 100 },
            { "top_f_193", 100 },
            { "top_f_194", 100 },
            { "top_f_195", 100 },
            { "top_f_196", 100 },
            { "top_f_197", 100 },
            { "top_f_198", 100 },
            { "top_f_199", 100 },
            { "top_f_200", 100 },
            { "top_f_201", 100 },
            { "top_f_202", 100 },
            { "top_f_203", 100 },
            { "top_f_204", 100 },
            { "top_f_205", 100 },
            { "top_f_206", 100 },
            { "top_f_207", 100 },
            { "top_f_208", 100 },
            { "top_f_209", 100 },
            { "top_f_210", 100 },

            { "under_m_27", 100 },
            { "under_m_28", 100 },
            { "under_m_29", 100 },
            { "under_m_30", 100 },
            { "under_m_31", 100 },
            { "under_m_32", 100 },
            { "under_m_33", 100 },
            { "under_m_34", 100 },
            { "under_m_35", 100 },

            { "under_f_20", 100 },
            { "under_f_21", 100 },
            { "under_f_22", 100 },
            { "under_f_23", 100 },
            { "under_f_24", 100 },

            { "pants_m_45", 100 },
            { "pants_m_46", 100 },
            { "pants_m_47", 100 },
            { "pants_m_48", 100 },
            { "pants_m_49", 100 },
            { "pants_m_50", 100 },
            { "pants_m_51", 100 },
            { "pants_m_52", 100 },
            { "pants_m_53", 100 },
            { "pants_m_54", 100 },
            { "pants_m_55", 100 },
            { "pants_m_56", 100 },
            { "pants_m_57", 100 },
            { "pants_m_58", 100 },
            { "pants_m_59", 100 },
            { "pants_m_60", 100 },
            { "pants_m_61", 100 },
            { "pants_m_62", 100 },
            { "pants_m_63", 100 },
            { "pants_m_64", 100 },
            { "pants_m_65", 100 },
            { "pants_m_66", 100 },
            { "pants_m_67", 100 },
            { "pants_m_68", 100 },
            { "pants_m_69", 100 },
            { "pants_m_70", 100 },
            { "pants_m_71", 100 },
            { "pants_m_72", 100 },
            { "pants_m_73", 100 },
            { "pants_m_74", 100 },
            { "pants_m_75", 100 },

            { "pants_f_49", 100 },
            { "pants_f_50", 100 },
            { "pants_f_51", 100 },
            { "pants_f_52", 100 },
            { "pants_f_53", 100 },
            { "pants_f_54", 100 },
            { "pants_f_55", 100 },
            { "pants_f_56", 100 },
            { "pants_f_57", 100 },
            { "pants_f_58", 100 },
            { "pants_f_59", 100 },
            { "pants_f_60", 100 },
            { "pants_f_61", 100 },
            { "pants_f_62", 100 },
            { "pants_f_63", 100 },
            { "pants_f_64", 100 },
            { "pants_f_65", 100 },
            { "pants_f_66", 100 },
            { "pants_f_67", 100 },
            { "pants_f_68", 100 },
            { "pants_f_69", 100 },
            { "pants_f_70", 100 },
            { "pants_f_71", 100 },
            { "pants_f_72", 100 },
            { "pants_f_73", 100 },
            { "pants_f_74", 100 },
            { "pants_f_75", 100 },
            { "pants_f_76", 100 },
            { "pants_f_77", 100 },
            { "pants_f_78", 100 },
            { "pants_f_79", 100 },
            { "pants_f_80", 100 },
            { "pants_f_81", 100 },
            { "pants_f_82", 100 },
            { "pants_f_83", 100 },
            { "pants_f_84", 100 },

            { "shoes_m_48", 100 },
            { "shoes_m_49", 100 },
            { "shoes_m_50", 100 },
            { "shoes_m_51", 100 },
            { "shoes_m_52", 100 },
            { "shoes_m_53", 100 },
            { "shoes_m_54", 100 },
            { "shoes_m_55", 100 },
            { "shoes_m_56", 100 },
            { "shoes_m_57", 100 },
            { "shoes_m_58", 100 },
            { "shoes_m_59", 100 },
            { "shoes_m_60", 100 },
            { "shoes_m_61", 100 },
            { "shoes_m_62", 100 },
            { "shoes_m_63", 100 },
            { "shoes_m_64", 100 },
            { "shoes_m_65", 100 },
            { "shoes_m_66", 100 },
            { "shoes_m_67", 100 },
            { "shoes_m_68", 100 },
            { "shoes_m_69", 100 },
            { "shoes_m_70", 100 },

            { "shoes_f_47", 100 },
            { "shoes_f_48", 100 },
            { "shoes_f_49", 100 },
            { "shoes_f_50", 100 },
            { "shoes_f_51", 100 },
            { "shoes_f_52", 100 },
            { "shoes_f_53", 100 },
            { "shoes_f_54", 100 },
            { "shoes_f_55", 100 },
            { "shoes_f_56", 100 },
            { "shoes_f_57", 100 },
            { "shoes_f_58", 100 },
            { "shoes_f_59", 100 },
            { "shoes_f_60", 100 },
            { "shoes_f_61", 100 },
            { "shoes_f_62", 100 },
            { "shoes_f_63", 100 },
            { "shoes_f_64", 100 },
            { "shoes_f_65", 100 },
            { "shoes_f_66", 100 },
            { "shoes_f_67", 100 },
            { "shoes_f_68", 100 },
            { "shoes_f_69", 100 },
            { "shoes_f_70", 100 },
            { "shoes_f_71", 100 },
            { "shoes_f_72", 100 },
            { "shoes_f_73", 100 },
            { "shoes_f_74", 100 },
            { "shoes_f_75", 100 },
            { "shoes_f_76", 100 },
            { "shoes_f_77", 100 },
            { "shoes_f_78", 100 },
            { "shoes_f_79", 100 },

            { "hat_m_38", 100 },
            { "hat_m_39", 100 },
            { "hat_m_40", 100 },
            { "hat_m_41", 100 },
            { "hat_m_42", 100 },
            { "hat_m_43", 100 },
            { "hat_m_44", 100 },
            { "hat_m_45", 100 },
            { "hat_m_46", 100 },
            { "hat_m_47", 100 },
            { "hat_m_48", 100 },
            { "hat_m_49", 100 },
            { "hat_m_50", 100 },
            { "hat_m_51", 100 },
            { "hat_m_52", 100 },
            { "hat_m_53", 100 },
            { "hat_m_54", 100 },

            { "hat_f_37", 100 },
            { "hat_f_38", 100 },
            { "hat_f_39", 100 },
            { "hat_f_40", 100 },
            { "hat_f_41", 100 },
            { "hat_f_42", 100 },
            { "hat_f_43", 100 },
            { "hat_f_44", 100 },
            { "hat_f_45", 100 },
            { "hat_f_46", 100 },
            { "hat_f_47", 100 },
            { "hat_f_48", 100 },
            { "hat_f_49", 100 },

            { "accs_m_13", 100 },
            { "accs_m_14", 100 },
            { "accs_m_15", 100 },
            { "accs_m_16", 100 },
            { "accs_m_17", 100 },
            { "accs_m_18", 100 },
            { "accs_m_19", 100 },
            { "accs_m_20", 100 },
            { "accs_m_21", 100 },
            { "accs_m_22", 100 },

            { "accs_f_6", 100 },
            { "accs_f_7", 100 },
            { "accs_f_8", 100 },
            { "accs_f_9", 100 },
            { "accs_f_10", 100 },
            { "accs_f_11", 100 },

            { "watches_m_12", 100 },
            { "watches_m_13", 100 },
            { "watches_m_14", 100 },
            { "watches_m_15", 100 },
            { "watches_m_16", 100 },
            { "watches_m_17", 100 },
            { "watches_m_18", 100 },
            { "watches_m_19", 100 },
            { "watches_m_20", 100 },
            { "watches_m_21", 100 },
            { "watches_m_22", 100 },
            { "watches_m_23", 100 },
            { "watches_m_24", 100 },
            { "watches_m_25", 100 },

            { "watches_f_8", 100 },
            { "watches_f_9", 100 },
            { "watches_f_10", 100 },
            { "watches_f_11", 100 },
            { "watches_f_12", 100 },
            { "watches_f_13", 100 },
            { "watches_f_14", 100 },
            { "watches_f_15", 100 },

            { "glasses_m_10", 100 },
            { "glasses_m_11", 100 },
            { "glasses_m_12", 100 },
            { "glasses_m_13", 100 },
            { "glasses_m_14", 100 },
            { "glasses_m_15", 100 },
            { "glasses_m_16", 100 },
            { "glasses_m_17", 100 },
            { "glasses_m_18", 100 },
            { "glasses_m_19", 100 },
            { "glasses_m_20", 100 },
            { "glasses_m_21", 100 },
            { "glasses_m_22", 100 },
            { "glasses_m_23", 100 },

            { "glasses_f_8", 100 },
            { "glasses_f_9", 100 },
            { "glasses_f_10", 100 },
            { "glasses_f_11", 100 },
            { "glasses_f_12", 100 },
            { "glasses_f_13", 100 },
            { "glasses_f_14", 100 },
            { "glasses_f_15", 100 },
            { "glasses_f_16", 100 },
            { "glasses_f_17", 100 },
            { "glasses_f_18", 100 },
            { "glasses_f_19", 100 },
            { "glasses_f_20", 100 },
            { "glasses_f_21", 100 },
            { "glasses_f_22", 100 },
            { "glasses_f_23", 100 },
            { "glasses_f_24", 100 },
            { "glasses_f_25", 100 },

            { "gloves_m_5", 100 },
            { "gloves_m_6", 100 },
            { "gloves_m_7", 100 },
            { "gloves_m_8", 100 },

            { "gloves_f_5", 100 },
            { "gloves_f_6", 100 },
            { "gloves_f_7", 100 },
            { "gloves_f_8", 100 },

            { "bracelet_m_0", 100 },
            { "bracelet_m_1", 100 },
            { "bracelet_m_2", 100 },
            { "bracelet_m_3", 100 },
            { "bracelet_m_4", 100 },
            { "bracelet_m_5", 100 },

            { "bracelet_f_0", 100 },
            { "bracelet_f_1", 100 },
            { "bracelet_f_2", 100 },
            { "bracelet_f_3", 100 },
            { "bracelet_f_4", 100 },
            { "bracelet_f_5", 100 },
            { "bracelet_f_6", 100 },
            { "bracelet_f_7", 100 },
            { "bracelet_f_8", 100 },
            { "bracelet_f_9", 100 },
            { "bracelet_f_10", 100 },
            { "bracelet_f_11", 100 },
            { "bracelet_f_12", 100 },
        };
        #endregion

        public Shop()
        {
            TempBinds = new List<int>();

            LastBuyRequested = DateTime.Now;
            LastSent = DateTime.Now;

            CurrentType = Types.None;

            Events.Add("Shop::Show", async (object[] args) =>
            {
                Types type = (Types)(int)args[0];
                float margin = (float)args[1];
                float? heading = args.Length > 2 ? (float?)args[2] : null;

                await Show(type, margin, heading);
            });

            Events.Add("Shop::Close::Server", (object[] args) => Close(false));

            Events.Add("Shop::Close", (object[] args) => { Close(true); });

            Events.Add("Shop::Choose", (object[] args) =>
            {
                if (CurrentType == Types.ClothesShop1 || CurrentType == Types.ClothesShop2 || CurrentType == Types.ClothesShop3)
                {
                    bool newItem = false;

                    if (CurrentItem != (string)args[0])
                        newItem = true;

                    CurrentItem = (string)args[0];
                    CurrentVariation = (int)args[1];

                    Data.Clothes.Wear(CurrentItem, CurrentVariation);

                    var data = Data.Clothes.GetData(CurrentItem);

                    if (data == null)
                        return;

                    if (newItem)
                    {
                        if (data.ItemType == Data.Items.Types.Under)
                        {
                            if ((data as Data.Clothes.Under).ExtraData == null && (data as Data.Clothes.Under).BestTop != null && (data as Data.Clothes.Under).BestTop.ExtraData != null)
                                CEF.Notification.ShowHint(Locale.Notifications.Hints.ClothesShopUnderExtraNotNeedTop, false);
                            else if ((data as Data.Clothes.Under).ExtraData != null && (data as Data.Clothes.Under).BestTop != null && (data as Data.Clothes.Under).BestTop.ExtraData == null)
                                CEF.Notification.ShowHint(Locale.Notifications.Hints.ClothesShopUnderExtraNeedTop, false);
                        }
                    }

                    var variation = CurrentVariation < data.Textures.Length && CurrentVariation >= 0 ? data.Textures[CurrentVariation] : 0;

                    //Utils.ConsoleOutput($"ID: {CurrentItem}, Var: {CurrentVariation}, Drawable: {data.Drawable}, Texture: {variation}");
                }
            });

            Events.Add("Shop::Action", (object[] args) =>
            {
                if (CurrentType == Types.ClothesShop1 || CurrentType == Types.ClothesShop2 || CurrentType == Types.ClothesShop3)
                {
                    Data.Clothes.Action(CurrentItem, CurrentVariation);
                }
            });

            Events.Add("Shop::NavChange", (object[] args) =>
            {
                var id = (int)args[0];

                CurrentNavigation = id;

                if (AllowedCameraStates == null)
                    return;

                if (CurrentType == Types.ClothesShop1 || CurrentType == Types.ClothesShop2 || CurrentType == Types.ClothesShop3)
                {
                    if (id == 0 || id == 1)
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.Head));
                    else if (id == 2 || id == 3 || id == 4)
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.Body));
                    else if (id == 6 || id == 5)
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.Legs));
                    else if (id == 7)
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.Foots));
                    else if (id == 8)
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.LeftHand));
                    else if (id == 9)
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.RightHand));
                }
            });

            Events.Add("Shop::ClearChoice", (object[] args) =>
            {
                CurrentItem = null;
                CurrentVariation = 0;

                if (CurrentType == Types.ClothesShop1 || CurrentType == Types.ClothesShop2 || CurrentType == Types.ClothesShop3)
                {
                    if (CurrentNavigation == 0)
                        Data.Clothes.Unwear(Data.Items.Types.Hat);
                    else if (CurrentNavigation == 1)
                        Data.Clothes.Unwear(Data.Items.Types.Glasses);
                    else if (CurrentNavigation == 2)
                        Data.Clothes.Unwear(Data.Items.Types.Top);
                    else if (CurrentNavigation == 3)
                        Data.Clothes.Unwear(Data.Items.Types.Under);
                    else if (CurrentNavigation == 4)
                        Data.Clothes.Unwear(Data.Items.Types.Accessory);
                    else if (CurrentNavigation == 5)
                        Data.Clothes.Unwear(Data.Items.Types.Gloves);
                    else if (CurrentNavigation == 6)
                        Data.Clothes.Unwear(Data.Items.Types.Pants);
                    else if (CurrentNavigation == 7)
                        Data.Clothes.Unwear(Data.Items.Types.Shoes);
                    else if (CurrentNavigation == 8)
                        Data.Clothes.Unwear(Data.Items.Types.Watches);
                    else if (CurrentNavigation == 9)
                        Data.Clothes.Unwear(Data.Items.Types.Bracelet);
                }
            });

            Events.Add("Shop::Buy", (object[] args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null || CurrentItem == null)
                    return;

                bool useCash = (bool)args[0];

                var prices = GetPrices(CurrentType);

                if (prices?.ContainsKey(CurrentItem) != true)
                    return;

                if (DateTime.Now.Subtract(LastBuyRequested).TotalMilliseconds > 5000)
                {
                    CEF.Notification.Show(Notification.Types.Question, Locale.Notifications.ApproveHeader, Locale.Notifications.Money.AdmitToBuy, 5000);

                    LastBuyRequested = DateTime.Now;
                }
                else
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    Events.CallRemote("Shop::Buy", CurrentItem, CurrentVariation, 1, useCash);

                    LastBuyRequested = DateTime.MinValue;
                }
            });
        }

        public static async System.Threading.Tasks.Task Show(Types type, float margin, float? heading = null)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            CurrentType = type;

            await Browser.Render(Browser.IntTypes.Shop, true);

            if (heading != null)
            {
                DefaultHeading = (float)heading;

                Additional.ExtraColshape.InteractionColshapesAllowed = false;

                Additional.SkyCamera.FadeScreen(true);

                CEF.HUD.ShowHUD(false);
                CEF.Chat.Show(false);

                GameEvents.Render -= CharacterCreation.PlayerLookCursor;
                GameEvents.Render += CharacterCreation.PlayerLookCursor;

                GameEvents.Render -= GameEvents.DisableAllControls;
                GameEvents.Render += GameEvents.DisableAllControls;

                KeyBinds.DisableAll(KeyBinds.Types.Cursor);

                (new AsyncTask(async () =>
                {
                    Additional.Camera.Enable(Additional.Camera.StateTypes.WholePed, Player.LocalPlayer, Player.LocalPlayer, 0);

                    Additional.SkyCamera.FadeScreen(false);

                    CEF.Notification.ShowHint(Locale.Notifications.CharacterCreation.CtrlMovePed, true, 5000);

                    CurrentCameraStateNum = 0;

                    TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Control, true, () =>
                    {
                        if (CursorTask != null)
                            return;

                        LastCursorPos = RAGE.Ui.Cursor.Position;

                        CursorTask = new AsyncTask(() => OnTickMouse(), 10, true);
                        CursorTask.Run();
                    }));

                    TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Control, false, () =>
                    {
                        if (CursorTask == null)
                            return;

                        CursorTask.Cancel();

                        CursorTask = null;
                    }));

                    TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.V, true, () =>
                    {
                        ChangeView(++CurrentCameraStateNum);
                    }));

                    Browser.Window.ExecuteJs("Shop.draw", type);

                    if (type == Types.ClothesShop1 || type == Types.ClothesShop2 || type == Types.ClothesShop3)
                    {
                        AllowedCameraStates = new Additional.Camera.StateTypes[] { Additional.Camera.StateTypes.WholePed, Additional.Camera.StateTypes.Head, Additional.Camera.StateTypes.Body, Additional.Camera.StateTypes.RightHand, Additional.Camera.StateTypes.LeftHand, Additional.Camera.StateTypes.Legs, Additional.Camera.StateTypes.Foots };

                        RealClothes = new Dictionary<int, (int, int)>()
                            {
                                { 1, (Player.LocalPlayer.GetDrawableVariation(1), Player.LocalPlayer.GetTextureVariation(1)) },
                                { 2, (Player.LocalPlayer.GetDrawableVariation(2), 0) },
                                { 3, (Player.LocalPlayer.GetDrawableVariation(3), Player.LocalPlayer.GetTextureVariation(3)) },
                                { 4, (Player.LocalPlayer.GetDrawableVariation(4), Player.LocalPlayer.GetTextureVariation(4)) },
                                { 5, (Player.LocalPlayer.GetDrawableVariation(5), Player.LocalPlayer.GetTextureVariation(5)) },
                                { 6, (Player.LocalPlayer.GetDrawableVariation(6), Player.LocalPlayer.GetTextureVariation(6)) },
                                { 7, (Player.LocalPlayer.GetDrawableVariation(7), Player.LocalPlayer.GetTextureVariation(7)) },
                                { 8, (Player.LocalPlayer.GetDrawableVariation(8), Player.LocalPlayer.GetTextureVariation(8)) },
                                { 9, (Player.LocalPlayer.GetDrawableVariation(9), Player.LocalPlayer.GetTextureVariation(9)) },
                                { 10, (Player.LocalPlayer.GetDrawableVariation(10), Player.LocalPlayer.GetTextureVariation(10)) },
                                { 11, (Player.LocalPlayer.GetDrawableVariation(11), Player.LocalPlayer.GetTextureVariation(11)) },
                            };

                        RealAccessories = new Dictionary<int, (int, int)>()
                            {
                                { 0, (Player.LocalPlayer.GetPropIndex(0), Player.LocalPlayer.GetPropTextureIndex(0)) },
                                { 1, (Player.LocalPlayer.GetPropIndex(1), Player.LocalPlayer.GetPropTextureIndex(1)) },
                                { 2, (Player.LocalPlayer.GetPropIndex(2), Player.LocalPlayer.GetPropTextureIndex(2)) },
                                { 6, (Player.LocalPlayer.GetPropIndex(6), Player.LocalPlayer.GetPropTextureIndex(6)) },
                                { 7, (Player.LocalPlayer.GetPropIndex(7), Player.LocalPlayer.GetPropTextureIndex(7)) },
                            };

                        Player.LocalPlayer.SetComponentVariation(5, 0, 0, 2);
                        Player.LocalPlayer.SetComponentVariation(9, 0, 0, 2);

                        var currentTop = Data.Clothes.AllClothes.Where(x => x.Value.Sex == pData.Sex && x.Value.ItemType == Data.Items.Types.Top && x.Value.Drawable == RealClothes[11].Item1).Select(x => x.Key).FirstOrDefault();
                        var currentUnder = Data.Clothes.AllClothes.Where(x => x.Value.Sex == pData.Sex && x.Value.ItemType == Data.Items.Types.Under && x.Value.Drawable == RealClothes[8].Item1).Select(x => x.Key).FirstOrDefault();
                        var currentGloves = Data.Clothes.AllClothes.Where(x => x.Value.Sex == pData.Sex && x.Value.ItemType == Data.Items.Types.Gloves && (x.Value as Data.Clothes.Gloves).BestTorsos.ContainsValue(RealClothes[3].Item1)).Select(x => x.Key).FirstOrDefault();

                        if (currentTop != null)
                            Player.LocalPlayer.SetData("TempClothes::Top", new Data.Clothes.TempClothes(currentTop, RealClothes[11].Item2));

                        if (currentUnder != null)
                            Player.LocalPlayer.SetData("TempClothes::Under", new Data.Clothes.TempClothes(currentUnder, RealClothes[8].Item2));

                        if (currentGloves != null)
                            Player.LocalPlayer.SetData("TempClothes::Gloves", new Data.Clothes.TempClothes(currentGloves, RealClothes[3].Item2));

                        CEF.Notification.ShowHint(Locale.Notifications.Hints.ClothesShopOrder, false, 7500);

                        Dictionary<string, int> prices = GetPrices(CurrentType);

                        if (prices == null)
                            return;

                        List<object[]> hats = new List<object[]>();
                        List<object[]> tops = new List<object[]>();
                        List<object[]> unders = new List<object[]>();
                        List<object[]> pants = new List<object[]>();
                        List<object[]> shoes = new List<object[]>();
                        List<object[]> accs = new List<object[]>();
                        List<object[]> glasses = new List<object[]>();
                        List<object[]> gloves = new List<object[]>();
                        List<object[]> watches = new List<object[]>();
                        List<object[]> bracelets = new List<object[]>();

                        var clearingItem = new object[] { "clear", "Ничего", 0, 0, 0, false };

                        hats.Add(clearingItem);
                        tops.Add(clearingItem);
                        unders.Add(clearingItem);
                        pants.Add(clearingItem);
                        shoes.Add(clearingItem);
                        accs.Add(clearingItem);
                        glasses.Add(clearingItem);
                        gloves.Add(clearingItem);
                        watches.Add(clearingItem);
                        bracelets.Add(clearingItem);

                        foreach (var x in prices)
                        {
                            var data = Data.Clothes.GetData(x.Key);

                            if (data == null || data.Sex != pData.Sex)
                                continue;

                            if (data is Data.Clothes.Hat)
                                hats.Add(new object[] { x.Key, Data.Items.GetName(x.Key), x.Value * margin, x.Value * margin, data.Textures.Length, (data as Data.Clothes.Hat).ExtraData != null });
                            else if (data is Data.Clothes.Top)
                                tops.Add(new object[] { x.Key, Data.Items.GetName(x.Key), x.Value * margin, x.Value * margin, data.Textures.Length, (data as Data.Clothes.Top).ExtraData != null });
                            else if (data is Data.Clothes.Under)
                                unders.Add(new object[] { x.Key, Data.Items.GetName(x.Key), x.Value * margin, x.Value * margin, data.Textures.Length, (data as Data.Clothes.Under).ExtraData != null });
                            else if (data is Data.Clothes.Pants)
                                pants.Add(new object[] { x.Key, Data.Items.GetName(x.Key), x.Value * margin, x.Value * margin, data.Textures.Length, false });
                            else if (data is Data.Clothes.Shoes)
                                shoes.Add(new object[] { x.Key, Data.Items.GetName(x.Key), x.Value * margin, x.Value * margin, data.Textures.Length, false });
                            else if (data is Data.Clothes.Accessory)
                                accs.Add(new object[] { x.Key, Data.Items.GetName(x.Key), x.Value * margin, x.Value * margin, data.Textures.Length, false });
                            else if (data is Data.Clothes.Glasses)
                                glasses.Add(new object[] { x.Key, Data.Items.GetName(x.Key), x.Value * margin, x.Value * margin, data.Textures.Length, false });
                            else if (data is Data.Clothes.Gloves)
                                gloves.Add(new object[] { x.Key, Data.Items.GetName(x.Key), x.Value * margin, x.Value * margin, data.Textures.Length, false });
                            else if (data is Data.Clothes.Watches)
                                watches.Add(new object[] { x.Key, Data.Items.GetName(x.Key), x.Value * margin, x.Value * margin, data.Textures.Length, false });
                            else if (data is Data.Clothes.Bracelet)
                                bracelets.Add(new object[] { x.Key, Data.Items.GetName(x.Key), x.Value * margin, x.Value * margin, data.Textures.Length, false });
                        }

                        Browser.Window.ExecuteJs("Shop.fillContainer", 0, hats);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 1, glasses);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 2, tops);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 3, unders);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 4, accs);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 5, gloves);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 6, pants);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 7, shoes);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 8, watches);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 9, bracelets);

                        Browser.Switch(Browser.IntTypes.Shop, true);

                        Cursor.Show(true, true);
                    }
                }, 1500, false, 0)).Run();
            }
        }

        public static void Close(bool request = true)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (request)
            {
                if (LastSent.IsSpam(1000, false, false))
                    return;

                Events.CallRemote("Business::Exit");

                LastSent = DateTime.Now;
            }
            else
            {
                AllowedCameraStates = null;

                if (CurrentType == Types.ClothesShop1 || CurrentType == Types.ClothesShop2 || CurrentType == Types.ClothesShop2)
                {
                    Player.LocalPlayer.ResetData("TempClothes::Top");
                    Player.LocalPlayer.ResetData("TempClothes::Under");
                    Player.LocalPlayer.ResetData("TempClothes::Gloves");
                    Player.LocalPlayer.ResetData("TempClothes::Hat");

                    foreach (var x in RealClothes)
                    {
                        Player.LocalPlayer.SetComponentVariation(x.Key, x.Value.Item1, x.Value.Item2, 2);

                        if (x.Key == 2)
                        {
                            pData.HairOverlay = pData.HairOverlay;
                        }
                    }

                    Player.LocalPlayer.ClearAllProps();

                    foreach (var x in RealAccessories)
                    {
                        Player.LocalPlayer.SetPropIndex(x.Key, x.Value.Item1, x.Value.Item2, true);
                    }

                    RealClothes.Clear();
                    RealAccessories.Clear();
                }

                Browser.Render(Browser.IntTypes.Shop, false);

                CurrentType = Types.None;

                foreach (var x in TempBinds)
                    RAGE.Input.Unbind(x);

                TempBinds.Clear();

                Cursor.Show(false, false);

                Additional.ExtraColshape.InteractionColshapesAllowed = false;

                Additional.SkyCamera.FadeScreen(true);

                (new AsyncTask(() =>
                {
                    GameEvents.Render -= CharacterCreation.PlayerLookCursor;
                    GameEvents.Render -= GameEvents.DisableAllControls;

                    CEF.Chat.Show(true);

                    if (!Settings.Interface.HideHUD)
                        CEF.HUD.ShowHUD(true);

                    KeyBinds.EnableAll();

                    Additional.Camera.Disable();

                    Additional.SkyCamera.FadeScreen(false);
                }, 1500, false, 0)).Run();

                (new AsyncTask(() =>
                {
                    Additional.ExtraColshape.InteractionColshapesAllowed = true;
                }, 2500, false, 0)).Run();
            }
        }

        private static Dictionary<string, int> GetPrices(Types type)
        {
            if (type == Types.ClothesShop1)
                return ClothesShop1_Prices;
            else if (type == Types.ClothesShop2)
                return ClothesShop2_Prices;
            else if (type == Types.ClothesShop3)
                return null;

            return null;
        }

        private static void OnTickMouse()
        {
            var curPos = RAGE.Ui.Cursor.Position;
            var dist = curPos.Distance(LastCursorPos);
            var newHeading = CurrentVehicle == null ? Player.LocalPlayer.GetHeading() : CurrentVehicle.GetHeading();

            if (curPos.X > LastCursorPos.X)
                newHeading += dist / 10;
            else if (curPos.X < LastCursorPos.X)
                newHeading -= dist / 10;
            else if (curPos.X == LastCursorPos.X)
            {
                if (curPos.X == 0)
                    newHeading -= 5;
                else if (curPos.X + 10 >= GameEvents.ScreenResolution.X)
                    newHeading += 5;
            }

            if (RAGE.Game.Pad.GetDisabledControlNormal(0, 241) == 1f)
            {
                Additional.Camera.Fov -= 1;
            }
            else if (RAGE.Game.Pad.GetDisabledControlNormal(0, 242) == 1f)
            {
                Additional.Camera.Fov += 1;
            }

            if (newHeading > 360f)
                newHeading = 0f;
            else if (newHeading < 0f)
                newHeading = 360f;

            if (CurrentVehicle != null)
                CurrentVehicle.SetHeading(newHeading);
            else
                Player.LocalPlayer.SetHeading(newHeading);

            LastCursorPos = curPos;
        }

        private static void ChangeView(int camStateNum)
        {
            if (AllowedCameraStates == null)
                return;

            if (camStateNum >= AllowedCameraStates.Length || AllowedCameraStates.Length < camStateNum)
                camStateNum = 0;

            CurrentCameraStateNum = camStateNum;

            if (CurrentVehicle != null)
            {
                CurrentVehicle.SetHeading(DefaultHeading);
            }
            else
            {
                Player.LocalPlayer.SetHeading(DefaultHeading);

                Additional.Camera.FromState(AllowedCameraStates[camStateNum], Player.LocalPlayer, Player.LocalPlayer, -1);
            }
        }
    }
}
