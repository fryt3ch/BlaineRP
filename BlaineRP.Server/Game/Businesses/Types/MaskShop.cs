using System.Collections.Generic;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Businesses
{
    public class MaskShop : ClothesShop
    {
        public static BusinessType DefaultType => BusinessType.MaskShop;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {
                { "mask_m_0", 10 },
                { "mask_m_1", 10 },
                { "mask_m_2", 10 },
                { "mask_m_3", 10 },
                { "mask_m_4", 10 },
                { "mask_m_5", 10 },
                { "mask_m_6", 10 },
                { "mask_m_7", 10 },
                { "mask_m_8", 10 },
                { "mask_m_9", 10 },
                { "mask_m_10", 10 },
                { "mask_m_11", 10 },
                { "mask_m_12", 10 },
                { "mask_m_13", 10 },
                { "mask_m_14", 10 },
                { "mask_m_15", 10 },
                { "mask_m_16", 10 },
                { "mask_m_17", 10 },
                { "mask_m_18", 10 },
                { "mask_m_19", 10 },
                { "mask_m_20", 10 },
                { "mask_m_21", 10 },
                { "mask_m_22", 10 },
                { "mask_m_23", 10 },
                { "mask_m_24", 10 },
                { "mask_m_25", 10 },
                { "mask_m_26", 10 },
                { "mask_m_27", 10 },
                { "mask_m_28", 10 },

                { "mask_f_0", 10 },
                { "mask_f_1", 10 },
            }
        };

        public MaskShop(int ID, Vector3 Position, Vector4 PositionInteract, Vector4 ViewPosition) : base(ID, Position, ViewPosition, DefaultType, PositionInteract)
        {

        }
    }
}