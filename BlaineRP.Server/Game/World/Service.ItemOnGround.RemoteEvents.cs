using System;
using System.Linq;
using BlaineRP.Server.Game.Animations;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Inventory;
using BlaineRP.Server.Game.Items;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.World
{
    public partial class Service
    {
        public partial class ItemOnGround
        {
            internal class RemoteEvents : Script
            {
                [RemoteEvent("Inventory::Take")]
                private static void InventoryTake(Player player, uint UID, int amount)
                {
                    (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                    if (sRes.IsSpammer)
                        return;

                    PlayerData pData = sRes.Data;

                    if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked || player.Vehicle != null)
                        return;

                    ItemOnGround item = GetItemOnGround(UID);

                    if (item?.Item == null)
                        return;

                    if (item.Type == Types.PlacedItem && !item.PlayerHasAccess(pData, false, true))
                        return;

                    if (!player.IsNearToEntity(item.Object, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                        return;

                    float curWeight = pData.Items.Sum(x => x?.Weight ?? 0f);

                    int freeIdx = -1, curAmount = 1;

                    if (item.Item is Items.IStackable stackable)
                    {
                        curAmount = stackable.Amount;

                        if (amount > curAmount)
                            amount = curAmount;

                        if (curWeight + amount * item.Item.BaseWeight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
                            amount = (int)Math.Floor((Properties.Settings.Static.MAX_INVENTORY_WEIGHT - curWeight) / item.Item.BaseWeight);

                        if (amount > 0)
                            for (var i = 0; i < pData.Items.Length; i++)
                            {
                                Item curItem = pData.Items[i];

                                if (curItem == null)
                                {
                                    if (freeIdx < 0)
                                        freeIdx = i;

                                    continue;
                                }

                                if (curItem.ID == item.Item.ID && curItem is Items.IStackable curItemStackable && curItemStackable.Amount + amount <= curItemStackable.MaxAmount)
                                {
                                    freeIdx = i;

                                    break;
                                }
                            }
                    }
                    else
                    {
                        if (amount != 1)
                            amount = 1;

                        if (curWeight + item.Item.Weight <= Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
                            freeIdx = Array.IndexOf(pData.Items, null);
                    }

                    if (amount <= 0 || freeIdx < 0)
                    {
                        player.Notify("Inventory::NoSpace");

                        return;
                    }

                    if (pData.CanPlayAnimNow())
                        pData.PlayAnim(FastType.Pickup, Properties.Settings.Static.InventoryPickupAnimationTime);

                    if (amount == curAmount)
                    {
                        if (pData.Items[freeIdx] != null)
                        {
                            item.Delete(true);

                            ((Items.IStackable)pData.Items[freeIdx]).Amount += amount;

                            pData.Items[freeIdx].Update();
                        }
                        else
                        {
                            if (item.Type == Types.PlacedItem && item.Item is Items.PlaceableItem placeableItem)
                                placeableItem.Remove(pData);
                            else
                                item.Delete(false);

                            pData.Items[freeIdx] = item.Item;
                        }
                    }
                    else
                    {
                        ((Items.IStackable)item.Item).Amount -= amount;

                        item.UpdateAmount();

                        if (pData.Items[freeIdx] != null)
                        {
                            ((Items.IStackable)pData.Items[freeIdx]).Amount += amount;

                            pData.Items[freeIdx].Update();
                        }
                        else
                        {
                            pData.Items[freeIdx] = Items.Stuff.CreateItem(item.Item.ID, 0, amount);
                        }
                    }

                    player.InventoryUpdate(GroupTypes.Items, freeIdx, pData.Items[freeIdx].ToClientJson(GroupTypes.Items));

                    MySQL.CharacterItemsUpdate(pData.Info);
                }

                [RemoteEvent("Item::IOGL")]
                private static void ItemOnGroundLock(Player player, uint uid, bool state)
                {
                    (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                    if (sRes.IsSpammer)
                        return;

                    PlayerData pData = sRes.Data;

                    if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                        return;

                    ItemOnGround iog = GetItemOnGround(uid);

                    if (iog == null || iog.Type != Types.PlacedItem)
                        return;

                    if (!iog.PlayerHasAccess(pData, false, true))
                        return;

                    if (iog.IsLocked == state)
                        return;

                    iog.IsLocked = state;
                }
            }
        }
    }
}