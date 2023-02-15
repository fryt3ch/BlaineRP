using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BCRPServer.Game.Jobs
{
    public class Trucker : Job, IVehicles
    {
        public const int MinimalRewardX = 1500;
        public const int MaximalRewardX = 2000;

        public const int MinimalRewardA = 1000;
        public const int MaximalRewardA = 1500;

        public static List<Trucker> AllTruckerJobs { get; set; }

        public class OrderInfo
        {
            public VehicleData CurrentVehicle { get; set; }

            public uint Reward { get; set; }

            public bool GotMaterials { get; set; }

            public int MPIdx { get; set; }

            public Game.Businesses.Business TargetBusiness { get; set; }

            public OrderInfo(Game.Businesses.Business TargetBusiness)
            {
                this.TargetBusiness = TargetBusiness;
            }
        }

        public Dictionary<int, OrderInfo> ActiveOrders { get; private set; } = new Dictionary<int, OrderInfo>();

        public void AddCustomOrder(Game.Businesses.Business business)
        {
            var orderInfo = new OrderInfo(business);

            ActiveOrders.Add(business.ID, orderInfo);

            orderInfo.Reward = (uint)Utils.Randoms.Chat.Next(MinimalRewardX, MaximalRewardX);

            orderInfo.MPIdx = GetFarthestMaterialsPositionIdx(business.PositionInfo);

            TriggerEventToWorkers("Job::TR::OC", $"{business.ID}_{orderInfo.MPIdx}_{orderInfo.Reward}");
        }

        public void AddDefaultOrder(Game.Businesses.Business business)
        {
            var orderInfo = new OrderInfo(business);

            ActiveOrders.Add(-business.ID, orderInfo);

            orderInfo.Reward = (uint)Utils.Randoms.Chat.Next(MinimalRewardX, MaximalRewardX);

            orderInfo.MPIdx = GetFarthestMaterialsPositionIdx(business.PositionInfo);

            TriggerEventToWorkers("Job::TR::OC", $"{-business.ID}_{orderInfo.MPIdx}_{orderInfo.Reward}");
        }

        public void RemoveOrder(int id)
        {
            if (ActiveOrders.Remove(id))
            {
                TriggerEventToWorkers("Job::TR::OC", id);
            }
        }

        public bool TryAddRandomDefaultOrder()
        {
            var businesses = Game.Businesses.Business.All.Values.Where(x => x.PositionInfo != null && x.ClosestTruckerJob == this && !ActiveOrders.ContainsKey(-x.ID)).ToList();

            if (businesses.Count == 0)
                return false;

            var business = businesses[Utils.Randoms.Chat.Next(0, businesses.Count)];

            AddDefaultOrder(business);

            return true;
        }

        public List<Vector3> MaterialsPositions { get; set; }

        public List<VehicleData> Vehicles { get; set; } = new List<VehicleData>();

        public string NumberplateText { get; set; } = "TRUCK";

        public uint VehicleRentPrice { get; set; }

        public override string ClientData => $"{Id}, {Position.ToCSharpStr()}, new List<Vector3>(){{{string.Join(',', MaterialsPositions.Select(x => x.ToCSharpStr()))}}}";

        public Trucker(Utils.Vector4 Position) : base(Types.Trucker, Position)
        {

        }

        public void SetOrderAsTaken(int orderId, OrderInfo oInfo, VehicleData vData)
        {
            oInfo.CurrentVehicle = vData;

            TriggerEventToWorkers("Job::TR::OC", orderId);
        }

        public void SetOrderAsNotTaken(int orderId, OrderInfo oInfo)
        {
            oInfo.CurrentVehicle = null;

            TriggerEventToWorkers("Job::TR::OC", $"{orderId}_{oInfo.MPIdx}_{oInfo.Reward}");
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

            pData.Player.TriggerEvent("Player::SCJ", Id, jobVehicleData.Vehicle.Id, ActiveOrders.Where(x => x.Value.CurrentVehicle == null).Select(x => $"{x.Key}_{x.Value.MPIdx}_{x.Value.Reward}").ToList());
        }

        public override void Initialize()
        {
            Vehicles.Add(VehicleData.NewJob(Id, Data.Vehicles.GetData("pounder"), Utils.Colour.DefWhite, Utils.Colour.DefBlack, new Utils.Vector4(36.48936f, 6342.64f, 31.30971f, 14.86628f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, Data.Vehicles.GetData("pounder"), Utils.Colour.DefWhite, Utils.Colour.DefBlack, new Utils.Vector4(30.00755f, 6338.54f, 31.3096f, 15.64089f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, Data.Vehicles.GetData("pounder"), Utils.Colour.DefWhite, Utils.Colour.DefBlack, new Utils.Vector4(23.15289f, 6334.313f, 31.30952f, 15.82415f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, Data.Vehicles.GetData("pounder"), Utils.Colour.DefWhite, Utils.Colour.DefBlack, new Utils.Vector4(16.22741f, 6331.058f, 31.30931f, 16.68959f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, Data.Vehicles.GetData("pounder"), Utils.Colour.DefWhite, Utils.Colour.DefBlack, new Utils.Vector4(9.375045f, 6326.348f, 31.30978f, 16.85452f), Utils.Dimensions.Main));

            Vehicles.Add(VehicleData.NewJob(Id, Data.Vehicles.GetData("pounder"), Utils.Colour.DefBlack, Utils.Colour.DefBlack, new Utils.Vector4(13.45169f, 6349.37f, 31.30666f, 211.8596f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, Data.Vehicles.GetData("pounder"), Utils.Colour.DefBlack, Utils.Colour.DefBlack, new Utils.Vector4(18.80654f, 6355.293f, 31.30764f, 213.5229f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, Data.Vehicles.GetData("pounder"), Utils.Colour.DefBlack, Utils.Colour.DefBlack, new Utils.Vector4(24.34281f, 6360.932f, 31.30667f, 213.5152f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, Data.Vehicles.GetData("pounder"), Utils.Colour.DefBlack, Utils.Colour.DefBlack, new Utils.Vector4(29.61494f, 6366.637f, 31.30571f, 214.6733f), Utils.Dimensions.Main));
        }

        public override void PostInitialize()
        {
            int counter = 5;

            while (counter > 0 && TryAddRandomDefaultOrder())
            {
                counter--;
            }
        }
    }
}
