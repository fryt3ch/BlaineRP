using Newtonsoft.Json;

namespace BlaineRP.Server.Game.EntitiesData.Players.Customization
{
    public class HairStyle
    {
        [JsonProperty(PropertyName = "I")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "O")]
        public byte Overlay { get; set; }

        [JsonProperty(PropertyName = "C")]
        public byte Color { get; set; }

        [JsonProperty(PropertyName = "C2")]
        public byte Color2 { get; set; }

        public HairStyle(int Id, byte Overlay, byte Color, byte Color2)
        {
            this.Id = Id;
            this.Overlay = Overlay;
            this.Color = Color;
            this.Color2 = Color2;
        }
    }
}