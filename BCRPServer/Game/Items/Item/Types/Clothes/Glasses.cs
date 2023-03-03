using BCRPServer.Game.Data;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BCRPServer.Game.Items
{
    public class Glasses : Clothes, Clothes.IProp
    {
        new public class ItemData : Clothes.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, 0.2f, "prop_cs_sol_glasses", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region ItemData Male
            { "glasses_m_0", new ItemData("Спортивные очки", true, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_1", new ItemData("Очки Панто", true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_2", new ItemData("Спортивные очки #2", true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_3", new ItemData("Прямоугольные очки", true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_4", new ItemData("Очки Авиаторы обычные", true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_5", new ItemData("Спортивные очки #3", true, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_6", new ItemData("Очки обычные", true, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "glasses_m_7", new ItemData("Очки Авиаторы обычные #2", true, 18, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_8", new ItemData("Очки USA", true, 21, new int[] { 0 }, null) },
            { "glasses_m_9", new ItemData("Очки USA #2", true, 22, new int[] { 0 }, null) },
            { "glasses_m_10", new ItemData("Спортивные очки", true, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_11", new ItemData("Browline", true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_12", new ItemData("Авиаторы", true, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_13", new ItemData("Wayfarer", true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_14", new ItemData("Авиаторы #2", true, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_15", new ItemData("Прямоугольные очки #2", true, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_16", new ItemData("Прямоугольные очки #3", true, 17, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_17", new ItemData("Wayfarer #2", true, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_18", new ItemData("Авиаторы защищенные", true, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "glasses_m_19", new ItemData("Очки модника", true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, null) },
            { "glasses_m_20", new ItemData("Очки неоновые", true, 30, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "glasses_m_21", new ItemData("Очки клубные", true, 31, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "glasses_m_22", new ItemData("Очки современные", true, 32, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "glasses_m_23", new ItemData("Очки модника #2", true, 33, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            #endregion

            #region ItemData Female
            { "glasses_f_0", new ItemData("Прямоугольные очки", false, 0, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_1", new ItemData("Спортивные очки", false, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_2", new ItemData("Круглые очки", false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_3", new ItemData("Очки-кошки", false, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_4", new ItemData("Овальные очки", false, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_5", new ItemData("Спортивные очки #2", false, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_6", new ItemData("Очки USA", false, 22, new int[] { 0 }, null) },
            { "glasses_f_7", new ItemData("Очки USA #2", false, 23, new int[] { 0 }, null) },
            { "glasses_f_8", new ItemData("Очки переплетающиеся", false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_9", new ItemData("Очки строгие", false, 18, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_10", new ItemData("Очки строгие #2", false, 19, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_11", new ItemData("Очки DS", false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_12", new ItemData("Очки DS #2", false, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "glasses_f_13", new ItemData("Авиаторы", false, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "glasses_f_14", new ItemData("Очки-кошки #2", false, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_15", new ItemData("Wayfarer", false, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "glasses_f_16", new ItemData("Wayfarer #2", false, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "glasses_f_17", new ItemData("Wayfarer #3", false, 24, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_18", new ItemData("Прямоугольные очки #2", false, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_19", new ItemData("Овальные очки #2", false, 17, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "glasses_f_20", new ItemData("Авиаторы защищенные", false, 30, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "glasses_f_21", new ItemData("Очки модницы", false, 31, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, null) },
            { "glasses_f_22", new ItemData("Очки неоновые", false, 32, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "glasses_f_23", new ItemData("Очки клубные", false, 33, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "glasses_f_24", new ItemData("Очки современные", false, 34, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "glasses_f_25", new ItemData("Очки модницы #2", false, 35, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
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

        public Glasses(string ID, int Variation) : base(ID, IDList[ID], typeof(Glasses), Variation)
        {

        }
    }
}
