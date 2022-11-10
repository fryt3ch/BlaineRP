using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BCRPServer
{
    class ServerEvents : Script
    {
        public static Queue<uint> FreeItemUIDs { get; set; }
        public static Queue<uint> FreeContainersIDs { get; set; }
        public static Queue<int> FreeVehiclesIDs { get; set; }

        public static List<string> UsedNumberplates { get; set; } = new List<string>();
        public static List<string> UsedWeaponTags { get; set; } = new List<string>();

        public static Dictionary<int, PlayerData.PlayerInfo> AllPlayers { get; set; }

        public static bool IsRestarting = false;

        /// <summary>Кол-во средств, которое получит безработный игрок</summary>
        private const int JoblessBenefits = 500;

        #region On Start
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            var currentTime = Utils.GetCurrentTime();

            Utils.ConsoleOutput("~Red~###########################################################################################~/~");

            Utils.ConsoleOutput($"~White~Blaine~/~ ~Blue~Role~/~ ~Red~Play~/~ server mode | Developed by ~Red~frytech~/~ | Version: ~Green~{Settings.VERSION}~/~");

            Utils.ConsoleOutput();

            try
            {
                Utils.ConsoleOutput("~Red~[BRPMode]~/~ Copying .cs files to client_resources...");

                DirectoryInfo ClientCSPackagesTarget = new DirectoryInfo(Settings.DIR_CLIENT_PACKAGES_CS_PATH);
                DirectoryInfo ClientCSPackagesSource = new DirectoryInfo(Settings.DIR_CLIENT_SOURCES_PATH);

                ClientCSPackagesTarget.Delete(true);
                ClientCSPackagesTarget.Create();

                foreach (var script in ClientCSPackagesSource.GetFiles("*.cs"))
                    File.Copy(script.FullName, ClientCSPackagesTarget.FullName + "\\" + script.Name, true);

                foreach (var dir in ClientCSPackagesSource.GetDirectories().Where(x => x.Name != "bin" && x.Name != "obj"))
                {
                    var newSubDir = ClientCSPackagesTarget.CreateSubdirectory(dir.Name);

                    foreach (var file in dir.GetFiles("*.cs"))
                        File.Copy(file.FullName, newSubDir.FullName + "\\" + file.Name, true);
                }
            }
            catch (Exception ex)
            {

            }

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Establishing connection with databases");
            Utils.ConsoleOutput($" | {(MySQL.InitConnection() ? "~Green~Success~/~" : "~Red~Error~/~")}", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Setting offline status to all characters");
            Utils.ConsoleOutput($" | ~Red~[{MySQL.SetOfflineAll()}]~/~", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Setting main server configuration");

            NAPI.Server.SetAutoRespawnAfterDeath(false);
            NAPI.Server.SetAutoSpawnOnConnect(false);
            NAPI.Server.SetGlobalServerChat(false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Loadings global dimension blips");
            Utils.ConsoleOutput($" | ~Red~[{Game.Map.Blips.LoadAll()}]~/~", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Setting global dimension weather");

            NAPI.World.SetWeather((Weather)new Random().Next(0, 10));

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Loading all items");
            Utils.ConsoleOutput($" | ~Red~[{Game.Items.Items.LoadAll()}]~/~", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Loading all vehicles");
            Utils.ConsoleOutput($" | ~Red~[{Game.Data.Vehicles.LoadAll()}]~/~", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Clearing unused items & Getting free items UID's");
            var deletedItemsCount = MySQL.LoadFreeItemsUIDs();
            Utils.ConsoleOutput($" | ~Red~Free UID's: [{FreeItemUIDs.Count}] | Cleared items amount: [{deletedItemsCount}]~/~", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Getting free containers UID's");
            MySQL.LoadFreeContainersUIDs();
            Utils.ConsoleOutput($" | ~Red~[{FreeContainersIDs.Count}]~/~", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Getting free vehicles UID's");
            MySQL.LoadFreeVehiclesUIDs();
            Utils.ConsoleOutput($" | ~Red~[{FreeVehiclesIDs.Count}]~/~", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Loading players information");
            MySQL.LoadPlayersInfo();
            Utils.ConsoleOutput($" | ~Red~[{AllPlayers.Count}]~/~", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Loading all businesses");
            Utils.ConsoleOutput($" | ~Red~[{Game.Businesses.Business.LoadAll()}]~/~", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Loading all houses");
            Utils.ConsoleOutput($" | ~Red~[{Game.Houses.House.LoadAll()}]~/~", false);

            Utils.ConsoleOutput("~Red~###########################################################################################~/~");
            Utils.ConsoleOutput();

            Additional.ConsoleCommands.Activate();

            UsedWeaponTags = new List<string>();
            UsedNumberplates = new List<string>();

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);

                    var currentTime = Utils.GetCurrentTime();

                    if (currentTime.Minute == 0 && currentTime.Second == 0)
                    {
                        var minSessionTimeForPaydayMinutes = Settings.MIN_SESSION_TIME_FOR_PAYDAY / 60;

                        NAPI.Task.Run(() =>
                        {
                            foreach (var pData in PlayerData.Players.Values)
                            {
                                if (pData?.Player?.Exists != true)
                                    return;

                                Sync.Chat.SendServer("PayDay", pData.Player);

                                Task.Run(async () =>
                                {
                                    if (!await pData.WaitAsync())
                                        return;

                                    var player = pData.Player;

                                    if (pData.LastData.SessionTime < Settings.MIN_SESSION_TIME_FOR_PAYDAY)
                                    {
                                        NAPI.Task.RunSafe(() =>
                                        {
                                            player?.Notify("PayDay::FailTime", minSessionTimeForPaydayMinutes, pData.LastData.SessionTime);
                                        });

                                        pData.Release();

                                        return;
                                    }

                                    pData.LastData.SessionTime = 0;

                                    if (pData.BankAccount == null)
                                    {
                                        NAPI.Task.RunSafe(() =>
                                        {
                                            player?.Notify("PayDay::FailBank");
                                        });

                                        pData.Release();

                                        return;
                                    }

                                    if (pData.Fraction == PlayerData.FractionTypes.None)
                                    {
                                        await pData.AddCash(JoblessBenefits);
                                    }

                                    pData.Release();
                                });
                            }
                        });
                    }

                    if (currentTime.Minute == 30 && currentTime.Second == 0)
                    {
                        var newWeather = Settings.Weathers[(new Random().Next(0, Settings.Weathers.Count))];

                        NAPI.Task.Run(() =>
                        {
                            NAPI.World.SetWeather(newWeather);
                        });
                    }
                }
            });
        }
        #endregion

        #region Update
        [ServerEvent(Event.Update)]
        public void OnUpdate()
        {
            var currentTime = Utils.GetCurrentTime().AddHours(2);

            NAPI.World.SetTime(currentTime.Hour, currentTime.Minute, currentTime.Second);
        }
        #endregion
    }
}
