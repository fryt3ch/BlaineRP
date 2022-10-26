using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BCRPServer.Additional
{
    class Events
    {
        public enum RemoteEventTypes
        {
            AdminOnly = -1,
            PlayerHasTempData = 0,
            PlayerHasData,

        }

        public enum EventAwaitTypes
        {
            NAPI = 0,
            Custom,
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class RemoteEventAttribute : Attribute
        {
            public RemoteEventAttribute(string Name, RemoteEventTypes Type, EventAwaitTypes AwaitType = EventAwaitTypes.Custom, int SpamTimeout = 1000)
            {

            }
        }

        private class Instance
        {
            /// <summary>Данные команды</summary>
            public RemoteEventAttribute Attribute { get; }
            /// <summary>Параметры команды</summary>
            public ParameterInfo[] Parameters { get; }
            /// <summary>Данные метода команды</summary>
            public MethodInfo MethodInfo { get; }

            public Instance(MethodInfo MethodInfo)
            {
                this.MethodInfo = MethodInfo;

                this.Parameters = MethodInfo.GetParameters();
                this.Attribute = MethodInfo.GetCustomAttribute<RemoteEventAttribute>();
            }
        }

        [RemoteEvent("", RemoteEventTypes.PlayerHasData)]
        private static void Aasd(Player player)
        {

        }
    }
}
