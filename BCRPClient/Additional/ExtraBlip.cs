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
        }

        public static ExtraBlip CreateGPS(Vector3 pos, uint dim, bool drawRoute)
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

            CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Notifications.Blip.Header, Locale.Notifications.Blip.TypesText.GetValueOrDefault(ExtraBlip.Types.GPS) ?? "null");

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
                    Colshape.Delete();
                else
                    return false;
            }

            Colshape = new Additional.Sphere(Position, range, false, Utils.RedColor, Dimension, null);

            Colshape.ActionType = ExtraColshape.ActionTypes.ReachableBlip;
            Colshape.Data = this;

            return true;
        }

        public void SetAsNotReachable()
        {
            if (Colshape?.ActionType == ExtraColshape.ActionTypes.ReachableBlip)
            {
                Colshape.Delete();

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
                All.Remove(Blip);

                Blip.Destroy();
            }

            Colshape?.Delete();
        }

        public static void DestroyAllByType(Types type) => All.Where(x => x.Value?.Type == type).ToList().ForEach(x => x.Value.Destroy());
    }
}
