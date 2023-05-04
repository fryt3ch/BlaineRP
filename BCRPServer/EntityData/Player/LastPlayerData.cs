using Newtonsoft.Json;

namespace BCRPServer
{
    public partial class PlayerData
    {
        public class LastPlayerData
        {
            [JsonProperty(PropertyName = "D")]
            /// <summary>Последнее измерение</summary>
            public uint Dimension { get; set; }

            [JsonProperty(PropertyName = "L")]
            /// <summary>Последние координаты</summary>
            public Utils.Vector4 Position { get; set; }

            [JsonProperty(PropertyName = "H")]
            /// <summary>Последнее здоровье</summary>
            public int Health { get; set; }

            [JsonProperty(PropertyName = "ST")]
            /// <summary>Время в секундах, наигранное за последнюю сессию</summary>
            /// <remarks>Обнуляется каждый час</remarks>
            public int SessionTime { get; set; }

            [JsonProperty(PropertyName = "S")]
            public int Satiety { get; set; }

            [JsonProperty(PropertyName = "M")]
            public int Mood { get; set; }

            public void UpdatePosition(Utils.Vector4 position, uint dimension, bool updateDb)
            {
                Position = position;

                Dimension = dimension;
            }
        }
    }
}
