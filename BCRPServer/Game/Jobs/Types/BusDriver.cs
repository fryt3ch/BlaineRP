using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Jobs
{
    public class BusDriver : Job, IVehicles
    {
        public class RouteData
        {
            public List<Vector3> Positions { get; set; }

            public uint Reward { get; set; }

            public RouteData(uint Reward, List<Vector3> Positions)
            {
                this.Reward = Reward;

                this.Positions = Positions;
            }
        }

        public override string ClientData => $"{Id}, {Position.ToCSharpStr()}, new List<(uint, List<Vector3>)>(){{{string.Join(',', Routes.Select(x => $"({x.Reward}, new List<Vector3>(){{{string.Join(',', x.Positions.Select(y => y.ToCSharpStr()))}}})"))}}}";

        public List<VehicleData.VehicleInfo> Vehicles { get; set; } = new List<VehicleData.VehicleInfo>();

        public uint VehicleRentPrice { get; set; }

        public List<RouteData> Routes { get; set; }

        public override void SetPlayerJob(PlayerData pData, params object[] args)
        {
            base.SetPlayerJob(pData);

            var jobVehicleData = (VehicleData)args[0];

            pData.Player.TriggerEvent("Player::SCJ", Id, jobVehicleData.Vehicle.Id);

            Sync.Quest.StartQuest(pData, Sync.Quest.QuestData.Types.JBD1, 0, 0);
        }

        public override void SetPlayerNoJob(PlayerData.PlayerInfo pInfo)
        {
            base.SetPlayerNoJob(pInfo);

            pInfo.Quests.GetValueOrDefault(Sync.Quest.QuestData.Types.JBD1)?.Cancel(pInfo);

            Vehicles.Where(x => x.OwnerID == pInfo.CID).FirstOrDefault()?.VehicleData?.Delete(false);
        }

        public override bool CanPlayerDoThisJob(PlayerData pData)
        {
            if (!pData.HasLicense(PlayerData.LicenseTypes.D, true))
                return false;

            return true;
        }

        public override void Initialize()
        {
            if (SubId == 0)
            {
                var numberplateText = "BUSBC";

                var vType = Game.Data.Vehicles.GetData("coach");

                var colour1 = new Utils.Colour(255, 255, 255, 255);
                var colour2 = new Utils.Colour(255, 0, 0, 255);

                Vehicles.Add(VehicleData.NewJob(Id, numberplateText, vType, colour1, colour2, new Utils.Vector4(-766.2029f, 5524.796f, 34.31338f, 28.3842f), Settings.MAIN_DIMENSION));
                Vehicles.Add(VehicleData.NewJob(Id, numberplateText, vType, colour1, colour2, new Utils.Vector4(-762.9031f, 5526.446f, 34.31443f, 29.5885f), Settings.MAIN_DIMENSION));
                Vehicles.Add(VehicleData.NewJob(Id, numberplateText, vType, colour1, colour2, new Utils.Vector4(-759.7948f, 5528.181f, 34.31483f, 30.55079f), Settings.MAIN_DIMENSION));
                Vehicles.Add(VehicleData.NewJob(Id, numberplateText, vType, colour1, colour2, new Utils.Vector4(-756.5928f, 5529.695f, 34.31594f, 29.94526f), Settings.MAIN_DIMENSION));
                Vehicles.Add(VehicleData.NewJob(Id, numberplateText, vType, colour1, colour2, new Utils.Vector4(-753.6445f, 5532.07f, 34.31636f, 29.10272f), Settings.MAIN_DIMENSION));
                Vehicles.Add(VehicleData.NewJob(Id, numberplateText, vType, colour1, colour2, new Utils.Vector4(-750.3279f, 5533.969f, 34.31674f, 29.43421f), Settings.MAIN_DIMENSION));
            }
            else
            {
                var numberplateText = "BUSLS";

                var vType = Game.Data.Vehicles.GetData("bus");
            }
        }

        public override void OnWorkerExit(PlayerData pData)
        {

        }

        public override void PostInitialize()
        {

        }

        public void OnVehicleRespawned(VehicleData.VehicleInfo vInfo, PlayerData.PlayerInfo pInfo)
        {
            if (pInfo != null)
            {
                SetPlayerNoJob(pInfo);
            }
        }

        public BusDriver(Utils.Vector4 Position) : base(Types.BusDriver, Position)
        {

        }
    }
}
