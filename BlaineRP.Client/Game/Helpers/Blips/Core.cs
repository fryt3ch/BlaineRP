using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Helpers.Blips
{
    [Script(int.MaxValue)]
    public class Core
    {
        public Core()
        {
            // default reachable gps
            Events.Add("Blip::CreateGPS", (args) =>
            {
                var pos = (Vector3)args[0];
                var dim = Utils.Convert.ToUInt32(args[1]);
                var drawRoute = (bool)args[2];

                CreateGPS(pos, dim, drawRoute);
            });

            Events.Add("Blip::Tracker", (args) =>
            {
                var type = (int)args[0];

                var x = (float)args[1]; var y = (float)args[2];

                var entity = (Entity)args[3] as GameEntity;

                ExtraBlip blip = null;

                string key = null;

                if (type == 0) // taxi
                {
                    blip = new ExtraBlip(198, new Vector3(x, y, 0), "Водитель такси", 1f, 5, 255, 0f, false, 0, 0f, uint.MaxValue);

                    key = "Taxi";
                }
                else if (type == 1) // police call
                {
                    blip = new ExtraBlip(198, new Vector3(x, y, 0), "Сотрудник полиции", 1f, 29, 255, 0f, false, 0, 0f, uint.MaxValue);

                    key = "PoliceCall";
                }

                if (key == null || blip == null)
                    return;

                Player.LocalPlayer.GetData<ExtraBlip>($"TrackerBlip::{key}")?.Destroy();
                Player.LocalPlayer.GetData<AsyncTask>($"TrackerBlip::Task::{key}")?.Cancel();

                var task = entity?.Exists == true ? new AsyncTask(() => { if (!entity.Exists) return; var coords = RAGE.Game.Entity.GetEntityCoords(entity.Handle, false); blip.SetCoords(coords.X, coords.Y, coords.Z); }, 250, true, 0) : new AsyncTask(() => { Player.LocalPlayer.GetData<ExtraBlip>($"TrackerBlip::{key}")?.Destroy(); Player.LocalPlayer.GetData<AsyncTask>($"TrackerBlip::Task::{key}")?.Cancel(); Player.LocalPlayer.ResetData($"TrackerBlip::{key}"); Player.LocalPlayer.ResetData($"TrackerBlip::Task::{key}"); }, 5000, false, 0);

                Player.LocalPlayer.SetData($"TrackerBlip::Task::{key}", task);
                Player.LocalPlayer.SetData($"TrackerBlip::{key}", blip);

                task.Run();
            });
        }

        public static void DestroyTrackerBlipByKey(string key)
        {
            Player.LocalPlayer.GetData<ExtraBlip>($"TrackerBlip::{key}")?.Destroy();
            Player.LocalPlayer.GetData<AsyncTask>($"TrackerBlip::Task::{key}")?.Cancel();

            Player.LocalPlayer.ResetData($"TrackerBlip::{key}");
            Player.LocalPlayer.ResetData($"TrackerBlip::Task::{key}");
        }

        public static ExtraBlip CreateGPS(Vector3 pos, uint dim, bool drawRoute, string notificationAddText = null)
        {
            var blip = new ExtraBlip(162, pos, "", 1f, 3, 255, 0f, false, 0, 0f, dim, ExtraBlip.Types.GPS);

            blip.SetAsReachable();

            if (drawRoute)
            {
                blip.SetRoute(true);
            }
            else
            {

            }

            Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_GPS"), Locale.Get("BLIP_GEN_GPS_ADDED_0") + (notificationAddText == null ? string.Empty : notificationAddText));

            return blip;
        }
    }
}
