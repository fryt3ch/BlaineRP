using BlaineRP.Server.UtilsT;
using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Estates
{
    public abstract partial class HouseBase
    {
        public static Colour DefaultLightColour => new Colour(255, 187, 96, 255);
        
        public class Light
        {
            [JsonProperty(PropertyName = "S")]
            public bool State { get; set; }

            [JsonProperty(PropertyName = "C")]
            public Colour Colour { get; set; }

            public Light(bool State, Colour Colour)
            {
                this.State = State;
                this.Colour = Colour;
            }

            public Light() { }
        }
    }
}