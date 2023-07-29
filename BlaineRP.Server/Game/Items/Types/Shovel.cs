using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.Animations;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Inventory;
using BlaineRP.Server.Sync;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Items
{
    public partial class Shovel : Item, IUsable
    {
        public new class ItemData : Item.ItemData
        {
            public class RandomItem
            {
                public string Id { get; set; }

                public int MinAmount { get; set; }

                public int MaxAmount { get; set; }

                public RandomItem(string id, int minAmount = 1, int maxAmount = 1)
                {
                    Id = id;
                    MinAmount = minAmount;
                    MaxAmount = maxAmount;
                }
            }

            private static Dictionary<float, List<RandomItem>> _allRandomItems = new Dictionary<float, List<RandomItem>>()
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
                var rProb = SRandom.NextDouble();

                var rItems = _allRandomItems.OrderBy(x => Math.Abs(rProb - x.Key)).ThenByDescending(x => x).First();

                var rItem = rItems.Value[SRandom.NextInt32(0, rItems.Value.Count)];

                if (rItem.MinAmount != rItem.MaxAmount)
                {
                    return (rItem.Id, SRandom.NextInt32(rItem.MinAmount, rItem.MaxAmount + 1));
                }
                else
                {
                    return (rItem.Id, rItem.MinAmount);
                }
            }

            public override string ClientData => $"\"{Name}\", {Weight}f";

            public ItemData(string name, string model, float weight) : base(name, weight, model)
            {

            }
        }

        [JsonIgnore]
        public new ItemData Data => (ItemData)base.Data;

        [JsonIgnore]
        public bool InUse { get; set; }

        public bool StartUse(PlayerData pData, GroupTypes group, int slot, bool needUpdate, params object[] args)
        {
            if (InUse)
                return false;

            InUse = true;

            pData.Player.AttachObject(Model, AttachmentType.ItemShovel, -1, null, 10_000 - 100 * pData.Info.Skills[SkillTypes.Strength]);

            pData.PlayAnim(GeneralType.ShovelProcess0);

            if (needUpdate && slot >= 0)
            {
                pData.Player.InventoryUpdate(group, slot, ToClientJson(group));
            }

            return true;
        }

        public bool StopUse(PlayerData pData, GroupTypes group, int slot, bool needUpdate, params object[] args)
        {
            if (!InUse)
                return false;

            InUse = false;

            pData.Player.DetachObject(AttachmentType.ItemShovel);

            pData.StopGeneralAnim();

            if (needUpdate && slot >= 0)
            {
                pData.Player.InventoryUpdate(group, slot, ToClientJson(group));
            }

            return true;
        }

        public Shovel(string id) : base(id, IdList[id], typeof(Shovel))
        {

        }
    }
}
