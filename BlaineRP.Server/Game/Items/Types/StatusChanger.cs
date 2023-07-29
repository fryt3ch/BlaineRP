using Newtonsoft.Json;
using System;
using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Game.Items
{
    public abstract class StatusChanger : Item
    {
        public new abstract class ItemData : Item.ItemData
        {
            public int Satiety { get; set; }

            public int Mood { get; set; }

            public int Health { get; set; }

            public ItemData(string name, float weight, string[] models, int satiety = 0, int mood = 0, int health = 0) : base(name, weight, models)
            {
                Satiety = satiety;

                Mood = mood;

                Health = health;
            }
        }

        [JsonIgnore]
        public new ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        public abstract void Apply(PlayerData pData);

        public StatusChanger(string id, Item.ItemData data, Type type) : base(id, data, type)
        {

        }
    }
}
