using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class WorkbenchTool : Item
    {
        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();

        public new class ItemData : Item.ItemData
        {
            public ItemData(string name) : base(name, 0f)
            {
            }
        }
    }
}