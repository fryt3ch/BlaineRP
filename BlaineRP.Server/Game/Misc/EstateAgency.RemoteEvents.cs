using BlaineRP.Server.Game.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Misc
{
    public partial class EstateAgency
    {
        internal class RemoteEvents : Script
        {
            [RemoteProc("EstAgency::GPS")]
            public static bool EstateAgencyBuyGps(Player player, int estAgencyId, int posId, byte gpsType)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                PlayerData pData = sRes.Data;

                if (pData.IsFrozen)
                    return false;

                var estAgency = Get(estAgencyId);

                if (estAgency == null || posId < 0 || posId >= estAgency.Positions.Length)
                    return false;

                if (player.Dimension != Properties.Settings.Static.MainDimension || player.Position.DistanceTo(estAgency.Positions[posId]) > 5f)
                    return false;

                if (gpsType == 0)
                {
                    ulong newBalance;

                    if (!pData.TryRemoveCash(estAgency.HouseGPSPrice, out newBalance, true, null))
                        return false;

                    pData.SetCash(newBalance);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            
            [RemoteProc("EstAgency::GD")]
            public static string EstateAgencyGetData(Player player, int estAgencyId, int posId)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                var pData = sRes.Data;

                if (pData.IsFrozen)
                    return null;

                var estAgency = Game.Misc.EstateAgency.Get(estAgencyId);

                if (estAgency == null || posId < 0 || posId >= estAgency.Positions.Length)
                    return null;

                if (player.Dimension != Properties.Settings.Static.MainDimension || player.Position.DistanceTo(estAgency.Positions[posId]) > 5f)
                    return null;

                return $"{estAgency.HouseGPSPrice}";
            }
        }
    }
}