using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Additional;
using BlaineRP.Server.Extensions.GTANetworkAPI;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using BlaineRP.Server.Game.Management.AntiCheat;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Estates
{
    public partial class House : HouseBase
    {
        /// <summary>Словарь всех домов</summary>
        public static Dictionary<uint, House> All { get; private set; } = new Dictionary<uint, House>();

        public Vector4 GarageOutside { get; set; }

        /// <summary>Тип гаража</summary>
        public Garage.Style GarageData { get; private set; }

        public override Vector4 PositionParams { get; }

        public House(uint HID, Vector4 PositionParams, Style.RoomTypes RoomType, uint Price, Garage.Types? GarageType = null, Vector4 GarageOutside = null) : base(HID, Types.House, RoomType)
        {
            this.PositionParams = PositionParams;

            this.Price = Price;
            this.Dimension = (uint)(HID + Properties.Settings.Profile.Current.Game.HouseDimensionBaseOffset);

            if (GarageType is Garage.Types gType)
            {
                this.GarageData = Garage.Style.Get(gType, 0);
            }

            this.GarageOutside = GarageOutside;

            All.Add(HID, this);
        }

        public static House Get(uint id) => All.GetValueOrDefault(id);

        public override void UpdateOwner(PlayerInfo pInfo)
        {
            base.UpdateOwner(pInfo);

            World.Service.SetSharedData($"House::{Id}::OName", pInfo == null ? null : $"{pInfo.Name} {pInfo.Surname} [#{pInfo.CID}]");
        }

        public override bool IsEntityNearEnter(Entity entity) => entity.Dimension == Properties.Settings.Static.MainDimension && entity.Position.DistanceIgnoreZ(PositionParams.Position) <= Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE;

        public bool IsEntityNearVehicleEnter(Entity entity) => GarageOutside == null ? false : entity.Dimension == Properties.Settings.Static.MainDimension && entity.Position.DistanceIgnoreZ(GarageOutside.Position) <= Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE;

        public override void SetPlayersInside(bool teleport, params Player[] players)
        {
            if (teleport)
            {
                var sData = StyleData;

                Utils.TeleportPlayers(sData.Position, false, Dimension, sData.Heading, true, players);
            }
            else
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(players, "House::Enter", ToClientJson());
            }
        }

        public override void SetPlayersOutside(bool teleport, params Player[] players)
        {
            if (teleport)
            {
                Utils.TeleportPlayers(PositionParams.Position, false, Properties.Settings.Static.MainDimension, PositionParams.RotationZ, true, players);
            }
            else
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(players, "House::Exit");
            }
        }

        public override void ChangeOwner(PlayerInfo pInfo, bool buyGov = false)
        {
            if (Owner != null)
            {
                Owner.PlayerData?.RemoveHouseProperty(this);
            }

            if (pInfo != null)
            {
                pInfo.PlayerData?.AddHouseProperty(this);

                var minBalance = Properties.Settings.Static.MIN_PAID_HOURS_HOUSE_APS * (uint)Tax;

                if (buyGov && Balance < minBalance)
                    SetBalance(minBalance, null);
            }

            var vehsInGarage = GetVehiclesInGarage();

            foreach (var x in vehsInGarage)
            {
                x.SetToVehiclePound();
            }

            foreach (var x in Settlers.Keys)
                SettlePlayer(x, false, null);

            UpdateOwner(pInfo);

            MySQL.HouseUpdateOwner(this);
        }

        public void SetVehicleToGarage(VehicleData vData, int slot)
        {
            vData.EngineOn = false;

            var vPos = GarageData.VehiclePositions[slot];

            vData.AttachBoatToTrailer();

            vData.Vehicle.Teleport(vPos.Position, Dimension, vPos.RotationZ, true, VehicleTeleportType.All);

            vData.SetFreezePosition(vPos.Position, vPos.RotationZ);
            vData.IsInvincible = true;

            SetVehicleToGarageOnlyData(vData.Info, slot);
        }

        public void SetVehicleToGarageOnSpawn(VehicleData vData)
        {
            var vPos = GarageData.VehiclePositions[vData.LastData.GarageSlot];

            vData.Vehicle.Position = vPos.Position;
            vData.Vehicle.SetHeading(vPos.RotationZ);

            vData.IsFrozen = true;
            vData.IsInvincible = true;

            vData.AttachBoatToTrailer();
        }

        public void SetVehicleToGarageOnlyData(VehicleInfo vInfo, int slot)
        {
            var vPos = GarageData.VehiclePositions[slot];

            vInfo.LastData.Position.X = vPos.X;
            vInfo.LastData.Position.Y = vPos.Y;
            vInfo.LastData.Position.Z = vPos.Z;

            vInfo.LastData.Heading = vPos.RotationZ;

            vInfo.LastData.Dimension = Dimension;

            vInfo.LastData.GarageSlot = slot;

            MySQL.VehicleDeletionUpdate(vInfo);
        }

        public List<VehicleInfo> GetVehiclesInGarage()
        {
            if (GarageData == null || Owner == null)
                return new List<VehicleInfo>();

            return Owner.OwnedVehicles.Where(x => x.LastData.GarageSlot >= 0 && (x.VehicleData?.Vehicle.Dimension ?? x.LastData.Dimension) == Dimension).ToList();
        }
    }
}
