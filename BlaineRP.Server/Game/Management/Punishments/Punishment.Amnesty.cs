using System;
using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Management.Punishments
{
    public partial class Punishment
    {
        public class Amnesty
        {
            [JsonProperty(PropertyName = "D")]
            public DateTime Date { get; set; }

            [JsonProperty(PropertyName = "R")]
            public string Reason { get; set; }

            [JsonProperty(PropertyName = "CID")]
            public uint CID { get; set; }
        }
    }
}