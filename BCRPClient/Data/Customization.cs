using RAGE;
using System;
using System.Collections.Generic;

namespace BCRPClient.Data
{
    public class Customization
    {
        #region Classes
        public class HeadBlend
        {
            public byte ShapeFirst, ShapeSecond, ShapeThird, SkinFirst, SkinSecond, SkinThird;
            public float ShapeMix, SkinMix, ThirdMix;

            public void SetFather(byte value)
            {
                this.ShapeSecond = value;
                this.SkinSecond = value;
            }

            public void SetMother(byte value)
            {
                this.ShapeFirst = value;
                this.SkinFirst = value;
            }

            public byte GetFather() => this.ShapeSecond;
            public byte GetMother() => this.ShapeFirst;

            public HeadBlend()
            {
                this.ShapeThird = 0;
                this.SkinThird = 0;
                this.ThirdMix = 0f;
            }
        }

        public class HeadOverlay
        {
            public byte Index, Color, SecondaryColor;
            public float Opacity;

            public HeadOverlay() { }
        }

        public class Decoration
        {
            public uint Collection, Overlay;

            public Decoration() { }
        }

        public class HairStyle
        {
            public int ID { get; set; }
            public byte Overlay { get; set; }
            public byte Color { get; set; }
            public byte Color2 { get; set; }
        }

        public class HairOverlay
        {
            public uint Collection { get; set; }
            public uint Overlay { get; set; }

            public HairOverlay(string Collection, string Overlay)
            {
                this.Collection = RAGE.Util.Joaat.Hash(Collection);
                this.Overlay = RAGE.Util.Joaat.Hash(Overlay);
            }
        }
        #endregion

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

        private static List<HairOverlay> MaleHairOverlays = new List<HairOverlay>()
        {
            null, // 0

            new HairOverlay("mpbeach_overlays", "FM_Hair_Fuzz"), // 1

            new HairOverlay("multiplayer_overlays", "NG_M_Hair_001"), // 2
            new HairOverlay("multiplayer_overlays", "NG_M_Hair_002"), // 3
            new HairOverlay("multiplayer_overlays", "NG_M_Hair_003"), // 4
            new HairOverlay("multiplayer_overlays", "NG_M_Hair_004"), // 5
            new HairOverlay("multiplayer_overlays", "NG_M_Hair_005"), // 6
            new HairOverlay("multiplayer_overlays", "NG_M_Hair_006"), // 7
            new HairOverlay("multiplayer_overlays", "NG_M_Hair_007"), // 8
            new HairOverlay("multiplayer_overlays", "NG_M_Hair_008"), // 9
            new HairOverlay("multiplayer_overlays", "NG_M_Hair_009"), // 10
            new HairOverlay("multiplayer_overlays", "NG_M_Hair_010"), // 11
            new HairOverlay("multiplayer_overlays", "NG_M_Hair_011"), // 12
            new HairOverlay("multiplayer_overlays", "NG_M_Hair_012"), // 13
            new HairOverlay("multiplayer_overlays", "NG_M_Hair_013"), // 14
            new HairOverlay("multiplayer_overlays", "NG_M_Hair_014"), // 15
            new HairOverlay("multiplayer_overlays", "NG_M_Hair_015"), // 16

            new HairOverlay("multiplayer_overlays", "NGBea_M_Hair_000"), // 17
            new HairOverlay("multiplayer_overlays", "NGBea_M_Hair_001"), // 18
            new HairOverlay("multiplayer_overlays", "NGBus_M_Hair_000"), // 19
            new HairOverlay("multiplayer_overlays", "NGBus_M_Hair_001"), // 20
            new HairOverlay("multiplayer_overlays", "NGHip_M_Hair_000"), // 21
            new HairOverlay("multiplayer_overlays", "NGHip_M_Hair_001"), // 22
            new HairOverlay("multiplayer_overlays", "NGInd_M_Hair_000"), // 23

            new HairOverlay("mplowrider_overlays", "LR_M_Hair_000"), // 24
            new HairOverlay("mplowrider_overlays", "LR_M_Hair_001"), // 25
            new HairOverlay("mplowrider_overlays", "LR_M_Hair_002"), // 26
            new HairOverlay("mplowrider_overlays", "LR_M_Hair_003"), // 27

            new HairOverlay("mpbiker_overlays", "MP_Biker_Hair_000_M"), // 28
            new HairOverlay("mpbiker_overlays", "MP_Biker_Hair_001_M"), // 29
            new HairOverlay("mpbiker_overlays", "MP_Biker_Hair_002_M"), // 30
            new HairOverlay("mpbiker_overlays", "MP_Biker_Hair_003_M"), // 31
            new HairOverlay("mpbiker_overlays", "MP_Biker_Hair_004_M"), // 32
            new HairOverlay("mpbiker_overlays", "MP_Biker_Hair_005_M"), // 33
            new HairOverlay("mpbiker_overlays", "MP_Biker_Hair_006_M"), // 34

            new HairOverlay("mpgunrunning_overlays", "MP_Gunrunning_Hair_M_000_M"), // 35
            new HairOverlay("mpgunrunning_overlays", "MP_Gunrunning_Hair_M_001_M"), // 36
        };

