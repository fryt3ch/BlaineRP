using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BCRPServer.Game.Jobs
{
    public class Cabbie : Job, IVehicles
    {
        public class OrderInfo
        {
            public bool Exists => ActiveOrders.ContainsValue(this);

            public Entity Entity { get; set; }

            public Vector3 Position { get; set; }

            public PlayerData.PlayerInfo CurrentWorker { get; set; }

            public CancellationTokenSource GPSTrackerCTS { get; set; }

            public OrderInfo(Entity Entity, Vector3 Position)
            {
                this.Entity = Entity;
                this.Position = Position;
            }
        }

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

            TriggerEventToWorkersByJobType(Types.Cabbie, "Job::CAB::OC", $"{id}_{orderInfo.Position.X}_{orderInfo.Position.Y}_{orderInfo.Position.Z}");
        }

        public static void RemoveOrder(uint id, OrderInfo oInfo, bool success)
        {
            if (ActiveOrders.Remove(id))
            {
                if (oInfo.GPSTrackerCTS != null)
                {
                    oInfo.GPSTrackerCTS.Cancel();
                    oInfo.GPSTrackerCTS.Dispose();
                }

                FreeOrderId(id);

                if (oInfo.CurrentWorker == null)
                {
                    TriggerEventToWorkersByJobType(Types.Cabbie, "Job::CAB::OC", id);
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

            TriggerEventToWorkersByJobType(Types.Cabbie, "Job::CAB::OC", orderId);

            if (oInfo.Entity is Player player)
            {
                player.TriggerEvent("Taxi::UO", pDataDriver.Player.Id, pDataDriver.Info.PhoneNumber);

                if (oInfo.GPSTrackerCTS != null)
                {
                    oInfo.GPSTrackerCTS.Cancel();
                    oInfo.GPSTrackerCTS.Dispose();
                }

                oInfo.GPSTrackerCTS = new CancellationTokenSource();

                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        var doAction = true;

                        while (doAction)
                        {
                            await System.Threading.Tasks.Task.Delay(5000, oInfo.GPSTrackerCTS.Token);

                            NAPI.Task.Run(() =>
                            {
                                if (!oInfo.Exists || oInfo.CurrentWorker == null)
                                {
                                    doAction = false;

                                    return;
                                }

                                var pos = pDataDriver.Player.Position;

                                player.SendGPSTracker(0, pos.X, pos.Y, pDataDriver.Player);
                            });
                        }
                    }
                    catch(System.Exception ex) { }
                });
            }
        }

        public static void SetOrderAsNotTaken(uint orderId, OrderInfo oInfo)
        {
            oInfo.CurrentWorker = null;

            if (oInfo.GPSTrackerCTS != null)
            {
                oInfo.GPSTrackerCTS.Cancel();
                oInfo.GPSTrackerCTS.Dispose();
            }

            TriggerEventToWorkersByJobType(Types.Cabbie, "Job::CAB::OC", $"{orderId}_{oInfo.Position.X}_{oInfo.Position.Y}_{oInfo.Position.Z}");

            if (oInfo.Entity is Player player)
            {
                player.TriggerEvent("Taxi::UO", false);
            }
        }

        public override string ClientData => $"{Id}, {Position.ToCSharpStr()}";

        public List<VehicleData> Vehicles { get; set; } = new List<VehicleData>();

        public string NumberplateText { get; set; } = "TAXI";

        public uint VehicleRentPrice { get; set; }

        public Cabbie(Utils.Vector4 Position) : base(Types.Cabbie, Position)
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

        public override void SetPlayerNoJob(PlayerData.PlayerInfo pInfo)
        {
            base.SetPlayerNoJob(pInfo);

            var orderPair = ActiveOrders.Where(x => x.Value.CurrentWorker == pInfo).FirstOrDefault();

            if (orderPair.Value != null)
            {
                SetOrderAsNotTaken(orderPair.Key, orderPair.Value);
            }

            Vehicles.Where(x => x.OwnerID == pInfo.CID).FirstOrDefault()?.Delete(false);
        }

        public override void Initialize()
        {
            var taxiVehData = Data.Vehicles.GetData("taxi");

            var taxiColour1 = new Utils.Colour(255, 207, 32, 255);
            var taxiColour2 = new Utils.Colour(255, 207, 32, 255);

            Vehicles.Add(VehicleData.NewJob(Id, taxiVehData, taxiColour1, taxiColour2, new Utils.Vector4(-255.9184f, 6056.99f, 31.54631f, 124.0276f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, taxiVehData, taxiColour1, taxiColour2, new Utils.Vector4(-258.7205f, 6059.436f, 31.34031f, 126.3335f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, taxiVehData, taxiColour1, taxiColour2, new Utils.Vector4(-261.5727f, 6062.245f, 31.17303f, 125.3396f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, taxiVehData, taxiColour1, taxiColour2, new Utils.Vector4(-264.7574f, 6064.779f, 31.07093f, 126.9705f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, taxiVehData, taxiColour1, taxiColour2, new Utils.Vector4(-267.6993f, 6067.127f, 31.07048f, 126.3737f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, taxiVehData, taxiColour1, taxiColour2, new Utils.Vector4(-270.1129f, 6069.721f, 31.07076f, 123.6445f), Utils.Dimensions.Main));
        }

        public override void PostInitialize()
        {

        }

        public void OnVehicleRespawned(VehicleData vData)
        {

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
