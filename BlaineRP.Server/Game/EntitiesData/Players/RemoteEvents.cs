using GTANetworkAPI;

namespace BlaineRP.Server.Game.EntitiesData.Players
{
    internal partial class RemoteEvents : Script
    {
        [ServerEvent(Event.PlayerWeaponSwitch)]
        private static void OnPlayerWeaponSwitch(Player player, uint oldWeapon, uint newWeapon)
        {
            if (oldWeapon == 2725352035 && newWeapon == oldWeapon)
                return;

            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.WeaponComponents != null)
                player.TriggerEventToStreamed("Players::WCD::U", player);
        }
        
        [RemoteEvent("dmswme")]
        private static void DamageSystemWoundMe(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsWounded || pData.IsKnocked)
                return;

            pData.IsWounded = true;
        }
    }
}