using RAGE;

namespace BlaineRP.Client.Utils
{
    internal static class Geometry
    {
        /// <summary>Метод для преобразования радиан в градусы</summary>
        /// <param name="radians">Радианы</param>
        public static float RadiansToDegrees(float radians) => (float)(180f / System.Math.PI) * radians;

        /// <summary>Метод для преобразования градусов в радианы</summary>
        /// <param name="degrees">Градусы</param>
        public static float DegreesToRadians(float degrees) => (float)(System.Math.PI / 180f) * degrees;

        /// <summary>Метод для полуечения противоположного угла</summary>
        /// <param name="angle">Угол</param>
        public static float GetOppositeAngle(float angle) => (angle + 180) % 360;

        /// <summary>Метод для преобразования вектора поворота в вектор направления</summary>
        /// <param name="rotation">Вектор вращения</param>
        /// <returns></returns>
        public static Vector3 RotationToDirection(Vector3 rotation)
        {
            var z = DegreesToRadians(rotation.Z);
            var x = DegreesToRadians(rotation.X);

            var num = System.Math.Abs(System.Math.Cos(x));

            return new Vector3((float)(-System.Math.Sin(z) * num), (float)(System.Math.Cos(z) * num), (float)System.Math.Sin(x));
        }

        public static Vector3 GetRotationToFacePointTo(Vector3 position, Vector3 target)
        {
            var direction = target - position;

            var pitch = RadiansToDegrees((float)System.Math.Atan2(direction.Z, System.Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y)));
            var roll = RadiansToDegrees((float)System.Math.Atan2(direction.Z, System.Math.Sqrt(direction.Y * direction.Y + direction.Z * direction.Z)));
            var yaw = RadiansToDegrees((float)System.Math.Atan2(direction.Y, direction.X)) - 90f; // subtract 90f to fit game coord system

            return new Vector3(pitch, roll, yaw);
        }

        public static float GetRotationZToFacePointTo(Vector3 position, Vector3 target) => RadiansToDegrees((float)System.Math.Atan2(target.Y - position.Y, target.X - position.X)) - 90f;

        public static void RotatePoint(Vector3 point, Vector3 originPoint, float angle)
        {
            angle = (float)(angle * System.Math.PI / 180);

            float x = point.X, y = point.Y;
            float cos = (float)System.Math.Cos(angle), sin = (float)System.Math.Sin(angle);

            point.X = cos * (x - originPoint.X) - sin * (y - originPoint.Y) + originPoint.X;
            point.Y = sin * (x - originPoint.X) + cos * (y - originPoint.Y) + originPoint.Y;
        }
    }
}
