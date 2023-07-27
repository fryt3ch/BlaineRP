using System;
using GTANetworkAPI;

namespace BlaineRP.Server.Extensions.GTANetworkAPI
{
    public static class Vector3Extensions
    {
        /// <summary>Найти расстояние между двумя точками в 3D пространстве</summary>
        /// <remarks>Игнорирует ось Z</remarks>
        /// <param name="pos1">Точка 1</param>
        /// <param name="pos2">Точка 2</param>
        public static float DistanceIgnoreZ(this global::GTANetworkAPI.Vector3 pos1, global::GTANetworkAPI.Vector3 pos2) => (float)Math.Sqrt((float)Math.Pow(pos1.X - pos2.X, 2) + (float)Math.Pow(pos1.Y - pos2.Y, 2));
    }
}