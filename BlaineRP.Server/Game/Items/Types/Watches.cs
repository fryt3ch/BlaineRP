using Newtonsoft.Json;
using System.Collections.Generic;
using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Game.Items
{
    public partial class Watches : Clothes, Clothes.IProp
    {
        public new class ItemData : Clothes.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeId == null ? "null" : $"\"{SexAlternativeId}\"")}";

            public ItemData(string name, bool sex, int drawable, int[] textures, string sexAlternativeId = null) : base(name, 0.1f, "prop_jewel_02b", sex, drawable, textures, sexAlternativeId) { }
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

            player.SetAccessories(Slot, data.Drawable, data.Textures[variation]);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.ClearAccessory(Slot);
        }

        public Watches(string id, int variation) : base(id, IdList[id], typeof(Watches), variation)
        {

        }
    }
}
