using System.Collections.Generic;
using BlaineRP.Client.Game.Animations;
using BlaineRP.Client.Game.Attachments;

namespace BlaineRP.Client.Game.Items
{
    public class Food : StatusChanger, IStackable
    {
        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();

        public new class ItemData : StatusChanger.ItemData, Item.ItemData.IStackable
        {
            public ItemData(string name, float weight, int satiety, int mood, int health, int maxAmount) : base(name, weight, satiety, mood, health)
            {
                MaxAmount = maxAmount;
            }

            public FastType Animation { get; set; }

            public AttachmentType AttachType { get; set; }
            public int MaxAmount { get; set; }
        }
    }
}