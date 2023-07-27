using System;

namespace BlaineRP.Server.UtilsT
{
    public class Vector2
    {
        public float X { get; set; }

        public float Y { get; set; }

        public Vector2(float X = 0f, float Y = 0f)
        {
            this.X = X;
            this.Y = Y;
        }

        public float Distance(Vector2 pos1, Vector2 pos2) => (float)Math.Sqrt((float)Math.Pow(pos1.X - pos2.X, 2) + (float)Math.Pow(pos1.Y - pos2.Y, 2));
    }
}