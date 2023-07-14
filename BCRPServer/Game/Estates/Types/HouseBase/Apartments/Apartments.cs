using GTANetworkAPI;
using System.Collections.Generic;

namespace BCRPServer.Game.Estates
{
    public partial class Apartments : HouseBase
    {
        public partial class ApartmentsRoot
        {
            public static Dictionary<uint, ApartmentsRoot> All { get; set; } = new Dictionary<uint, ApartmentsRoot>();

            public enum ShellTypes : byte
            {
                None = 0,

                HighEnd_0 = 1,

                MediumEnd_0 = 2,

                LowEnd_1_0 = 3,
                LowEnd_2_0 = 4,
                LowEnd_3_0 = 5,
                LowEnd_4_0 = 6,
                LowEnd_5_0 = 7,
            }

            public class ShellData
            {
                public Utils.Vector4 EnterPosition { get; set; }

                public Utils.Vector4[][] ElevatorPositions { get; set; }

                public Utils.Vector4[][] ApartmentsPositions { get; set; }

                public ushort FloorsAmount { get; set; }
                public ushort StartFloor { get; set; }

                public ShellData()
                {

                }

                public Utils.Vector4 GetFloorPosition(ushort floorIdx, ushort subIdx = 0)
                {
                    if (floorIdx >= ElevatorPositions.Length)
                        return null;

                    if (subIdx >= ElevatorPositions[floorIdx].Length)
                        return null;

                    var d = ElevatorPositions[floorIdx][subIdx];

                    return d;
                }

                public Utils.Vector4 GetApartmentsPositionByIdx(int idx)
                {
                    if (idx < 0)
                        return null;

                    for (int i = 0; i < ApartmentsPositions.Length; i++)
                    {
                        if (idx < ApartmentsPositions[i].Length)
                            return ApartmentsPositions[i][idx];

                        idx -= ApartmentsPositions[i].Length;
                    }

                    return null;
                }

                public Utils.Vector4 GetApartmentsPosition(ushort floorIdx, ushort subIdx)
                {
                    if (floorIdx >= ApartmentsPositions.Length)
                        return null;

                    if (subIdx >= ApartmentsPositions[floorIdx].Length)
                        return null;

                    var d = ApartmentsPositions[floorIdx][subIdx];

                    return d;
                }

                public int GetApartmentsIdx(ushort floorIdx, ushort subIdx)
                {
                    if (floorIdx >= ApartmentsPositions.Length)
                        return -1;

                    if (subIdx >= ApartmentsPositions[floorIdx].Length)
                        return -1;

                    var totalCount = (int)subIdx;

                    for (ushort i = 0; i < floorIdx; i++)
                    {
                        totalCount += ApartmentsPositions[i].Length;
                    }

                    return totalCount;
                }
            }

