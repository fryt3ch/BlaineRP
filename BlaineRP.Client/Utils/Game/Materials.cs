using RAGE;
using System;
using System.Collections.Generic;

namespace BlaineRP.Client.Utils.Game
{
    public static class Materials
    {
        public enum Types : uint
        {
            Unknown = 0,

            SandLoose = 2699818980,
            SandCompact = 510490462,
            SandWet = 909950165,
            SandDryDeep = 509508168,
            SandWetDeep = 1288448767,

            MudHard = 2352068586,
            MudPothole = 312396330,
            MudSoft = 1635937914,
            MudDeep = 1109728704,

            Soil = 3594309083,

            DirtTrack = 2409420175,

            GrassLong = 3833216577,
            Grass = 1333033863,
            GrassShort = 3008270349,

            GravelSmall = 951832588,
            GravelLarge = 2128369009,
            GravelDeep = 3938260814,
        }

        private static readonly HashSet<Types> CanBeDugTypes = new HashSet<Types>()
        {
            Types.SandLoose, Types.SandCompact, Types.SandWet, Types.SandDryDeep, Types.SandWetDeep,
            Types.MudHard, Types.MudPothole, Types.MudSoft, Types.MudDeep,

            Types.Soil,

            Types.DirtTrack,

            Types.GrassLong, Types.Grass, Types.GrassShort,

            Types.GravelSmall, Types.GravelLarge, Types.GravelDeep,
        };

        public static bool CanTypeBeDug(Types type) => CanBeDugTypes.Contains(type);

        public static Types GetTypeByRaycast(Vector3 startPos, Vector3 endPos, int ignoreHandle, int flags = 31)
        {
            int hit = -1, materialHash = 0;

            var vector = new Vector3();

            var result = RAGE.Game.Shapetest.GetShapeTestResultEx(RAGE.Game.Shapetest.StartShapeTestRay(startPos.X, startPos.Y, startPos.Z, endPos.X, endPos.Y, endPos.Z, 31, ignoreHandle, 4), ref hit, vector, vector, ref materialHash, ref hit);

            if (result != 2 || materialHash == 0)
                return Types.Unknown;

            var materialHashUInt = Convert.ToUInt32(materialHash);

            if (Enum.IsDefined(typeof(Types), materialHashUInt))
                return (Types)materialHashUInt;

            return Types.Unknown;
        }
    }
}
