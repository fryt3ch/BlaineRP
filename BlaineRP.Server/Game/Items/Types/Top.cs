using Newtonsoft.Json;
using System.Collections.Generic;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Players.Customization;
using BlaineRP.Server.Game.EntitiesData.Players.Customization.Clothes;

namespace BlaineRP.Server.Game.Items
{
    public partial class Top : Clothes, Clothes.IToggleable
    {
        public new class ItemData : Clothes.ItemData, Clothes.ItemData.IToggleable
        {
            public int BestTorso { get; set; }

            public ExtraData ExtraData { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {BestTorso}, {(ExtraData == null ? "null" : $"new Top.ItemData.ExtraData({ExtraData.Drawable}, {ExtraData.BestTorso})")}, {(SexAlternativeId == null ? "null" : $"\"{SexAlternativeId}\"")}";

            public ItemData(string name, bool sex, int drawable, int[] textures, int bestTorso, ExtraData extraData = null, string sexAlternativeId = null) : base(name, 0.3f, "prop_ld_shirt_01", sex, drawable, textures, sexAlternativeId)
            {
                BestTorso = bestTorso;
                ExtraData = extraData;
            }

            public ItemData(bool sex, int drawable, int[] textures, int bestTorso, ExtraData extraData = null, string sexAlternativeId = null) : this(null, sex, drawable, textures, bestTorso, extraData, sexAlternativeId) { }
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

            if (Toggled)
            {
                player.SetClothes(Slot, data.ExtraData.Drawable, data.Textures[variation]);
                player.SetClothes(3, data.ExtraData.BestTorso, 0);
            }
            else
            {
                player.SetClothes(Slot, data.Drawable, data.Textures[variation]);
                player.SetClothes(3, data.BestTorso, 0);
            }

            if (pData.Armour != null)
            {
                var aData = pData.Armour.Data;

                var aVar = pData.Armour.Var;

                if (Data.Sex != pData.Sex)
                {
                    aData = pData.Armour.SexAlternativeData;

                    if (aData == null)
                        return;

                    if (aVar >= aData.Textures.Length)
                        aVar = aData.Textures.Length;
                }

                player.SetClothes(9, aData.DrawableTop, aData.Textures[aVar]);
            }

            if (pData.Clothes[2] != null)
                pData.Clothes[2].Wear(pData);
            else
                pData.Accessories[7]?.Wear(pData);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.SetClothes(Slot, Service.GetNudeDrawable(ClothesTypes.Top, pData.Sex), 0);
            player.SetClothes(3, Service.GetNudeDrawable(ClothesTypes.Gloves, pData.Sex), 0);

            if (pData.Armour != null)
            {
                var aData = pData.Armour.Data;

                var aVar = pData.Armour.Var;

                if (Data.Sex != pData.Sex)
                {
                    aData = pData.Armour.SexAlternativeData;

                    if (aData == null)
                        return;

                    if (aVar >= aData.Textures.Length)
                        aVar = aData.Textures.Length;
                }

                player.SetClothes(9, aData.Drawable, aData.Textures[aVar]);
            }

            if (pData.Clothes[2] != null)
                pData.Clothes[2].Wear(pData);
            else
                pData.Accessories[7]?.Wear(pData);
        }

        public void Action(PlayerData pData)
        {
            if (Data.ExtraData == null)
                return;

            Toggled = !Toggled;

            Wear(pData);
        }

        public Top(string id, int variation) : base(id, IdList[id], typeof(Top), variation)
        {

        }
    }
}
