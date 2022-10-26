using RAGE;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BCRPClient.Additional
{
    class Storage : Events.Script
    {
        public static string LastData = null;
        public static bool GotData = false;

        public Storage()
        {
            Events.Add("Storage::Temp", (object[] args) =>
            {
                LastData = (string)args[0];

                GotData = true;
            });
        }

        /// <summary>Получить данные из ПК игрока</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Ключ (разделитель - ::)</param>
        public static T GetData<T>(string key)
        {
            LastData = null;
            GotData = false;

            Utils.JsEval($"mp.events.callLocal(\"Storage::Temp\", mp.storage.data.{key.Replace("::", "_")});");

            while (!GotData)
                RAGE.Game.Invoker.Wait(0);

            return LastData != null ? RAGE.Util.Json.Deserialize<T>(LastData) : default(T);
        }

        /// <summary>Сохранить данные локально на ПК игрока</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Ключ (разделитель - ::)</param>
        /// <param name="value">Значение</param>
        public static void SetData<T>(string key, T value) => Utils.JsEval($"mp.storage.data.{key.Replace("::", "_")} = '{RAGE.Util.Json.Serialize(value)}';");
    }
}
