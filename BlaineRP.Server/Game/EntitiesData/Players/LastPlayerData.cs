using System;
using BlaineRP.Server.UtilsT;
using Newtonsoft.Json;

namespace BlaineRP.Server.EntitiesData.Players
{
    public class LastPlayerData
    {
        [JsonProperty(PropertyName = "D")]
        /// <summary>Последнее измерение</summary>
        public uint Dimension { get; set; }

        [JsonProperty(PropertyName = "L")]
        /// <summary>Последние координаты</summary>
        public Vector4 Position { get; set; }

        [JsonProperty(PropertyName = "H")]
        /// <summary>Последнее здоровье</summary>
        public int Health { get; set; }

        [JsonProperty(PropertyName = "ST")]
        /// <summary>Время, наигранное за последнюю сессию</summary>
        /// <remarks>Обнуляется каждый час</remarks>
        public TimeSpan SessionTime { get; set; }

        [JsonProperty(PropertyName = "S")]
        public byte Satiety { get; set; }

        [JsonProperty(PropertyName = "M")]
        public byte Mood { get; set; }

        [JsonProperty(PropertyName = "DA")]
        public byte DrugAddiction { get; set; }

        public void UpdatePosition(Vector4 position, uint dimension, bool updateDb)
        {
            Position = position;

            Dimension = dimension;
        }
    }
}
