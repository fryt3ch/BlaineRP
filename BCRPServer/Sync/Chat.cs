using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.Sync
{
    class Chat : Script
    {
        /// <summary>Генератор случайных чисел для /try</summary>
        private static Random Random = new Random(Utils.GetCurrentTime().Ticks.GetHashCode());

        #region All Types
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

            /// <summary>/me над игроком</summary>
            MePlayer,
            /// <summary>/try над игроком</summary>
            TryPlayer,

            Ban,
            BanHard,
            Kick,
            Mute,
            Jail,
            UnBan,
            UnMute,
            UnJail,
            News,
            Advert,
        }
        #endregion

        #region Send
        [RemoteEvent("Chat::Send")]
        public static void OnChatSend(Player player, int typeNum, string message)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Types), typeNum))
                return;

            Types type = (Types)typeNum;

            if (type > Types.Admin)
                return;

            if (type <= Types.Fraction)
            {
                SendLocal(type, player, message, null);
            }
            else if (type == Types.Goverment || type == Types.Admin) // add if of who can call
            {
                SendGlobal(type, player, message);
            }
        }
        #endregion

        #region Send Global
        /// <summary>Метод для отправки глобального сообщения в чат со стороны игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <param name="type">Тип сообщения</param>
        /// <param name="sender">Сущность отправителя</param>
        /// <param name="message">Сообщение</param>
        /// <param name="targetStr">Строка цели</param>
        /// <param name="time">Время (для наказаний)</param>
        public static void SendGlobal(Types type, Player sender, string message, string targetStr = null, string time = null)
        {
            NAPI.ClientEvent.TriggerClientEventForAll("Chat::ShowGlobalMessage", sender.Name, (int)type, message, targetStr, time);
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
            float range = Settings.CHAT_MAX_RANGE_DEFAULT;

            if (type == Types.Whisper)
                range = Settings.CHAT_MAX_RANGE_WHISPER;
            else if (type == Types.Shout)
                range = Settings.CHAT_MAX_RANGE_LOUD;

            if (type != Types.Try && type != Types.TryPlayer)
            {
                if (target != null)
                    NAPI.Task.Run(() =>
                    {
                        if (sender?.Exists != true)
                            return;

                        sender.TriggerEventInDistance(range, "Chat::ShowCasualMessage", sender.Handle, (int)type, message, target.Handle);
                    });
                else
                    NAPI.Task.Run(() =>
                    {
                        if (sender?.Exists != true)
                            return;

                        sender.TriggerEventInDistance(range, "Chat::ShowCasualMessage", sender.Handle, (int)type, message);
                    });
            }
            else
            {
                bool result = Random.Next(0, 2) != 0;

                NAPI.Task.Run(() =>
                {
                    if (sender?.Exists != true)
                        return;

                    if (target != null)
                        sender.TriggerEventInDistance(range, "Chat::ShowCasualMessage", sender.Handle, (int)type, message + $"*{(result ? "true" : "false")}", target.Handle);
                    else
                        sender.TriggerEventInDistance(range, "Chat::ShowCasualMessage", sender.Handle, (int)type, message + $"*{(result ? "true" : "false")}");
                });

                return result;
            }

            return true;
        }
        #endregion
    }
}
