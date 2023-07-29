using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Items
{
    /// <summary>Этот интерфейс реализуют классы таких предметов, которые способны стакаться</summary>
    public interface IStackable
    {
        /// <summary>Максимальное кол-во единиц предмета в стаке</summary>
        [JsonIgnore]
        public int MaxAmount { get; }

        /// <summary>Общий вес стака</summary>
        [JsonIgnore]
        public float Weight { get; }

        [JsonProperty(PropertyName = "A", Order = int.MinValue + 1)]
        /// <summary>Кол-во единиц предмета в стаке</summary>
        public int Amount { get; set; }
    }
}