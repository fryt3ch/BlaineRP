using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.EntitiesData.Vehicles;
using BlaineRP.Server.Game.Quests;
using BlaineRP.Server.Sync;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Jobs
{
    public partial class Collector : Job, IVehicleRelated
    {
        public const int MinimalReward = 100;
        public const int MaximalReward = 125;

        public static List<Collector> AllCollectorJobs { get; set; }

        private Queue<uint> FreeOrderIds = new Queue<uint>();

        private uint LastMaxOrderId { get; set; }

        private uint GetNextOrderId()
        {
            uint result;

            if (FreeOrderIds.TryDequeue(out result))
                return result;

            return LastMaxOrderId += 1;
        }

        private void FreeOrderId(uint id)
        {
            if (LastMaxOrderId < id)
                LastMaxOrderId = id;

            FreeOrderIds.Enqueue(id);
        }

        public void AddDefaultOrder(Game.Businesses.Business business)
        {
            var orderInfo = new OrderInfo(business);

            var id = GetNextOrderId();

            ActiveOrders.Add(id, orderInfo);

            orderInfo.Reward = (uint)Math.Floor((business.PositionInfo.DistanceTo(Position.Position) / 1000f) * SRandom.NextInt32(MinimalReward, MaximalReward));

            TriggerEventToWorkers("Job::CL::OC", $"{id}_{business.ID}_{orderInfo.Reward}");
        }

        public void RemoveOrder(uint id, OrderInfo oInfo)
        {
            if (ActiveOrders.Remove(id))
            {
                FreeOrderId(id);

                if (oInfo.CurrentWorker == null)
                    TriggerEventToWorkers("Job::CL::OC", id);
            }
        }

        public bool TryAddRandomDefaultOrder()
        {
            var businesses = Game.Businesses.Business.All.Values.Where(x => x.PositionInfo != null && x.ClosestCollectorJob == this).ToList();

            if (businesses.Count == 0)
                return false;

            var business = businesses[SRandom.NextInt32(0, businesses.Count)];

            AddDefaultOrder(business);

            return true;
        }

        public void SetOrderAsTaken(uint orderId, OrderInfo oInfo, PlayerData pData)
        {
            oInfo.CurrentWorker = pData.Info;

            TriggerEventToWorkers("Job::CL::OC", orderId);
        }

        public void SetOrderAsNotTaken(uint orderId, OrderInfo oInfo)
        {
            oInfo.CurrentWorker = null;

            TriggerEventToWorkers("Job::CL::OC", $"{orderId}_{oInfo.TargetBusiness.ID}_{oInfo.Reward}");
        }

        public Dictionary<uint, OrderInfo> ActiveOrders { get; private set; } = new Dictionary<uint, OrderInfo>();

        public override string ClientData => $"{Id}, {Position.ToCSharpStr()}, {BankId}";

        public List<VehicleInfo> Vehicles { get; set; } = new List<VehicleInfo>();

        public uint VehicleRentPrice { get; set; }

        public int BankId { get; set; }

        public override void SetPlayerJob(PlayerData pData, params object[] args)
        {
            base.SetPlayerJob(pData);

            var jobVehicleData = (VehicleData)args[0];

            pData.Player.TriggerEvent("Player::SCJ", Id, jobVehicleData.Vehicle.Id, ActiveOrders.Where(x => x.Value.CurrentWorker == null).Select(x => $"{x.Key}_{x.Value.TargetBusiness.ID}_{x.Value.Reward}").ToList());

            Quest.StartQuest(pData, QuestType.JCL1, 0, 0);
        }

        public override void SetPlayerNoJob(PlayerInfo pInfo)
        {
            base.SetPlayerNoJob(pInfo);

            pInfo.Quests.GetValueOrDefault(QuestType.JCL1)?.Cancel(pInfo);

            var orderPair = ActiveOrders.Where(x => x.Value.CurrentWorker == pInfo).FirstOrDefault();

            if (orderPair.Value != null)
            {
                SetOrderAsNotTaken(orderPair.Key, orderPair.Value);
            }

            Vehicles.Where(x => x.OwnerID == pInfo.CID).FirstOrDefault()?.VehicleData?.Delete(false);
        }

        public override bool CanPlayerDoThisJob(PlayerData pData)
        {
            if (!pData.HasLicense(LicenseType.C, true))
                return false;

            return true;
        }

        public override void OnWorkerExit(PlayerData pData)
        {

        }

        public override void PostInitialize()
        {
            int counter = 3;

            while (counter > 0 && TryAddRandomDefaultOrder())
            {
                counter--;
            }
        }

        public void OnVehicleRespawned(VehicleInfo vInfo, PlayerInfo pInfo)
        {
            if (pInfo != null)
            {
                SetPlayerNoJob(pInfo);
            }
        }

        public Collector(Vector4 Position, int BankId) : base(JobType.Collector, Position)
        {
            this.BankId = BankId;
        }
    }
}
