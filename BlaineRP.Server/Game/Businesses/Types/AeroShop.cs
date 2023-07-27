using System.Collections.Generic;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Businesses
{
    public class AeroShop : VehicleShop
    {
        public static BusinessType DefaultType => BusinessType.AeroShop;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {
                { "akula", 100 },
                { "annihilator", 100 },
                { "annihilator2", 100 },
                { "buzzard", 100 },
                { "buzzard2", 100 },
                { "cargobob", 100 },
                { "cargobob2", 100 },
                { "cargobob3", 100 },
                { "cargobob4", 100 },
                { "conada", 100 },
                { "frogger", 100 },
                { "frogger2", 100 },
                { "havok", 100 },
                { "hunter", 100 },
                { "maverick", 100 },
                { "savage", 100 },
                { "seasparrow", 100 },
                { "seasparrow2", 100 },
                { "seasparrow3", 100 },
                { "skylift", 100 },
                { "supervolito", 100 },
                { "supervolito2", 100 },
                { "swift", 100 },
                { "swift2", 100 },
                { "valkyrie", 100 },
                { "valkyrie2", 100 },
                { "volatus", 100 },
                { "polmav", 100 },
                { "alphaz1", 100 },
                { "avenger", 100 },
                { "avenger2", 100 },
                { "besra", 100 },
                { "bombushka", 100 },
                { "cargoplane", 100 },
                { "cuban800", 100 },
                { "dodo", 100 },
                { "duster", 100 },
                { "howard", 100 },
                { "hydra", 100 },
                { "jet", 100 },
                { "lazer", 100 },
                { "luxor", 100 },
                { "luxor2", 100 },
                { "mammatus", 100 },
                { "microlight", 100 },
                { "miljet", 100 },
                { "mogul", 100 },
                { "molotok", 100 },
                { "nimbus", 100 },
                { "nokota", 100 },
                { "pyro", 100 },
                { "rogue", 100 },
                { "seabreeze", 100 },
                { "shamal", 100 },
                { "starling", 100 },
                { "strikeforce", 100 },
                { "stunt", 100 },
                { "titan", 100 },
                { "tula", 100 },
                { "velum", 100 },
                { "velum2", 100 },
                { "vestra", 100 },
                { "volatol", 100 },
                { "alkonost", 100 },
            }
        };

        public AeroShop(int ID, Vector3 Position, Vector4 EnterProperties, Vector4[] AfterBuyPositions, Vector4 PositionInteract) : base(ID, Position, EnterProperties, DefaultType, AfterBuyPositions, PositionInteract)
        {

        }
    }
}