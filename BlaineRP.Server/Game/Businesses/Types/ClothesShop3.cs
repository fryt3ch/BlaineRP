using System.Collections.Generic;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Businesses
{
    public class ClothesShop3 : ClothesShop
    {
        public static BusinessType DefaultType => BusinessType.ClothesShop3;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {

            }
        };

        public ClothesShop3(int ID, Vector3 Position, Vector4 PositionInteract) : base(ID, Position, new Vector4(-1447.433f, -243.1756f, 49.82227f, 70f), DefaultType, PositionInteract)
        {

        }
    }
}