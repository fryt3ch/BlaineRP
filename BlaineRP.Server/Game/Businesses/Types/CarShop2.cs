using System.Collections.Generic;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Businesses
{
    public class CarShop2 : VehicleShop
    {
        public static BusinessType DefaultType => BusinessType.CarShop2;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {

            }
        };

        public CarShop2(int ID, Vector3 Position, Vector4 EnterProperties, Vector4[] AfterBuyPositions, Vector4 PositionInteract) : base(ID, Position, EnterProperties, DefaultType, AfterBuyPositions, PositionInteract)
        {

        }
    }
}