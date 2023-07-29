using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Game.Items
{
    public partial class Holster : Clothes, IContainer
    {
        public new class ItemData : Clothes.ItemData, Item.ItemData.IContainer
        {
            public int DrawableWeapon { get; set; }

            public byte MaxSlots { get; } = 1;

            public float MaxWeight { get; } = float.MaxValue;

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeId == null ? "null" : $"\"{SexAlternativeId}\"")}";

            public ItemData(string name, bool sex, int drawable, int[] textures, int drawableWeapon, string sexAlternativeId = null) : base(name, 0.15f, "prop_holster_01", sex, drawable, textures, sexAlternativeId)
            {
                DrawableWeapon = drawableWeapon;
            }
        }

        [JsonIgnore]
        public new ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        public new ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; }

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

        public Holster(string id, int variation = 0) : base(id, IdList[id], typeof(Holster), variation)
        {
            Items = new Item[Data.MaxSlots];
        }
    }
}
