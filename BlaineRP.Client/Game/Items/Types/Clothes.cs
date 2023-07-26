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
            public ItemData(string name, float weight, bool sex, int drawable, int[] textures, string sexAlternativeId = null) : base(name, weight)
            {
                Drawable = drawable;
                Textures = textures;

                Sex = sex;

                SexAlternativeId = sexAlternativeId;
            }

            public bool Sex { get; set; }

            public int Drawable { get; set; }

            public int[] Textures { get; set; }

            public string SexAlternativeId { get; set; }

            public class ExtraData
            {
                public ExtraData(int drawable, int bestTorso)
                {
                    Drawable = drawable;

                    BestTorso = bestTorso;
                }

                public int Drawable { get; set; }

                public int BestTorso { get; set; }
            }

            public interface IToggleable
            {
                public ExtraData ExtraData { get; set; }
            }
        }
    }
}