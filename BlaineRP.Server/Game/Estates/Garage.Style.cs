using System.Collections.Generic;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Estates
{
    public partial class Garage
    {
        public partial class Style
        {
            public static Dictionary<Types, Dictionary<byte, Style>> All { get; set; } = new Dictionary<Types, Dictionary<byte, Style>>();

            public Types Type { get; set; }

            public byte Variation { get; set; }

            public List<Vector4> VehiclePositions { get; set; }

            public Vector4 EnterPosition { get; set; }

            public int MaxVehicles => VehiclePositions.Count;

            public Style(Types Type, byte Variation, Vector4 EnterPosition, List<Vector4> VehiclePositions)
            {
                this.Type = Type;

                this.Variation = Variation;

                this.EnterPosition = EnterPosition;

                this.VehiclePositions = VehiclePositions;

                if (!All.ContainsKey(Type))
                    All.Add(Type, new Dictionary<byte, Style>() { { Variation, this } });
                else
                    All[Type].Add(Variation, this);
            }

            public static Style Get(Types type, byte variation) => All.GetValueOrDefault(type).GetValueOrDefault(variation);
        }
    }
}