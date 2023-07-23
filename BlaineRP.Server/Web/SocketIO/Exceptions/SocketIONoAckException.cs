﻿using System;

namespace BlaineRP.Server.Web.SocketIO.Exceptions
{
    public class SocketIONoAckException : Exception
    {
        public SocketIONoAckException() : base(message: "No ack received by target server, was it expected?")
        {

        }
    }
}
