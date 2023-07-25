﻿using System.Linq;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Game.World;
using BlaineRP.Client.Game.Wrappers.Blips;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management.Commands
{
    partial class Core
    {
        [Command("ping", false, "")]
        public static async void Ping()
        {
            if (Commands.Core.LastSent.IsSpam(500, false, true))
                return;

            Commands.Core.LastSent = World.Core.ServerTime;

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

            Notification.Show(Notification.Types.Success, Locale.Notifications.Commands.Header, string.Format(Settings.User.Other.DebugLabels ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "DL"));
        }

        [Command("raytrace", false, "Показ дополнительных сведений о всех сущностях")]
        public static void Raytrace(bool? state = null)
        {
            if (state == null)
                Settings.User.Other.RaytraceEnabled = !Settings.User.Other.RaytraceEnabled;
            else
                Settings.User.Other.RaytraceEnabled = (bool)state;

            Notification.Show(Notification.Types.Success, Locale.Notifications.Commands.Header, string.Format(Settings.User.Other.RaytraceEnabled ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "Raytace"));
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
                var veh = id > ushort.MaxValue ? RAGE.Elements.Entities.Vehicles.Streamed.Where(x => VehicleData.GetData(x)?.VID == id).FirstOrDefault() : RAGE.Elements.Entities.Vehicles.Streamed.Where(x => x.RemoteId == id).FirstOrDefault();

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

                Sync.Vehicles.Lock(state, veh);
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
