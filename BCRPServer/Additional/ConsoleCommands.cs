using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.Additional
{
    class ConsoleCommands
    {
        private static bool Active = false;

        #region All Commands
        public static Dictionary<string, Action> List = new Dictionary<string, Action>()
        {
            #region Exit
            { "exit", () =>
                {
                    ServerEvents.IsRestarting = true;
                    Active = false;

                    NAPI.Task.Run(() =>
                    {
                        Console.WriteLine("Server is shutting down...");

                        foreach (var player in NAPI.Pools.GetAllPlayers())
                            Utils.KickSilent(player, "Сервер был отключён!", 2000);

                        NAPI.Task.Run(() =>
                        {
                            Environment.Exit(0);
                        }, Settings.SERVER_STOP_DELAY);
                    });
                }
            },
            #endregion

            #region Kick All
            { "kickall", () =>
                {
                    NAPI.Task.Run(() =>
                    {
                        Console.WriteLine("Kicking all players...");

                        foreach (var player in NAPI.Pools.GetAllPlayers())
                            Utils.KickSilent(player, "Вы были кикнуты!", 2000);
                    });
                }
            },
            #endregion

            #region Close / Open Access To Server
            { "closejoining", () =>
                {
                    ServerEvents.IsRestarting = true;
                }
            },

            { "openjoining", () =>
                {
                    ServerEvents.IsRestarting = false;
                }
            },
            #endregion
        };
        #endregion

        public static void Activate()
        {
            if (Active)
                return;

            Active = true;

            Task.Run(() =>
            {
                while (Active)
                {
                    try
                    {
                        var cmd = Console.ReadLine();

                        if (!Additional.ConsoleCommands.List.ContainsKey(cmd))
                        {
                            Console.WriteLine("Command not found!");

                            continue;
                        }

                        Additional.ConsoleCommands.List[cmd].Invoke();
                    }
                    catch (Exception ex) { }
                }

                Active = false;
            });
        }
    }
}
