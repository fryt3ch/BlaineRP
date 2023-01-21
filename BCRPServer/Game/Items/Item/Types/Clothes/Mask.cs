using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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
            { "mask_m_0", new ItemData("Маска свиньи", true, 1, new int[] { 0 }, "mask_f_0") },
            { "mask_m_1", new ItemData("Маска 'Череп'", true, 2, new int[] { 0 }, "mask_f_1") },

            { "mask_f_0", new ItemData("Маска свиньи", false, 1, new int[] { 0 }, "mask_m_0") },
            { "mask_f_1", new ItemData("Маска 'Череп'", false, 2, new int[] { 0 }, "mask_m_1") },
        };

        public const int Slot = 1;

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; set => base.SexAlternativeData = value; }

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
