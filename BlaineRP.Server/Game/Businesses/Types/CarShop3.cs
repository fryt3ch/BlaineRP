using System.Collections.Generic;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Businesses
{
    public class CarShop3 : VehicleShop
    {
        public static BusinessType DefaultType => BusinessType.CarShop3;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {

            }
        };

        public CarShop3(int ID, Vector3 Position, Vector4 EnterProperties, Vector4[] AfterBuyPositions, Vector4 PositionInteract) : base(ID, Position, EnterProperties, DefaultType, AfterBuyPositions, PositionInteract)
        {

        }
    }
}