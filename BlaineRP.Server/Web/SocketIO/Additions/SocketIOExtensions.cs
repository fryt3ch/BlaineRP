using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlaineRP.Server.Web.SocketIO
{
    public static class SocketIOExtensions
    {
        public static Task<SocketIOClient.SocketIOResponse> TriggerProc(this SocketIOClient.SocketIO socket, CancellationToken ct, string eventName, params object[] args)
        {
            var tcs = new TaskCompletionSource<SocketIOClient.SocketIOResponse>(null);

            if (ct != CancellationToken.None)
            {
                ct.Register(() =>
                {
                    tcs.TrySetCanceled();
                });
            }

            socket.EmitAsync(eventName, (resp) =>
            {
                tcs.TrySetResult(resp);
            }, args);

            return tcs.Task;
        }

        public static Task<SocketIOClient.SocketIOResponse> TriggerProc(this SocketIOClient.SocketIO socket, string eventName, params object[] args) => socket.TriggerProc(CancellationToken.None, eventName, args);

        public static Task TriggerEvent(this SocketIOClient.SocketIO socket, CancellationToken ct, string eventName, params object[] args)
        {
            return socket.EmitAsync(eventName, ct, args);
        }

        public static Task TriggerEvent(this SocketIOClient.SocketIO socket, string eventName, params object[] args) => socket.TriggerEvent(CancellationToken.None, eventName, args);
    }
}
