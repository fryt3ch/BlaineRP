using Newtonsoft.Json;
using System.Collections.Generic;
using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Game.Items
{
    public partial class Hat : Clothes, Clothes.IToggleable, Clothes.IProp
    {
        public new class ItemData : Clothes.ItemData, Clothes.ItemData.IToggleable
        {
            public ExtraData ExtraData { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(ExtraData == null ? "null" : $"new Hat.ItemData.ExtraData({ExtraData.Drawable}, {ExtraData.BestTorso})")}, {(SexAlternativeId == null ? "null" : $"\"{SexAlternativeId}\"")}";

            public ItemData(string name, bool sex, int drawable, int[] textures, ExtraData extraData = null, string sexAlternativeId = null) : base(name, 0.1f, "prop_proxy_hat_01", sex, drawable, textures, sexAlternativeId)
            {
                ExtraData = extraData;
            }
        }

        [JsonIgnore]
        /// <summary>Переключено ли состояние</summary>
        public bool Toggled { get; set; }

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

            if (data.ExtraData != null)
            {
                if (Toggled)
                {
                    player.SetAccessories(Slot, data.ExtraData.Drawable, data.Textures[variation]);

                    pData.Hat = $"{data.ExtraData.Drawable}|{data.Textures[variation]}";
                }
                else
                {
                    player.SetAccessories(Slot, data.Drawable, data.Textures[variation]);

                    pData.Hat = $"{data.Drawable}|{data.Textures[variation]}";
                }
            }
            else
            {
                player.SetAccessories(Slot, data.Drawable, data.Textures[variation]);

                pData.Hat = $"{data.Drawable}|{data.Textures[variation]}";
            }
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.ClearAccessory(0);

            pData.Hat = null;
        }

        public void Action(PlayerData pData)
        {
            if (Data.ExtraData == null)
                return;

            Toggled = !Toggled;

            Wear(pData);
        }

        public Hat(string id, int variation) : base(id, IdList[id], typeof(Hat), variation)
        {

        }
    }
}
