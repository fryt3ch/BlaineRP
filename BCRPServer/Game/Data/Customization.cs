using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Data
{
    public class Customization
    {
        public enum ClothesTypes
        {
            Top = 11,
            Under = 8,
            Pants = 4,
            Shoes = 6,
            Gloves = 3,
            Mask = 1,
            Accessory = 7,
            Bag = 5,
            Armour = 9,
        }

        public enum AccessoryTypes
        {
            Hat = 0,
            Glasses = 1,
            Earrings = 2,
            Watches = 6,
            Bracelet = 7,
        }

        private static Dictionary<bool, Dictionary<ClothesTypes, int>> NudeClothes = new Dictionary<bool, Dictionary<ClothesTypes, int>>()
        {
            {
                true,

                new Dictionary<ClothesTypes, int>()
                {
                    { ClothesTypes.Top, 15 },
                    { ClothesTypes.Under, 15 },
                    { ClothesTypes.Gloves, 15 },
                    { ClothesTypes.Pants, 21 },
                    { ClothesTypes.Shoes, 34 },
                    { ClothesTypes.Accessory, 0 },
                    { ClothesTypes.Mask, 0 },
                    { ClothesTypes.Bag, 0 },
                }
            },

            {
                false,

                new Dictionary<ClothesTypes, int>()
                {
                    { ClothesTypes.Top, 15 },
                    { ClothesTypes.Under, 15 },
                    { ClothesTypes.Gloves, 15 },
                    { ClothesTypes.Pants, 15 },
                    { ClothesTypes.Shoes, 35 },
                    { ClothesTypes.Accessory, 0 },
                    { ClothesTypes.Mask, 0 },
                    { ClothesTypes.Bag, 0 }
                }
            },
        };

        public class HairStyle
        {
            public int ID { get; set; }

            public byte Overlay { get; set; }

            public byte Color { get; set; }

            public byte Color2 { get; set; }

            public HairStyle(int ID, byte Overlay, byte Color, byte Color2)
            {
                this.ID = ID;
                this.Overlay = Overlay;
                this.Color = Color;
                this.Color2 = Color2;
            }
        }

        #region Defaults
        public class Defaults
        {
            public static HairStyle HairStyle = new HairStyle(0, 0, 0, 0);

            public static HeadBlend HeadBlend = new HeadBlend { ShapeFirst = 21, ShapeSecond = 0, SkinFirst = 21, SkinSecond = 0, ShapeMix = 0.5f, SkinMix = 0.5f, ShapeThird = 0, SkinThird = 0, ThirdMix = 0f };

            public static float[] FaceFeatures = new float[20] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            public static Dictionary<int, HeadOverlay> HeadOverlays = new Dictionary<int, HeadOverlay>()
            {
                { 0, new HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 1 } }, // Blemishes (0-23)
                { 1, new HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 1 } }, // Facial Hair (0-28)
                { 2, new HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 1 } }, // Eye Brows
                { 3, new HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 1 } }, // Ageing (0-33)
                { 4, new HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 0.5f } }, // Makeup (0-14)
                { 5, new HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 0.5f } }, // Blush (0-32)
                { 6, new HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 1 } }, // Complexion (0-11)
                { 7, new HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 1 } }, // Sun Damage (0-10)
                { 8, new HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 0.5f } }, // Lipstick (0-9)
                { 9, new HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 1 } }, // Moles/Freckles (0-17)
                { 10, new HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 1 } }, // Chest Hair (0-16)
                { 11, new HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 1 } }, // Body Blemishes (0-11)
                { 12, new HeadOverlay() { Index = 255, Color = 0, SecondaryColor = 0, Opacity = 0 } }, // Add Body Blemishes (0-1)
            };

            public static byte EyeColor = 0;

            public static Decoration[] Decorations = new Decoration[] { };
        }
        #endregion

        #region All Hairs
        private static Dictionary<int, int> MaleHairs = new Dictionary<int, int>()
        {
            // GTA Default
            { 0, 0 }, { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 }, { 5, 5 }, { 6, 6 }, { 7, 7 }, { 8, 8 }, { 9, 9 }, { 10, 10 }, { 11, 11 }, { 12, 12 }, { 13, 13 }, { 14, 14 }, { 15, 15 }, { 16, 16 }, { 17, 17 },{ 18, 18 }, { 19, 19 }, { 20, 20 }, { 21, 21 }, { 22, 22 }, { 23, 24 }, { 24, 25 }, { 25, 26 }, { 26, 27 }, { 27, 28}, { 28, 29 }, { 29, 30 }, { 30, 31 }, { 31, 32 }, { 32,33 }, { 33, 34 }, { 34, 35 }, { 35, 36 }, { 36, 72}, { 37, 73}, { 38, 74 }, { 39, 75 }, { 40, 76 },

            // Modded
        };

        private static Dictionary<int, int> FemaleHairs = new Dictionary<int, int>()
        {
            // GTA Default
            { 0, 0 }, { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 }, { 5, 5 }, { 6, 6 }, { 7, 7 }, { 8, 8 }, { 9, 9 }, { 10, 10 }, { 11, 11 }, { 12, 12 }, { 13, 13 }, { 14, 14 }, { 15, 15 }, { 16, 16 }, { 17, 17 },{ 18, 18 }, { 19, 19 }, { 20, 20 }, { 21, 21 }, { 22, 22 }, { 23, 23 }, { 24, 25 }, { 25, 26 }, { 26, 27 }, { 27, 28}, { 28, 29 }, { 29, 30 }, { 30, 31 }, { 31, 32 }, { 32,33 }, { 33, 34 }, { 34, 35 }, { 35, 36 }, { 36, 37 }, { 37, 38 }, { 38, 76}, { 39, 77}, { 40, 78 }, { 41, 79 }, { 42, 80 },

            // Modded
        };
        #endregion

        #region Stuff
        public static int GetHair(bool sex, int id)
        {
            if (sex)
            {
                return MaleHairs.GetValueOrDefault(id);
            }
            else
            {
                return FemaleHairs.GetValueOrDefault(id);
            }
        }

        public static int GetNudeDrawable(ClothesTypes cType, bool sex) => NudeClothes[sex].GetValueOrDefault(cType);
        #endregion
    }
}
