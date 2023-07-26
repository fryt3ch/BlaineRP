using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Estates;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class Elevator
    {
        public enum ContextTypes
        {
            None = 0,

            ApartmentsRoot,
        }

        public Elevator()
        {
            TempEscBind = -1;

            Events.Add("Elevator::Floor",
                (args) =>
                {
                    var floor = (int)args[0];

                    if (HouseMenu.LastSent.IsSpam(500, false, false))
                        return;

                    if (CurrentContext == ContextTypes.ApartmentsRoot)
                    {
                        ApartmentsRoot aRoot = Player.LocalPlayer.GetData<ApartmentsRoot>("ApartmentsRoot::Current");

                        if (aRoot == null)
                            return;

                        ApartmentsRoot.ShellData shell = aRoot.Shell;

                        int elevI, elevJ;

                        if (!shell.GetClosestElevator(Player.LocalPlayer.Position, out elevI, out elevJ))
                            return;

                        if (floor < shell.StartFloor)
                            floor = shell.StartFloor;

                        int curFloor = elevI + shell.StartFloor;

                        if (curFloor == floor)
                        {
                            Notification.ShowError(Locale.Notifications.General.ElevatorCurrentFloor);

                            return;
                        }

                        Events.CallRemote("ARoot::Elevator", elevI, elevJ, floor - shell.StartFloor);

                        HouseMenu.LastSent = World.Core.ServerTime;

                        Close();
                    }
                }
            );
        }

        public static bool IsActive => Browser.IsActive(Browser.IntTypes.Elevator);

        private static int TempEscBind { get; set; }

        private static ContextTypes CurrentContext { get; set; }

        public static async System.Threading.Tasks.Task Show(int maxFloor, int? curFloor, ContextTypes contextType)
        {
            if (IsActive)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            await Browser.Render(Browser.IntTypes.Elevator, true, true);

            CurrentContext = contextType;

            Browser.Window.ExecuteJs("Elevator.setMaxFloor", maxFloor);
            Browser.Window.ExecuteJs("Elevator.setCurrentFloor", curFloor);

            Cursor.Show(true, true);

            TempEscBind = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CurrentContext = ContextTypes.None;

            Browser.Render(Browser.IntTypes.Elevator, false);

            if (TempEscBind != -1)
                Input.Core.Unbind(TempEscBind);

            TempEscBind = -1;

            Cursor.Show(false, false);
        }
    }
}