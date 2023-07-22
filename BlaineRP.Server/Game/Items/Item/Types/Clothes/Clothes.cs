using Newtonsoft.Json;
using System;

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

        new public abstract class ItemData : Item.ItemData
        {
            public class ExtraData
            {
                public int Drawable { get; set; }

                public int BestTorso { get; set; }

                public ExtraData(int Drawable, int BestTorso)
                {
                    this.Drawable = Drawable;

                    this.BestTorso = BestTorso;
                }
            }

            public interface IToggleable
            {
                public ExtraData ExtraData { get; set; }
            }

            public bool Sex { get; set; }

            public int Drawable { get; set; }

            public int[] Textures { get; set; }

            public string SexAlternativeID { get; set; }

            public ItemData(string Name, float Weight, string Model, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, Weight, Model)
            {
                this.Drawable = Drawable;
                this.Textures = Textures;

                this.Sex = Sex;

                this.SexAlternativeID = SexAlternativeID;
            }
        }

        public abstract void Wear(PlayerData pData);

        public abstract void Unwear(PlayerData pData);

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        public ItemData SexAlternativeData => Data.SexAlternativeID is string str ? (ItemData)Stuff.GetData(str, Type) : null;

        [JsonIgnore]
        public int Slot => Game.Data.Customization.GetClothesIdxByType(Type);

        [JsonProperty(PropertyName = "V")]
        /// <summary>Вариация одежды</summary>
        public int Var { get; set; }

        public Clothes(string ID, Item.ItemData Data, Type Type, int Var) : base(ID, Data, Type)
        {
            this.Var = Var >= this.Data.Textures.Length ? this.Data.Textures.Length - 1 : (Var < 0 ? 0 : Var);
        }
    }
}
