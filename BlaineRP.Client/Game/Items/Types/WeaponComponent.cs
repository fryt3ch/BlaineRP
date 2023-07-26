using System.Collections.Generic;
using BlaineRP.Client.Game.Management.Weapons;

namespace BlaineRP.Client.Game.Items
{
    public class WeaponComponent : Item
    {
        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();

        public new class ItemData : Item.ItemData
        {
            public ItemData(string name, float weight, WeaponComponentTypes type) : base(name, weight)
            {
                Type = type;
            }

            public WeaponComponentTypes Type { get; set; }
        }
    }
}