using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Inventory;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Items
{
    public partial class Weapon
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("Weapon::Reload")]
            public static void WeaponReload(Player player)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return;

                GroupTypes group;
                int slot;

                if (pData.TryGetActiveWeapon(out Weapon weapon, out group, out slot))
                    pData.InventoryAction(group, slot, 6);
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

                Weapon hWeapon = pData.Holster?.Weapon;

                if (hWeapon?.Equiped == true)
                    hWeapon.Ammo--;
            }
        }
    }
}