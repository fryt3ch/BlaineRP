using RAGE;
using RAGE.Elements;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Additional
{
    public class ExtraBlips : Events.Script
    {
        public ExtraBlips()
        {
            // default reachable gps
            Events.Add("Blip::CreateGPS", (args) =>
            {
                var pos = (Vector3)args[0];
                var dim = args[1].ToUInt32();
                var drawRoute = (bool)args[2];

                CreateGPS(pos, dim, drawRoute);
            });

            Events.Add("Blip::Tracker", (args) =>
            {
                var type = (int)args[0];

                var x = (float)args[1]; var y = (float)args[2];

                var entity = (Entity)args[3] as GameEntity;

                Blip blip = null;

                string key = null;

                if (type == 0) // taxi
                {
                    blip = new Blip(198, new Vector3(x, y, 0), "Водитель такси", 1f, 5, 255, 0f, false, 0, 0f, uint.MaxValue);

                    key = "Taxi";
                }

                if (key == null || blip == null)
                    return;

                Player.LocalPlayer.GetData<Blip>($"TrackerBlip::{key}")?.Destroy();
                Player.LocalPlayer.GetData<AsyncTask>($"TrackerBlip::Task::{key}")?.Cancel();

                var task = entity?.Exists == true ? new AsyncTask(() => { if (!entity.Exists) return; var coords = RAGE.Game.Entity.GetEntityCoords(entity.Handle, false); blip.SetCoords(coords.X, coords.Y, coords.Z); }, 250, true, 0) : new AsyncTask(() => { Player.LocalPlayer.GetData<Blip>($"TrackerBlip::{key}")?.Destroy(); Player.LocalPlayer.GetData<AsyncTask>($"TrackerBlip::Task::{key}")?.Cancel(); Player.LocalPlayer.ResetData($"TrackerBlip::{key}"); Player.LocalPlayer.ResetData($"TrackerBlip::Task::{key}"); }, 5000, false, 0);

                Player.LocalPlayer.SetData($"TrackerBlip::Task::{key}", task);
                Player.LocalPlayer.SetData($"TrackerBlip::{key}", blip);

                task.Run();
            });
        }

        public static void DestroyTrackerBlipByKey(string key)
        {
            Player.LocalPlayer.GetData<Blip>($"TrackerBlip::{key}")?.Destroy();
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
                blip.ToggleRouting(true);
            }
            else
            {

            }

            CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Notifications.Blip.Header, (Locale.Notifications.Blip.TypesText.GetValueOrDefault(ExtraBlip.Types.GPS) ?? "null") + (notificationAddText == null ? string.Empty : notificationAddText));

            return blip;
        }
    }

    public class ExtraBlip
    {
        public enum Types
        {
            Default = 0,
            GPS,
            Furniture,
            AutoPilot,
        }

        private static Dictionary<Blip, ExtraBlip> All { get; set; } = new Dictionary<Blip, ExtraBlip>();

        /// <summary>Получить блип по айди (локальный)</summary>
        public static ExtraBlip GetById(int id) => All.Where(x => x.Key?.Id == id).Select(x => x.Value).FirstOrDefault();

        /// <summary>Получить блип по айди (серверный)</summary>
        public static ExtraBlip GetByRemoteId(int id) => All.Where(x => x.Key?.RemoteId == id).Select(x => x.Value).FirstOrDefault();

        /// <summary>Получить  блип по его держателю</summary>
        public static ExtraBlip Get(Blip blip) => All.GetValueOrDefault(blip);

        public Blip Blip { get; set; }

        public Vector3 Position { get; set; }

        public Additional.ExtraColshape Colshape { get; set; }

        public uint Dimension { get => Blip.Dimension; set => Blip.Dimension = value; }

        public bool Exists => All.ContainsKey(Blip);

        public Types Type { get; set; }

        public ExtraBlip(uint Sprite, Vector3 Position, string Name = "", float Scale = 1f, int Colour = 0, int Alpha = 255, float DrawDistance = 0f, bool ShortRange = false, int Rotation = 0, float Radius = 0f, uint Dimension = uint.MaxValue, Types Type = Types.Default)
        {
            if (Type != Types.Default)
            {
                if (Name == null || Name.Length == 0)
                    Name = Locale.General.Blip.TypesNames.GetValueOrDefault(Type) ?? "null";

                DestroyAllByType(Type);
            }

            this.Blip = new Blip(Sprite, Position, Name, Scale, Colour, Alpha, DrawDistance, ShortRange, Rotation, Radius, Dimension);

            this.Position = Position;
            this.Type = Type;

            All.Add(this.Blip, this);
        }

        public bool SetAsReachable(float range = 2.5f)
        {
            if (Colshape != null)
            {
                if (Colshape.ActionType == ExtraColshape.ActionTypes.ReachableBlip)
                    Colshape.Destroy();
                else
                    return false;
            }

            Colshape = new Additional.Circle(Position, range, false, Utils.RedColor, Dimension, null);

            Colshape.ActionType = ExtraColshape.ActionTypes.ReachableBlip;
            Colshape.Data = this;

            return true;
        }

        public void SetAsNotReachable()
        {
            if (Colshape?.ActionType == ExtraColshape.ActionTypes.ReachableBlip)
            {
                Colshape.Destroy();

                Colshape = null;
            }
        }

        public void ToggleRouting(bool state)
        {
            this.Blip.SetRoute(state);
        }

        public void Destroy()
        {
            if (Blip != null)
            {
                if (All.Remove(Blip))
                {
                    Blip.Destroy();
                }
            }

            Colshape?.Destroy();
        }

        public static void DestroyAllByType(Types type) => All.Where(x => x.Value?.Type == type).ToList().ForEach(x => x.Value.Destroy());
    }
}
