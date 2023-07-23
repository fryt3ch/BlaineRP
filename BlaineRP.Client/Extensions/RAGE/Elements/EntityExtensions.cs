using System;
using System.Collections.Generic;
using System.Text;
using RAGE.Elements;

namespace BlaineRP.Client.ExtensionsT.RAGE.Elements
{
    internal static class EntityExtensions
    {
        /// <summary>Получить серверную информацию сущности</summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="entity">Сущность</param>
        /// <param name="key">Ключ</param>
        /// <param name="otherwise">Возвращаемая информация в случае отсутствия у сущности таковой</param>
        /// <return>Информация о сущности по ключу (если таковой нет - T otherwise)</return>
        public static T GetSharedData<T>(this Entity entity, string key, T otherwise = default(T)) => (T)(entity.GetSharedData(key) ?? otherwise);

        /// <summary>Проверить, имеет ли сущность информацию</summary>
        /// <param name="entity">Сущность</param>
        /// <param name="key">Ключ</param>
        /// <return>true / false</return>
        public static bool HasSharedData(this Entity entity, string key) => entity.GetSharedData(key) != null;
    }
}
