using GTANetworkAPI;

namespace BCRPServer.Sync
{
    class Chat
    {
        public enum Types
        {
            /// <summary>/say</summary>
            Say,
            /// <summary>/s</summary>
            Shout,
            /// <summary>/w</summary>
            Whisper,
            /// <summary>/n - OOC чат</summary>
            NonRP,
            /// <summary>/me</summary>
            Me,
            /// <summary>/do</summary>
            Do,
            /// <summary>/todo</summary>
            Todo,
            /// <summary>/try</summary>
            Try,
            /// <summary>/f /r</summary>
            Fraction,
            /// <summary>/o</summary>
            Organisation,
            /// <summary>/d</summary>
            Department,
            /// <summary>/gov</summary>
            Goverment,
            /// <summary>/amsg</summary>
            Admin,

            Ban,
            BanHard,
            Kick,
            Mute,
            Jail,
            Warn,
            UnBan,
            UnMute,
            UnJail,
            UnWarn,
            News,
            Advert,
        }

        #region Send Global
        /// <summary>Метод для отправки глобального сообщения в чат со стороны игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="type">Тип сообщения</param>
        /// <param name="sender">Сущность отправителя</param>
        /// <param name="message">Сообщение</param>
        /// <param name="targetStr">Строка цели</param>
        /// <param name="time">Время (для наказаний)</param>
        public static void SendGlobal(Types type, string senderStr, string message, string targetStr = null, string time = null)
        {
            NAPI.ClientEvent.TriggerClientEventForAll("Chat::ShowGlobalMessage", senderStr, (int)type, message, targetStr, time);
        }
        #endregion

        #region Send Server
        /// <summary>Метод для отправки сообщения конкретному игроку со стороны сервера</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="msg">Сообщение</param>
        /// <param name="target">Сущность игрока</param>
        public static void SendServer(string msg, Player target) => target?.TriggerEvent("Chat::ShowServerMessage", msg);

        /// <summary>Метод для отправки сообщения всем игрокам со стороны сервера</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="msg">Сообщение</param>
        public static void SendServer(string msg) => NAPI.ClientEvent.TriggerClientEventForAll("Chat::ShowServerMessage", msg);
        #endregion

        #region Send Local
        /// <summary>Метод для отправки локального сообщения в чат со стороны игрока</summary>
        /// <param name="type">Тип сообщения</param>
        /// <param name="sender">Сущность отправителя</param>
        /// <param name="message">Сообщение</param>
        /// <param name="target">Сущность цели</param>
        /// <returns>true/false если type = Try, true - в любом другом случае</returns>
        public static bool SendLocal(Types type, Player sender, string message, Player target = null)
        {
            var range = type == Types.Whisper ? Settings.CHAT_MAX_RANGE_WHISPER : type == Types.Shout ? Settings.CHAT_MAX_RANGE_LOUD : Settings.CHAT_MAX_RANGE_DEFAULT;

            if (type != Types.Try)
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
                var result = SRandom.NextInt32(0, 2) != 0;

                if (target != null)
                    sender.TriggerEventInDistance(range, "Chat::SCM", sender.Id, (int)type, message + $"*{(result ? 1 : 0)}", target.Id);
                else
                    sender.TriggerEventInDistance(range, "Chat::SCM", sender.Id, (int)type, message + $"*{(result ? 1 : 0)}");

                return result;
            }

            return true;
        }
        #endregion
    }
}
