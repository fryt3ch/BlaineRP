using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Items
{
    /// <summary>Этот интерфейс реализуют классы таких предметов, которые способны тратиться</summary>
    /// <remarks>Не использовать одновременно с IStackable!</remarks>
    public interface IConsumable
    {
        /// <summary>Максимальное кол-во единиц предмета</summary>
        [JsonIgnore]
        public int MaxAmount { get; }

        [JsonProperty(PropertyName = "A", Order = int.MinValue + 1)]
        /// <summary>Кол-во оставшихся единиц предмета</summary>
        public int Amount { get; set; }
    }
}