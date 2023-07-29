using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Items
{
    /// <summary>Этот интерфейс реализуют классы таких предметов, которые могут хранить в себе другие предметы</summary>
    public interface IContainer
    {
        /// <summary>Предметы в контейнере</summary>
        /// <value>Массив объектов класса Game.Items.Item, в котором null - пустой слот</value>
        public Item[] Items { get; set; }

        /// <summary>Вес контейнера</summary>
        [JsonIgnore]
        public float Weight { get; }
    }
}