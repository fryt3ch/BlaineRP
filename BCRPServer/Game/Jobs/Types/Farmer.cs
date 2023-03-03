using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Jobs
{
    public class Farmer : Job, IVehicles
    {
        public Farmer(Game.Businesses.Farm FarmBusiness) : base(Types.Farmer, FarmBusiness.PositionInteract)
        {
            this.FarmBusiness = FarmBusiness;
        }

        public override string ClientData => $"{Id}, {FarmBusiness.ID}";

        public static uint LittleShovelModel { get; } = NAPI.Util.GetHashKey("prop_buck_spade_09");

        public Game.Businesses.Farm FarmBusiness { get; set; }

        public List<VehicleData> Vehicles { get; set; } = new List<VehicleData>();

        public uint VehicleRentPrice { get; set; }

        public override void Initialize()
        {
            var numberplate = "FARM";
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

                pInfo.PlayerData.Player.DetachObject(Sync.AttachSystem.Types.FarmPlantSmallShovel);

                Data.Customization.SetNoUniform(pInfo.PlayerData);
            }

            base.SetPlayerNoJob(pInfo);
        }

        public override bool CanPlayerDoThisJob(PlayerData pData)
        {
            return true;
        }

        public override void OnWorkerExit(PlayerData pData)
        {

        }

        public override void PostInitialize()
        {

        }

        public void OnVehicleRespawned(VehicleData vData)
        {

        }

        public static bool TryGetPlayerCurrentCropInfo(PlayerData pData, out int fieldIdx, out byte row, out byte col)
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
            row = byte.Parse(strData[1]);
            col = byte.Parse(strData[2]);

            return true;
        }

        public static void SetPlayerCurrentCropInfo(PlayerData pData, int fieldIdx, byte row, byte col) => pData.Player.SetData("FJOBD::CCI", $"{fieldIdx}&{row}&{col}");

        public static void ResetPlayerCurrentCropInfo(PlayerData pData) => pData.Player.ResetData("FJOBD::CCI");
    }
}
