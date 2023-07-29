using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using BlaineRP.Server.Game.Phone;
using BlaineRP.Server.Game.Quests;
using BlaineRP.Server.Sync;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Jobs
{
    public partial class Trucker : Job, IVehicleRelated
    {
        public const int MinimalRewardX = 1500;
        public const int MaximalRewardX = 2000;

        public const int MinimalRewardA = 1000;
        public const int MaximalRewardA = 1500;

        public static List<Trucker> AllTruckerJobs { get; set; }

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

        public Dictionary<uint, OrderInfo> ActiveOrders { get; private set; } = new Dictionary<uint, OrderInfo>();

        public uint AddCustomOrder(Game.Businesses.Business business)
        {
            var orderInfo = new OrderInfo(business);

            var id = GetNextOrderId();

            ActiveOrders.Add(id, orderInfo);

            orderInfo.Reward = (uint)SRandom.NextInt32(MinimalRewardX, MaximalRewardX);

            orderInfo.MPIdx = GetFarthestMaterialsPositionIdx(business.PositionInfo);

            TriggerEventToWorkers("Job::TR::OC", $"{id}_{business.ID}_{orderInfo.MPIdx}_{orderInfo.Reward}");

            return id;
        }

        public void AddDefaultOrder(Game.Businesses.Business business)
        {
            var orderInfo = new OrderInfo(business);

            var id = GetNextOrderId();

            ActiveOrders.Add(id, orderInfo);

            orderInfo.Reward = (uint)SRandom.NextInt32(MinimalRewardA, MaximalRewardA);

            orderInfo.MPIdx = GetFarthestMaterialsPositionIdx(business.PositionInfo);

            TriggerEventToWorkers("Job::TR::OC", $"{id}_{business.ID}_{orderInfo.MPIdx}_{orderInfo.Reward}");
        }

        public void RemoveOrder(uint id, OrderInfo oInfo)
        {
            if (ActiveOrders.Remove(id))
            {
                FreeOrderId(id);

                if (oInfo.CurrentWorker == null)
                    TriggerEventToWorkers("Job::TR::OC", id);
            }
        }

        public bool TryAddRandomDefaultOrder()
        {
            var businesses = Game.Businesses.Business.All.Values.Where(x => x.PositionInfo != null && x.ClosestTruckerJob == this).ToList();

            if (businesses.Count == 0)
                return false;

            var business = businesses[SRandom.NextInt32(0, businesses.Count)];

            AddDefaultOrder(business);

            return true;
        }

        public void SetOrderAsTaken(uint orderId, OrderInfo oInfo, PlayerData pData)
        {
            oInfo.CurrentWorker = pData.Info;

            TriggerEventToWorkers("Job::TR::OC", orderId);

            if (oInfo.IsCustom && oInfo.TargetBusiness.Owner is PlayerInfo pInfo)
            {
                var sms = new SMS((uint)DefaultNumbers.Delivery, pInfo, string.Format(SMS.GetDefaultSmsMessage(SMS.PredefinedTypes.DeliveryBusinessOrderTaken), orderId));

                pData.Info.AddSms(sms, true);
            }
        }

        public void SetOrderAsNotTaken(uint orderId, OrderInfo oInfo)
        {
            oInfo.CurrentWorker = null;

            TriggerEventToWorkers("Job::TR::OC", $"{orderId}_{oInfo.TargetBusiness.ID}_{oInfo.MPIdx}_{oInfo.Reward}");

            if (oInfo.IsCustom && oInfo.TargetBusiness.Owner is PlayerInfo pInfo)
            {
                var sms = new SMS((uint)DefaultNumbers.Delivery, pInfo, string.Format(SMS.GetDefaultSmsMessage(SMS.PredefinedTypes.DeliveryBusinessOrderDelay), orderId));

                pInfo.AddSms(sms, true);
            }
        }

        public List<Vector3> MaterialsPositions { get; set; }

        public List<VehicleInfo> Vehicles { get; set; } = new List<VehicleInfo>();

        public uint VehicleRentPrice { get; set; }

        public override string ClientData => $"{Id}, {Position.ToCSharpStr()}, new System.Collections.Generic.List<RAGE.Vector3>(){{{string.Join(',', MaterialsPositions.Select(x => x.ToCSharpStr()))}}}";

        public Trucker(Vector4 Position) : base(JobType.Trucker, Position)
        {

        }

        public int GetFarthestMaterialsPositionIdx(Vector3 pos)
        {
            var maxDistance = pos.DistanceTo(MaterialsPositions[0]);

            var maxIdx = 0;

            for (int i = 1; i < MaterialsPositions.Count; i++)
            {
                var dist = pos.DistanceTo(MaterialsPositions[i]);

                if (dist > maxDistance)
                {
                    maxDistance = dist;

                    maxIdx = i;
                }
            }

            return maxIdx;
        }

        public override void SetPlayerJob(PlayerData pData, params object[] args)
        {
            base.SetPlayerJob(pData);

            var jobVehicleData = (VehicleData)args[0];

            pData.Player.TriggerEvent("Player::SCJ", Id, jobVehicleData.Vehicle.Id, ActiveOrders.Where(x => x.Value.CurrentWorker == null).Select(x => $"{x.Key}_{x.Value.TargetBusiness.ID}_{x.Value.MPIdx}_{x.Value.Reward}").ToList());

            Quest.StartQuest(pData, QuestType.JTR1, 0, 0);
        }

        public override void SetPlayerNoJob(PlayerInfo pInfo)
        {
            base.SetPlayerNoJob(pInfo);

            pInfo.Quests.GetValueOrDefault(QuestType.JTR1)?.Cancel(pInfo);

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

        public override void OnWorkerExit(PlayerData pData)
        {

        }
    }
}
