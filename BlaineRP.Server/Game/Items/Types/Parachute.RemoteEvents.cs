using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.Inventory;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Items
{
    public partial class Parachute
    {
        internal class RemoteEvents
        {
            [RemoteEvent("Player::ParachuteS")]
            private static void ParachuteState(Player player, bool state)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                var pData = sRes.Data;

                if (state)
                    for (var slot = 0; slot < pData.Items.Length; slot++)
                    {
                        if (pData.Items[slot] is Parachute parachute && parachute.InUse)
                            if (parachute.StopUse(pData, GroupTypes.Items, slot, false, "DONT_CANCEL_TASK_CLIENT"))
                            {
                                parachute.Delete();

                                pData.Items[slot] = null;

                                player.InventoryUpdate(GroupTypes.Items, slot, ToClientJson(pData.Items[slot], GroupTypes.Items));

                                MySQL.CharacterItemsUpdate(pData.Info);

                                player.AttachObject(Attachments.Service.Models.ParachuteSync, AttachmentType.ParachuteSync, -1, null);

                                return;
                            }
                    }
                else
                    player.DetachObject(AttachmentType.ParachuteSync);
            }
        }
    }
}