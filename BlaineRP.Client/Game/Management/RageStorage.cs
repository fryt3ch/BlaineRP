using BlaineRP.Client.Utils.Game;
using RAGE;

namespace BlaineRP.Client.Game.Management
{
    [Script(int.MaxValue)]
    public class RageStorage
    {
        public static string LastData = null;
        public static bool GotData = false;

        public RageStorage()
        {
            Events.Add("Storage::Temp",
                (object[] args) =>
                {
                    LastData = (string)args[0];

                    GotData = true;
                }
            );
        }

        /// <summary>Получить данные из ПК игрока</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Ключ (разделитель - ::)</param>
        public static T GetData<T>(string key)
        {
            LastData = null;
            GotData = false;
            Invoker.JsEval($"mp.events.callLocal(\"Storage::Temp\", mp.storage.data.{key.Replace("::", "_")});");

            while (!GotData)
            {
                RAGE.Game.Invoker.Wait(0);
            }

            return LastData != null ? RAGE.Util.Json.Deserialize<T>(LastData) : default(T);
        }

        /// <summary>Сохранить данные локально на ПК игрока</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Ключ (разделитель - ::)</param>
        /// <param name="value">Значение</param>
        public static void SetData<T>(string key, T value)
        {
            Invoker.JsEval($"mp.storage.data.{key.Replace("::", "_")} = '{RAGE.Util.Json.Serialize(value)}';");
        }
    }
}