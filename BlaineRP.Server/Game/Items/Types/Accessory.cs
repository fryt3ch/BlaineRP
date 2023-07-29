using Newtonsoft.Json;
using System.Collections.Generic;
using BlaineRP.Server.EntitiesData.Players;

namespace BlaineRP.Server.Game.Items
{
    public class Accessory : Clothes
    {
        new public class ItemData : Clothes.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, 0.2f, "p_jewel_necklace_02", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region Accessories Male
            { "accs_m_0", new ItemData("Цепочка Бисмарк", true, 16, new int[] { 0, 1, 2 }, null) },
            { "accs_m_1", new ItemData("Цепочка Бисмарк #2", true, 17, new int[] { 0, 1, 2 }, null) },
            { "accs_m_2", new ItemData("Шарф", true, 30, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "accs_m_3", new ItemData("Цепочка с балаклавой", true, 44, new int[] { 0 }, null) },
            { "accs_m_4", new ItemData("Цепочка с якорным плетением", true, 74, new int[] { 0, 1 }, null) },
            { "accs_m_5", new ItemData("Цепь плетение Фигаро", true, 85, new int[] { 0, 1 }, null) },
            { "accs_m_6", new ItemData("Цепь плетение Ролло", true, 87, new int[] { 0, 1 }, null) },
            { "accs_m_7", new ItemData("Цепь M", true, 110, new int[] { 0, 1 }, null) },
            { "accs_m_8", new ItemData("Шарф Арафатка", true, 112, new int[] { 0, 1, 2 }, null) },
            { "accs_m_9", new ItemData("Наушники", true, 114, new int[] { 0 }, null) },
            { "accs_m_10", new ItemData("Цепь с колесом", true, 119, new int[] { 0, 1 }, null) },
            { "accs_m_11", new ItemData("Наушники #2", true, 124, new int[] { 0, 1 }, null) },
            { "accs_m_12", new ItemData("Галстук пионера", true, 151, new int[] { 0 }, null) },
            { "accs_m_13", new ItemData("Галстук Виндзор", true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_m_14", new ItemData("Бабочка", true, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_m_15", new ItemData("Узкий галстук", true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_m_16", new ItemData("Галстук Виндзор длинный", true, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_m_17", new ItemData("Галстук Виндзор узкий", true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_m_18", new ItemData("Бабочка цветная", true, 32, new int[] { 0, 1, 2 }, null) },
            { "accs_m_19", new ItemData("Галстук Регат", true, 37, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_m_20", new ItemData("Галстук обычный", true, 38, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_m_21", new ItemData("Галстук Регат удлиненный", true, 39, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_m_22", new ItemData("Аккуратная бабочка", true, 118, new int[] { 0 }, null) },
            #endregion

            #region Accessories Female
            { "accs_f_0", new ItemData("Шарф Арафатка", false, 83, new int[] { 0, 1, 2 }, null) },
            { "accs_f_1", new ItemData("Шарф Арафатка #2", false, 9, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "accs_f_2", new ItemData("Шарф Арафатка #3", false, 15, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "accs_f_3", new ItemData("Наушники", false, 85, new int[] { 0 }, null) },
            { "accs_f_4", new ItemData("Наушники #2", false, 94, new int[] { 0, 1 }, null) },
            { "accs_f_5", new ItemData("Галстук пионера", false, 120, new int[] { 0 }, null) },
            { "accs_f_6", new ItemData("Бусы длинные", false, 12, new int[] { 0, 1, 2 }, null) },
            { "accs_f_7", new ItemData("Галстук Французский", false, 13, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "accs_f_8", new ItemData("Галстук Stillini", false, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_f_9", new ItemData("Строгий галстук", false, 21, new int[] { 0, 1, 2 }, null) },
            { "accs_f_10", new ItemData("Строгий дизайнерский галстук", false, 22, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_f_11", new ItemData("Бабочка", false, 23, new int[] { 0, 1, 2 }, null) },
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

            player.SetClothes(Slot, data.Drawable, data.Textures[variation]);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.SetClothes(Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Accessory, pData.Sex), 0);
        }

        public Accessory(string ID, int Variation) : base(ID, IDList[ID], typeof(Accessory), Variation)
        {

        }
    }
}
