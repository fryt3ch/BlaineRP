using System.Linq;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Inventory;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Items
{
    public partial class FishingRod
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("MG::F::P")]
            public static void FishingProcess(Player player, float fishZCoord)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                IUsable item;
                int slot;

                if (pData.TryGetCurrentItemInUse(out item, out slot))
                    if (item is FishingRod fRod)
                    {
                        if (pData.AttachedObjects.Where(x => x.Type == AttachmentType.ItemFishG).Any())
                            return;

                        fRod.StartCatchProcess(pData, 10000, 0.00095f, 3, fishZCoord);
                    }
            }

            [RemoteEvent("MG::F::F")]
            public static void FishingFinish(Player player, bool success)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                IUsable item;
                int slot;

                if (pData.TryGetCurrentItemInUse(out item, out slot))
                    if (item is FishingRod fRod)
                    {
                        fRod.StopUse(pData, GroupTypes.Items, slot, true);

                        if (success)
                        {
                            (string Id, int Amount) rItem = ItemData.GetRandomItem();

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
    }
}