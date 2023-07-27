using System.Collections.Generic;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Businesses
{
    public class BagShop : ClothesShop
    {
        public static BusinessType DefaultType => BusinessType.BagShop;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {
                { "bag_m_0", 10 },
                { "bag_m_1", 10 },
                { "bag_m_2", 10 },
                { "bag_m_3", 10 },
                { "bag_m_4", 10 },
                { "bag_m_5", 10 },
                { "bag_m_6", 10 },
                { "bag_m_7", 10 },

                { "bag_f_0", 10 },
            }
        };

        public BagShop(int ID, Vector3 Position, Vector4 PositionInteract, Vector4 ViewPosition) : base(ID, Position, ViewPosition, DefaultType, PositionInteract)
        {

        }
    }
}