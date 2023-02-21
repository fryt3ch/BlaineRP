using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Items
{
    public class Shovel : Item, IUsable
    {
        new public class ItemData : Item.ItemData
        {
            public class RandomItem
            {
                public string Id { get; set; }

                public int MinAmount { get; set; }

                public int MaxAmount { get; set; }

                public RandomItem(string Id, int MinAmount = 1, int MaxAmount = 1)
                {
                    this.Id = Id;
                    this.MinAmount = MinAmount;
                    this.MaxAmount = MaxAmount;
                }
            }

            private static Dictionary<float, List<RandomItem>> AllRandomItems = new Dictionary<float, List<RandomItem>>()
            {
                {
                    0.9f,

                    new List<RandomItem>()
                    {
                        new RandomItem("mis_1", 1, 3),
                    }
                },
            };

            public static (string Id, int Amount) GetRandomItem()
            {
                var rProb = Utils.Randoms.Chat.NextDouble();

                var rItems = AllRandomItems.OrderBy(x => Math.Abs(rProb - x.Key)).ThenByDescending(x => x).First();

                var rItem = rItems.Value[Utils.Randoms.Chat.Next(0, rItems.Value.Count)];

                if (rItem.MinAmount != rItem.MaxAmount)
                {
                    return (rItem.Id, Utils.Randoms.Chat.Next(rItem.MinAmount, rItem.MaxAmount + 1));
                }
                else
                {
                    return (rItem.Id, rItem.MinAmount);
                }
            }

            public override string ClientData => $"\"{Name}\", {Weight}f";

            public ItemData(string Name, string Model, float Weight) : base(Name, Weight, Model)
            {

            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "shovel_0", new ItemData("Лопата", "prop_tool_shovel2", 1.5f) },
        };

        [JsonIgnore]
        new public ItemData Data => (ItemData)base.Data;

        [JsonIgnore]
        public bool InUse { get; set; }

        public bool StartUse(PlayerData pData, Inventory.Groups group, int slot, bool needUpdate, params object[] args)
        {
            if (InUse)
                return false;

            InUse = true;

            pData.Player.AttachObject(Model, Sync.AttachSystem.Types.ItemShovel, -1, null, 10_000 - 100 * pData.Skills[PlayerData.SkillTypes.Strength]);

            pData.PlayAnim(Sync.Animations.GeneralTypes.ShovelProcess0);

            if (needUpdate && slot >= 0)
            {
                pData.Player.InventoryUpdate(group, slot, this.ToClientJson(group));
            }

            return true;
        }

        public bool StopUse(PlayerData pData, Inventory.Groups group, int slot, bool needUpdate, params object[] args)
        {
            if (!InUse)
                return false;

            InUse = false;

            pData.Player.DetachObject(Sync.AttachSystem.Types.ItemShovel);

            pData.StopGeneralAnim();

            if (needUpdate && slot >= 0)
            {
                pData.Player.InventoryUpdate(group, slot, this.ToClientJson(group));
            }

            return true;
        }

        public Shovel(string ID) : base(ID, IDList[ID], typeof(Shovel))
        {

        }
    }
}
