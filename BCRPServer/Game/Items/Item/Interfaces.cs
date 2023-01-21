using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Items
{
    public interface IUsable
    {
        [JsonIgnore]
        public bool InUse { get; set; }

        public void StartUse(PlayerData pData, Inventory.Groups group, int slot, bool needUpdate, params object[] args);

        public void StopUse(PlayerData pData, Inventory.Groups group, int slot, bool needUpdate, params object[] args);
    }

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

    /// <summary>Этот интерфейс реализуют классы таких предметов, которые могут надеваться на игрока</summary>
    public interface IWearable
    {
        /// <summary>Метод для того, чтобы надеть предмет на игрока</summary>
        /// <param name="pData">Сущность игрока</param>
        void Wear(PlayerData pData);

        /// <summary>Метод для того, чтобы снять предмет с игрока</summary>
        /// <param name="pData">Сущность игрока</param>
        void Unwear(PlayerData pData);
    }

    /// <summary>Этот интерфейс реализуют классы таких предметов, которые способны стакаться</summary>
    public interface IStackable
    {
        /// <summary>Максимальное кол-во единиц предмета в стаке</summary>
        [JsonIgnore]
        public int MaxAmount { get; }

        /// <summary>Общий вес стака</summary>
        [JsonIgnore]
        public float Weight { get; }

        [JsonProperty(PropertyName = "A")]
        /// <summary>Кол-во единиц предмета в стаке</summary>
        public int Amount { get; set; }
    }

    /// <summary>Этот интерфейс реализуют классы таких предметов, которые, помимо названия, имеют уникальный тэг</summary>
    public interface ITagged
    {
        [JsonProperty(PropertyName = "T")]
        /// <summary>Тэг</summary>
        public string Tag { get; set; }
    }

    /// <summary>Этот интерфейс реализуют классы таких предметов, которые способны тратиться</summary>
    /// <remarks>Не использовать одновременно с IStackable!</remarks>
    public interface IConsumable
    {
        /// <summary>Максимальное кол-во единиц предмета</summary>
        [JsonIgnore]
        public int MaxAmount { get; }

        [JsonProperty(PropertyName = "A")]
        /// <summary>Кол-во оставшихся единиц предмета</summary>
        public int Amount { get; set; }
    }

    public interface ICraftIngredient
    {

    }
}
