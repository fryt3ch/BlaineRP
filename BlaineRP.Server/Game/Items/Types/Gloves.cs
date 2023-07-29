using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Players.Customization;
using BlaineRP.Server.Game.EntitiesData.Players.Customization.Clothes;

namespace BlaineRP.Server.Game.Items
{
    public partial class Gloves : Clothes
    {
        public new class ItemData : Clothes.ItemData
        {
            public Dictionary<int, int> BestTorsos { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, new Dictionary<int, int>() {{ {string.Join(", ", BestTorsos.Select(x => $"{{ {x.Key}, {x.Value} }}"))} }}, {(SexAlternativeId == null ? "null" : $"\"{SexAlternativeId}\"")}";

            public ItemData(string name, bool sex, int drawable, int[] textures, Dictionary<int, int> bestTorsos, string sexAlternativeId = null) : base(name, 0.1f, "prop_ld_tshirt_02", sex, drawable, textures, sexAlternativeId)
            {
                BestTorsos = bestTorsos;
            }
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

            int curTorso = player.GetClothesDrawable(3);

            int bestTorso;

            if (data.BestTorsos.TryGetValue(curTorso, out bestTorso))
                player.SetClothes(3, bestTorso, data.Textures[variation]);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            if (player.GetClothesDrawable(11) == Service.GetNudeDrawable(ClothesTypes.Top, pData.Sex))
                player.SetClothes(Slot, Service.GetNudeDrawable(ClothesTypes.Gloves, pData.Sex), 0);

            if (pData.Clothes[1] != null)
                pData.Clothes[1].Wear(pData);
            else
                pData.Clothes[2]?.Wear(pData);
        }

        public Gloves(string id, int variation) : base(id, IdList[id], typeof(Gloves), variation)
        {

        }
    }
}
