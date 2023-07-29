using BlaineRP.Server.Game.Inventory;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Items
{
    public partial class Shovel
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("MG::SHOV::F")]
            public static void ShovelFinish(Player player)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                var pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                IUsable item;
                int slot;

                if (pData.TryGetCurrentItemInUse(out item, out slot))
                    if (item is Shovel shovel)
                    {
                        shovel.StopUse(pData, GroupTypes.Items, slot, true);

                        (string Id, int Amount) rItem = ItemData.GetRandomItem();

                        pData.GiveItemDropExcess(out _, rItem.Id, 0, rItem.Amount, false, false);

                        player.TriggerEvent("Item::FCN", rItem.Id, rItem.Amount);
                    }
            }
        }
    }
}