        private static List<HairOverlay> FemaleHairOverlays = new List<HairOverlay>()
        {
            null,

            new HairOverlay("mpbeach_overlays", "FM_Hair_Fuzz"),

            new HairOverlay("multiplayer_overlays", "NG_F_Hair_001"),
            new HairOverlay("multiplayer_overlays", "NG_F_Hair_002"),
            new HairOverlay("multiplayer_overlays", "NG_F_Hair_003"),
            new HairOverlay("multiplayer_overlays", "NG_F_Hair_004"),
            new HairOverlay("multiplayer_overlays", "NG_F_Hair_005"),
            new HairOverlay("multiplayer_overlays", "NG_F_Hair_006"),
            new HairOverlay("multiplayer_overlays", "NG_F_Hair_007"),
            new HairOverlay("multiplayer_overlays", "NG_F_Hair_008"),
            new HairOverlay("multiplayer_overlays", "NG_F_Hair_009"),
            new HairOverlay("multiplayer_overlays", "NG_F_Hair_010"),
            new HairOverlay("multiplayer_overlays", "NG_F_Hair_011"),
            new HairOverlay("multiplayer_overlays", "NG_F_Hair_012"),
            new HairOverlay("multiplayer_overlays", "NG_F_Hair_013"),
            new HairOverlay("multiplayer_overlays", "NG_F_Hair_014"),
            new HairOverlay("multiplayer_overlays", "NG_F_Hair_015"),

            new HairOverlay("multiplayer_overlays", "NGBea_F_Hair_000"),
            new HairOverlay("multiplayer_overlays", "NGBea_F_Hair_001"),
            new HairOverlay("multiplayer_overlays", "NGBus_F_Hair_000"),
            new HairOverlay("multiplayer_overlays", "NGBus_F_Hair_001"),
            new HairOverlay("multiplayer_overlays", "NGHip_F_Hair_000"),
            new HairOverlay("multiplayer_overlays", "NGHip_F_Hair_001"),
            new HairOverlay("multiplayer_overlays", "NGInd_F_Hair_000"),

            new HairOverlay("mplowrider_overlays", "LR_F_Hair_000"),
            new HairOverlay("mplowrider_overlays", "LR_F_Hair_001"),
            new HairOverlay("mplowrider_overlays", "LR_F_Hair_002"),
            new HairOverlay("mplowrider_overlays", "LR_M_Hair_003"),

            new HairOverlay("mpbiker_overlays", "MP_Biker_Hair_000_F"),
            new HairOverlay("mpbiker_overlays", "MP_Biker_Hair_001_F"),
            new HairOverlay("mpbiker_overlays", "MP_Biker_Hair_002_F"),
            new HairOverlay("mpbiker_overlays", "MP_Biker_Hair_003_F"),
            new HairOverlay("mpbiker_overlays", "MP_Biker_Hair_004_F"),
            new HairOverlay("mpbiker_overlays", "MP_Biker_Hair_005_F"),
            new HairOverlay("mpbiker_overlays", "MP_Biker_Hair_006_F"),

            new HairOverlay("mpgunrunning_overlays", "MP_Gunrunning_Hair_F_000_F"),
            new HairOverlay("mpgunrunning_overlays", "MP_Gunrunning_Hair_F_001_F"),
        };

