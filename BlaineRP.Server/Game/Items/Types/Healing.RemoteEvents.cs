using System.Linq;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Inventory;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Items
{
    public partial class Healing
    {
        internal class RemoteEvents : Script
        {
            [RemoteProc("Player::ResurrectItem")]
            private static byte ResurrectItem(Player player, Player target, int itemIdx)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return 0;

                PlayerData pData = sRes.Data;

                if (itemIdx < 0 || itemIdx >= pData.Items.Length)
                    return 0;

                if (!pData.CanUseInventory(true) ||
                    pData.IsCuffed ||
                    pData.IsFrozen ||
                    pData.IsKnocked ||
                    pData.IsAttachedToEntity != null ||
                    pData.HasAnyHandAttachedObject ||
                    pData.AttachedEntities.Any())
                    return 0;

                PlayerData tData = target.GetMainData();

                if (tData == null || pData == tData)
                    return 0;

                if (!tData.Player.IsNearToEntity(pData.Player, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                    return 0;

                if (!tData.IsKnocked || tData.IsAttachedToEntity != null)
                    return 0;

                var healingItem = pData.Items[itemIdx] as Healing;

                if (healingItem == null || healingItem.Data.ResurrectionChance <= 0)
                    return 1;

                var fData = Fractions.Fraction.Get(pData.Fraction) as Fractions.EMS;

                double? overrideResurrectChance = fData == null ? (double?)null : 1d;

                healingItem.ResurrectPlayer(pData, tData, overrideResurrectChance, null);

                healingItem.Amount--;

                if (healingItem.Amount <= 0)
                {
                    healingItem.Delete();

                    pData.Items[itemIdx] = null;

                    MySQL.CharacterItemsUpdate(pData.Info);
                }
                else
                {
                    healingItem.Update();
                }

                pData.Player.InventoryUpdate(GroupTypes.Items, itemIdx, ToClientJson(pData.Items[itemIdx], GroupTypes.Items));

                Management.Chat.Service.SendLocal(Management.Chat.MessageType.Me, pData.Player, Language.Strings.Get("CHAT_PLAYER_RESURRECT_0"), tData.Player);

                return 255;
            }

            [RemoteEvent("Player::ResurrectFinish")]
            private static void ResurrectFinish(Player player)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                var attach = pData.AttachedEntities.Where(x => x.Type == AttachmentType.PlayerResurrect && x.EntityType == EntityType.Player).FirstOrDefault();

                if (attach == null)
                    return;

                var dataD = attach.SyncData.Split('_');

                Player targetPlayer = Utils.GetPlayerByID(attach.Id);

                PlayerData tData = targetPlayer.GetMainData();

                if (tData == null)
                    return;

                pData.Player.DetachEntity(targetPlayer);

                bool resurrect = dataD[1] == "1";

                if (resurrect)
                {
                    if (tData.IsKnocked)
                    {
                        tData.SetAsNotKnocked();

                        player.NotifySuccess(Language.Strings.Get("NTFC_PLAYER_RESURRECT_0", tData.GetNameForPlayer(pData)));
                        targetPlayer.NotifySuccess(Language.Strings.Get("NTFC_PLAYER_RESURRECT_1", pData.GetNameForPlayer(tData)));
                    }
                }
                else
                {
                    player.NotifySuccess(Language.Strings.Get("NTFC_PLAYER_RESURRECT_2", tData.GetNameForPlayer(pData)));
                    targetPlayer.NotifySuccess(Language.Strings.Get("NTFC_PLAYER_RESURRECT_3", pData.GetNameForPlayer(tData)));
                }

                Management.Chat.Service.SendLocal(Management.Chat.MessageType.Try, pData.Player, Language.Strings.Get("CHAT_PLAYER_RESURRECT_1"), tData.Player, resurrect);
            }
        }
    }
}