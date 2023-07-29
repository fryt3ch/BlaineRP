using Newtonsoft.Json;
using System;
using BlaineRP.Server.EntitiesData.Players;

namespace BlaineRP.Server.Game.Items
{
    public abstract class StatusChanger : Item
    {
        new public abstract class ItemData : Item.ItemData
        {
            public int Satiety { get; set; }

            public int Mood { get; set; }

            public int Health { get; set; }

            public ItemData(string Name, float Weight, string[] Models, int Satiety = 0, int Mood = 0, int Health = 0) : base(Name, Weight, Models)
            {
                this.Satiety = Satiety;

                this.Mood = Mood;

                this.Health = Health;
            }
        }

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        public abstract void Apply(PlayerData pData);

        public StatusChanger(string ID, Item.ItemData Data, Type Type) : base(ID, Data, Type)
        {

        }
    }
}
