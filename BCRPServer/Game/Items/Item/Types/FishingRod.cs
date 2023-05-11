using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static BCRPServer.Game.Items.Inventory;

namespace BCRPServer.Game.Items
{
    public class FishingRod : Item, IUsable
    {
        new public class ItemData : Item.ItemData
        {
            public static uint FakeFishModel { get; } = NAPI.Util.GetHashKey("prop_starfish_01");

            public static string BaitId { get; } = "mis_0";
            public static string WormId { get; } = "mis_1";

            public class FishingData
            {
                public string UsedBaitId { get; private set; }

                public FishingData(string UsedBaitId)
                {
                    this.UsedBaitId = UsedBaitId;
                }
            }

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

            private static Dictionary<decimal, RandomItem[]> AllRandomItems = new Dictionary<decimal, RandomItem[]>()
            {
                {
                    0.05m,

                    new RandomItem[]
                    {
                        new RandomItem("am_5.56", 10, 50),
                    }
                },

                {
                    0.30m,

                    new RandomItem[]
                    {
                        new RandomItem("f_acod", 1, 1),
                    }
                },
            };

            public static (string Id, int Amount) GetRandomItem()
            {
                var rProb = (decimal)Utils.Randoms.Chat.NextDouble();

                var rItems = AllRandomItems.OrderBy(x => Math.Abs(rProb - x.Key)).ThenByDescending(x => x).First();

                var rItem = rItems.Value[Utils.Randoms.Chat.Next(0, rItems.Value.Length)];

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

        public bool StartUse(PlayerData pData, Inventory.Groups group, int slot, bool needUpdate, params object[] args)
        {
            if (InUse)
                return false;

            var baitId = (string)args[0];

            var baitIdx = -1;

            for (int i = 0; i < pData.Items.Length; i++)
            {
                if (pData.Items[i]?.ID == baitId)
                {
                    baitIdx = i;

                    break;
                }
            }

            if (baitIdx < 0)
            {
                pData.Player.Notify("Inventory::NoItem");

                return false;
            }

            var bait = pData.Items[baitIdx] as Game.Items.MiscStackable;

            if (bait == null)
                return false;

            bait.Amount -= 1;

            if (bait.Amount <= 0)
            {
                bait.Delete();

                pData.Items[baitIdx] = null;

                MySQL.CharacterItemsUpdate(pData.Info);
            }
            else
            {
                bait.Update();
            }

            InUse = true;

            pData.Player.AttachObject(Model, Sync.AttachSystem.Types.ItemFishingRodG, -1, null, 5_000);

            pData.PlayAnim(Sync.Animations.GeneralTypes.FishingIdle0);

            if (needUpdate && slot >= 0)
            {
                pData.Player.InventoryUpdate(group, slot, this.ToClientJson(group), Groups.Items, baitIdx, Game.Items.Item.ToClientJson(pData.Items[baitIdx], Groups.Items));
            }

            return true;
        }

        public bool StopUse(PlayerData pData, Inventory.Groups group, int slot, bool needUpdate, params object[] args)
        {
            if (!InUse)
                return false;

            InUse = false;

            pData.Player.DetachObject(Sync.AttachSystem.Types.ItemFishingRodG);
            pData.Player.DetachObject(Sync.AttachSystem.Types.ItemFishG);

            pData.StopGeneralAnim();

            if (needUpdate && slot >= 0)
            {
                pData.Player.InventoryUpdate(group, slot, this.ToClientJson(group));
            }

            return true;
        }

        public void StartCatchProcess(PlayerData pData, int maxCatchTime, float fishSpeed, int catchCount, float fishZCoord)
        {
            if (!InUse)
                return;

            var fPos = pData.Player.GetFrontOf(7.5f);

            pData.Player.AttachObject(ItemData.FakeFishModel, Sync.AttachSystem.Types.ItemFishG, -1, $"{fPos.X}&{fPos.Y}&{fishZCoord}", maxCatchTime, fishSpeed, catchCount);

            pData.PlayAnim(Sync.Animations.GeneralTypes.FishingIdle0);

            pData.PlayAnim(Sync.Animations.GeneralTypes.FishingProcess0);
        }

        public FishingRod(string ID) : base(ID, IDList[ID], typeof(FishingRod))
        {

        }
    }
}
