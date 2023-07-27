using System.Collections.Generic;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Businesses
{
    public class JewelleryShop : ClothesShop
    {
        public static BusinessType DefaultType => BusinessType.JewelleryShop;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {
                { "ring_m_0", 10 },
                { "ring_m_1", 10 },

                { "ears_m_0", 10 },
                { "ears_m_1", 10 },
                { "ears_m_2", 10 },
                { "ears_m_3", 10 },
                { "ears_m_4", 10 },
                { "ears_m_5", 10 },
                { "ears_m_6", 10 },
                { "ears_m_7", 10 },
                { "ears_m_8", 10 },
                { "ears_m_9", 10 },
                { "ears_m_10", 10 },
                { "ears_m_11", 10 },
                { "ears_m_12", 10 },
                { "ears_m_13", 10 },
                { "ears_m_14", 10 },
                { "ears_m_15", 10 },
                { "ears_m_16", 10 },
                { "ears_m_17", 10 },
                { "ears_m_18", 10 },
                { "ears_m_19", 10 },
                { "ears_m_20", 10 },
                { "ears_m_21", 10 },
                { "ears_m_22", 10 },
                { "ears_m_23", 10 },
                { "ears_m_24", 10 },
                { "ears_m_25", 10 },
                { "ears_m_26", 10 },
                { "ears_m_27", 10 },
                { "ears_m_28", 10 },
                { "ears_m_29", 10 },
                { "ears_m_30", 10 },
                { "ears_m_31", 10 },
                { "ears_m_32", 10 },
                { "ears_m_33", 10 },
                { "ears_m_34", 10 },
                { "ears_m_35", 10 },
                { "ears_m_36", 10 },

                { "accs_m_0", 10 },
                { "accs_m_1", 10 },
                { "accs_m_3", 10 },
                { "accs_m_4", 10 },
                { "accs_m_5", 10 },
                { "accs_m_6", 10 },
                { "accs_m_7", 10 },
                { "accs_m_10", 10 },

                { "ring_f_0", 10 },
                { "ring_f_1", 10 },

                { "ears_f_0", 10 },
                { "ears_f_1", 10 },
                { "ears_f_2", 10 },
                { "ears_f_3", 10 },
                { "ears_f_4", 10 },
                { "ears_f_5", 10 },
                { "ears_f_6", 10 },
                { "ears_f_7", 10 },
                { "ears_f_8", 10 },
                { "ears_f_9", 10 },
                { "ears_f_10", 10 },
                { "ears_f_11", 10 },
                { "ears_f_12", 10 },
                { "ears_f_13", 10 },
                { "ears_f_14", 10 },
                { "ears_f_15", 10 },
                { "ears_f_16", 10 },
                { "ears_f_17", 10 },

                { "accs_f_6", 10 },
            }
        };

        public JewelleryShop(int ID, Vector3 Position, Vector4 PositionInteract, Vector4 ViewPosition) : base(ID, Position, ViewPosition, DefaultType, PositionInteract)
        {

        }
    }
}