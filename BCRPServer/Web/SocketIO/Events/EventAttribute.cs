using System;

namespace BCRPServer.Web.SocketIO.Events
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class EventAttribute : Attribute
    {
        public readonly string EventName;

        public EventAttribute(string eventName)
        {
            this.EventName = eventName;
        }
    }
}
