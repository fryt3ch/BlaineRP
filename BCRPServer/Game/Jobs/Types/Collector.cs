using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Jobs
{
    public class Collector : Job, IVehicles
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

        public class OrderInfo
        {
            public PlayerData.PlayerInfo CurrentWorker { get; set; }

            public uint Reward { get; set; }

            public Game.Businesses.Business TargetBusiness { get; set; }

            public OrderInfo(Game.Businesses.Business TargetBusiness)
            {
                this.TargetBusiness = TargetBusiness;
            }
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

        public List<VehicleData.VehicleInfo> Vehicles { get; set; } = new List<VehicleData.VehicleInfo>();

        public uint VehicleRentPrice { get; set; }

        public int BankId { get; set; }

        public override void SetPlayerJob(PlayerData pData, params object[] args)
        {
            base.SetPlayerJob(pData);

            var jobVehicleData = (VehicleData)args[0];

            pData.Player.TriggerEvent("Player::SCJ", Id, jobVehicleData.Vehicle.Id, ActiveOrders.Where(x => x.Value.CurrentWorker == null).Select(x => $"{x.Key}_{x.Value.TargetBusiness.ID}_{x.Value.Reward}").ToList());

            Sync.Quest.StartQuest(pData, Sync.Quest.QuestData.Types.JCL1, 0, 0);
        }

        public override void SetPlayerNoJob(PlayerData.PlayerInfo pInfo)
        {
            base.SetPlayerNoJob(pInfo);

            pInfo.Quests.GetValueOrDefault(Sync.Quest.QuestData.Types.JCL1)?.Cancel(pInfo);

            var orderPair = ActiveOrders.Where(x => x.Value.CurrentWorker == pInfo).FirstOrDefault();

            if (orderPair.Value != null)
            {
                SetOrderAsNotTaken(orderPair.Key, orderPair.Value);
            }

            Vehicles.Where(x => x.OwnerID == pInfo.CID).FirstOrDefault()?.VehicleData?.Delete(false);
        }

        public override bool CanPlayerDoThisJob(PlayerData pData)
        {
            if (!pData.HasLicense(PlayerData.LicenseTypes.C, true))
                return false;

            return true;
        }

        public override void Initialize()
        {
            var numberplateText = $"BANK{BankId}";

            var subId = SubId;

            var vType = Game.Data.Vehicles.GetData("stockade");

            if (subId == 0)
            {
                var colour1 = new Utils.Colour(255, 255, 255, 255);
                var colour2 = new Utils.Colour(255, 0, 0, 255);

                Vehicles.Add(VehicleData.NewJob(Id, numberplateText, vType, colour1, colour2, new Utils.Vector4(-125.2996f, 6476.03f, 31.06822f, 134.7541f), Utils.Dimensions.Main));
                Vehicles.Add(VehicleData.NewJob(Id, numberplateText, vType, colour1, colour2, new Utils.Vector4(-128.2522f, 6479.262f, 31.07171f, 134.5773f), Utils.Dimensions.Main));
            }
            else if (subId == 1)
            {
                var colour1 = new Utils.Colour(255, 255, 255, 255);
                var colour2 = new Utils.Colour(158, 186, 91, 255);

                Vehicles.Add(VehicleData.NewJob(Id, numberplateText, vType, colour1, colour2, new Utils.Vector4(162.2404f, -1081.599f, 28.79756f, 1.330183f), Utils.Dimensions.Main));
                Vehicles.Add(VehicleData.NewJob(Id, numberplateText, vType, colour1, colour2, new Utils.Vector4(158.5186f, -1081.227f, 28.79802f, 1.025265f), Utils.Dimensions.Main));
                Vehicles.Add(VehicleData.NewJob(Id, numberplateText, vType, colour1, colour2, new Utils.Vector4(154.839f, -1081.596f, 28.79737f, 1.333282f), Utils.Dimensions.Main));
                Vehicles.Add(VehicleData.NewJob(Id, numberplateText, vType, colour1, colour2, new Utils.Vector4(150.9679f, -1081.523f, 28.79806f, 1f), Utils.Dimensions.Main));
                Vehicles.Add(VehicleData.NewJob(Id, numberplateText, vType, colour1, colour2, new Utils.Vector4(147.2328f, -1081.582f, 28.79786f, 1f), Utils.Dimensions.Main));
                Vehicles.Add(VehicleData.NewJob(Id, numberplateText, vType, colour1, colour2, new Utils.Vector4(143.5911f, -1081.642f, 28.79869f, 1f), Utils.Dimensions.Main));
                Vehicles.Add(VehicleData.NewJob(Id, numberplateText, vType, colour1, colour2, new Utils.Vector4(139.8136f, -1081.303f, 28.7997f, 1f), Utils.Dimensions.Main));
            }
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

        public void OnVehicleRespawned(VehicleData.VehicleInfo vInfo, PlayerData.PlayerInfo pInfo)
        {
            if (pInfo != null)
            {
                SetPlayerNoJob(pInfo);
            }
        }

        public Collector(Utils.Vector4 Position, int BankId) : base(Types.Collector, Position)
        {
            this.BankId = BankId;
        }
    }
}
