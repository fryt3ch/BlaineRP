using BlaineRP.Server.Game.Inventory;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Items
{
    public partial interface IUsable
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("Player::SUCI")]
            private static void StopUseCurrentItem(Player player)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                var pData = sRes.Data;

                IUsable item;
                int slot;

                if (pData.TryGetCurrentItemInUse(out item, out slot))
                    item.StopUse(pData, GroupTypes.Items, slot, true);
            }
        }
    }
}