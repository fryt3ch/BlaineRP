using GTANetworkAPI;

namespace BlaineRP.Server.Events.Players
{
    internal class Reports : Script
    {
        [RemoteProc("Report::Send")]
        private static bool Send(Player player, string message)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (message == null)
                return false;

            if (pData.IsMuted)
                return false;

            var currentReport = Sync.Report.GetByStarterPlayer(pData.Info);

            if (currentReport == null)
            {
                currentReport = new Sync.Report(pData.Info, message);
            }
            else
            {
                currentReport.AddMessage(pData, message);
            }

            return true;
        }
    }
}
