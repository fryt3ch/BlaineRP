using System.Collections.Generic;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Businesses
{
    public class BoatShop : VehicleShop
    {
        public static BusinessType DefaultType => BusinessType.BoatShop;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {
                { "avisa", 100 },
                { "dinghy", 100 },
                { "dinghy2", 100 },
                { "dinghy3", 100 },
                { "dinghy4", 100 },
                { "dinghy5", 100 },
                { "jetmax", 100 },
                { "kosatka", 100 },
                { "longfin", 100 },
                { "marquis", 100 },
                { "patrolboat", 100 },
                { "predator", 100 },
                { "seashark", 100 },
                { "seashark2", 100 },
                { "seashark3", 100 },
                { "speeder", 100 },
                { "speeder2", 100 },
                { "squalo", 100 },
                { "submersible", 100 },
                { "submersible2", 100 },
                { "suntrap", 100 },
                { "toro", 100 },
                { "toro2", 100 },
                { "tropic", 100 },
                { "tropic2", 100 },
                { "tug", 100 },
            }
        };

        public BoatShop(int ID, Vector3 Position, Vector4 EnterProperties, Vector4[] AfterBuyPositions, Vector4 PositionInteract) : base(ID, Position, EnterProperties, DefaultType, AfterBuyPositions, PositionInteract)
        {

        }
    }
}