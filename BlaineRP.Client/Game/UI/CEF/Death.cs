using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.UI.CEF
{
    [Script(int.MaxValue)]
    public class Death
    {
        public static bool IsActive { get => Browser.IsActive(Browser.IntTypes.Death); }

        private static AsyncTask Task = null;

        private static int SecondsLeft = 0;

        public Death()
        {
            Events.Add("Death::Ressurect", (object[] args) =>
            {
                if (!IsActive)
                    return;

                Player.LocalPlayer.SetHealth(0);

                Close();
            });

            Events.Add("Death::Wait", (object[] args) =>
            {
                if (!IsActive)
                    return;

                CEF.Notification.Show(Notification.Types.Success, Locale.Notifications.General.Death.Header, Locale.Notifications.General.Death.EMSNotified);

                Browser.Window.ExecuteJs("Death.switchButton", "death-die", false);
                Browser.Window.ExecuteJs("Death.switchButton", "death-er", false);

                SecondsLeft += 300;
            });
        }

        public static async System.Threading.Tasks.Task Show()
        {
            if (IsActive || PlayerData.GetData(Player.LocalPlayer)?.IsKnocked != true)
                return;

            await Browser.Render(Browser.IntTypes.Death, true);

            Browser.Window.ExecuteJs("Death.switchButton", "death-die", false);
            Browser.Window.ExecuteJs("Death.switchButton", "death-er", true);

            Browser.Switch(Browser.IntTypes.Death, true);

            CEF.Cursor.Show(true, true);

            if (Task != null)
                Task.Cancel();

            SecondsLeft = 30;

            Task = new AsyncTask(() =>
            {
                if (SecondsLeft > 0)
                {
                    Browser.Window.ExecuteJs("Death.updateSecs", --SecondsLeft);

                    if (SecondsLeft == 0)
                        Browser.Window.ExecuteJs("Death.switchButton", "death-die", true);
                }
            }, 1000, true, 0);

            Task.Run();
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            Browser.Render(Browser.IntTypes.Death, false);

            Task?.Cancel();

            CEF.Cursor.Show(false, false);
        }
    }
}
