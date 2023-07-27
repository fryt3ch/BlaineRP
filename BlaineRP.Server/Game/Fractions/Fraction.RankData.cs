using System.Collections.Generic;
using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Fractions
{
    public abstract partial class Fraction
    {
        public class RankData
        {
            [JsonProperty(PropertyName = "N")]
            public string Name { get; set; }

            /// <summary>Стандарные Id прав для всех фракций</summary>
            /// <remarks>0 - Доступ к складу, 1 - Приглашение, 2 - Повышение/понижение<br/>3 - Увольнение, 4 - Респавн транспорта</remarks>
            [JsonProperty(PropertyName = "P")]
            public Dictionary<uint, byte> Permissions { get; set; }

            [JsonProperty(PropertyName = "S")]
            public uint Salary { get; set; }
        }
    }
}