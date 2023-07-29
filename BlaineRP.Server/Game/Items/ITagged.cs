using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Items
{
    /// <summary>Этот интерфейс реализуют классы таких предметов, которые, помимо названия, имеют уникальный тэг</summary>
    public interface ITagged
    {
        [JsonProperty(PropertyName = "T", Order = int.MinValue + 2)]
        /// <summary>Тэг</summary>
        public string Tag { get; set; }
    }
}