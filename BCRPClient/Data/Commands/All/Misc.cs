﻿using RAGE;
using RAGE.Elements;
using System.Linq;

namespace BCRPClient.Data
{
    partial class Commands
    {
        [Command("dl", false, "Показ дополнительных сведений о всех сущностях")]
        public static void DebugLabels(bool? state = null)
        {
            if (state == null)
                Settings.Other.DebugLabels = !Settings.Other.DebugLabels;
            else
                Settings.Other.DebugLabels = (bool)state;

            CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Notifications.Commands.Header, string.Format(Settings.Other.DebugLabels ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "DL"));
        }

        [Command("raytrace", false, "Показ дополнительных сведений о всех сущностях")]
        public static void Raytrace(bool? state = null)
        {
            if (state == null)
                Settings.Other.RaytraceEnabled = !Settings.Other.RaytraceEnabled;
            else
                Settings.Other.RaytraceEnabled = (bool)state;

            CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Notifications.Commands.Header, string.Format(Settings.Other.RaytraceEnabled ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "Raytace"));
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
    }
}
