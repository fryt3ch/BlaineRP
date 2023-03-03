using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Items
{
    public class Holster : Clothes, IContainer
    {
        new public class ItemData : Clothes.ItemData
        {
            public int DrawableWeapon { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, int DrawableWeapon, string SexAlternativeID = null) : base(Name, 0.15f, "prop_holster_01", Sex, Drawable, Textures, SexAlternativeID)
            {
                this.DrawableWeapon = DrawableWeapon;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "hl_m_0", new ItemData("Кобура на ногу", true, 148, new int[] { 0 }, 146, null) },
            { "hl_m_1", new ItemData("Кобура простая", true, 149, new int[] { 0 }, 147, null) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; }

        [JsonIgnore]
        public Item[] Items { get; set; }

        [JsonIgnore]
        public Weapon Weapon => (Weapon)Items[0];

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

            player.SetClothes(Slot, Items[0] == null ? data.Drawable : data.DrawableWeapon, data.Textures[variation]);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.SetClothes(Slot, 0, 0);
        }

        public void WearWeapon(PlayerData pData)
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

            player.SetClothes(Slot, data.DrawableWeapon, data.Textures[variation]);
        }

        public void UnwearWeapon(PlayerData pData)
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

        [JsonIgnore]
        public override float Weight { get => BaseWeight + Items.Sum(x => x?.Weight ?? 0f); }

        public Holster(string ID, int Variation = 0) : base(ID, IDList[ID], typeof(Holster), Variation)
        {
            this.Items = new Item[1];
        }
    }
}
