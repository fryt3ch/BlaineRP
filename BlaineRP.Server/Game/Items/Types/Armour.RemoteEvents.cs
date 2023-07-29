using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Inventory;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Items
{
    public partial class Armour
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("Players::ArmourBroken")]
            private static void ArmourBroken(Player player)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                Armour arm = pData.Armour;

                if (arm == null)
                    return;

                pData.Armour = null;

                arm.Unwear(pData);

                player.InventoryUpdate(GroupTypes.Armour, ToClientJson(null, GroupTypes.Armour));

                MySQL.CharacterArmourUpdate(pData.Info);

                arm.Delete();
            }
        }
    }
}