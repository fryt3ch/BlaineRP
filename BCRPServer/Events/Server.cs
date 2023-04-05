using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BCRPServer.Events
{
    public class Server : Script
    {
        public static bool IsRestarting = false;

        /// <summary>Кол-во средств, которое получит безработный игрок</summary>
        private const uint JoblessBenefits = 500;

        public static bool PayDayEstateTaxIsEnabled { get; set; } = true;

        public static byte PayDayX { get; set; } = 1;

        #region On Start
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            CultureInfo.DefaultThreadCurrentCulture = Settings.CultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = Settings.CultureInfo;
            CultureInfo.CurrentCulture = Settings.CultureInfo;

            Events.Commands.Commands.LoadAll();
            Events.NPC.NPC.LoadAll();

            Sync.World.Initialize();

            var currentTime = Utils.GetCurrentTime();

            Utils.ConsoleOutput("~Red~###########################################################################################~/~");

            Utils.ConsoleOutput($"~Red~Blaine RolePlay~/~ server mode | Developed by ~Red~frytech~/~ | Version: ~Green~{Settings.VERSION}~/~");

            Utils.ConsoleOutput();

            try
            {
                Utils.ConsoleOutput("~Red~[BRPMode]~/~ Copying .cs files to client_resources...");

                var ClientCSPackagesTarget = new DirectoryInfo(Settings.DIR_CLIENT_PACKAGES_CS_PATH);
                var ClientCSPackagesSource = new DirectoryInfo(Settings.DIR_CLIENT_SOURCES_PATH);

                ClientCSPackagesTarget.Delete(true);
                ClientCSPackagesTarget.Create();

                foreach (var script in ClientCSPackagesSource.GetFiles("*.cs"))
                    File.Copy(script.FullName, ClientCSPackagesTarget.FullName + "\\" + script.Name, true);

                foreach (var dir in ClientCSPackagesSource.GetDirectories().Where(x => x.Name != "bin" && x.Name != "obj"))
                {
                    var newDir = new DirectoryInfo(ClientCSPackagesTarget.FullName + "\\" + dir.Name);

                    newDir.Create();

                    Utils.CloneDirectory(dir, newDir);
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
            Game.Businesses.Business.LoadPrices();

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Loading all items [DATA]");
            Utils.ConsoleOutput($" | ~Red~[{Game.Items.Stuff.LoadAll()}]~/~", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Loading all furniture [DATA]");
            Utils.ConsoleOutput($" | ~Red~[{Game.Estates.Furniture.ItemData.LoadAll()}]~/~", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Loading all vehicles [DATA]");
            Utils.ConsoleOutput($" | ~Red~[{Game.Data.Vehicles.LoadAll()}]~/~", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Loading all businesses [DATA]");
            Utils.ConsoleOutput($" | ~Red~[{Game.Businesses.Business.LoadAll()}]~/~", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Loading all jobs [DATA]");
            Utils.ConsoleOutput($" | ~Red~[{Game.Jobs.Job.InitializeAll()}]~/~", false);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Loading all fractions [DATA]");
            Utils.ConsoleOutput($" | ~Red~[{Game.Fractions.Fraction.InitializeAll()}]~/~", false);

            Game.Estates.HouseBase.Style.LoadAll();
            Game.Estates.Garage.Style.LoadAll();

            Game.Bank.LoadAll();

            Sync.Quest.InitializeAll();
            #endregion

            #region Database Data Load Section
            MySQL.UpdateServerData();

            MySQL.LoadAll();
            MySQL.UpdateFreeUIDs();

            Game.Estates.House.LoadAll();
            Game.Estates.Apartments.LoadAll();
            Game.Estates.Garage.LoadAll();

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Clearing unused items & Getting free items UID's");
            Utils.ConsoleOutput($" | ~Red~Free UID's: [{Game.Items.Item.FreeIDs.Count}]~/~", false);

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{Game.Items.Item.All.Count} items");

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{Game.Items.Container.All.Count} containers");

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{PlayerData.PlayerInfo.All.Count} players");

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{VehicleData.VehicleInfo.All.Count} vehicles");

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{Game.Businesses.Business.All.Count} businesses");

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{PlayerData.PlayerInfo.All.Values.Select(x => x.Gifts.Count).Sum()} gifts");

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{Game.Estates.House.All.Count} houses");

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{Game.Estates.Apartments.All.Count} apartments");

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{Game.Estates.Garage.All.Count} garages");

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{Game.Estates.Furniture.All.Count} furniture");

            GC.Collect();
            #endregion

            Utils.ConsoleOutput("~Red~###########################################################################################~/~");
            Utils.ConsoleOutput();

            foreach (var x in Game.Jobs.Job.AllJobs.Values)
            {
                x.PostInitialize();
            }

            Game.Fractions.Fraction.PostInitializeAll();

            Game.Businesses.Business.ReplaceClientsideLines();

            Game.Autoschool.InitializeAll();

            /*            var truck = VehicleData.NewTemp(Game.Data.Vehicles.GetData("bison"), Utils.Colour.FromRageColour(Utils.RedColor), Utils.Colour.FromRageColour(Utils.RedColor), new Vector3(-740.3475f, 5813.844f, 18f), 255f, Utils.Dimensions.Main);

                        var boat = VehicleData.NewTemp(Game.Data.Vehicles.GetData("dinghy"), Utils.Colour.FromRageColour(Utils.RedColor), Utils.Colour.FromRageColour(Utils.RedColor), Utils.DefaultSpawnPosition, 0f, Utils.Dimensions.Main);

                        boat.AttachBoatToTrailer();*/

            Additional.ConsoleCommands.Activate();

            Utils.SetWeather(Utils.WeatherTypes.CLEAR);

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);

                    var currentTime = Utils.GetCurrentTime();

                    if (currentTime.Second == 0)
                    {
                        if (currentTime.Minute == 0)
                        {
                            NAPI.Task.Run(() =>
                            {
                                DoPayDay(true);
                            });

                            GC.Collect();
                        }
                        else if (currentTime.Minute == 30)
                        {
                            var newWeather = Settings.Weathers[(new Random().Next(0, Settings.Weathers.Count))];

                            NAPI.Task.Run(() =>
                            {
                                Utils.SetWeather(newWeather);
                            });
                        }
                    }
                }
            });
        }
        #endregion

        public static void DoPayDay(bool isAuto)
        {
            if (PayDayEstateTaxIsEnabled)
            {
                foreach (var x in Game.Estates.House.All.Values)
                {
                    if (x.Owner == null)
                        continue;

                    ulong newBalance;

                    if (x.TryRemoveMoneyBalance((uint)x.Tax, out newBalance, false, null))
                    {
                        x.SetBalance(newBalance, "PAYDAY");
                    }
                    else
                    {
                        x.ChangeOwner(null);
                    }
                }

                foreach (var x in Game.Estates.Apartments.All.Values)
                {
                    if (x.Owner == null)
                        continue;

                    ulong newBalance;

                    if (x.TryRemoveMoneyBalance((uint)x.Tax, out newBalance, false, null))
                    {
                        x.SetBalance(newBalance, "PAYDAY");
                    }
                    else
                    {
                        x.ChangeOwner(null);
                    }
                }

                foreach (var x in Game.Estates.Garage.All.Values)
                {
                    if (x.Owner == null)
                        continue;

                    ulong newBalance;

                    if (x.TryRemoveMoneyBalance((uint)x.Tax, out newBalance, false, null))
                    {
                        x.SetBalance(newBalance, "PAYDAY");
                    }
                    else
                    {
                        x.ChangeOwner(null);
                    }
                }

                foreach (var x in Game.Businesses.Business.All.Values)
                {
                    if (x.Owner == null)
                        continue;

                    ulong newBalance;

                    if (x.TryRemoveMoneyBank(x.Rent, out newBalance, false, null))
                    {
                        x.SetBank(newBalance);
                    }
                    else
                    {
                        x.SellToGov(true, true);
                    }
                }
            }

            foreach (var pData in PlayerData.All.Values)
            {
                var player = pData.Player;

                if (isAuto && pData.LastData.SessionTime < Settings.MIN_SESSION_TIME_FOR_PAYDAY)
                {
                    player.TriggerEvent("opday", pData.LastData.SessionTime);

                    continue;
                }

                pData.LastData.SessionTime = 0;

                if (pData.BankAccount == null)
                {
                    player.TriggerEvent("opday");

                    continue;
                }
                else
                {
                    uint joblessBenefit = JoblessBenefits, fractionSalary = 0, organisationSalary = 0;

                    if (pData.Fraction != Game.Fractions.Types.None)
                    {
                        var fData = Game.Fractions.Fraction.Get(pData.Fraction);

                        fractionSalary = fData.Salary[pData.Info.FractionRank];

                        joblessBenefit = 0;
                    }

                    ulong totalSalary = (joblessBenefit + fractionSalary + organisationSalary) * PayDayX;

                    ulong newBalance;

                    if (pData.BankAccount.TryAddMoneyDebit(totalSalary, out newBalance, true, null))
                        pData.BankAccount.SetDebitBalance(newBalance, "PAYDAY");

                    player.TriggerEvent("opday", joblessBenefit, fractionSalary, organisationSalary);

                    continue;
                }
            }
        }

        [ServerEvent(Event.Update)]
        public void OnUpdate()
        {
            var currentTime = Utils.GetCurrentTime();

            Sync.World.SetSharedData("cst", currentTime.GetUnixTimestampMil());
        }

        public static async Task OnServerShutdown()
        {
            IsRestarting = true;

            foreach (var player in NAPI.Pools.GetAllPlayers())
            {
                Utils.Kick(player, "Сервер был отключён!");
            }

            foreach (var vehicle in VehicleData.All.Values)
                vehicle?.Delete(false);

            foreach (var x in Game.Businesses.Business.All.Values)
                MySQL.BusinessUpdateOnRestart(x);

            foreach (var x in Game.Estates.House.All.Values)
                MySQL.HouseUpdateOnRestart(x);

            foreach (var x in Game.Estates.Apartments.All.Values)
                MySQL.HouseUpdateOnRestart(x);

            foreach (var x in Game.Estates.Garage.All.Values)
                MySQL.GarageUpdateOnRestart(x);

            await Task.Delay(Settings.SERVER_STOP_DELAY);

            await MySQL.Wait();

            MySQL.DoAllQueries();
        }

        /*        private static List<string> VehicleDataLines = new List<string>();

                [RemoteEvent("vehicle_data_p")]
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

        [RemoteEvent("debug_save")]
        private static void DebugSaveText(Player player, string str)
        {
            File.AppendAllText(@"debug-save.txt", str + Environment.NewLine);
        }
    }
}
