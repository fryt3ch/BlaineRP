using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Game.Items
{
    public class FishingRod : Item, IUsable
    {
        new public class ItemData : Item.ItemData
        {
            public static uint FakeFishItem { get; } = NAPI.Util.GetHashKey("prop_starfish_01");

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
                    0.05f,

                    new List<RandomItem>()
                    {
                        new RandomItem("am_5.56", 10, 50),
                    }
                },

                {
                    0.30f,

                    new List<RandomItem>()
                    {
                        new RandomItem("f_acod", 1, 1),
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
            { "rod_0", new ItemData("Удочка (обычн.)", "prop_fishing_rod_02", 1f) },
            { "rod_1", new ItemData("Удочка (улучш.)", "prop_fishing_rod_02", 1f) },
        };

        [JsonIgnore]
        new public ItemData Data => (ItemData)base.Data;

        [JsonIgnore]
        public bool InUse { get; set; }

        public void StartUse(PlayerData pData, Inventory.Groups group, int slot, bool needUpdate, params object[] args)
        {
            if (InUse)
                return;

            InUse = true;

            pData.Player.AttachObject(Model, Sync.AttachSystem.Types.ItemFishingRodG, -1, null, 5_000);

            pData.PlayAnim(Sync.Animations.GeneralTypes.FishingIdle0);

            if (needUpdate && slot >= 0)
            {
                pData.Player.InventoryUpdate(group, slot, this.ToClientJson(group));
            }
        }

        public void StopUse(PlayerData pData, Inventory.Groups group, int slot, bool needUpdate, params object[] args)
        {
            if (!InUse)
                return;

            InUse = false;

            pData.Player.DetachObject(Sync.AttachSystem.Types.ItemFishingRodG);
            pData.Player.DetachObject(Sync.AttachSystem.Types.ItemFishG);

            pData.StopAnim();

            if (needUpdate && slot >= 0)
            {
                pData.Player.InventoryUpdate(group, slot, this.ToClientJson(group));
            }
        }

        public void StartCatchProcess(PlayerData pData, int maxCatchTime, float fishSpeed, int catchCount, float fishZCoord)
        {
            if (!InUse)
                return;

            var fPos = pData.Player.GetFrontOf(7.5f);

            pData.Player.AttachObject(ItemData.FakeFishItem, Sync.AttachSystem.Types.ItemFishG, -1, $"{fPos.X}&{fPos.Y}&{fishZCoord}", maxCatchTime, fishSpeed, catchCount);

            pData.PlayAnim(Sync.Animations.GeneralTypes.FishingIdle0);

            pData.PlayAnim(Sync.Animations.GeneralTypes.FishingProcess0);
        }

        public FishingRod(string ID) : base(ID, IDList[ID], typeof(FishingRod))
        {

        }
    }
}
