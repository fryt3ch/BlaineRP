using Newtonsoft.Json;

namespace BlaineRP.Server.Game.EntitiesData.Players.Customization
{
    public class HeadBlend
    {
        [JsonProperty(PropertyName = "SF")]
        public byte ShapeFirst { get; set; }

        [JsonProperty(PropertyName = "SS")]
        public byte ShapeSecond { get; set; }

        [JsonProperty(PropertyName = "ST")]
        public byte ShapeThird { get; set; }

        [JsonProperty(PropertyName = "SNF")]
        public byte SkinFirst { get; set; }

        [JsonProperty(PropertyName = "SNS")]
        public byte SkinSecond { get; set; }

        [JsonProperty(PropertyName = "SNT")]
        public byte SkinThird { get; set; }

        [JsonProperty(PropertyName = "SM")]
        public float ShapeMix { get; set; }

        [JsonProperty(PropertyName = "SNM")]
        public float SkinMix { get; set; }

        [JsonProperty(PropertyName = "TM")]
        public float ThirdMix { get; set; }

        [JsonIgnore]
        public GTANetworkAPI.HeadBlend RageHeadBlend => new GTANetworkAPI.HeadBlend() { ShapeFirst = ShapeFirst, ShapeSecond = ShapeSecond, ShapeThird = ShapeThird, SkinFirst = SkinFirst, SkinSecond = SkinSecond, SkinThird = SkinThird, ShapeMix = ShapeMix, SkinMix = SkinMix, ThirdMix = ThirdMix };

        public HeadBlend(GTANetworkAPI.HeadBlend hBlend)
        {
            ShapeFirst = hBlend.ShapeFirst;
            ShapeSecond = hBlend.ShapeSecond;
            ShapeThird = hBlend.ShapeThird;

            SkinFirst = hBlend.SkinFirst;
            SkinSecond = hBlend.SkinSecond;
            SkinThird = hBlend.SkinThird;

            ShapeMix = hBlend.ShapeMix;
            SkinMix = hBlend.SkinMix;
            ThirdMix = hBlend.ThirdMix;
        }

        public HeadBlend()
        {

        }
    }
}