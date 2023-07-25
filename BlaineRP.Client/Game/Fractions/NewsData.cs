using System.Collections.Generic;
using Newtonsoft.Json;

namespace BlaineRP.Client.Game.Fractions
{
    public class NewsData
    {
        [JsonProperty(PropertyName = "A")]
        public Dictionary<int, string> All { get; set; }

        [JsonProperty(PropertyName = "P")]
        public int PinnedId { get; set; }
    }
}