using RAGE;
using RAGE.Elements;

namespace BCRPClient.Data
{
    public class Shovel : Events.Script
    {
        public Shovel()
        {
            Events.Add("MG::SHOV::S", (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                CancelAllTasks();

                if (args == null || args.Length == 0)
                    return;

                var timeToCatch = (int)args[0];

                AsyncTask task = null;

                task = new AsyncTask(async () =>
                {
                    await RAGE.Game.Invoker.WaitAsync(timeToCatch);

                    if (!Utils.IsTaskStillPending("MG::SHOV::S::D", task))
                        return;

                    Events.CallRemote("MG::SHOV::F");
                }, 0, false, 0);

                Utils.SetTaskAsPending("MG::SHOV::S::D", task);
            });
        }

        private static void CancelAllTasks()
        {
            Utils.CancelPendingTask("MG::SHOV::S::D");
        }
    }
}