using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BlaineRP.Server.Web.SocketIO
{
    internal class Service
    {
        private static SocketIOClient.SocketIO _connection;

        public static SocketIOClient.SocketIO Connection => _connection;

        public static async void Start(string url, string user, string password)
        {
            _connection = new SocketIOClient.SocketIO(url, new SocketIOClient.SocketIOOptions()
            {
                Path = "/socket.io",
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,

                EIO = SocketIOClient.EngineIO.V4,

                Reconnection = true,

                ConnectionTimeout = TimeSpan.FromMilliseconds(5_000),

                Auth = new { user = user, password = password, },
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
                    _connection.ConnectAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Task.Delay(1_000);

                    Console.WriteLine($"[Socket.IO | Server] Error, trying to reconnect... ({ex?.Message ?? "null"})");
                }
            }

            foreach (var method in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsClass && x.Namespace?.StartsWith("BCRPServer.Web.SocketIO.Events") == true).SelectMany(x => x.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)))
            {
                var attr = method.GetCustomAttribute<Web.SocketIO.Events.EventAttribute>();

                if (attr == null)
                    continue;

                var deleg = (Action<SocketIOClient.SocketIOResponse>)method.CreateDelegate(typeof(Action<SocketIOClient.SocketIOResponse>));

                _connection.On(attr.EventName, deleg);
            }
        }
    }
}
