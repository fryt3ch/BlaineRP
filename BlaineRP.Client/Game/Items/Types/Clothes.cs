namespace BlaineRP.Client.Game.Items
{
    public abstract class Clothes : Item, IWearable
    {
        public interface IToggleable
        {

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
                    this.Drawable = drawable;

                    this.BestTorso = bestTorso;
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

            public ItemData(string name, float weight, bool sex, int drawable, int[] textures, string sexAlternativeId = null) : base(name, weight)
            {
                this.Drawable = drawable;
                this.Textures = textures;

                this.Sex = sex;

                this.SexAlternativeId = sexAlternativeId;
            }
        }
    }
}