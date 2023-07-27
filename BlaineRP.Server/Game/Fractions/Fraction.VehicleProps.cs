using System;
using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Fractions
{
    public abstract partial class Fraction
    {
        public class VehicleProps
        {
            [JsonProperty(PropertyName = "R")]
            public byte MinimalRank { get; set; }

            [JsonIgnore]
            public DateTime LastRespawnedTime { get; set; }
        }
    }
}