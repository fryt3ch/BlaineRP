using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using BlaineRP.Server.Game.Quests;
using BlaineRP.Server.Sync;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Jobs
{
    public partial class BusDriver : Job, IVehicleRelated
    {
        public override string ClientData => $"{Id}, {Position.ToCSharpStr()}, new System.Collections.Generic.List<(uint, System.Collections.Generic.List<RAGE.Vector3>)>(){{{string.Join(',', Routes.Select(x => $"({x.Reward}, new System.Collections.Generic.List<RAGE.Vector3>(){{{string.Join(',', x.Positions.Select(y => y.ToCSharpStr()))}}})"))}}}";

        public List<VehicleInfo> Vehicles { get; set; } = new List<VehicleInfo>();

        public uint VehicleRentPrice { get; set; }

        public List<RouteData> Routes { get; set; }

        public override void SetPlayerJob(PlayerData pData, params object[] args)
        {
            base.SetPlayerJob(pData);

            var jobVehicleData = (VehicleData)args[0];

            pData.Player.TriggerEvent("Player::SCJ", Id, jobVehicleData.Vehicle.Id);

            Quest.StartQuest(pData, QuestType.JBD1, 0, 0);
        }

        public override void SetPlayerNoJob(PlayerInfo pInfo)
        {
            base.SetPlayerNoJob(pInfo);

            pInfo.Quests.GetValueOrDefault(QuestType.JBD1)?.Cancel(pInfo);

            Vehicles.Where(x => x.OwnerID == pInfo.CID).FirstOrDefault()?.VehicleData?.Delete(false);
        }

        public override bool CanPlayerDoThisJob(PlayerData pData)
        {
            if (!pData.HasLicense(LicenseType.D, true))
                return false;

            return true;
        }

        public override void OnWorkerExit(PlayerData pData)
        {

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

        public BusDriver(Vector4 Position) : base(JobType.BusDriver, Position)
        {

        }
    }
}
