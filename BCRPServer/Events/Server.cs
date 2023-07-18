using GTANetworkAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
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

        private static Timer _payDayTimer;


        [ServerEvent(Event.ResourceStart)]
        public static void OnResourceStart()
        {
            var currentDir = Directory.GetCurrentDirectory();

            Settings.SetProfile(Properties.SettingsProfile.GetDefault());

            Properties.SettingsProfile.SaveProfile(Settings.CurrentProfile, "brpSettings.json");

            Settings.CurrentProfile.GetClientsideData();

            // Settings Step

            CultureInfo.DefaultThreadCurrentCulture = Settings.CurrentProfile.General.CultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = Settings.CurrentProfile.General.CultureInfo;
            CultureInfo.CurrentCulture = Settings.CurrentProfile.General.CultureInfo;

            Properties.Language.Culture = Settings.CurrentProfile.General.CultureInfo;

            Events.Commands.Commands.LoadAll();
            Events.NPC.NPC.LoadAll();

            Sync.World.Initialize();

            var currentTime = Utils.GetCurrentTime();

            Utils.ConsoleOutput("~Red~###########################################################################################~/~");

            var appVersionInfo = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);

            Utils.ConsoleOutput($"~Red~{appVersionInfo.ProductName}~/~ | Developed by ~Red~{appVersionInfo.CompanyName}~/~ | Version: ~Green~{appVersionInfo.ProductVersion}~/~");

            Utils.ConsoleOutput();

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Copying .js files to client_packages...");

            var clientPackagesTarget = new DirectoryInfo(currentDir + Settings.ClientPackagesTargetPath);

            foreach (var script in (new DirectoryInfo(currentDir + Settings.ClientScriptsSourcePath + @"\Properties\JavaScript")).GetFiles("*.js"))
                File.Copy(script.FullName, clientPackagesTarget.FullName + "\\" + script.Name, true);

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Copying .cs files to client_packages/cs_packages...");

            var clientCSPackagesTarget = new DirectoryInfo(currentDir + Settings.ClientScriptsTargetPath);
            var clientCSPackagesSource = new DirectoryInfo(currentDir + Settings.ClientScriptsSourcePath);

            clientCSPackagesTarget.Delete(true);
            clientCSPackagesTarget.Create();

            foreach (var script in clientCSPackagesSource.GetFiles("*.cs"))
                File.Copy(script.FullName, clientCSPackagesTarget.FullName + "\\" + script.Name, true);

            foreach (var dir in clientCSPackagesSource.GetDirectories().Where(x => x.Name != "bin" && x.Name != "obj" && x.Name != "Properties"))
            {
                var newDir = new DirectoryInfo(clientCSPackagesTarget.FullName + "\\" + dir.Name);

                newDir.Create();

                Utils.CloneDirectory(dir, newDir);
            }

            var clientLangResManager = new ResourceManager("BCRPClient.Properties.Language", System.Reflection.Assembly.LoadFrom(currentDir + Settings.ResourcesPath + @"\BCRPClient.dll"));

            var langStrings = new Dictionary<string, string>();

            foreach (DictionaryEntry x in clientLangResManager.GetResourceSet(Properties.Language.Culture ?? CultureInfo.CurrentCulture, true, true))
                langStrings.Add((string)x.Key, (string)x.Value);

            Utils.FillFileToReplaceRegion(currentDir + Settings.ClientScriptsTargetPath + @"\Language\Strings.cs", "TEXTS_TO_REPLACE", langStrings.Select(x => $"{{ \"{x.Key}\", \"{x.Value}\" }},").ToList());

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

            Web.SocketIO.Service.Start(Settings.CurrentProfile.Web.SocketIOHost, Settings.CurrentProfile.Web.SocketIOUser, Settings.CurrentProfile.Web.SocketIOPassword);

            // Local Data Load Step

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

            // DB Load Step

            int loadedItemsAmount = 0;

            MySQL.LoadAll(out loadedItemsAmount);

            Game.Estates.House.LoadAll();
            Game.Estates.Apartments.LoadAll();
            Game.Estates.Garage.LoadAll();

            Utils.ConsoleOutput("~Red~[BRPMode]~/~ Clearing unused items & Getting free items UID's");
            Utils.ConsoleOutput($" | ~Red~Free UID's: [{Game.Items.Item.UidHandler.FreeUidsCount}]~/~", false);

            Utils.ConsoleOutput($"~Red~[BRPMode]~/~ Loaded ~Red~{loadedItemsAmount} items");

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

            Utils.ConsoleOutput("~Red~###########################################################################################~/~");
            Utils.ConsoleOutput();

            foreach (var x in Game.Jobs.Job.AllJobs.Values)
            {
                x.PostInitialize();
            }

            Game.Fractions.Fraction.PostInitializeAll();

            Game.Businesses.Business.ReplaceClientsideLines();

            Game.Autoschool.InitializeAll();

            Game.Misc.FishBuyer.InitializeAll();
            Game.Misc.VehicleDestruction.InitializeAll();
            Game.Misc.EstateAgency.InitializeAll();
            Game.Misc.MarketStall.Initialize();

            Game.Misc.Elevator.InitializeAll();

            Sync.DoorSystem.InitializeAll();

            Game.Casino.Casino.InitializeAll();

            Additional.ConsoleCommands.Activate();

            Sync.Weather.StartRealWeatherSync(new string[] { "LA", "Sacramento", "NY", "Dublin" , "Moscow", "Kaliningrad", "Omsk" }, true, 0, -1);

            _payDayTimer = new Timer((obj) =>
            {
                var curTime = Utils.GetCurrentTime();

                if (curTime.Minute != 0 || curTime.Second != 0)
                    return;

                var giveBankSavings = curTime.Hour == 0;

                NAPI.Task.Run(() =>
                {
                    DoPayDay(true);

                    if (giveBankSavings)
                    {
                        GiveBankSavings();
                    }
                }, 0);
            }, null, 1_000, 1_000);

            MySQL.StartService();
        }

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

                if (isAuto && pData.LastData.SessionTime < Settings.CurrentProfile.Game.PayDayMinimalSessionTimeToReceive.TotalSeconds)
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

                        fractionSalary = fData.Ranks[pData.Info.FractionRank].Salary;

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

        public static void GiveBankSavings()
        {
            foreach (var x in PlayerData.PlayerInfo.All)
            {
                var bankAccount = x.Value.BankAccount;

                if (bankAccount == null)
                    continue;

                if (bankAccount.MinSavingsBalance <= 0)
                {
                    continue;
                }

                bankAccount.GiveSavingsBenefit();
            }
        }

        [ServerEvent(Event.Update)]
        public static void OnUpdate()
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

        [RemoteProc("debug_gethouseinfo")]
        private static object DebugGetHouseInfo(Player player, uint houseId)
        {
            var h = Game.Estates.House.Get(houseId);

            if (h == null)
                return null;

            return $"{h.PositionParams.RotationZ}_{h.GarageOutside?.RotationZ ?? 0f}";
        }
    }
}
