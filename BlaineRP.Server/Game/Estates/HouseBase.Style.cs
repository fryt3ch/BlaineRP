using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Estates
{
    public abstract partial class HouseBase
    {
        public partial class Style
        {
            private HashSet<RoomTypes> SupportedRoomTypes { get; }
            private HashSet<HouseBase.Types> SupportedHouseTypes { get; }

            private HashSet<ushort> FamiliarTypes { get; }

            public Vector4 InteriorPosition { get; }

            public Vector3 Position { get; }

            public float Heading { get; }

            private DoorInfo[] Doors { get; }

            private LightInfo[][] Lights { get; }

            public int LightsAmount => Lights.Length;

            public int DoorsAmount => Doors.Length;

            public uint Price { get; }

            public ushort ParentType { get; }

            /// <summary>Словарь планировок</summary>
            private static Dictionary<ushort, Style> All { get; set; }

            public static Style Get(ushort sType) => All.GetValueOrDefault(sType);

            public bool IsPositionInsideInterior(Vector3 position) => InteriorPosition.Position.DistanceTo(position) <= InteriorPosition.RotationZ;

            public Style(ushort Type, Vector3 Position, float Heading, Vector4 InteriorPosition, DoorInfo[] Doors, LightInfo[][] Lights, uint Price, HashSet<RoomTypes> SupportedRoomTypes, HashSet<HouseBase.Types> SupportedHouseTypes, Vector3 Offset = null)
            {
                All.Add(Type, this);

                this.FamiliarTypes = new HashSet<ushort>();

                this.SupportedHouseTypes = SupportedHouseTypes;
                this.SupportedRoomTypes = SupportedRoomTypes;

                this.Position = Position;
                this.Heading = Heading;
                this.InteriorPosition = InteriorPosition;
                this.Price = Price;

                this.Doors = Doors;
                this.Lights = Lights;

                this.ParentType = Type;

                for (int i = 0; i < Doors.Length; i++)
                {
                    Doors[i].Position += InteriorPosition.Position;
                }

                for (int i = 0; i < Lights.Length; i++)
                {
                    for (int j = 0; j < Lights[i].Length; j++)
                        Lights[i][j].Position += InteriorPosition.Position;
                }

                if (Offset != null)
                {
                    for (int i = 0; i < Doors.Length; i++)
                    {
                        Doors[i].Position += Offset;
                    }

                    for (int i = 0; i < Lights.Length; i++)
                    {
                        for (int j = 0; j < Lights[i].Length; j++)
                            Lights[i][j].Position += Offset;
                    }
                }
            }

            public Style(ushort Type, ushort ParentType, Vector3 Offset, uint Price)
            {
                All.Add(Type, this);

                this.ParentType = ParentType;

                var parent = Get(ParentType);

                this.FamiliarTypes = new HashSet<ushort>() { ParentType };

                foreach (var x in parent.FamiliarTypes)
                {
                    var s = Get(x);

                    s.FamiliarTypes.Add(Type);

                    this.FamiliarTypes.Add(x);
                }

                parent.FamiliarTypes.Add(Type);

                this.Heading = parent.Heading;
                this.Position = parent.Position + Offset;
                this.InteriorPosition = new Vector4(parent.InteriorPosition.Position + Offset, parent.InteriorPosition.RotationZ);

                this.Doors = new DoorInfo[parent.Doors.Length];

                for (int i = 0; i < parent.Doors.Length; i++)
                {
                    var x = parent.Doors[i];

                    this.Doors[i] = new DoorInfo(x.Model, x.Position - parent.InteriorPosition.Position + this.InteriorPosition.Position);
                }

                this.Lights = new LightInfo[parent.Lights.Length][];

                for (int i = 0; i < parent.Lights.Length; i++)
                {
                    var x = parent.Lights[i];

                    this.Lights[i] = new LightInfo[x.Length];

                    for (int j = 0; j < x.Length; j++)
                    {
                        var y = x[j];

                        this.Lights[i][j] = new LightInfo(y.Model, y.Position - parent.InteriorPosition.Position + this.InteriorPosition.Position);
                    }
                }

                this.SupportedRoomTypes = parent.SupportedRoomTypes.ToHashSet();
                this.SupportedHouseTypes = parent.SupportedHouseTypes.ToHashSet();

                this.Price = Price;
            }

            public bool IsHouseTypeSupported(HouseBase.Types hType) => SupportedHouseTypes.Contains(hType);
            public bool IsRoomTypeSupported(RoomTypes rType) => SupportedRoomTypes.Contains(rType);
            public bool IsTypeFamiliar(ushort type) => FamiliarTypes.Contains(type);
        }
    }
}