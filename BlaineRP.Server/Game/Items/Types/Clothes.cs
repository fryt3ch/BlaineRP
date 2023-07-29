using Newtonsoft.Json;
using System;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Players.Customization;

namespace BlaineRP.Server.Game.Items
{
    public abstract class Clothes : Item, IWearable
    {
        public interface IToggleable
        {
            [JsonProperty(PropertyName = "S")]
            public bool Toggled { get; set; }

            public void Action(PlayerData pData);
        }

        public interface IProp
        {

        }

        public new abstract class ItemData : Item.ItemData
        {
            public class ExtraData
            {
                public int Drawable { get; set; }

                public int BestTorso { get; set; }

                public ExtraData(int drawable, int bestTorso)
                {
                    Drawable = drawable;

                    BestTorso = bestTorso;
                }
            }

            public interface IToggleable
            {
                public ExtraData ExtraData { get; set; }
            }

            public bool Sex { get; set; }

            public int Drawable { get; set; }

            public int[] Textures { get; set; }

            public string SexAlternativeId { get; set; }

            public ItemData(string name, float weight, string model, bool sex, int drawable, int[] textures, string sexAlternativeId = null) : base(name, weight, model)
            {
                Drawable = drawable;
                Textures = textures;

                Sex = sex;

                SexAlternativeId = sexAlternativeId;
            }
        }

        public abstract void Wear(PlayerData pData);

        public abstract void Unwear(PlayerData pData);

        [JsonIgnore]
        public new ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        public ItemData SexAlternativeData => Data.SexAlternativeId is string str ? (ItemData)Stuff.GetData(str, Type) : null;

        [JsonIgnore]
        public int Slot => Service.GetClothesIdxByType(Type);

        [JsonProperty(PropertyName = "V")]
        /// <summary>Вариация одежды</summary>
        public int Var { get; set; }

        public Clothes(string id, Item.ItemData data, Type type, int var) : base(id, data, type)
        {
            Var = var >= Data.Textures.Length ? Data.Textures.Length - 1 : (var < 0 ? 0 : var);
        }
    }
}
