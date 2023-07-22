using GTANetworkAPI;

namespace BlaineRP.Server.Events.Players.Jobs
{
    public class Main : Script
    {
        [RemoteProc("Job::GTCSI")]
        private static uint GetTotalCashSalaryInfo(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            return Game.Jobs.Job.GetPlayerTotalCashSalary(pData);
        }
    }
}
