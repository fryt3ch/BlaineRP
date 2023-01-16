using Newtonsoft.Json;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Data
{
    public class Customization
    {
        #region Classes
        public class TattooData
        {
            public enum ZoneTypes
            {
                Head = 0,
                Torso,
                LeftArm,
                RightArm,
                LeftLeg,
                RightLeg,
            }

            public ZoneTypes ZoneType { get; set; }

            public uint CollectionHash { get; set; }

            public uint? HashMale { get; set; }

            public uint? HashFemale { get; set; }

            public string Name { get; set; }

            public TattooData(string CollectionString, string Name, string HashMaleString, string HashFemaleString, ZoneTypes ZoneType)
            {
                this.Name = Name;
                this.ZoneType = ZoneType;

                this.CollectionHash = RAGE.Util.Joaat.Hash(CollectionString);

                if (HashMaleString != null)
                    this.HashMale = RAGE.Util.Joaat.Hash(HashMaleString);

                if (HashFemaleString != null)
                    this.HashFemale = RAGE.Util.Joaat.Hash(HashFemaleString);
            }

            public bool TryApply(Player player)
            {
                var sex = player.GetSex();

                if (sex)
                {
                    if (HashMale is uint hmu)
                        player.SetDecoration(CollectionHash, hmu);
                    else
                        return false;
                }
                else
                {
                    if (HashFemale is uint hmu)
                        player.SetDecoration(CollectionHash, hmu);
                    else
                        return false;
                }

                return true;
            }

            public static void ClearAll(Player player)
            {
                player.ClearDecorations();

                if (Sync.Players.GetData(player) is Sync.Players.PlayerData pData)
                    pData.HairOverlay?.Apply(player);
            }
        }

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
            [JsonProperty(PropertyName = "I")]
            public byte Index { get; set; }

            [JsonProperty(PropertyName = "C")]
            public byte Color { get; set; }

            [JsonProperty(PropertyName = "C2")]
            public byte SecondaryColor { get; set; }

            [JsonProperty(PropertyName = "O")]
            public float Opacity { get; set; }

            public HeadOverlay() { }
        }

        public class Decoration
        {
            public uint Collection, Overlay;

            public Decoration() { }
        }

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

            public HairStyle() { }
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

            public void Apply(Player player) => player.SetFacialDecoration(Collection, Overlay);

            public static void ClearAll(Player player)
            {
                player.ClearFacialDecorations();

                if (Sync.Players.GetData(player) is Sync.Players.PlayerData pData && pData.Decorations is List<int> decors)
                {
                    foreach (var x in decors)
                        GetTattooData(x)?.TryApply(player);
                }
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

        public static List<HairOverlay> MaleHairOverlays { get; private set; } = new List<HairOverlay>()
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

        public static List<HairOverlay> FemaleHairOverlays { get; private set; } = new List<HairOverlay>()
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

        public static Dictionary<int, int> MaleDefaultHairOverlays { get; private set; } = new Dictionary<int, int>()
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

        public static Dictionary<int, int> FemaleDefaultHairOverlays { get; private set; } = new Dictionary<int, int>()
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

        public static Dictionary<int, TattooData> AllTattoos { get; private set; } = new Dictionary<int, TattooData>()
        {
            { 0, new TattooData("mpairraces_overlays", "TAT_AR_000", "MP_Airraces_Tattoo_000_M", "MP_Airraces_Tattoo_000_F", TattooData.ZoneTypes.Torso) },
            { 1, new TattooData("mpairraces_overlays", "TAT_AR_001", "MP_Airraces_Tattoo_001_M", "MP_Airraces_Tattoo_001_F", TattooData.ZoneTypes.Torso) },
            { 2, new TattooData("mpairraces_overlays", "TAT_AR_002", "MP_Airraces_Tattoo_002_M", "MP_Airraces_Tattoo_002_F", TattooData.ZoneTypes.Torso) },
            { 3, new TattooData("mpairraces_overlays", "TAT_AR_003", "MP_Airraces_Tattoo_003_M", "MP_Airraces_Tattoo_003_F", TattooData.ZoneTypes.LeftArm) },
            { 4, new TattooData("mpairraces_overlays", "TAT_AR_004", "MP_Airraces_Tattoo_004_M", "MP_Airraces_Tattoo_004_F", TattooData.ZoneTypes.Torso) },
            { 5, new TattooData("mpairraces_overlays", "TAT_AR_005", "MP_Airraces_Tattoo_005_M", "MP_Airraces_Tattoo_005_F", TattooData.ZoneTypes.Torso) },
            { 6, new TattooData("mpairraces_overlays", "TAT_AR_006", "MP_Airraces_Tattoo_006_M", "MP_Airraces_Tattoo_006_F", TattooData.ZoneTypes.Torso) },
            { 7, new TattooData("mpairraces_overlays", "TAT_AR_007", "MP_Airraces_Tattoo_007_M", "MP_Airraces_Tattoo_007_F", TattooData.ZoneTypes.Torso) },

            { 8, new TattooData("mpbeach_overlays", "TAT_BB_018", "MP_Bea_M_Back_000", null, TattooData.ZoneTypes.Torso) },
            { 9, new TattooData("mpbeach_overlays", "TAT_BB_019", "MP_Bea_M_Chest_000", null, TattooData.ZoneTypes.Torso) },
            { 10, new TattooData("mpbeach_overlays", "TAT_BB_020", "MP_Bea_M_Chest_001", null, TattooData.ZoneTypes.Torso) },
            { 11, new TattooData("mpbeach_overlays", "TAT_BB_021", "MP_Bea_M_Head_000", null, TattooData.ZoneTypes.Head) },
            { 12, new TattooData("mpbeach_overlays", "TAT_BB_022", "MP_Bea_M_Head_001", null, TattooData.ZoneTypes.Head) },
            { 13, new TattooData("mpbeach_overlays", "TAT_BB_031", "MP_Bea_M_Head_002", null, TattooData.ZoneTypes.Head) },
            { 14, new TattooData("mpbeach_overlays", "TAT_BB_027", "MP_Bea_M_Lleg_000", null, TattooData.ZoneTypes.LeftLeg) },
            { 15, new TattooData("mpbeach_overlays", "TAT_BB_025", "MP_Bea_M_Rleg_000", null, TattooData.ZoneTypes.RightLeg) },
            { 16, new TattooData("mpbeach_overlays", "TAT_BB_026", "MP_Bea_M_RArm_000", null, TattooData.ZoneTypes.RightArm) },
            { 17, new TattooData("mpbeach_overlays", "TAT_BB_024", "MP_Bea_M_LArm_000", null, TattooData.ZoneTypes.LeftArm) },
            { 18, new TattooData("mpbeach_overlays", "TAT_BB_017", "MP_Bea_M_LArm_001", null, TattooData.ZoneTypes.LeftArm) },
            { 19, new TattooData("mpbeach_overlays", "TAT_BB_028", "MP_Bea_M_Neck_000", null, TattooData.ZoneTypes.Head) },
            { 20, new TattooData("mpbeach_overlays", "TAT_BB_029", "MP_Bea_M_Neck_001", null, TattooData.ZoneTypes.Head) },
            { 21, new TattooData("mpbeach_overlays", "TAT_BB_030", "MP_Bea_M_RArm_001", null, TattooData.ZoneTypes.RightArm) },
            { 22, new TattooData("mpbeach_overlays", "TAT_BB_023", "MP_Bea_M_Stom_000", null, TattooData.ZoneTypes.Torso) },
            { 23, new TattooData("mpbeach_overlays", "TAT_BB_032", "MP_Bea_M_Stom_001", null, TattooData.ZoneTypes.Torso) },
            { 24, new TattooData("mpbeach_overlays", "TAT_BB_003", null, "MP_Bea_F_Back_000", TattooData.ZoneTypes.Torso) },
            { 25, new TattooData("mpbeach_overlays", "TAT_BB_001", null, "MP_Bea_F_Back_001", TattooData.ZoneTypes.Torso) },
            { 26, new TattooData("mpbeach_overlays", "TAT_BB_005", null, "MP_Bea_F_Back_002", TattooData.ZoneTypes.Torso) },
            { 27, new TattooData("mpbeach_overlays", "TAT_BB_012", null, "MP_Bea_F_Chest_000", TattooData.ZoneTypes.Torso) },
            { 28, new TattooData("mpbeach_overlays", "TAT_BB_013", null, "MP_Bea_F_Chest_001", TattooData.ZoneTypes.Torso) },
            { 29, new TattooData("mpbeach_overlays", "TAT_BB_000", null, "MP_Bea_F_Chest_002", TattooData.ZoneTypes.Torso) },
            { 30, new TattooData("mpbeach_overlays", "TAT_BB_006", null, "MP_Bea_F_RSide_000", TattooData.ZoneTypes.Torso) },
            { 31, new TattooData("mpbeach_overlays", "TAT_BB_007", null, "MP_Bea_F_RLeg_000", TattooData.ZoneTypes.RightLeg) },
            { 32, new TattooData("mpbeach_overlays", "TAT_BB_015", null, "MP_Bea_F_RArm_001", TattooData.ZoneTypes.RightArm) },
            { 33, new TattooData("mpbeach_overlays", "TAT_BB_008", null, "MP_Bea_F_Neck_000", TattooData.ZoneTypes.Head) },
            { 34, new TattooData("mpbeach_overlays", "TAT_BB_011", null, "MP_Bea_F_Should_000", TattooData.ZoneTypes.Torso) },
            { 35, new TattooData("mpbeach_overlays", "TAT_BB_004", null, "MP_Bea_F_Should_001", TattooData.ZoneTypes.Torso) },
            { 36, new TattooData("mpbeach_overlays", "TAT_BB_014", null, "MP_Bea_F_Stom_000", TattooData.ZoneTypes.Torso) },
            { 37, new TattooData("mpbeach_overlays", "TAT_BB_009", null, "MP_Bea_F_Stom_001", TattooData.ZoneTypes.Torso) },
            { 38, new TattooData("mpbeach_overlays", "TAT_BB_010", null, "MP_Bea_F_Stom_002", TattooData.ZoneTypes.Torso) },
            { 39, new TattooData("mpbeach_overlays", "TAT_BB_002", null, "MP_Bea_F_LArm_000", TattooData.ZoneTypes.LeftArm) },
            { 40, new TattooData("mpbeach_overlays", "TAT_BB_016", null, "MP_Bea_F_LArm_001", TattooData.ZoneTypes.LeftArm) },

            { 41, new TattooData("mpbiker_overlays", "TAT_BI_000", "MP_MP_Biker_Tat_000_M", "MP_MP_Biker_Tat_000_F", TattooData.ZoneTypes.Torso) },
            { 42, new TattooData("mpbiker_overlays", "TAT_BI_001", "MP_MP_Biker_Tat_001_M", "MP_MP_Biker_Tat_001_F", TattooData.ZoneTypes.Torso) },
            { 43, new TattooData("mpbiker_overlays", "TAT_BI_002", "MP_MP_Biker_Tat_002_M", "MP_MP_Biker_Tat_002_F", TattooData.ZoneTypes.LeftLeg) },
            { 44, new TattooData("mpbiker_overlays", "TAT_BI_003", "MP_MP_Biker_Tat_003_M", "MP_MP_Biker_Tat_003_F", TattooData.ZoneTypes.Torso) },
            { 45, new TattooData("mpbiker_overlays", "TAT_BI_004", "MP_MP_Biker_Tat_004_M", "MP_MP_Biker_Tat_004_F", TattooData.ZoneTypes.RightLeg) },
            { 46, new TattooData("mpbiker_overlays", "TAT_BI_005", "MP_MP_Biker_Tat_005_M", "MP_MP_Biker_Tat_005_F", TattooData.ZoneTypes.Torso) },
            { 47, new TattooData("mpbiker_overlays", "TAT_BI_006", "MP_MP_Biker_Tat_006_M", "MP_MP_Biker_Tat_006_F", TattooData.ZoneTypes.Torso) },
            { 48, new TattooData("mpbiker_overlays", "TAT_BI_007", "MP_MP_Biker_Tat_007_M", "MP_MP_Biker_Tat_007_F", TattooData.ZoneTypes.RightArm) },
            { 49, new TattooData("mpbiker_overlays", "TAT_BI_008", "MP_MP_Biker_Tat_008_M", "MP_MP_Biker_Tat_008_F", TattooData.ZoneTypes.Torso) },
            { 50, new TattooData("mpbiker_overlays", "TAT_BI_009", "MP_MP_Biker_Tat_009_M", "MP_MP_Biker_Tat_009_F", TattooData.ZoneTypes.Head) },
            { 51, new TattooData("mpbiker_overlays", "TAT_BI_010", "MP_MP_Biker_Tat_010_M", "MP_MP_Biker_Tat_010_F", TattooData.ZoneTypes.Torso) },
            { 52, new TattooData("mpbiker_overlays", "TAT_BI_011", "MP_MP_Biker_Tat_011_M", "MP_MP_Biker_Tat_011_F", TattooData.ZoneTypes.Torso) },
            { 53, new TattooData("mpbiker_overlays", "TAT_BI_012", "MP_MP_Biker_Tat_012_M", "MP_MP_Biker_Tat_012_F", TattooData.ZoneTypes.LeftArm) },
            { 54, new TattooData("mpbiker_overlays", "TAT_BI_013", "MP_MP_Biker_Tat_013_M", "MP_MP_Biker_Tat_013_F", TattooData.ZoneTypes.Torso) },
            { 55, new TattooData("mpbiker_overlays", "TAT_BI_014", "MP_MP_Biker_Tat_014_M", "MP_MP_Biker_Tat_014_F", TattooData.ZoneTypes.RightArm) },
            { 56, new TattooData("mpbiker_overlays", "TAT_BI_015", "MP_MP_Biker_Tat_015_M", "MP_MP_Biker_Tat_015_F", TattooData.ZoneTypes.LeftLeg) },
            { 57, new TattooData("mpbiker_overlays", "TAT_BI_016", "MP_MP_Biker_Tat_016_M", "MP_MP_Biker_Tat_016_F", TattooData.ZoneTypes.LeftArm) },
            { 58, new TattooData("mpbiker_overlays", "TAT_BI_017", "MP_MP_Biker_Tat_017_M", "MP_MP_Biker_Tat_017_F", TattooData.ZoneTypes.Torso) },
            { 59, new TattooData("mpbiker_overlays", "TAT_BI_018", "MP_MP_Biker_Tat_018_M", "MP_MP_Biker_Tat_018_F", TattooData.ZoneTypes.Torso) },
            { 60, new TattooData("mpbiker_overlays", "TAT_BI_019", "MP_MP_Biker_Tat_019_M", "MP_MP_Biker_Tat_019_F", TattooData.ZoneTypes.Torso) },
            { 61, new TattooData("mpbiker_overlays", "TAT_BI_020", "MP_MP_Biker_Tat_020_M", "MP_MP_Biker_Tat_020_F", TattooData.ZoneTypes.LeftArm) },
            { 62, new TattooData("mpbiker_overlays", "TAT_BI_021", "MP_MP_Biker_Tat_021_M", "MP_MP_Biker_Tat_021_F", TattooData.ZoneTypes.Torso) },
            { 63, new TattooData("mpbiker_overlays", "TAT_BI_022", "MP_MP_Biker_Tat_022_M", "MP_MP_Biker_Tat_022_F", TattooData.ZoneTypes.RightLeg) },
            { 64, new TattooData("mpbiker_overlays", "TAT_BI_023", "MP_MP_Biker_Tat_023_M", "MP_MP_Biker_Tat_023_F", TattooData.ZoneTypes.Torso) },
            { 65, new TattooData("mpbiker_overlays", "TAT_BI_024", "MP_MP_Biker_Tat_024_M", "MP_MP_Biker_Tat_024_F", TattooData.ZoneTypes.LeftArm) },
            { 66, new TattooData("mpbiker_overlays", "TAT_BI_025", "MP_MP_Biker_Tat_025_M", "MP_MP_Biker_Tat_025_F", TattooData.ZoneTypes.LeftArm) },
            { 67, new TattooData("mpbiker_overlays", "TAT_BI_026", "MP_MP_Biker_Tat_026_M", "MP_MP_Biker_Tat_026_F", TattooData.ZoneTypes.Torso) },
            { 68, new TattooData("mpbiker_overlays", "TAT_BI_027", "MP_MP_Biker_Tat_027_M", "MP_MP_Biker_Tat_027_F", TattooData.ZoneTypes.LeftLeg) },
            { 69, new TattooData("mpbiker_overlays", "TAT_BI_028", "MP_MP_Biker_Tat_028_M", "MP_MP_Biker_Tat_028_F", TattooData.ZoneTypes.RightLeg) },
            { 70, new TattooData("mpbiker_overlays", "TAT_BI_029", "MP_MP_Biker_Tat_029_M", "MP_MP_Biker_Tat_029_F", TattooData.ZoneTypes.Torso) },
            { 71, new TattooData("mpbiker_overlays", "TAT_BI_030", "MP_MP_Biker_Tat_030_M", "MP_MP_Biker_Tat_030_F", TattooData.ZoneTypes.Torso) },
            { 72, new TattooData("mpbiker_overlays", "TAT_BI_031", "MP_MP_Biker_Tat_031_M", "MP_MP_Biker_Tat_031_F", TattooData.ZoneTypes.Torso) },
            { 73, new TattooData("mpbiker_overlays", "TAT_BI_032", "MP_MP_Biker_Tat_032_M", "MP_MP_Biker_Tat_032_F", TattooData.ZoneTypes.Torso) },
            { 74, new TattooData("mpbiker_overlays", "TAT_BI_033", "MP_MP_Biker_Tat_033_M", "MP_MP_Biker_Tat_033_F", TattooData.ZoneTypes.RightArm) },
            { 75, new TattooData("mpbiker_overlays", "TAT_BI_034", "MP_MP_Biker_Tat_034_M", "MP_MP_Biker_Tat_034_F", TattooData.ZoneTypes.Torso) },
            { 76, new TattooData("mpbiker_overlays", "TAT_BI_035", "MP_MP_Biker_Tat_035_M", "MP_MP_Biker_Tat_035_F", TattooData.ZoneTypes.LeftArm) },
            { 77, new TattooData("mpbiker_overlays", "TAT_BI_036", "MP_MP_Biker_Tat_036_M", "MP_MP_Biker_Tat_036_F", TattooData.ZoneTypes.LeftLeg) },
            { 78, new TattooData("mpbiker_overlays", "TAT_BI_037", "MP_MP_Biker_Tat_037_M", "MP_MP_Biker_Tat_037_F", TattooData.ZoneTypes.LeftLeg) },
            { 79, new TattooData("mpbiker_overlays", "TAT_BI_038", "MP_MP_Biker_Tat_038_M", "MP_MP_Biker_Tat_038_F", TattooData.ZoneTypes.Head) },
            { 80, new TattooData("mpbiker_overlays", "TAT_BI_039", "MP_MP_Biker_Tat_039_M", "MP_MP_Biker_Tat_039_F", TattooData.ZoneTypes.Torso) },
            { 81, new TattooData("mpbiker_overlays", "TAT_BI_040", "MP_MP_Biker_Tat_040_M", "MP_MP_Biker_Tat_040_F", TattooData.ZoneTypes.RightLeg) },
            { 82, new TattooData("mpbiker_overlays", "TAT_BI_041", "MP_MP_Biker_Tat_041_M", "MP_MP_Biker_Tat_041_F", TattooData.ZoneTypes.Torso) },
            { 83, new TattooData("mpbiker_overlays", "TAT_BI_042", "MP_MP_Biker_Tat_042_M", "MP_MP_Biker_Tat_042_F", TattooData.ZoneTypes.RightArm) },
            { 84, new TattooData("mpbiker_overlays", "TAT_BI_043", "MP_MP_Biker_Tat_043_M", "MP_MP_Biker_Tat_043_F", TattooData.ZoneTypes.Torso) },
            { 85, new TattooData("mpbiker_overlays", "TAT_BI_044", "MP_MP_Biker_Tat_044_M", "MP_MP_Biker_Tat_044_F", TattooData.ZoneTypes.LeftLeg) },
            { 86, new TattooData("mpbiker_overlays", "TAT_BI_045", "MP_MP_Biker_Tat_045_M", "MP_MP_Biker_Tat_045_F", TattooData.ZoneTypes.LeftArm) },
            { 87, new TattooData("mpbiker_overlays", "TAT_BI_046", "MP_MP_Biker_Tat_046_M", "MP_MP_Biker_Tat_046_F", TattooData.ZoneTypes.RightArm) },
            { 88, new TattooData("mpbiker_overlays", "TAT_BI_047", "MP_MP_Biker_Tat_047_M", "MP_MP_Biker_Tat_047_F", TattooData.ZoneTypes.RightArm) },
            { 89, new TattooData("mpbiker_overlays", "TAT_BI_048", "MP_MP_Biker_Tat_048_M", "MP_MP_Biker_Tat_048_F", TattooData.ZoneTypes.RightLeg) },
            { 90, new TattooData("mpbiker_overlays", "TAT_BI_049", "MP_MP_Biker_Tat_049_M", "MP_MP_Biker_Tat_049_F", TattooData.ZoneTypes.RightArm) },
            { 91, new TattooData("mpbiker_overlays", "TAT_BI_050", "MP_MP_Biker_Tat_050_M", "MP_MP_Biker_Tat_050_F", TattooData.ZoneTypes.Torso) },
            { 92, new TattooData("mpbiker_overlays", "TAT_BI_051", "MP_MP_Biker_Tat_051_M", "MP_MP_Biker_Tat_051_F", TattooData.ZoneTypes.Head) },
            { 93, new TattooData("mpbiker_overlays", "TAT_BI_052", "MP_MP_Biker_Tat_052_M", "MP_MP_Biker_Tat_052_F", TattooData.ZoneTypes.Torso) },
            { 94, new TattooData("mpbiker_overlays", "TAT_BI_053", "MP_MP_Biker_Tat_053_M", "MP_MP_Biker_Tat_053_F", TattooData.ZoneTypes.LeftArm) },
            { 95, new TattooData("mpbiker_overlays", "TAT_BI_054", "MP_MP_Biker_Tat_054_M", "MP_MP_Biker_Tat_054_F", TattooData.ZoneTypes.RightArm) },
            { 96, new TattooData("mpbiker_overlays", "TAT_BI_055", "MP_MP_Biker_Tat_055_M", "MP_MP_Biker_Tat_055_F", TattooData.ZoneTypes.LeftArm) },
            { 97, new TattooData("mpbiker_overlays", "TAT_BI_056", "MP_MP_Biker_Tat_056_M", "MP_MP_Biker_Tat_056_F", TattooData.ZoneTypes.LeftLeg) },
            { 98, new TattooData("mpbiker_overlays", "TAT_BI_057", "MP_MP_Biker_Tat_057_M", "MP_MP_Biker_Tat_057_F", TattooData.ZoneTypes.LeftLeg) },
            { 99, new TattooData("mpbiker_overlays", "TAT_BI_058", "MP_MP_Biker_Tat_058_M", "MP_MP_Biker_Tat_058_F", TattooData.ZoneTypes.Torso) },
            { 100, new TattooData("mpbiker_overlays", "TAT_BI_059", "MP_MP_Biker_Tat_059_M", "MP_MP_Biker_Tat_059_F", TattooData.ZoneTypes.Torso) },
            { 101, new TattooData("mpbiker_overlays", "TAT_BI_060", "MP_MP_Biker_Tat_060_M", "MP_MP_Biker_Tat_060_F", TattooData.ZoneTypes.Torso) },

            { 102, new TattooData("mpbusiness_overlays", "TAT_BUS_005", "MP_Buis_M_Neck_000", null, TattooData.ZoneTypes.Head) },
            { 103, new TattooData("mpbusiness_overlays", "TAT_BUS_006", "MP_Buis_M_Neck_001", null, TattooData.ZoneTypes.Head) },
            { 104, new TattooData("mpbusiness_overlays", "TAT_BUS_007", "MP_Buis_M_Neck_002", null, TattooData.ZoneTypes.Head) },
            { 105, new TattooData("mpbusiness_overlays", "TAT_BUS_008", "MP_Buis_M_Neck_003", null, TattooData.ZoneTypes.Head) },
            { 106, new TattooData("mpbusiness_overlays", "TAT_BUS_003", "MP_Buis_M_LeftArm_000", null, TattooData.ZoneTypes.LeftArm) },
            { 107, new TattooData("mpbusiness_overlays", "TAT_BUS_004", "MP_Buis_M_LeftArm_001", null, TattooData.ZoneTypes.LeftArm) },
            { 108, new TattooData("mpbusiness_overlays", "TAT_BUS_009", "MP_Buis_M_RightArm_000", null, TattooData.ZoneTypes.RightArm) },
            { 109, new TattooData("mpbusiness_overlays", "TAT_BUS_010", "MP_Buis_M_RightArm_001", null, TattooData.ZoneTypes.RightArm) },
            { 110, new TattooData("mpbusiness_overlays", "TAT_BUS_011", "MP_Buis_M_Stomach_000", null, TattooData.ZoneTypes.Torso) },
            { 111, new TattooData("mpbusiness_overlays", "TAT_BUS_001", "MP_Buis_M_Chest_000", null, TattooData.ZoneTypes.Torso) },
            { 112, new TattooData("mpbusiness_overlays", "TAT_BUS_002", "MP_Buis_M_Chest_001", null, TattooData.ZoneTypes.Torso) },
            { 113, new TattooData("mpbusiness_overlays", "TAT_BUS_000", "MP_Buis_M_Back_000", null, TattooData.ZoneTypes.Torso) },
            { 114, new TattooData("mpbusiness_overlays", "TAT_BUS_F_002", null, "MP_Buis_F_Chest_000", TattooData.ZoneTypes.Torso) },
            { 115, new TattooData("mpbusiness_overlays", "TAT_BUS_F_003", null, "MP_Buis_F_Chest_001", TattooData.ZoneTypes.Torso) },
            { 116, new TattooData("mpbusiness_overlays", "TAT_BUS_F_004", null, "MP_Buis_F_Chest_002", TattooData.ZoneTypes.Torso) },
            { 117, new TattooData("mpbusiness_overlays", "TAT_BUS_F_011", null, "MP_Buis_F_Stom_000", TattooData.ZoneTypes.Torso) },
            { 118, new TattooData("mpbusiness_overlays", "TAT_BUS_F_012", null, "MP_Buis_F_Stom_001", TattooData.ZoneTypes.Torso) },
            { 119, new TattooData("mpbusiness_overlays", "TAT_BUS_F_013", null, "MP_Buis_F_Stom_002", TattooData.ZoneTypes.Torso) },
            { 120, new TattooData("mpbusiness_overlays", "TAT_BUS_F_000", null, "MP_Buis_F_Back_000", TattooData.ZoneTypes.Torso) },
            { 121, new TattooData("mpbusiness_overlays", "TAT_BUS_F_001", null, "MP_Buis_F_Back_001", TattooData.ZoneTypes.Torso) },
            { 122, new TattooData("mpbusiness_overlays", "TAT_BUS_F_007", null, "MP_Buis_F_Neck_000", TattooData.ZoneTypes.Head) },
            { 123, new TattooData("mpbusiness_overlays", "TAT_BUS_F_008", null, "MP_Buis_F_Neck_001", TattooData.ZoneTypes.Head) },
            { 124, new TattooData("mpbusiness_overlays", "TAT_BUS_F_009", null, "MP_Buis_F_RArm_000", TattooData.ZoneTypes.RightArm) },
            { 125, new TattooData("mpbusiness_overlays", "TAT_BUS_F_005", null, "MP_Buis_F_LArm_000", TattooData.ZoneTypes.LeftArm) },
            { 126, new TattooData("mpbusiness_overlays", "TAT_BUS_F_006", null, "MP_Buis_F_LLeg_000", TattooData.ZoneTypes.LeftLeg) },
            { 127, new TattooData("mpbusiness_overlays", "TAT_BUS_F_010", null, "MP_Buis_F_RLeg_000", TattooData.ZoneTypes.RightLeg) },

            { 128, new TattooData("mpchristmas2017_overlays", "TAT_H27_000", "MP_Christmas2017_Tattoo_000_M", "MP_Christmas2017_Tattoo_000_F", TattooData.ZoneTypes.Torso) },
            { 129, new TattooData("mpchristmas2017_overlays", "TAT_H27_001", "MP_Christmas2017_Tattoo_001_M", "MP_Christmas2017_Tattoo_001_F", TattooData.ZoneTypes.LeftArm) },
            { 130, new TattooData("mpchristmas2017_overlays", "TAT_H27_002", "MP_Christmas2017_Tattoo_002_M", "MP_Christmas2017_Tattoo_002_F", TattooData.ZoneTypes.Torso) },
            { 131, new TattooData("mpchristmas2017_overlays", "TAT_H27_003", "MP_Christmas2017_Tattoo_003_M", "MP_Christmas2017_Tattoo_003_F", TattooData.ZoneTypes.Torso) },
            { 132, new TattooData("mpchristmas2017_overlays", "TAT_H27_004", "MP_Christmas2017_Tattoo_004_M", "MP_Christmas2017_Tattoo_004_F", TattooData.ZoneTypes.LeftArm) },
            { 133, new TattooData("mpchristmas2017_overlays", "TAT_H27_005", "MP_Christmas2017_Tattoo_005_M", "MP_Christmas2017_Tattoo_005_F", TattooData.ZoneTypes.Torso) },
            { 134, new TattooData("mpchristmas2017_overlays", "TAT_H27_006", "MP_Christmas2017_Tattoo_006_M", "MP_Christmas2017_Tattoo_006_F", TattooData.ZoneTypes.RightArm) },
            { 135, new TattooData("mpchristmas2017_overlays", "TAT_H27_007", "MP_Christmas2017_Tattoo_007_M", "MP_Christmas2017_Tattoo_007_F", TattooData.ZoneTypes.LeftArm) },
            { 136, new TattooData("mpchristmas2017_overlays", "TAT_H27_008", "MP_Christmas2017_Tattoo_008_M", "MP_Christmas2017_Tattoo_008_F", TattooData.ZoneTypes.Torso) },
            { 137, new TattooData("mpchristmas2017_overlays", "TAT_H27_009", "MP_Christmas2017_Tattoo_009_M", "MP_Christmas2017_Tattoo_009_F", TattooData.ZoneTypes.Torso) },
            { 138, new TattooData("mpchristmas2017_overlays", "TAT_H27_010", "MP_Christmas2017_Tattoo_010_M", "MP_Christmas2017_Tattoo_010_F", TattooData.ZoneTypes.Torso) },
            { 139, new TattooData("mpchristmas2017_overlays", "TAT_H27_011", "MP_Christmas2017_Tattoo_011_M", "MP_Christmas2017_Tattoo_011_F", TattooData.ZoneTypes.Torso) },
            { 140, new TattooData("mpchristmas2017_overlays", "TAT_H27_012", "MP_Christmas2017_Tattoo_012_M", "MP_Christmas2017_Tattoo_012_F", TattooData.ZoneTypes.RightArm) },
            { 141, new TattooData("mpchristmas2017_overlays", "TAT_H27_013", "MP_Christmas2017_Tattoo_013_M", "MP_Christmas2017_Tattoo_013_F", TattooData.ZoneTypes.LeftArm) },
            { 142, new TattooData("mpchristmas2017_overlays", "TAT_H27_014", "MP_Christmas2017_Tattoo_014_M", "MP_Christmas2017_Tattoo_014_F", TattooData.ZoneTypes.RightArm) },
            { 143, new TattooData("mpchristmas2017_overlays", "TAT_H27_015", "MP_Christmas2017_Tattoo_015_M", "MP_Christmas2017_Tattoo_015_F", TattooData.ZoneTypes.Torso) },
            { 144, new TattooData("mpchristmas2017_overlays", "TAT_H27_016", "MP_Christmas2017_Tattoo_016_M", "MP_Christmas2017_Tattoo_016_F", TattooData.ZoneTypes.Torso) },
            { 145, new TattooData("mpchristmas2017_overlays", "TAT_H27_017", "MP_Christmas2017_Tattoo_017_M", "MP_Christmas2017_Tattoo_017_F", TattooData.ZoneTypes.RightArm) },
            { 146, new TattooData("mpchristmas2017_overlays", "TAT_H27_018", "MP_Christmas2017_Tattoo_018_M", "MP_Christmas2017_Tattoo_018_F", TattooData.ZoneTypes.RightArm) },
            { 147, new TattooData("mpchristmas2017_overlays", "TAT_H27_019", "MP_Christmas2017_Tattoo_019_M", "MP_Christmas2017_Tattoo_019_F", TattooData.ZoneTypes.Torso) },
            { 148, new TattooData("mpchristmas2017_overlays", "TAT_H27_020", "MP_Christmas2017_Tattoo_020_M", "MP_Christmas2017_Tattoo_020_F", TattooData.ZoneTypes.Torso) },
            { 149, new TattooData("mpchristmas2017_overlays", "TAT_H27_021", "MP_Christmas2017_Tattoo_021_M", "MP_Christmas2017_Tattoo_021_F", TattooData.ZoneTypes.Torso) },
            { 150, new TattooData("mpchristmas2017_overlays", "TAT_H27_022", "MP_Christmas2017_Tattoo_022_M", "MP_Christmas2017_Tattoo_022_F", TattooData.ZoneTypes.Torso) },
            { 151, new TattooData("mpchristmas2017_overlays", "TAT_H27_023", "MP_Christmas2017_Tattoo_023_M", "MP_Christmas2017_Tattoo_023_F", TattooData.ZoneTypes.RightArm) },
            { 152, new TattooData("mpchristmas2017_overlays", "TAT_H27_024", "MP_Christmas2017_Tattoo_024_M", "MP_Christmas2017_Tattoo_024_F", TattooData.ZoneTypes.Torso) },
            { 153, new TattooData("mpchristmas2017_overlays", "TAT_H27_025", "MP_Christmas2017_Tattoo_025_M", "MP_Christmas2017_Tattoo_025_F", TattooData.ZoneTypes.LeftArm) },
            { 154, new TattooData("mpchristmas2017_overlays", "TAT_H27_026", "MP_Christmas2017_Tattoo_026_M", "MP_Christmas2017_Tattoo_026_F", TattooData.ZoneTypes.Torso) },
            { 155, new TattooData("mpchristmas2017_overlays", "TAT_H27_027", "MP_Christmas2017_Tattoo_027_M", "MP_Christmas2017_Tattoo_027_F", TattooData.ZoneTypes.Torso) },
            { 156, new TattooData("mpchristmas2017_overlays", "TAT_H27_028", "MP_Christmas2017_Tattoo_028_M", "MP_Christmas2017_Tattoo_028_F", TattooData.ZoneTypes.RightArm) },
            { 157, new TattooData("mpchristmas2017_overlays", "TAT_H27_029", "MP_Christmas2017_Tattoo_029_M", "MP_Christmas2017_Tattoo_029_F", TattooData.ZoneTypes.LeftArm) },

            { 158, new TattooData("mpchristmas2_overlays", "TAT_X2_000", "MP_Xmas2_M_Tat_000", "MP_Xmas2_F_Tat_000", TattooData.ZoneTypes.LeftArm) },
            { 159, new TattooData("mpchristmas2_overlays", "TAT_X2_001", "MP_Xmas2_M_Tat_001", "MP_Xmas2_F_Tat_001", TattooData.ZoneTypes.LeftLeg) },
            { 160, new TattooData("mpchristmas2_overlays", "TAT_X2_002", "MP_Xmas2_M_Tat_002", "MP_Xmas2_F_Tat_002", TattooData.ZoneTypes.LeftLeg) },
            { 161, new TattooData("mpchristmas2_overlays", "TAT_X2_003", "MP_Xmas2_M_Tat_003", "MP_Xmas2_F_Tat_003", TattooData.ZoneTypes.RightArm) },
            { 162, new TattooData("mpchristmas2_overlays", "TAT_X2_004", "MP_Xmas2_M_Tat_004", "MP_Xmas2_F_Tat_004", TattooData.ZoneTypes.RightArm) },
            { 163, new TattooData("mpchristmas2_overlays", "TAT_X2_005", "MP_Xmas2_M_Tat_005", "MP_Xmas2_F_Tat_005", TattooData.ZoneTypes.Torso) },
            { 164, new TattooData("mpchristmas2_overlays", "TAT_X2_006", "MP_Xmas2_M_Tat_006", "MP_Xmas2_F_Tat_006", TattooData.ZoneTypes.Torso) },
            { 165, new TattooData("mpchristmas2_overlays", "TAT_X2_007", "MP_Xmas2_M_Tat_007", "MP_Xmas2_F_Tat_007", TattooData.ZoneTypes.Head) },
            { 166, new TattooData("mpchristmas2_overlays", "TAT_X2_008", "MP_Xmas2_M_Tat_008", "MP_Xmas2_F_Tat_008", TattooData.ZoneTypes.RightArm) },
            { 167, new TattooData("mpchristmas2_overlays", "TAT_X2_009", "MP_Xmas2_M_Tat_009", "MP_Xmas2_F_Tat_009", TattooData.ZoneTypes.Torso) },
            { 168, new TattooData("mpchristmas2_overlays", "TAT_X2_010", "MP_Xmas2_M_Tat_010", "MP_Xmas2_F_Tat_010", TattooData.ZoneTypes.LeftArm) },
            { 169, new TattooData("mpchristmas2_overlays", "TAT_X2_011", "MP_Xmas2_M_Tat_011", "MP_Xmas2_F_Tat_011", TattooData.ZoneTypes.Torso) },
            { 170, new TattooData("mpchristmas2_overlays", "TAT_X2_012", "MP_Xmas2_M_Tat_012", "MP_Xmas2_F_Tat_012", TattooData.ZoneTypes.LeftArm) },
            { 171, new TattooData("mpchristmas2_overlays", "TAT_X2_013", "MP_Xmas2_M_Tat_013", "MP_Xmas2_F_Tat_013", TattooData.ZoneTypes.Torso) },
            { 172, new TattooData("mpchristmas2_overlays", "TAT_X2_014", "MP_Xmas2_M_Tat_014", "MP_Xmas2_F_Tat_014", TattooData.ZoneTypes.RightLeg) },
            { 173, new TattooData("mpchristmas2_overlays", "TAT_X2_015", "MP_Xmas2_M_Tat_015", "MP_Xmas2_F_Tat_015", TattooData.ZoneTypes.Torso) },
            { 174, new TattooData("mpchristmas2_overlays", "TAT_X2_016", "MP_Xmas2_M_Tat_016", "MP_Xmas2_F_Tat_016", TattooData.ZoneTypes.Torso) },
            { 175, new TattooData("mpchristmas2_overlays", "TAT_X2_017", "MP_Xmas2_M_Tat_017", "MP_Xmas2_F_Tat_017", TattooData.ZoneTypes.Torso) },
            { 176, new TattooData("mpchristmas2_overlays", "TAT_X2_018", "MP_Xmas2_M_Tat_018", "MP_Xmas2_F_Tat_018", TattooData.ZoneTypes.Torso) },
            { 177, new TattooData("mpchristmas2_overlays", "TAT_X2_019", "MP_Xmas2_M_Tat_019", "MP_Xmas2_F_Tat_019", TattooData.ZoneTypes.Torso) },
            { 178, new TattooData("mpchristmas2_overlays", "TAT_X2_020", "MP_Xmas2_M_Tat_020", "MP_Xmas2_F_Tat_020", TattooData.ZoneTypes.LeftArm) },
            { 179, new TattooData("mpchristmas2_overlays", "TAT_X2_021", "MP_Xmas2_M_Tat_021", "MP_Xmas2_F_Tat_021", TattooData.ZoneTypes.LeftArm) },
            { 180, new TattooData("mpchristmas2_overlays", "TAT_X2_022", "MP_Xmas2_M_Tat_022", "MP_Xmas2_F_Tat_022", TattooData.ZoneTypes.RightArm) },
            { 181, new TattooData("mpchristmas2_overlays", "TAT_X2_023", "MP_Xmas2_M_Tat_023", "MP_Xmas2_F_Tat_023", TattooData.ZoneTypes.RightArm) },
            { 182, new TattooData("mpchristmas2_overlays", "TAT_X2_024", "MP_Xmas2_M_Tat_024", "MP_Xmas2_F_Tat_024", TattooData.ZoneTypes.Head) },
            { 183, new TattooData("mpchristmas2_overlays", "TAT_X2_025", "MP_Xmas2_M_Tat_025", "MP_Xmas2_F_Tat_025", TattooData.ZoneTypes.Head) },
            { 184, new TattooData("mpchristmas2_overlays", "TAT_X2_026", "MP_Xmas2_M_Tat_026", "MP_Xmas2_F_Tat_026", TattooData.ZoneTypes.RightArm) },
            { 185, new TattooData("mpchristmas2_overlays", "TAT_X2_027", "MP_Xmas2_M_Tat_027", "MP_Xmas2_F_Tat_027", TattooData.ZoneTypes.RightArm) },
            { 186, new TattooData("mpchristmas2_overlays", "TAT_X2_028", "MP_Xmas2_M_Tat_028", "MP_Xmas2_F_Tat_028", TattooData.ZoneTypes.Torso) },
            { 187, new TattooData("mpchristmas2_overlays", "TAT_X2_029", "MP_Xmas2_M_Tat_029", "MP_Xmas2_F_Tat_029", TattooData.ZoneTypes.Head) },

            { 188, new TattooData("mpchristmas3_overlays", "TAT_X6_000", "MP_Christmas3_Tat_000_M", "MP_Christmas3_Tat_000_F", TattooData.ZoneTypes.LeftArm) },
            { 189, new TattooData("mpchristmas3_overlays", "TAT_X6_001", "MP_Christmas3_Tat_001_M", "MP_Christmas3_Tat_001_F", TattooData.ZoneTypes.LeftArm) },
            { 190, new TattooData("mpchristmas3_overlays", "TAT_X6_002", "MP_Christmas3_Tat_002_M", "MP_Christmas3_Tat_002_F", TattooData.ZoneTypes.RightArm) },
            { 191, new TattooData("mpchristmas3_overlays", "TAT_X6_003", "MP_Christmas3_Tat_003_M", "MP_Christmas3_Tat_003_F", TattooData.ZoneTypes.RightArm) },
            { 192, new TattooData("mpchristmas3_overlays", "TAT_X6_004", "MP_Christmas3_Tat_004_M", "MP_Christmas3_Tat_004_F", TattooData.ZoneTypes.Torso) },
            { 193, new TattooData("mpchristmas3_overlays", "TAT_X6_005", "MP_Christmas3_Tat_005_M", "MP_Christmas3_Tat_005_F", TattooData.ZoneTypes.Torso) },
            { 194, new TattooData("mpchristmas3_overlays", "TAT_X6_006", "MP_Christmas3_Tat_006_M", "MP_Christmas3_Tat_006_F", TattooData.ZoneTypes.Torso) },
            { 195, new TattooData("mpchristmas3_overlays", "TAT_X6_007", "MP_Christmas3_Tat_007_M", "MP_Christmas3_Tat_007_F", TattooData.ZoneTypes.Torso) },
            { 196, new TattooData("mpchristmas3_overlays", "TAT_X6_008", "MP_Christmas3_Tat_008_M", "MP_Christmas3_Tat_008_F", TattooData.ZoneTypes.RightLeg) },
            { 197, new TattooData("mpchristmas3_overlays", "TAT_X6_009", "MP_Christmas3_Tat_009_M", "MP_Christmas3_Tat_009_F", TattooData.ZoneTypes.LeftLeg) },
            { 198, new TattooData("mpchristmas3_overlays", "TAT_X6_010", "MP_Christmas3_Tat_010_M", "MP_Christmas3_Tat_010_F", TattooData.ZoneTypes.Head) },
            { 199, new TattooData("mpchristmas3_overlays", "TAT_X6_011", "MP_Christmas3_Tat_011_M", "MP_Christmas3_Tat_011_F", TattooData.ZoneTypes.Head) },
            { 200, new TattooData("mpchristmas3_overlays", "TAT_X6_012", "MP_Christmas3_Tat_012_M", "MP_Christmas3_Tat_012_F", TattooData.ZoneTypes.Head) },
            { 201, new TattooData("mpchristmas3_overlays", "TAT_X6_013", "MP_Christmas3_Tat_013_M", "MP_Christmas3_Tat_013_F", TattooData.ZoneTypes.Head) },
            { 202, new TattooData("mpchristmas3_overlays", "TAT_X6_014", "MP_Christmas3_Tat_014_M", "MP_Christmas3_Tat_014_F", TattooData.ZoneTypes.Torso) },
            { 203, new TattooData("mpchristmas3_overlays", "TAT_X6_015", "MP_Christmas3_Tat_015_M", "MP_Christmas3_Tat_015_F", TattooData.ZoneTypes.Torso) },
            { 204, new TattooData("mpchristmas3_overlays", "TAT_X6_016", "MP_Christmas3_Tat_016_M", "MP_Christmas3_Tat_016_F", TattooData.ZoneTypes.Torso) },
            { 205, new TattooData("mpchristmas3_overlays", "TAT_X6_017", "MP_Christmas3_Tat_017_M", "MP_Christmas3_Tat_017_F", TattooData.ZoneTypes.Torso) },
            { 206, new TattooData("mpchristmas3_overlays", "TAT_X6_018", "MP_Christmas3_Tat_018_M", "MP_Christmas3_Tat_018_F", TattooData.ZoneTypes.Torso) },
            { 207, new TattooData("mpchristmas3_overlays", "TAT_X6_019", "MP_Christmas3_Tat_019_M", "MP_Christmas3_Tat_019_F", TattooData.ZoneTypes.Torso) },
            { 208, new TattooData("mpchristmas3_overlays", "TAT_X6_020", "MP_Christmas3_Tat_020_M", "MP_Christmas3_Tat_020_F", TattooData.ZoneTypes.Torso) },
            { 209, new TattooData("mpchristmas3_overlays", "TAT_X6_021", "MP_Christmas3_Tat_021_M", "MP_Christmas3_Tat_021_F", TattooData.ZoneTypes.Torso) },
            { 210, new TattooData("mpchristmas3_overlays", "TAT_X6_022", "MP_Christmas3_Tat_022_M", "MP_Christmas3_Tat_022_F", TattooData.ZoneTypes.Torso) },
            { 211, new TattooData("mpchristmas3_overlays", "TAT_X6_023", "MP_Christmas3_Tat_023_M", "MP_Christmas3_Tat_023_F", TattooData.ZoneTypes.Torso) },
            { 212, new TattooData("mpchristmas3_overlays", "TAT_X6_024", "MP_Christmas3_Tat_024_M", "MP_Christmas3_Tat_024_F", TattooData.ZoneTypes.Torso) },
            { 213, new TattooData("mpchristmas3_overlays", "TAT_X6_025", "MP_Christmas3_Tat_025_M", "MP_Christmas3_Tat_025_F", TattooData.ZoneTypes.Torso) },
            { 214, new TattooData("mpchristmas3_overlays", "TAT_X6_026", "MP_Christmas3_Tat_026_M", "MP_Christmas3_Tat_026_F", TattooData.ZoneTypes.LeftArm) },
            { 215, new TattooData("mpchristmas3_overlays", "TAT_X6_027", "MP_Christmas3_Tat_027_M", "MP_Christmas3_Tat_027_F", TattooData.ZoneTypes.RightArm) },
            { 216, new TattooData("mpchristmas3_overlays", "TAT_X6_028", "MP_Christmas3_Tat_028_M", "MP_Christmas3_Tat_028_F", TattooData.ZoneTypes.LeftArm) },
            { 217, new TattooData("mpchristmas3_overlays", "TAT_X6_029", "MP_Christmas3_Tat_029_M", "MP_Christmas3_Tat_029_F", TattooData.ZoneTypes.RightArm) },
            { 218, new TattooData("mpchristmas3_overlays", "TAT_X6_030", "MP_Christmas3_Tat_030_M", "MP_Christmas3_Tat_030_F", TattooData.ZoneTypes.LeftArm) },
            { 219, new TattooData("mpchristmas3_overlays", "TAT_X6_031", "MP_Christmas3_Tat_031_M", "MP_Christmas3_Tat_031_F", TattooData.ZoneTypes.RightArm) },
            { 220, new TattooData("mpchristmas3_overlays", "TAT_X6_032", "MP_Christmas3_Tat_032_M", "MP_Christmas3_Tat_032_F", TattooData.ZoneTypes.RightArm) },
            { 221, new TattooData("mpchristmas3_overlays", "TAT_X6_033", "MP_Christmas3_Tat_033_M", "MP_Christmas3_Tat_033_F", TattooData.ZoneTypes.LeftArm) },
            { 222, new TattooData("mpchristmas3_overlays", "TAT_X6_034", "MP_Christmas3_Tat_034_M", "MP_Christmas3_Tat_034_F", TattooData.ZoneTypes.LeftLeg) },
            { 223, new TattooData("mpchristmas3_overlays", "TAT_X6_035", "MP_Christmas3_Tat_035_M", "MP_Christmas3_Tat_035_F", TattooData.ZoneTypes.RightLeg) },
            { 224, new TattooData("mpchristmas3_overlays", "TAT_X6_036", "MP_Christmas3_Tat_036_M", "MP_Christmas3_Tat_036_F", TattooData.ZoneTypes.RightLeg) },
            { 225, new TattooData("mpchristmas3_overlays", "TAT_X6_037", "MP_Christmas3_Tat_037_M", "MP_Christmas3_Tat_037_F", TattooData.ZoneTypes.LeftLeg) },
            { 226, new TattooData("mpchristmas3_overlays", "TAT_X6_038", "MP_Christmas3_Tat_038_M", "MP_Christmas3_Tat_038_F", TattooData.ZoneTypes.LeftLeg) },
            { 227, new TattooData("mpchristmas3_overlays", "TAT_X6_039", "MP_Christmas3_Tat_039_M", "MP_Christmas3_Tat_039_F", TattooData.ZoneTypes.RightLeg) },
            { 228, new TattooData("mpchristmas3_overlays", "TAT_X6_040", "MP_Christmas3_Tat_040_M", "MP_Christmas3_Tat_040_F", TattooData.ZoneTypes.Torso) },
            { 229, new TattooData("mpchristmas3_overlays", "TAT_X6_041", "MP_Christmas3_Tat_041_M", "MP_Christmas3_Tat_041_F", TattooData.ZoneTypes.Torso) },
            { 230, new TattooData("mpchristmas3_overlays", "TAT_X6_042", "MP_Christmas3_Tat_042_M", "MP_Christmas3_Tat_042_F", TattooData.ZoneTypes.Torso) },
            { 231, new TattooData("mpchristmas3_overlays", "TAT_X6_043", "MP_Christmas3_Tat_043_M", "MP_Christmas3_Tat_043_F", TattooData.ZoneTypes.Torso) },
            { 232, new TattooData("mpchristmas3_overlays", "TAT_X6_044", "MP_Christmas3_Tat_044_M", "MP_Christmas3_Tat_044_F", TattooData.ZoneTypes.Torso) },
            { 233, new TattooData("mpchristmas3_overlays", "TAT_X6_045", "MP_Christmas3_Tat_045_M", "MP_Christmas3_Tat_045_F", TattooData.ZoneTypes.Torso) },
            { 234, new TattooData("mpchristmas3_overlays", "TAT_X6_046", "MP_Christmas3_Tat_046_M", "MP_Christmas3_Tat_046_F", TattooData.ZoneTypes.Torso) },
            { 235, new TattooData("mpchristmas3_overlays", "TAT_X6_047", "MP_Christmas3_Tat_047_M", "MP_Christmas3_Tat_047_F", TattooData.ZoneTypes.Torso) },
            { 236, new TattooData("mpchristmas3_overlays", "TAT_X6_048", "MP_Christmas3_Tat_048_M", "MP_Christmas3_Tat_048_F", TattooData.ZoneTypes.Torso) },
            { 237, new TattooData("mpchristmas3_overlays", "TAT_X6_049", "MP_Christmas3_Tat_049_M", "MP_Christmas3_Tat_049_F", TattooData.ZoneTypes.Torso) },
            { 238, new TattooData("mpchristmas3_overlays", "TAT_X6_050", "MP_Christmas3_Tat_050_M", "MP_Christmas3_Tat_050_F", TattooData.ZoneTypes.Torso) },
            { 239, new TattooData("mpchristmas3_overlays", "TAT_X6_051", "MP_Christmas3_Tat_051_M", "MP_Christmas3_Tat_051_F", TattooData.ZoneTypes.Torso) },

            { 240, new TattooData("mpgunrunning_overlays", "TAT_GR_000", "MP_Gunrunning_Tattoo_000_M", "MP_Gunrunning_Tattoo_000_F", TattooData.ZoneTypes.Torso) },
            { 241, new TattooData("mpgunrunning_overlays", "TAT_GR_001", "MP_Gunrunning_Tattoo_001_M", "MP_Gunrunning_Tattoo_001_F", TattooData.ZoneTypes.Torso) },
            { 242, new TattooData("mpgunrunning_overlays", "TAT_GR_002", "MP_Gunrunning_Tattoo_002_M", "MP_Gunrunning_Tattoo_002_F", TattooData.ZoneTypes.RightArm) },
            { 243, new TattooData("mpgunrunning_overlays", "TAT_GR_003", "MP_Gunrunning_Tattoo_003_M", "MP_Gunrunning_Tattoo_003_F", TattooData.ZoneTypes.Head) },
            { 244, new TattooData("mpgunrunning_overlays", "TAT_GR_004", "MP_Gunrunning_Tattoo_004_M", "MP_Gunrunning_Tattoo_004_F", TattooData.ZoneTypes.LeftArm) },
            { 245, new TattooData("mpgunrunning_overlays", "TAT_GR_005", "MP_Gunrunning_Tattoo_005_M", "MP_Gunrunning_Tattoo_005_F", TattooData.ZoneTypes.LeftLeg) },
            { 246, new TattooData("mpgunrunning_overlays", "TAT_GR_006", "MP_Gunrunning_Tattoo_006_M", "MP_Gunrunning_Tattoo_006_F", TattooData.ZoneTypes.RightLeg) },
            { 247, new TattooData("mpgunrunning_overlays", "TAT_GR_007", "MP_Gunrunning_Tattoo_007_M", "MP_Gunrunning_Tattoo_007_F", TattooData.ZoneTypes.LeftLeg) },
            { 248, new TattooData("mpgunrunning_overlays", "TAT_GR_008", "MP_Gunrunning_Tattoo_008_M", "MP_Gunrunning_Tattoo_008_F", TattooData.ZoneTypes.LeftArm) },
            { 249, new TattooData("mpgunrunning_overlays", "TAT_GR_009", "MP_Gunrunning_Tattoo_009_M", "MP_Gunrunning_Tattoo_009_F", TattooData.ZoneTypes.Torso) },
            { 250, new TattooData("mpgunrunning_overlays", "TAT_GR_010", "MP_Gunrunning_Tattoo_010_M", "MP_Gunrunning_Tattoo_010_F", TattooData.ZoneTypes.Torso) },
            { 251, new TattooData("mpgunrunning_overlays", "TAT_GR_011", "MP_Gunrunning_Tattoo_011_M", "MP_Gunrunning_Tattoo_011_F", TattooData.ZoneTypes.LeftLeg) },
            { 252, new TattooData("mpgunrunning_overlays", "TAT_GR_012", "MP_Gunrunning_Tattoo_012_M", "MP_Gunrunning_Tattoo_012_F", TattooData.ZoneTypes.Torso) },
            { 253, new TattooData("mpgunrunning_overlays", "TAT_GR_013", "MP_Gunrunning_Tattoo_013_M", "MP_Gunrunning_Tattoo_013_F", TattooData.ZoneTypes.Torso) },
            { 254, new TattooData("mpgunrunning_overlays", "TAT_GR_014", "MP_Gunrunning_Tattoo_014_M", "MP_Gunrunning_Tattoo_014_F", TattooData.ZoneTypes.Torso) },
            { 255, new TattooData("mpgunrunning_overlays", "TAT_GR_015", "MP_Gunrunning_Tattoo_015_M", "MP_Gunrunning_Tattoo_015_F", TattooData.ZoneTypes.LeftArm) },
            { 256, new TattooData("mpgunrunning_overlays", "TAT_GR_016", "MP_Gunrunning_Tattoo_016_M", "MP_Gunrunning_Tattoo_016_F", TattooData.ZoneTypes.LeftArm) },
            { 257, new TattooData("mpgunrunning_overlays", "TAT_GR_017", "MP_Gunrunning_Tattoo_017_M", "MP_Gunrunning_Tattoo_017_F", TattooData.ZoneTypes.Torso) },
            { 258, new TattooData("mpgunrunning_overlays", "TAT_GR_018", "MP_Gunrunning_Tattoo_018_M", "MP_Gunrunning_Tattoo_018_F", TattooData.ZoneTypes.Torso) },
            { 259, new TattooData("mpgunrunning_overlays", "TAT_GR_019", "MP_Gunrunning_Tattoo_019_M", "MP_Gunrunning_Tattoo_019_F", TattooData.ZoneTypes.Torso) },
            { 260, new TattooData("mpgunrunning_overlays", "TAT_GR_020", "MP_Gunrunning_Tattoo_020_M", "MP_Gunrunning_Tattoo_020_F", TattooData.ZoneTypes.Torso) },
            { 261, new TattooData("mpgunrunning_overlays", "TAT_GR_021", "MP_Gunrunning_Tattoo_021_M", "MP_Gunrunning_Tattoo_021_F", TattooData.ZoneTypes.RightArm) },
            { 262, new TattooData("mpgunrunning_overlays", "TAT_GR_022", "MP_Gunrunning_Tattoo_022_M", "MP_Gunrunning_Tattoo_022_F", TattooData.ZoneTypes.Torso) },
            { 263, new TattooData("mpgunrunning_overlays", "TAT_GR_023", "MP_Gunrunning_Tattoo_023_M", "MP_Gunrunning_Tattoo_023_F", TattooData.ZoneTypes.LeftLeg) },
            { 264, new TattooData("mpgunrunning_overlays", "TAT_GR_024", "MP_Gunrunning_Tattoo_024_M", "MP_Gunrunning_Tattoo_024_F", TattooData.ZoneTypes.RightArm) },
            { 265, new TattooData("mpgunrunning_overlays", "TAT_GR_025", "MP_Gunrunning_Tattoo_025_M", "MP_Gunrunning_Tattoo_025_F", TattooData.ZoneTypes.LeftArm) },
            { 266, new TattooData("mpgunrunning_overlays", "TAT_GR_026", "MP_Gunrunning_Tattoo_026_M", "MP_Gunrunning_Tattoo_026_F", TattooData.ZoneTypes.RightLeg) },
            { 267, new TattooData("mpgunrunning_overlays", "TAT_GR_027", "MP_Gunrunning_Tattoo_027_M", "MP_Gunrunning_Tattoo_027_F", TattooData.ZoneTypes.LeftArm) },
            { 268, new TattooData("mpgunrunning_overlays", "TAT_GR_028", "MP_Gunrunning_Tattoo_028_M", "MP_Gunrunning_Tattoo_028_F", TattooData.ZoneTypes.Torso) },
            { 269, new TattooData("mpgunrunning_overlays", "TAT_GR_029", "MP_Gunrunning_Tattoo_029_M", "MP_Gunrunning_Tattoo_029_F", TattooData.ZoneTypes.Torso) },
            { 270, new TattooData("mpgunrunning_overlays", "TAT_GR_030", "MP_Gunrunning_Tattoo_030_M", "MP_Gunrunning_Tattoo_030_F", TattooData.ZoneTypes.RightLeg) },

            { 271, new TattooData("mpheist3_overlays", "TAT_H3_000", "mpHeist3_Tat_000_M", "mpHeist3_Tat_000_F", TattooData.ZoneTypes.Head) },
            { 272, new TattooData("mpheist3_overlays", "TAT_H3_001", "mpHeist3_Tat_001_M", "mpHeist3_Tat_001_F", TattooData.ZoneTypes.Head) },
            { 273, new TattooData("mpheist3_overlays", "TAT_H3_002", "mpHeist3_Tat_002_M", "mpHeist3_Tat_002_F", TattooData.ZoneTypes.Head) },
            { 274, new TattooData("mpheist3_overlays", "TAT_H3_003", "mpHeist3_Tat_003_M", "mpHeist3_Tat_003_F", TattooData.ZoneTypes.Head) },
            { 275, new TattooData("mpheist3_overlays", "TAT_H3_004", "mpHeist3_Tat_004_M", "mpHeist3_Tat_004_F", TattooData.ZoneTypes.Head) },
            { 276, new TattooData("mpheist3_overlays", "TAT_H3_005", "mpHeist3_Tat_005_M", "mpHeist3_Tat_005_F", TattooData.ZoneTypes.Head) },
            { 277, new TattooData("mpheist3_overlays", "TAT_H3_006", "mpHeist3_Tat_006_M", "mpHeist3_Tat_006_F", TattooData.ZoneTypes.Head) },
            { 278, new TattooData("mpheist3_overlays", "TAT_H3_007", "mpHeist3_Tat_007_M", "mpHeist3_Tat_007_F", TattooData.ZoneTypes.Head) },
            { 279, new TattooData("mpheist3_overlays", "TAT_H3_008", "mpHeist3_Tat_008_M", "mpHeist3_Tat_008_F", TattooData.ZoneTypes.Head) },
            { 280, new TattooData("mpheist3_overlays", "TAT_H3_009", "mpHeist3_Tat_009_M", "mpHeist3_Tat_009_F", TattooData.ZoneTypes.Head) },
            { 281, new TattooData("mpheist3_overlays", "TAT_H3_010", "mpHeist3_Tat_010_M", "mpHeist3_Tat_010_F", TattooData.ZoneTypes.Head) },
            { 282, new TattooData("mpheist3_overlays", "TAT_H3_011", "mpHeist3_Tat_011_M", "mpHeist3_Tat_011_F", TattooData.ZoneTypes.Head) },
            { 283, new TattooData("mpheist3_overlays", "TAT_H3_012", "mpHeist3_Tat_012_M", "mpHeist3_Tat_012_F", TattooData.ZoneTypes.Head) },
            { 284, new TattooData("mpheist3_overlays", "TAT_H3_013", "mpHeist3_Tat_013_M", "mpHeist3_Tat_013_F", TattooData.ZoneTypes.Head) },
            { 285, new TattooData("mpheist3_overlays", "TAT_H3_014", "mpHeist3_Tat_014_M", "mpHeist3_Tat_014_F", TattooData.ZoneTypes.Head) },
            { 286, new TattooData("mpheist3_overlays", "TAT_H3_015", "mpHeist3_Tat_015_M", "mpHeist3_Tat_015_F", TattooData.ZoneTypes.Head) },
            { 287, new TattooData("mpheist3_overlays", "TAT_H3_016", "mpHeist3_Tat_016_M", "mpHeist3_Tat_016_F", TattooData.ZoneTypes.Head) },
            { 288, new TattooData("mpheist3_overlays", "TAT_H3_017", "mpHeist3_Tat_017_M", "mpHeist3_Tat_017_F", TattooData.ZoneTypes.Head) },
            { 289, new TattooData("mpheist3_overlays", "TAT_H3_018", "mpHeist3_Tat_018_M", "mpHeist3_Tat_018_F", TattooData.ZoneTypes.Head) },
            { 290, new TattooData("mpheist3_overlays", "TAT_H3_019", "mpHeist3_Tat_019_M", "mpHeist3_Tat_019_F", TattooData.ZoneTypes.Head) },
            { 291, new TattooData("mpheist3_overlays", "TAT_H3_020", "mpHeist3_Tat_020_M", "mpHeist3_Tat_020_F", TattooData.ZoneTypes.Head) },
            { 292, new TattooData("mpheist3_overlays", "TAT_H3_021", "mpHeist3_Tat_021_M", "mpHeist3_Tat_021_F", TattooData.ZoneTypes.Head) },
            { 293, new TattooData("mpheist3_overlays", "TAT_H3_022", "mpHeist3_Tat_022_M", "mpHeist3_Tat_022_F", TattooData.ZoneTypes.Head) },
            { 294, new TattooData("mpheist3_overlays", "TAT_H3_023", "mpHeist3_Tat_023_M", "mpHeist3_Tat_023_F", TattooData.ZoneTypes.Torso) },
            { 295, new TattooData("mpheist3_overlays", "TAT_H3_024", "mpHeist3_Tat_024_M", "mpHeist3_Tat_024_F", TattooData.ZoneTypes.Torso) },
            { 296, new TattooData("mpheist3_overlays", "TAT_H3_025", "mpHeist3_Tat_025_M", "mpHeist3_Tat_025_F", TattooData.ZoneTypes.Torso) },
            { 297, new TattooData("mpheist3_overlays", "TAT_H3_026", "mpHeist3_Tat_026_M", "mpHeist3_Tat_026_F", TattooData.ZoneTypes.Torso) },
            { 298, new TattooData("mpheist3_overlays", "TAT_H3_027", "mpHeist3_Tat_027_M", "mpHeist3_Tat_027_F", TattooData.ZoneTypes.Torso) },
            { 299, new TattooData("mpheist3_overlays", "TAT_H3_028", "mpHeist3_Tat_028_M", "mpHeist3_Tat_028_F", TattooData.ZoneTypes.Torso) },
            { 300, new TattooData("mpheist3_overlays", "TAT_H3_029", "mpHeist3_Tat_029_M", "mpHeist3_Tat_029_F", TattooData.ZoneTypes.Torso) },
            { 301, new TattooData("mpheist3_overlays", "TAT_H3_030", "mpHeist3_Tat_030_M", "mpHeist3_Tat_030_F", TattooData.ZoneTypes.Torso) },
            { 302, new TattooData("mpheist3_overlays", "TAT_H3_031", "mpHeist3_Tat_031_M", "mpHeist3_Tat_031_F", TattooData.ZoneTypes.RightLeg) },
            { 303, new TattooData("mpheist3_overlays", "TAT_H3_032", "mpHeist3_Tat_032_M", "mpHeist3_Tat_032_F", TattooData.ZoneTypes.LeftLeg) },
            { 304, new TattooData("mpheist3_overlays", "TAT_H3_033", "mpHeist3_Tat_033_M", "mpHeist3_Tat_033_F", TattooData.ZoneTypes.Torso) },
            { 305, new TattooData("mpheist3_overlays", "TAT_H3_034", "mpHeist3_Tat_034_M", "mpHeist3_Tat_034_F", TattooData.ZoneTypes.RightArm) },
            { 306, new TattooData("mpheist3_overlays", "TAT_H3_035", "mpHeist3_Tat_035_M", "mpHeist3_Tat_035_F", TattooData.ZoneTypes.Torso) },
            { 307, new TattooData("mpheist3_overlays", "TAT_H3_036", "mpHeist3_Tat_036_M", "mpHeist3_Tat_036_F", TattooData.ZoneTypes.Torso) },
            { 308, new TattooData("mpheist3_overlays", "TAT_H3_037", "mpHeist3_Tat_037_M", "mpHeist3_Tat_037_F", TattooData.ZoneTypes.Torso) },
            { 309, new TattooData("mpheist3_overlays", "TAT_H3_038", "mpHeist3_Tat_038_M", "mpHeist3_Tat_038_F", TattooData.ZoneTypes.Torso) },
            { 310, new TattooData("mpheist3_overlays", "TAT_H3_039", "mpHeist3_Tat_039_M", "mpHeist3_Tat_039_F", TattooData.ZoneTypes.Torso) },
            { 311, new TattooData("mpheist3_overlays", "TAT_H3_040", "mpHeist3_Tat_040_M", "mpHeist3_Tat_040_F", TattooData.ZoneTypes.LeftArm) },
            { 312, new TattooData("mpheist3_overlays", "TAT_H3_041", "mpHeist3_Tat_041_M", "mpHeist3_Tat_041_F", TattooData.ZoneTypes.LeftArm) },
            { 313, new TattooData("mpheist3_overlays", "TAT_H3_042", "mpHeist3_Tat_042_M", "mpHeist3_Tat_042_F", TattooData.ZoneTypes.Head) },
            { 314, new TattooData("mpheist3_overlays", "TAT_H3_043", "mpHeist3_Tat_043_M", "mpHeist3_Tat_043_F", TattooData.ZoneTypes.Head) },
            { 315, new TattooData("mpheist3_overlays", "TAT_H3_044", "mpHeist3_Tat_044_M", "mpHeist3_Tat_044_F", TattooData.ZoneTypes.Head) },

            { 316, new TattooData("mpheist4_overlays", "TAT_H4_000", "MP_Heist4_Tat_000_M", "MP_Heist4_Tat_000_F", TattooData.ZoneTypes.RightArm) },
            { 317, new TattooData("mpheist4_overlays", "TAT_H4_001", "MP_Heist4_Tat_001_M", "MP_Heist4_Tat_001_F", TattooData.ZoneTypes.RightArm) },
            { 318, new TattooData("mpheist4_overlays", "TAT_H4_002", "MP_Heist4_Tat_002_M", "MP_Heist4_Tat_002_F", TattooData.ZoneTypes.RightArm) },
            { 319, new TattooData("mpheist4_overlays", "TAT_H4_003", "MP_Heist4_Tat_003_M", "MP_Heist4_Tat_003_F", TattooData.ZoneTypes.RightArm) },
            { 320, new TattooData("mpheist4_overlays", "TAT_H4_004", "MP_Heist4_Tat_004_M", "MP_Heist4_Tat_004_F", TattooData.ZoneTypes.Torso) },
            { 321, new TattooData("mpheist4_overlays", "TAT_H4_005", "MP_Heist4_Tat_005_M", "MP_Heist4_Tat_005_F", TattooData.ZoneTypes.RightArm) },
            { 322, new TattooData("mpheist4_overlays", "TAT_H4_006", "MP_Heist4_Tat_006_M", "MP_Heist4_Tat_006_F", TattooData.ZoneTypes.RightArm) },
            { 323, new TattooData("mpheist4_overlays", "TAT_H4_007", "MP_Heist4_Tat_007_M", "MP_Heist4_Tat_007_F", TattooData.ZoneTypes.RightArm) },
            { 324, new TattooData("mpheist4_overlays", "TAT_H4_008", "MP_Heist4_Tat_008_M", "MP_Heist4_Tat_008_F", TattooData.ZoneTypes.RightArm) },
            { 325, new TattooData("mpheist4_overlays", "TAT_H4_009", "MP_Heist4_Tat_009_M", "MP_Heist4_Tat_009_F", TattooData.ZoneTypes.LeftArm) },
            { 326, new TattooData("mpheist4_overlays", "TAT_H4_010", "MP_Heist4_Tat_010_M", "MP_Heist4_Tat_010_F", TattooData.ZoneTypes.LeftLeg) },
            { 327, new TattooData("mpheist4_overlays", "TAT_H4_011", "MP_Heist4_Tat_011_M", "MP_Heist4_Tat_011_F", TattooData.ZoneTypes.RightArm) },
            { 328, new TattooData("mpheist4_overlays", "TAT_H4_012", "MP_Heist4_Tat_012_M", "MP_Heist4_Tat_012_F", TattooData.ZoneTypes.RightArm) },
            { 329, new TattooData("mpheist4_overlays", "TAT_H4_013", "MP_Heist4_Tat_013_M", "MP_Heist4_Tat_013_F", TattooData.ZoneTypes.Torso) },
            { 330, new TattooData("mpheist4_overlays", "TAT_H4_014", "MP_Heist4_Tat_014_M", "MP_Heist4_Tat_014_F", TattooData.ZoneTypes.Torso) },
            { 331, new TattooData("mpheist4_overlays", "TAT_H4_015", "MP_Heist4_Tat_015_M", "MP_Heist4_Tat_015_F", TattooData.ZoneTypes.Torso) },
            { 332, new TattooData("mpheist4_overlays", "TAT_H4_016", "MP_Heist4_Tat_016_M", "MP_Heist4_Tat_016_F", TattooData.ZoneTypes.Torso) },
            { 333, new TattooData("mpheist4_overlays", "TAT_H4_017", "MP_Heist4_Tat_017_M", "MP_Heist4_Tat_017_F", TattooData.ZoneTypes.Torso) },
            { 334, new TattooData("mpheist4_overlays", "TAT_H4_018", "MP_Heist4_Tat_018_M", "MP_Heist4_Tat_018_F", TattooData.ZoneTypes.Torso) },
            { 335, new TattooData("mpheist4_overlays", "TAT_H4_019", "MP_Heist4_Tat_019_M", "MP_Heist4_Tat_019_F", TattooData.ZoneTypes.Torso) },
            { 336, new TattooData("mpheist4_overlays", "TAT_H4_020", "MP_Heist4_Tat_020_M", "MP_Heist4_Tat_020_F", TattooData.ZoneTypes.Torso) },
            { 337, new TattooData("mpheist4_overlays", "TAT_H4_021", "MP_Heist4_Tat_021_M", "MP_Heist4_Tat_021_F", TattooData.ZoneTypes.Torso) },
            { 338, new TattooData("mpheist4_overlays", "TAT_H4_022", "MP_Heist4_Tat_022_M", "MP_Heist4_Tat_022_F", TattooData.ZoneTypes.Torso) },
            { 339, new TattooData("mpheist4_overlays", "TAT_H4_023", "MP_Heist4_Tat_023_M", "MP_Heist4_Tat_023_F", TattooData.ZoneTypes.Torso) },
            { 340, new TattooData("mpheist4_overlays", "TAT_H4_024", "MP_Heist4_Tat_024_M", "MP_Heist4_Tat_024_F", TattooData.ZoneTypes.LeftLeg) },
            { 341, new TattooData("mpheist4_overlays", "TAT_H4_025", "MP_Heist4_Tat_025_M", "MP_Heist4_Tat_025_F", TattooData.ZoneTypes.LeftLeg) },
            { 342, new TattooData("mpheist4_overlays", "TAT_H4_026", "MP_Heist4_Tat_026_M", "MP_Heist4_Tat_026_F", TattooData.ZoneTypes.RightArm) },
            { 343, new TattooData("mpheist4_overlays", "TAT_H4_027", "MP_Heist4_Tat_027_M", "MP_Heist4_Tat_027_F", TattooData.ZoneTypes.RightLeg) },
            { 344, new TattooData("mpheist4_overlays", "TAT_H4_028", "MP_Heist4_Tat_028_M", "MP_Heist4_Tat_028_F", TattooData.ZoneTypes.LeftLeg) },
            { 345, new TattooData("mpheist4_overlays", "TAT_H4_029", "MP_Heist4_Tat_029_M", "MP_Heist4_Tat_029_F", TattooData.ZoneTypes.LeftLeg) },
            { 346, new TattooData("mpheist4_overlays", "TAT_H4_030", "MP_Heist4_Tat_030_M", "MP_Heist4_Tat_030_F", TattooData.ZoneTypes.Torso) },
            { 347, new TattooData("mpheist4_overlays", "TAT_H4_031", "MP_Heist4_Tat_031_M", "MP_Heist4_Tat_031_F", TattooData.ZoneTypes.RightArm) },
            { 348, new TattooData("mpheist4_overlays", "TAT_H4_032", "MP_Heist4_Tat_032_M", "MP_Heist4_Tat_032_F", TattooData.ZoneTypes.RightArm) },

            { 349, new TattooData("mphipster_overlays", "TAT_HP_000", "FM_Hip_M_Tat_000", "FM_Hip_F_Tat_000", TattooData.ZoneTypes.Torso) },
            { 350, new TattooData("mphipster_overlays", "TAT_HP_001", "FM_Hip_M_Tat_001", "FM_Hip_F_Tat_001", TattooData.ZoneTypes.RightArm) },
            { 351, new TattooData("mphipster_overlays", "TAT_HP_002", "FM_Hip_M_Tat_002", "FM_Hip_F_Tat_002", TattooData.ZoneTypes.Torso) },
            { 352, new TattooData("mphipster_overlays", "TAT_HP_003", "FM_Hip_M_Tat_003", "FM_Hip_F_Tat_003", TattooData.ZoneTypes.LeftArm) },
            { 353, new TattooData("mphipster_overlays", "TAT_HP_004", "FM_Hip_M_Tat_004", "FM_Hip_F_Tat_004", TattooData.ZoneTypes.RightArm) },
            { 354, new TattooData("mphipster_overlays", "TAT_HP_005", "FM_Hip_M_Tat_005", "FM_Hip_F_Tat_005", TattooData.ZoneTypes.Head) },
            { 355, new TattooData("mphipster_overlays", "TAT_HP_006", "FM_Hip_M_Tat_006", "FM_Hip_F_Tat_006", TattooData.ZoneTypes.Torso) },
            { 356, new TattooData("mphipster_overlays", "TAT_HP_007", "FM_Hip_M_Tat_007", "FM_Hip_F_Tat_007", TattooData.ZoneTypes.LeftArm) },
            { 357, new TattooData("mphipster_overlays", "TAT_HP_008", "FM_Hip_M_Tat_008", "FM_Hip_F_Tat_008", TattooData.ZoneTypes.RightArm) },
            { 358, new TattooData("mphipster_overlays", "TAT_HP_009", "FM_Hip_M_Tat_009", "FM_Hip_F_Tat_009", TattooData.ZoneTypes.LeftLeg) },
            { 359, new TattooData("mphipster_overlays", "TAT_HP_010", "FM_Hip_M_Tat_010", "FM_Hip_F_Tat_010", TattooData.ZoneTypes.RightArm) },
            { 360, new TattooData("mphipster_overlays", "TAT_HP_011", "FM_Hip_M_Tat_011", "FM_Hip_F_Tat_011", TattooData.ZoneTypes.Torso) },
            { 361, new TattooData("mphipster_overlays", "TAT_HP_012", "FM_Hip_M_Tat_012", "FM_Hip_F_Tat_012", TattooData.ZoneTypes.Torso) },
            { 362, new TattooData("mphipster_overlays", "TAT_HP_013", "FM_Hip_M_Tat_013", "FM_Hip_F_Tat_013", TattooData.ZoneTypes.Torso) },
            { 363, new TattooData("mphipster_overlays", "TAT_HP_014", "FM_Hip_M_Tat_014", "FM_Hip_F_Tat_014", TattooData.ZoneTypes.RightArm) },
            { 364, new TattooData("mphipster_overlays", "TAT_HP_015", "FM_Hip_M_Tat_015", "FM_Hip_F_Tat_015", TattooData.ZoneTypes.LeftArm) },
            { 365, new TattooData("mphipster_overlays", "TAT_HP_016", "FM_Hip_M_Tat_016", "FM_Hip_F_Tat_016", TattooData.ZoneTypes.LeftArm) },
            { 366, new TattooData("mphipster_overlays", "TAT_HP_017", "FM_Hip_M_Tat_017", "FM_Hip_F_Tat_017", TattooData.ZoneTypes.RightArm) },
            { 367, new TattooData("mphipster_overlays", "TAT_HP_018", "FM_Hip_M_Tat_018", "FM_Hip_F_Tat_018", TattooData.ZoneTypes.RightArm) },
            { 368, new TattooData("mphipster_overlays", "TAT_HP_019", "FM_Hip_M_Tat_019", "FM_Hip_F_Tat_019", TattooData.ZoneTypes.LeftLeg) },
            { 369, new TattooData("mphipster_overlays", "TAT_HP_020", "FM_Hip_M_Tat_020", "FM_Hip_F_Tat_020", TattooData.ZoneTypes.RightArm) },
            { 370, new TattooData("mphipster_overlays", "TAT_HP_021", "FM_Hip_M_Tat_021", "FM_Hip_F_Tat_021", TattooData.ZoneTypes.Head) },
            { 371, new TattooData("mphipster_overlays", "TAT_HP_022", "FM_Hip_M_Tat_022", "FM_Hip_F_Tat_022", TattooData.ZoneTypes.RightArm) },
            { 372, new TattooData("mphipster_overlays", "TAT_HP_023", "FM_Hip_M_Tat_023", "FM_Hip_F_Tat_023", TattooData.ZoneTypes.RightArm) },
            { 373, new TattooData("mphipster_overlays", "TAT_HP_024", "FM_Hip_M_Tat_024", "FM_Hip_F_Tat_024", TattooData.ZoneTypes.Torso) },
            { 374, new TattooData("mphipster_overlays", "TAT_HP_025", "FM_Hip_M_Tat_025", "FM_Hip_F_Tat_025", TattooData.ZoneTypes.Torso) },
            { 375, new TattooData("mphipster_overlays", "TAT_HP_026", "FM_Hip_M_Tat_026", "FM_Hip_F_Tat_026", TattooData.ZoneTypes.LeftArm) },
            { 376, new TattooData("mphipster_overlays", "TAT_HP_027", "FM_Hip_M_Tat_027", "FM_Hip_F_Tat_027", TattooData.ZoneTypes.LeftArm) },
            { 377, new TattooData("mphipster_overlays", "TAT_HP_028", "FM_Hip_M_Tat_028", "FM_Hip_F_Tat_028", TattooData.ZoneTypes.LeftArm) },
            { 378, new TattooData("mphipster_overlays", "TAT_HP_029", "FM_Hip_M_Tat_029", "FM_Hip_F_Tat_029", TattooData.ZoneTypes.Torso) },
            { 379, new TattooData("mphipster_overlays", "TAT_HP_030", "FM_Hip_M_Tat_030", "FM_Hip_F_Tat_030", TattooData.ZoneTypes.Torso) },
            { 380, new TattooData("mphipster_overlays", "TAT_HP_031", "FM_Hip_M_Tat_031", "FM_Hip_F_Tat_031", TattooData.ZoneTypes.Torso) },
            { 381, new TattooData("mphipster_overlays", "TAT_HP_032", "FM_Hip_M_Tat_032", "FM_Hip_F_Tat_032", TattooData.ZoneTypes.Torso) },
            { 382, new TattooData("mphipster_overlays", "TAT_HP_033", "FM_Hip_M_Tat_033", "FM_Hip_F_Tat_033", TattooData.ZoneTypes.Torso) },
            { 383, new TattooData("mphipster_overlays", "TAT_HP_034", "FM_Hip_M_Tat_034", "FM_Hip_F_Tat_034", TattooData.ZoneTypes.LeftArm) },
            { 384, new TattooData("mphipster_overlays", "TAT_HP_035", "FM_Hip_M_Tat_035", "FM_Hip_F_Tat_035", TattooData.ZoneTypes.Torso) },
            { 385, new TattooData("mphipster_overlays", "TAT_HP_036", "FM_Hip_M_Tat_036", "FM_Hip_F_Tat_036", TattooData.ZoneTypes.RightArm) },
            { 386, new TattooData("mphipster_overlays", "TAT_HP_037", "FM_Hip_M_Tat_037", "FM_Hip_F_Tat_037", TattooData.ZoneTypes.LeftArm) },
            { 387, new TattooData("mphipster_overlays", "TAT_HP_038", "FM_Hip_M_Tat_038", "FM_Hip_F_Tat_038", TattooData.ZoneTypes.RightLeg) },
            { 388, new TattooData("mphipster_overlays", "TAT_HP_039", "FM_Hip_M_Tat_039", "FM_Hip_F_Tat_039", TattooData.ZoneTypes.LeftArm) },
            { 389, new TattooData("mphipster_overlays", "TAT_HP_040", "FM_Hip_M_Tat_040", "FM_Hip_F_Tat_040", TattooData.ZoneTypes.LeftLeg) },
            { 390, new TattooData("mphipster_overlays", "TAT_HP_041", "FM_Hip_M_Tat_041", "FM_Hip_F_Tat_041", TattooData.ZoneTypes.Torso) },
            { 391, new TattooData("mphipster_overlays", "TAT_HP_042", "FM_Hip_M_Tat_042", "FM_Hip_F_Tat_042", TattooData.ZoneTypes.RightLeg) },
            { 392, new TattooData("mphipster_overlays", "TAT_HP_043", "FM_Hip_M_Tat_043", "FM_Hip_F_Tat_043", TattooData.ZoneTypes.LeftArm) },
            { 393, new TattooData("mphipster_overlays", "TAT_HP_044", "FM_Hip_M_Tat_044", "FM_Hip_F_Tat_044", TattooData.ZoneTypes.RightArm) },
            { 394, new TattooData("mphipster_overlays", "TAT_HP_045", "FM_Hip_M_Tat_045", "FM_Hip_F_Tat_045", TattooData.ZoneTypes.RightArm) },
            { 395, new TattooData("mphipster_overlays", "TAT_HP_046", "FM_Hip_M_Tat_046", "FM_Hip_F_Tat_046", TattooData.ZoneTypes.Torso) },
            { 396, new TattooData("mphipster_overlays", "TAT_HP_047", "FM_Hip_M_Tat_047", "FM_Hip_F_Tat_047", TattooData.ZoneTypes.Torso) },
            { 397, new TattooData("mphipster_overlays", "TAT_HP_048", "FM_Hip_M_Tat_048", "FM_Hip_F_Tat_048", TattooData.ZoneTypes.LeftArm) },

            { 398, new TattooData("mpimportexport_overlays", "TAT_IE_000", "MP_MP_ImportExport_Tat_000_M", "MP_MP_ImportExport_Tat_000_F", TattooData.ZoneTypes.Torso) },
            { 399, new TattooData("mpimportexport_overlays", "TAT_IE_001", "MP_MP_ImportExport_Tat_001_M", "MP_MP_ImportExport_Tat_001_F", TattooData.ZoneTypes.Torso) },
            { 400, new TattooData("mpimportexport_overlays", "TAT_IE_002", "MP_MP_ImportExport_Tat_002_M", "MP_MP_ImportExport_Tat_002_F", TattooData.ZoneTypes.Torso) },
            { 401, new TattooData("mpimportexport_overlays", "TAT_IE_003", "MP_MP_ImportExport_Tat_003_M", "MP_MP_ImportExport_Tat_003_F", TattooData.ZoneTypes.RightArm) },
            { 402, new TattooData("mpimportexport_overlays", "TAT_IE_004", "MP_MP_ImportExport_Tat_004_M", "MP_MP_ImportExport_Tat_004_F", TattooData.ZoneTypes.LeftArm) },
            { 403, new TattooData("mpimportexport_overlays", "TAT_IE_005", "MP_MP_ImportExport_Tat_005_M", "MP_MP_ImportExport_Tat_005_F", TattooData.ZoneTypes.RightArm) },
            { 404, new TattooData("mpimportexport_overlays", "TAT_IE_006", "MP_MP_ImportExport_Tat_006_M", "MP_MP_ImportExport_Tat_006_F", TattooData.ZoneTypes.RightArm) },
            { 405, new TattooData("mpimportexport_overlays", "TAT_IE_007", "MP_MP_ImportExport_Tat_007_M", "MP_MP_ImportExport_Tat_007_F", TattooData.ZoneTypes.RightArm) },
            { 406, new TattooData("mpimportexport_overlays", "TAT_IE_008", "MP_MP_ImportExport_Tat_008_M", "MP_MP_ImportExport_Tat_008_F", TattooData.ZoneTypes.LeftArm) },
            { 407, new TattooData("mpimportexport_overlays", "TAT_IE_009", "MP_MP_ImportExport_Tat_009_M", "MP_MP_ImportExport_Tat_009_F", TattooData.ZoneTypes.Torso) },
            { 408, new TattooData("mpimportexport_overlays", "TAT_IE_010", "MP_MP_ImportExport_Tat_010_M", "MP_MP_ImportExport_Tat_010_F", TattooData.ZoneTypes.Torso) },
            { 409, new TattooData("mpimportexport_overlays", "TAT_IE_011", "MP_MP_ImportExport_Tat_011_M", "MP_MP_ImportExport_Tat_011_F", TattooData.ZoneTypes.Torso) },

            { 410, new TattooData("mplowrider2_overlays", "TAT_S2_000", "MP_LR_Tat_000_M", "MP_LR_Tat_000_F", TattooData.ZoneTypes.Torso) },
            { 411, new TattooData("mplowrider2_overlays", "TAT_S2_003", "MP_LR_Tat_003_M", "MP_LR_Tat_003_F", TattooData.ZoneTypes.RightArm) },
            { 412, new TattooData("mplowrider2_overlays", "TAT_S2_006", "MP_LR_Tat_006_M", "MP_LR_Tat_006_F", TattooData.ZoneTypes.LeftArm) },
            { 413, new TattooData("mplowrider2_overlays", "TAT_S2_008", "MP_LR_Tat_008_M", "MP_LR_Tat_008_F", TattooData.ZoneTypes.Torso) },
            { 414, new TattooData("mplowrider2_overlays", "TAT_S2_011", "MP_LR_Tat_011_M", "MP_LR_Tat_011_F", TattooData.ZoneTypes.Torso) },
            { 415, new TattooData("mplowrider2_overlays", "TAT_S2_012", "MP_LR_Tat_012_M", "MP_LR_Tat_012_F", TattooData.ZoneTypes.Torso) },
            { 416, new TattooData("mplowrider2_overlays", "TAT_S2_016", "MP_LR_Tat_016_M", "MP_LR_Tat_016_F", TattooData.ZoneTypes.Torso) },
            { 417, new TattooData("mplowrider2_overlays", "TAT_S2_018", "MP_LR_Tat_018_M", "MP_LR_Tat_018_F", TattooData.ZoneTypes.LeftArm) },
            { 418, new TattooData("mplowrider2_overlays", "TAT_S2_019", "MP_LR_Tat_019_M", "MP_LR_Tat_019_F", TattooData.ZoneTypes.Torso) },
            { 419, new TattooData("mplowrider2_overlays", "TAT_S2_022", "MP_LR_Tat_022_M", "MP_LR_Tat_022_F", TattooData.ZoneTypes.LeftArm) },
            { 420, new TattooData("mplowrider2_overlays", "TAT_S2_028", "MP_LR_Tat_028_M", "MP_LR_Tat_028_F", TattooData.ZoneTypes.RightArm) },
            { 421, new TattooData("mplowrider2_overlays", "TAT_S2_029", "MP_LR_Tat_029_M", "MP_LR_Tat_029_F", TattooData.ZoneTypes.LeftLeg) },
            { 422, new TattooData("mplowrider2_overlays", "TAT_S2_030", "MP_LR_Tat_030_M", "MP_LR_Tat_030_F", TattooData.ZoneTypes.RightLeg) },
            { 423, new TattooData("mplowrider2_overlays", "TAT_S2_031", "MP_LR_Tat_031_M", "MP_LR_Tat_031_F", TattooData.ZoneTypes.Torso) },
            { 424, new TattooData("mplowrider2_overlays", "TAT_S2_032", "MP_LR_Tat_032_M", "MP_LR_Tat_032_F", TattooData.ZoneTypes.Torso) },
            { 425, new TattooData("mplowrider2_overlays", "TAT_S2_035", "MP_LR_Tat_035_M", "MP_LR_Tat_035_F", TattooData.ZoneTypes.RightArm) },

            { 426, new TattooData("mplowrider_overlays", "TAT_S1_001", "MP_LR_Tat_001_M", "MP_LR_Tat_001_F", TattooData.ZoneTypes.Torso) },
            { 427, new TattooData("mplowrider_overlays", "TAT_S1_002", "MP_LR_Tat_002_M", "MP_LR_Tat_002_F", TattooData.ZoneTypes.Torso) },
            { 428, new TattooData("mplowrider_overlays", "TAT_S1_004", "MP_LR_Tat_004_M", "MP_LR_Tat_004_F", TattooData.ZoneTypes.Torso) },
            { 429, new TattooData("mplowrider_overlays", "TAT_S1_005", "MP_LR_Tat_005_M", "MP_LR_Tat_005_F", TattooData.ZoneTypes.LeftArm) },
            { 430, new TattooData("mplowrider_overlays", "TAT_S1_007", "MP_LR_Tat_007_M", "MP_LR_Tat_007_F", TattooData.ZoneTypes.LeftLeg) },
            { 431, new TattooData("mplowrider_overlays", "TAT_S1_009", "MP_LR_Tat_009_M", "MP_LR_Tat_009_F", TattooData.ZoneTypes.Torso) },
            { 432, new TattooData("mplowrider_overlays", "TAT_S1_010", "MP_LR_Tat_010_M", "MP_LR_Tat_010_F", TattooData.ZoneTypes.Torso) },
            { 433, new TattooData("mplowrider_overlays", "TAT_S1_013", "MP_LR_Tat_013_M", "MP_LR_Tat_013_F", TattooData.ZoneTypes.Torso) },
            { 434, new TattooData("mplowrider_overlays", "TAT_S1_014", "MP_LR_Tat_014_M", "MP_LR_Tat_014_F", TattooData.ZoneTypes.Torso) },
            { 435, new TattooData("mplowrider_overlays", "TAT_S1_015", "MP_LR_Tat_015_M", "MP_LR_Tat_015_F", TattooData.ZoneTypes.RightArm) },
            { 436, new TattooData("mplowrider_overlays", "TAT_S1_017", "MP_LR_Tat_017_M", "MP_LR_Tat_017_F", TattooData.ZoneTypes.RightLeg) },
            { 437, new TattooData("mplowrider_overlays", "TAT_S1_020", "MP_LR_Tat_020_M", "MP_LR_Tat_020_F", TattooData.ZoneTypes.LeftLeg) },
            { 438, new TattooData("mplowrider_overlays", "TAT_S1_021", "MP_LR_Tat_021_M", "MP_LR_Tat_021_F", TattooData.ZoneTypes.Torso) },
            { 439, new TattooData("mplowrider_overlays", "TAT_S1_023", "MP_LR_Tat_023_M", "MP_LR_Tat_023_F", TattooData.ZoneTypes.RightLeg) },
            { 440, new TattooData("mplowrider_overlays", "TAT_S1_026", "MP_LR_Tat_026_M", "MP_LR_Tat_026_F", TattooData.ZoneTypes.Torso) },
            { 441, new TattooData("mplowrider_overlays", "TAT_S1_027", "MP_LR_Tat_027_M", "MP_LR_Tat_027_F", TattooData.ZoneTypes.LeftArm) },
            { 442, new TattooData("mplowrider_overlays", "TAT_S1_033", "MP_LR_Tat_033_M", "MP_LR_Tat_033_F", TattooData.ZoneTypes.LeftArm) },

            { 443, new TattooData("mpluxe2_overlays", "TAT_L2_002", "MP_LUXE_TAT_002_M", "MP_LUXE_TAT_002_F", TattooData.ZoneTypes.Torso) },
            { 444, new TattooData("mpluxe2_overlays", "TAT_L2_005", "MP_LUXE_TAT_005_M", "MP_LUXE_TAT_005_F", TattooData.ZoneTypes.LeftArm) },
            { 445, new TattooData("mpluxe2_overlays", "TAT_L2_010", "MP_LUXE_TAT_010_M", "MP_LUXE_TAT_010_F", TattooData.ZoneTypes.RightArm) },
            { 446, new TattooData("mpluxe2_overlays", "TAT_L2_011", "MP_LUXE_TAT_011_M", "MP_LUXE_TAT_011_F", TattooData.ZoneTypes.LeftLeg) },
            { 447, new TattooData("mpluxe2_overlays", "TAT_L2_012", "MP_LUXE_TAT_012_M", "MP_LUXE_TAT_012_F", TattooData.ZoneTypes.Torso) },
            { 448, new TattooData("mpluxe2_overlays", "TAT_L2_016", "MP_LUXE_TAT_016_M", "MP_LUXE_TAT_016_F", TattooData.ZoneTypes.LeftArm) },
            { 449, new TattooData("mpluxe2_overlays", "TAT_L2_017", "MP_LUXE_TAT_017_M", "MP_LUXE_TAT_017_F", TattooData.ZoneTypes.RightArm) },
            { 450, new TattooData("mpluxe2_overlays", "TAT_L2_018", "MP_LUXE_TAT_018_M", "MP_LUXE_TAT_018_F", TattooData.ZoneTypes.LeftArm) },
            { 451, new TattooData("mpluxe2_overlays", "TAT_L2_022", "MP_LUXE_TAT_022_M", "MP_LUXE_TAT_022_F", TattooData.ZoneTypes.Torso) },
            { 452, new TattooData("mpluxe2_overlays", "TAT_L2_023", "MP_LUXE_TAT_023_M", "MP_LUXE_TAT_023_F", TattooData.ZoneTypes.RightLeg) },
            { 453, new TattooData("mpluxe2_overlays", "TAT_L2_025", "MP_LUXE_TAT_025_M", "MP_LUXE_TAT_025_F", TattooData.ZoneTypes.Torso) },
            { 454, new TattooData("mpluxe2_overlays", "TAT_L2_026", "MP_LUXE_TAT_026_M", "MP_LUXE_TAT_026_F", TattooData.ZoneTypes.RightArm) },
            { 455, new TattooData("mpluxe2_overlays", "TAT_L2_027", "MP_LUXE_TAT_027_M", "MP_LUXE_TAT_027_F", TattooData.ZoneTypes.Torso) },
            { 456, new TattooData("mpluxe2_overlays", "TAT_L2_028", "MP_LUXE_TAT_028_M", "MP_LUXE_TAT_028_F", TattooData.ZoneTypes.LeftArm) },
            { 457, new TattooData("mpluxe2_overlays", "TAT_L2_029", "MP_LUXE_TAT_029_M", "MP_LUXE_TAT_029_F", TattooData.ZoneTypes.Torso) },
            { 458, new TattooData("mpluxe2_overlays", "TAT_L2_030", "MP_LUXE_TAT_030_M", "MP_LUXE_TAT_030_F", TattooData.ZoneTypes.RightArm) },
            { 459, new TattooData("mpluxe2_overlays", "TAT_L2_031", "MP_LUXE_TAT_031_M", "MP_LUXE_TAT_031_F", TattooData.ZoneTypes.LeftArm) },

            { 460, new TattooData("mpluxe_overlays", "TAT_LX_000", "MP_LUXE_TAT_000_M", "MP_LUXE_TAT_000_F", TattooData.ZoneTypes.LeftLeg) },
            { 461, new TattooData("mpluxe_overlays", "TAT_LX_001", "MP_LUXE_TAT_001_M", "MP_LUXE_TAT_001_F", TattooData.ZoneTypes.RightLeg) },
            { 462, new TattooData("mpluxe_overlays", "TAT_LX_003", "MP_LUXE_TAT_003_M", "MP_LUXE_TAT_003_F", TattooData.ZoneTypes.Torso) },
            { 463, new TattooData("mpluxe_overlays", "TAT_LX_004", "MP_LUXE_TAT_004_M", "MP_LUXE_TAT_004_F", TattooData.ZoneTypes.RightArm) },
            { 464, new TattooData("mpluxe_overlays", "TAT_LX_006", "MP_LUXE_TAT_006_M", "MP_LUXE_TAT_006_F", TattooData.ZoneTypes.Torso) },
            { 465, new TattooData("mpluxe_overlays", "TAT_LX_007", "MP_LUXE_TAT_007_M", "MP_LUXE_TAT_007_F", TattooData.ZoneTypes.Torso) },
            { 466, new TattooData("mpluxe_overlays", "TAT_LX_008", "MP_LUXE_TAT_008_M", "MP_LUXE_TAT_008_F", TattooData.ZoneTypes.Torso) },
            { 467, new TattooData("mpluxe_overlays", "TAT_LX_009", "MP_LUXE_TAT_009_M", "MP_LUXE_TAT_009_F", TattooData.ZoneTypes.LeftArm) },
            { 468, new TattooData("mpluxe_overlays", "TAT_LX_013", "MP_LUXE_TAT_013_M", "MP_LUXE_TAT_013_F", TattooData.ZoneTypes.RightArm) },
            { 469, new TattooData("mpluxe_overlays", "TAT_LX_014", "MP_LUXE_TAT_014_M", "MP_LUXE_TAT_014_F", TattooData.ZoneTypes.Torso) },
            { 470, new TattooData("mpluxe_overlays", "TAT_LX_015", "MP_LUXE_TAT_015_M", "MP_LUXE_TAT_015_F", TattooData.ZoneTypes.Torso) },
            { 471, new TattooData("mpluxe_overlays", "TAT_LX_019", "MP_LUXE_TAT_019_M", "MP_LUXE_TAT_019_F", TattooData.ZoneTypes.RightArm) },
            { 472, new TattooData("mpluxe_overlays", "TAT_LX_020", "MP_LUXE_TAT_020_M", "MP_LUXE_TAT_020_F", TattooData.ZoneTypes.LeftArm) },
            { 473, new TattooData("mpluxe_overlays", "TAT_LX_021", "MP_LUXE_TAT_021_M", "MP_LUXE_TAT_021_F", TattooData.ZoneTypes.LeftArm) },
            { 474, new TattooData("mpluxe_overlays", "TAT_LX_024", "MP_LUXE_TAT_024_M", "MP_LUXE_TAT_024_F", TattooData.ZoneTypes.Torso) },

            { 475, new TattooData("mpsecurity_overlays", "TAT_FX_000", "MP_Security_Tat_000_M", "MP_Security_Tat_000_F", TattooData.ZoneTypes.RightArm) },
            { 476, new TattooData("mpsecurity_overlays", "TAT_FX_001", "MP_Security_Tat_001_M", "MP_Security_Tat_001_F", TattooData.ZoneTypes.Head) },
            { 477, new TattooData("mpsecurity_overlays", "TAT_FX_002", "MP_Security_Tat_002_M", "MP_Security_Tat_002_F", TattooData.ZoneTypes.Head) },
            { 478, new TattooData("mpsecurity_overlays", "TAT_FX_003", "MP_Security_Tat_003_M", "MP_Security_Tat_003_F", TattooData.ZoneTypes.RightLeg) },
            { 479, new TattooData("mpsecurity_overlays", "TAT_FX_004", "MP_Security_Tat_004_M", "MP_Security_Tat_004_F", TattooData.ZoneTypes.Torso) },
            { 480, new TattooData("mpsecurity_overlays", "TAT_FX_005", "MP_Security_Tat_005_M", "MP_Security_Tat_005_F", TattooData.ZoneTypes.RightArm) },
            { 481, new TattooData("mpsecurity_overlays", "TAT_FX_006", "MP_Security_Tat_006_M", "MP_Security_Tat_006_F", TattooData.ZoneTypes.LeftArm) },
            { 482, new TattooData("mpsecurity_overlays", "TAT_FX_007", "MP_Security_Tat_007_M", "MP_Security_Tat_007_F", TattooData.ZoneTypes.RightArm) },
            { 483, new TattooData("mpsecurity_overlays", "TAT_FX_008", "MP_Security_Tat_008_M", "MP_Security_Tat_008_F", TattooData.ZoneTypes.Torso) },
            { 484, new TattooData("mpsecurity_overlays", "TAT_FX_009", "MP_Security_Tat_009_M", "MP_Security_Tat_009_F", TattooData.ZoneTypes.RightArm) },
            { 485, new TattooData("mpsecurity_overlays", "TAT_FX_010", "MP_Security_Tat_010_M", "MP_Security_Tat_010_F", TattooData.ZoneTypes.LeftArm) },
            { 486, new TattooData("mpsecurity_overlays", "TAT_FX_011", "MP_Security_Tat_011_M", "MP_Security_Tat_011_F", TattooData.ZoneTypes.LeftArm) },
            { 487, new TattooData("mpsecurity_overlays", "TAT_FX_012", "MP_Security_Tat_012_M", "MP_Security_Tat_012_F", TattooData.ZoneTypes.RightArm) },
            { 488, new TattooData("mpsecurity_overlays", "TAT_FX_013", "MP_Security_Tat_013_M", "MP_Security_Tat_013_F", TattooData.ZoneTypes.Torso) },
            { 489, new TattooData("mpsecurity_overlays", "TAT_FX_014", "MP_Security_Tat_014_M", "MP_Security_Tat_014_F", TattooData.ZoneTypes.Torso) },
            { 490, new TattooData("mpsecurity_overlays", "TAT_FX_015", "MP_Security_Tat_015_M", "MP_Security_Tat_015_F", TattooData.ZoneTypes.Torso) },
            { 491, new TattooData("mpsecurity_overlays", "TAT_FX_016", "MP_Security_Tat_016_M", "MP_Security_Tat_016_F", TattooData.ZoneTypes.Torso) },
            { 492, new TattooData("mpsecurity_overlays", "TAT_FX_017", "MP_Security_Tat_017_M", "MP_Security_Tat_017_F", TattooData.ZoneTypes.Torso) },
            { 493, new TattooData("mpsecurity_overlays", "TAT_FX_018", "MP_Security_Tat_018_M", "MP_Security_Tat_018_F", TattooData.ZoneTypes.Torso) },
            { 494, new TattooData("mpsecurity_overlays", "TAT_FX_019", "MP_Security_Tat_019_M", "MP_Security_Tat_019_F", TattooData.ZoneTypes.LeftArm) },
            { 495, new TattooData("mpsecurity_overlays", "TAT_FX_020", "MP_Security_Tat_020_M", "MP_Security_Tat_020_F", TattooData.ZoneTypes.RightArm) },
            { 496, new TattooData("mpsecurity_overlays", "TAT_FX_021", "MP_Security_Tat_021_M", "MP_Security_Tat_021_F", TattooData.ZoneTypes.RightLeg) },
            { 497, new TattooData("mpsecurity_overlays", "TAT_FX_022", "MP_Security_Tat_022_M", "MP_Security_Tat_022_F", TattooData.ZoneTypes.LeftLeg) },
            { 498, new TattooData("mpsecurity_overlays", "TAT_FX_023", "MP_Security_Tat_023_M", "MP_Security_Tat_023_F", TattooData.ZoneTypes.LeftLeg) },
            { 499, new TattooData("mpsecurity_overlays", "TAT_FX_024", "MP_Security_Tat_024_M", "MP_Security_Tat_024_F", TattooData.ZoneTypes.Torso) },
            { 500, new TattooData("mpsecurity_overlays", "TAT_FX_025", "MP_Security_Tat_025_M", "MP_Security_Tat_025_F", TattooData.ZoneTypes.Torso) },
            { 501, new TattooData("mpsecurity_overlays", "TAT_FX_026", "MP_Security_Tat_026_M", "MP_Security_Tat_026_F", TattooData.ZoneTypes.Torso) },
            { 502, new TattooData("mpsecurity_overlays", "TAT_FX_027", "MP_Security_Tat_027_M", "MP_Security_Tat_027_F", TattooData.ZoneTypes.Head) },

            { 503, new TattooData("mpsmuggler_overlays", "TAT_SM_000", "MP_Smuggler_Tattoo_000_M", "MP_Smuggler_Tattoo_000_F", TattooData.ZoneTypes.Torso) },
            { 504, new TattooData("mpsmuggler_overlays", "TAT_SM_001", "MP_Smuggler_Tattoo_001_M", "MP_Smuggler_Tattoo_001_F", TattooData.ZoneTypes.RightArm) },
            { 505, new TattooData("mpsmuggler_overlays", "TAT_SM_002", "MP_Smuggler_Tattoo_002_M", "MP_Smuggler_Tattoo_002_F", TattooData.ZoneTypes.Torso) },
            { 506, new TattooData("mpsmuggler_overlays", "TAT_SM_003", "MP_Smuggler_Tattoo_003_M", "MP_Smuggler_Tattoo_003_F", TattooData.ZoneTypes.Torso) },
            { 507, new TattooData("mpsmuggler_overlays", "TAT_SM_004", "MP_Smuggler_Tattoo_004_M", "MP_Smuggler_Tattoo_004_F", TattooData.ZoneTypes.LeftArm) },
            { 508, new TattooData("mpsmuggler_overlays", "TAT_SM_005", "MP_Smuggler_Tattoo_005_M", "MP_Smuggler_Tattoo_005_F", TattooData.ZoneTypes.RightArm) },
            { 509, new TattooData("mpsmuggler_overlays", "TAT_SM_006", "MP_Smuggler_Tattoo_006_M", "MP_Smuggler_Tattoo_006_F", TattooData.ZoneTypes.Torso) },
            { 510, new TattooData("mpsmuggler_overlays", "TAT_SM_007", "MP_Smuggler_Tattoo_007_M", "MP_Smuggler_Tattoo_007_F", TattooData.ZoneTypes.Torso) },
            { 511, new TattooData("mpsmuggler_overlays", "TAT_SM_008", "MP_Smuggler_Tattoo_008_M", "MP_Smuggler_Tattoo_008_F", TattooData.ZoneTypes.LeftArm) },
            { 512, new TattooData("mpsmuggler_overlays", "TAT_SM_009", "MP_Smuggler_Tattoo_009_M", "MP_Smuggler_Tattoo_009_F", TattooData.ZoneTypes.Torso) },
            { 513, new TattooData("mpsmuggler_overlays", "TAT_SM_010", "MP_Smuggler_Tattoo_010_M", "MP_Smuggler_Tattoo_010_F", TattooData.ZoneTypes.Torso) },
            { 514, new TattooData("mpsmuggler_overlays", "TAT_SM_011", "MP_Smuggler_Tattoo_011_M", "MP_Smuggler_Tattoo_011_F", TattooData.ZoneTypes.Head) },
            { 515, new TattooData("mpsmuggler_overlays", "TAT_SM_012", "MP_Smuggler_Tattoo_012_M", "MP_Smuggler_Tattoo_012_F", TattooData.ZoneTypes.Head) },
            { 516, new TattooData("mpsmuggler_overlays", "TAT_SM_013", "MP_Smuggler_Tattoo_013_M", "MP_Smuggler_Tattoo_013_F", TattooData.ZoneTypes.Torso) },
            { 517, new TattooData("mpsmuggler_overlays", "TAT_SM_014", "MP_Smuggler_Tattoo_014_M", "MP_Smuggler_Tattoo_014_F", TattooData.ZoneTypes.LeftArm) },
            { 518, new TattooData("mpsmuggler_overlays", "TAT_SM_015", "MP_Smuggler_Tattoo_015_M", "MP_Smuggler_Tattoo_015_F", TattooData.ZoneTypes.Torso) },
            { 519, new TattooData("mpsmuggler_overlays", "TAT_SM_016", "MP_Smuggler_Tattoo_016_M", "MP_Smuggler_Tattoo_016_F", TattooData.ZoneTypes.Torso) },
            { 520, new TattooData("mpsmuggler_overlays", "TAT_SM_017", "MP_Smuggler_Tattoo_017_M", "MP_Smuggler_Tattoo_017_F", TattooData.ZoneTypes.Torso) },
            { 521, new TattooData("mpsmuggler_overlays", "TAT_SM_018", "MP_Smuggler_Tattoo_018_M", "MP_Smuggler_Tattoo_018_F", TattooData.ZoneTypes.Torso) },
            { 522, new TattooData("mpsmuggler_overlays", "TAT_SM_019", "MP_Smuggler_Tattoo_019_M", "MP_Smuggler_Tattoo_019_F", TattooData.ZoneTypes.Torso) },
            { 523, new TattooData("mpsmuggler_overlays", "TAT_SM_020", "MP_Smuggler_Tattoo_020_M", "MP_Smuggler_Tattoo_020_F", TattooData.ZoneTypes.RightLeg) },
            { 524, new TattooData("mpsmuggler_overlays", "TAT_SM_021", "MP_Smuggler_Tattoo_021_M", "MP_Smuggler_Tattoo_021_F", TattooData.ZoneTypes.Torso) },
            { 525, new TattooData("mpsmuggler_overlays", "TAT_SM_022", "MP_Smuggler_Tattoo_022_M", "MP_Smuggler_Tattoo_022_F", TattooData.ZoneTypes.Torso) },
            { 526, new TattooData("mpsmuggler_overlays", "TAT_SM_023", "MP_Smuggler_Tattoo_023_M", "MP_Smuggler_Tattoo_023_F", TattooData.ZoneTypes.RightArm) },
            { 527, new TattooData("mpsmuggler_overlays", "TAT_SM_024", "MP_Smuggler_Tattoo_024_M", "MP_Smuggler_Tattoo_024_F", TattooData.ZoneTypes.Torso) },
            { 528, new TattooData("mpsmuggler_overlays", "TAT_SM_025", "MP_Smuggler_Tattoo_025_M", "MP_Smuggler_Tattoo_025_F", TattooData.ZoneTypes.Torso) },

            { 529, new TattooData("mpstunt_overlays", "TAT_ST_000", "MP_MP_Stunt_Tat_000_M", "MP_MP_Stunt_Tat_000_F", TattooData.ZoneTypes.Head) },
            { 530, new TattooData("mpstunt_overlays", "TAT_ST_001", "MP_MP_Stunt_tat_001_M", "MP_MP_Stunt_tat_001_F", TattooData.ZoneTypes.LeftArm) },
            { 531, new TattooData("mpstunt_overlays", "TAT_ST_002", "MP_MP_Stunt_tat_002_M", "MP_MP_Stunt_tat_002_F", TattooData.ZoneTypes.LeftArm) },
            { 532, new TattooData("mpstunt_overlays", "TAT_ST_003", "MP_MP_Stunt_tat_003_M", "MP_MP_Stunt_tat_003_F", TattooData.ZoneTypes.RightArm) },
            { 533, new TattooData("mpstunt_overlays", "TAT_ST_004", "MP_MP_Stunt_tat_004_M", "MP_MP_Stunt_tat_004_F", TattooData.ZoneTypes.Head) },
            { 534, new TattooData("mpstunt_overlays", "TAT_ST_005", "MP_MP_Stunt_tat_005_M", "MP_MP_Stunt_tat_005_F", TattooData.ZoneTypes.RightLeg) },
            { 535, new TattooData("mpstunt_overlays", "TAT_ST_006", "MP_MP_Stunt_tat_006_M", "MP_MP_Stunt_tat_006_F", TattooData.ZoneTypes.Head) },
            { 536, new TattooData("mpstunt_overlays", "TAT_ST_007", "MP_MP_Stunt_tat_007_M", "MP_MP_Stunt_tat_007_F", TattooData.ZoneTypes.LeftLeg) },
            { 537, new TattooData("mpstunt_overlays", "TAT_ST_008", "MP_MP_Stunt_tat_008_M", "MP_MP_Stunt_tat_008_F", TattooData.ZoneTypes.LeftArm) },
            { 538, new TattooData("mpstunt_overlays", "TAT_ST_009", "MP_MP_Stunt_tat_009_M", "MP_MP_Stunt_tat_009_F", TattooData.ZoneTypes.RightArm) },
            { 539, new TattooData("mpstunt_overlays", "TAT_ST_010", "MP_MP_Stunt_tat_010_M", "MP_MP_Stunt_tat_010_F", TattooData.ZoneTypes.RightArm) },
            { 540, new TattooData("mpstunt_overlays", "TAT_ST_011", "MP_MP_Stunt_tat_011_M", "MP_MP_Stunt_tat_011_F", TattooData.ZoneTypes.Torso) },
            { 541, new TattooData("mpstunt_overlays", "TAT_ST_012", "MP_MP_Stunt_tat_012_M", "MP_MP_Stunt_tat_012_F", TattooData.ZoneTypes.Torso) },
            { 542, new TattooData("mpstunt_overlays", "TAT_ST_013", "MP_MP_Stunt_tat_013_M", "MP_MP_Stunt_tat_013_F", TattooData.ZoneTypes.LeftLeg) },
            { 543, new TattooData("mpstunt_overlays", "TAT_ST_014", "MP_MP_Stunt_tat_014_M", "MP_MP_Stunt_tat_014_F", TattooData.ZoneTypes.Torso) },
            { 544, new TattooData("mpstunt_overlays", "TAT_ST_015", "MP_MP_Stunt_tat_015_M", "MP_MP_Stunt_tat_015_F", TattooData.ZoneTypes.RightLeg) },
            { 545, new TattooData("mpstunt_overlays", "TAT_ST_016", "MP_MP_Stunt_tat_016_M", "MP_MP_Stunt_tat_016_F", TattooData.ZoneTypes.RightArm) },
            { 546, new TattooData("mpstunt_overlays", "TAT_ST_017", "MP_MP_Stunt_tat_017_M", "MP_MP_Stunt_tat_017_F", TattooData.ZoneTypes.Head) },
            { 547, new TattooData("mpstunt_overlays", "TAT_ST_018", "MP_MP_Stunt_tat_018_M", "MP_MP_Stunt_tat_018_F", TattooData.ZoneTypes.Torso) },
            { 548, new TattooData("mpstunt_overlays", "TAT_ST_019", "MP_MP_Stunt_tat_019_M", "MP_MP_Stunt_tat_019_F", TattooData.ZoneTypes.Torso) },
            { 549, new TattooData("mpstunt_overlays", "TAT_ST_020", "MP_MP_Stunt_tat_020_M", "MP_MP_Stunt_tat_020_F", TattooData.ZoneTypes.RightLeg) },
            { 550, new TattooData("mpstunt_overlays", "TAT_ST_021", "MP_MP_Stunt_tat_021_M", "MP_MP_Stunt_tat_021_F", TattooData.ZoneTypes.LeftLeg) },
            { 551, new TattooData("mpstunt_overlays", "TAT_ST_022", "MP_MP_Stunt_tat_022_M", "MP_MP_Stunt_tat_022_F", TattooData.ZoneTypes.LeftArm) },
            { 552, new TattooData("mpstunt_overlays", "TAT_ST_023", "MP_MP_Stunt_tat_023_M", "MP_MP_Stunt_tat_023_F", TattooData.ZoneTypes.LeftArm) },
            { 553, new TattooData("mpstunt_overlays", "TAT_ST_024", "MP_MP_Stunt_tat_024_M", "MP_MP_Stunt_tat_024_F", TattooData.ZoneTypes.Torso) },
            { 554, new TattooData("mpstunt_overlays", "TAT_ST_025", "MP_MP_Stunt_tat_025_M", "MP_MP_Stunt_tat_025_F", TattooData.ZoneTypes.RightLeg) },
            { 555, new TattooData("mpstunt_overlays", "TAT_ST_026", "MP_MP_Stunt_tat_026_M", "MP_MP_Stunt_tat_026_F", TattooData.ZoneTypes.Torso) },
            { 556, new TattooData("mpstunt_overlays", "TAT_ST_027", "MP_MP_Stunt_tat_027_M", "MP_MP_Stunt_tat_027_F", TattooData.ZoneTypes.Torso) },
            { 557, new TattooData("mpstunt_overlays", "TAT_ST_028", "MP_MP_Stunt_tat_028_M", "MP_MP_Stunt_tat_028_F", TattooData.ZoneTypes.LeftLeg) },
            { 558, new TattooData("mpstunt_overlays", "TAT_ST_029", "MP_MP_Stunt_tat_029_M", "MP_MP_Stunt_tat_029_F", TattooData.ZoneTypes.Torso) },
            { 559, new TattooData("mpstunt_overlays", "TAT_ST_030", "MP_MP_Stunt_tat_030_M", "MP_MP_Stunt_tat_030_F", TattooData.ZoneTypes.Torso) },
            { 560, new TattooData("mpstunt_overlays", "TAT_ST_031", "MP_MP_Stunt_tat_031_M", "MP_MP_Stunt_tat_031_F", TattooData.ZoneTypes.LeftLeg) },
            { 561, new TattooData("mpstunt_overlays", "TAT_ST_032", "MP_MP_Stunt_tat_032_M", "MP_MP_Stunt_tat_032_F", TattooData.ZoneTypes.RightLeg) },
            { 562, new TattooData("mpstunt_overlays", "TAT_ST_033", "MP_MP_Stunt_tat_033_M", "MP_MP_Stunt_tat_033_F", TattooData.ZoneTypes.Torso) },
            { 563, new TattooData("mpstunt_overlays", "TAT_ST_034", "MP_MP_Stunt_tat_034_M", "MP_MP_Stunt_tat_034_F", TattooData.ZoneTypes.Torso) },
            { 564, new TattooData("mpstunt_overlays", "TAT_ST_035", "MP_MP_Stunt_tat_035_M", "MP_MP_Stunt_tat_035_F", TattooData.ZoneTypes.LeftArm) },
            { 565, new TattooData("mpstunt_overlays", "TAT_ST_036", "MP_MP_Stunt_tat_036_M", "MP_MP_Stunt_tat_036_F", TattooData.ZoneTypes.RightArm) },
            { 566, new TattooData("mpstunt_overlays", "TAT_ST_037", "MP_MP_Stunt_tat_037_M", "MP_MP_Stunt_tat_037_F", TattooData.ZoneTypes.Torso) },
            { 567, new TattooData("mpstunt_overlays", "TAT_ST_038", "MP_MP_Stunt_tat_038_M", "MP_MP_Stunt_tat_038_F", TattooData.ZoneTypes.RightArm) },
            { 568, new TattooData("mpstunt_overlays", "TAT_ST_039", "MP_MP_Stunt_tat_039_M", "MP_MP_Stunt_tat_039_F", TattooData.ZoneTypes.LeftArm) },
            { 569, new TattooData("mpstunt_overlays", "TAT_ST_040", "MP_MP_Stunt_tat_040_M", "MP_MP_Stunt_tat_040_F", TattooData.ZoneTypes.Torso) },
            { 570, new TattooData("mpstunt_overlays", "TAT_ST_041", "MP_MP_Stunt_tat_041_M", "MP_MP_Stunt_tat_041_F", TattooData.ZoneTypes.Torso) },
            { 571, new TattooData("mpstunt_overlays", "TAT_ST_042", "MP_MP_Stunt_tat_042_M", "MP_MP_Stunt_tat_042_F", TattooData.ZoneTypes.Head) },
            { 572, new TattooData("mpstunt_overlays", "TAT_ST_043", "MP_MP_Stunt_tat_043_M", "MP_MP_Stunt_tat_043_F", TattooData.ZoneTypes.LeftArm) },
            { 573, new TattooData("mpstunt_overlays", "TAT_ST_044", "MP_MP_Stunt_tat_044_M", "MP_MP_Stunt_tat_044_F", TattooData.ZoneTypes.Torso) },
            { 574, new TattooData("mpstunt_overlays", "TAT_ST_045", "MP_MP_Stunt_tat_045_M", "MP_MP_Stunt_tat_045_F", TattooData.ZoneTypes.RightLeg) },
            { 575, new TattooData("mpstunt_overlays", "TAT_ST_046", "MP_MP_Stunt_tat_046_M", "MP_MP_Stunt_tat_046_F", TattooData.ZoneTypes.Torso) },
            { 576, new TattooData("mpstunt_overlays", "TAT_ST_047", "MP_MP_Stunt_tat_047_M", "MP_MP_Stunt_tat_047_F", TattooData.ZoneTypes.RightLeg) },
            { 577, new TattooData("mpstunt_overlays", "TAT_ST_048", "MP_MP_Stunt_tat_048_M", "MP_MP_Stunt_tat_048_F", TattooData.ZoneTypes.Torso) },
            { 578, new TattooData("mpstunt_overlays", "TAT_ST_049", "MP_MP_Stunt_tat_049_M", "MP_MP_Stunt_tat_049_F", TattooData.ZoneTypes.RightArm) },

            { 579, new TattooData("mpsum2_overlays", "TAT_SB_000", "MP_Sum2_Tat_000_M", "MP_Sum2_Tat_000_F", TattooData.ZoneTypes.Head) },
            { 580, new TattooData("mpsum2_overlays", "TAT_SB_001", "MP_Sum2_Tat_001_M", "MP_Sum2_Tat_001_F", TattooData.ZoneTypes.Head) },
            { 581, new TattooData("mpsum2_overlays", "TAT_SB_002", "MP_Sum2_Tat_002_M", "MP_Sum2_Tat_002_F", TattooData.ZoneTypes.LeftLeg) },
            { 582, new TattooData("mpsum2_overlays", "TAT_SB_003", "MP_Sum2_Tat_003_M", "MP_Sum2_Tat_003_F", TattooData.ZoneTypes.Torso) },
            { 583, new TattooData("mpsum2_overlays", "TAT_SB_004", "MP_Sum2_Tat_004_M", "MP_Sum2_Tat_004_F", TattooData.ZoneTypes.Torso) },
            { 584, new TattooData("mpsum2_overlays", "TAT_SB_005", "MP_Sum2_Tat_005_M", "MP_Sum2_Tat_005_F", TattooData.ZoneTypes.Torso) },
            { 585, new TattooData("mpsum2_overlays", "TAT_SB_006", "MP_Sum2_Tat_006_M", "MP_Sum2_Tat_006_F", TattooData.ZoneTypes.Torso) },
            { 586, new TattooData("mpsum2_overlays", "TAT_SB_007", "MP_Sum2_Tat_007_M", "MP_Sum2_Tat_007_F", TattooData.ZoneTypes.Torso) },
            { 587, new TattooData("mpsum2_overlays", "TAT_SB_008", "MP_Sum2_Tat_008_M", "MP_Sum2_Tat_008_F", TattooData.ZoneTypes.LeftArm) },
            { 588, new TattooData("mpsum2_overlays", "TAT_SB_009", "MP_Sum2_Tat_009_M", "MP_Sum2_Tat_009_F", TattooData.ZoneTypes.LeftArm) },
            { 589, new TattooData("mpsum2_overlays", "TAT_SB_010", "MP_Sum2_Tat_010_M", "MP_Sum2_Tat_010_F", TattooData.ZoneTypes.LeftArm) },
            { 590, new TattooData("mpsum2_overlays", "TAT_SB_011", "MP_Sum2_Tat_011_M", "MP_Sum2_Tat_011_F", TattooData.ZoneTypes.RightArm) },
            { 591, new TattooData("mpsum2_overlays", "TAT_SB_012", "MP_Sum2_Tat_012_M", "MP_Sum2_Tat_012_F", TattooData.ZoneTypes.RightArm) },
            { 592, new TattooData("mpsum2_overlays", "TAT_SB_013", "MP_Sum2_Tat_013_M", "MP_Sum2_Tat_013_F", TattooData.ZoneTypes.RightArm) },
            { 593, new TattooData("mpsum2_overlays", "TAT_SB_014", "MP_Sum2_Tat_014_M", "MP_Sum2_Tat_014_F", TattooData.ZoneTypes.LeftLeg) },
            { 594, new TattooData("mpsum2_overlays", "TAT_SB_015", "MP_Sum2_Tat_015_M", "MP_Sum2_Tat_015_F", TattooData.ZoneTypes.LeftLeg) },
            { 595, new TattooData("mpsum2_overlays", "TAT_SB_016", "MP_Sum2_Tat_016_M", "MP_Sum2_Tat_016_F", TattooData.ZoneTypes.LeftLeg) },
            { 596, new TattooData("mpsum2_overlays", "TAT_SB_017", "MP_Sum2_Tat_017_M", "MP_Sum2_Tat_017_F", TattooData.ZoneTypes.RightLeg) },
            { 597, new TattooData("mpsum2_overlays", "TAT_SB_018", "MP_Sum2_Tat_018_M", "MP_Sum2_Tat_018_F", TattooData.ZoneTypes.Head) },
            { 598, new TattooData("mpsum2_overlays", "TAT_SB_019", "MP_Sum2_Tat_019_M", "MP_Sum2_Tat_019_F", TattooData.ZoneTypes.Head) },
            { 599, new TattooData("mpsum2_overlays", "TAT_SB_020", "MP_Sum2_Tat_020_M", "MP_Sum2_Tat_020_F", TattooData.ZoneTypes.Head) },
            { 600, new TattooData("mpsum2_overlays", "TAT_SB_021", "MP_Sum2_Tat_021_M", "MP_Sum2_Tat_021_F", TattooData.ZoneTypes.Head) },
            { 601, new TattooData("mpsum2_overlays", "TAT_SB_022", "MP_Sum2_Tat_022_M", "MP_Sum2_Tat_022_F", TattooData.ZoneTypes.Head) },
            { 602, new TattooData("mpsum2_overlays", "TAT_SB_023", "MP_Sum2_Tat_023_M", "MP_Sum2_Tat_023_F", TattooData.ZoneTypes.Head) },
            { 603, new TattooData("mpsum2_overlays", "TAT_SB_024", "MP_Sum2_Tat_024_M", "MP_Sum2_Tat_024_F", TattooData.ZoneTypes.Head) },
            { 604, new TattooData("mpsum2_overlays", "TAT_SB_025", "MP_Sum2_Tat_025_M", "MP_Sum2_Tat_025_F", TattooData.ZoneTypes.Head) },
            { 605, new TattooData("mpsum2_overlays", "TAT_SB_026", "MP_Sum2_Tat_026_M", "MP_Sum2_Tat_026_F", TattooData.ZoneTypes.Head) },
            { 606, new TattooData("mpsum2_overlays", "TAT_SB_027", "MP_Sum2_Tat_027_M", "MP_Sum2_Tat_027_F", TattooData.ZoneTypes.Head) },
            { 607, new TattooData("mpsum2_overlays", "TAT_SB_028", "MP_Sum2_Tat_028_M", "MP_Sum2_Tat_028_F", TattooData.ZoneTypes.LeftArm) },
            { 608, new TattooData("mpsum2_overlays", "TAT_SB_029", "MP_Sum2_Tat_029_M", "MP_Sum2_Tat_029_F", TattooData.ZoneTypes.LeftArm) },
            { 609, new TattooData("mpsum2_overlays", "TAT_SB_030", "MP_Sum2_Tat_030_M", "MP_Sum2_Tat_030_F", TattooData.ZoneTypes.RightArm) },
            { 610, new TattooData("mpsum2_overlays", "TAT_SB_031", "MP_Sum2_Tat_031_M", "MP_Sum2_Tat_031_F", TattooData.ZoneTypes.RightArm) },
            { 611, new TattooData("mpsum2_overlays", "TAT_SB_032", "MP_Sum2_Tat_032_M", "MP_Sum2_Tat_032_F", TattooData.ZoneTypes.LeftLeg) },
            { 612, new TattooData("mpsum2_overlays", "TAT_SB_033", "MP_Sum2_Tat_033_M", "MP_Sum2_Tat_033_F", TattooData.ZoneTypes.RightLeg) },
            { 613, new TattooData("mpsum2_overlays", "TAT_SB_034", "MP_Sum2_Tat_034_M", "MP_Sum2_Tat_034_F", TattooData.ZoneTypes.RightLeg) },
            { 614, new TattooData("mpsum2_overlays", "TAT_SB_035", "MP_Sum2_Tat_035_M", "MP_Sum2_Tat_035_F", TattooData.ZoneTypes.Torso) },
            { 615, new TattooData("mpsum2_overlays", "TAT_SB_036", "MP_Sum2_Tat_036_M", "MP_Sum2_Tat_036_F", TattooData.ZoneTypes.Torso) },
            { 616, new TattooData("mpsum2_overlays", "TAT_SB_037", "MP_Sum2_Tat_037_M", "MP_Sum2_Tat_037_F", TattooData.ZoneTypes.Torso) },
            { 617, new TattooData("mpsum2_overlays", "TAT_SB_038", "MP_Sum2_Tat_038_M", "MP_Sum2_Tat_038_F", TattooData.ZoneTypes.Torso) },
            { 618, new TattooData("mpsum2_overlays", "TAT_SB_039", "MP_Sum2_Tat_039_M", "MP_Sum2_Tat_039_F", TattooData.ZoneTypes.Torso) },
            { 619, new TattooData("mpsum2_overlays", "TAT_SB_040", "MP_Sum2_Tat_040_M", "MP_Sum2_Tat_040_F", TattooData.ZoneTypes.Torso) },
            { 620, new TattooData("mpsum2_overlays", "TAT_SB_041", "MP_Sum2_Tat_041_M", "MP_Sum2_Tat_041_F", TattooData.ZoneTypes.Torso) },
            { 621, new TattooData("mpsum2_overlays", "TAT_SB_042", "MP_Sum2_Tat_042_M", "MP_Sum2_Tat_042_F", TattooData.ZoneTypes.Torso) },
            { 622, new TattooData("mpsum2_overlays", "TAT_SB_043", "MP_Sum2_Tat_043_M", "MP_Sum2_Tat_043_F", TattooData.ZoneTypes.Torso) },
            { 623, new TattooData("mpsum2_overlays", "TAT_SB_044", "MP_Sum2_Tat_044_M", "MP_Sum2_Tat_044_F", TattooData.ZoneTypes.Torso) },
            { 624, new TattooData("mpsum2_overlays", "TAT_SB_045", "MP_Sum2_Tat_045_M", "MP_Sum2_Tat_045_F", TattooData.ZoneTypes.RightArm) },
            { 625, new TattooData("mpsum2_overlays", "TAT_SB_046", "MP_Sum2_Tat_046_M", "MP_Sum2_Tat_046_F", TattooData.ZoneTypes.RightArm) },
            { 626, new TattooData("mpsum2_overlays", "TAT_SB_047", "MP_Sum2_Tat_047_M", "MP_Sum2_Tat_047_F", TattooData.ZoneTypes.RightArm) },
            { 627, new TattooData("mpsum2_overlays", "TAT_SB_048", "MP_Sum2_Tat_048_M", "MP_Sum2_Tat_048_F", TattooData.ZoneTypes.RightArm) },
            { 628, new TattooData("mpsum2_overlays", "TAT_SB_049", "MP_Sum2_Tat_049_M", "MP_Sum2_Tat_049_F", TattooData.ZoneTypes.LeftArm) },
            { 629, new TattooData("mpsum2_overlays", "TAT_SB_050", "MP_Sum2_Tat_050_M", "MP_Sum2_Tat_050_F", TattooData.ZoneTypes.RightLeg) },
            { 630, new TattooData("mpsum2_overlays", "TAT_SB_051", "MP_Sum2_Tat_051_M", "MP_Sum2_Tat_051_F", TattooData.ZoneTypes.RightLeg) },
            { 631, new TattooData("mpsum2_overlays", "TAT_SB_052", "MP_Sum2_Tat_052_M", "MP_Sum2_Tat_052_F", TattooData.ZoneTypes.RightLeg) },
            { 632, new TattooData("mpsum2_overlays", "TAT_SB_053", "MP_Sum2_Tat_053_M", "MP_Sum2_Tat_053_F", TattooData.ZoneTypes.LeftLeg) },
            { 633, new TattooData("mpsum2_overlays", "TAT_SB_054", "MP_Sum2_Tat_054_M", "MP_Sum2_Tat_054_F", TattooData.ZoneTypes.LeftLeg) },
            { 634, new TattooData("mpsum2_overlays", "TAT_SB_055", "MP_Sum2_Tat_055_M", "MP_Sum2_Tat_055_F", TattooData.ZoneTypes.LeftLeg) },
            { 635, new TattooData("mpsum2_overlays", "TAT_SB_056", "MP_Sum2_Tat_056_M", "MP_Sum2_Tat_056_F", TattooData.ZoneTypes.LeftLeg) },
            { 636, new TattooData("mpsum2_overlays", "TAT_SB_057", "MP_Sum2_Tat_057_M", "MP_Sum2_Tat_057_F", TattooData.ZoneTypes.Torso) },
            { 637, new TattooData("mpsum2_overlays", "TAT_SB_058", "MP_Sum2_Tat_058_M", "MP_Sum2_Tat_058_F", TattooData.ZoneTypes.Torso) },
            { 638, new TattooData("mpsum2_overlays", "TAT_SB_059", "MP_Sum2_Tat_059_M", "MP_Sum2_Tat_059_F", TattooData.ZoneTypes.Torso) },
            { 639, new TattooData("mpsum2_overlays", "TAT_SB_060", "MP_Sum2_Tat_060_M", "MP_Sum2_Tat_060_F", TattooData.ZoneTypes.Torso) },
            { 640, new TattooData("mpsum2_overlays", "TAT_SB_061", "MP_Sum2_Tat_061_M", "MP_Sum2_Tat_061_F", TattooData.ZoneTypes.Torso) },
            { 641, new TattooData("mpsum2_overlays", "TAT_SB_062", "MP_Sum2_Tat_062_M", "MP_Sum2_Tat_062_F", TattooData.ZoneTypes.Torso) },

            { 642, new TattooData("mpvinewood_overlays", "TAT_VW_000", "MP_Vinewood_Tat_000_M", "MP_Vinewood_Tat_000_F", TattooData.ZoneTypes.Torso) },
            { 643, new TattooData("mpvinewood_overlays", "TAT_VW_001", "MP_Vinewood_Tat_001_M", "MP_Vinewood_Tat_001_F", TattooData.ZoneTypes.Torso) },
            { 644, new TattooData("mpvinewood_overlays", "TAT_VW_002", "MP_Vinewood_Tat_002_M", "MP_Vinewood_Tat_002_F", TattooData.ZoneTypes.LeftArm) },
            { 645, new TattooData("mpvinewood_overlays", "TAT_VW_003", "MP_Vinewood_Tat_003_M", "MP_Vinewood_Tat_003_F", TattooData.ZoneTypes.Torso) },
            { 646, new TattooData("mpvinewood_overlays", "TAT_VW_004", "MP_Vinewood_Tat_004_M", "MP_Vinewood_Tat_004_F", TattooData.ZoneTypes.RightArm) },
            { 647, new TattooData("mpvinewood_overlays", "TAT_VW_005", "MP_Vinewood_Tat_005_M", "MP_Vinewood_Tat_005_F", TattooData.ZoneTypes.LeftArm) },
            { 648, new TattooData("mpvinewood_overlays", "TAT_VW_006", "MP_Vinewood_Tat_006_M", "MP_Vinewood_Tat_006_F", TattooData.ZoneTypes.Torso) },
            { 649, new TattooData("mpvinewood_overlays", "TAT_VW_007", "MP_Vinewood_Tat_007_M", "MP_Vinewood_Tat_007_F", TattooData.ZoneTypes.Torso) },
            { 650, new TattooData("mpvinewood_overlays", "TAT_VW_008", "MP_Vinewood_Tat_008_M", "MP_Vinewood_Tat_008_F", TattooData.ZoneTypes.Torso) },
            { 651, new TattooData("mpvinewood_overlays", "TAT_VW_009", "MP_Vinewood_Tat_009_M", "MP_Vinewood_Tat_009_F", TattooData.ZoneTypes.Torso) },
            { 652, new TattooData("mpvinewood_overlays", "TAT_VW_010", "MP_Vinewood_Tat_010_M", "MP_Vinewood_Tat_010_F", TattooData.ZoneTypes.Torso) },
            { 653, new TattooData("mpvinewood_overlays", "TAT_VW_011", "MP_Vinewood_Tat_011_M", "MP_Vinewood_Tat_011_F", TattooData.ZoneTypes.Torso) },
            { 654, new TattooData("mpvinewood_overlays", "TAT_VW_012", "MP_Vinewood_Tat_012_M", "MP_Vinewood_Tat_012_F", TattooData.ZoneTypes.Torso) },
            { 655, new TattooData("mpvinewood_overlays", "TAT_VW_013", "MP_Vinewood_Tat_013_M", "MP_Vinewood_Tat_013_F", TattooData.ZoneTypes.LeftLeg) },
            { 656, new TattooData("mpvinewood_overlays", "TAT_VW_014", "MP_Vinewood_Tat_014_M", "MP_Vinewood_Tat_014_F", TattooData.ZoneTypes.LeftArm) },
            { 657, new TattooData("mpvinewood_overlays", "TAT_VW_015", "MP_Vinewood_Tat_015_M", "MP_Vinewood_Tat_015_F", TattooData.ZoneTypes.Torso) },
            { 658, new TattooData("mpvinewood_overlays", "TAT_VW_016", "MP_Vinewood_Tat_016_M", "MP_Vinewood_Tat_016_F", TattooData.ZoneTypes.Torso) },
            { 659, new TattooData("mpvinewood_overlays", "TAT_VW_017", "MP_Vinewood_Tat_017_M", "MP_Vinewood_Tat_017_F", TattooData.ZoneTypes.Torso) },
            { 660, new TattooData("mpvinewood_overlays", "TAT_VW_018", "MP_Vinewood_Tat_018_M", "MP_Vinewood_Tat_018_F", TattooData.ZoneTypes.RightArm) },
            { 661, new TattooData("mpvinewood_overlays", "TAT_VW_019", "MP_Vinewood_Tat_019_M", "MP_Vinewood_Tat_019_F", TattooData.ZoneTypes.LeftArm) },
            { 662, new TattooData("mpvinewood_overlays", "TAT_VW_020", "MP_Vinewood_Tat_020_M", "MP_Vinewood_Tat_020_F", TattooData.ZoneTypes.RightLeg) },
            { 663, new TattooData("mpvinewood_overlays", "TAT_VW_021", "MP_Vinewood_Tat_021_M", "MP_Vinewood_Tat_021_F", TattooData.ZoneTypes.Torso) },
            { 664, new TattooData("mpvinewood_overlays", "TAT_VW_022", "MP_Vinewood_Tat_022_M", "MP_Vinewood_Tat_022_F", TattooData.ZoneTypes.Torso) },
            { 665, new TattooData("mpvinewood_overlays", "TAT_VW_023", "MP_Vinewood_Tat_023_M", "MP_Vinewood_Tat_023_F", TattooData.ZoneTypes.Torso) },
            { 666, new TattooData("mpvinewood_overlays", "TAT_VW_024", "MP_Vinewood_Tat_024_M", "MP_Vinewood_Tat_024_F", TattooData.ZoneTypes.Torso) },
            { 667, new TattooData("mpvinewood_overlays", "TAT_VW_025", "MP_Vinewood_Tat_025_M", "MP_Vinewood_Tat_025_F", TattooData.ZoneTypes.RightArm) },
            { 668, new TattooData("mpvinewood_overlays", "TAT_VW_026", "MP_Vinewood_Tat_026_M", "MP_Vinewood_Tat_026_F", TattooData.ZoneTypes.LeftArm) },
            { 669, new TattooData("mpvinewood_overlays", "TAT_VW_027", "MP_Vinewood_Tat_027_M", "MP_Vinewood_Tat_027_F", TattooData.ZoneTypes.LeftLeg) },
            { 670, new TattooData("mpvinewood_overlays", "TAT_VW_028", "MP_Vinewood_Tat_028_M", "MP_Vinewood_Tat_028_F", TattooData.ZoneTypes.RightArm) },
            { 671, new TattooData("mpvinewood_overlays", "TAT_VW_029", "MP_Vinewood_Tat_029_M", "MP_Vinewood_Tat_029_F", TattooData.ZoneTypes.Torso) },
            { 672, new TattooData("mpvinewood_overlays", "TAT_VW_030", "MP_Vinewood_Tat_030_M", "MP_Vinewood_Tat_030_F", TattooData.ZoneTypes.Torso) },
            { 673, new TattooData("mpvinewood_overlays", "TAT_VW_031", "MP_Vinewood_Tat_031_M", "MP_Vinewood_Tat_031_F", TattooData.ZoneTypes.Torso) },
            { 674, new TattooData("mpvinewood_overlays", "TAT_VW_032", "MP_Vinewood_Tat_032_M", "MP_Vinewood_Tat_032_F", TattooData.ZoneTypes.Torso) },

            { 675, new TattooData("multiplayer_overlays", "TAT_FM_008", "FM_Tat_Award_M_000", "FM_Tat_Award_F_000", TattooData.ZoneTypes.Head) },
            { 676, new TattooData("multiplayer_overlays", "TAT_FM_009", "FM_Tat_Award_M_001", "FM_Tat_Award_F_001", TattooData.ZoneTypes.LeftArm) },
            { 677, new TattooData("multiplayer_overlays", "TAT_FM_010", "FM_Tat_Award_M_002", "FM_Tat_Award_F_002", TattooData.ZoneTypes.RightArm) },
            { 678, new TattooData("multiplayer_overlays", "TAT_FM_011", "FM_Tat_Award_M_003", "FM_Tat_Award_F_003", TattooData.ZoneTypes.Torso) },
            { 679, new TattooData("multiplayer_overlays", "TAT_FM_012", "FM_Tat_Award_M_004", "FM_Tat_Award_F_004", TattooData.ZoneTypes.Torso) },
            { 680, new TattooData("multiplayer_overlays", "TAT_FM_013", "FM_Tat_Award_M_005", "FM_Tat_Award_F_005", TattooData.ZoneTypes.Torso) },
            { 681, new TattooData("multiplayer_overlays", "TAT_FM_014", "FM_Tat_Award_M_006", "FM_Tat_Award_F_006", TattooData.ZoneTypes.RightLeg) },
            { 682, new TattooData("multiplayer_overlays", "TAT_FM_015", "FM_Tat_Award_M_007", "FM_Tat_Award_F_007", TattooData.ZoneTypes.LeftArm) },
            { 683, new TattooData("multiplayer_overlays", "TAT_FM_016", "FM_Tat_Award_M_008", "FM_Tat_Award_F_008", TattooData.ZoneTypes.Torso) },
            { 684, new TattooData("multiplayer_overlays", "TAT_FM_017", "FM_Tat_Award_M_009", "FM_Tat_Award_F_009", TattooData.ZoneTypes.LeftLeg) },
            { 685, new TattooData("multiplayer_overlays", "TAT_FM_018", "FM_Tat_Award_M_010", "FM_Tat_Award_F_010", TattooData.ZoneTypes.RightArm) },
            { 686, new TattooData("multiplayer_overlays", "TAT_FM_019", "FM_Tat_Award_M_011", "FM_Tat_Award_F_011", TattooData.ZoneTypes.Torso) },
            { 687, new TattooData("multiplayer_overlays", "TAT_FM_020", "FM_Tat_Award_M_012", "FM_Tat_Award_F_012", TattooData.ZoneTypes.Torso) },
            { 688, new TattooData("multiplayer_overlays", "TAT_FM_021", "FM_Tat_Award_M_013", "FM_Tat_Award_F_013", TattooData.ZoneTypes.Torso) },
            { 689, new TattooData("multiplayer_overlays", "TAT_FM_022", "FM_Tat_Award_M_014", "FM_Tat_Award_F_014", TattooData.ZoneTypes.Torso) },
            { 690, new TattooData("multiplayer_overlays", "TAT_FM_023", "FM_Tat_Award_M_015", "FM_Tat_Award_F_015", TattooData.ZoneTypes.LeftArm) },
            { 691, new TattooData("multiplayer_overlays", "TAT_FM_024", "FM_Tat_Award_M_016", "FM_Tat_Award_F_016", TattooData.ZoneTypes.Torso) },
            { 692, new TattooData("multiplayer_overlays", "TAT_FM_025", "FM_Tat_Award_M_017", "FM_Tat_Award_F_017", TattooData.ZoneTypes.Torso) },
            { 693, new TattooData("multiplayer_overlays", "TAT_FM_026", "FM_Tat_Award_M_018", "FM_Tat_Award_F_018", TattooData.ZoneTypes.Torso) },
            { 694, new TattooData("multiplayer_overlays", "TAT_FM_027", "FM_Tat_Award_M_019", "FM_Tat_Award_F_019", TattooData.ZoneTypes.Torso) },
            { 695, new TattooData("multiplayer_overlays", "TAT_FM_204", "FM_Tat_M_000", "FM_Tat_F_000", TattooData.ZoneTypes.RightArm) },
            { 696, new TattooData("multiplayer_overlays", "TAT_FM_205", "FM_Tat_M_001", "FM_Tat_F_001", TattooData.ZoneTypes.RightArm) },
            { 697, new TattooData("multiplayer_overlays", "TAT_FM_209", "FM_Tat_M_002", "FM_Tat_F_002", TattooData.ZoneTypes.LeftLeg) },
            { 698, new TattooData("multiplayer_overlays", "TAT_FM_206", "FM_Tat_M_003", "FM_Tat_F_003", TattooData.ZoneTypes.RightArm) },
            { 699, new TattooData("multiplayer_overlays", "TAT_FM_219", "FM_Tat_M_004", "FM_Tat_F_004", TattooData.ZoneTypes.Torso) },
            { 700, new TattooData("multiplayer_overlays", "TAT_FM_201", "FM_Tat_M_005", "FM_Tat_F_005", TattooData.ZoneTypes.LeftArm) },
            { 701, new TattooData("multiplayer_overlays", "TAT_FM_202", "FM_Tat_M_006", "FM_Tat_F_006", TattooData.ZoneTypes.LeftArm) },
            { 702, new TattooData("multiplayer_overlays", "TAT_FM_210", "FM_Tat_M_007", "FM_Tat_F_007", TattooData.ZoneTypes.RightLeg) },
            { 703, new TattooData("multiplayer_overlays", "TAT_FM_211", "FM_Tat_M_008", "FM_Tat_F_008", TattooData.ZoneTypes.LeftLeg) },
            { 704, new TattooData("multiplayer_overlays", "TAT_FM_213", "FM_Tat_M_009", "FM_Tat_F_009", TattooData.ZoneTypes.Torso) },
            { 705, new TattooData("multiplayer_overlays", "TAT_FM_218", "FM_Tat_M_010", "FM_Tat_F_010", TattooData.ZoneTypes.Torso) },
            { 706, new TattooData("multiplayer_overlays", "TAT_FM_214", "FM_Tat_M_011", "FM_Tat_F_011", TattooData.ZoneTypes.Torso) },
            { 707, new TattooData("multiplayer_overlays", "TAT_FM_220", "FM_Tat_M_012", "FM_Tat_F_012", TattooData.ZoneTypes.Torso) },
            { 708, new TattooData("multiplayer_overlays", "TAT_FM_215", "FM_Tat_M_013", "FM_Tat_F_013", TattooData.ZoneTypes.Torso) },
            { 709, new TattooData("multiplayer_overlays", "TAT_FM_207", "FM_Tat_M_014", "FM_Tat_F_014", TattooData.ZoneTypes.RightArm) },
            { 710, new TattooData("multiplayer_overlays", "TAT_FM_203", "FM_Tat_M_015", "FM_Tat_F_015", TattooData.ZoneTypes.LeftArm) },
            { 711, new TattooData("multiplayer_overlays", "TAT_FM_216", "FM_Tat_M_016", "FM_Tat_F_016", TattooData.ZoneTypes.Torso) },
            { 712, new TattooData("multiplayer_overlays", "TAT_FM_212", "FM_Tat_M_017", "FM_Tat_F_017", TattooData.ZoneTypes.RightLeg) },
            { 713, new TattooData("multiplayer_overlays", "TAT_FM_208", "FM_Tat_M_018", "FM_Tat_F_018", TattooData.ZoneTypes.RightArm) },
            { 714, new TattooData("multiplayer_overlays", "TAT_FM_217", "FM_Tat_M_019", "FM_Tat_F_019", TattooData.ZoneTypes.Torso) },
            { 715, new TattooData("multiplayer_overlays", "TAT_FM_221", "FM_Tat_M_020", "FM_Tat_F_020", TattooData.ZoneTypes.Torso) },
            { 716, new TattooData("multiplayer_overlays", "TAT_FM_222", "FM_Tat_M_021", "FM_Tat_F_021", TattooData.ZoneTypes.LeftLeg) },
            { 717, new TattooData("multiplayer_overlays", "TAT_FM_223", "FM_Tat_M_022", "FM_Tat_F_022", TattooData.ZoneTypes.RightLeg) },
            { 718, new TattooData("multiplayer_overlays", "TAT_FM_224", "FM_Tat_M_023", "FM_Tat_F_023", TattooData.ZoneTypes.LeftLeg) },
            { 719, new TattooData("multiplayer_overlays", "TAT_FM_225", "FM_Tat_M_024", "FM_Tat_F_024", TattooData.ZoneTypes.Torso) },
            { 720, new TattooData("multiplayer_overlays", "TAT_FM_226", "FM_Tat_M_025", "FM_Tat_F_025", TattooData.ZoneTypes.Torso) },
            { 721, new TattooData("multiplayer_overlays", "TAT_FM_227", "FM_Tat_M_026", "FM_Tat_F_026", TattooData.ZoneTypes.LeftLeg) },
            { 722, new TattooData("multiplayer_overlays", "TAT_FM_228", "FM_Tat_M_027", "FM_Tat_F_027", TattooData.ZoneTypes.RightArm) },
            { 723, new TattooData("multiplayer_overlays", "TAT_FM_229", "FM_Tat_M_028", "FM_Tat_F_028", TattooData.ZoneTypes.RightArm) },
            { 724, new TattooData("multiplayer_overlays", "TAT_FM_230", "FM_Tat_M_029", "FM_Tat_F_029", TattooData.ZoneTypes.Torso) },
            { 725, new TattooData("multiplayer_overlays", "TAT_FM_231", "FM_Tat_M_030", "FM_Tat_F_030", TattooData.ZoneTypes.Torso) },
            { 726, new TattooData("multiplayer_overlays", "TAT_FM_232", "FM_Tat_M_031", "FM_Tat_F_031", TattooData.ZoneTypes.LeftArm) },
            { 727, new TattooData("multiplayer_overlays", "TAT_FM_233", "FM_Tat_M_032", "FM_Tat_F_032", TattooData.ZoneTypes.LeftLeg) },
            { 728, new TattooData("multiplayer_overlays", "TAT_FM_234", "FM_Tat_M_033", "FM_Tat_F_033", TattooData.ZoneTypes.LeftLeg) },
            { 729, new TattooData("multiplayer_overlays", "TAT_FM_235", "FM_Tat_M_034", "FM_Tat_F_034", TattooData.ZoneTypes.Torso) },
            { 730, new TattooData("multiplayer_overlays", "TAT_FM_236", "FM_Tat_M_035", "FM_Tat_F_035", TattooData.ZoneTypes.LeftLeg) },
            { 731, new TattooData("multiplayer_overlays", "TAT_FM_237", "FM_Tat_M_036", "FM_Tat_F_036", TattooData.ZoneTypes.Torso) },
            { 732, new TattooData("multiplayer_overlays", "TAT_FM_238", "FM_Tat_M_037", "FM_Tat_F_037", TattooData.ZoneTypes.LeftLeg) },
            { 733, new TattooData("multiplayer_overlays", "TAT_FM_239", "FM_Tat_M_038", "FM_Tat_F_038", TattooData.ZoneTypes.RightArm) },
            { 734, new TattooData("multiplayer_overlays", "TAT_FM_240", "FM_Tat_M_039", "FM_Tat_F_039", TattooData.ZoneTypes.RightLeg) },
            { 735, new TattooData("multiplayer_overlays", "TAT_FM_241", "FM_Tat_M_040", "FM_Tat_F_040", TattooData.ZoneTypes.RightLeg) },
            { 736, new TattooData("multiplayer_overlays", "TAT_FM_242", "FM_Tat_M_041", "FM_Tat_F_041", TattooData.ZoneTypes.LeftArm) },
            { 737, new TattooData("multiplayer_overlays", "TAT_FM_243", "FM_Tat_M_042", "FM_Tat_F_042", TattooData.ZoneTypes.RightLeg) },
            { 738, new TattooData("multiplayer_overlays", "TAT_FM_244", "FM_Tat_M_043", "FM_Tat_F_043", TattooData.ZoneTypes.RightLeg) },
            { 739, new TattooData("multiplayer_overlays", "TAT_FM_245", "FM_Tat_M_044", "FM_Tat_F_044", TattooData.ZoneTypes.Torso) },
            { 740, new TattooData("multiplayer_overlays", "TAT_FM_246", "FM_Tat_M_045", "FM_Tat_F_045", TattooData.ZoneTypes.Torso) },
            { 741, new TattooData("multiplayer_overlays", "TAT_FM_247", "FM_Tat_M_047", "FM_Tat_F_047", TattooData.ZoneTypes.RightArm) },
        };

        #region Stuff
        public static TattooData GetTattooData(int id) => AllTattoos.GetValueOrDefault(id);
        #region Hair
        public static int GetHair(bool sex, int id)
        {
            var hair = 0;

            if (sex)
                MaleHairs.TryGetValue(id, out hair);
            else
                FemaleHairs.TryGetValue(id, out hair);

            return hair;
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
            var hair = 0;

            if (sex)
                MaleDefaultHairOverlays.TryGetValue(hairId, out hair);
            else
                FemaleDefaultHairOverlays.TryGetValue(hairId, out hair);

            return hair;
        }
        #endregion

        #endregion
    }
}
