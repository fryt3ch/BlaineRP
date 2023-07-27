using System;
using Newtonsoft.Json;

namespace BlaineRP.Server.EntitiesData.Players
{
    public class MilitaryTag
    {
        [JsonProperty(PropertyName = "I")]
        public DateTime IssueDate { get; set; }

        [JsonProperty(PropertyName = "F")]
        public Game.Fractions.FractionType IssueFraction { get; set; }

        [JsonProperty(PropertyName = "N")]
        public string GiverName { get; set; }

        public MilitaryTag()
        {

        }
    }
}