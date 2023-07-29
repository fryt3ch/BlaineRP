using System;
using BlaineRP.Client.Extensions.RAGE;
using BlaineRP.Client.Game.Management.Camera;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Utils.Game
{
    public static class Raycast
    {
        [Flags]
        public enum TraceFlags : uint
        {
            None = 0,
            IntersectWorld = 1,
            IntersectVehicles = 2,
            IntersectPedsSimpleCollision = 4,
            IntersectPeds = 8,
            IntersectObjects = 16,
            IntersectWater = 32,
            Unknown = 128,
            IntersectFoliage = 256,
            IntersectEverything = 4294967295,
        }

        public static Entity GetEntityByRaycast(Vector3 startPos, Vector3 endPos, int ignoreHandle = 0, int flags = 31)
        {
            //RAGE.Game.Graphics.DrawLine(startPos.X, startPos.Y, startPos.Z, endPos.X, endPos.Y, endPos.Z, 255, 0, 0, 255);

            int hit = -1, endEntity = -1;

            var vector = new Vector3();

            int result = RAGE.Game.Shapetest.GetShapeTestResult(
                RAGE.Game.Shapetest.StartShapeTestCapsule(startPos.X, startPos.Y, startPos.Z, endPos.X, endPos.Y, endPos.Z, 0.25f, flags, ignoreHandle, 4),
                ref hit,
                vector,
                vector,
                ref endEntity
            );

            if (result != 2 || endEntity <= 0)
                return null;

            int type = RAGE.Game.Entity.GetEntityType(endEntity);

            switch (type)
            {
                // Ped
                case 1:
                    return (Entity)Misc.GetPlayerByHandle(endEntity, true) ?? Misc.GetPedByHandle(endEntity, true);
                // Vehicle
                case 2:
                    return Misc.GetVehicleByHandle(endEntity, true);
                // Object
                case 3:
                    return Misc.GetMapObjectByHandle(endEntity, true);
                default:
                    return null;
            }
        }

        public static Vector3 GetWaterIntersectionCoord(Vector3 startPos, Vector3 endPos, int flags, int ignoreHandle)
        {
            Vector3 pos = Vector3.Zero;

            if (!RAGE.Game.Invoker.Invoke<bool>(RAGE.Game.Natives.TestProbeAgainstAllWater, startPos.X, startPos.Y, startPos.Z, endPos.X, endPos.Y, endPos.Z, 128, pos))
                return null;

            int hit = -1, materialHash = 0, eHit = -1;

            var vector = new Vector3();

            RAGE.Game.Shapetest.GetShapeTestResultEx(RAGE.Game.Shapetest.StartShapeTestRay(startPos.X, startPos.Y, startPos.Z, pos.X, pos.Y, pos.Z, flags, ignoreHandle, 4),
                ref hit,
                vector,
                vector,
                ref materialHash,
                ref eHit
            );

            if (eHit > 0 || materialHash != 0)
                return null;

            return pos;
        }

        public static Vector3 GetNearestWaterIntersectionCoord(Vector3 startPos,
                                                               float heading,
                                                               Vector3 baseOffset,
                                                               float range,
                                                               float offsetStep,
                                                               float offsetZ,
                                                               float angleRotation = 90f,
                                                               float minimalWaterHeight = 1f,
                                                               int flags = 31,
                                                               int ignoreHandle = 0)
        {
            float t = 360f / angleRotation;

            float c = range / offsetStep + 1;

            startPos += baseOffset;

            for (var i = 0; i < t; i++)
            {
                for (var j = 1; j < c; j++)
                {
                    Vector3 endPos = Service.GetFrontOf(startPos, heading + angleRotation * i, offsetStep * j);

                    endPos.Z += offsetZ;

                    Vector3 pos = GetWaterIntersectionCoord(startPos, endPos, flags, ignoreHandle);

                    if (pos == null)
                        continue;

                    float wHeight = -1f;

                    RAGE.Game.Misc.GetGroundZFor3dCoord(pos.X, pos.Y, pos.Z, ref wHeight, false);

                    wHeight = pos.Z - wHeight;

                    if (wHeight >= minimalWaterHeight)
                        return pos;
                }
            }

            return null;
        }

        public static Vector3 FindEntityWaterIntersectionCoord(GameEntity gEntity,
                                                               Vector3 baseOffset,
                                                               float range,
                                                               float offsetStep,
                                                               float offsetZ,
                                                               float angleRotation = 90f,
                                                               float minimalWaterHeight = 1f,
                                                               int flags = 31)
        {
            return GetNearestWaterIntersectionCoord(RAGE.Game.Entity.GetEntityCoords(gEntity.Handle, false),
                RAGE.Game.Entity.GetEntityHeading(gEntity.Handle),
                baseOffset,
                range,
                offsetStep,
                offsetZ,
                angleRotation,
                minimalWaterHeight,
                flags,
                gEntity.Handle
            );
        }

        public static Entity GetEntityPedLookAt(PedBase ped, float distance)
        {
            Vector3 headCoord = ped.GetBoneCoords(12844, 0f, 0f, 0f);
            Vector3 screenCenterCoord = headCoord.MinimizeDistance(Graphics.GetWorldCoordFromScreenCoord(0.5f, 0.5f), distance);

            if (Settings.User.Other.RaytraceEnabled)
                RAGE.Game.Graphics.DrawLine(headCoord.X, headCoord.Y, headCoord.Z, screenCenterCoord.X, screenCenterCoord.Y, screenCenterCoord.Z, 255, 0, 0, 255);

            return GetEntityByRaycast(headCoord, screenCenterCoord, ped.Handle, 31);
        }

        public static Entity GetEntityPedPointsAt(PedBase ped, float distance)
        {
            Vector3 fingerCoord = ped.GetBoneCoords(26613, 0f, 0f, 0f);
            Vector3 screenCenterCoord = fingerCoord.MinimizeDistance(Graphics.GetWorldCoordFromScreenCoord(0.5f, 0.5f), distance);

            if (Settings.User.Other.RaytraceEnabled)
                RAGE.Game.Graphics.DrawLine(fingerCoord.X, fingerCoord.Y, fingerCoord.Z, screenCenterCoord.X, screenCenterCoord.Y, screenCenterCoord.Z, 0, 255, 0, 255);

            return GetEntityByRaycast(fingerCoord, screenCenterCoord, ped.Handle, 31);
        }
    }
}