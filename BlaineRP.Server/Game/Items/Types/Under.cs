using Newtonsoft.Json;
using System.Collections.Generic;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Players.Customization;
using BlaineRP.Server.Game.EntitiesData.Players.Customization.Clothes;

namespace BlaineRP.Server.Game.Items
{
    public partial class Under : Clothes, Clothes.IToggleable
    {
        public new class ItemData : Clothes.ItemData, Clothes.ItemData.IToggleable
        {
            public Top.ItemData BestTop { get; set; }

            public int BestTorso { get; set; }

            public ExtraData ExtraData { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(BestTop == null ? "null" : $"new Top.ItemData({BestTop.Sex.ToString().ToLower()}, {BestTop.Drawable}, new int[] {{ {string.Join(", ", BestTop.Textures)} }}, {BestTorso}, {(BestTop.ExtraData == null ? "null" : $"new Under.ItemData.ExtraData({BestTop.ExtraData.Drawable}, {BestTop.ExtraData.BestTorso})")}, {(SexAlternativeId == null ? "null" : $"\"{SexAlternativeId}\"")})")} , {BestTorso}, {(ExtraData == null ? "null" : $"new Under.ItemData.ExtraData({ExtraData.Drawable}, {ExtraData.BestTorso})")}, {(SexAlternativeId == null ? "null" : $"\"{SexAlternativeId}\"")}";

            public ItemData(string name, bool sex, int drawable, int[] textures, Top.ItemData bestTop, int bestTorso, ExtraData extraData = null, string sexAlternativeId = null) : base(name, 0.2f, "prop_ld_tshirt_02", sex, drawable, textures, sexAlternativeId)
            {
                BestTop = bestTop;
                ExtraData = extraData;

                BestTorso = bestTorso;
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

            if (pData.Clothes[1] == null && data.BestTop != null)
            {
                if (Toggled && data.BestTop.ExtraData != null)
                {
                    player.SetClothes(11, data.BestTop.ExtraData.Drawable, data.Textures[variation]);
                    player.SetClothes(3, data.BestTop.ExtraData.BestTorso, 0);
                }
                else
                {
                    player.SetClothes(11, data.BestTop.Drawable, data.Textures[variation]);
                    player.SetClothes(3, data.BestTop.BestTorso, 0);
                }
            }
            else
            {
                if (Toggled && data.ExtraData != null)
                {
                    player.SetClothes(Slot, data.ExtraData.Drawable, data.Textures[variation]);
                    player.SetClothes(3, data.ExtraData.BestTorso, 0);
                }
                else
                {
                    player.SetClothes(Slot, data.Drawable, data.Textures[variation]);
                    player.SetClothes(3, data.BestTorso, 0);
                }
            }

            pData.Accessories[7]?.Wear(pData);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            if (pData.Clothes[1] == null)
            {
                player.SetClothes(11, Service.GetNudeDrawable(ClothesTypes.Top, pData.Sex), 0);
                player.SetClothes(Slot, Service.GetNudeDrawable(ClothesTypes.Top, pData.Sex), 0);
                player.SetClothes(3, Service.GetNudeDrawable(ClothesTypes.Gloves, pData.Sex), 0);

                pData.Accessories[7]?.Wear(pData);
            }
            else
            {
                player.SetClothes(Slot, Service.GetNudeDrawable(ClothesTypes.Under, pData.Sex), 0);

                pData.Clothes[1].Wear(pData);
            }
        }

        public void Action(PlayerData pData)
        {
            if (Data.ExtraData == null)
                return;

            Toggled = !Toggled;

            Wear(pData);
        }

        public Under(string id, int variation) : base(id, IdList[id], typeof(Under), variation)
        {

        }
    }
}
