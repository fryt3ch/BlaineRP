using GTANetworkAPI;

namespace BlaineRP.Server.Game.Jobs
{
    public abstract partial class Job
    {
        public class RemoteEvents : Script
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
}