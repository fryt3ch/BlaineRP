using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Game.Input.Enums;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Scripts
{
    [Script(int.MaxValue)]
    public class Farm
    {
        public Farm()
        {
            Events.Add("MG::OTC::S",
                (args) =>
                {
                    var orangesAmount = (int)args[0];

                    StartOrangeTreeCollect(orangesAmount);
                }
            );

            Events.Add("MG::COWC::S",
                (args) =>
                {
                    StartCowMilk();
                }
            );

            Events.Add("MiniGames::OrangesCollected",
                (args) =>
                {
                    StopOrangeTreeCollect(false);

                    Events.CallRemote("Job::FARM::OTFC");
                }
            );
        }

        private static AsyncTask TempTask { get; set; }

        public static void StartCowMilk()
        {
            Input.Core.Get(BindTypes.Crouch).Disable();

            Main.Update -= RenderCowMilk;
            Main.Update += RenderCowMilk;

            TempTask?.Cancel();

            TempTask = new AsyncTask(() =>
                {
                    StopCowMilk(false);

                    Events.CallRemote("Job::FARM::COWFC");
                },
                5000,
                false,
                0
            );

            TempTask.Run();
        }

        public static void StopCowMilk(bool callRemote)
        {
            Player.LocalPlayer.GetData<MapObject>("FARMAT::TEMPBUCKET0")?.Destroy();

            Player.LocalPlayer.ResetData("FARMAT::TEMPBUCKET0");

            Input.Core.Get(BindTypes.Crouch).Enable();

            Main.Update -= RenderCowMilk;

            TempTask?.Cancel();

            TempTask = null;

            if (callRemote)
                Events.CallRemote("Job::FARM::SCOWP");
        }

        private static void RenderCowMilk()
        {
            if (PlayerActions.IsAnyActionActive(false, PlayerActions.Types.Knocked, PlayerActions.Types.Frozen, PlayerActions.Types.Cuffed))
            {
                StopCowMilk(true);

                return;
            }
        }

        public static async void StartOrangeTreeCollect(int orangesAmount)
        {
            Main.Update -= RenderOrangeTreeCollect;
            Main.Update += RenderOrangeTreeCollect;

            await Browser.Render(Browser.IntTypes.MinigameOrangePicking, true, true);

            if (!Settings.User.Interface.HideHUD)
                HUD.ShowHUD(false);

            UI.CEF.Chat.Show(false);

            Browser.Window.ExecuteJs("MG.OP.draw", orangesAmount);

            Cursor.Show(true, true);

            RAGE.Game.Graphics.TransitionToBlurred(0f);

            if (Player.LocalPlayer.HasData("MG::TempData::OrangePicking::EscBind"))
            {
                int bind = Player.LocalPlayer.GetData<int>("MG::TempData::OrangePicking::EscBind");

                Input.Core.Unbind(bind);
            }

            Player.LocalPlayer.SetData("MG::TempData::OrangePicking::EscBind", Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => StopOrangeTreeCollect(true)));
        }

        public static void StopOrangeTreeCollect(bool callRemote)
        {
            Main.Update -= RenderOrangeTreeCollect;

            RAGE.Game.Graphics.TransitionFromBlurred(0f);

            if (!Settings.User.Interface.HideHUD)
                HUD.ShowHUD(true);

            UI.CEF.Chat.Show(true);

            Browser.Render(Browser.IntTypes.MinigameOrangePicking, false, false);

            Cursor.Show(false, false);

            if (Player.LocalPlayer.HasData("MG::TempData::OrangePicking::EscBind"))
            {
                int bind = Player.LocalPlayer.GetData<int>("MG::TempData::OrangePicking::EscBind");

                Input.Core.Unbind(bind);

                Player.LocalPlayer.ResetData("MG::TempData::OrangePicking::EscBind");
            }

            if (callRemote)
                Events.CallRemote("Job::FARM::SOTP");
        }

        private static void RenderOrangeTreeCollect()
        {
            if (PlayerActions.IsAnyActionActive(false, PlayerActions.Types.Knocked, PlayerActions.Types.Frozen, PlayerActions.Types.Cuffed))
            {
                StopOrangeTreeCollect(true);

                return;
            }
        }
    }
}