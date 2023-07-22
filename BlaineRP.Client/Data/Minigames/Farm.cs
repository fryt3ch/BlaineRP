using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Data.Minigames
{
    [Script(int.MaxValue)]
    public class Farm 
    {
        private static AsyncTask TempTask { get; set; }

        public Farm()
        {
            Events.Add("MG::OTC::S", (args) =>
            {
                var orangesAmount = (int)args[0];

                StartOrangeTreeCollect(orangesAmount);
            });

            Events.Add("MG::COWC::S", (args) =>
            {
                StartCowMilk();
            });

            Events.Add("MiniGames::OrangesCollected", (args) =>
            {
                StopOrangeTreeCollect(false);

                Events.CallRemote("Job::FARM::OTFC");
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

        public static async void StartOrangeTreeCollect(int orangesAmount)
        {
            GameEvents.Update -= RenderOrangeTreeCollect;
            GameEvents.Update += RenderOrangeTreeCollect;

            await CEF.Browser.Render(CEF.Browser.IntTypes.MinigameOrangePicking, true, true);

            if (!Settings.User.Interface.HideHUD)
                CEF.HUD.ShowHUD(false);

            CEF.Chat.Show(false);

            CEF.Browser.Window.ExecuteJs("MG.OP.draw", orangesAmount);

            CEF.Cursor.Show(true, true);

            RAGE.Game.Graphics.TransitionToBlurred(0f);

            if (Player.LocalPlayer.HasData("MG::TempData::OrangePicking::EscBind"))
            {
                var bind = Player.LocalPlayer.GetData<int>("MG::TempData::OrangePicking::EscBind");

                KeyBinds.Unbind(bind);
            }

            Player.LocalPlayer.SetData("MG::TempData::OrangePicking::EscBind", KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => StopOrangeTreeCollect(true)));
        }

        public static void StopOrangeTreeCollect(bool callRemote)
        {
            GameEvents.Update -= RenderOrangeTreeCollect;

            RAGE.Game.Graphics.TransitionFromBlurred(0f);

            if (!Settings.User.Interface.HideHUD)
                CEF.HUD.ShowHUD(true);

            CEF.Chat.Show(true);

            CEF.Browser.Render(CEF.Browser.IntTypes.MinigameOrangePicking, false, false);

            CEF.Cursor.Show(false, false);

            if (Player.LocalPlayer.HasData("MG::TempData::OrangePicking::EscBind"))
            {
                var bind = Player.LocalPlayer.GetData<int>("MG::TempData::OrangePicking::EscBind");

                KeyBinds.Unbind(bind);

                Player.LocalPlayer.ResetData("MG::TempData::OrangePicking::EscBind");
            }

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
