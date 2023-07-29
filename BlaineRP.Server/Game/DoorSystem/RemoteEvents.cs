using BlaineRP.Server.Game.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.DoorSystem
{
    public class RemoteEvents : Script
    {
        [RemoteProc("Door::Lock")]
        public static bool DoorLock(Player player, uint doorId, bool state)
        {
            (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            PlayerData pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            Door door = Service.GetDoorById(doorId);

            if (door == null)
                return false;

            if (door.Dimension != player.Dimension || player.Position.DistanceTo(door.Position) > 5f)
                return false;

            if (Door.GetLockState(doorId) == state)
                return false;

            if (!door.GetCheckFunctionResult(pData))
                return false;

            Door.SetLockState(doorId, state, true);

            return true;
        }
    }
}