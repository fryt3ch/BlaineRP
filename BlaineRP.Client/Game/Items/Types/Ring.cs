using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class Ring : Clothes, Clothes.IToggleable, Clothes.IProp
    {
        public new class ItemData : Clothes.ItemData
        {
            public uint Model { get; private set; }

            public ItemData(string name, float weight, bool sex, uint model, string sexAlternativeId = null) : base(name, weight, sex, 1, new int[] { 0 }, sexAlternativeId)
            {
                this.Model = model;
            }
        }

        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();
    }
}