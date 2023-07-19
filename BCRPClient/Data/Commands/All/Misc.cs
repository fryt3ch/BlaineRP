using BCRPClient.Sync;
using RAGE;
using RAGE.Elements;
using System.Linq;

namespace BCRPClient.Data
{
    partial class Commands
    {
        [Command("ping", false, "")]
        public static async void Ping()
        {
            if (LastSent.IsSpam(500, false, true))
                return;

            LastSent = Sync.World.ServerTime;

            var res = Utils.ToDecimal(await Events.CallRemoteProc("Misc::GetPing"));

            Events.CallLocal("Chat::ShowServerMessage", $"[Ping] Ваш текущий пинг: {res}");
        }

        [Command("dl", false, "Показ дополнительных сведений о всех сущностях")]
        public static void DebugLabels(bool? state = null)
        {
            if (state == null)
                Settings.User.Other.DebugLabels = !Settings.User.Other.DebugLabels;
            else
                Settings.User.Other.DebugLabels = (bool)state;

            CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Notifications.Commands.Header, string.Format(Settings.User.Other.DebugLabels ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "DL"));
        }

        [Command("raytrace", false, "Показ дополнительных сведений о всех сущностях")]
        public static void Raytrace(bool? state = null)
        {
            if (state == null)
                Settings.User.Other.RaytraceEnabled = !Settings.User.Other.RaytraceEnabled;
            else
                Settings.User.Other.RaytraceEnabled = (bool)state;

            CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Notifications.Commands.Header, string.Format(Settings.User.Other.RaytraceEnabled ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "Raytace"));
        }

        [Command("lock", false, "Блокировка транспорта")]
        public static void Lock(uint? id = null, bool? state = null)
        {
            if (id == null)
            {
                Sync.Vehicles.Lock(state, null);
            }
            else
            {
                var veh = id > ushort.MaxValue ? RAGE.Elements.Entities.Vehicles.Streamed.Where(x => Sync.Vehicles.GetData(x)?.VID == id).FirstOrDefault() : RAGE.Elements.Entities.Vehicles.Streamed.Where(x => x.RemoteId == id).FirstOrDefault();

                if (veh == null)
                {
                    CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.Interaction.Header, Locale.Notifications.Interaction.NotFound);

                    return;
                }

                if (Vector3.Distance(Player.LocalPlayer.Position, veh.Position) > 10f)
                {
                    CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.Interaction.Header, Locale.Notifications.Interaction.DistanceTooLarge);

                    return;
                }

                Sync.Vehicles.Lock(state, veh);
            }
        }

        [Command("report", false, "Связь с администрацией", "rep", "ask")]
        public static void Report(string text)
        {
            CEF.Menu.ReportSend(text);
        }

        [Command("cleargps", false, "Связь с администрацией", "cgps", "gpsclear", "gpsc")]
        public static void ClearGPS()
        {
            Additional.ExtraBlip.DestroyAllByType(Additional.ExtraBlip.Types.GPS);
        }
    }
}
