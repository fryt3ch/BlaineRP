using System;
using BlaineRP.Server.Game.Inventory;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Items
{
    public partial class WeaponSkin
    {
        internal class RemoteEvents : Script
        {
            [RemoteProc("WSkins::Rm")]
            private static bool WeaponSkinsRemove(Player player, int wSkinTypeNum)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                if (!Enum.IsDefined(typeof(ItemData.Types), wSkinTypeNum))
                    return false;

                var wSkinType = (ItemData.Types)wSkinTypeNum;

                var pData = sRes.Data;

                if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return false;

                int wsIdx = -1;

                for (var i = 0; i < pData.Info.WeaponSkins.Count; i++)
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

                int freeIdx = -1;

                for (var i = 0; i < pData.Items.Length; i++)
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

                player.InventoryUpdate(GroupTypes.Items, freeIdx, ws.ToClientJson(GroupTypes.Items));

                player.TriggerEvent("Player::WSkins::Update", false, ws.ID);

                MySQL.CharacterWeaponSkinsUpdate(pData.Info);
                MySQL.CharacterItemsUpdate(pData.Info);

                for (var i = 0; i < pData.Weapons.Length; i++)
                {
                    if (pData.Weapons[i] is Weapon weapon)
                        weapon.UpdateWeaponComponents(pData);
                }

                if (pData.Holster?.Items[0] is Weapon hWeapon)
                    hWeapon.UpdateWeaponComponents(pData);

                return true;
            }
        }
    }
}