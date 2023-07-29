using System;
using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Items;
using GTANetworkAPI;
using static BlaineRP.Server.Game.Inventory.Service;
using static BlaineRP.Server.Game.World.Service;

namespace BlaineRP.Server.Game.Inventory
{
    public class RemoteEvents : Script
    {
        [RemoteEvent("Inventory::Replace")]
        private static void InventoryReplace(Player player, int to, int slotTo, int from, int slotFrom, int amount)
        {
            (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            PlayerData pData = sRes.Data;

            if (!Enum.IsDefined(typeof(GroupTypes), to) || !Enum.IsDefined(typeof(GroupTypes), from))
                return;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            Replace(pData, (GroupTypes)to, slotTo, (GroupTypes)from, slotFrom, amount);
        }

        [RemoteEvent("Inventory::Action")]
        private static void InventoryAction(Player player, int group, int slot, int action, string data)
        {
            (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            PlayerData pData = sRes.Data;

            if (data == null)
                return;

            if (!Enum.IsDefined(typeof(GroupTypes), group) || slot < 0 || action < 5)
                return;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            Action(pData, (GroupTypes)group, slot, action, data.Split('&'));
        }

        [RemoteEvent("Inventory::Drop")]
        private static void InventoryDrop(Player player, int slotStr, int slot, int amount)
        {
            (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            PlayerData pData = sRes.Data;

            if (!Enum.IsDefined(typeof(GroupTypes), slotStr) || slot < 0)
                return;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            Drop(pData, (GroupTypes)slotStr, slot, amount);
        }
    }
}