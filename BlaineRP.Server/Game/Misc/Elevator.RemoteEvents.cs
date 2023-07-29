using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Misc
{
    public partial class Elevator
    {
        internal class RemoteEvents : Script
        {
            [RemoteProc("Elevator::TP")]
            public static bool ElevatorTeleport(Player player, uint elevatorIdFrom, uint elevatorIdTo)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen || pData.IsAttachedToEntity != null)
                    return false;

                var elevatorFrom = Get(elevatorIdFrom);

                if (elevatorFrom == null)
                    return false;

                if (!elevatorFrom.LinkedElevators.Contains(elevatorIdTo))
                    return false;

                var elevatorTo = Get(elevatorIdTo);

                if (elevatorTo == null)
                    return false;

                if (player.Dimension != elevatorFrom.Dimension || player.Position.DistanceTo(elevatorFrom.Position.Position) > 5f)
                    return false;

                if (!elevatorTo.GetCheckFunctionResult(pData, true))
                    return false;

                Teleport(pData, elevatorFrom, elevatorTo, true);

                return true;
            }
        }
    }
}