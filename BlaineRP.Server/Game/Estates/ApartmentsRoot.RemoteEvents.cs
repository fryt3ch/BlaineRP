using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Estates
{
    public partial class ApartmentsRoot
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("ARoot::Elevator")]
            public static void ApartmentsRootElevator(Player player, ushort curFloor, ushort subIdx, ushort destFloor)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                ApartmentsRoot aRoot = pData.CurrentApartmentsRoot;

                if (aRoot == null)
                    return;

                ShellData shell = aRoot.Shell;

                Vector4 curFloorPos = shell.GetFloorPosition(curFloor, subIdx);

                if (curFloorPos == null)
                    return;

                if (Vector3.Distance(player.Position, curFloorPos.Position) > Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE)
                    return;

                curFloorPos = shell.GetFloorPosition(destFloor, subIdx);

                if (curFloorPos == null)
                    return;

                player.Teleport(curFloorPos.Position, false, aRoot.Dimension, curFloorPos.RotationZ, true);
            }

            [RemoteEvent("ARoot::Enter")]
            public static void ApartmentsRootEnter(Player player, uint id)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                var aRoot = Get(id);

                if (aRoot == null)
                    return;

                if (player.Dimension != Properties.Settings.Static.MainDimension ||
                    Vector3.Distance(player.Position, aRoot.EnterParams.Position) > Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE)
                    return;

                aRoot.SetPlayersInside(true, player);
            }

            [RemoteEvent("ARoot::Exit")]
            public static void ApartmentsRootExit(Player player)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                ApartmentsRoot aRoot = pData.CurrentApartmentsRoot;

                if (aRoot == null)
                    return;

                if (Vector3.Distance(player.Position, aRoot.Shell.EnterPosition.Position) > Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE)
                    return;

                aRoot.SetPlayersOutside(true, player);
            }
        }
    }
}