        private static Dictionary<int, int> MaleDefaultHairOverlays = new Dictionary<int, int>()
        {
            {0,  0},
            {1,  2},
            {2,  3},
            {3,  4},
            {4,  5},
            {5,  6},
            {6,  7},
            {7,  8},
            {8,  9},
            {9,  10},
            {10,  11},
            {11,  12},
            {12,  13},
            {13,  14},
            {14,  15},
            {15,  16},
            {16,  17},
            {17,  18},
            {18,  19},
            {19,  20},
            {20,  21},
            {21,  22},
            {22,  23},
            {23,  24},
            {24,  25},
            {25,  26},
            {26,  27},
            {27,  26},
            {28,  26},
            {29,  0},
            {30,  28},
            {31,  29},
            {32,  30},
            {33,  31},
            {34,  32},
            {35,  33},
            {36,  35},
            {37,  36},
            {38,  35},
            {39,  2},
            {40,  30},
        };

        private static Dictionary<int, int> FemaleDefaultHairOverlays = new Dictionary<int, int>()
        {
            {0,  0},
            {1,  2},
            {2,  3},
            {3,  4},
            {4,  5},
            {5,  6},
            {6,  7},
            {7,  8},
            {8,  9},
            {9,  10},
            {10,  11},
            {11,  12},
            {12,  13},
            {13,  14},
            {14,  15},
            {15,  16},
            {16,  17},
            {17,  18},
            {18,  8},
            {19,  19},
            {20,  20},
            {21,  18},
            {22,  21},
            {23,  23},
            {24,  24},
            {25,  25},
            {26,  26},
            {27,  27},
            {28,  27},
            {29,  0},
            {30,  0},
            {31,  28},
            {32,  29},
            {33,  30},
            {34,  31},
            {35,  4},
            {36,  34},
            {37,  32},
            {38,  35},
            {39,  36},
            {40,  0},
            {41,  33},
            {42,  33},
        };

        #region Stuff
        #region Hair
        public static int GetHair(bool sex, int id)
        {
            if (sex)
            {
                if (!MaleHairs.ContainsKey(id))
                    return 0;

                return MaleHairs[id];
            }
            else
            {
                if (!FemaleHairs.ContainsKey(id))
                    return 0;

                return FemaleHairs[id];
            }
        }

        public static HairOverlay GetHairOverlay(bool sex, int id)
        {
            if (sex)
            {
                if (id < 0 || id >= MaleHairOverlays.Count)
                    return null;

                return MaleHairOverlays[id];
            }
            else
            {
                if (id < 0 || id >= FemaleHairOverlays.Count)
                    return null;

                return FemaleHairOverlays[id];
            }
        }

        public static int GetDefaultHairOverlayId(bool sex, int hairId)
        {
            if (sex)
            {
                if (!MaleDefaultHairOverlays.ContainsKey(hairId))
                    return 0;
                else
                    return MaleDefaultHairOverlays[hairId];
            }
            else
            {
                if (!FemaleDefaultHairOverlays.ContainsKey(hairId))
                    return 0;
                else
                    return FemaleDefaultHairOverlays[hairId];
            }
        }
        #endregion

        #endregion
    }
}
