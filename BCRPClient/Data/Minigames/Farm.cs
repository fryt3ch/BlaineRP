using RAGE;
using RAGE.Elements;

namespace BCRPClient.Data.Minigames
{
    public class Farm : Events.Script
    {
        private static AsyncTask TempTask { get; set; }

        public Farm()
        {
            Events.Add("MG::OTC::S", (args) =>
            {
                StartOrangeTreeCollect();
            });

            Events.Add("MG::COWC::S", (args) =>
            {
                StartCowMilk();
            });
        }

        public static void StartCowMilk()
        {
            KeyBinds.Get(KeyBinds.Types.Crouch).Disable();

            GameEvents.Update -= RenderCowMilk;
            GameEvents.Update += RenderCowMilk;

            TempTask?.Cancel();

            TempTask = new AsyncTask(() =>
            {
                StopCowMilk(false);

                Events.CallRemote("Job::FARM::COWFC");
            }, 5000, false, 0);

            TempTask.Run();
        }

        public static void StopCowMilk(bool callRemote)
        {
            Player.LocalPlayer.GetData<MapObject>("FARMAT::TEMPBUCKET0")?.Destroy();

            Player.LocalPlayer.ResetData("FARMAT::TEMPBUCKET0");

            KeyBinds.Get(KeyBinds.Types.Crouch).Enable();

            GameEvents.Update -= RenderCowMilk;

            TempTask?.Cancel();

            TempTask = null;

            if (callRemote)
            {
                Events.CallRemote("Job::FARM::SCOWP");
            }
        }

        private static void RenderCowMilk()
        {
            if (!Utils.CanDoSomething(false, Utils.Actions.Knocked, Utils.Actions.Frozen, Utils.Actions.Cuffed))
            {
                StopCowMilk(true);

                return;
            }
        }

        public static void StartOrangeTreeCollect()
        {
            GameEvents.Update -= RenderOrangeTreeCollect;
            GameEvents.Update += RenderOrangeTreeCollect;

            TempTask?.Cancel();

            TempTask = new AsyncTask(() =>
            {
                StopOrangeTreeCollect(false);

                Events.CallRemote("Job::FARM::OTFC");
            }, 5000, false, 0);

            TempTask.Run();
        }

        public static void StopOrangeTreeCollect(bool callRemote)
        {
            GameEvents.Update -= RenderOrangeTreeCollect;

            TempTask?.Cancel();

            TempTask = null;

            if (callRemote)
            {
                Events.CallRemote("Job::FARM::SOTP");
            }
        }

        private static void RenderOrangeTreeCollect()
        {
            if (!Utils.CanDoSomething(false, Utils.Actions.Knocked, Utils.Actions.Frozen, Utils.Actions.Cuffed))
            {
                StopOrangeTreeCollect(true);

                return;
            }
        }
    }
}
