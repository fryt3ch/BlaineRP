using System;
using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Fractions
{
    public partial class Police
    {
        public class APBInfo
        {
            [JsonProperty(PropertyName = "TN")]
            public string TargetName { get; set; }

            [JsonProperty(PropertyName = "D")]
            public string Details { get; set; }

            [JsonProperty(PropertyName = "LD")]
            public string LargeDetails { get; set; }

            [JsonProperty(PropertyName = "M")]
            public string Member { get; set; }

            [JsonProperty(PropertyName = "F")]
            public FractionType FractionType { get; set; }

            [JsonProperty(PropertyName = "T")]
            public DateTime Time { get; set; }

            public APBInfo()
            {

            }
        }
    }
}