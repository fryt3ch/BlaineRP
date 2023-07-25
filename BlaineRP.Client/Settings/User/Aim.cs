using BlaineRP.Client.Game.Management;

namespace BlaineRP.Client.Settings.User
{
    public static class Aim
    {
        public enum Types
        {
            Default = 1, Dot, Cross
        }

        public static class Default
        {
            public static Types Type = Types.Default;
            public static Utils.Colour Color = new Utils.Colour(255, 255, 255);
            public static float Alpha = 1f;
            public static float Scale = 0.5f;
        }

        public static Types _Type;
        public static Utils.Colour _Color;
        public static float _Alpha;
        public static float _Scale;

        public static Types Type { get => _Type; set { if (value != _Type) RageStorage.SetData("Settings::Interface::Aim::Type", value); _Type = value; } }
        public static Utils.Colour Color { get => _Color; set { if (value != _Color) RageStorage.SetData("Settings::Interface::Aim::Color", value); _Color = value; } }
        public static float Alpha { get => _Alpha; set { if (value != _Alpha) RageStorage.SetData("Settings::Interface::Aim::Alpha", value); _Alpha = value; } }
        public static float Scale { get => _Scale; set { if (value != _Scale) RageStorage.SetData("Settings::Interface::Aim::Scale", value); _Scale = value; } }
    }
}
