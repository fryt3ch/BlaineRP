using GTANetworkAPI;
using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Estates
{
    public abstract partial class HouseBase
    {
        public partial class Style
        {
            public class DoorInfo
            {
                [JsonProperty(PropertyName = "M")]
                public uint Model { get; set; }

                [JsonProperty(PropertyName = "P")]
                public Vector3 Position { get; set; }

                public DoorInfo(string Model, Vector3 Position) : this(NAPI.Util.GetHashKey(Model), Position)
                {

                }

                public DoorInfo(uint Model, Vector3 Position)
                {
                    this.Model = Model;
                    this.Position = Position;
                }
            }
        }
    }
}