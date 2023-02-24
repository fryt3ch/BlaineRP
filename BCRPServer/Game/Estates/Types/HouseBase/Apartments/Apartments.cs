using GTANetworkAPI;
using System.Collections.Generic;

namespace BCRPServer.Game.Estates
{
    public partial class Apartments : HouseBase
    {
        public partial class ApartmentsRoot
        {
            public static Dictionary<Types, ApartmentsRoot> All { get; set; } = new Dictionary<Types, ApartmentsRoot>();

            /// <summary>Типы многоквартирных домов</summary>
            public enum Types
            {
                Cheap1 = 0,
            }

            public Types Type { get; set; }

            public Utils.Vector4 EnterParams { get; set; }

            public Utils.Vector4 ExitParams { get; set; }

            public int StartFloor { get; set; }

            public int FloorsAmount { get; set; }

            public Utils.Vector4 FloorPosition { get; set; }

            public float FloorDistZ { get; set; }

            /// <summary>Измерение многоквартирного дома</summary>
            public uint Dimension { get; set; }

            public ApartmentsRoot(Types Type, Utils.Vector4 EnterParams, Utils.Vector4 ExitParams, int FloorsAmount, Utils.Vector4 FloorPosition, float FloorDistZ, int StartFloor = 1)
            {
                this.Type = Type;

                this.EnterParams = EnterParams;
                this.ExitParams = ExitParams;

                this.Dimension = (uint)(Utils.ApartmentsRootDimBase + (int)Type);

                this.FloorsAmount = FloorsAmount;
                this.FloorPosition = FloorPosition;
                this.FloorDistZ = FloorDistZ;
                this.StartFloor = StartFloor;

                All.Add(Type, this);
            }

            public static ApartmentsRoot Get(Types type) => All.GetValueOrDefault(type);

            public Vector3 GetFloorPosition(int floor)
            {
                if (floor < StartFloor || floor > FloorsAmount)
                    return null;

                return new Vector3(FloorPosition.X, FloorPosition.Y, FloorPosition.Z + (floor - StartFloor) * FloorDistZ);
            }

            public void SetPlayersInside(bool teleport, params Player[] players)
            {
                if (teleport)
                {
                    var pos = ExitParams;

                    Utils.TeleportPlayers(pos.Position, false, Dimension, pos.RotationZ, true, players);
                }
                else
                {
                    NAPI.ClientEvent.TriggerClientEventToPlayers(players, "ARoot::Enter", Type);
                }
            }

            public void SetPlayersOutside(bool teleport, params Player[] players)
            {
                if (teleport)
                {
                    var pos = EnterParams;

                    Utils.TeleportPlayers(pos.Position, false, Utils.Dimensions.Main, pos.RotationZ, true, players);
                }
                else
                {
                    NAPI.ClientEvent.TriggerClientEventToPlayers(players, "ARoot::Exit");
                }
            }
        }

        /// <summary>Словарь всех квартир</summary>
        public static Dictionary<uint, Apartments> All { get; set; } = new Dictionary<uint, Apartments>();

        public static Dictionary<int, float> TaxCoeffs { get; set; }

        /// <summary>Данные многоквартирного дома</summary>
        public ApartmentsRoot Root { get; private set; }

        public Apartments(uint HID, Utils.Vector4 EnterParams, ApartmentsRoot.Types RootType, Style.RoomTypes RoomType, int Price) : base(HID, EnterParams, Types.Apartments, RoomType)
        {
            this.Root = ApartmentsRoot.Get(RootType);

            this.Price = Price;

            this.Dimension = (uint)(HID + Utils.ApartmentsDimBase);

            All.Add(HID, this);
        }

        public static Apartments Get(uint id) => All.GetValueOrDefault(id);

        public override void UpdateOwner(PlayerData.PlayerInfo pInfo)
        {
            base.UpdateOwner(pInfo);

            Sync.World.SetSharedData($"Apartments::{Id}::OName", pInfo == null ? null : $"{pInfo.Name} {pInfo.Surname} [#{pInfo.CID}]");
        }

        public override bool IsEntityNearEnter(Entity entity) => entity.Dimension == Root.Dimension && entity.Position.DistanceIgnoreZ(PositionParams.Position) <= Settings.ENTITY_INTERACTION_MAX_DISTANCE;

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

                Utils.TeleportPlayers(pos.Position, false, Utils.Dimensions.Main, pos.RotationZ, true, players);
            }
            else
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(players, "House::Exit");
            }
        }

        public override void ChangeOwner(PlayerData.PlayerInfo pInfo)
        {
            if (Owner != null)
            {
                Owner.PlayerData?.RemoveApartmentsProperty(this);
            }

            if (pInfo != null)
            {
                pInfo.PlayerData?.AddApartmentsProperty(this);
            }

            foreach (var x in Settlers.Keys)
                SettlePlayer(x, false, null);

            UpdateOwner(pInfo);

            MySQL.HouseUpdateOwner(this);
        }
    }
}
