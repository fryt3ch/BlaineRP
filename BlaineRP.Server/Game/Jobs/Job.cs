using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Jobs
{
    public abstract partial class Job
    {
        public static Dictionary<int, Job> AllJobs { get; private set; } = new Dictionary<int, Job>();

        public static Job Get(int jobId) => jobId == 0 ? null : AllJobs.GetValueOrDefault(jobId);

        private static void Add(Job job) => AllJobs.Add(job.Id, job);

        public JobType Type { get; set; }

        public int Id { get; set; }

        public int SubId => AllJobs.Values.Where(x => x.Type == Type).ToList().IndexOf(this);

        public Vector4 Position { get; set; }

        public List<PlayerData> Workers => PlayerData.All.Values.Where(x => x.Player.GetData<int>("CJob") == Id).ToList();

        public abstract string ClientData { get; }

        public Job(JobType Type, Vector4 Position)
        {
            this.Type = Type;

            this.Position = Position;

            this.Id = AllJobs.Count + 1;

            Add(this);
        }

        public virtual void SetPlayerNoJob(PlayerInfo pInfo)
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

        public static void TriggerEventToWorkersByJobType(JobType type, string eventName, params object[] args)
        {
            var workers = GetWorkersByJobType(type);

            if (workers.Count == 0)
                return;

            NAPI.ClientEvent.TriggerClientEventToPlayers(workers.Select(x => x.Player).ToArray(), eventName, args);
        }

        public static List<PlayerData> GetWorkersByJobType(JobType type) => PlayerData.All.Values.Where(x => x.CurrentJob?.Type == type).ToList();

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
}
