using Newtonsoft.Json;

namespace BlaineRP.Server.Game.EntitiesData.Players.Customization
{
    public class HeadOverlay
    {
        [JsonProperty(PropertyName = "I")]
        public byte Index { get; set; }

        [JsonProperty(PropertyName = "C")]
        public byte Color { get; set; }

        [JsonProperty(PropertyName = "C2")]
        public byte SecondaryColor { get; set; }

        [JsonProperty(PropertyName = "O")]
        public float Opacity { get; set; }

        [JsonIgnore]
        public GTANetworkAPI.HeadOverlay RageHeadOverlay => new GTANetworkAPI.HeadOverlay() { Index = Index, Color = Color, SecondaryColor = SecondaryColor, Opacity = Opacity };

        public HeadOverlay(GTANetworkAPI.HeadOverlay hOverlay)
        {
            Index = hOverlay.Index;
            Color = hOverlay.Color;
            SecondaryColor = hOverlay.SecondaryColor;
            Opacity = hOverlay.Opacity;
        }

        public HeadOverlay()
        {

        }
    }
}