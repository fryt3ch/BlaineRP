using GTANetworkAPI;
using Newtonsoft.Json;

namespace BlaineRP.Server
{
    public partial class VehicleData
    {
        public class LastVehicleData
        {
            [JsonProperty(PropertyName = "F")]
            public float Fuel { get; set; }

            [JsonProperty(PropertyName = "M")]
            public float Mileage { get; set; }

            [JsonProperty(PropertyName = "P")]
            public Vector3 Position { get; set; }

            [JsonProperty(PropertyName = "H")]
            public float Heading { get; set; }

            [JsonProperty(PropertyName = "D")]
            public uint Dimension { get; set; }

            [JsonProperty(PropertyName = "GS")]
            public int GarageSlot { get; set; }

            public LastVehicleData() { }
        }
    }
}
