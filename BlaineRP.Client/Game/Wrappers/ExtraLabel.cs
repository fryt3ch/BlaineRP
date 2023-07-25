using System.Collections.Generic;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Wrappers
{
    public class ExtraLabel
    {
        private static List<ExtraLabel> All { get; set; } = new List<ExtraLabel>();
        private static List<ExtraLabel> Streamed { get; set; } = new List<ExtraLabel>();

        private static readonly Utils.UidHandlers.UInt16 _uidHandler = new Utils.UidHandlers.UInt16(0);

        private Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        public Vector3 Position { get => Label.Position; set => Label.Position = value; }

        public bool LOS { get => Label.LOS; set => Label.LOS = value; }

        public int Font { get => Label.Font; set => Label.Font = value; }

        public bool Center { get; set; }

        public RGBA Color { get => Label.Color; set => Label.Color = value; }

        public uint Dimension { get => Label.Dimension; set => Label.Dimension = value; }

        public string Text { get => Label.Text; set => Label.Text = value; }

        public float DrawDistance { get => Label.DrawDistance; set => Label.DrawDistance = value; }

        public ushort Id { get; private set; }

        public bool Exists => All.Contains(this);

        private TextLabel Label { get; set; }

        public ExtraLabel(Vector3 Position, string Text, RGBA Colour, float DrawDistance, int Rotation, bool ShortRange, uint Dimension)
        {
            Label = new TextLabel(Position, Text, Colour, DrawDistance, Rotation, ShortRange, Dimension);

            return;

            Id = _uidHandler.MoveNextUid();

            All.Add(this);

            this.Text = Text;

            this.Color = new RGBA(Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);

            this.DrawDistance = DrawDistance;
            this.Dimension = Dimension;

            this.Position = Position;
        }

        public static void Initialize()
        {
            return;

            (new Utils.AsyncTask(() =>
            {
                var pos = Player.LocalPlayer.Position;
                var dim = Player.LocalPlayer.Dimension;

                foreach (var x in All)
                {
                    if ((x.Dimension == uint.MaxValue || x.Dimension == dim) && x.Position.DistanceTo(pos) <= Settings.App.Profile.Current.Game.StreamDistance)
                    {
                        if (!Streamed.Contains(x))
                            Streamed.Add(x);
                    }
                    else
                    {
                        Streamed.Remove(x);
                    }
                }
            }, 1000, true, 1000)).Run();

            Main.Render -= Render;
            Main.Render += Render;
        }

        public void SetData<T>(string key, T value)
        {
            if (!Data.TryAdd(key, value))
                Data[key] = value;
        }

        public T GetData<T>(string key)
        {
            if (Data.GetValueOrDefault(key) is T objT)
                return objT;

            return default(T);
        }

        public bool ResetData(string key) => Data.Remove(key);

        public void Destroy()
        {
            if (Label == null)
                return;

            Label.Destroy();

            Label = null;

            return;

            if (!All.Remove(this))
                return;

            Streamed.Remove(this);

            _uidHandler.SetUidAsFree(Id);

            Data.Clear();

            Data = null;
        }

        private static void Render()
        {
            float posX = 0f, posY = 0f;

            var pos = Player.LocalPlayer.Position;

            Streamed.ForEach(x =>
            {
                if (x.Position.DistanceTo(pos) <= x.DrawDistance)
                {
                    if (!Graphics.GetScreenCoordFromWorldCoord(x.Position, ref posX, ref posY))
                        return;

                    Graphics.DrawText(x.Text, posX, posY, (byte)x.Color.Red, (byte)x.Color.Green, (byte)x.Color.Blue, (byte)x.Color.Alpha, 0.4f, (RAGE.Game.Font)x.Font, true, true);
                }
            });
        }
    }
}
