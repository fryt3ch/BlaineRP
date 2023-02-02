using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    Active = false;

                    NAPI.Task.Run(async () =>
                    {
                        Console.WriteLine("Server is shutting down...");

                        await Events.Server.OnServerShutdown();

                        Environment.Exit(0);
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

            { "kill", () =>
                {
                    NAPI.Task.Run(() =>
                    {
                        NAPI.Pools.GetAllPlayers().FirstOrDefault()?.SetHealth(0);
                    });
                }
            },

            #region Close / Open Access To Server
            { "closejoining", () =>
                {
                    Events.Server.IsRestarting = true;
                }
            },

            { "openjoining", () =>
                {
                    Events.Server.IsRestarting = false;
                }
            },

                        { "vrep", () =>
                {
                    NAPI.Task.Run(() =>
                    {
                        NAPI.Pools.GetAllVehicles().FirstOrDefault()?.SetFixed();
                    });
                } },

                                        { "vrepv", () =>
                {
                    NAPI.Task.Run(() =>
                    {
                        NAPI.Pools.GetAllVehicles().FirstOrDefault()?.SetVisualFixed();
                    });
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
