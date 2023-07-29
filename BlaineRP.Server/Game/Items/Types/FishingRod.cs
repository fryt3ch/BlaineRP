using GTANetworkAPI;
using Newtonsoft.Json;
using System.Collections.Generic;
using BlaineRP.Server.Game.Animations;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Inventory;
using BlaineRP.Server.Sync;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Items
{
    public partial class FishingRod : Item, IUsable
    {
        public new class ItemData : Item.ItemData
        {
            public static uint FakeFishModel { get; } = NAPI.Util.GetHashKey("prop_starfish_01");

            public static string BaitId { get; } = "mis_0";
            public static string WormId { get; } = "mis_1";

            public class FishingData
            {
                public string UsedBaitId { get; private set; }

                public FishingData(string usedBaitId)
                {
                    UsedBaitId = usedBaitId;
                }
            }

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

            private static ChancePicker<RandomItem> ChancePicker { get; set; } = new ChancePicker<RandomItem>
            (
                new ChancePicker<RandomItem>.Item<RandomItem>(0.70d, new RandomItem("f_acod", 1, 1)),
                new ChancePicker<RandomItem>.Item<RandomItem>(0.30d, new RandomItem("am_5.56", 10, 50))
            );

            public static (string Id, int Amount) GetRandomItem()
            {
                var rProb = (decimal)SRandom.NextDouble();

                var rItem = ChancePicker.GetNextItem(out _);

                if (rItem.MinAmount != rItem.MaxAmount)
                {
                    return (rItem.Id, SRandom.NextInt32S(rItem.MinAmount, rItem.MaxAmount + 1));
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

            var bait = pData.Items[baitIdx] as MiscStackable;

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

            pData.Player.AttachObject(Model, AttachmentType.ItemFishingRodG, -1, null, 5_000);

            pData.PlayAnim(GeneralType.FishingIdle0);

            if (needUpdate && slot >= 0)
            {
                pData.Player.InventoryUpdate(group, slot, ToClientJson(group), GroupTypes.Items, baitIdx, ToClientJson(pData.Items[baitIdx], GroupTypes.Items));
            }

            return true;
        }

        public bool StopUse(PlayerData pData, GroupTypes group, int slot, bool needUpdate, params object[] args)
        {
            if (!InUse)
                return false;

            InUse = false;

            pData.Player.DetachObject(AttachmentType.ItemFishingRodG);
            pData.Player.DetachObject(AttachmentType.ItemFishG);

            pData.StopGeneralAnim();

            if (needUpdate && slot >= 0)
            {
                pData.Player.InventoryUpdate(group, slot, ToClientJson(group));
            }

            return true;
        }

        public void StartCatchProcess(PlayerData pData, int maxCatchTime, float fishSpeed, int catchCount, float fishZCoord)
        {
            if (!InUse)
                return;

            var fPos = pData.Player.GetFrontOf(7.5f);

            pData.Player.AttachObject(ItemData.FakeFishModel, AttachmentType.ItemFishG, -1, $"{fPos.X}&{fPos.Y}&{fishZCoord}", maxCatchTime, fishSpeed, catchCount);

            pData.PlayAnim(GeneralType.FishingIdle0);

            pData.PlayAnim(GeneralType.FishingProcess0);
        }

        public FishingRod(string id) : base(id, IdList[id], typeof(FishingRod))
        {

        }
    }
}
