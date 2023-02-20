using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Game.Jobs
{
    public class Cabbie : Job, IVehicles
    {
        public class OrderInfo
        {
            public Entity Entity { get; set; }

            public Vector3 Position { get; set; }

            public VehicleData CurrentVehicle { get; set; }

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

        public static void RemoveOrder(uint id, OrderInfo oInfo)
        {
            if (ActiveOrders.Remove(id))
            {
                FreeOrderId(id);

                TriggerEventToWorkersByJobType(Types.Cabbie, "Job::CAB::OC", id);

                if (oInfo.Entity is Player player)
                {
                    player.TriggerEvent("Taxi::UO");
                }
            }
        }

        public static void SetOrderAsTaken(uint orderId, OrderInfo oInfo, VehicleData vData, PlayerData pDataDriver)
        {
            oInfo.CurrentVehicle = vData;

            TriggerEventToWorkersByJobType(Types.Cabbie, "Job::CAB::OC", orderId);

            if (oInfo.Entity is Player player)
            {
                player.TriggerEvent("Taxi::UO", pDataDriver.Player.Id, pDataDriver.Info.PhoneNumber);
            }
        }

        public static void SetOrderAsNotTaken(uint orderId, OrderInfo oInfo)
        {
            oInfo.CurrentVehicle = null;

            TriggerEventToWorkersByJobType(Types.Cabbie, "Job::TR::OC", $"{orderId}_{oInfo.Position.X}_{oInfo.Position.Y}_{oInfo.Position.Z}");

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

            var jobVehicleData = (VehicleData)args[0];

            pData.Player.TriggerEvent("Player::SCJ", Id, jobVehicleData.Vehicle.Id, ActiveOrders.Where(x => x.Value.CurrentVehicle == null).Select(x => $"{x.Key}_{x.Value.Position.X}_{x.Value.Position.Y}_{x.Value.Position.Z}").ToList());

            Sync.Quest.StartQuest(pData, Sync.Quest.QuestData.Types.JCAB1, 0, 0);
        }

        public override void SetPlayerNoJob(PlayerData.PlayerInfo pInfo)
        {
            base.SetPlayerNoJob(pInfo);

            pInfo.Quests.GetValueOrDefault(Sync.Quest.QuestData.Types.JCAB1)?.Cancel(pInfo);
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
            var order = ActiveOrders.Where(x => x.Value.CurrentVehicle == vData).Select(x => x.Value).FirstOrDefault();

            if (order != null)
            {
                order.CurrentVehicle = null;

                // notify ordered player
            }
        }
    }
}
