using GTANetworkAPI;
using System.Collections.Generic;

namespace BCRPServer.Game.Businesses
{
    public class GasStation : Shop
    {
        public static Types DefaultType => Types.GasStation;

        public static MaterialsData InitMaterialsData => new MaterialsData(1, 2, 5)
        {
            Prices = new Dictionary<string, uint>()
            {
                { "gas_g_0", 1 },
                { "gas_e_0", 0 },
            }
        };

        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, {GasolinesPosition.ToCSharpStr()}, {PositionInteract.ToCSharpStr()}";

        public static Dictionary<Game.Data.Vehicles.Vehicle.FuelTypes, string> GasIds { get; private set; } = new Dictionary<Game.Data.Vehicles.Vehicle.FuelTypes, string>()
        {
            { Game.Data.Vehicles.Vehicle.FuelTypes.Petrol, "gas_g_0" },

            { Game.Data.Vehicles.Vehicle.FuelTypes.Electricity, "gas_e_0" },
        };

        public bool IsGas(string itemId) => GasIds.ContainsValue(itemId);

        public Vector3 GasolinesPosition { get; set; }

        public GasStation(int ID, Vector3 PositionInfo, Utils.Vector4 PositionInteract, Vector3 GasolinesPosition) : base(ID, PositionInfo, PositionInteract, DefaultType)
        {
            this.GasolinesPosition = GasolinesPosition;
        }
    }
}
