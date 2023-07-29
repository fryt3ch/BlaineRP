using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Game.Items
{
    public partial class Bag : Clothes, IContainer
    {
        public new class ItemData : Clothes.ItemData, Item.ItemData.IContainer
        {
            /// <summary>Максимальное кол-во слотов</summary>
            public byte MaxSlots { get; set; }

            /// <summary>Максимальный вес содержимого</summary>
            public float MaxWeight { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {MaxSlots}, {MaxWeight}, {(SexAlternativeId == null ? "null" : $"\"{SexAlternativeId}\"")}";

            public ItemData(string name, bool sex, int drawable, int[] textures, byte maxSlots, float maxWeight, string sexAlternativeId = null) : base(name, 0.25f, "prop_cs_heist_bag_01", sex, drawable, textures, sexAlternativeId)
            {
                MaxSlots = maxSlots;

                MaxWeight = maxWeight;
            }
        }

        [JsonIgnore]
        public new ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        public new ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; }

        /// <summary>Предметы внутри</summary>
        [JsonIgnore]
        public Item[] Items { get; set; }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            if (pData.Items.Where(x => (x as Parachute)?.InUse == true).Any()) // pData.BeltOn ||
                return;

            var data = Data;

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            player.SetClothes(Slot, data.Drawable, data.Textures[variation]);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            if (pData.Items.Where(x => (x as Parachute)?.InUse == true).Any()) // pData.BeltOn ||
                return;

            player.SetClothes(Slot, 0, 0);
        }

        /// <summary>Итоговый вес</summary>
        /// <remarks>Включает в себя вес самой сумки!</remarks>
        [JsonIgnore]
        public override float Weight { get => BaseWeight + Items.Sum(x => x?.Weight ?? 0f); }

        public Bag(string id, int variation = 0) : base(id, IdList[id], typeof(Bag), variation)
        {
            Items = new Item[Data.MaxSlots];
        }
    }
}
