using GTANetworkAPI;
using System.Collections.Generic;
using BlaineRP.Server.Extensions.GTANetworkAPI;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Estates
{
    public partial class Apartments : HouseBase
    {
        /// <summary>Словарь всех квартир</summary>
        public static Dictionary<uint, Apartments> All { get; set; } = new Dictionary<uint, Apartments>();

        public static Dictionary<int, float> TaxCoeffs { get; set; }

        public uint RootId { get; }

        public ushort FloorIdx { get; }

        public ushort SubIdx { get; }

        public override Vector4 PositionParams => ApartmentsRoot.Get(RootId).Shell.GetApartmentsPosition(FloorIdx, SubIdx);

        /// <summary>Данные многоквартирного дома</summary>
        public ApartmentsRoot Root => ApartmentsRoot.Get(RootId);

        public Apartments(uint HID, uint RootId, ushort FloorIdx, ushort SubIdx, Style.RoomTypes RoomType, uint Price) : base(HID, Types.Apartments, RoomType)
        {
            this.RootId = RootId;

            this.FloorIdx = FloorIdx;
            this.SubIdx = SubIdx;

            this.Price = Price;

            this.Dimension = HID + Properties.Settings.Profile.Current.Game.ApartmentsDimensionBaseOffset;

            All.Add(HID, this);
        }

        public static Apartments Get(uint id) => All.GetValueOrDefault(id);

        public override void UpdateOwner(PlayerInfo pInfo)
        {
            base.UpdateOwner(pInfo);

            World.Service.SetSharedData($"Apartments::{Id}::OName", pInfo == null ? null : $"{pInfo.Name} {pInfo.Surname} [#{pInfo.CID}]");
        }

        public override bool IsEntityNearEnter(Entity entity) => entity.Dimension == Root.Dimension && entity.Position.DistanceIgnoreZ(PositionParams.Position) <= Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE;

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
                var root = Root;

                var pos = PositionParams;

                Utils.TeleportPlayers(pos.Position, false, root.Dimension, pos.RotationZ, true, players);
            }
            else
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(players, "House::Exit");
            }
        }

        public void SetPlayersOutsideOfRoot(bool teleport, params Player[] players)
        {
            if (teleport)
            {
                var root = Root;

                var pos = root.EnterParams;

                Utils.TeleportPlayers(pos.Position, false, Properties.Settings.Static.MainDimension, pos.RotationZ, true, players);
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
                Owner.PlayerData?.RemoveApartmentsProperty(this);
            }

            if (pInfo != null)
            {
                pInfo.PlayerData?.AddApartmentsProperty(this);

                var minBalance = Properties.Settings.Static.MIN_PAID_HOURS_HOUSE_APS * (uint)Tax;

                if (buyGov && Balance < minBalance)
                    SetBalance(minBalance, null);
            }

            foreach (var x in Settlers.Keys)
                SettlePlayer(x, false, null);

            UpdateOwner(pInfo);

            MySQL.HouseUpdateOwner(this);
        }
    }
}
