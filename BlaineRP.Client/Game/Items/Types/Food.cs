using System.Collections.Generic;
using BlaineRP.Client.Game.Animations;
using BlaineRP.Client.Game.Management.Attachments;

namespace BlaineRP.Client.Game.Items
{
    public class Food : StatusChanger, IStackable
    {
        public new class ItemData : StatusChanger.ItemData, Item.ItemData.IStackable
        {
            public int MaxAmount { get; set; }

            public FastTypes Animation { get; set; }

            public AttachmentTypes AttachType { get; set; }

            public ItemData(string name, float weight, int satiety, int mood, int health, int maxAmount) : base(name, weight, satiety, mood, health)
            {
                this.MaxAmount = maxAmount;
            }
        }

        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();
    }
}