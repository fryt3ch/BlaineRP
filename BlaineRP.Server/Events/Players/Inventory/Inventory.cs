using GTANetworkAPI;
using System;
using System.Linq;
using static BlaineRP.Server.Game.Items.Inventory;

namespace BlaineRP.Server.Events.Players
{
    partial class Inventory : Script
    {
        [RemoteEvent("Inventory::Replace")]
        private static void InventoryReplace(Player player, int to, int slotTo, int from, int slotFrom, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Game.Items.Inventory.GroupTypes), to) || !Enum.IsDefined(typeof(Game.Items.Inventory.GroupTypes), from))
                return;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            Replace(pData, (Game.Items.Inventory.GroupTypes)to, slotTo, (Game.Items.Inventory.GroupTypes)from, slotFrom, amount);
        }

        [RemoteEvent("Inventory::Action")]
        private static void InventoryAction(Player player, int group, int slot, int action, string data)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (data == null)
                return;

            if (!Enum.IsDefined(typeof(Game.Items.Inventory.GroupTypes), group) || slot < 0 || action < 5)
                return;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            Action(pData, (Game.Items.Inventory.GroupTypes)group, slot, action, data.Split('&'));
        }

        [RemoteEvent("Inventory::Drop")]
        private static void InventoryDrop(Player player, int slotStr, int slot, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Game.Items.Inventory.GroupTypes), slotStr) || slot < 0)
                return;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            Drop(pData, (Game.Items.Inventory.GroupTypes)slotStr, slot, amount);
        }

        [RemoteEvent("Inventory::Take")]
        private static void InventoryTake(Player player, uint UID, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked || player.Vehicle != null)
                return;

            var item = Sync.World.GetItemOnGround(UID);

            if (item?.Item == null)
                return;

            if (item.Type == Sync.World.ItemOnGround.Types.PlacedItem && !item.PlayerHasAccess(pData, false, true))
                return;

            if (!player.IsNearToEntity(item.Object, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            var curWeight = pData.Items.Sum(x => x?.Weight ?? 0f);

            int freeIdx = -1, curAmount = 1;

            if (item.Item is Game.Items.IStackable stackable)
            {
                curAmount = stackable.Amount;

                if (amount > curAmount)
                    amount = curAmount;

                if (curWeight + amount * item.Item.BaseWeight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
                {
                    amount = (int)Math.Floor((Properties.Settings.Static.MAX_INVENTORY_WEIGHT - curWeight) / item.Item.BaseWeight);
                }

                if (amount > 0)
                {
                    for (int i = 0; i < pData.Items.Length; i++)
                    {
                        var curItem = pData.Items[i];

                        if (curItem == null)
                        {
                            if (freeIdx < 0)
                                freeIdx = i;

                            continue;
                        }

                        if (curItem.ID == item.Item.ID && curItem is Game.Items.IStackable curItemStackable && curItemStackable.Amount + amount <= curItemStackable.MaxAmount)
                        {
                            freeIdx = i;

                            break;
                        }
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
                pData.PlayAnim(Sync.Animations.FastTypes.Pickup, Properties.Settings.Static.InventoryPickupAnimationTime);

            if (amount == curAmount)
            {
                if (pData.Items[freeIdx] != null)
                {
                    item.Delete(true);

                    ((Game.Items.IStackable)pData.Items[freeIdx]).Amount += amount;

                    pData.Items[freeIdx].Update();
                }
                else
                {
                    if (item.Type == Sync.World.ItemOnGround.Types.PlacedItem && item.Item is Game.Items.PlaceableItem placeableItem)
                    {
                        placeableItem.Remove(pData);
                    }
                    else
                    {
                        item.Delete(false);
                    }

                    pData.Items[freeIdx] = item.Item;
                }
            }
            else
            {
                ((Game.Items.IStackable)item.Item).Amount -= amount;

                item.UpdateAmount();

                if (pData.Items[freeIdx] != null)
                {
                    ((Game.Items.IStackable)pData.Items[freeIdx]).Amount += amount;

                    pData.Items[freeIdx].Update();
                }
                else
                {
                    pData.Items[freeIdx] = Game.Items.Stuff.CreateItem(item.Item.ID, 0, amount);
                }
            }

            player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, freeIdx, pData.Items[freeIdx].ToClientJson(Game.Items.Inventory.GroupTypes.Items));

            MySQL.CharacterItemsUpdate(pData.Info);
        }

        [RemoteEvent("Item::IOGL")]
        private static void ItemOnGroundLock(Player player, uint uid, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            var iog = Sync.World.GetItemOnGround(uid);

            if (iog == null || iog.Type != Sync.World.ItemOnGround.Types.PlacedItem)
                return;

            if (!iog.PlayerHasAccess(pData, false, true))
                return;

            if (iog.IsLocked == state)
                return;

            iog.IsLocked = state;
        }

        [RemoteEvent("opws")]
        public static void OnPlayerWeaponShot(Player player)
        {
            if (player?.Exists != true)
                return;

            var pData = PlayerData.Get(player);

            if (pData == null)
                return;

            if (pData.Weapons[0]?.Equiped == true)
            {
                if (pData.Weapons[0].Ammo <= 0)
                    return;

                pData.Weapons[0].Ammo--;

                return;
            }
            else if (pData.Weapons[1]?.Equiped == true)
            {
                if (pData.Weapons[1].Ammo <= 0)
                    return;

                pData.Weapons[1].Ammo--;

                return;
            }

            var hWeapon = pData.Holster?.Weapon;

            if (hWeapon?.Equiped == true)
            {
                hWeapon.Ammo--;
            }
        }

        [RemoteEvent("Weapon::Reload")]
        public static void WeaponReload(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            Game.Items.Weapon weapon;
            Game.Items.Inventory.GroupTypes group;
            int slot;

            if (pData.TryGetActiveWeapon(out weapon, out group, out slot))
            {
                pData.InventoryAction(group, slot, 6);
            }
        }

        [RemoteEvent("Gift::Collect")]
        private static void GiftCollect(Player player, uint id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            var gift = pData.Gifts.Where(x => x.ID == id).FirstOrDefault();

            if (gift == null)
                return;

            if (gift.Collect(pData))
            {
                pData.Gifts.Remove(gift);

                gift.Delete();

                player.TriggerEvent("Menu::Gifts::Update", false, id);
            }
        }

        [RemoteProc("WSkins::Rm")]
        private static bool WeaponSkinsRemove(Player player, int wSkinTypeNum)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            if (!Enum.IsDefined(typeof(Game.Items.WeaponSkin.ItemData.Types), wSkinTypeNum))
                return false;

            var wSkinType = (Game.Items.WeaponSkin.ItemData.Types)wSkinTypeNum;

            var pData = sRes.Data;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return false;

            int wsIdx = -1;

            for (int i = 0; i < pData.Info.WeaponSkins.Count; i++)
            {
                var x = pData.Info.WeaponSkins[i];

                if (x.Data.Type == wSkinType)
                {
                    wsIdx = i;

                    break;
                }
            }

            if (wsIdx < 0)
                return false;

            var ws = pData.Info.WeaponSkins[wsIdx];

            var freeIdx = -1;

            for (int i = 0; i < pData.Items.Length; i++)
            {
                if (pData.Items[i] == null)
                {
                    freeIdx = i;

                    break;
                }
            }

            if (freeIdx < 0)
            {
                player.Notify("Inventory::NoSpace");

                return false;
            }

            pData.Info.WeaponSkins.RemoveAt(wsIdx);

            pData.Items[freeIdx] = ws;

            player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, freeIdx, ws.ToClientJson(Game.Items.Inventory.GroupTypes.Items));

            player.TriggerEvent("Player::WSkins::Update", false, ws.ID);

            MySQL.CharacterWeaponSkinsUpdate(pData.Info);
            MySQL.CharacterItemsUpdate(pData.Info);

            for (int i = 0; i < pData.Weapons.Length; i++)
            {
                if (pData.Weapons[i] is Game.Items.Weapon weapon)
                {
                    weapon.UpdateWeaponComponents(pData);
                }
            }

            if (pData.Holster?.Items[0] is Game.Items.Weapon hWeapon)
            {
                hWeapon.UpdateWeaponComponents(pData);
            }

            return true;
        }

        [RemoteEvent("Player::SUCI")]
        private static void StopUseCurrentItem(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            Game.Items.IUsable item;
            int slot;

            if (pData.TryGetCurrentItemInUse(out item, out slot))
            {
                item.StopUse(pData, Game.Items.Inventory.GroupTypes.Items, slot, true);
            }
        }

        [RemoteEvent("Player::ParachuteS")]
        private static void ParachuteState(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (state)
            {
                for (int slot = 0; slot < pData.Items.Length; slot++)
                {
                    if (pData.Items[slot] is Game.Items.Parachute parachute && parachute.InUse)
                    {
                        if (parachute.StopUse(pData, Game.Items.Inventory.GroupTypes.Items, slot, false, "DONT_CANCEL_TASK_CLIENT"))
                        {
                            parachute.Delete();

                            pData.Items[slot] = null;

                            player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, slot, Game.Items.Item.ToClientJson(pData.Items[slot], Game.Items.Inventory.GroupTypes.Items));

                            MySQL.CharacterItemsUpdate(pData.Info);

                            player.AttachObject(Sync.AttachSystem.Models.ParachuteSync, Sync.AttachSystem.Types.ParachuteSync, -1, null);

                            return;
                        }
                    }
                }
            }
            else
            {
                player.DetachObject(Sync.AttachSystem.Types.ParachuteSync);
            }
        }

        [RemoteProc("Player::NoteEdit")]
        private static bool NoteEdit(Player player, int invGroupNum, int slot, string text)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (slot < 0 || !Enum.IsDefined(typeof(Game.Items.Inventory.GroupTypes), invGroupNum))
                return false;

            var invGroup = (Game.Items.Inventory.GroupTypes)invGroupNum;

            if (invGroup != Game.Items.Inventory.GroupTypes.Items && invGroup != Game.Items.Inventory.GroupTypes.Bag)
                return false;

            // text check

            Game.Items.Note note;

            if (invGroup == Game.Items.Inventory.GroupTypes.Items)
            {
                if (slot >= pData.Items.Length)
                    return false;

                note = pData.Items[slot] as Game.Items.Note;
            }
            else
            {
                if (pData.Bag == null || slot >= pData.Bag.Items.Length)
                    return false;

                note = pData.Bag.Items[slot] as Game.Items.Note;
            }

            if (note == null)
                return false;

            var iData = note.Data;

            if ((iData.Type & Game.Items.Note.ItemData.Types.Write) == Game.Items.Note.ItemData.Types.Write || (((iData.Type & Game.Items.Note.ItemData.Types.WriteTextNullOnly) == Game.Items.Note.ItemData.Types.WriteTextNullOnly) && note.Text == null))
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

        [RemoteProc("Player::ResurrectItem")]
        private static byte ResurrectItem(Player player, Player target, int itemIdx)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (itemIdx < 0 || itemIdx >= pData.Items.Length)
                return 0;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked || pData.IsAttachedToEntity != null || pData.HasAnyHandAttachedObject || pData.AttachedEntities.Any())
                return 0;

            var tData = target.GetMainData();

            if (tData == null || pData == tData)
                return 0;

            if (!tData.Player.IsNearToEntity(pData.Player, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                return 0;

            if (!tData.IsKnocked || tData.IsAttachedToEntity != null)
                return 0;

            var healingItem = pData.Items[itemIdx] as Game.Items.Healing;

            if (healingItem == null || healingItem.Data.ResurrectionChance <= 0)
                return 1;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.EMS;

            var overrideResurrectChance = fData == null ? (double?)null : 1d;

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

            pData.Player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, itemIdx, Game.Items.Item.ToClientJson(pData.Items[itemIdx], Game.Items.Inventory.GroupTypes.Items));

            Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, pData.Player, Language.Strings.Get("CHAT_PLAYER_RESURRECT_0"), tData.Player);

            return 255;
        }

        [RemoteEvent("Player::ResurrectFinish")]
        private static void ResurrectFinish(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var attach = pData.AttachedEntities.Where(x => x.Type == Sync.AttachSystem.Types.PlayerResurrect && x.EntityType == EntityType.Player).FirstOrDefault();

            if (attach == null)
                return;

            var dataD = attach.SyncData.Split('_');

            var targetPlayer = Utils.GetPlayerByID(attach.Id);

            var tData = targetPlayer.GetMainData();

            if (tData == null)
                return;

            pData.Player.DetachEntity(targetPlayer);

            var resurrect = dataD[1] == "1";

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

            Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Try, pData.Player, Language.Strings.Get("CHAT_PLAYER_RESURRECT_1"), tData.Player, resurrect);
        }
    }
}
