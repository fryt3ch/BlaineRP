using System;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Inventory;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Items
{
    public partial class Note
    {
        internal class RemoteEvents : Script
        {
            [RemoteProc("Player::NoteEdit")]
            private static bool NoteEdit(Player player, int invGroupNum, int slot, string text)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                PlayerData pData = sRes.Data;

                if (slot < 0 || !Enum.IsDefined(typeof(GroupTypes), invGroupNum))
                    return false;

                var invGroup = (GroupTypes)invGroupNum;

                if (invGroup != GroupTypes.Items && invGroup != GroupTypes.Bag)
                    return false;

                // text check

                Note note;

                if (invGroup == GroupTypes.Items)
                {
                    if (slot >= pData.Items.Length)
                        return false;

                    note = pData.Items[slot] as Note;
                }
                else
                {
                    if (pData.Bag == null || slot >= pData.Bag.Items.Length)
                        return false;

                    note = pData.Bag.Items[slot] as Note;
                }

                if (note == null)
                    return false;

                ItemData iData = note.Data;

                if ((iData.Type & ItemData.Types.Write) == ItemData.Types.Write ||
                    (iData.Type & ItemData.Types.WriteTextNullOnly) == ItemData.Types.WriteTextNullOnly && note.Text == null)
                {
                    note.Text = text;

                    note.Update();

                    //player.InventoryUpdate(invGroup, slot, note.ToClientJson(invGroup));

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}