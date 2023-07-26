using RAGE;

namespace BlaineRP.Client.Utils.Game
{
    public static class Graphics
    {
        /// <summary>Получить позицию на экране точки в игровом пространстве</summary>
        /// <remarks>Лучше не использовать для рендера, при каждом вызове создает объект класса Vector2</remarks>
        /// <param name="pos">Позиция</param>
        /// <returns>Vector2 - успешно, null - в противном случае</returns>
        public static RAGE.Ui.Cursor.Vector2 GetScreenCoordFromWorldCoord(Vector3 pos)
        {
            var result = new RAGE.Ui.Cursor.Vector2(0f, 0f);

            return !RAGE.Game.Graphics.GetScreenCoordFromWorldCoord(pos.X, pos.Y, pos.Z, ref result.X, ref result.Y) ? null : result;
        }

        public static void DrawSphere(Vector3 pos, float radius, byte r, byte g, byte b, float opacity)
        {
            RAGE.Game.Invoker.Invoke(0x799017F9E3B10112, pos.X, pos.Y, pos.Z, radius, r, g, b, opacity);
        }

        /// <summary>Получить позицию на экране точки в игровом пространстве</summary>
        /// <remarks>Облегченная версия метода, использовать для рендера</remarks>
        /// <param name="pos">Позиция</param>
        /// <returns>true - успешно, false - в противном случае</returns>
        public static bool GetScreenCoordFromWorldCoord(Vector3 pos, ref float x, ref float y)
        {
            return RAGE.Game.Graphics.GetScreenCoordFromWorldCoord(pos.X, pos.Y, pos.Z, ref x, ref y);
        }

        /// <summary>Функция для отрисовки текста на экране</summary>
        /// <remarks>Вызов необходимо осуществлять каждый кадр!</remarks>
        /// <param name="text">Текст</param>
        /// <param name="x">Координата по оси X (от 0 до 1)</param>
        /// <param name="y">Координата по оси X (от 0 до 1)</param>
        /// <param name="red">R</param>
        /// <param name="green">G</param>
        /// <param name="blue">B</param>
        /// <param name="alpha">Непрозрачность</param>
        /// <param name="scale">Масштаб</param>
        /// <param name="fontType">Шрифт</param>
        /// <param name="outline">Обводка</param>
        public static void DrawText(string text,
                                    float x,
                                    float y,
                                    byte red = 255,
                                    byte green = 255,
                                    byte blue = 255,
                                    byte alpha = 255,
                                    float scale = 0.4f,
                                    RAGE.Game.Font fontType = RAGE.Game.Font.ChaletComprimeCologne,
                                    bool outline = true,
                                    bool center = true)
        {
            RAGE.Game.Ui.SetTextProportional(true);

            RAGE.Game.Ui.SetTextEdge(1, 0, 0, 0, 255);

            RAGE.Game.Ui.SetTextFont((int)fontType);
            RAGE.Game.Ui.SetTextCentre(center);
            RAGE.Game.Ui.SetTextColour(red, green, blue, alpha);
            RAGE.Game.Ui.SetTextScale(scale, scale);

            //RAGE.Game.Ui.SetTextWrap(0f, 1f);

            if (outline)
                RAGE.Game.Ui.SetTextOutline();

            RAGE.Game.Ui.BeginTextCommandDisplayText("CELL_EMAIL_BCON");

            RAGE.Game.Ui.AddTextComponentSubstringPlayerName(text);

            RAGE.Game.Ui.EndTextCommandDisplayText(x, y, 0);
        }

        public static Vector3 GetWorldCoordFromScreenCoord(float x, float y, float maxDistance = 100f)
        {
            return GetWorldCoordFromScreenCoord(RAGE.Game.Cam.GetGameplayCamCoord(), RAGE.Game.Cam.GetGameplayCamRot(0), x, y, maxDistance);
        }

        /// <summary>Метод для преобразования координаты на экране в игровую координату</summary>
        /// <param name="camPos">Позиция камеры</param>
        /// <param name="camRot">Вектор вращения камеры</param>
        /// <param name="coord">
        ///     2D координата на экране (коэфициенты! например, при X = 960, а Y = 1080, а текущее разрешение
        ///     1920x1080 - передавать X = 0.5, Y = 1
        /// </param>
        /// <param name="maxDistance">Максимальная дистанция</param>
        public static Vector3 GetWorldCoordFromScreenCoord(Vector3 camPos, Vector3 camRot, float x, float y, float maxDistance = 100f)
        {
            Vector3 camForward = Geometry.RotationToDirection(camRot);

            Vector3 rotUp = camRot + new Vector3(maxDistance, 0, 0);
            Vector3 rotDown = camRot + new Vector3(-maxDistance, 0, 0);
            Vector3 rotLeft = camRot + new Vector3(0, 0, -maxDistance);
            Vector3 rotRight = camRot + new Vector3(0, 0, maxDistance);

            Vector3 camRight = Geometry.RotationToDirection(rotRight) - Geometry.RotationToDirection(rotLeft);
            Vector3 camUp = Geometry.RotationToDirection(rotUp) - Geometry.RotationToDirection(rotDown);

            float rollRad = -Geometry.DegreesToRadians(camRot.Y);

            Vector3 camRightRoll = camRight * (float)System.Math.Cos(rollRad) - camUp * (float)System.Math.Sin(rollRad);
            Vector3 camUpRoll = camRight * (float)System.Math.Sin(rollRad) + camUp * (float)System.Math.Cos(rollRad);

            Vector3 point3D = camPos + camForward * maxDistance + camRightRoll + camUpRoll;

            float point2dX = 0, point2dY = 0;

            if (!GetScreenCoordFromWorldCoord(point3D, ref point2dX, ref point2dY))
                return camPos + camForward * maxDistance;

            Vector3 point3DZero = camPos + camForward * maxDistance;

            float point2dZeroX = 0, point2dZeroY = 0;

            if (!GetScreenCoordFromWorldCoord(point3DZero, ref point2dZeroX, ref point2dZeroY))
                return camPos + camForward * maxDistance;

            const double eps = 0.001d;

            if (System.Math.Abs(point2dX - point2dZeroX) < eps || System.Math.Abs(point2dY - point2dZeroY) < eps)
                return camPos + camForward * maxDistance;

            float scaleX = (x - point2dZeroX) / (point2dX - point2dZeroX);
            float scaleY = (y - point2dZeroY) / (point2dY - point2dZeroY);

            return camPos + camForward * maxDistance + camRightRoll * scaleX + camUpRoll * scaleY;
        }
    }
}