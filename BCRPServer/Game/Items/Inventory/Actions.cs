using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Items
{
    public partial class Inventory
    {
        private static Dictionary<Type, Dictionary<int, Func<PlayerData, Game.Items.Item, GroupTypes, int, string[], ResultTypes>>> Actions { get; set; } = new Dictionary<Type, Dictionary<int, Func<PlayerData, Game.Items.Item, GroupTypes, int, string[], ResultTypes>>>()
        {
            {
                typeof(Game.Items.Weapon),

                new Dictionary<int, Func<PlayerData, Game.Items.Item, GroupTypes, int, string[], ResultTypes>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            var weapon = (Game.Items.Weapon)item;

                            if (group == GroupTypes.Items || group == GroupTypes.Bag)
                            {
                                int newSlot = 0;

                                if (pData.Weapons[0] != null)
                                {
                                    newSlot = 1;

                                    if (pData.Weapons[1] != null)
                                    {
                                        newSlot = -1;

                                        if (pData.Holster?.Items[0] != null)
                                            newSlot = 2;
                                    }
                                }

                                if (newSlot < 0)
                                    return ResultTypes.Error;

                                return Replace(pData, newSlot != 2 ? GroupTypes.Weapons : GroupTypes.Holster, newSlot, group, slot, -1);
                            }
                            else if (group == GroupTypes.Weapons)
                            {
                                if (weapon.Equiped)
                                {
                                    weapon.Unequip(pData);

                                    player.InventoryUpdate(group, slot, Game.Items.Item.ToClientJson(weapon, group));

                                    Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_PLAYER_WEAPON_UNEQUIP_0", Game.Items.Stuff.GetItemNameWithTag(weapon, null, out _)));
                                }
                                else
                                {
                                    if (player.Vehicle != null && !pData.Weapons[slot].Data.CanUseInVehicle)
                                    {
                                        player.Notify("Weapon::InVehicleRestricted");
                                    }
                                    else
                                    {
                                        int idxToCheck = slot == 0 ? 1 : 0;

                                        if (pData.Weapons[idxToCheck]?.Equiped == true)
                                        {
                                            pData.Weapons[idxToCheck].Unequip(pData);

                                            player.InventoryUpdate(group, idxToCheck, Game.Items.Item.ToClientJson(pData.Weapons[idxToCheck], group));
                                        }
                                        else if (pData.Holster != null && pData.Holster.Items[0] is Game.Items.Weapon hWeapon && hWeapon.Equiped)
                                        {
                                            hWeapon.Unequip(pData);

                                            player.InventoryUpdate(GroupTypes.Holster, 2, Game.Items.Item.ToClientJson(hWeapon, GroupTypes.Holster));
                                        }

                                        weapon.Equip(pData);

                                        player.InventoryUpdate(group, slot, Game.Items.Item.ToClientJson(weapon, group));

                                        Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_PLAYER_WEAPON_EQUIP_0", Game.Items.Stuff.GetItemNameWithTag(weapon, null, out _)));
                                    }
                                }

                                return ResultTypes.Success;
                            }
                            else if (group == GroupTypes.Holster)
                            {
                                if (weapon.Equiped)
                                {
                                    weapon.Unequip(pData);

                                    player.InventoryUpdate(group, 2, Game.Items.Item.ToClientJson(weapon, group));

                                    Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_PLAYER_WEAPON_UNEQUIP_1", Game.Items.Stuff.GetItemNameWithTag(weapon, null, out _)));
                                }
                                else
                                {
                                    for (int i = 0; i < pData.Weapons.Length; i++)
                                    {
                                        if (pData.Weapons[i] is Game.Items.Weapon wWeapon && wWeapon.Equiped)
                                        {
                                            wWeapon.Unequip(pData);

                                            player.InventoryUpdate(GroupTypes.Weapons, i, Game.Items.Item.ToClientJson(wWeapon, GroupTypes.Weapons));
                                        }
                                    }

                                    weapon.Equip(pData);

                                    player.InventoryUpdate(group, 2, Game.Items.Item.ToClientJson(weapon, group));

                                    Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_PLAYER_WEAPON_EQUIP_1", Game.Items.Stuff.GetItemNameWithTag(weapon, null, out _)));
                                }

                                return ResultTypes.Success;
                            }

                            return ResultTypes.Error;
                        }
                    },

                    {
                        6,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            var weapon = (Game.Items.Weapon)item;

                            if (group == GroupTypes.Weapons || group == GroupTypes.Holster)
                            {
                                int ammoToFill = weapon.Data.MaxAmmo - weapon.Ammo;

                                if (ammoToFill <= 0)
                                    return ResultTypes.Success;

                                int ammoIdx = -1;
                                int maxAmmo = 0;

                                for (int i = 0; i < pData.Items.Length; i++)
                                {
                                    if (pData.Items[i] is Game.Items.Ammo amItem && amItem.ID == weapon.Data.AmmoID && maxAmmo < amItem.Amount)
                                    {
                                        ammoIdx = i;
                                        maxAmmo = amItem.Amount;
                                    }
                                }

                                if (ammoIdx < 0)
                                    return ResultTypes.Error;

                                return Replace(pData, group, slot, GroupTypes.Items, ammoIdx, ammoToFill);
                            }

                            return ResultTypes.Error;
                        }
                    },

                    {
                        7,

                        (pData, item, group, slot, args) =>
                        {
                            if (group == GroupTypes.Weapons || group == GroupTypes.Holster)
                            {
                                var weapon = (Game.Items.Weapon)item;

                                if (weapon.Items[0] is Game.Items.WeaponComponent wc)
                                {
                                    var freeIdx = -1;
                                    var totalWeight = 0f;

                                    for (int i = 0; i < pData.Items.Length; i++)
                                    {
                                        if (pData.Items[i] == null)
                                        {
                                            if (freeIdx < 0)
                                                freeIdx = i;
                                        }
                                        else
                                        {
                                            totalWeight += pData.Items[i].Weight;
                                        }
                                    }

                                    if (freeIdx < 0 || (totalWeight + wc.Weight >= Properties.Settings.Static.MAX_INVENTORY_WEIGHT))
                                        return ResultTypes.NoSpace;

                                    pData.Items[freeIdx] = wc;
                                    weapon.Items[0] = null;

                                    weapon.UpdateWeaponComponents(pData);

                                    pData.Player.InventoryUpdate(group, slot, weapon.ToClientJson(group));
                                    pData.Player.InventoryUpdate(GroupTypes.Items, freeIdx, wc.ToClientJson(GroupTypes.Items));

                                    weapon.Update();

                                    MySQL.CharacterItemsUpdate(pData.Info);
                                }
                            }

                            return ResultTypes.Error;
                        }
                    },

                    {
                        8,

                        (pData, item, group, slot, args) =>
                        {
                            if (group == GroupTypes.Weapons || group == GroupTypes.Holster)
                            {
                                var weapon = (Game.Items.Weapon)item;

                                if (weapon.Items[1] is Game.Items.WeaponComponent wc)
                                {
                                    var freeIdx = -1;
                                    var totalWeight = 0f;

                                    for (int i = 0; i < pData.Items.Length; i++)
                                    {
                                        if (pData.Items[i] == null)
                                        {
                                            if (freeIdx < 0)
                                                freeIdx = i;
                                        }
                                        else
                                        {
                                            totalWeight += pData.Items[i].Weight;
                                        }
                                    }

                                    if (freeIdx < 0 || (totalWeight + wc.Weight >= Properties.Settings.Static.MAX_INVENTORY_WEIGHT))
                                        return ResultTypes.NoSpace;

                                    pData.Items[freeIdx] = wc;
                                    weapon.Items[1] = null;

                                    weapon.UpdateWeaponComponents(pData);

                                    pData.Player.InventoryUpdate(group, slot, weapon.ToClientJson(group));
                                    pData.Player.InventoryUpdate(GroupTypes.Items, freeIdx, wc.ToClientJson(GroupTypes.Items));

                                    weapon.Update();

                                    MySQL.CharacterItemsUpdate(pData.Info);
                                }
                            }

                            return ResultTypes.Error;
                        }
                    },

                    {
                        9,

                        (pData, item, group, slot, args) =>
                        {
                            if (group == GroupTypes.Weapons || group == GroupTypes.Holster)
                            {
                                var weapon = (Game.Items.Weapon)item;

                                if (weapon.Items[2] is Game.Items.WeaponComponent wc)
                                {
                                    var freeIdx = -1;
                                    var totalWeight = 0f;

                                    for (int i = 0; i < pData.Items.Length; i++)
                                    {
                                        if (pData.Items[i] == null)
                                        {
                                            if (freeIdx < 0)
                                                freeIdx = i;
                                        }
                                        else
                                        {
                                            totalWeight += pData.Items[i].Weight;
                                        }
                                    }

                                    if (freeIdx < 0 || (totalWeight + wc.Weight >= Properties.Settings.Static.MAX_INVENTORY_WEIGHT))
                                        return ResultTypes.NoSpace;

                                    pData.Items[freeIdx] = wc;
                                    weapon.Items[2] = null;

                                    weapon.UpdateWeaponComponents(pData);

                                    pData.Player.InventoryUpdate(group, slot, weapon.ToClientJson(group));
                                    pData.Player.InventoryUpdate(GroupTypes.Items, freeIdx, wc.ToClientJson(GroupTypes.Items));

                                    weapon.Update();

                                    MySQL.CharacterItemsUpdate(pData.Info);
                                }
                            }

                            return ResultTypes.Error;
                        }
                    },

                    {
                        10,

                        (pData, item, group, slot, args) =>
                        {
                            if (group == GroupTypes.Weapons || group == GroupTypes.Holster)
                            {
                                var weapon = (Game.Items.Weapon)item;

                                if (weapon.Items[3] is Game.Items.WeaponComponent wc)
                                {
                                    var freeIdx = -1;
                                    var totalWeight = 0f;

                                    for (int i = 0; i < pData.Items.Length; i++)
                                    {
                                        if (pData.Items[i] == null)
                                        {
                                            if (freeIdx < 0)
                                                freeIdx = i;
                                        }
                                        else
                                        {
                                            totalWeight += pData.Items[i].Weight;
                                        }
                                    }

                                    if (freeIdx < 0 || (totalWeight + wc.Weight >= Properties.Settings.Static.MAX_INVENTORY_WEIGHT))
                                        return ResultTypes.NoSpace;

                                    pData.Items[freeIdx] = wc;
                                    weapon.Items[3] = null;

                                    weapon.UpdateWeaponComponents(pData);

                                    pData.Player.InventoryUpdate(group, slot, weapon.ToClientJson(group));
                                    pData.Player.InventoryUpdate(GroupTypes.Items, freeIdx, wc.ToClientJson(GroupTypes.Items));

                                    weapon.Update();

                                    MySQL.CharacterItemsUpdate(pData.Info);
                                }
                            }

                            return ResultTypes.Error;
                        }
                    },
                }
            },

            {
                typeof(Game.Items.Bag),

                new Dictionary<int, Func<PlayerData, Game.Items.Item, GroupTypes, int, string[], ResultTypes>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (group == GroupTypes.Items)
                            {
                                return Replace(pData, GroupTypes.BagItem, 0, group, slot, -1);
                            }

                            return ResultTypes.Error;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.Holster),

                new Dictionary<int, Func<PlayerData, Game.Items.Item, GroupTypes, int, string[], ResultTypes>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (group == GroupTypes.Items || group == GroupTypes.Bag)
                            {
                                return Replace(pData, GroupTypes.HolsterItem, 0, group, slot, -1);
                            }

                            return ResultTypes.Error;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.Armour),

                new Dictionary<int, Func<PlayerData, Game.Items.Item, GroupTypes, int, string[], ResultTypes>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (group == GroupTypes.Items || group == GroupTypes.Bag)
                            {
                                return Replace(pData, GroupTypes.Armour, 0, group, slot, -1);
                            }

                            return ResultTypes.Error;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.Clothes),

                new Dictionary<int, Func<PlayerData, Game.Items.Item, GroupTypes, int, string[], ResultTypes>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var clothes = (Game.Items.Clothes)item;

                            var player = pData.Player;

                            if (group == GroupTypes.Items || group == GroupTypes.Bag)
                            {
                                int slotTo;

                                if (AccessoriesSlots.TryGetValue(item.Type, out slotTo))
                                {
                                    return Replace(pData, GroupTypes.Accessories, slotTo, group, slot, -1);
                                }
                                else
                                {
                                    return Replace(pData, GroupTypes.Clothes, ClothesSlots[item.Type], group, slot, -1);
                                }
                            }
                            else if (group == GroupTypes.Clothes || group == GroupTypes.Accessories)
                            {
                                if (item is Game.Items.Clothes.IToggleable tItem)
                                {
                                    if (Game.Data.Customization.IsUniformElementActive(pData, clothes is Game.Items.Clothes.IProp ? 1000 + clothes.Slot : clothes.Slot, true))
                                        return ResultTypes.Error;

                                    tItem.Action(pData);
                                }

                                return ResultTypes.Success;
                            }

                            return ResultTypes.Error;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.StatusChanger),

                new Dictionary<int, Func<PlayerData, Game.Items.Item, GroupTypes, int, string[], ResultTypes>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (pData.IsAttachedToEntity != null || pData.IsAnyAnimOn() || pData.HasAnyItemInUse() || pData.HasAnyHandAttachedObject)
                            {
                                player.Notify("ASP::ARN");

                                return ResultTypes.Error;
                            }

                            if (group == GroupTypes.Items || group == GroupTypes.Bag)
                            {
                                ((Game.Items.StatusChanger)item).Apply(pData);

                                if (item is Game.Items.IStackable itemStackable)
                                {
                                    if (itemStackable.Amount == 1)
                                    {
                                        item.Delete();

                                        item = null;
                                    }
                                    else
                                        itemStackable.Amount -= 1;
                                }
                                else if (item is Game.Items.IConsumable itemConsumable)
                                {
                                    if (itemConsumable.Amount == 1)
                                    {
                                        item.Delete();

                                        item = null;
                                    }
                                    else
                                        itemConsumable.Amount -= 1;
                                }

                                if (group == GroupTypes.Bag)
                                {
                                    if (item == null)
                                    {
                                        pData.Bag.Items[slot] = null;

                                        pData.Bag.Update();
                                    }
                                    else
                                    {
                                        item.Update();
                                    }
                                }
                                else
                                {
                                    if (item == null)
                                    {
                                        pData.Items[slot] = null;

                                        MySQL.CharacterItemsUpdate(pData.Info);
                                    }
                                    else
                                    {
                                        item.Update();
                                    }
                                }

                                var upd = Game.Items.Item.ToClientJson(item, group);

                                player.InventoryUpdate(group, slot, upd);

                                return ResultTypes.Success;
                            }

                            return ResultTypes.Error;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.VehicleKey),

                new Dictionary<int, Func<PlayerData, Item, GroupTypes, int, string[], ResultTypes>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var vk = (Game.Items.VehicleKey)item;

                            var vInfo = vk.VehicleInfo;

                            if (!vk.IsKeyValid(vInfo))
                            {
                                pData.Player.Notify("Vehicle::KE");

                                return ResultTypes.Error;
                            }

                            if (Sync.Vehicles.TryLocateOwnedVehicle(pData, vInfo))
                                return ResultTypes.Error;

                            return ResultTypes.Success;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.WeaponSkin),

                new Dictionary<int, Func<PlayerData, Item, GroupTypes, int, string[], ResultTypes>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var ws = (Game.Items.WeaponSkin)item;

                            var wsData = ws.Data;

                            int oldWsIdx = -1;

                            for (int i = 0; i < pData.Info.WeaponSkins.Count; i++)
                            {
                                var x = pData.Info.WeaponSkins[i];

                                if (x.Data.Type == wsData.Type)
                                {
                                    oldWsIdx = i;

                                    break;
                                }
                            }

                            var oldWs = oldWsIdx >= 0 ? pData.Info.WeaponSkins[oldWsIdx] : null;

                            if (oldWs != null)
                            {
                                pData.Info.WeaponSkins.RemoveAt(oldWsIdx);

                                pData.Info.WeaponSkins.Add(ws);
                            }
                            else
                            {
                                pData.Info.WeaponSkins.Add(ws);
                            }

                            if (group == GroupTypes.Bag)
                            {
                                pData.Bag.Items[slot] = oldWs;

                                pData.Bag.Update();
                            }
                            else if (group == GroupTypes.Items)
                            {
                                pData.Items[slot] = oldWs;

                                MySQL.CharacterItemsUpdate(pData.Info);
                            }

                            MySQL.CharacterWeaponSkinsUpdate(pData.Info);

                            pData.Player.InventoryUpdate(group, slot, Game.Items.Item.ToClientJson(oldWs, group));

                            pData.Player.TriggerEvent("Player::WSkins::Update", true, ws.ID);

                            for (int i = 0; i < pData.Weapons.Length; i++)
                            {
                                if (pData.Weapons[i] is Game.Items.Weapon weapon)
                                    weapon.UpdateWeaponComponents(pData);
                            }

                            if (pData.Holster?.Items[0] is Game.Items.Weapon hWeapon)
                            {
                                hWeapon.UpdateWeaponComponents(pData);
                            }

                            return ResultTypes.Success;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.FishingRod),

                new Dictionary<int, Func<PlayerData, Item, GroupTypes, int, string[], ResultTypes>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (group != GroupTypes.Items)
                                return ResultTypes.ActionRestricted;

                            var itemU = (Game.Items.FishingRod)item;

                            if (itemU.InUse)
                            {
                                if (!itemU.StopUse(pData, group, slot, true))
                                    return ResultTypes.Error;
                            }
                            else
                            {
                                if (player.Vehicle != null || pData.IsAttachedToEntity != null || pData.IsAnyAnimOn() || pData.HasAnyItemInUse() || pData.HasAnyHandAttachedObject)
                                {
                                    player.Notify("ASP::ARN");

                                    return ResultTypes.Error;
                                }

                                if (!itemU.StartUse(pData, group, slot, true, Game.Items.FishingRod.ItemData.BaitId))
                                    return ResultTypes.Error;
                            }

                            return ResultTypes.Success;
                        }
                    },

                    {
                        6,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (group != GroupTypes.Items)
                                return ResultTypes.ActionRestricted;

                            var itemU = (Game.Items.FishingRod)item;

                            if (itemU.InUse)
                            {
                                if (!itemU.StopUse(pData, group, slot, true))
                                    return ResultTypes.Error;
                            }
                            else
                            {
                                if (player.Vehicle != null || pData.IsAttachedToEntity != null || pData.IsAnyAnimOn() || pData.HasAnyItemInUse() || pData.HasAnyHandAttachedObject)
                                {
                                    player.Notify("ASP::ARN");

                                    return ResultTypes.Error;
                                }

                                if (!itemU.StartUse(pData, group, slot, true, Game.Items.FishingRod.ItemData.WormId))
                                    return ResultTypes.Error;
                            }

                            return ResultTypes.Success;
                        }
                    },
                }
            },

            {
                typeof(Game.Items.PlaceableItem),

                new Dictionary<int, Func<PlayerData, Item, GroupTypes, int, string[], ResultTypes>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            if (group != GroupTypes.Items)
                                return ResultTypes.ActionRestricted;

                            if (args.Length != 4)
                                return ResultTypes.Error;

                            float posX, posY, posZ, rotZ;

                            if (!float.TryParse(args[0], out posX) || !float.TryParse(args[1], out posY) || !float.TryParse(args[2], out posZ) || !float.TryParse(args[3], out rotZ))
                                return ResultTypes.Error;

                            var itemP = (Game.Items.PlaceableItem)item;

                            var iog = itemP.Install(pData, new GTANetworkAPI.Vector3(posX, posY, posZ), new GTANetworkAPI.Vector3(0f, 0f, rotZ));

                            if (iog != null)
                            {
                                pData.Items[slot] = null;

                                pData.Player.InventoryUpdate(group, slot, Item.ToClientJson(null, group));

                                MySQL.CharacterItemsUpdate(pData.Info);

                                return ResultTypes.Success;
                            }

                            return ResultTypes.Error;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.Shovel),

                new Dictionary<int, Func<PlayerData, Item, GroupTypes, int, string[], ResultTypes>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (group != GroupTypes.Items)
                                return ResultTypes.ActionRestricted;

                            var itemU = (Game.Items.Shovel)item;

                            if (itemU.InUse)
                            {
                                if (!itemU.StopUse(pData, group, slot, true))
                                    return ResultTypes.Error;
                            }
                            else
                            {
                                if (player.Vehicle != null || pData.IsAttachedToEntity != null || pData.IsAnyAnimOn() || pData.HasAnyItemInUse() || pData.HasAnyHandAttachedObject)
                                {
                                    player.Notify("ASP::ARN");

                                    return ResultTypes.Error;
                                }

                                if (!itemU.StartUse(pData, group, slot, true))
                                    return ResultTypes.Error;
                            }

                            return ResultTypes.Success;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.MetalDetector),

                new Dictionary<int, Func<PlayerData, Item, GroupTypes, int, string[], ResultTypes>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (group != GroupTypes.Items)
                                return ResultTypes.ActionRestricted;

                            var itemU = (Game.Items.MetalDetector)item;

                            if (itemU.InUse)
                            {
                                if (!itemU.StopUse(pData, group, slot, true))
                                    return ResultTypes.Error;
                            }
                            else
                            {
                                if (player.Vehicle != null || pData.IsAttachedToEntity != null || pData.IsAnyAnimOn() || pData.HasAnyItemInUse() || pData.HasAnyHandAttachedObject)
                                {
                                    player.Notify("ASP::ARN");

                                    return ResultTypes.Error;
                                }

                                if (!itemU.StartUse(pData, group, slot, true))
                                    return ResultTypes.Error;
                            }

                            return ResultTypes.Success;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.Parachute),

                new Dictionary<int, Func<PlayerData, Item, GroupTypes, int, string[], ResultTypes>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (group != GroupTypes.Items)
                                return ResultTypes.ActionRestricted;

                            var itemU = (Game.Items.Parachute)item;

                            if (itemU.InUse)
                            {
                                if (!itemU.StopUse(pData, group, slot, true))
                                    return ResultTypes.Error;
                            }
                            else
                            {
                                if (pData.AttachedObjects.Where(x => x.Type == Sync.AttachSystem.Types.ParachuteSync).Any() || pData.HasAnyItemInUse())
                                {
                                    player.Notify("ASP::ARN");

                                    return ResultTypes.Error;
                                }

                                if (!itemU.StartUse(pData, group, slot, true))
                                    return ResultTypes.Error;
                            }

                            return ResultTypes.Success;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.Note),

                new Dictionary<int, Func<PlayerData, Item, GroupTypes, int, string[], ResultTypes>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            var itemU = (Game.Items.Note)item;

                            var iData = itemU.Data;

                            if ((iData.Type & Note.ItemData.Types.Read) == Note.ItemData.Types.Read)
                            {
                                pData.Player.TriggerEvent("Inventory::Note", itemU.Text ?? itemU.Data.DefaultText);
                            }
                            else
                            {
                                pData.Player.Notify("Inv::NRNOTE");
                            }

                            return ResultTypes.Success;
                        }
                    },

                    {
                        6,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            var itemU = (Game.Items.Note)item;

                            var iData = itemU.Data;

                            if ((iData.Type & Note.ItemData.Types.Write) == Note.ItemData.Types.Write || (((iData.Type & Note.ItemData.Types.WriteTextNullOnly) == Note.ItemData.Types.WriteTextNullOnly) && itemU.Text == null))
                            {
                                pData.Player.TriggerEvent("Inventory::Note", (int)group, slot, itemU.Text ?? itemU.Data.DefaultText);
                            }
                            else
                            {
                                pData.Player.Notify("Inv::NWNOTE");
                            }

                            return ResultTypes.Success;
                        }
                    },
                }
            },
        };
    }
}
