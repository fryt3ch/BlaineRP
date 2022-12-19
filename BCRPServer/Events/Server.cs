using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.Events
{
    public class Server : Script
    {
        public static bool IsRestarting = false;

        /// <summary>Кол-во средств, которое получит безработный игрок</summary>
        private const int JoblessBenefits = 500;

        #region On Start
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            Sync.World.Initialize();

            var currentTime = Utils.GetCurrentTime();

            Utils.ConsoleOutput("~Red~###########################################################################################~/~");

            Utils.ConsoleOutput($"~Red~Blaine RolePlay~/~ server mode | Developed by ~Red~frytech~/~ | Version: ~Green~{Settings.VERSION}~/~");

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

            #region Settings Section
            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Establishing connection with databases");
            Utils.ConsoleOutput($" | {(MySQL.InitConnection() ? "~Green~Success~/~" : "~Red~Error~/~")}", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Setting offline status to all characters");
            Utils.ConsoleOutput($" | ~Red~[{MySQL.SetOfflineAll()}]~/~", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Setting main server configuration");

            NAPI.Server.SetAutoRespawnAfterDeath(false);
            NAPI.Server.SetAutoSpawnOnConnect(false);
            NAPI.Server.SetGlobalServerChat(false);

            NAPI.Server.SetGlobalDefaultCommandMessages(false);

            NAPI.Server.SetLogCommandParamParserExceptions(false);
            NAPI.Server.SetLogRemoteEventParamParserExceptions(true);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Loadings global dimension blips");
            Utils.ConsoleOutput($" | ~Red~[{Game.Map.Blips.LoadAll()}]~/~", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Setting global dimension weather");

            MySQL.StartService();
            #endregion

            #region Local Data Load Section
            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Loading all items");
            Utils.ConsoleOutput($" | ~Red~[{Game.Items.Items.LoadAll()}]~/~", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Loading all vehicles");
            Utils.ConsoleOutput($" | ~Red~[{Game.Data.Vehicles.LoadAll()}]~/~", false);
            #endregion

            Game.Houses.HouseBase.Style.LoadAll();
            Game.Houses.Garage.Style.LoadAll();

            Game.Bank.LoadAll();

            #region Database Data Load Section
            MySQL.LoadAll();
            MySQL.UpdateFreeUIDs();

            Game.Businesses.Business.LoadAll();

            Game.Houses.House.LoadAll();
            Game.Houses.Apartments.LoadAll();
            Game.Houses.Garage.LoadAll();

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Clearing unused items & Getting free items UID's");
            Utils.ConsoleOutput($" | ~Red~Free UID's: [{Game.Items.Item.FreeIDs.Count}]~/~", false);

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{Game.Items.Item.All.Count} items");

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{Game.Items.Container.All.Count} containers");

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{PlayerData.PlayerInfo.All.Count} players");

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{VehicleData.VehicleInfo.All.Count} vehicles");

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{Game.Businesses.Business.All.Count} businesses");

            //Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{Game.Items.Gift.All.Count} gifts");

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{Game.Houses.House.All.Count} houses");

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{Game.Houses.Apartments.All.Count} apartments");

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{Game.Houses.Garage.All.Count} garages");

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{Game.Houses.Furniture.All.Count} furniture");

            GC.Collect();
            #endregion

            Utils.ConsoleOutput("~Red~###########################################################################################~/~");
            Utils.ConsoleOutput();

            Additional.ConsoleCommands.Activate();

            Utils.SetWeather(Utils.WeatherTypes.CLEAR);

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
                            foreach (var pData in PlayerData.All.Values)
                            {
                                if (pData?.Player?.Exists != true)
                                    continue;

                                var player = pData.Player;

                                Sync.Chat.SendServer("PayDay", player);

                                if (pData.LastData.SessionTime < Settings.MIN_SESSION_TIME_FOR_PAYDAY)
                                {
                                    player.Notify("PayDay::FailTime", minSessionTimeForPaydayMinutes, pData.LastData.SessionTime);

                                    continue;
                                }

                                pData.LastData.SessionTime = 0;

                                if (pData.BankAccount == null)
                                {
                                    player.Notify("PayDay::FailBank");

                                    continue;
                                }

                                /*                                if (pData.Fraction == PlayerData.FractionTypes.None)
                                                                {
                                                                    pData.AddCash(JoblessBenefits);
                                                                }*/
                            }
                        });

                        GC.Collect();
                    }

                    if (currentTime.Minute == 30 && currentTime.Second == 0)
                    {
                        var newWeather = Settings.Weathers[(new Random().Next(0, Settings.Weathers.Count))];

                        NAPI.Task.Run(() =>
                        {
                            Utils.SetWeather(newWeather);
                        });
                    }
                }
            });
        }
        #endregion

        [ServerEvent(Event.Update)]
        public void OnUpdate()
        {
            var currentTime = Utils.GetCurrentTime().AddHours(2);

            NAPI.World.SetTime(currentTime.Hour, currentTime.Minute, currentTime.Second);
        }

        private static List<string> VehicleDataLines = new List<string>();


/*        [RemoteEvent("vehicle_data_p")]
        private static void VehicleDataProcess(Player player, string model, string data)
        {
            VehicleDataLines.Add($"\"{model}\":{data},");
        }

        [RemoteEvent("vehicle_data_f")]
        private static async void VehicleDataFinish(Player player, int count)
        {
            while (VehicleDataLines.Count < count)
                await Task.Delay(25);

            VehicleDataLines[VehicleDataLines.Count - 1] = VehicleDataLines[VehicleDataLines.Count - 1].Substring(0, VehicleDataLines[VehicleDataLines.Count - 1].Length - 1);

            VehicleDataLines.Insert(0, "{");
            VehicleDataLines.Add("}");

            File.WriteAllLines("vehicleData.new.json", VehicleDataLines);
        }*/
    }
}
