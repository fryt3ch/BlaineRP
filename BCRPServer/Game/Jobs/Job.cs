using GTANetworkAPI;
using GTANetworkMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Game.Jobs
{
    public abstract class Job
    {
        public static Dictionary<int, Job> AllJobs { get; private set; } = new Dictionary<int, Job>();

        public static Job Get(int jobId) => jobId == 0 ? null : AllJobs.GetValueOrDefault(jobId);

        private static void Add(Job job) => AllJobs.Add(job.Id, job);

        public enum Types
        {
            /// <summary>Работа дальнобойщика</summary>
            Trucker = 0,
            /// <summary>Работа таксиста</summary>
            Cabbie,
        }

        public Types Type { get; set; }

        public int Id { get; set; }

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

            var lines = new List<string>();

            foreach (var x in AllJobs.Values)
            {
                x.Initialize();

                lines.Add($"new {x.GetType().Name}({x.ClientData});");
            }

            Trucker.AllTruckerJobs = AllJobs.Values.Select(x => x as Trucker).Where(x => x != null).ToList();

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "JOBS_TO_REPLACE", lines);

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
    }

    public interface IVehicles
    {
        public List<VehicleData> Vehicles { get; set; }

        public string NumberplateText { get; set; }

        public uint VehicleRentPrice { get; set; }

        public void OnVehicleRespawned(VehicleData vData);
    }
}
