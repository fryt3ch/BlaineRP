using System;
using RAGE;

namespace BlaineRP.Client.Extensions.RAGE
{
    public static class Vector3Extensions
    {
        public static Vector3 GetFrontOf(this Vector3 pos, float rotationZ = 0f, float coeffXY = 1.2f)
        {
            double radians = -rotationZ * Math.PI / 180;

            return new Vector3(pos.X + (float)(coeffXY * Math.Sin(radians)), pos.Y + (float)(coeffXY * Math.Cos(radians)), pos.Z);
        }

        /// <summary>Найти расстояние между двумя точками в 3D пространстве</summary>
        /// <remarks>Игнорирует ось Z</remarks>
        /// <param name="pos1">Точка 1</param>
        /// <param name="pos2">Точка 2</param>
        public static float DistanceIgnoreZ(this Vector3 pos1, Vector3 pos2)
        {
            return (float)Math.Sqrt((float)Math.Pow(pos1.X - pos2.X, 2) + (float)Math.Pow(pos1.Y - pos2.Y, 2));
        }

        /// <summary>Уменьшить расстояние между двумя точками до максимально возможной</summary>
        /// <param name="pos1">Точка 1 (источник)</param>
        /// <param name="pos2">Точка 2 (конечная)</param>
        /// <return>Измененная (если требуется) точка 2 (конечная), которая будет ближе к точке 1 (источник)</return>
        public static Vector3 MinimizeDistance(this Vector3 pos1, Vector3 pos2, float maxDistance)
        {
            float distance = pos1.DistanceTo(pos2);

            if (distance <= maxDistance)
                return pos2;

            Vector3 vector = pos2 - pos1;

            pos2 = pos1 + vector * (maxDistance / (distance - maxDistance));

            return pos2;
        }

        /// <summary>Найти середину между двумя точками в 3D пространстве</summary>
        /// <param name="pos1">Точка 1</param>
        /// <param name="pos2">Точка 2</param>
        public static Vector3 GetMiddle(this Vector3 pos1, Vector3 pos2)
        {
            return new Vector3((pos1.X + pos2.X) / 2f, (pos1.Y + pos2.Y) / 2f, (pos1.Z + pos2.Z) / 2f);
        }
    }
}