using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BCRPServer.Game.Data
{
    public class Customization
    {
        public const int MASKS_1_MALE_COMP_IDX_BASE_OFFSET = 226;
        public const int MASKS_1_FEMALE_COMP_IDX_BASE_OFFSET = 0;

        public const int HAIRS_2_MALE_COMP_IDX_BASE_OFFSET = 0;
        public const int HAIRS_2_FEMALE_COMP_IDX_BASE_OFFSET = 0;

        public const int TORSOS_3_MALE_COMP_IDX_BASE_OFFSET = 0;
        public const int TORSOS_3_FEMALE_COMP_IDX_BASE_OFFSET = 0;

        public const int LEGS_4_MALE_COMP_IDX_BASE_OFFSET = 0;
        public const int LEGS_4_FEMALE_COMP_IDX_BASE_OFFSET = 0;

        public const int BAGS_5_MALE_COMP_IDX_BASE_OFFSET = 111;
        public const int BAGS_5_FEMALE_COMP_IDX_BASE_OFFSET = 111;

        public const int SHOES_6_MALE_COMP_IDX_BASE_OFFSET = 135;
        public const int SHOES_6_FEMALE_COMP_IDX_BASE_OFFSET = 130;

        public const int ACCS_7_MALE_COMP_IDX_BASE_OFFSET = 175;
        public const int ACCS_7_FEMALE_COMP_IDX_BASE_OFFSET = 0;

        public const int UNDERS_8_MALE_COMP_IDX_BASE_OFFSET = 0;
        public const int UNDERS_8_FEMALE_COMP_IDX_BASE_OFFSET = 0;

        public const int ARMOURS_9_MALE_COMP_IDX_BASE_OFFSET = 0;
        public const int ARMOURS_9_FEMALE_COMP_IDX_BASE_OFFSET = 0;

        public const int DECLS_10_MALE_COMP_IDX_BASE_OFFSET = 174;
        public const int DECLS_10_FEMALE_COMP_IDX_BASE_OFFSET = 158;

        public const int TOPS_11_MALE_COMP_IDX_BASE_OFFSET = 495;
        public const int TOPS_11_FEMALE_COMP_IDX_BASE_OFFSET = 473;

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
            Decals = 10,
        }

        public enum AccessoryTypes
        {
            Hat = 0,
            Glasses = 1,
            Earrings = 2,
            Watches = 6,
            Bracelet = 7,
        }

        private static Dictionary<Type, int> ClothesTypesDict = new Dictionary<Type, int>()
        {
            { typeof(Items.Top), (int)ClothesTypes.Top },
            { typeof(Items.Under), (int)ClothesTypes.Under },
            { typeof(Items.Pants), (int)ClothesTypes.Pants },
            { typeof(Items.Shoes), (int)ClothesTypes.Shoes },
            { typeof(Items.Gloves), (int)ClothesTypes.Gloves },
            { typeof(Items.Mask), (int)ClothesTypes.Mask },
            { typeof(Items.Accessory), (int)ClothesTypes.Accessory },
            { typeof(Items.Bag), (int)ClothesTypes.Bag },
            { typeof(Items.Armour), (int)ClothesTypes.Armour },
            { typeof(Items.Holster), (int)ClothesTypes.Decals },

            { typeof(Items.Hat), (int)AccessoryTypes.Hat },
            { typeof(Items.Glasses), (int)AccessoryTypes.Glasses },
            { typeof(Items.Earrings), (int)AccessoryTypes.Earrings },
            { typeof(Items.Watches), (int)AccessoryTypes.Watches },
            { typeof(Items.Bracelet), (int)AccessoryTypes.Bracelet },
        };

        public static int GetClothesIdxByType(Type type) => ClothesTypesDict.GetValueOrDefault(type);

        public enum UniformTypes
        {
            Farmer = 0,

            FractionPaletoPolice_0,
            FractionPaletoPolice_1,
            FractionPaletoPolice_2,

            FractionPaletoEMS_0,
            FractionPaletoEMS_1,
            FractionPaletoEMS_2,
        }

        // if >= 1000 -> prop
        public static Dictionary<UniformTypes, Dictionary<int, Tuple<int, int>[]>> Uniforms = new Dictionary<UniformTypes, Dictionary<int, Tuple<int, int>[]>>()
        {
            {
                UniformTypes.Farmer,

                new Dictionary<int, Tuple<int, int>[]>()
                {
                    { 11, new Tuple<int, int>[] { new Tuple<int, int>(43, 0), new Tuple<int, int>(86, 0) } },
                    { 4, new Tuple<int, int>[] { new Tuple<int, int>(43, 0), new Tuple<int, int>(25, 0) } },
                    { 8, new Tuple<int, int>[] { new Tuple<int, int>(15, 0), new Tuple<int, int>(14, 0) } },
                    { 6, new Tuple<int, int>[] { new Tuple<int, int>(27, 0), new Tuple<int, int>(26, 0) } },
                    { 3, new Tuple<int, int>[] { new Tuple<int, int>(70, 0), new Tuple<int, int>(0, 0) } },
                    { 7, new Tuple<int, int>[] { new Tuple<int, int>(0, 0), new Tuple<int, int>(0, 0) } },
                }
            },

            {
                UniformTypes.FractionPaletoPolice_0,

                new Dictionary<int, Tuple<int, int>[]>()
                {
                    { 11, new Tuple<int, int>[] { new Tuple<int, int>(318, 3), new Tuple<int, int>(0, 0) } },
                    { 4, new Tuple<int, int>[] { new Tuple<int, int>(141, 6), new Tuple<int, int>(0, 0) } },
                    { 8, new Tuple<int, int>[] { new Tuple<int, int>(153, 0), new Tuple<int, int>(0, 0) } },
                    { 6, new Tuple<int, int>[] { new Tuple<int, int>(10, 0), new Tuple<int, int>(0, 0) } },
                    { 3, new Tuple<int, int>[] { new Tuple<int, int>(11, 0), new Tuple<int, int>(0, 0) } },
                    { 7, new Tuple<int, int>[] { new Tuple<int, int>(38, 0), new Tuple<int, int>(0, 0) } },
                }
            },

            {
                UniformTypes.FractionPaletoPolice_1,

                new Dictionary<int, Tuple<int, int>[]>()
                {
                    { 11, new Tuple<int, int>[] { new Tuple<int, int>(50, 0), new Tuple<int, int>(0, 0) } },
                    { 4, new Tuple<int, int>[] { new Tuple<int, int>(130, 1), new Tuple<int, int>(0, 0) } },
                    { 8, new Tuple<int, int>[] { new Tuple<int, int>(129, 0), new Tuple<int, int>(0, 0) } },
                    { 6, new Tuple<int, int>[] { new Tuple<int, int>(60, 0), new Tuple<int, int>(0, 0) } },
                    { 3, new Tuple<int, int>[] { new Tuple<int, int>(17, 0), new Tuple<int, int>(0, 0) } },
                    { 7, new Tuple<int, int>[] { new Tuple<int, int>(128, 0), new Tuple<int, int>(0, 0) } },
                    { 1, new Tuple<int, int>[] { new Tuple<int, int>(130, 0), new Tuple<int, int>(0, 0) } },

                    { 1000 + 0, new Tuple<int, int>[] { new Tuple<int, int>(150, 0), new Tuple<int, int>(-1, 0) } },
                    { 1000 + 1, new Tuple<int, int>[] { new Tuple<int, int>(-1, 0), new Tuple<int, int>(-1, 0) } },
                }
            },

            {
                UniformTypes.FractionPaletoPolice_2,

                new Dictionary<int, Tuple<int, int>[]>()
                {
                    { 11, new Tuple<int, int>[] { new Tuple<int, int>(25, 3), new Tuple<int, int>(0, 0) } },
                    { 4, new Tuple<int, int>[] { new Tuple<int, int>(24, 6), new Tuple<int, int>(0, 0) } },
                    { 8, new Tuple<int, int>[] { new Tuple<int, int>(6, 0), new Tuple<int, int>(0, 0) } },
                    { 6, new Tuple<int, int>[] { new Tuple<int, int>(20, 0), new Tuple<int, int>(0, 0) } },
                    { 3, new Tuple<int, int>[] { new Tuple<int, int>(11, 0), new Tuple<int, int>(0, 0) } },
                    { 7, new Tuple<int, int>[] { new Tuple<int, int>(24, 2), new Tuple<int, int>(0, 0) } },
                }
            },

            {
                UniformTypes.FractionPaletoEMS_0,

                new Dictionary<int, Tuple<int, int>[]>()
                {
                    { 11, new Tuple<int, int>[] { new Tuple<int, int>(TOPS_11_MALE_COMP_IDX_BASE_OFFSET + 0, 0), new Tuple<int, int>(0, 0) } },
                    { 4, new Tuple<int, int>[] { new Tuple<int, int>(28, 8), new Tuple<int, int>(0, 0) } },
                    { 8, new Tuple<int, int>[] { new Tuple<int, int>(11, 7), new Tuple<int, int>(0, 0) } },
                    { 6, new Tuple<int, int>[] { new Tuple<int, int>(7, 0), new Tuple<int, int>(0, 0) } },
                    { 3, new Tuple<int, int>[] { new Tuple<int, int>(1, 0), new Tuple<int, int>(0, 0) } },
                    { 7, new Tuple<int, int>[] { new Tuple<int, int>(ACCS_7_MALE_COMP_IDX_BASE_OFFSET + 0, 0), new Tuple<int, int>(0, 0) } },
                }
            },

            {
                UniformTypes.FractionPaletoEMS_1,

                new Dictionary<int, Tuple<int, int>[]>()
                {
                    { 11, new Tuple<int, int>[] { new Tuple<int, int>(319, 0), new Tuple<int, int>(0, 0) } },
                    { 4, new Tuple<int, int>[] { new Tuple<int, int>(35, 0), new Tuple<int, int>(0, 0) } },
                    { 8, new Tuple<int, int>[] { new Tuple<int, int>(154, 0), new Tuple<int, int>(0, 0) } },
                    { 6, new Tuple<int, int>[] { new Tuple<int, int>(15, 0), new Tuple<int, int>(0, 0) } },
                    { 3, new Tuple<int, int>[] { new Tuple<int, int>(92, 0), new Tuple<int, int>(0, 0) } },
                    { 7, new Tuple<int, int>[] { new Tuple<int, int>(126, 0), new Tuple<int, int>(0, 0) } },
                }
            },

            {
                UniformTypes.FractionPaletoEMS_2,

                new Dictionary<int, Tuple<int, int>[]>()
                {
                    { 11, new Tuple<int, int>[] { new Tuple<int, int>(TOPS_11_MALE_COMP_IDX_BASE_OFFSET + 0, 0), new Tuple<int, int>(0, 0) } },
                    { 7, new Tuple<int, int>[] { new Tuple<int, int>(ACCS_7_MALE_COMP_IDX_BASE_OFFSET + 0, 0), new Tuple<int, int>(0, 0) } },
                }
            },
        };

        public static void ApplyUniform(PlayerData pData, UniformTypes uType)
        {
            var data = Uniforms.GetValueOrDefault(uType);

            if (data == null)
                return;

            var idx = pData.Sex ? 0 : 1;

            foreach (var x in data)
            {
                if (x.Key >= 1000)
                {
                    if (x.Value[idx].Item1 < 0)
                    {
                        pData.Player.ClearAccessory(x.Key - 1000);

                        if (x.Key == 1000)
                            pData.Hat = null;
                    }
                    else
                    {
                        pData.Player.SetAccessories(x.Key - 1000, x.Value[idx].Item1, x.Value[idx].Item2);

                        if (x.Key == 1000)
                            pData.Hat = $"{x.Value[idx].Item1}|{x.Value[idx].Item2}";
                    }
                }
                else
                {
                    pData.Player.SetClothes(x.Key, x.Value[idx].Item1, x.Value[idx].Item2);
                }
            }

            pData.CurrentUniform = uType;
        }

        public static bool IsUniformElementActive(PlayerData pData, int elementIdx, bool notifyIf)
        {
            if (pData.CurrentUniform is UniformTypes uType)
            {
                var data = Uniforms.GetValueOrDefault(uType);

                if (data == null)
                    return false;

                if (data.ContainsKey(elementIdx))
                {
                    if (notifyIf)
                    {
                        pData.Player.Notify("Inv::CCWUA");
                    }

                    return true;
                }
            }

            return false;
        }

        public static bool IsInUniform(PlayerData pData, bool notifyIf)
        {
            if (pData.CurrentUniform == null)
                return false;

            if (notifyIf)
            {
                pData.Player.Notify("Inv::CDTWUA");
            }

            return true;
        }

        public static void SetNoUniform(PlayerData pData)
        {
            pData.CurrentUniform = null;

            pData.UpdateClothes();
        }

        public class TattooData
        {
            public enum ZoneTypes
            {
                Face = 0,
                Temple,
                Neck,
                Mouth,

                Chest,
                Stomath,
                Back,

                LeftArm,
                RightArm,

                LeftLeg,
                RightLeg,
            }

            public ZoneTypes ZoneType { get; private set; }

            public TattooData(ZoneTypes ZoneType)
            {
                this.ZoneType = ZoneType;
            }
        }

        private static Dictionary<int, TattooData> AllTattoos = new Dictionary<int, TattooData>()
        {
            { 0, new TattooData(TattooData.ZoneTypes.Chest) },
            { 1, new TattooData(TattooData.ZoneTypes.Back) },
            { 2, new TattooData(TattooData.ZoneTypes.Back) },
            { 3, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 4, new TattooData(TattooData.ZoneTypes.Back) },
            { 5, new TattooData(TattooData.ZoneTypes.Back) },
            { 6, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 7, new TattooData(TattooData.ZoneTypes.Back) },
            { 8, new TattooData(TattooData.ZoneTypes.Back) },
            { 9, new TattooData(TattooData.ZoneTypes.Chest) },
            { 10, new TattooData(TattooData.ZoneTypes.Chest) },
            { 11, new TattooData(TattooData.ZoneTypes.Face) },
            { 12, new TattooData(TattooData.ZoneTypes.Temple) },
            { 13, new TattooData(TattooData.ZoneTypes.Temple) },
            { 14, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 15, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 16, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 17, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 18, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 19, new TattooData(TattooData.ZoneTypes.Neck) },
            { 20, new TattooData(TattooData.ZoneTypes.Neck) },
            { 21, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 22, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 23, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 24, new TattooData(TattooData.ZoneTypes.Back) },
            { 25, new TattooData(TattooData.ZoneTypes.Back) },
            { 26, new TattooData(TattooData.ZoneTypes.Back) },
            { 27, new TattooData(TattooData.ZoneTypes.Chest) },
            { 28, new TattooData(TattooData.ZoneTypes.Chest) },
            { 29, new TattooData(TattooData.ZoneTypes.Chest) },
            { 30, new TattooData(TattooData.ZoneTypes.Chest) },
            { 31, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 32, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 33, new TattooData(TattooData.ZoneTypes.Neck) },
            { 34, new TattooData(TattooData.ZoneTypes.Back) },
            { 35, new TattooData(TattooData.ZoneTypes.Back) },
            { 36, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 37, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 38, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 39, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 40, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 41, new TattooData(TattooData.ZoneTypes.Chest) },
            { 42, new TattooData(TattooData.ZoneTypes.Chest) },
            { 43, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 44, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 45, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 46, new TattooData(TattooData.ZoneTypes.Chest) },
            { 47, new TattooData(TattooData.ZoneTypes.Back) },
            { 48, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 49, new TattooData(TattooData.ZoneTypes.Back) },
            { 50, new TattooData(TattooData.ZoneTypes.Face) },
            { 51, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 52, new TattooData(TattooData.ZoneTypes.Back) },
            { 53, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 54, new TattooData(TattooData.ZoneTypes.Chest) },
            { 55, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 56, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 57, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 58, new TattooData(TattooData.ZoneTypes.Back) },
            { 59, new TattooData(TattooData.ZoneTypes.Chest) },
            { 60, new TattooData(TattooData.ZoneTypes.Chest) },
            { 61, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 62, new TattooData(TattooData.ZoneTypes.Back) },
            { 63, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 64, new TattooData(TattooData.ZoneTypes.Chest) },
            { 65, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 66, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 67, new TattooData(TattooData.ZoneTypes.Chest) },
            { 68, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 69, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 70, new TattooData(TattooData.ZoneTypes.Chest) },
            { 71, new TattooData(TattooData.ZoneTypes.Back) },
            { 72, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 73, new TattooData(TattooData.ZoneTypes.Chest) },
            { 74, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 75, new TattooData(TattooData.ZoneTypes.Chest) },
            { 76, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 77, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 78, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 79, new TattooData(TattooData.ZoneTypes.Neck) },
            { 80, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 81, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 82, new TattooData(TattooData.ZoneTypes.Chest) },
            { 83, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 84, new TattooData(TattooData.ZoneTypes.Back) },
            { 85, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 86, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 87, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 88, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 89, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 90, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 91, new TattooData(TattooData.ZoneTypes.Chest) },
            { 92, new TattooData(TattooData.ZoneTypes.Neck) },
            { 93, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 94, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 95, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 96, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 97, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 98, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 99, new TattooData(TattooData.ZoneTypes.Chest) },
            { 100, new TattooData(TattooData.ZoneTypes.Chest) },
            { 101, new TattooData(TattooData.ZoneTypes.Chest) },
            { 102, new TattooData(TattooData.ZoneTypes.Neck) },
            { 103, new TattooData(TattooData.ZoneTypes.Neck) },
            { 104, new TattooData(TattooData.ZoneTypes.Neck) },
            { 105, new TattooData(TattooData.ZoneTypes.Neck) },
            { 106, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 107, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 108, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 109, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 110, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 111, new TattooData(TattooData.ZoneTypes.Chest) },
            { 112, new TattooData(TattooData.ZoneTypes.Chest) },
            { 113, new TattooData(TattooData.ZoneTypes.Back) },
            { 114, new TattooData(TattooData.ZoneTypes.Chest) },
            { 115, new TattooData(TattooData.ZoneTypes.Chest) },
            { 116, new TattooData(TattooData.ZoneTypes.Chest) },
            { 117, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 118, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 119, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 120, new TattooData(TattooData.ZoneTypes.Back) },
            { 121, new TattooData(TattooData.ZoneTypes.Back) },
            { 122, new TattooData(TattooData.ZoneTypes.Neck) },
            { 123, new TattooData(TattooData.ZoneTypes.Neck) },
            { 124, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 125, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 126, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 127, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 128, new TattooData(TattooData.ZoneTypes.Chest) },
            { 129, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 130, new TattooData(TattooData.ZoneTypes.Back) },
            { 131, new TattooData(TattooData.ZoneTypes.Chest) },
            { 132, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 133, new TattooData(TattooData.ZoneTypes.Back) },
            { 134, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 135, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 136, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 137, new TattooData(TattooData.ZoneTypes.Back) },
            { 138, new TattooData(TattooData.ZoneTypes.Back) },
            { 139, new TattooData(TattooData.ZoneTypes.Back) },
            { 140, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 141, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 142, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 143, new TattooData(TattooData.ZoneTypes.Back) },
            { 144, new TattooData(TattooData.ZoneTypes.Back) },
            { 145, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 146, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 147, new TattooData(TattooData.ZoneTypes.Chest) },
            { 148, new TattooData(TattooData.ZoneTypes.Chest) },
            { 149, new TattooData(TattooData.ZoneTypes.Back) },
            { 150, new TattooData(TattooData.ZoneTypes.Back) },
            { 151, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 152, new TattooData(TattooData.ZoneTypes.Back) },
            { 153, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 154, new TattooData(TattooData.ZoneTypes.Chest) },
            { 155, new TattooData(TattooData.ZoneTypes.Back) },
            { 156, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 157, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 158, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 159, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 160, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 161, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 162, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 163, new TattooData(TattooData.ZoneTypes.Back) },
            { 164, new TattooData(TattooData.ZoneTypes.Back) },
            { 165, new TattooData(TattooData.ZoneTypes.Neck) },
            { 166, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 167, new TattooData(TattooData.ZoneTypes.Chest) },
            { 168, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 169, new TattooData(TattooData.ZoneTypes.Back) },
            { 170, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 171, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 172, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 173, new TattooData(TattooData.ZoneTypes.Back) },
            { 174, new TattooData(TattooData.ZoneTypes.Chest) },
            { 175, new TattooData(TattooData.ZoneTypes.Chest) },
            { 176, new TattooData(TattooData.ZoneTypes.Chest) },
            { 177, new TattooData(TattooData.ZoneTypes.Chest) },
            { 178, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 179, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 180, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 181, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 182, new TattooData(TattooData.ZoneTypes.Neck) },
            { 183, new TattooData(TattooData.ZoneTypes.Neck) },
            { 184, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 185, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 186, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 187, new TattooData(TattooData.ZoneTypes.Neck) },
            { 188, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 189, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 190, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 191, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 192, new TattooData(TattooData.ZoneTypes.Chest) },
            { 193, new TattooData(TattooData.ZoneTypes.Chest) },
            { 194, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 195, new TattooData(TattooData.ZoneTypes.Back) },
            { 196, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 197, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 198, new TattooData(TattooData.ZoneTypes.Neck) },
            { 199, new TattooData(TattooData.ZoneTypes.Neck) },
            { 200, new TattooData(TattooData.ZoneTypes.Neck) },
            { 201, new TattooData(TattooData.ZoneTypes.Neck) },
            { 202, new TattooData(TattooData.ZoneTypes.Back) },
            { 203, new TattooData(TattooData.ZoneTypes.Chest) },
            { 204, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 205, new TattooData(TattooData.ZoneTypes.Chest) },
            { 206, new TattooData(TattooData.ZoneTypes.Chest) },
            { 207, new TattooData(TattooData.ZoneTypes.Chest) },
            { 208, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 209, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 210, new TattooData(TattooData.ZoneTypes.Back) },
            { 211, new TattooData(TattooData.ZoneTypes.Back) },
            { 212, new TattooData(TattooData.ZoneTypes.Back) },
            { 213, new TattooData(TattooData.ZoneTypes.Back) },
            { 214, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 215, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 216, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 217, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 218, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 219, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 220, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 221, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 222, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 223, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 224, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 225, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 226, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 227, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 228, new TattooData(TattooData.ZoneTypes.Chest) },
            { 229, new TattooData(TattooData.ZoneTypes.Chest) },
            { 230, new TattooData(TattooData.ZoneTypes.Chest) },
            { 231, new TattooData(TattooData.ZoneTypes.Chest) },
            { 232, new TattooData(TattooData.ZoneTypes.Chest) },
            { 233, new TattooData(TattooData.ZoneTypes.Chest) },
            { 234, new TattooData(TattooData.ZoneTypes.Chest) },
            { 235, new TattooData(TattooData.ZoneTypes.Chest) },
            { 236, new TattooData(TattooData.ZoneTypes.Chest) },
            { 237, new TattooData(TattooData.ZoneTypes.Chest) },
            { 238, new TattooData(TattooData.ZoneTypes.Chest) },
            { 239, new TattooData(TattooData.ZoneTypes.Chest) },
            { 240, new TattooData(TattooData.ZoneTypes.Back) },
            { 241, new TattooData(TattooData.ZoneTypes.Back) },
            { 242, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 243, new TattooData(TattooData.ZoneTypes.Neck) },
            { 244, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 245, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 246, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 247, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 248, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 249, new TattooData(TattooData.ZoneTypes.Back) },
            { 250, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 251, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 252, new TattooData(TattooData.ZoneTypes.Chest) },
            { 253, new TattooData(TattooData.ZoneTypes.Back) },
            { 254, new TattooData(TattooData.ZoneTypes.Back) },
            { 255, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 256, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 257, new TattooData(TattooData.ZoneTypes.Chest) },
            { 258, new TattooData(TattooData.ZoneTypes.Back) },
            { 259, new TattooData(TattooData.ZoneTypes.Back) },
            { 260, new TattooData(TattooData.ZoneTypes.Chest) },
            { 261, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 262, new TattooData(TattooData.ZoneTypes.Back) },
            { 263, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 264, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 265, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 266, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 267, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 268, new TattooData(TattooData.ZoneTypes.Chest) },
            { 269, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 270, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 271, new TattooData(TattooData.ZoneTypes.Face) },
            { 272, new TattooData(TattooData.ZoneTypes.Face) },
            { 273, new TattooData(TattooData.ZoneTypes.Neck) },
            { 274, new TattooData(TattooData.ZoneTypes.Face) },
            { 275, new TattooData(TattooData.ZoneTypes.Face) },
            { 276, new TattooData(TattooData.ZoneTypes.Face) },
            { 277, new TattooData(TattooData.ZoneTypes.Face) },
            { 278, new TattooData(TattooData.ZoneTypes.Face) },
            { 279, new TattooData(TattooData.ZoneTypes.Neck) },
            { 280, new TattooData(TattooData.ZoneTypes.Face) },
            { 281, new TattooData(TattooData.ZoneTypes.Face) },
            { 282, new TattooData(TattooData.ZoneTypes.Face) },
            { 283, new TattooData(TattooData.ZoneTypes.Face) },
            { 284, new TattooData(TattooData.ZoneTypes.Face) },
            { 285, new TattooData(TattooData.ZoneTypes.Neck) },
            { 286, new TattooData(TattooData.ZoneTypes.Face) },
            { 287, new TattooData(TattooData.ZoneTypes.Face) },
            { 288, new TattooData(TattooData.ZoneTypes.Neck) },
            { 289, new TattooData(TattooData.ZoneTypes.Mouth) },
            { 290, new TattooData(TattooData.ZoneTypes.Neck) },
            { 291, new TattooData(TattooData.ZoneTypes.Face) },
            { 292, new TattooData(TattooData.ZoneTypes.Face) },
            { 293, new TattooData(TattooData.ZoneTypes.Face) },
            { 294, new TattooData(TattooData.ZoneTypes.Back) },
            { 295, new TattooData(TattooData.ZoneTypes.Back) },
            { 296, new TattooData(TattooData.ZoneTypes.Chest) },
            { 297, new TattooData(TattooData.ZoneTypes.Chest) },
            { 298, new TattooData(TattooData.ZoneTypes.Back) },
            { 299, new TattooData(TattooData.ZoneTypes.Back) },
            { 300, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 301, new TattooData(TattooData.ZoneTypes.Back) },
            { 302, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 303, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 304, new TattooData(TattooData.ZoneTypes.Chest) },
            { 305, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 306, new TattooData(TattooData.ZoneTypes.Chest) },
            { 307, new TattooData(TattooData.ZoneTypes.Back) },
            { 308, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 309, new TattooData(TattooData.ZoneTypes.Back) },
            { 310, new TattooData(TattooData.ZoneTypes.Back) },
            { 311, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 312, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 313, new TattooData(TattooData.ZoneTypes.Face) },
            { 314, new TattooData(TattooData.ZoneTypes.Face) },
            { 315, new TattooData(TattooData.ZoneTypes.Face) },
            { 316, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 317, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 318, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 319, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 320, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 321, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 322, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 323, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 324, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 325, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 326, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 327, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 328, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 329, new TattooData(TattooData.ZoneTypes.Back) },
            { 330, new TattooData(TattooData.ZoneTypes.Back) },
            { 331, new TattooData(TattooData.ZoneTypes.Back) },
            { 332, new TattooData(TattooData.ZoneTypes.Back) },
            { 333, new TattooData(TattooData.ZoneTypes.Back) },
            { 334, new TattooData(TattooData.ZoneTypes.Back) },
            { 335, new TattooData(TattooData.ZoneTypes.Back) },
            { 336, new TattooData(TattooData.ZoneTypes.Back) },
            { 337, new TattooData(TattooData.ZoneTypes.Back) },
            { 338, new TattooData(TattooData.ZoneTypes.Chest) },
            { 339, new TattooData(TattooData.ZoneTypes.Chest) },
            { 340, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 341, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 342, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 343, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 344, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 345, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 346, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 347, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 348, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 349, new TattooData(TattooData.ZoneTypes.Back) },
            { 350, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 351, new TattooData(TattooData.ZoneTypes.Chest) },
            { 352, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 353, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 354, new TattooData(TattooData.ZoneTypes.Neck) },
            { 355, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 356, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 357, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 358, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 359, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 360, new TattooData(TattooData.ZoneTypes.Back) },
            { 361, new TattooData(TattooData.ZoneTypes.Back) },
            { 362, new TattooData(TattooData.ZoneTypes.Chest) },
            { 363, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 364, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 365, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 366, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 367, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 368, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 369, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 370, new TattooData(TattooData.ZoneTypes.Neck) },
            { 371, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 372, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 373, new TattooData(TattooData.ZoneTypes.Back) },
            { 374, new TattooData(TattooData.ZoneTypes.Back) },
            { 375, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 376, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 377, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 378, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 379, new TattooData(TattooData.ZoneTypes.Back) },
            { 380, new TattooData(TattooData.ZoneTypes.Back) },
            { 381, new TattooData(TattooData.ZoneTypes.Back) },
            { 382, new TattooData(TattooData.ZoneTypes.Chest) },
            { 383, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 384, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 385, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 386, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 387, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 388, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 389, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 390, new TattooData(TattooData.ZoneTypes.Back) },
            { 391, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 392, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 393, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 394, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 395, new TattooData(TattooData.ZoneTypes.Back) },
            { 396, new TattooData(TattooData.ZoneTypes.Chest) },
            { 397, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 398, new TattooData(TattooData.ZoneTypes.Back) },
            { 399, new TattooData(TattooData.ZoneTypes.Back) },
            { 400, new TattooData(TattooData.ZoneTypes.Back) },
            { 401, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 402, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 403, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 404, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 405, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 406, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 407, new TattooData(TattooData.ZoneTypes.Back) },
            { 408, new TattooData(TattooData.ZoneTypes.Back) },
            { 409, new TattooData(TattooData.ZoneTypes.Back) },
            { 410, new TattooData(TattooData.ZoneTypes.Back) },
            { 411, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 412, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 413, new TattooData(TattooData.ZoneTypes.Back) },
            { 414, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 415, new TattooData(TattooData.ZoneTypes.Chest) },
            { 416, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 417, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 418, new TattooData(TattooData.ZoneTypes.Chest) },
            { 419, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 420, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 421, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 422, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 423, new TattooData(TattooData.ZoneTypes.Back) },
            { 424, new TattooData(TattooData.ZoneTypes.Back) },
            { 425, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 426, new TattooData(TattooData.ZoneTypes.Chest) },
            { 427, new TattooData(TattooData.ZoneTypes.Chest) },
            { 428, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 429, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 430, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 431, new TattooData(TattooData.ZoneTypes.Back) },
            { 432, new TattooData(TattooData.ZoneTypes.Back) },
            { 433, new TattooData(TattooData.ZoneTypes.Chest) },
            { 434, new TattooData(TattooData.ZoneTypes.Back) },
            { 435, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 436, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 437, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 438, new TattooData(TattooData.ZoneTypes.Back) },
            { 439, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 440, new TattooData(TattooData.ZoneTypes.Chest) },
            { 441, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 442, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 443, new TattooData(TattooData.ZoneTypes.Chest) },
            { 444, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 445, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 446, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 447, new TattooData(TattooData.ZoneTypes.Chest) },
            { 448, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 449, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 450, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 451, new TattooData(TattooData.ZoneTypes.Back) },
            { 452, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 453, new TattooData(TattooData.ZoneTypes.Chest) },
            { 454, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 455, new TattooData(TattooData.ZoneTypes.Chest) },
            { 456, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 457, new TattooData(TattooData.ZoneTypes.Back) },
            { 458, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 459, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 460, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 461, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 462, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 463, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 464, new TattooData(TattooData.ZoneTypes.Back) },
            { 465, new TattooData(TattooData.ZoneTypes.Chest) },
            { 466, new TattooData(TattooData.ZoneTypes.Chest) },
            { 467, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 468, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 469, new TattooData(TattooData.ZoneTypes.Chest) },
            { 470, new TattooData(TattooData.ZoneTypes.Chest) },
            { 471, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 472, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 473, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 474, new TattooData(TattooData.ZoneTypes.Back) },
            { 475, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 476, new TattooData(TattooData.ZoneTypes.Face) },
            { 477, new TattooData(TattooData.ZoneTypes.Face) },
            { 478, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 479, new TattooData(TattooData.ZoneTypes.Back) },
            { 480, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 481, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 482, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 483, new TattooData(TattooData.ZoneTypes.Back) },
            { 484, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 485, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 486, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 487, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 488, new TattooData(TattooData.ZoneTypes.Back) },
            { 489, new TattooData(TattooData.ZoneTypes.Back) },
            { 490, new TattooData(TattooData.ZoneTypes.Back) },
            { 491, new TattooData(TattooData.ZoneTypes.Chest) },
            { 492, new TattooData(TattooData.ZoneTypes.Chest) },
            { 493, new TattooData(TattooData.ZoneTypes.Chest) },
            { 494, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 495, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 496, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 497, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 498, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 499, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 500, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 501, new TattooData(TattooData.ZoneTypes.Back) },
            { 502, new TattooData(TattooData.ZoneTypes.Neck) },
            { 503, new TattooData(TattooData.ZoneTypes.Chest) },
            { 504, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 505, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 506, new TattooData(TattooData.ZoneTypes.Back) },
            { 507, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 508, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 509, new TattooData(TattooData.ZoneTypes.Back) },
            { 510, new TattooData(TattooData.ZoneTypes.Chest) },
            { 511, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 512, new TattooData(TattooData.ZoneTypes.Back) },
            { 513, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 514, new TattooData(TattooData.ZoneTypes.Neck) },
            { 515, new TattooData(TattooData.ZoneTypes.Neck) },
            { 516, new TattooData(TattooData.ZoneTypes.Back) },
            { 517, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 518, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 519, new TattooData(TattooData.ZoneTypes.Back) },
            { 520, new TattooData(TattooData.ZoneTypes.Back) },
            { 521, new TattooData(TattooData.ZoneTypes.Back) },
            { 522, new TattooData(TattooData.ZoneTypes.Chest) },
            { 523, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 524, new TattooData(TattooData.ZoneTypes.Chest) },
            { 525, new TattooData(TattooData.ZoneTypes.Back) },
            { 526, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 527, new TattooData(TattooData.ZoneTypes.Back) },
            { 528, new TattooData(TattooData.ZoneTypes.Back) },
            { 529, new TattooData(TattooData.ZoneTypes.Neck) },
            { 530, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 531, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 532, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 533, new TattooData(TattooData.ZoneTypes.Face) },
            { 534, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 535, new TattooData(TattooData.ZoneTypes.Neck) },
            { 536, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 537, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 538, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 539, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 540, new TattooData(TattooData.ZoneTypes.Chest) },
            { 541, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 542, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 543, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 544, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 545, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 546, new TattooData(TattooData.ZoneTypes.Neck) },
            { 547, new TattooData(TattooData.ZoneTypes.Chest) },
            { 548, new TattooData(TattooData.ZoneTypes.Chest) },
            { 549, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 550, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 551, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 552, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 553, new TattooData(TattooData.ZoneTypes.Back) },
            { 554, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 555, new TattooData(TattooData.ZoneTypes.Back) },
            { 556, new TattooData(TattooData.ZoneTypes.Chest) },
            { 557, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 558, new TattooData(TattooData.ZoneTypes.Back) },
            { 559, new TattooData(TattooData.ZoneTypes.Back) },
            { 560, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 561, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 562, new TattooData(TattooData.ZoneTypes.Chest) },
            { 563, new TattooData(TattooData.ZoneTypes.Back) },
            { 564, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 565, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 566, new TattooData(TattooData.ZoneTypes.Back) },
            { 567, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 568, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 569, new TattooData(TattooData.ZoneTypes.Back) },
            { 570, new TattooData(TattooData.ZoneTypes.Back) },
            { 571, new TattooData(TattooData.ZoneTypes.Neck) },
            { 572, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 573, new TattooData(TattooData.ZoneTypes.Chest) },
            { 574, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 575, new TattooData(TattooData.ZoneTypes.Back) },
            { 576, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 577, new TattooData(TattooData.ZoneTypes.Back) },
            { 578, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 579, new TattooData(TattooData.ZoneTypes.Face) },
            { 580, new TattooData(TattooData.ZoneTypes.Face) },
            { 581, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 582, new TattooData(TattooData.ZoneTypes.Chest) },
            { 583, new TattooData(TattooData.ZoneTypes.Back) },
            { 584, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 585, new TattooData(TattooData.ZoneTypes.Back) },
            { 586, new TattooData(TattooData.ZoneTypes.Back) },
            { 587, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 588, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 589, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 590, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 591, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 592, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 593, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 594, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 595, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 596, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 597, new TattooData(TattooData.ZoneTypes.Face) },
            { 598, new TattooData(TattooData.ZoneTypes.Face) },
            { 599, new TattooData(TattooData.ZoneTypes.Face) },
            { 600, new TattooData(TattooData.ZoneTypes.Face) },
            { 601, new TattooData(TattooData.ZoneTypes.Face) },
            { 602, new TattooData(TattooData.ZoneTypes.Mouth) },
            { 603, new TattooData(TattooData.ZoneTypes.Face) },
            { 604, new TattooData(TattooData.ZoneTypes.Neck) },
            { 605, new TattooData(TattooData.ZoneTypes.Neck) },
            { 606, new TattooData(TattooData.ZoneTypes.Neck) },
            { 607, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 608, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 609, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 610, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 611, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 612, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 613, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 614, new TattooData(TattooData.ZoneTypes.Back) },
            { 615, new TattooData(TattooData.ZoneTypes.Back) },
            { 616, new TattooData(TattooData.ZoneTypes.Back) },
            { 617, new TattooData(TattooData.ZoneTypes.Back) },
            { 618, new TattooData(TattooData.ZoneTypes.Back) },
            { 619, new TattooData(TattooData.ZoneTypes.Chest) },
            { 620, new TattooData(TattooData.ZoneTypes.Chest) },
            { 621, new TattooData(TattooData.ZoneTypes.Chest) },
            { 622, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 623, new TattooData(TattooData.ZoneTypes.Chest) },
            { 624, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 625, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 626, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 627, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 628, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 629, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 630, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 631, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 632, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 633, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 634, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 635, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 636, new TattooData(TattooData.ZoneTypes.Back) },
            { 637, new TattooData(TattooData.ZoneTypes.Chest) },
            { 638, new TattooData(TattooData.ZoneTypes.Chest) },
            { 639, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 640, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 641, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 642, new TattooData(TattooData.ZoneTypes.Chest) },
            { 643, new TattooData(TattooData.ZoneTypes.Back) },
            { 644, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 645, new TattooData(TattooData.ZoneTypes.Chest) },
            { 646, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 647, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 648, new TattooData(TattooData.ZoneTypes.Back) },
            { 649, new TattooData(TattooData.ZoneTypes.Back) },
            { 650, new TattooData(TattooData.ZoneTypes.Back) },
            { 651, new TattooData(TattooData.ZoneTypes.Back) },
            { 652, new TattooData(TattooData.ZoneTypes.Back) },
            { 653, new TattooData(TattooData.ZoneTypes.Back) },
            { 654, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 655, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 656, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 657, new TattooData(TattooData.ZoneTypes.Back) },
            { 658, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 659, new TattooData(TattooData.ZoneTypes.Back) },
            { 660, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 661, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 662, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 663, new TattooData(TattooData.ZoneTypes.Back) },
            { 664, new TattooData(TattooData.ZoneTypes.Chest) },
            { 665, new TattooData(TattooData.ZoneTypes.Chest) },
            { 666, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 667, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 668, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 669, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 670, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 671, new TattooData(TattooData.ZoneTypes.Back) },
            { 672, new TattooData(TattooData.ZoneTypes.Back) },
            { 673, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 674, new TattooData(TattooData.ZoneTypes.Back) },
            { 675, new TattooData(TattooData.ZoneTypes.Face) },
            { 676, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 677, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 678, new TattooData(TattooData.ZoneTypes.Chest) },
            { 679, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 680, new TattooData(TattooData.ZoneTypes.Back) },
            { 681, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 682, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 683, new TattooData(TattooData.ZoneTypes.Back) },
            { 684, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 685, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 686, new TattooData(TattooData.ZoneTypes.Chest) },
            { 687, new TattooData(TattooData.ZoneTypes.Chest) },
            { 688, new TattooData(TattooData.ZoneTypes.Chest) },
            { 689, new TattooData(TattooData.ZoneTypes.Back) },
            { 690, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 691, new TattooData(TattooData.ZoneTypes.Back) },
            { 692, new TattooData(TattooData.ZoneTypes.Back) },
            { 693, new TattooData(TattooData.ZoneTypes.Back) },
            { 694, new TattooData(TattooData.ZoneTypes.Back) },
            { 695, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 696, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 697, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 698, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 699, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 700, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 701, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 702, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 703, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 704, new TattooData(TattooData.ZoneTypes.Back) },
            { 705, new TattooData(TattooData.ZoneTypes.Chest) },
            { 706, new TattooData(TattooData.ZoneTypes.Back) },
            { 707, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 708, new TattooData(TattooData.ZoneTypes.Back) },
            { 709, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 710, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 711, new TattooData(TattooData.ZoneTypes.Back) },
            { 712, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 713, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 714, new TattooData(TattooData.ZoneTypes.Back) },
            { 715, new TattooData(TattooData.ZoneTypes.Back) },
            { 716, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 717, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 718, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 719, new TattooData(TattooData.ZoneTypes.Chest) },
            { 720, new TattooData(TattooData.ZoneTypes.Chest) },
            { 721, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 722, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 723, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 724, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 725, new TattooData(TattooData.ZoneTypes.Back) },
            { 726, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 727, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 728, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 729, new TattooData(TattooData.ZoneTypes.Chest) },
            { 730, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 731, new TattooData(TattooData.ZoneTypes.Stomath) },
            { 732, new TattooData(TattooData.ZoneTypes.LeftLeg) },
            { 733, new TattooData(TattooData.ZoneTypes.RightArm) },
            { 734, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 735, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 736, new TattooData(TattooData.ZoneTypes.LeftArm) },
            { 737, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 738, new TattooData(TattooData.ZoneTypes.RightLeg) },
            { 739, new TattooData(TattooData.ZoneTypes.Chest) },
            { 740, new TattooData(TattooData.ZoneTypes.Back) },
            { 741, new TattooData(TattooData.ZoneTypes.RightArm) },
        };

        public static TattooData GetTattooData(int tattooIdx) => AllTattoos.GetValueOrDefault(tattooIdx);

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

        #region Defaults
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

        public class Defaults
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
        #endregion

        #region All Hairs
        private static Dictionary<int, int> MaleHairs = new Dictionary<int, int>()
        {
            // GTA Default
            { 0, 0 }, { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 }, { 5, 5 }, { 6, 6 }, { 7, 7 }, { 8, 8 }, { 9, 9 }, { 10, 10 }, { 11, 11 }, { 12, 12 }, { 13, 13 }, { 14, 14 }, { 15, 15 }, { 16, 16 }, { 17, 17 },{ 18, 18 }, { 19, 19 }, { 20, 20 }, { 21, 21 }, { 22, 22 }, { 23, 24 }, { 24, 25 }, { 25, 26 }, { 26, 27 }, { 27, 28}, { 28, 29 }, { 29, 30 }, { 30, 31 }, { 31, 32 }, { 32,33 }, { 33, 34 }, { 34, 35 }, { 35, 36 }, { 36, 72 }, { 37, 73 }, { 38, 74 }, { 39, 75 }, { 40, 76 },

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
