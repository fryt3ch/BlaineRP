using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlaineRP.Server.Additional
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

                        await Main.OnServerShutdown();

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
                            Utils.Kick(player, "Вы были кикнуты!");
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
                    Main.IsRestarting = true;
                }
            },

            { "openjoining", () =>
                {
                    Main.IsRestarting = false;
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
