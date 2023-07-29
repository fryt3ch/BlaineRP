using System.Collections.Generic;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.EntitiesData.Players.Customization
{
    public static class Defaults
    {
        public static HairStyle HairStyle { get; private set; } = new HairStyle(0, 0, 0, 0);

        public static GTANetworkAPI.HeadBlend HeadBlend { get; private set; } = new GTANetworkAPI.HeadBlend { ShapeFirst = 21, ShapeSecond = 0, SkinFirst = 21, SkinSecond = 0, ShapeMix = 0.5f, SkinMix = 0.5f, ShapeThird = 0, SkinThird = 0, ThirdMix = 0f };

        public static float[] FaceFeatures { get; private set; } = new float[20] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public static Dictionary<int, GTANetworkAPI.HeadOverlay> HeadOverlays { get; private set; } = new Dictionary<int, GTANetworkAPI.HeadOverlay>()
        {
            { 0, new GTANetworkAPI.HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 1f } }, // Blemishes (0-23)
            { 1, new GTANetworkAPI.HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 1f } }, // Facial Hair (0-28)
            { 2, new GTANetworkAPI.HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 1f } }, // Eye Brows
            { 3, new GTANetworkAPI.HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 1f } }, // Ageing (0-33)
            { 4, new GTANetworkAPI.HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 0.5f } }, // Makeup (0-14)
            { 5, new GTANetworkAPI.HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 0.5f } }, // Blush (0-32)
            { 6, new GTANetworkAPI.HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 1f } }, // Complexion (0-11)
            { 7, new GTANetworkAPI.HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 1f } }, // Sun Damage (0-10)
            { 8, new GTANetworkAPI.HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 0.5f } }, // Lipstick (0-9)
            { 9, new GTANetworkAPI.HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 1f } }, // Moles/Freckles (0-17)
            { 10, new GTANetworkAPI.HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 1f } }, // Chest Hair (0-16)
            { 11, new GTANetworkAPI.HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 1f } }, // Body Blemishes (0-11)
            { 12, new GTANetworkAPI.HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 0f } }, // Add Body Blemishes (0-1)
        };

        public static byte EyeColor { get; private set; } = 0;

        public static Decoration[] Decorations { get; private set; } = new Decoration[] { };
    }
}