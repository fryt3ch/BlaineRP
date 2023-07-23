using System;

namespace BlaineRP.Server.Web.SocketIO.Exceptions
{
    public class SocketIOResultException : Exception
    {
        public readonly byte ResultType;
        public readonly SocketIOClient.SocketIOResponse Response;

        public SocketIOResultException(byte resultType, SocketIOClient.SocketIOResponse response = null, string errorMsg = "") : base(errorMsg)
        {
            ResultType = resultType;
            Response = response;
        }
    }
}
