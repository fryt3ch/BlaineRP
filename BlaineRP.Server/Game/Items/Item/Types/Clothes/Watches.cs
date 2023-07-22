using Newtonsoft.Json;
using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public class Watches : Clothes, Clothes.IProp
    {
        new public class ItemData : Clothes.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, 0.1f, "prop_jewel_02b", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region ItemData Male
            { "watches_m_0", new ItemData("Спортивные часы", true, 1, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "watches_m_1", new ItemData("Классические часы", true, 3, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "watches_m_2", new ItemData("Спортивные часы #2", true, 5, new int[] { 0, 1, 2, 3 }, null) },
            { "watches_m_3", new ItemData("Часы угловатые", true, 7, new int[] { 0, 1, 2 }, null) },
            { "watches_m_4", new ItemData("Часы с механизмом кинетик", true, 10, new int[] { 0, 1, 2 }, null) },
            { "watches_m_5", new ItemData("Часы обычные", true, 12, new int[] { 0, 1, 2 }, null) },
            { "watches_m_6", new ItemData("Часы с картинками", true, 13, new int[] { 0, 1, 2 }, null) },
            { "watches_m_7", new ItemData("Часы аккуратные", true, 14, new int[] { 0, 1, 2 }, null) },
            { "watches_m_8", new ItemData("Часы с большим циферблатом", true, 15, new int[] { 0, 1, 2 }, null) },
            { "watches_m_9", new ItemData("Часы с механизмом кинетик #2", true, 20, new int[] { 0, 1, 2 }, null) },
            { "watches_m_10", new ItemData("Часы смарт", true, 21, new int[] { 0, 1, 2 }, null) },
            { "watches_m_11", new ItemData("Часы квадратные", true, 36, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_m_12", new ItemData("Механические часы", true, 0, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "watches_m_13", new ItemData("Хронометр", true, 6, new int[] { 0, 1, 2 }, null) },
            { "watches_m_14", new ItemData("Хронограф", true, 8, new int[] { 0, 1, 2 }, null) },
            { "watches_m_15", new ItemData("Часы двойные", true, 9, new int[] { 0, 1, 2 }, null) },
            { "watches_m_16", new ItemData("Хронограф #2", true, 11, new int[] { 0, 1, 2 }, null) },
            { "watches_m_17", new ItemData("Часы с большим циферблатом #2", true, 16, new int[] { 0, 1, 2 }, null) },
            { "watches_m_18", new ItemData("Часы необычные #2", true, 17, new int[] { 0, 1, 2 }, null) },
            { "watches_m_19", new ItemData("Механические часы #2", true, 18, new int[] { 0, 1, 2 }, null) },
            { "watches_m_20", new ItemData("Хронограф #3", true, 19, new int[] { 0, 1, 2 }, null) },
            { "watches_m_21", new ItemData("Часы солидные", true, 30, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_m_22", new ItemData("MIDO", true, 31, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_m_23", new ItemData("Tudor", true, 32, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "watches_m_24", new ItemData("Longines", true, 34, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_m_25", new ItemData("TAG", true, 35, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            #endregion

            #region ItemData Female
            { "watches_f_0", new ItemData("Diesel", false, 3, new int[] { 0, 1, 2 }, null) },
            { "watches_f_1", new ItemData("Skagen", false, 4, new int[] { 0, 1, 2 }, null) },
            { "watches_f_2", new ItemData("Часы шестиугольные", false, 5, new int[] { 0, 1, 2 }, null) },
            { "watches_f_3", new ItemData("Часы с механизмом кинетик", false, 6, new int[] { 0, 1, 2 }, null) },
            { "watches_f_4", new ItemData("Часы спортивные", false, 8, new int[] { 0, 1, 2 }, null) },
            { "watches_f_5", new ItemData("Часы круглые", false, 24, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_f_6", new ItemData("Часы аккуратные", false, 25, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_f_7", new ItemData("Часы с большим циферблатом", false, 26, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_f_8", new ItemData("Часы позолоченные", false, 2, new int[] { 0, 1, 2, 3 }, null) },
            { "watches_f_9", new ItemData("Seiko", false, 7, new int[] { 0, 1, 2 }, null) },
            { "watches_f_10", new ItemData("Часы позолоченные #2", false, 9, new int[] { 0, 1, 2 }, null) },
            { "watches_f_11", new ItemData("Часы солидные", false, 19, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_f_12", new ItemData("MIDO", false, 20, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_f_13", new ItemData("Tudor", false, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "watches_f_14", new ItemData("Longines", false, 23, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_f_15", new ItemData("Часы круглые", false, 24, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            #endregion
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

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

            player.SetAccessories(Slot, data.Drawable, data.Textures[variation]);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.ClearAccessory(Slot);
        }

        public Watches(string ID, int Variation) : base(ID, IDList[ID], typeof(Watches), Variation)
        {

        }
    }
}
