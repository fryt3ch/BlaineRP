using System.Collections.Generic;
using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Fractions
{
    public abstract partial class Fraction
    {
        public class NewsData
        {
            [JsonProperty(PropertyName = "A")]
            public Dictionary<int, string> All { get; set; }

            [JsonProperty(PropertyName = "P")]
            public int PinnedId { get; set; }

            [JsonIgnore]
            public Queue<int> FreeIdxes { get; set; } = new Queue<int>();
        }
    }
}