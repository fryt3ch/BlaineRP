using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.Sync
{
    class WeaponSystem : Script
    {
        public WeaponSystem()
        {

        }

        #region Update Ammo
        public static void UpdateAmmo(PlayerData pData, Game.Items.Weapon weapon, bool setUnarmedAfter = false)
        {
            var player = pData.Player;

            if (setUnarmedAfter)
                weapon.Equiped = false;

            var lastAmmo = weapon.Ammo;

            var curAmmo = NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return -1;

                var amount = NAPI.Player.GetPlayerWeaponAmmo(player, weapon.Data.Hash);

                if (setUnarmedAfter)
                    player.SetWeapon((uint)WeaponHash.Unarmed);

                return amount;
            }).GetAwaiter().GetResult();

            if (curAmmo < 0)
                return;

            if (curAmmo < lastAmmo)
            {
                weapon.Ammo = curAmmo;
                weapon.Update();
            }
        }
        #endregion

        #region Reload
        [RemoteEvent("Weapon::Reload")]
        public static async Task Reload(Player player, int currentAmmo)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var weapon = pData.ActiveWeapon;

                if (weapon == null)
                    return;

                if (currentAmmo > weapon.Value.WeaponItem.Ammo || currentAmmo < 0)
                    currentAmmo = 0;

                if (currentAmmo == weapon.Value.WeaponItem.Data.MaxAmmo)
                    return;

                weapon.Value.WeaponItem.Ammo = currentAmmo;

                await pData.InventoryAction(weapon.Value.Group, weapon.Value.Slot, 6);
            });

            pData.Release();
        }
        #endregion
    }
}
