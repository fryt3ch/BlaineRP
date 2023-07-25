using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Game.Wrappers.Colshapes;
using BlaineRP.Client.Game.Wrappers.Colshapes.Enums;
using BlaineRP.Client.Game.Wrappers.Colshapes.Types;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Wrappers.Blips
{
    public class ExtraBlip
    {
        public enum Types
        {
            Default = 0,
            [Language.Localized("BLIPS_DEF_GPS_NAME_0", "NAME_0")]
            GPS,
            [Language.Localized("BLIPS_DEF_FURNITURE_NAME_0", "NAME_0")]
            Furniture,
            [Language.Localized("BLIPS_DEF_AUTOPILOT_NAME_0", "NAME_0")]
            AutoPilot,
        }

        private static Dictionary<Blip, ExtraBlip> All { get; set; } = new Dictionary<Blip, ExtraBlip>();

        /// <summary>Получить блип по айди (локальный)</summary>
        public static ExtraBlip GetById(int id) => All.Where(x => x.Key?.Id == id).Select(x => x.Value).FirstOrDefault();

        /// <summary>Получить блип по айди (серверный)</summary>
        public static ExtraBlip GetByRemoteId(int id) => All.Where(x => x.Key?.RemoteId == id).Select(x => x.Value).FirstOrDefault();

        /// <summary>Получить  блип по его держателю</summary>
        public static ExtraBlip Get(Blip blip) => All.GetValueOrDefault(blip);

        private string _Name { get; set; }

        private byte _Colour { get; set; }

        private byte _Display { get; set; }

        private int _FlashInterval { get; set; }

        private bool _IsShortRange { get; set; }

        public Blip Blip { get; set; }

        public Vector3 Position { get; set; }

        public ExtraColshape Colshape { get; set; }

        public uint Dimension { get => Blip.Dimension; set => Blip.Dimension = value; }

        public uint Sprite { get => Blip.Model; set => Blip.Model = value; }

        public string Name { get => _Name; set => SetName(value); }

        public byte Colour { get => _Colour; set => SetColour(value); }

        public byte Display { get => _Display; set => SetDisplay(value); }

        public int FlashInterval { get => _FlashInterval; set => SetFlashInterval(value); }

        public bool IsShortRange { get => _IsShortRange; set => SetAsShortRange(value); }

        public bool Exists => All.ContainsKey(Blip);

        public Types Type { get; set; }

        public ExtraBlip(uint Sprite, Vector3 Position, string Name = "", float Scale = 1f, byte Colour = 0, int Alpha = 255, float DrawDistance = 0f, bool ShortRange = false, int Rotation = 0, float Radius = 0f, uint Dimension = uint.MaxValue, Types Type = Types.Default)
        {
            if (Type != Types.Default)
            {
                if (Name == null || Name.Length == 0)
                    Name = Locale.Get(Language.Strings.GetKeyFromTypeByMemberName(Type.GetType(), Type.ToString(), "NAME_0") ?? "null");

                DestroyAllByType(Type);
            }

            this.Blip = new Blip(Sprite, Position, Name, Scale, Colour, Alpha, DrawDistance, ShortRange, Rotation, Radius, Dimension);

            this._Name = Name;
            this._Colour = Colour;
            this._Display = 2;
            this._IsShortRange = ShortRange;

            this.Position = new Vector3(Position.X, Position.Y, Position.Z);
            this.Type = Type;

            All.Add(this.Blip, this);
        }

        public bool SetAsReachable(float range = 2.5f)
        {
            if (Colshape != null)
            {
                if (Colshape.ActionType == ActionTypes.ReachableBlip)
                    Colshape.Destroy();
                else
                    return false;
            }

            Colshape = new Circle(Position, range, false, Utils.Misc.RedColor, Dimension, null)
            {
                ApproveType = ApproveTypes.None,

                ActionType = ActionTypes.ReachableBlip,

                Data = this,
            };

            return true;
        }

        public void SetAsNotReachable()
        {
            if (Colshape?.ActionType == ActionTypes.ReachableBlip)
            {
                Colshape.Destroy();

                Colshape = null;
            }
        }

        public void SetRoute(bool state)
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

        private void SetAsShortRange(bool state)
        {
            _IsShortRange = state;

            Blip.SetAsShortRange(state);
        }

        private void SetColour(byte colour)
        {
            _Colour = colour;

            Blip.SetColour(colour);
        }

        private void SetFlashInterval(int interval)
        {
            if (interval < 0)
                interval = 0;

            _FlashInterval = interval;

            if (interval == 0)
            {
                Blip.SetFlashes(false);
            }
            else
            {
                Blip.SetFlashes(true);

                Blip.SetFlashInterval(interval);
            }
        }

        private void SetDisplay(byte state)
        {
            _Display = state;

            Blip.SetDisplay(state);
        }

        private void SetName(string name)
        {
            if (name == null)
                name = string.Empty;

            if (name == _Name)
                return;

            _Name = name;

            Blip.SetName(name);
        }

        public void SetCoords(float x, float y, float z)
        {
            this.Position = new Vector3(x, y, z);

            Blip.SetCoords(x, y, z);
        }

        public void SetData<T>(string key, T value) => Blip.SetData(key, value);

        public T GetData<T>(string key) => Blip.GetData<T>(key);

        public static void DestroyAllByType(Types type) => All.Where(x => x.Value?.Type == type).ToList().ForEach(x => x.Value.Destroy());

        public static void RefreshAllBlips()
        {
            foreach (var x in All.Values.ToList())
            {
                //x.SetName(x.Name);

                x.SetColour(x.Colour);

                x.SetDisplay(x.Display);

                x.SetFlashInterval(x.FlashInterval);

                x.SetAsShortRange(x.IsShortRange);
            }
        }
    }
}