using System.Text.RegularExpressions;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Management.Chat
{
    partial class Service
    {
        public static Regex MessageTodoRegex { get; } = new Regex(@".+\*.+");

        public static Regex MessageRegex { get; } = new Regex(@".{1,150}");
        
        /// <summary>Метод для отправки глобального сообщения в чат со стороны игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="type">Тип сообщения</param>
        /// <param name="sender">Сущность отправителя</param>
        /// <param name="message">Сообщение</param>
        /// <param name="targetStr">Строка цели</param>
        /// <param name="time">Время (для наказаний)</param>
        public static void SendGlobal(MessageType type, string senderStr, string message, string targetStr = null, string time = null)
        {
            NAPI.ClientEvent.TriggerClientEventForAll("Chat::ShowGlobalMessage", senderStr, (int)type, message, targetStr, time);
        }
        
        /// <summary>Метод для отправки сообщения конкретному игроку со стороны сервера</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="msg">Сообщение</param>
        /// <param name="target">Сущность игрока</param>
        public static void SendServer(string msg, Player target) => target?.TriggerEvent("Chat::ShowServerMessage", msg);

        /// <summary>Метод для отправки сообщения всем игрокам со стороны сервера</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="msg">Сообщение</param>
        public static void SendServer(string msg) => NAPI.ClientEvent.TriggerClientEventForAll("Chat::ShowServerMessage", msg);
        
        /// <summary>Метод для отправки локального сообщения в чат со стороны игрока</summary>
        /// <param name="type">Тип сообщения</param>
        /// <param name="sender">Сущность отправителя</param>
        /// <param name="message">Сообщение</param>
        /// <param name="target">Сущность цели</param>
        /// <returns>true/false если type = Try, true - в любом другом случае</returns>
        public static bool SendLocal(MessageType type, Player sender, string message, Player target = null, params object[] args)
        {
            var range = type == MessageType.Whisper ? Properties.Settings.Static.CHAT_MAX_RANGE_WHISPER : type == MessageType.Shout ? Properties.Settings.Static.CHAT_MAX_RANGE_LOUD : Properties.Settings.Static.CHAT_MAX_RANGE_DEFAULT;

            if (type != MessageType.Try)
            {
                if (target != null)
                {
                    sender.TriggerEventInDistance(range, "Chat::SCM", sender.Id, (int)type, message, target.Id);
                }
                else
                {
                    sender.TriggerEventInDistance(range, "Chat::SCM", sender.Id, (int)type, message);
                }
            }
            else
            {
                var result = args.Length > 0 && args[0] is bool resultB ? resultB : SRandom.NextInt32(0, 2) != 0;

                if (target != null)
                    sender.TriggerEventInDistance(range, "Chat::SCM", sender.Id, (int)type, message + $"*{(result ? 1 : 0)}", target.Id);
                else
                    sender.TriggerEventInDistance(range, "Chat::SCM", sender.Id, (int)type, message + $"*{(result ? 1 : 0)}");

                return result;
            }

            return true;
        }
    }
}
