using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.Web
{
    internal class Service
    {
        private static SocketIO _connection;

        public static SocketIO Connection => _connection;

        public static async System.Threading.Tasks.Task Connect()
        {
            _connection = new SocketIO("http://localhost:7777", new SocketIOOptions()
            {
                Path = "/socket.io",
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,

                EIO = EngineIO.V4,

                Reconnection = true,

                ConnectionTimeout = TimeSpan.FromMilliseconds(5_000),

                Auth = new { user = "brp_server_1", password = "63c209c3-3505-443a-b234-91e3046e2894", },
            });

            _connection.OnConnected += (sender, args) =>
            {
                Console.WriteLine($"[Socket.IO | Server] Successfully connected!");
            };

            _connection.OnDisconnected += (sender, str) =>
            {
                Console.WriteLine($"[Socket.IO | Server] Disconnected! {str ?? "null"}");
            };

            _connection.OnReconnectAttempt += (sender, arg) =>
            {
                Console.WriteLine($"[Socket.IO | Server] Error, trying to reconnect... [{arg}]");
            };

            _connection.On("Registration::Confirmed", (resp) =>
            {

            });

            while (!_connection.Connected)
            {
                try
                {
                    await _connection.ConnectAsync();
                }
                catch (Exception ex)
                {
                    await Task.Delay(1_000);

                    Console.WriteLine($"[Socket.IO | Server] Error, trying to reconnect... ({ex?.Message ?? "null"})");
                }
            }

            await _connection.EmitAsync("test_event_server", (resp) =>
            {
                Console.WriteLine(resp.GetValue(0));

            }, (new { test_data1 = "PHP", test_data2 = "GOVNO" }));
        }
    }
}