            private static Dictionary<ShellTypes, ShellData> Shells = new Dictionary<ShellTypes, ShellData>()
            {
                {
                    ShellTypes.HighEnd_0,

                    new ShellData()
                    {
                        StartFloor = 2,

                        FloorsAmount = 10,

                        EnterPosition = new Utils.Vector4(-697.593f, 3497.643f, -183.9665f, 267.2679f),

                        ElevatorPositions = new Utils.Vector4[][]
                        {
                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-702.8301f, 3503.024f, -180.2704f, 175.7214f),
                                new Utils.Vector4(-699.9159f, 3500.053f, -180.2704f, 85.07867f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-702.8301f, 3503.024f, -180.2704f + 3.7f * 1, 175.7214f),
                                new Utils.Vector4(-699.9159f, 3500.053f, -180.2704f + 3.7f * 1, 85.07867f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-702.8301f, 3503.024f, -180.2704f + 3.7f * 2, 175.7214f),
                                new Utils.Vector4(-699.9159f, 3500.053f, -180.2704f + 3.7f * 2, 85.07867f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-702.8301f, 3503.024f, -180.2704f + 3.7f * 3, 175.7214f),
                                new Utils.Vector4(-699.9159f, 3500.053f, -180.2704f + 3.7f * 3, 85.07867f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-702.8301f, 3503.024f, -180.2704f + 3.7f * 4, 175.7214f),
                                new Utils.Vector4(-699.9159f, 3500.053f, -180.2704f + 3.7f * 4, 85.07867f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-702.8301f, 3503.024f, -180.2704f + 3.7f * 5, 175.7214f),
                                new Utils.Vector4(-699.9159f, 3500.053f, -180.2704f + 3.7f * 5, 85.07867f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-702.8301f, 3503.024f, -180.2704f + 3.7f * 6, 175.7214f),
                                new Utils.Vector4(-699.9159f, 3500.053f, -180.2704f + 3.7f * 6, 85.07867f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-702.8301f, 3503.024f, -180.2704f + 3.7f * 7, 175.7214f),
                                new Utils.Vector4(-699.9159f, 3500.053f, -180.2704f + 3.7f * 7, 85.07867f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-702.8301f, 3503.024f, -180.2704f + 3.7f * 8, 175.7214f),
                                new Utils.Vector4(-699.9159f, 3500.053f, -180.2704f + 3.7f * 8, 85.07867f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-702.8301f, 3503.024f, -180.2704f + 3.7f * 9, 175.7214f),
                                new Utils.Vector4(-699.9159f, 3500.053f, -180.2704f + 3.7f * 9, 85.07867f),
                            },
                        },

                        ApartmentsPositions = new Utils.Vector4[][]
                        {
                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-707.1363f, 3504.645f, -180.2665f, 266.8018f),
                                new Utils.Vector4(-705.5142f, 3510.334f, -180.2666f, 88.19016f),
                                new Utils.Vector4(-707.0286f, 3516.093f, -180.2666f, 264.0901f),
                                new Utils.Vector4(-705.5581f, 3521.777f, -180.2666f, 83.84389f),
                                new Utils.Vector4(-706.4679f, 3526.787f, -180.2666f, 169.9961f),
                                new Utils.Vector4(-707.109f, 3493.222f, -180.2665f, 272.0522f),
                                new Utils.Vector4(-705.5453f, 3487.513f, -180.2666f, 88.37497f),
                                new Utils.Vector4(-707.1467f, 3481.759f, -180.2665f, 264.6181f),
                                new Utils.Vector4(-705.621f, 3476.189f, -180.2665f, 86.95593f),
                                new Utils.Vector4(-706.3455f, 3471.134f, -180.2665f, 5.657025f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-707.1363f, 3504.645f, -180.2665f + 3.7f * 1, 266.8018f),
                                new Utils.Vector4(-705.5142f, 3510.334f, -180.2666f + 3.7f * 1, 88.19016f),
                                new Utils.Vector4(-707.0286f, 3516.093f, -180.2666f + 3.7f * 1, 264.0901f),
                                new Utils.Vector4(-705.5581f, 3521.777f, -180.2666f + 3.7f * 1, 83.84389f),
                                new Utils.Vector4(-706.4679f, 3526.787f, -180.2666f + 3.7f * 1, 169.9961f),
                                new Utils.Vector4(-707.109f, 3493.222f, -180.2665f + 3.7f * 1, 272.0522f),
                                new Utils.Vector4(-705.5453f, 3487.513f, -180.2666f + 3.7f * 1, 88.37497f),
                                new Utils.Vector4(-707.1467f, 3481.759f, -180.2665f + 3.7f * 1, 264.6181f),
                                new Utils.Vector4(-705.621f, 3476.189f, -180.2665f + 3.7f * 1, 86.95593f),
                                new Utils.Vector4(-706.3455f, 3471.134f, -180.2665f + 3.7f * 1, 5.657025f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-707.1363f, 3504.645f, -180.2665f + 3.7f * 2, 266.8018f),
                                new Utils.Vector4(-705.5142f, 3510.334f, -180.2666f + 3.7f * 2, 88.19016f),
                                new Utils.Vector4(-707.0286f, 3516.093f, -180.2666f + 3.7f * 2, 264.0901f),
                                new Utils.Vector4(-705.5581f, 3521.777f, -180.2666f + 3.7f * 2, 83.84389f),
                                new Utils.Vector4(-706.4679f, 3526.787f, -180.2666f + 3.7f * 2, 169.9961f),
                                new Utils.Vector4(-707.109f, 3493.222f, -180.2665f + 3.7f * 2, 272.0522f),
                                new Utils.Vector4(-705.5453f, 3487.513f, -180.2666f + 3.7f * 2, 88.37497f),
                                new Utils.Vector4(-707.1467f, 3481.759f, -180.2665f + 3.7f * 2, 264.6181f),
                                new Utils.Vector4(-705.621f, 3476.189f, -180.2665f + 3.7f * 2, 86.95593f),
                                new Utils.Vector4(-706.3455f, 3471.134f, -180.2665f + 3.7f * 2, 5.657025f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-707.1363f, 3504.645f, -180.2665f + 3.7f * 3, 266.8018f),
                                new Utils.Vector4(-705.5142f, 3510.334f, -180.2666f + 3.7f * 3, 88.19016f),
                                new Utils.Vector4(-707.0286f, 3516.093f, -180.2666f + 3.7f * 3, 264.0901f),
                                new Utils.Vector4(-705.5581f, 3521.777f, -180.2666f + 3.7f * 3, 83.84389f),
                                new Utils.Vector4(-706.4679f, 3526.787f, -180.2666f + 3.7f * 3, 169.9961f),
                                new Utils.Vector4(-707.109f, 3493.222f, -180.2665f + 3.7f * 3, 272.0522f),
                                new Utils.Vector4(-705.5453f, 3487.513f, -180.2666f + 3.7f * 3, 88.37497f),
                                new Utils.Vector4(-707.1467f, 3481.759f, -180.2665f + 3.7f * 3, 264.6181f),
                                new Utils.Vector4(-705.621f, 3476.189f, -180.2665f + 3.7f * 3, 86.95593f),
                                new Utils.Vector4(-706.3455f, 3471.134f, -180.2665f + 3.7f * 3, 5.657025f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-707.1363f, 3504.645f, -180.2665f + 3.7f * 4, 266.8018f),
                                new Utils.Vector4(-705.5142f, 3510.334f, -180.2666f + 3.7f * 4, 88.19016f),
                                new Utils.Vector4(-707.0286f, 3516.093f, -180.2666f + 3.7f * 4, 264.0901f),
                                new Utils.Vector4(-705.5581f, 3521.777f, -180.2666f + 3.7f * 4, 83.84389f),
                                new Utils.Vector4(-706.4679f, 3526.787f, -180.2666f + 3.7f * 4, 169.9961f),
                                new Utils.Vector4(-707.109f, 3493.222f, -180.2665f + 3.7f * 4, 272.0522f),
                                new Utils.Vector4(-705.5453f, 3487.513f, -180.2666f + 3.7f * 4, 88.37497f),
                                new Utils.Vector4(-707.1467f, 3481.759f, -180.2665f + 3.7f * 4, 264.6181f),
                                new Utils.Vector4(-705.621f, 3476.189f, -180.2665f + 3.7f * 4, 86.95593f),
                                new Utils.Vector4(-706.3455f, 3471.134f, -180.2665f + 3.7f * 4, 5.657025f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-707.1363f, 3504.645f, -180.2665f + 3.7f * 5, 266.8018f),
                                new Utils.Vector4(-705.5142f, 3510.334f, -180.2666f + 3.7f * 5, 88.19016f),
                                new Utils.Vector4(-707.0286f, 3516.093f, -180.2666f + 3.7f * 5, 264.0901f),
                                new Utils.Vector4(-705.5581f, 3521.777f, -180.2666f + 3.7f * 5, 83.84389f),
                                new Utils.Vector4(-706.4679f, 3526.787f, -180.2666f + 3.7f * 5, 169.9961f),
                                new Utils.Vector4(-707.109f, 3493.222f, -180.2665f + 3.7f * 5, 272.0522f),
                                new Utils.Vector4(-705.5453f, 3487.513f, -180.2666f + 3.7f * 5, 88.37497f),
                                new Utils.Vector4(-707.1467f, 3481.759f, -180.2665f + 3.7f * 5, 264.6181f),
                                new Utils.Vector4(-705.621f, 3476.189f, -180.2665f + 3.7f * 5, 86.95593f),
                                new Utils.Vector4(-706.3455f, 3471.134f, -180.2665f + 3.7f * 5, 5.657025f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-707.1363f, 3504.645f, -180.2665f + 3.7f * 6, 266.8018f),
                                new Utils.Vector4(-705.5142f, 3510.334f, -180.2666f + 3.7f * 6, 88.19016f),
                                new Utils.Vector4(-707.0286f, 3516.093f, -180.2666f + 3.7f * 6, 264.0901f),
                                new Utils.Vector4(-705.5581f, 3521.777f, -180.2666f + 3.7f * 6, 83.84389f),
                                new Utils.Vector4(-706.4679f, 3526.787f, -180.2666f + 3.7f * 6, 169.9961f),
                                new Utils.Vector4(-707.109f, 3493.222f, -180.2665f + 3.7f * 6, 272.0522f),
                                new Utils.Vector4(-705.5453f, 3487.513f, -180.2666f + 3.7f * 6, 88.37497f),
                                new Utils.Vector4(-707.1467f, 3481.759f, -180.2665f + 3.7f * 6, 264.6181f),
                                new Utils.Vector4(-705.621f, 3476.189f, -180.2665f + 3.7f * 6, 86.95593f),
                                new Utils.Vector4(-706.3455f, 3471.134f, -180.2665f + 3.7f * 6, 5.657025f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-707.1363f, 3504.645f, -180.2665f + 3.7f * 7, 266.8018f),
                                new Utils.Vector4(-705.5142f, 3510.334f, -180.2666f + 3.7f * 7, 88.19016f),
                                new Utils.Vector4(-707.0286f, 3516.093f, -180.2666f + 3.7f * 7, 264.0901f),
                                new Utils.Vector4(-705.5581f, 3521.777f, -180.2666f + 3.7f * 7, 83.84389f),
                                new Utils.Vector4(-706.4679f, 3526.787f, -180.2666f + 3.7f * 7, 169.9961f),
                                new Utils.Vector4(-707.109f, 3493.222f, -180.2665f + 3.7f * 7, 272.0522f),
                                new Utils.Vector4(-705.5453f, 3487.513f, -180.2666f + 3.7f * 7, 88.37497f),
                                new Utils.Vector4(-707.1467f, 3481.759f, -180.2665f + 3.7f * 7, 264.6181f),
                                new Utils.Vector4(-705.621f, 3476.189f, -180.2665f + 3.7f * 7, 86.95593f),
                                new Utils.Vector4(-706.3455f, 3471.134f, -180.2665f + 3.7f * 7, 5.657025f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-707.1363f, 3504.645f, -180.2665f + 3.7f * 8, 266.8018f),
                                new Utils.Vector4(-705.5142f, 3510.334f, -180.2666f + 3.7f * 8, 88.19016f),
                                new Utils.Vector4(-707.0286f, 3516.093f, -180.2666f + 3.7f * 8, 264.0901f),
                                new Utils.Vector4(-705.5581f, 3521.777f, -180.2666f + 3.7f * 8, 83.84389f),
                                new Utils.Vector4(-706.4679f, 3526.787f, -180.2666f + 3.7f * 8, 169.9961f),
                                new Utils.Vector4(-707.109f, 3493.222f, -180.2665f + 3.7f * 8, 272.0522f),
                                new Utils.Vector4(-705.5453f, 3487.513f, -180.2666f + 3.7f * 8, 88.37497f),
                                new Utils.Vector4(-707.1467f, 3481.759f, -180.2665f + 3.7f * 8, 264.6181f),
                                new Utils.Vector4(-705.621f, 3476.189f, -180.2665f + 3.7f * 8, 86.95593f),
                                new Utils.Vector4(-706.3455f, 3471.134f, -180.2665f + 3.7f * 8, 5.657025f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-707.1363f, 3504.645f, -180.2665f + 3.7f * 9, 266.8018f),
                                new Utils.Vector4(-705.5142f, 3510.334f, -180.2666f + 3.7f * 9, 88.19016f),
                                new Utils.Vector4(-707.0286f, 3516.093f, -180.2666f + 3.7f * 9, 264.0901f),
                                new Utils.Vector4(-705.5581f, 3521.777f, -180.2666f + 3.7f * 9, 83.84389f),
                                new Utils.Vector4(-706.4679f, 3526.787f, -180.2666f + 3.7f * 9, 169.9961f),
                                new Utils.Vector4(-707.109f, 3493.222f, -180.2665f + 3.7f * 9, 272.0522f),
                                new Utils.Vector4(-705.5453f, 3487.513f, -180.2666f + 3.7f * 9, 88.37497f),
                                new Utils.Vector4(-707.1467f, 3481.759f, -180.2665f + 3.7f * 9, 264.6181f),
                                new Utils.Vector4(-705.621f, 3476.189f, -180.2665f + 3.7f * 9, 86.95593f),
                                new Utils.Vector4(-706.3455f, 3471.134f, -180.2665f + 3.7f * 9, 5.657025f),
                            },
                        },
                    }
                },

                {
                    ShellTypes.MediumEnd_0,

                    new ShellData()
                    {
                        StartFloor = 2,

                        FloorsAmount = 10,

                        EnterPosition = new Utils.Vector4(-743.8352f, 3494.641f, -184.0275f, 266.6665f),

                        ElevatorPositions = new Utils.Vector4[][]
                        {
                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-747.1423f, 3498.099f, -180.3276f, 180.1312f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-747.1423f, 3498.099f, -180.3276f + 3.7f * 1, 180.1312f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-747.1423f, 3498.099f, -180.3276f + 3.7f * 2, 180.1312f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-747.1423f, 3498.099f, -180.3276f + 3.7f * 3, 180.1312f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-747.1423f, 3498.099f, -180.3276f + 3.7f * 4, 180.1312f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-747.1423f, 3498.099f, -180.3276f + 3.7f * 5, 180.1312f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-747.1423f, 3498.099f, -180.3276f + 3.7f * 6, 180.1312f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-747.1423f, 3498.099f, -180.3276f + 3.7f * 7, 180.1312f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-747.1423f, 3498.099f, -180.3276f + 3.7f * 8, 180.1312f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-747.1423f, 3498.099f, -180.3276f + 3.7f * 9, 180.1312f),
                            },
                        },

                        ApartmentsPositions = new Utils.Vector4[][]
                        {
                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-751.3419f, 3501.685f, -180.3276f, 265.8782f),
                                new Utils.Vector4(-749.7281f, 3507.42f, -180.3277f, 88.47911f),
                                new Utils.Vector4(-751.457f, 3513.108f, -180.3275f, 272.6219f),
                                new Utils.Vector4(-749.7033f, 3518.836f, -180.3277f, 85.58039f),
                                new Utils.Vector4(-750.5911f, 3523.635f, -180.3276f, 174.9762f),
                                new Utils.Vector4(-751.3865f, 3490.265f, -180.3276f, 271.9004f),
                                new Utils.Vector4(-749.9337f, 3484.568f, -180.3277f, 89.77471f),
                                new Utils.Vector4(-751.5001f, 3478.83f, -180.3275f, 270.5908f),
                                new Utils.Vector4(-749.7493f, 3473.245f, -180.3276f, 93.2873f),
                                new Utils.Vector4(-750.5829f, 3468.194f, -180.3277f, 357.6786f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-751.3419f, 3501.685f, -180.3276f + 3.7f * 1, 265.8782f),
                                new Utils.Vector4(-749.7281f, 3507.42f, -180.3277f + 3.7f * 1, 88.47911f),
                                new Utils.Vector4(-751.457f, 3513.108f, -180.3275f + 3.7f * 1, 272.6219f),
                                new Utils.Vector4(-749.7033f, 3518.836f, -180.3277f + 3.7f * 1, 85.58039f),
                                new Utils.Vector4(-750.5911f, 3523.635f, -180.3276f + 3.7f * 1, 174.9762f),
                                new Utils.Vector4(-751.3865f, 3490.265f, -180.3276f + 3.7f * 1, 271.9004f),
                                new Utils.Vector4(-749.9337f, 3484.568f, -180.3277f + 3.7f * 1, 89.77471f),
                                new Utils.Vector4(-751.5001f, 3478.83f, -180.3275f + 3.7f * 1, 270.5908f),
                                new Utils.Vector4(-749.7493f, 3473.245f, -180.3276f + 3.7f * 1, 93.2873f),
                                new Utils.Vector4(-750.5829f, 3468.194f, -180.3277f + 3.7f * 1, 357.6786f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-751.3419f, 3501.685f, -180.3276f + 3.7f * 2, 265.8782f),
                                new Utils.Vector4(-749.7281f, 3507.42f, -180.3277f + 3.7f * 2, 88.47911f),
                                new Utils.Vector4(-751.457f, 3513.108f, -180.3275f + 3.7f * 2, 272.6219f),
                                new Utils.Vector4(-749.7033f, 3518.836f, -180.3277f + 3.7f * 2, 85.58039f),
                                new Utils.Vector4(-750.5911f, 3523.635f, -180.3276f + 3.7f * 2, 174.9762f),
                                new Utils.Vector4(-751.3865f, 3490.265f, -180.3276f + 3.7f * 2, 271.9004f),
                                new Utils.Vector4(-749.9337f, 3484.568f, -180.3277f + 3.7f * 2, 89.77471f),
                                new Utils.Vector4(-751.5001f, 3478.83f, -180.3275f + 3.7f * 2, 270.5908f),
                                new Utils.Vector4(-749.7493f, 3473.245f, -180.3276f + 3.7f * 2, 93.2873f),
                                new Utils.Vector4(-750.5829f, 3468.194f, -180.3277f + 3.7f * 2, 357.6786f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-751.3419f, 3501.685f, -180.3276f + 3.7f * 3, 265.8782f),
                                new Utils.Vector4(-749.7281f, 3507.42f, -180.3277f + 3.7f * 3, 88.47911f),
                                new Utils.Vector4(-751.457f, 3513.108f, -180.3275f + 3.7f * 3, 272.6219f),
                                new Utils.Vector4(-749.7033f, 3518.836f, -180.3277f + 3.7f * 3, 85.58039f),
                                new Utils.Vector4(-750.5911f, 3523.635f, -180.3276f + 3.7f * 3, 174.9762f),
                                new Utils.Vector4(-751.3865f, 3490.265f, -180.3276f + 3.7f * 3, 271.9004f),
                                new Utils.Vector4(-749.9337f, 3484.568f, -180.3277f + 3.7f * 3, 89.77471f),
                                new Utils.Vector4(-751.5001f, 3478.83f, -180.3275f + 3.7f * 3, 270.5908f),
                                new Utils.Vector4(-749.7493f, 3473.245f, -180.3276f + 3.7f * 3, 93.2873f),
                                new Utils.Vector4(-750.5829f, 3468.194f, -180.3277f + 3.7f * 3, 357.6786f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-751.3419f, 3501.685f, -180.3276f + 3.7f * 4, 265.8782f),
                                new Utils.Vector4(-749.7281f, 3507.42f, -180.3277f + 3.7f * 4, 88.47911f),
                                new Utils.Vector4(-751.457f, 3513.108f, -180.3275f + 3.7f * 4, 272.6219f),
                                new Utils.Vector4(-749.7033f, 3518.836f, -180.3277f + 3.7f * 4, 85.58039f),
                                new Utils.Vector4(-750.5911f, 3523.635f, -180.3276f + 3.7f * 4, 174.9762f),
                                new Utils.Vector4(-751.3865f, 3490.265f, -180.3276f + 3.7f * 4, 271.9004f),
                                new Utils.Vector4(-749.9337f, 3484.568f, -180.3277f + 3.7f * 4, 89.77471f),
                                new Utils.Vector4(-751.5001f, 3478.83f, -180.3275f + 3.7f * 4, 270.5908f),
                                new Utils.Vector4(-749.7493f, 3473.245f, -180.3276f + 3.7f * 4, 93.2873f),
                                new Utils.Vector4(-750.5829f, 3468.194f, -180.3277f + 3.7f * 4, 357.6786f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-751.3419f, 3501.685f, -180.3276f + 3.7f * 5, 265.8782f),
                                new Utils.Vector4(-749.7281f, 3507.42f, -180.3277f + 3.7f * 5, 88.47911f),
                                new Utils.Vector4(-751.457f, 3513.108f, -180.3275f + 3.7f * 5, 272.6219f),
                                new Utils.Vector4(-749.7033f, 3518.836f, -180.3277f + 3.7f * 5, 85.58039f),
                                new Utils.Vector4(-750.5911f, 3523.635f, -180.3276f + 3.7f * 5, 174.9762f),
                                new Utils.Vector4(-751.3865f, 3490.265f, -180.3276f + 3.7f * 5, 271.9004f),
                                new Utils.Vector4(-749.9337f, 3484.568f, -180.3277f + 3.7f * 5, 89.77471f),
                                new Utils.Vector4(-751.5001f, 3478.83f, -180.3275f + 3.7f * 5, 270.5908f),
                                new Utils.Vector4(-749.7493f, 3473.245f, -180.3276f + 3.7f * 5, 93.2873f),
                                new Utils.Vector4(-750.5829f, 3468.194f, -180.3277f + 3.7f * 5, 357.6786f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-751.3419f, 3501.685f, -180.3276f + 3.7f * 6, 265.8782f),
                                new Utils.Vector4(-749.7281f, 3507.42f, -180.3277f + 3.7f * 6, 88.47911f),
                                new Utils.Vector4(-751.457f, 3513.108f, -180.3275f + 3.7f * 6, 272.6219f),
                                new Utils.Vector4(-749.7033f, 3518.836f, -180.3277f + 3.7f * 6, 85.58039f),
                                new Utils.Vector4(-750.5911f, 3523.635f, -180.3276f + 3.7f * 6, 174.9762f),
                                new Utils.Vector4(-751.3865f, 3490.265f, -180.3276f + 3.7f * 6, 271.9004f),
                                new Utils.Vector4(-749.9337f, 3484.568f, -180.3277f + 3.7f * 6, 89.77471f),
                                new Utils.Vector4(-751.5001f, 3478.83f, -180.3275f + 3.7f * 6, 270.5908f),
                                new Utils.Vector4(-749.7493f, 3473.245f, -180.3276f + 3.7f * 6, 93.2873f),
                                new Utils.Vector4(-750.5829f, 3468.194f, -180.3277f + 3.7f * 6, 357.6786f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-751.3419f, 3501.685f, -180.3276f + 3.7f * 7, 265.8782f),
                                new Utils.Vector4(-749.7281f, 3507.42f, -180.3277f + 3.7f * 7, 88.47911f),
                                new Utils.Vector4(-751.457f, 3513.108f, -180.3275f + 3.7f * 7, 272.6219f),
                                new Utils.Vector4(-749.7033f, 3518.836f, -180.3277f + 3.7f * 7, 85.58039f),
                                new Utils.Vector4(-750.5911f, 3523.635f, -180.3276f + 3.7f * 7, 174.9762f),
                                new Utils.Vector4(-751.3865f, 3490.265f, -180.3276f + 3.7f * 7, 271.9004f),
                                new Utils.Vector4(-749.9337f, 3484.568f, -180.3277f + 3.7f * 7, 89.77471f),
                                new Utils.Vector4(-751.5001f, 3478.83f, -180.3275f + 3.7f * 7, 270.5908f),
                                new Utils.Vector4(-749.7493f, 3473.245f, -180.3276f + 3.7f * 7, 93.2873f),
                                new Utils.Vector4(-750.5829f, 3468.194f, -180.3277f + 3.7f * 7, 357.6786f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-751.3419f, 3501.685f, -180.3276f + 3.7f * 8, 265.8782f),
                                new Utils.Vector4(-749.7281f, 3507.42f, -180.3277f + 3.7f * 8, 88.47911f),
                                new Utils.Vector4(-751.457f, 3513.108f, -180.3275f + 3.7f * 8, 272.6219f),
                                new Utils.Vector4(-749.7033f, 3518.836f, -180.3277f + 3.7f * 8, 85.58039f),
                                new Utils.Vector4(-750.5911f, 3523.635f, -180.3276f + 3.7f * 8, 174.9762f),
                                new Utils.Vector4(-751.3865f, 3490.265f, -180.3276f + 3.7f * 8, 271.9004f),
                                new Utils.Vector4(-749.9337f, 3484.568f, -180.3277f + 3.7f * 8, 89.77471f),
                                new Utils.Vector4(-751.5001f, 3478.83f, -180.3275f + 3.7f * 8, 270.5908f),
                                new Utils.Vector4(-749.7493f, 3473.245f, -180.3276f + 3.7f * 8, 93.2873f),
                                new Utils.Vector4(-750.5829f, 3468.194f, -180.3277f + 3.7f * 8, 357.6786f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-751.3419f, 3501.685f, -180.3276f + 3.7f * 9, 265.8782f),
                                new Utils.Vector4(-749.7281f, 3507.42f, -180.3277f + 3.7f * 9, 88.47911f),
                                new Utils.Vector4(-751.457f, 3513.108f, -180.3275f + 3.7f * 9, 272.6219f),
                                new Utils.Vector4(-749.7033f, 3518.836f, -180.3277f + 3.7f * 9, 85.58039f),
                                new Utils.Vector4(-750.5911f, 3523.635f, -180.3276f + 3.7f * 9, 174.9762f),
                                new Utils.Vector4(-751.3865f, 3490.265f, -180.3276f + 3.7f * 9, 271.9004f),
                                new Utils.Vector4(-749.9337f, 3484.568f, -180.3277f + 3.7f * 9, 89.77471f),
                                new Utils.Vector4(-751.5001f, 3478.83f, -180.3275f + 3.7f * 9, 270.5908f),
                                new Utils.Vector4(-749.7493f, 3473.245f, -180.3276f + 3.7f * 9, 93.2873f),
                                new Utils.Vector4(-750.5829f, 3468.194f, -180.3277f + 3.7f * 9, 357.6786f),
                            },
                        },
                    }
                },

                {
                    ShellTypes.LowEnd_1_0,

                    new ShellData()
                    {
                        StartFloor = 2,

                        FloorsAmount = 1,

                        EnterPosition = new Utils.Vector4(-802.7189f, 3499.903f, -179.0001f, 266.5319f),

                        ElevatorPositions = new Utils.Vector4[][]
                        {

                        },

                        ApartmentsPositions = new Utils.Vector4[][]
                        {
                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-795.5585f, 3501.655f, -177.0267f, 183.1305f),
                                new Utils.Vector4(-794.3295f, 3500.99f, -177.0267f, 87.74728f),
                                new Utils.Vector4(-794.4268f, 3498.915f, -177.0266f, 83.43294f),
                                new Utils.Vector4(-795.5344f, 3498.274f, -177.0266f, 0.7257882f),
                            },
                        },
                    }
                },

                {
                    ShellTypes.LowEnd_2_0,

                    new ShellData()
                    {
                        StartFloor = 2,

                        FloorsAmount = 2,

                        EnterPosition = new Utils.Vector4(-802.7189f, 3484.903f, -179.0001f, 266.5319f),

                        ElevatorPositions = new Utils.Vector4[][]
                        {

                        },

                        ApartmentsPositions = new Utils.Vector4[][]
                        {
                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-795.5585f, 3486.655f, -177.0267f, 183.1305f),
                                new Utils.Vector4(-794.3295f, 3485.99f, -177.0267f, 87.74728f),
                                new Utils.Vector4(-794.4268f, 3483.915f, -177.0266f, 83.43294f),
                                new Utils.Vector4(-795.5344f, 3483.274f, -177.0266f, 0.7257882f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-795.5585f, 3486.655f, -177.0267f + 3.7f * 1, 183.1305f),
                                new Utils.Vector4(-794.3295f, 3485.99f, -177.0267f + 3.7f * 1, 87.74728f),
                                new Utils.Vector4(-794.4268f, 3483.915f, -177.0266f + 3.7f * 1, 83.43294f),
                                new Utils.Vector4(-795.5344f, 3483.274f, -177.0266f + 3.7f * 1, 0.7257882f),
                            },
                        },
                    }
                },

                {
                    ShellTypes.LowEnd_3_0,

                    new ShellData()
                    {
                        StartFloor = 2,

                        FloorsAmount = 3,

                        EnterPosition = new Utils.Vector4(-802.7189f, 3469.903f, -179.0001f, 266.5319f),

                        ElevatorPositions = new Utils.Vector4[][]
                        {

                        },

                        ApartmentsPositions = new Utils.Vector4[][]
                        {
                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-795.5585f, 3471.655f, -177.0267f, 183.1305f),
                                new Utils.Vector4(-794.3295f, 3470.99f, -177.0267f, 87.74728f),
                                new Utils.Vector4(-794.4268f, 3468.915f, -177.0266f, 83.43294f),
                                new Utils.Vector4(-795.5344f, 3468.274f, -177.0266f, 0.7257882f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-795.5585f, 3471.655f, -177.0267f + 3.7f * 1, 183.1305f),
                                new Utils.Vector4(-794.3295f, 3470.99f, -177.0267f + 3.7f * 1, 87.74728f),
                                new Utils.Vector4(-794.4268f, 3468.915f, -177.0266f + 3.7f * 1, 83.43294f),
                                new Utils.Vector4(-795.5344f, 3468.274f, -177.0266f + 3.7f * 1, 0.7257882f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-795.5585f, 3471.655f, -177.0267f + 3.7f * 2, 183.1305f),
                                new Utils.Vector4(-794.3295f, 3470.99f, -177.0267f + 3.7f * 2, 87.74728f),
                                new Utils.Vector4(-794.4268f, 3468.915f, -177.0266f + 3.7f * 2, 83.43294f),
                                new Utils.Vector4(-795.5344f, 3468.274f, -177.0266f + 3.7f * 2, 0.7257882f),
                            },
                        },
                    }
                },

                {
                    ShellTypes.LowEnd_4_0,

                    new ShellData()
                    {
                        StartFloor = 2,

                        FloorsAmount = 4,

                        EnterPosition = new Utils.Vector4(-802.7189f, 3454.903f, -179.0001f, 266.5319f),

                        ElevatorPositions = new Utils.Vector4[][]
                        {

                        },

                        ApartmentsPositions = new Utils.Vector4[][]
                        {
                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-795.5585f, 3456.655f, -177.0267f, 183.1305f),
                                new Utils.Vector4(-794.3295f, 3455.99f, -177.0267f, 87.74728f),
                                new Utils.Vector4(-794.4268f, 3453.915f, -177.0266f, 83.43294f),
                                new Utils.Vector4(-795.5344f, 3453.274f, -177.0266f, 0.7257882f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-795.5585f, 3456.655f, -177.0267f + 3.7f * 1, 183.1305f),
                                new Utils.Vector4(-794.3295f, 3455.99f, -177.0267f + 3.7f * 1, 87.74728f),
                                new Utils.Vector4(-794.4268f, 3453.915f, -177.0266f + 3.7f * 1, 83.43294f),
                                new Utils.Vector4(-795.5344f, 3453.274f, -177.0266f + 3.7f * 1, 0.7257882f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-795.5585f, 3456.655f, -177.0267f + 3.7f * 2, 183.1305f),
                                new Utils.Vector4(-794.3295f, 3455.99f, -177.0267f + 3.7f * 2, 87.74728f),
                                new Utils.Vector4(-794.4268f, 3453.915f, -177.0266f + 3.7f * 2, 83.43294f),
                                new Utils.Vector4(-795.5344f, 3453.274f, -177.0266f + 3.7f * 2, 0.7257882f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-795.5585f, 3456.655f, -177.0267f + 3.7f * 3, 183.1305f),
                                new Utils.Vector4(-794.3295f, 3455.99f, -177.0267f + 3.7f * 3, 87.74728f),
                                new Utils.Vector4(-794.4268f, 3453.915f, -177.0266f + 3.7f * 3, 83.43294f),
                                new Utils.Vector4(-795.5344f, 3453.274f, -177.0266f + 3.7f * 3, 0.7257882f),
                            },
                        },
                    }
                },

                {
                    ShellTypes.LowEnd_5_0,

                    new ShellData()
                    {
                        StartFloor = 2,

                        FloorsAmount = 5,

                        EnterPosition = new Utils.Vector4(-802.7189f, 3439.903f, -179.0001f, 266.5319f),

                        ElevatorPositions = new Utils.Vector4[][]
                        {

                        },

                        ApartmentsPositions = new Utils.Vector4[][]
                        {
                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-795.5585f, 3441.655f, -177.0267f, 183.1305f),
                                new Utils.Vector4(-794.3295f, 3440.99f, -177.0267f, 87.74728f),
                                new Utils.Vector4(-794.4268f, 3438.915f, -177.0266f, 83.43294f),
                                new Utils.Vector4(-795.5344f, 3438.274f, -177.0266f, 0.7257882f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-795.5585f, 3441.655f, -177.0267f + 3.7f * 1, 183.1305f),
                                new Utils.Vector4(-794.3295f, 3440.99f, -177.0267f + 3.7f * 1, 87.74728f),
                                new Utils.Vector4(-794.4268f, 3438.915f, -177.0266f + 3.7f * 1, 83.43294f),
                                new Utils.Vector4(-795.5344f, 3438.274f, -177.0266f + 3.7f * 1, 0.7257882f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-795.5585f, 3441.655f, -177.0267f + 3.7f * 2, 183.1305f),
                                new Utils.Vector4(-794.3295f, 3440.99f, -177.0267f + 3.7f * 2, 87.74728f),
                                new Utils.Vector4(-794.4268f, 3438.915f, -177.0266f + 3.7f * 2, 83.43294f),
                                new Utils.Vector4(-795.5344f, 3438.274f, -177.0266f + 3.7f * 2, 0.7257882f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-795.5585f, 3441.655f, -177.0267f + 3.7f * 3, 183.1305f),
                                new Utils.Vector4(-794.3295f, 3440.99f, -177.0267f + 3.7f * 3, 87.74728f),
                                new Utils.Vector4(-794.4268f, 3438.915f, -177.0266f + 3.7f * 3, 83.43294f),
                                new Utils.Vector4(-795.5344f, 3438.274f, -177.0266f + 3.7f * 3, 0.7257882f),
                            },

                            new Utils.Vector4[]
                            {
                                new Utils.Vector4(-795.5585f, 3441.655f, -177.0267f + 3.7f * 4, 183.1305f),
                                new Utils.Vector4(-794.3295f, 3440.99f, -177.0267f + 3.7f * 4, 87.74728f),
                                new Utils.Vector4(-794.4268f, 3438.915f, -177.0266f + 3.7f * 4, 83.43294f),
                                new Utils.Vector4(-795.5344f, 3438.274f, -177.0266f + 3.7f * 4, 0.7257882f),
                            },
                        },
                    }
                },
            };

            public static ShellData GetShellDataByType(ShellTypes sType) => Shells.GetValueOrDefault(sType);

            public uint Id { get; private set; }

            public ShellTypes ShellType { get; private set; }

            public ShellData Shell => GetShellDataByType(ShellType);

            public Utils.Vector4 EnterParams { get; private set; }

            /// <summary>Измерение многоквартирного дома</summary>
            public uint Dimension { get; set; }

            public ApartmentsRoot(uint Id, ShellTypes ShellType, Utils.Vector4 EnterParams)
            {
                this.Id = Id;

                this.ShellType = ShellType;

                this.EnterParams = EnterParams;

                this.Dimension = Settings.CurrentProfile.Game.ApartmentsRootDimensionBaseOffset + Id;

                All.Add(Id, this);
            }

            public static ApartmentsRoot Get(uint id) => All.GetValueOrDefault(id);

            public void SetPlayersInside(bool teleport, params Player[] players)
            {
                if (teleport)
                {
                    var pos = Shell.EnterPosition;

                    Utils.TeleportPlayers(pos.Position, false, Dimension, pos.RotationZ, true, players);
                }
                else
                {
                    NAPI.ClientEvent.TriggerClientEventToPlayers(players, "ARoot::Enter", Id);
                }
            }

            public void SetPlayersOutside(bool teleport, params Player[] players)
            {
                if (teleport)
                {
                    var pos = EnterParams;

                    Utils.TeleportPlayers(pos.Position, false, Settings.CurrentProfile.Game.MainDimension, pos.RotationZ, true, players);
                }
                else
                {
                    NAPI.ClientEvent.TriggerClientEventToPlayers(players, "ARoot::Exit");
                }
            }
        }

        /// <summary>Словарь всех квартир</summary>
        public static Dictionary<uint, Apartments> All { get; set; } = new Dictionary<uint, Apartments>();

        public static Dictionary<int, float> TaxCoeffs { get; set; }

        public uint RootId { get; }

        public ushort FloorIdx { get; }

        public ushort SubIdx { get; }

        public override Utils.Vector4 PositionParams => ApartmentsRoot.Get(RootId).Shell.GetApartmentsPosition(FloorIdx, SubIdx);

        /// <summary>Данные многоквартирного дома</summary>
        public ApartmentsRoot Root => ApartmentsRoot.Get(RootId);

        public Apartments(uint HID, uint RootId, ushort FloorIdx, ushort SubIdx, Style.RoomTypes RoomType, uint Price) : base(HID, Types.Apartments, RoomType)
        {
            this.RootId = RootId;

            this.FloorIdx = FloorIdx;
            this.SubIdx = SubIdx;

            this.Price = Price;

            this.Dimension = HID + Settings.CurrentProfile.Game.ApartmentsDimensionBaseOffset;

            All.Add(HID, this);
        }

        public static Apartments Get(uint id) => All.GetValueOrDefault(id);

        public override void UpdateOwner(PlayerData.PlayerInfo pInfo)
        {
            base.UpdateOwner(pInfo);

            Sync.World.SetSharedData($"Apartments::{Id}::OName", pInfo == null ? null : $"{pInfo.Name} {pInfo.Surname} [#{pInfo.CID}]");
        }

        public override bool IsEntityNearEnter(Entity entity) => entity.Dimension == Root.Dimension && entity.Position.DistanceIgnoreZ(PositionParams.Position) <= Settings.ENTITY_INTERACTION_MAX_DISTANCE;

        public override void SetPlayersInside(bool teleport, params Player[] players)
        {
            if (teleport)
            {
                var sData = StyleData;

                Utils.TeleportPlayers(sData.Position, false, Dimension, sData.Heading, true, players);
            }
            else
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(players, "House::Enter", ToClientJson());
            }
        }

        public override void SetPlayersOutside(bool teleport, params Player[] players)
        {
            if (teleport)
            {
                var root = Root;

                var pos = PositionParams;

                Utils.TeleportPlayers(pos.Position, false, root.Dimension, pos.RotationZ, true, players);
            }
            else
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(players, "House::Exit");
            }
        }

        public void SetPlayersOutsideOfRoot(bool teleport, params Player[] players)
        {
            if (teleport)
            {
                var root = Root;

                var pos = root.EnterParams;

                Utils.TeleportPlayers(pos.Position, false, Settings.CurrentProfile.Game.MainDimension, pos.RotationZ, true, players);
            }
            else
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(players, "House::Exit");
            }
        }

        public override void ChangeOwner(PlayerData.PlayerInfo pInfo, bool buyGov = false)
        {
            if (Owner != null)
            {
                Owner.PlayerData?.RemoveApartmentsProperty(this);
            }

            if (pInfo != null)
            {
                pInfo.PlayerData?.AddApartmentsProperty(this);

                var minBalance = Settings.MIN_PAID_HOURS_HOUSE_APS * (uint)Tax;

                if (buyGov && Balance < minBalance)
                    SetBalance(minBalance, null);
            }

            foreach (var x in Settlers.Keys)
                SettlePlayer(x, false, null);

            UpdateOwner(pInfo);

            MySQL.HouseUpdateOwner(this);
        }
    }
}
