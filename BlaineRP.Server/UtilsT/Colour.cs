using GTANetworkAPI;
using Newtonsoft.Json;

namespace BlaineRP.Server.UtilsT
{
    public class Colour
    {
        [JsonIgnore]
        public static Colour DefBlack => new Colour(0, 0, 0, 255);

        [JsonIgnore]
        public static Colour DefWhite => new Colour(255, 255, 255, 255);

        [JsonIgnore]
        public static Colour DefRed => new Colour(255, 0, 0, 255);

        [JsonIgnore]
        public static Colour DefGreen => new Colour(0, 255, 0, 255);

        [JsonIgnore]
        public static Colour DefBlue => new Colour(0, 0, 255, 255);

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
            this.Red = byte.Parse(HEX.Substring(1, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            this.Green = byte.Parse(HEX.Substring(3, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            this.Blue = byte.Parse(HEX.Substring(5, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

            if (HEX.Length == 6)
                this.Alpha = byte.Parse(HEX.Substring(7, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            else
                this.Alpha = 255;
        }

        public Color ToRageColour() => new Color(Red, Green, Blue, Alpha);

        public static Colour FromRageColour(Color colour) => new Colour((byte)colour.Red, (byte)colour.Green, (byte)colour.Blue, (byte)colour.Alpha);
    }
}