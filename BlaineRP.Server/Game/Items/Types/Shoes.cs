using Newtonsoft.Json;
using System.Collections.Generic;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Players.Customization;
using BlaineRP.Server.Game.EntitiesData.Players.Customization.Clothes;

namespace BlaineRP.Server.Game.Items
{
    public partial class Shoes : Clothes
    {
        public new class ItemData : Clothes.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeId == null ? "null" : $"\"{SexAlternativeId}\"")}";

            public ItemData(string name, bool sex, int drawable, int[] textures, string sexAlternativeId = null) : base(name, 0.3f, "prop_ld_shoe_01", sex, drawable, textures, sexAlternativeId) { }
        }

        [JsonIgnore]
        public new ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        public new ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; }

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

            player.SetClothes(Slot, Service.GetNudeDrawable(ClothesTypes.Shoes, pData.Sex), 0);
        }

        public Shoes(string id, int variation) : base(id, IdList[id], typeof(Shoes), variation)
        {

        }
    }
}
