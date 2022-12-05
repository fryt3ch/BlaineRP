using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.Sync
{
    class WeaponSystem
    {
        public static void UpdateAmmo(PlayerData pData, Game.Items.Weapon weapon, bool setUnarmedAfter = false)
        {
            var player = pData.Player;

            if (setUnarmedAfter)
                weapon.Equiped = false;

            var lastAmmo = weapon.Ammo;

            var curAmmo = NAPI.Player.GetPlayerWeaponAmmo(player, weapon.Data.Hash);

            if (setUnarmedAfter)
                player.SetWeapon((uint)WeaponHash.Unarmed);

            if (curAmmo < 0)
                return;

            if (curAmmo < lastAmmo)
            {
                weapon.Ammo = curAmmo;

                weapon.Update();
            }
        }
    }
}
