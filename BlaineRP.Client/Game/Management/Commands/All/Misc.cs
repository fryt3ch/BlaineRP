using System.Linq;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Vehicles;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Scripts.Sync;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management.Commands
{
    partial class Service
    {
        [Command("ping", false, "")]
        public static async void Ping()
        {
            if (LastSent.IsSpam(500, false, true))
                return;

            LastSent = World.Core.ServerTime;

            var res = Convert.ToDecimal(await Events.CallRemoteProc("Misc::GetPing"));

            Events.CallLocal("Chat::ShowServerMessage", $"[Ping] Ваш текущий пинг: {res}");
        }

        [Command("dl", false, "Показ дополнительных сведений о всех сущностях")]
        public static void DebugLabels(bool? state = null)
        {
            if (state == null)
                Settings.User.Other.DebugLabels = !Settings.User.Other.DebugLabels;
            else
                Settings.User.Other.DebugLabels = (bool)state;

            Notification.Show(Notification.Types.Success,
                Locale.Notifications.Commands.Header,
                string.Format(Settings.User.Other.DebugLabels ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "DL")
            );
        }

        [Command("raytrace", false, "Показ дополнительных сведений о всех сущностях")]
        public static void Raytrace(bool? state = null)
        {
            if (state == null)
                Settings.User.Other.RaytraceEnabled = !Settings.User.Other.RaytraceEnabled;
            else
                Settings.User.Other.RaytraceEnabled = (bool)state;

            Notification.Show(Notification.Types.Success,
                Locale.Notifications.Commands.Header,
                string.Format(Settings.User.Other.RaytraceEnabled ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "Raytace")
            );
        }

        [Command("lock", false, "Блокировка транспорта")]
        public static void Lock(uint? id = null, bool? state = null)
        {
            if (id == null)
            {
                Vehicles.Lock(state, null);
            }
            else
            {
                Vehicle veh = id > ushort.MaxValue
                    ? Entities.Vehicles.Streamed.Where(x => VehicleData.GetData(x)?.VID == id).FirstOrDefault()
                    : Entities.Vehicles.Streamed.Where(x => x.RemoteId == id).FirstOrDefault();

                if (veh == null)
                {
                    Notification.Show(Notification.Types.Error, Locale.Notifications.Interaction.Header, Locale.Notifications.Interaction.NotFound);

                    return;
                }

                if (Vector3.Distance(Player.LocalPlayer.Position, veh.Position) > 10f)
                {
                    Notification.Show(Notification.Types.Error, Locale.Notifications.Interaction.Header, Locale.Notifications.Interaction.DistanceTooLarge);

                    return;
                }

                Vehicles.Lock(state, veh);
            }
        }

        [Command("report", false, "Связь с администрацией", "rep", "ask")]
        public static void Report(string text)
        {
            Menu.ReportSend(text);
        }

        [Command("cleargps", false, "Связь с администрацией", "cgps", "gpsclear", "gpsc")]
        public static void ClearGPS()
        {
            ExtraBlip.DestroyAllByType(ExtraBlip.Types.GPS);
        }
    }
}