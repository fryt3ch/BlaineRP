using GTANetworkAPI;
using Newtonsoft.Json;

namespace BlaineRP.Server.UtilsT
{
    public class Vector4
    {
        [JsonProperty(PropertyName = "P")]
        public Vector3 Position { get; set; }

        [JsonProperty(PropertyName = "RZ")]
        public float RotationZ { get; set; }

        [JsonIgnore]
        public float X => Position.X;

        [JsonIgnore]
        public float Y => Position.Y;

        [JsonIgnore]
        public float Z => Position.Z;

        public Vector4(float X, float Y, float Z, float RotationZ = 0f)
        {
            this.Position = new Vector3(X, Y, Z);

            this.RotationZ = RotationZ;
        }

        public Vector4(Vector3 Position, float RotationZ = 0f) : this(Position.X, Position.Y, Position.Z, RotationZ) { }

        public Vector4(Vector4 Position) : this(Position.X, Position.Y, Position.Z, Position.RotationZ) { }

        public Vector4() { }
    }
}