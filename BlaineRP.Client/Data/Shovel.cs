using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Data
{
    [Script(int.MaxValue)]
    public class Shovel
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

                    if (!AsyncTask.Methods.IsTaskStillPending("MG::SHOV::S::D", task))
                        return;

                    Events.CallRemote("MG::SHOV::F");
                }, 0, false, 0);

                AsyncTask.Methods.SetAsPending(task, "MG::SHOV::S::D");
            });
        }

        private static void CancelAllTasks()
        {
            AsyncTask.Methods.CancelPendingTask("MG::SHOV::S::D");
        }
    }
}