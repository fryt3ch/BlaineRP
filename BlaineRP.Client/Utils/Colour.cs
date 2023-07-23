using Newtonsoft.Json;

namespace BlaineRP.Client.Utils
{
    public class Colour
    {
        [JsonIgnore]
        /// <summary>Красный</summary>
        public byte Red { get; set; }

        [JsonIgnore]
        /// <summary>Зеленый</summary>
        public byte Green { get; set; }

        [JsonIgnore]
        /// <summary>Синий</summary>
        public byte Blue { get; set; }

        [JsonIgnore]
        /// <summary>Непрозрачность</summary>
        public byte Alpha { get; set; }

        [JsonProperty(PropertyName = "H")]
        public string HEX => $"#{Red:X2}{Green:X2}{Blue:X2}{Alpha:X2}";

        [JsonIgnore]
        public string HEXNoAlpha => $"#{Red:X2}{Green:X2}{Blue:X2}";

        public Colour(byte Red, byte Green, byte Blue, byte Alpha = 255)
        {
            this.Red = Red;
            this.Green = Green;
            this.Blue = Blue;

            this.Alpha = Alpha;
        }

        [JsonConstructor]
        public Colour(string HEX)
        {
            Red = byte.Parse(HEX.Substring(1, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            Green = byte.Parse(HEX.Substring(3, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            Blue = byte.Parse(HEX.Substring(5, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

            if (HEX.Length == 6)
                Alpha = byte.Parse(HEX.Substring(7, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            else
                Alpha = 255;
        }

        public System.Drawing.Color ToSystemColour() => System.Drawing.Color.FromArgb(Alpha, Red, Green, Blue);

        public static Colour FromSystemColour(System.Drawing.Color colour) => new Colour(colour.R, colour.G, colour.B, colour.A);
    }
}
