/*using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.Web
{
    internal static class LocalHubClient
    {
        private static HubConnection _hubConnection;

        private const string LocalHubBaseUrl = "http://localhost:5204/hubs/local";

        public static async Task StartService()
        {
            var t = new HubConnectionBuilder();

            _hubConnection = new HubConnectionBuilder().WithUrl(LocalHubBaseUrl, (options =>
            {
                options.AccessTokenProvider = (Func<Task<string?>>)(async () =>
                {
                    using (var client = new HttpClient())
                    {
                        var content = new FormUrlEncodedContent(new Dictionary<string, string>()
                        {
                            { "userId", "brp_server_1" },
                            { "password", "EHc%K?k0t*$u" },
                        });

                        try
                        {
                            var response = await client.PostAsync($"{LocalHubBaseUrl}/auth", content);

                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                var responseStr = await response.Content.ReadAsStringAsync();

                                var responseJson = JObject.Parse(responseStr);

                                return (string)responseJson["access_token"];
                            }
                            else
                            {
                                throw new Exception(response.StatusCode.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            return null;
                        }
                    }
                });
            })).AddNewtonsoftJsonProtocol().WithAutomaticReconnect().Build();

            _hubConnection.Reconnecting += (ex) =>
            {
                Console.WriteLine($"[SignalR | LocalHub] Error, trying to reconnect... {ex?.Message ?? "null"}");

                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += (connectionId) =>
            {
                if (!string.IsNullOrEmpty(connectionId))
                    Console.WriteLine($"[SignalR | LocalHub] Successfully reconnected! ConnectionId: {connectionId}");

                return Task.CompletedTask;
            };

            _hubConnection.Closed += (ex) =>
            {
                Console.WriteLine($"[SignalR | LocalHub] Error, connection was closed! {ex?.Message ?? "null"}");

                return Task.CompletedTask;
            };

            _hubConnection.On<string>("ReceiveData", (message) =>
            {

            });

            while (true)
            {
                try
                {
                    await _hubConnection.StartAsync();

                    if (_hubConnection.State == HubConnectionState.Connected && !string.IsNullOrEmpty(_hubConnection.ConnectionId))
                        Console.WriteLine($"[SignalR | LocalHub] Successfully connected! ConnectionId: {_hubConnection.ConnectionId}");

                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SignalR | LocalHub] Error, trying to reconnect... {ex?.Message ?? "null"}");

                    await Task.Delay(1_000);
                }
            }

            Task.Run(async () =>
            {
                await Task.Delay(10_000);

                await SendMessageToHub("aasdasda");
            });
        }

        public static async Task SendMessageToHub(string message)
        {
            await _hubConnection.InvokeAsync("SendMessage", message);
        }
    }
}
*/