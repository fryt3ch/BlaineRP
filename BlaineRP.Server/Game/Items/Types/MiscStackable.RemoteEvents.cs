using System.Linq;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.Inventory;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Items
{
    public partial class MiscStackable
    {
        internal class RemoteEvents : Script
        {
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

                var cuffAttach = pData.AttachedObjects.Where(x => x.Type == AttachmentType.Cuffs).FirstOrDefault();

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
                    if (item is IStackable stackable)
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

                        pData.Player.InventoryUpdate(GroupTypes.Items, itemIdx, ToClientJson(item, GroupTypes.Items));
                    }

                    return 255;
                }
            }
        }
    }
}