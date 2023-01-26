using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Events.Players
{
    class Minigames : Script
    {
        [RemoteEvent("MG::F::P")]
        public static void FishingProcess(Player player, float fishZCoord)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var curItem = pData.CurrentItemInUse;

            var fRod = curItem?.Item as Game.Items.FishingRod;

            if (fRod == null)
                return;

            if (pData.AttachedObjects.Where(x => x.Type == Sync.AttachSystem.Types.ItemFishG).Any())
                return;

            fRod.StartCatchProcess(pData, 10000, 0.00095f, 3, fishZCoord);
        }

        [RemoteEvent("MG::F::F")]
        public static void FishingFinish(Player player, bool success)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var curItem = pData.CurrentItemInUse;

            var fRod = curItem?.Item as Game.Items.FishingRod;

            if (fRod == null)
                return;

            fRod.StopUse(pData, Game.Items.Inventory.Groups.Items, curItem.Value.Slot, true);

            if (success)
            {
                var rItem = Game.Items.FishingRod.ItemData.GetRandomItem();

                pData.GiveItemDropExcess(rItem.Id, 0, rItem.Amount, false, false);

                player.TriggerEvent("Item::FCN", rItem.Id, rItem.Amount);
            }
            else
            {
                player.Notify("Inventory::FGNC");
            }
        }

        [RemoteEvent("MG::SHOV::F")]
        public static void ShovelFinish(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var curItem = pData.CurrentItemInUse;

            var shovel = curItem?.Item as Game.Items.Shovel;

            if (shovel == null)
                return;

            shovel.StopUse(pData, Game.Items.Inventory.Groups.Items, curItem.Value.Slot, true);

            var rItem = Game.Items.Shovel.ItemData.GetRandomItem();

            pData.GiveItemDropExcess(rItem.Id, 0, rItem.Amount, false, false);

            player.TriggerEvent("Item::FCN", rItem.Id, rItem.Amount);
        }
    }
}
