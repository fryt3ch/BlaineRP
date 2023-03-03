using Newtonsoft.Json;
using System.Collections.Generic;

namespace BCRPServer.Game.Items
{
    public class Mask : Clothes
    {
        new public class ItemData : Clothes.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, 0.15f, "prop_mask_specops", Sex, Drawable, Textures, SexAlternativeID)
            {

            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "mask_m_0", new ItemData("Маска свиньи", true, 1, new int[] { 0, 1, 2, 3 }, "mask_f_0") },
            { "mask_m_1", new ItemData("Маска 'Череп'", true, 2, new int[] { 0, 1, 2, 3 }, "mask_f_1") },
            { "mask_m_2", new ItemData("Маска обезьяны #1", true, 3, new int[] { 0 }, "mask_f_1") },
            { "mask_m_3", new ItemData("Маска хоккеиста #1", true, 4, new int[] { 0, 1, 2, 3 }, "mask_f_1") },
            { "mask_m_4", new ItemData("Маска обезьяны #2", true, 5, new int[] { 0, 1, 2, 3 }, "mask_f_1") },
            { "mask_m_5", new ItemData("Маска карнавальная #1", true, 6, new int[] { 0, 1, 2, 3 }, "mask_f_1") },
            { "mask_m_6", new ItemData("Маска орка", true, 7, new int[] { 0, 1, 2, 3 }, "mask_f_1") },
            { "mask_m_7", new ItemData("Маска Санты", true, 8, new int[] { 0, 1, 2 }, "mask_f_1") },
            { "mask_m_8", new ItemData("Маска оленя", true, 9, new int[] { 0 }, "mask_f_1") },
            { "mask_m_9", new ItemData("Маска снеговика", true, 10, new int[] { 0 }, "mask_f_1") },
            { "mask_m_10", new ItemData("Маска карнавальная #2", true, 11, new int[] { 0, 1, 2 }, "mask_f_1") },
            { "mask_m_11", new ItemData("Маска карнавальная #3", true, 12, new int[] { 0, 1, 2 }, "mask_f_1") },
            { "mask_m_13", new ItemData("Маска купидона", true, 13, new int[] { 0 }, "mask_f_1") },
            { "mask_m_14", new ItemData("Маска хоккеиста #2", true, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, "mask_f_1") },
            { "mask_m_15", new ItemData("Маска хоккеиста #3", true, 15, new int[] { 0, 1, 2, 3 }, "mask_f_1") },
            { "mask_m_16", new ItemData("Маска воина", true, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, "mask_f_1") },
            { "mask_m_17", new ItemData("Маска котика", true, 17, new int[] { 0, 1 }, "mask_f_1") },
            { "mask_m_18", new ItemData("Маска лисы", true, 18, new int[] { 0, 1 }, "mask_f_1") },
            { "mask_m_19", new ItemData("Маска совы", true, 19, new int[] { 0, 1 }, "mask_f_1") },
            { "mask_m_20", new ItemData("Маска енота", true, 20, new int[] { 0, 1 }, "mask_f_1") },
            { "mask_m_21", new ItemData("Маска медведя", true, 21, new int[] { 0, 1 }, "mask_f_1") },
            { "mask_m_22", new ItemData("Маска бизона", true, 22, new int[] { 0, 1 }, "mask_f_1") },
            { "mask_m_23", new ItemData("Маска быка", true, 23, new int[] { 0, 1 }, "mask_f_1") },
            { "mask_m_24", new ItemData("Маска орла", true, 24, new int[] { 0, 1 }, "mask_f_1") },
            { "mask_m_25", new ItemData("Маска стервятника", true, 25, new int[] { 0, 1 }, "mask_f_1") },
            { "mask_m_26", new ItemData("Маска волка", true, 26, new int[] { 0, 1 }, "mask_f_1") },
            { "mask_m_27", new ItemData("Боевая маска", true, 28, new int[] { 0, 1, 2, 3, 4 }, "mask_f_1") },
            { "mask_m_28", new ItemData("Маска скелета", true, 29, new int[] { 0, 1, 2, 3, 4 }, "mask_f_1") },

            { "mask_f_0", new ItemData("Маска свиньи", false, 1, new int[] { 0 }, "mask_m_0") },
            { "mask_f_1", new ItemData("Маска 'Череп'", false, 2, new int[] { 0 }, "mask_m_1") },
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

            player.SetClothes(Slot, 0, 0);
        }

        public Mask(string ID, int Variation = 0) : base(ID, IDList[ID], typeof(Mask), Variation)
        {

        }
    }
}
