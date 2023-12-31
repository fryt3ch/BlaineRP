﻿using System.Collections.Generic;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Businesses
{
    public class ClothesShop2 : ClothesShop
    {
        public static BusinessType DefaultType => BusinessType.ClothesShop2;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
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
            }
        };

        public ClothesShop2(int ID, Vector3 Position, Vector4 PositionInteract) : base(ID, Position, new Vector4(617.65f, 2766.828f, 42.0881f, 176f), DefaultType, PositionInteract)
        {

        }
    }
}