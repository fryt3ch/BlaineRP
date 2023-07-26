using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;
using RAGE.Ui;

namespace BlaineRP.Client.Extensions.RAGE.Elements
{
    internal static class EntityExtensions
    {
        /// <summary>Получить серверную информацию сущности</summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="entity">Сущность</param>
        /// <param name="key">Ключ</param>
        /// <param name="otherwise">Возвращаемая информация в случае отсутствия у сущности таковой</param>
        /// <return>Информация о сущности по ключу (если таковой нет - T otherwise)</return>
        public static T GetSharedData<T>(this Entity entity, string key, T otherwise = default)
        {
            return (T)(entity.GetSharedData(key) ?? otherwise);
        }

        /// <summary>Проверить, имеет ли сущность информацию</summary>
        /// <param name="entity">Сущность</param>
        /// <param name="key">Ключ</param>
        /// <return>true / false</return>
        public static bool HasSharedData(this Entity entity, string key)
        {
            return entity.GetSharedData(key) != null;
        }

        public static bool IsEntityNear(this Entity entity, float maxDistance)
        {
            return Player.LocalPlayer.Dimension == entity.Dimension && Vector3.Distance(Player.LocalPlayer.Position, entity.Position) <= maxDistance;
        }

        /// <summary>Метод для получения позиции сущности на экране</summary>
        /// <param name="entity">Сущность</param>
        /// <returns>Vector2 - успешно, null - в противном случае</returns>
        public static Cursor.Vector2 GetScreenPosition(this Entity entity)
        {
            return Graphics.GetScreenCoordFromWorldCoord(entity.GetRealPosition());
        }

        /// <summary>Метод для получения позиции сущности на экране</summary>
        /// <remarks>Облегченная версия метода, использовать для рендера</remarks>
        /// <param name="entity">Сущность</param>
        /// <returns>true - успешно, false - в противном случае</returns>
        public static bool GetScreenPosition(this Entity entity, ref float x, ref float y)
        {
            return Graphics.GetScreenCoordFromWorldCoord(entity.GetRealPosition(), ref x, ref y);
        }

        /// <summary>Метод для получения пределов размера модели сущности</summary>
        /// <remarks>
        ///     Необходимо посчитать (Maximum - Minimum) для получения размера модели, где Y - величина модели в длину. Если
        ///     нужно получить сразу размер модели - использовать GetModelSize()
        /// </remarks>
        /// <param name="entity">Сущность</param>
        /// <returns>Vector3 минимума и Vector3 максимума</returns>
        public static void GetModelDimensions(this Entity entity, out Vector3 min, out Vector3 max)
        {
            min = new Vector3(0f, 0f, 0f);
            max = new Vector3(0f, 0f, 0f);

            global::RAGE.Game.Misc.GetModelDimensions(entity.Model, min, max);
        }

        /// <summary>Метод для получения размера модели сущности</summary>
        /// <param name="entity">Сущность</param>
        /// <returns>Vector3, где X - размер в ширину, Y - в длину, Z - в высоту</returns>
        public static Vector3 GetModelSize(this Entity entity)
        {
            entity.GetModelDimensions(out Vector3 min, out Vector3 max);

            return max - min;
        }

        /// <summary>Метод для получения радиуса модели сущности</summary>
        /// <remarks>Фактически, получает максимальное значение из размеров по осям, полученных через GetModelSize</remarks>
        /// <param name="entity">Сущность</param>
        /// <returns>Радиус сущности</returns>
        public static float GetModelRange(this Entity entity)
        {
            Vector3 size = entity.GetModelSize();

            return size.X >= size.Y && size.X >= size.Z ? size.X : size.Y >= size.X && size.Y >= size.Z ? size.Y : size.Z;
        }

        public static Vector3 GetRealPosition(this Entity entity)
        {
            if (entity is GameEntity gEntity)
                return global::RAGE.Game.Entity.GetEntityCoords(gEntity.Handle, false);

            return entity.Position;
        }

        public static Vector3 GetWorldRotationOfBone(this GameEntityBase gEntity, int boneIdx)
        {
            return global::RAGE.Game.Invoker.Invoke<Vector3>(0xCE6294A232D03786, gEntity.Handle, boneIdx);
        }

        public static bool GetCanBeDamaged(this GameEntity gEntity)
        {
            return global::RAGE.Game.Invoker.Invoke<bool>(0xD95CC5D2AB15A09F, gEntity.Handle);
        }
    }
}