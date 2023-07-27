using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.EntitiesData.Vehicles;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Jobs
{
    public partial class Cabbie : Job, IVehicleRelated
    {
        public static Dictionary<uint, OrderInfo> ActiveOrders { get; private set; } = new Dictionary<uint, OrderInfo>();

        private static Queue<uint> FreeOrderIds = new Queue<uint>();

        private static uint LastMaxOrderId { get; set; }

        private static uint GetNextOrderId()
        {
            uint result;

            if (FreeOrderIds.TryDequeue(out result))
                return result;

            return LastMaxOrderId += 1;
        }

        private static void FreeOrderId(uint id)
        {
            if (LastMaxOrderId < id)
                LastMaxOrderId = id;

            FreeOrderIds.Enqueue(id);
        }

        public static void AddPlayerOrder(PlayerData pData)
        {
            var orderInfo = new OrderInfo(pData.Player, pData.Player.Position);

            var id = GetNextOrderId();

            ActiveOrders.Add(id, orderInfo);

            TriggerEventToWorkersByJobType(JobType.Cabbie, "Job::CAB::OC", $"{id}_{orderInfo.Position.X}_{orderInfo.Position.Y}_{orderInfo.Position.Z}");
        }

        public static void RemoveOrder(uint id, OrderInfo oInfo, bool success)
        {
            if (ActiveOrders.Remove(id))
            {
                if (oInfo.GPSTrackerTimer != null)
                {
                    oInfo.GPSTrackerTimer.Dispose();

                    oInfo.GPSTrackerTimer = null;
                }

                FreeOrderId(id);

                if (oInfo.CurrentWorker == null)
                {
                    TriggerEventToWorkersByJobType(JobType.Cabbie, "Job::CAB::OC", id);
                }
                else
                {
                    oInfo.CurrentWorker.PlayerData.Player.TriggerEvent("Job::CAB::OC", id, success);
                }

                if (oInfo.Entity is Player player)
                {
                    if (success)
                        player.TriggerEvent("Taxi::UO", true);
                    else
                        player.TriggerEvent("Taxi::UO");
                }
            }
        }

        public static void SetOrderAsTaken(uint orderId, OrderInfo oInfo, PlayerData pDataDriver)
        {
            oInfo.CurrentWorker = pDataDriver.Info;

            TriggerEventToWorkersByJobType(JobType.Cabbie, "Job::CAB::OC", orderId);

            if (oInfo.Entity is Player player)
            {
                player.TriggerEvent("Taxi::UO", pDataDriver.Player.Id, pDataDriver.Info.PhoneNumber);

                if (oInfo.GPSTrackerTimer != null)
                {
                    oInfo.GPSTrackerTimer.Dispose();
                }

                oInfo.GPSTrackerTimer = new Timer((obj) =>
                {
                    NAPI.Task.Run(() =>
                    {
                        if (!oInfo.Exists || oInfo.CurrentWorker == null)
                            return;

                        var pos = pDataDriver.Player.Position;

                        player.SendGPSTracker(0, pos.X, pos.Y, pDataDriver.Player);
                    });
                }, null, 5_000, 5_000);
            }
        }

        public static void SetOrderAsNotTaken(uint orderId, OrderInfo oInfo)
        {
            oInfo.CurrentWorker = null;

            if (oInfo.GPSTrackerTimer != null)
            {
                oInfo.GPSTrackerTimer.Dispose();

                oInfo.GPSTrackerTimer = null;
            }

            TriggerEventToWorkersByJobType(JobType.Cabbie, "Job::CAB::OC", $"{orderId}_{oInfo.Position.X}_{oInfo.Position.Y}_{oInfo.Position.Z}");

            if (oInfo.Entity is Player player)
            {
                player.TriggerEvent("Taxi::UO", false);
            }
        }

        public override string ClientData => $"{Id}, {Position.ToCSharpStr()}";

        public List<VehicleInfo> Vehicles { get; set; } = new List<VehicleInfo>();

        public uint VehicleRentPrice { get; set; }

        public Cabbie(Vector4 Position) : base(JobType.Cabbie, Position)
        {

        }

        public override void SetPlayerJob(PlayerData pData, params object[] args)
        {
            base.SetPlayerJob(pData);

            foreach (var x in ActiveOrders)
            {
                if (x.Value.Entity == pData.Player)
                {
                    RemoveOrder(x.Key, x.Value, false);

                    break;
                }
            }

            var jobVehicleData = (VehicleData)args[0];

            pData.Player.TriggerEvent("Player::SCJ", Id, jobVehicleData.Vehicle.Id, ActiveOrders.Where(x => x.Value.CurrentWorker == null).Select(x => $"{x.Key}_{x.Value.Position.X}_{x.Value.Position.Y}_{x.Value.Position.Z}").ToList());
        }

        public override void SetPlayerNoJob(PlayerInfo pInfo)
        {
            base.SetPlayerNoJob(pInfo);

            var orderPair = ActiveOrders.Where(x => x.Value.CurrentWorker == pInfo).FirstOrDefault();

            if (orderPair.Value != null)
            {
                SetOrderAsNotTaken(orderPair.Key, orderPair.Value);
            }

            Vehicles.Where(x => x.OwnerID == pInfo.CID).FirstOrDefault()?.VehicleData?.Delete(false);
        }

        public override bool CanPlayerDoThisJob(PlayerData pData)
        {
            if (!pData.HasLicense(LicenseType.B, true))
                return false;

            return true;
        }

        public override void PostInitialize()
        {

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
            var orderPair = ActiveOrders.Where(x => x.Value.CurrentWorker == pData.Info).FirstOrDefault();

            if (orderPair.Value != null)
            {
                SetOrderAsNotTaken(orderPair.Key, orderPair.Value);
            }
        }
    }
}
