using System;
using static RAGE.Ui.Cursor;

namespace BlaineRP.Client.Extensions.RAGE.Ui.Cursor
{
    internal static class Vector2Extensions
    {
        /// <summary>Найти расстояние между двумя точками в 2D пространстве</summary>
        /// <param name="pos1">Точка 1</param>
        /// <param name="pos2">Точка 2</param>
        public static float Distance(this Vector2 pos1, Vector2 pos2)
        {
            return (float)Math.Sqrt((float)Math.Pow(pos1.X - pos2.X, 2) + (float)Math.Pow(pos1.Y - pos2.Y, 2));
        }
    }
}