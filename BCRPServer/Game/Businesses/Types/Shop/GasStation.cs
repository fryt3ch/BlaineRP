using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Businesses
{
    public class GasStation : Shop
    {
        public static Types DefaultType => Types.GasStation;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {

            }
        };

        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, {GasolinesPosition.ToCSharpStr()}, {PositionInteract.ToCSharpStr()}";

        private static Dictionary<Game.Data.Vehicles.Vehicle.FuelTypes, int> GasPrices = new Dictionary<Game.Data.Vehicles.Vehicle.FuelTypes, int>()
        {
            { Game.Data.Vehicles.Vehicle.FuelTypes.Petrol, 10 },

            { Game.Data.Vehicles.Vehicle.FuelTypes.Electricity, 5 },
        };

        public Vector3 GasolinesPosition { get; set; }

        public int GetGasPrice(Game.Data.Vehicles.Vehicle.FuelTypes fType, bool addMargin)
        {
            int price;

            if (!GasPrices.TryGetValue(fType, out price))
                return -1;

            if (addMargin)
                return (int)Math.Floor(price * Margin);
            else
                return price;
        }

        public GasStation(int ID, Vector3 PositionInfo, Utils.Vector4 PositionInteract, Vector3 GasolinesPosition) : base(ID, PositionInfo, PositionInteract, DefaultType)
        {
            this.GasolinesPosition = GasolinesPosition;
        }
    }
}
