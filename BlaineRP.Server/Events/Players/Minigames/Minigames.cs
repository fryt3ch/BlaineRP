using GTANetworkAPI;
using System.Linq;

namespace BlaineRP.Server.Events.Players
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

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            Game.Items.IUsable item;
            int slot;

            if (pData.TryGetCurrentItemInUse(out item, out slot))
            {
                if (item is Game.Items.FishingRod fRod)
                {
                    if (pData.AttachedObjects.Where(x => x.Type == Sync.AttachSystem.Types.ItemFishG).Any())
                        return;

                    fRod.StartCatchProcess(pData, 10000, 0.00095f, 3, fishZCoord);
                }
            }
        }

        [RemoteEvent("MG::F::F")]
        public static void FishingFinish(Player player, bool success)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            Game.Items.IUsable item;
            int slot;

            if (pData.TryGetCurrentItemInUse(out item, out slot))
            {
                if (item is Game.Items.FishingRod fRod)
                {
                    fRod.StopUse(pData, Game.Items.Inventory.GroupTypes.Items, slot, true);

                    if (success)
                    {
                        var rItem = Game.Items.FishingRod.ItemData.GetRandomItem();

                        pData.GiveItemDropExcess(out _, rItem.Id, 0, rItem.Amount, false, false);

                        player.TriggerEvent("Item::FCN", rItem.Id, rItem.Amount);
                    }
                    else
                    {
                        player.Notify("Inventory::FGNC");
                    }
                }
            }
        }

        [RemoteEvent("MG::SHOV::F")]
        public static void ShovelFinish(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            Game.Items.IUsable item;
            int slot;

            if (pData.TryGetCurrentItemInUse(out item, out slot))
            {
                if (item is Game.Items.Shovel shovel)
                {
                    shovel.StopUse(pData, Game.Items.Inventory.GroupTypes.Items, slot, true);

                    var rItem = Game.Items.Shovel.ItemData.GetRandomItem();

                    pData.GiveItemDropExcess(out _, rItem.Id, 0, rItem.Amount, false, false);

                    player.TriggerEvent("Item::FCN", rItem.Id, rItem.Amount);
                }
            }
        }

        [RemoteProc("MG::LOCKPICK::Cuffs")]
        public static byte LockpickCuffs(Player player, bool success, int itemIdx)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsFrozen)
                return 0;

            if (itemIdx < 0 || itemIdx >= pData.Items.Length)
                return 0;

            var cuffAttach = pData.AttachedObjects.Where(x => x.Type == Sync.AttachSystem.Types.Cuffs).FirstOrDefault();

            if (cuffAttach == null)
                return 1;

            var item = pData.Items[itemIdx];

            if (item == null || item.ID != "mis_lockpick")
                return 2;

            if (success)
            {
                pData.Player.DetachObject(cuffAttach.Type);

                return 255;
            }
            else
            {
                if (item is Game.Items.IStackable stackable)
                {
                    stackable.Amount -= 1;

                    if (stackable.Amount <= 0)
                    {
                        item.Delete();

                        item = null;

                        pData.Items[itemIdx] = item;

                        MySQL.CharacterItemsUpdate(pData.Info);
                    }
                    else
                    {
                        item.Update();
                    }

                    pData.Player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, itemIdx, Game.Items.Item.ToClientJson(item, Game.Items.Inventory.GroupTypes.Items));
                }

                return 255;
            }
        }
    }
}
