using RAGE;

namespace BlaineRP.Client.Utils.Game
{
    internal static class Camera
    {

        /// <summary>Метод для получения координат, на которые смотрит игровая камера игрока</summary>
        /// <param name="distance">Дистанция</param>
        public static Vector3 GetCoordsFromCamera(float distance)
        {
            var coord = RAGE.Game.Cam.GetGameplayCamCoord();

            var rotation = RAGE.Game.Cam.GetGameplayCamRot(0);

            var tX = rotation.X * 0.0174532924f;
            var tZ = rotation.Z * 0.0174532924f;

            var num = (float)System.Math.Abs(System.Math.Cos(tX)) + distance;

            return new Vector3(coord.X + (float)-System.Math.Sin(tZ) * num, coord.Y + (float)System.Math.Cos(tZ) * num, coord.Z + (float)System.Math.Sin(tX) * 8.0f);
        }

        /// <summary>Метод проверяет, активен ли у локального игрока вид от первого лица</summary>
        public static bool IsFirstPersonActive() => RAGE.Game.Cam.GetFollowPedCamViewMode() == 4;

        public static void ResetGameplayCameraRotation()
        {
            RAGE.Game.Cam.SetGameplayCamRelativeHeading(0f);

            RAGE.Game.Invoker.Invoke(0x48608C3464F58AB4, 0f, 0f, 0f);
        }
    }
}
