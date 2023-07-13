using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Web.SocketIO.Exceptions
{
    public class SocketIONoAckException : Exception
    {
        public SocketIONoAckException() : base(message: "No ack received by target server, was it expected?")
        {

        }
    }
}
