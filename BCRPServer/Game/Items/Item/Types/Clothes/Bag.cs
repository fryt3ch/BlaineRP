using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Items
{
    public class Bag : Clothes, IContainer
    {
        new public class ItemData : Clothes.ItemData
        {
            /// <summary>Максимальное кол-во слотов</summary>
            public byte MaxSlots { get; set; }

            /// <summary>Максимальный вес содержимого</summary>
            public float MaxWeight { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {MaxSlots}, {MaxWeight}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, byte MaxSlots, float MaxWeight, string SexAlternativeID = null) : base(Name, 0.25f, "prop_cs_heist_bag_01", Sex, Drawable, Textures, SexAlternativeID)
            {
                this.MaxSlots = MaxSlots;

                this.MaxWeight = MaxWeight;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "bag_m_0", new ItemData("Обычная сумка", true, 81, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 10, 5f, "bag_f_0") },
            { "bag_m_1", new ItemData("Большая сумка", true, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 15, 10f, "bag_f_0") },

            { "bag_m_2", new ItemData("Сумка BIGNESS", true, 85, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, 10, 5f, "bag_f_0") },
            { "bag_m_3", new ItemData("Сумка BIGNESS (большая)", true, 86, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, 15, 10f, "bag_f_0") },

            { "bag_m_4", new ItemData("Сумка Hinterland", true, 40, new int[] { 0 }, 10, 5f, "bag_f_0") },
            { "bag_m_5", new ItemData("Сумка Hinterland (большая)", true, 41, new int[] { 0 }, 15, 10f, "bag_f_0") },

            { "bag_m_6", new ItemData("Обычная сумка #2", true, 44, new int[] { 0 }, 10, 5f, "bag_f_0") },
            { "bag_m_7", new ItemData("Большая сумка #2", true, 45, new int[] { 0 }, 15, 10f, "bag_f_0") },

            { "bag_f_0", new ItemData("Обычная сумка", false, 81, new int[] { 0, 1, 2, 3, 4, 5 }, 10, 5f, "bag_m_0") },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; }

        /// <summary>Предметы внутри</summary>
        [JsonIgnore]
        public Item[] Items { get; set; }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            if (pData.Items.Where(x => (x as Game.Items.Parachute)?.InUse == true).Any()) // pData.BeltOn ||
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

            if (pData.Items.Where(x => (x as Game.Items.Parachute)?.InUse == true).Any()) // pData.BeltOn ||
                return;

            player.SetClothes(Slot, 0, 0);
        }

        /// <summary>Итоговый вес</summary>
        /// <remarks>Включает в себя вес самой сумки!</remarks>
        [JsonIgnore]
        public override float Weight { get => BaseWeight + Items.Sum(x => x?.Weight ?? 0f); }

        public Bag(string ID, int Variation = 0) : base(ID, IDList[ID], typeof(Bag), Variation)
        {
            this.Items = new Item[Data.MaxSlots];
        }
    }
}
