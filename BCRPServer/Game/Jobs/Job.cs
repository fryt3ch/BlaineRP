using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Jobs
{
    public enum Types
    {
        /// <summary>Работа дальнобойщика</summary>
        Trucker = 0,
        /// <summary>Работа таксиста</summary>
        Cabbie,
        /// <summary>Работа водителя автобуса</summary>
        BusDriver,
        /// <summary>Работа инкассатора</summary>
        Collector,

        Farmer,
    }

    public abstract class Job
    {
        public static Dictionary<int, Job> AllJobs { get; private set; } = new Dictionary<int, Job>();

        public static Job Get(int jobId) => jobId == 0 ? null : AllJobs.GetValueOrDefault(jobId);

        private static void Add(Job job) => AllJobs.Add(job.Id, job);

        public Types Type { get; set; }

        public int Id { get; set; }

        public int SubId => AllJobs.Values.Where(x => x.Type == Type).ToList().IndexOf(this);

        public Utils.Vector4 Position { get; set; }

        public List<PlayerData> Workers => PlayerData.All.Values.Where(x => x.Player.GetData<int>("CJob") == Id).ToList();

        public abstract string ClientData { get; }

        public Job(Types Type, Utils.Vector4 Position)
        {
            this.Type = Type;

            this.Position = Position;

            this.Id = AllJobs.Count + 1;

            Add(this);
        }

        public static int InitializeAll()
        {
            new Trucker(new Utils.Vector4(50.92614f, 6337.754f, 31.38093f, 21.58484f))
            {
                VehicleRentPrice = 1_500,

                MaterialsPositions = new List<Vector3>()
                {
                    new Vector3(142.8727f, 6364.274f, 30.5f),
                }
            };

            new Cabbie(new Utils.Vector4(-271.4561f, 6074.177f, 31.68299f, 183.1857f))
            {
                VehicleRentPrice = 750,
            };

            new Cabbie(new Utils.Vector4(895.5431f, -178.9546f, 74.70026f, 254.4035f))
            {
                VehicleRentPrice = 750,
            };

            new BusDriver(new Utils.Vector4(-744.4896f, 5546.173f, 33.60594f, 121.2667f))
            {
                VehicleRentPrice = 1_000,

                Routes = new List<BusDriver.RouteData>()
                {
                    new BusDriver.RouteData(2_500, new List<Vector3>()
                    {
                        new Vector3(-782.8536f, 5500.472f, 34.38135f),
                        new Vector3(-945.3662f, 5418.015f, 38.22468f),
                        new Vector3(-773.1383f, 5482.208f, 34.35715f),
                        new Vector3(-785.9555f, 5536.9139f, 33.65596f),
                    }),
                }
            };

            new Collector(new Utils.Vector4(-90.54147f, 6471.977f, 31.29943f, 0f), 0)
            {
                VehicleRentPrice = 2_000,
            };

            new Collector(new Utils.Vector4(132.968f, -1056.943f, 29.19235f, 0f), 7)
            {
                VehicleRentPrice = 2_000,
            };

            new Farmer(Game.Businesses.Business.Get(38) as Game.Businesses.Farm)
            {
                VehicleRentPrice = 500,
            };

            var lines = new List<string>();

            foreach (var x in AllJobs.Values)
            {
                x.Initialize();

                lines.Add($"new Jobs.{x.GetType().Name}({x.ClientData});");
            }

            Trucker.AllTruckerJobs = AllJobs.Values.Select(x => x as Trucker).Where(x => x != null).ToList();
            Collector.AllCollectorJobs = AllJobs.Values.Select(x => x as Collector).Where(x => x != null).ToList();

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() + Settings.ClientScriptsTargetLocationsLoaderPath, "JOBS_TO_REPLACE", lines);

            return AllJobs.Count;
        }

        public virtual void SetPlayerNoJob(PlayerData.PlayerInfo pInfo)
        {
            if (pInfo.PlayerData != null)
            {
                pInfo.PlayerData.CurrentJob = null;

                pInfo.PlayerData.Player.TriggerEvent("Player::SCJ");
            }
        }

        public void TriggerEventToWorkers(string eventName, params object[] args)
        {
            var workers = Workers;

            if (workers.Count == 0)
                return;

            NAPI.ClientEvent.TriggerClientEventToPlayers(workers.Select(x => x.Player).ToArray(), eventName, args);
        }

        public static void TriggerEventToWorkersByJobType(Types type, string eventName, params object[] args)
        {
            var workers = GetWorkersByJobType(type);

            if (workers.Count == 0)
                return;

            NAPI.ClientEvent.TriggerClientEventToPlayers(workers.Select(x => x.Player).ToArray(), eventName, args);
        }

        public static List<PlayerData> GetWorkersByJobType(Types type) => PlayerData.All.Values.Where(x => x.CurrentJob?.Type == type).ToList();

        public abstract void Initialize();

        public abstract void PostInitialize();

        public virtual void SetPlayerJob(PlayerData pData, params object[] args)
        {
            pData.CurrentJob = this;
        }

        public abstract bool CanPlayerDoThisJob(PlayerData pData);

        public abstract void OnWorkerExit(PlayerData pData);

        public static uint GetPlayerTotalCashSalary(PlayerData pData) => pData.Player.GetData<uint>("JCMC::TS");

        public static void SetPlayerTotalCashSalary(PlayerData pData, uint value, bool notify)
        {
            if (notify)
            {
                pData.Player.TriggerEvent("Job::TSC", value, GetPlayerTotalCashSalary(pData));
            }

            pData.Player.SetData("JCMC::TS", value);
        }

        public static void ResetPlayerTotalCashSalary(PlayerData pData) => pData.Player.ResetData("JCMC::TS");
    }

    public interface IVehicles
    {
        public List<VehicleData.VehicleInfo> Vehicles { get; set; }

        public uint VehicleRentPrice { get; set; }

        public void OnVehicleRespawned(VehicleData.VehicleInfo vInfo, PlayerData.PlayerInfo pInfo);
    }
}
