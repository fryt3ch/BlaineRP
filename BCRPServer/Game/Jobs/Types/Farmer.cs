using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BCRPServer.Game.Jobs
{
    public class Farmer : Job, IVehicles
    {
        public Farmer(Game.Businesses.Farm FarmBusiness) : base(Types.Farmer, FarmBusiness.PositionInteract)
        {
            this.FarmBusiness = FarmBusiness;
        }

        public override string ClientData => $"{Id}, {FarmBusiness.ID}";

        public static uint LittleShovelModel => NAPI.Util.GetHashKey("prop_cs_trowel"); //prop_buck_spade_09

        public static uint BoxModel => NAPI.Util.GetHashKey("prop_cardbordbox_01a");

        public static uint WateringCanModel => NAPI.Util.GetHashKey("prop_wateringcan");

        public static uint EmptyBucketModel => NAPI.Util.GetHashKey("brp_p_farm_bucket_0");
        public static uint MilkBucketModel => NAPI.Util.GetHashKey("brp_p_farm_bucket_1");

        public static Game.Data.Vehicles.Vehicle TractorVehicleData => Game.Data.Vehicles.GetData("tractor2");
        public static Game.Data.Vehicles.Vehicle PlaneVehicleData => Game.Data.Vehicles.GetData("duster");

        public const byte ORANGES_ON_TREE_MIN_AMOUNT = 3;
        public const byte ORANGES_ON_TREE_MAX_AMOUNT = 12;

        public static decimal GetPlayerSalaryCoef(PlayerData.PlayerInfo pInfo) => 1m;

        private static Dictionary<Game.Businesses.Farm.CropField.Types, int> CropGrowTimes = new Dictionary<Businesses.Farm.CropField.Types, int>()
        {
            { Game.Businesses.Farm.CropField.Types.Cabbage, 45 * 60 },
            { Game.Businesses.Farm.CropField.Types.Pumpkin, 45 * 60 },

            { Game.Businesses.Farm.CropField.Types.Wheat, 90 * 60 },

            { Game.Businesses.Farm.CropField.Types.OrangeTree, 70 * 60 },

            { Game.Businesses.Farm.CropField.Types.Cow, 30 * 60 },
        };

        public static int GetGrowTimeForCrop(Game.Businesses.Farm.CropField.Types type) => CropGrowTimes.GetValueOrDefault(type);

        public static int CropFieldIrrigationDuration => 120 * 60;
        public static float CropFieldIrrigationTimeCoef => 0.75f;

        public Game.Businesses.Farm FarmBusiness { get; set; }

        public List<VehicleData.VehicleInfo> Vehicles { get; set; } = new List<VehicleData.VehicleInfo>();

        public uint VehicleRentPrice { get; set; }

        public override void Initialize()
        {
            var subId = SubId;

            var numberplate = $"FARM{FarmBusiness.ID}";

            var tractorType = TractorVehicleData;
            var planeType = PlaneVehicleData;

            if (subId == 0)
            {
                var tractorCol1 = new Utils.Colour(255, 0, 0, 255);
                var tractorCol2 = new Utils.Colour(0, 0, 0, 255);

                //2133.948f, 4783.06f, 41.31395f, 23.95123f - plane pos

                Vehicles.Add(VehicleData.NewJob(Id, numberplate, tractorType, tractorCol1, tractorCol2, new Utils.Vector4(1865.232f, 4872.092f, 44.38249f, 206.4608f), Utils.Dimensions.Main));
                Vehicles.Add(VehicleData.NewJob(Id, numberplate, tractorType, tractorCol1, tractorCol2, new Utils.Vector4(1870.744f, 4874.888f, 44.66641f, 206.8733f), Utils.Dimensions.Main));
                Vehicles.Add(VehicleData.NewJob(Id, numberplate, tractorType, tractorCol1, tractorCol2, new Utils.Vector4(1876.944f, 4878.157f, 45.11294f, 202.0746f), Utils.Dimensions.Main));
                Vehicles.Add(VehicleData.NewJob(Id, numberplate, tractorType, tractorCol1, tractorCol2, new Utils.Vector4(1859.166f, 4868.78f, 44.17879f, 210.0185f), Utils.Dimensions.Main));

                Vehicles.Add(VehicleData.NewJob(Id, numberplate, planeType, tractorCol1, tractorCol2, new Utils.Vector4(2143.947f, 4813.993f, 41.57311f, 114.3835f), Utils.Dimensions.Main));
            }

            foreach (var x in Vehicles.Where(x => x.Data == tractorType))
            {
                AttachHarvTrailOnTractor(x.VehicleData);
            }
        }

        public override void SetPlayerJob(PlayerData pData, params object[] args)
        {
            base.SetPlayerJob(pData, args);

            pData.Player.TriggerEvent("Player::SCJ", Id);

            Data.Customization.ApplyUniform(pData, Data.Customization.UniformTypes.Farmer);

            SetPlayerTotalCashSalary(pData, 0, false);
        }

        public override void SetPlayerNoJob(PlayerData.PlayerInfo pInfo)
        {
            if (pInfo.PlayerData != null)
            {
                var totalSalary = GetPlayerTotalCashSalary(pInfo.PlayerData);

                if (totalSalary > 0)
                {
                    ulong newCash;

                    if (pInfo.PlayerData.TryAddCash(totalSalary, out newCash, true))
                        pInfo.PlayerData.SetCash(newCash);
                }

                ResetPlayerTotalCashSalary(pInfo.PlayerData);

                if (HasPlayerCurrentCropInfo(pInfo.PlayerData))
                {
                    pInfo.PlayerData.Player.DetachObject(Sync.AttachSystem.Types.FarmPlantSmallShovel);
                }
                else if (HasPlayerCurrentOrangeTreeInfo(pInfo.PlayerData))
                {
                    if (!pInfo.PlayerData.Player.DetachObject(Sync.AttachSystem.Types.FarmWateringCan))
                    {
                        if (!pInfo.PlayerData.Player.DetachObject(Sync.AttachSystem.Types.FarmOrangeBoxCarry))
                        {
                            int treeIdx;

                            if (TryGetPlayerCurrentOrangeTreeInfo(pInfo.PlayerData, out treeIdx))
                            {
                                if (pInfo.PlayerData.GeneralAnim == Sync.Animations.GeneralTypes.TreeCollect0)
                                    pInfo.PlayerData.StopGeneralAnim();

                                Game.Jobs.Farmer.ResetPlayerCurrentOrangeTreeInfo(pInfo.PlayerData);
                            }
                        }
                    }
                }
                else if (HasPlayerCurrentCowInfo(pInfo.PlayerData))
                {
                    if (!pInfo.PlayerData.Player.DetachObject(Sync.AttachSystem.Types.FarmMilkBucketCarry))
                    {
                        int cowIdx;

                        if (TryGetPlayerCurrentCowInfo(pInfo.PlayerData, out cowIdx))
                        {
                            if (pInfo.PlayerData.GeneralAnim == Sync.Animations.GeneralTypes.MilkCow0)
                                pInfo.PlayerData.StopGeneralAnim();

                            Game.Jobs.Farmer.ResetPlayerCurrentCowInfo(pInfo.PlayerData);
                        }
                    }
                }

                ResetPlayerFieldsIrrigationData(pInfo.PlayerData);

                Data.Customization.SetNoUniform(pInfo.PlayerData);
            }

            pInfo.Quests.GetValueOrDefault(Sync.Quest.QuestData.Types.JFRM1)?.Cancel(pInfo);
            pInfo.Quests.GetValueOrDefault(Sync.Quest.QuestData.Types.JFRM2)?.Cancel(pInfo);

            Vehicles.Where(x => x.OwnerID == pInfo.CID).FirstOrDefault()?.VehicleData?.Delete(false);

            base.SetPlayerNoJob(pInfo);
        }

        public bool CanPlayerUseTractor(PlayerData pData)
        {
            return true;
        }

        public override bool CanPlayerDoThisJob(PlayerData pData)
        {
            return true;
        }

        public override void OnWorkerExit(PlayerData pData)
        {
            SetPlayerNoJob(pData.Info);
        }

        public override void PostInitialize()
        {

        }

        public void OnVehicleRespawned(VehicleData.VehicleInfo vInfo, PlayerData.PlayerInfo pInfo)
        {
            if (vInfo.Data == TractorVehicleData)
            {
                if (pInfo != null)
                {
                    pInfo.Quests.GetValueOrDefault(Sync.Quest.QuestData.Types.JFRM1)?.Cancel(pInfo);
                }

                if (vInfo.VehicleData != null)
                    AttachHarvTrailOnTractor(vInfo.VehicleData);
            }
            else if (vInfo.Data == PlaneVehicleData)
            {
                if (pInfo != null)
                {
                    pInfo.Quests.GetValueOrDefault(Sync.Quest.QuestData.Types.JFRM2)?.Cancel(pInfo);

                    if (pInfo.PlayerData != null)
                        ResetPlayerFieldsIrrigationData(pInfo.PlayerData);
                }
            }
        }

        public static bool HasPlayerCurrentCropInfo(PlayerData pData) => pData.Player.HasData("FJOBD::CCI");

        public static bool TryGetPlayerCurrentCropInfo(PlayerData pData, out int fieldIdx, out byte col, out byte row)
        {
            var strData = pData.Player.GetData<string>("FJOBD::CCI")?.Split('&');

            if (strData == null)
            {
                fieldIdx = 0;
                row = 0;
                col = 0;

                return false;
            }

            fieldIdx = int.Parse(strData[0]);
            col = byte.Parse(strData[1]);
            row = byte.Parse(strData[2]);

            return true;
        }

        public PlayerData GetCropCurrentWorker(int fieldIdx, byte col, byte row)
        {
            var str = $"{fieldIdx}&{col}&{row}";

            return Workers.Where(x => x.Player.GetData<string>("FJOBD::CCI") == str).FirstOrDefault();
        }

        public PlayerData GetOrangeTreeCurrentWorker(int treeIdx)
        {
            int t;

            return Workers.Where(x => TryGetPlayerCurrentOrangeTreeInfo(x, out t) && t == treeIdx).FirstOrDefault();
        }

        public PlayerData GetCowCurrentWorker(int cowIdx)
        {
            int t;

            return Workers.Where(x => TryGetPlayerCurrentCowInfo(x, out t) && t == cowIdx).FirstOrDefault();
        }

        public static void SetPlayerCurrentTimer(PlayerData pData, Timer timer) => pData.Player.SetData("FJOBD::Timer", timer);

        public static void ResetPlayerCurrentTimer(PlayerData pData) => pData.Player.ResetData("FJOBD::Timer");

        public static Timer GetPlayerCurrentTimer(PlayerData pData) => pData.Player.GetData<Timer>("FJOBD::Timer");

        public static void SetPlayerCurrentCropInfo(PlayerData pData, int fieldIdx, byte col, byte row) => pData.Player.SetData("FJOBD::CCI", $"{fieldIdx}&{col}&{row}");

        public static void ResetPlayerCurrentCropInfo(PlayerData pData)
        {
            pData.Player.ResetData("FJOBD::CCI");

            var timer = GetPlayerCurrentTimer(pData);

            if (timer != null)
            {
                timer.Dispose();

                ResetPlayerCurrentTimer(pData);
            }
        }

        public static bool HasPlayerCurrentOrangeTreeInfo(PlayerData pData) => pData.Player.HasData("FJOBD::COTI");

        public static bool TryGetPlayerCurrentOrangeTreeInfo(PlayerData pData, out int idx)
        {
            if (!pData.Player.HasData("FJOBD::COTI"))
            {
                idx = 0;

                return false;
            }

            idx = pData.Player.GetData<int>("FJOBD::COTI");

            return true;
        }

        public static void SetPlayerCurrentOrangeTreeInfo(PlayerData pData, int idx) => pData.Player.SetData("FJOBD::COTI", idx);

        public static void ResetPlayerCurrentOrangeTreeInfo(PlayerData pData)
        {
            pData.Player.ResetData("FJOBD::COTI");

            var timer = GetPlayerCurrentTimer(pData);

            if (timer != null)
            {
                timer.Dispose();

                ResetPlayerCurrentTimer(pData);
            }
        }

        public static bool HasPlayerCurrentCowInfo(PlayerData pData) => pData.Player.HasData("FJOBD::CCOWI");

        public static bool TryGetPlayerCurrentCowInfo(PlayerData pData, out int idx)
        {
            if (!pData.Player.HasData("FJOBD::CCOWI"))
            {
                idx = 0;

                return false;
            }

            idx = pData.Player.GetData<int>("FJOBD::CCOWI");

            return true;
        }

        public static void SetPlayerCurrentCowInfo(PlayerData pData, int idx) => pData.Player.SetData("FJOBD::CCOWI", idx);

        public static void ResetPlayerCurrentCowInfo(PlayerData pData)
        {
            pData.Player.ResetData("FJOBD::CCOWI");

            var timer = GetPlayerCurrentTimer(pData);

            if (timer != null)
            {
                timer.Dispose();

                ResetPlayerCurrentTimer(pData);
            }
        }

        public static Dictionary<int, HashSet<int>> GetPlayerFieldsIrrigationData(PlayerData pData) => pData.Player.GetData<Dictionary<int, HashSet<int>>>("FJOBD::FIRD");

        public static void SetPlayerFieldsIrrigationData(PlayerData pData) => pData.Player.SetData("FJOBD::FIRD", new Dictionary<int, HashSet<int>>());

        public static void ResetPlayerFieldsIrrigationData(PlayerData pData) => pData.Player.ResetData("FJOBD::FIRD");

        public static bool AttachHarvTrailOnTractor(VehicleData vData)
        {
            return vData.Vehicle.AttachObject(Game.Data.Vehicles.GetData("raketrailer").Model, Sync.AttachSystem.Types.TractorTrailFarmHarv, -1, null);
        }

        public void SetPlayerAsTractorTaker(PlayerData pData, VehicleData vData)
        {
            Sync.Quest.StartQuest(pData, Sync.Quest.QuestData.Types.JFRM1, 0, 0, $"{vData.Vehicle.Id}");
        }

        public void SetPlayerAsPlaneIrrigator(PlayerData pData, VehicleData vData)
        {
            Sync.Quest.StartQuest(pData, Sync.Quest.QuestData.Types.JFRM2, 0, 0, $"{vData.Vehicle.Id}");

            SetPlayerFieldsIrrigationData(pData);
        }
    }
}
