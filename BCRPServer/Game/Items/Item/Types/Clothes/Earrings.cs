using BCRPServer.Game.Data;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BCRPServer.Game.Items
{
    public class Earrings : Clothes, Clothes.IProp
    {
        new public class ItemData : Clothes.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, 0.01f, "p_tmom_earrings_s", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region ItemData Male
            { "ears_m_0", new ItemData("Солитер (левый)", true, 3, new int[] { 0, 1, 2 }, null) },
            { "ears_m_1", new ItemData("Солитер (правый)", true, 4, new int[] { 0, 1, 2 }, null) },
            { "ears_m_2", new ItemData("Солитер (оба)", true, 5, new int[] { 0, 1, 2 }, null) },
            { "ears_m_3", new ItemData("Гвоздники (левый)", true, 6, new int[] { 0, 1 }, null) },
            { "ears_m_4", new ItemData("Гвоздники (правый)", true, 7, new int[] { 0, 1 }, null) },
            { "ears_m_5", new ItemData("Гвоздники (оба)", true, 8, new int[] { 0, 1 }, null) },
            { "ears_m_6", new ItemData("Diamond (левый)", true, 9, new int[] { 0, 1, 2 }, null) },
            { "ears_m_7", new ItemData("Diamond (правый)", true, 10, new int[] { 0, 1, 2 }, null) },
            { "ears_m_8", new ItemData("Diamond (оба)", true, 11, new int[] { 0, 1, 2 }, null) },
            { "ears_m_9", new ItemData("Ромб (левый)", true, 12, new int[] { 0, 1, 2 }, null) },
            { "ears_m_10", new ItemData("Ромб (правый)", true, 13, new int[] { 0, 1, 2 }, null) },
            { "ears_m_11", new ItemData("Ромб (оба)", true, 14, new int[] { 0, 1, 2 }, null) },
            { "ears_m_12", new ItemData("Кнопка (левый)", true, 15, new int[] { 0, 1, 2 }, null) },
            { "ears_m_13", new ItemData("Кнопка (правый)", true, 16, new int[] { 0, 1, 2 }, null) },
            { "ears_m_14", new ItemData("Кнопка (оба)", true, 17, new int[] { 0, 1, 2 }, null) },
            { "ears_m_15", new ItemData("Квадрат платиновый (левый)", true, 18, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "ears_m_16", new ItemData("Квадрат платиновый (правый)", true, 19, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "ears_m_17", new ItemData("Квадрат платиновый (оба)", true, 20, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "ears_m_18", new ItemData("Серьги NS (левый)", true, 21, new int[] { 0, 1 }, null) },
            { "ears_m_19", new ItemData("Серьги NS (правый)", true, 22, new int[] { 0, 1 }, null) },
            { "ears_m_20", new ItemData("Серьги NS (оба)", true, 23, new int[] { 0, 1 }, null) },
            { "ears_m_21", new ItemData("Череп (левый)", true, 24, new int[] { 0, 1, 2, 3 }, null) },
            { "ears_m_22", new ItemData("Череп (правый)", true, 25, new int[] { 0, 1, 2, 3 }, null) },
            { "ears_m_23", new ItemData("Череп (оба)", true, 26, new int[] { 0, 1, 2, 3 }, null) },
            { "ears_m_24", new ItemData("Острый цилиндр (левый)", true, 27, new int[] { 0, 1 }, null) },
            { "ears_m_25", new ItemData("Острый цилиндр (правый)", true, 28, new int[] { 0, 1 }, null) },
            { "ears_m_26", new ItemData("Острый цилиндр (оба)", true, 29, new int[] { 0, 1 }, null) },
            { "ears_m_27", new ItemData("Черный сапфир (левый)", true, 30, new int[] { 0, 1, 2 }, null) },
            { "ears_m_28", new ItemData("Серьги NS (левый)", true, 21, new int[] { 0, 1 }, null) },
            { "ears_m_29", new ItemData("Черный сапфир (оба)", true, 32, new int[] { 0, 1, 2 }, null) },
            { "ears_m_30", new ItemData("Позолоченный NS (левый)", true, 33, new int[] { 0 }, null) },
            { "ears_m_31", new ItemData("Позолоченный NS (правый)", true, 34, new int[] { 0, 1 }, null) },
            { "ears_m_32", new ItemData("Позолоченный NS (оба)", true, 35, new int[] { 0, 1 }, null) },
            { "ears_m_33", new ItemData("Микрофоны (оба)", true, 37, new int[] { 0, 1 }, "ears_f_14") },
            { "ears_m_34", new ItemData("Карты (оба)", true, 38, new int[] { 0, 1, 2, 3 }, "ears_f_15") },
            { "ears_m_35", new ItemData("Игральные кости (оба)", true, 39, new int[] { 0, 1, 2, 3 }, "ears_f_16") },
            { "ears_m_36", new ItemData("Игральные фишки (оба)", true, 40, new int[] { 0, 1, 2, 3 }, "ears_f_17") },
            #endregion

            #region ItemData Female
            { "ears_f_0", new ItemData("Шандельеры", false, 3, new int[] { 0 }, null) },
            { "ears_f_1", new ItemData("Конго", false, 4, new int[] { 0 }, null) },
            { "ears_f_2", new ItemData("Серьги-жирандоль", false, 5, new int[] { 0 }, null) },
            { "ears_f_3", new ItemData("Серьги-протяжки", false, 6, new int[] { 0, 1, 2 }, null) },
            { "ears_f_4", new ItemData("Кластеры", false, 7, new int[] { 0, 1, 2 }, null) },
            { "ears_f_5", new ItemData("Петли", false, 8, new int[] { 0, 1, 2 }, null) },
            { "ears_f_6", new ItemData("Цепочки", false, 9, new int[] { 0, 1, 2 }, null) },
            { "ears_f_7", new ItemData("Петли с камнем", false, 10, new int[] { 0, 1, 2 }, null) },
            { "ears_f_8", new ItemData("Петли снежинки", false, 11, new int[] { 0, 1, 2 }, null) },
            { "ears_f_9", new ItemData("Кнопки", false, 12, new int[] { 0, 1, 2 }, null) },
            { "ears_f_10", new ItemData("Калаши", false, 13, new int[] { 0 }, null) },
            { "ears_f_11", new ItemData("Кольца переплет", false, 14, new int[] { 0 }, null) },
            { "ears_f_12", new ItemData("Кольца", false, 15, new int[] { 0 }, null) },
            { "ears_f_13", new ItemData("Кольца FY", false, 16, new int[] { 0 }, null) },
            { "ears_f_14", new ItemData("Микрофоны", false, 18, new int[] { 0, 1 }, "ears_m_33") },
            { "ears_f_15", new ItemData("Карты", false, 19, new int[] { 0, 1, 2, 3 }, "ears_m_34") },
            { "ears_f_16", new ItemData("Игральные кости", false, 20, new int[] { 0, 1, 2, 3 }, "ears_m_35") },
            { "ears_f_17", new ItemData("Игральные фишки", false, 21, new int[] { 0, 1, 2, 3 }, "ears_m_36") },
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

        public Earrings(string ID, int Variation) : base(ID, IDList[ID], typeof(Earrings), Variation)
        {

        }
    }
}
