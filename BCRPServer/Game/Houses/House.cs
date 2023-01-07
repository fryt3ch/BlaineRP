using GTANetworkAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Game.Houses
{
    public interface IDimensionable
    {
        public void SetPlayersInside(params Player[] players);

        public void SetPlayersOutside(params Player[] players);
    }

    public abstract class HouseBase : IDimensionable
    {
        public static Utils.Colour DefaultLightColour => new Utils.Colour(255, 187, 96, 255);

        public class Style
        {
            /// <summary>Типы планировки</summary>
            public enum Types
            {
                First = 0,
                Second,
                Third,
                Fourth,
                Fifth,
            }

            /// <summary>Типы комнат</summary>
            public enum RoomTypes
            {
                One = 1,
                Two = 2,
                Three = 3,
                Four = 4,
                Five = 5,
            }

            public Types Type { get; private set; }

            public RoomTypes RoomType { get; private set; }

            public HouseBase.Types HouseType { get; private set; }

            public Vector3 Position { get; private set; }

            public float Heading { get; private set; }

            public int LightsCount { get; private set; }

            public int DoorsCount { get; private set; }

            /// <summary>Словарь планировок</summary>
            private static Dictionary<HouseBase.Types, Dictionary<RoomTypes, Dictionary<Types, Style>>> All { get; set; } = new Dictionary<HouseBase.Types, Dictionary<RoomTypes, Dictionary<Types, Style>>>();

            public static Style Get(HouseBase.Types hType, RoomTypes rType, Types sType) => All.GetValueOrDefault(hType)?.GetValueOrDefault(rType)?.GetValueOrDefault(sType);

            public Style(HouseBase.Types HouseType, RoomTypes RoomType, Types Type, Vector3 Position, float Heading, int LightsCount = 0, int DoorsCount = 0)
            {
                this.HouseType = HouseType;
                this.RoomType = RoomType;

                this.Type = Type;

                this.Position = Position;
                this.Heading = Heading;

                this.LightsCount = LightsCount;
                this.DoorsCount = DoorsCount;

                if (!All.ContainsKey(HouseType))
                    All.Add(HouseType, new Dictionary<RoomTypes, Dictionary<Types, Style>>());

                if (!All[HouseType].ContainsKey(RoomType))
                    All[HouseType].Add(RoomType, new Dictionary<Types, Style>());

                All[HouseType][RoomType].Add(Type, this);
            }

            public static void LoadAll()
            {
                new Style(HouseBase.Types.House, RoomTypes.Two, Types.First, new Vector3(67.955511f, 70.03592f, -9f), 272f, 6, 5);
                new Style(HouseBase.Types.House, RoomTypes.Two, Types.Second, new Vector3(67.955511f, 70.03592f, -19f), 272f, 6, 5);

                new Style(HouseBase.Types.Apartments, RoomTypes.Two, Types.First, new Vector3(67.955511f, 70.03592f, -9f), 272f, 6, 5);
                new Style(HouseBase.Types.Apartments, RoomTypes.Two, Types.Second, new Vector3(67.955511f, 70.03592f, -19f), 272f, 6, 5);

                Game.Items.Container.AllSIDs.Add("h_locker", new Items.Container.Data(50, 150f, Items.Container.AllowedItemTypes.All, Items.Container.ContainerTypes.Locker));
                Game.Items.Container.AllSIDs.Add("h_wardrobe", new Items.Container.Data(50, 80f, Items.Container.AllowedItemTypes.Wardrobe, Items.Container.ContainerTypes.Wardrobe));
                Game.Items.Container.AllSIDs.Add("h_fridge", new Items.Container.Data(50, 100f, Items.Container.AllowedItemTypes.Fridge, Items.Container.ContainerTypes.Fridge));

                Game.Items.Container.AllSIDs.Add("a_locker", new Items.Container.Data(50, 150f, Items.Container.AllowedItemTypes.All, Items.Container.ContainerTypes.Locker));
                Game.Items.Container.AllSIDs.Add("a_wardrobe", new Items.Container.Data(50, 80f, Items.Container.AllowedItemTypes.Wardrobe, Items.Container.ContainerTypes.Wardrobe));
                Game.Items.Container.AllSIDs.Add("a_fridge", new Items.Container.Data(50, 100f, Items.Container.AllowedItemTypes.Fridge, Items.Container.ContainerTypes.Fridge));
            }
        }

        public class Light
        {
            [JsonProperty(PropertyName = "S")]
            public bool State { get; set; }

            [JsonProperty(PropertyName = "C")]
            public Utils.Colour Colour { get; set; }

            public Light(bool State, Utils.Colour Colour)
            {
                this.State = State;
                this.Colour = Colour;
            }

            public Light() { }
        }

        /// <summary>Типы домов</summary>
        public enum Types
        {
            /// <summary>Дом</summary>
            House = 0,
            /// <summary>Квартира</summary>
            Apartments,
        }

        public enum ClassTypes
        {
            A = 0,
            B,
            C,
            D,

            FA,
            FB,
            FC,
            FD,
        }

        private static Dictionary<ClassTypes, int> Taxes = new Dictionary<ClassTypes, int>()
        {
            { ClassTypes.A, 50 },
            { ClassTypes.B, 75 },
            { ClassTypes.C, 90 },
            { ClassTypes.D, 100 },

            { ClassTypes.FA, 50 },
            { ClassTypes.FB, 75 },
            { ClassTypes.FC, 90 },
            { ClassTypes.FD, 100 },
        };

        public static int GetTax(ClassTypes cType) => Taxes[cType];

        public static ClassTypes GetClass(HouseBase house)
        {
            if (house.Type == Types.House)
            {
                if (house.Price <= 100_000)
                    return ClassTypes.A;

                if (house.Price <= 500_000)
                    return ClassTypes.B;

                if (house.Price <= 1_000_000)
                    return ClassTypes.C;

                return ClassTypes.D;
            }
            else
            {
                if (house.Price <= 100_000)
                    return ClassTypes.FA;

                if (house.Price <= 500_000)
                    return ClassTypes.FB;

                if (house.Price <= 1_000_000)
                    return ClassTypes.FC;

                return ClassTypes.FD;
            }
        }

        /// <summary>ID дома</summary>
        public uint Id { get; set; }

        /// <summary>Тип дома</summary>
        public Types Type { get; set; }

        public Style.RoomTypes RoomType { get; set; }

        /// <summary>Тип планировки</summary>
        public Style StyleData { get; set; }

        /// <summary>Владелец</summary>
        public PlayerData.PlayerInfo Owner { get; set; }

        /// <summary>Список сожителей</summary>
        /// <remarks>0 - свет, 1 - двери, 2 - шкаф, 3 - гардероб, 4 - холодильник</remarks>
        public Dictionary<PlayerData.PlayerInfo, bool[]> Settlers { get; set; }

        /// <summary>Баланс дома</summary>
        public int Balance { get; set; }

        /// <summary>Заблокированы ли двери?</summary>
        public bool IsLocked { get; set; }

        public bool ContainersLocked { get; set; }

        /// <summary>Налог</summary>
        public int Tax => GetTax(Class);

        public Utils.Vector4 PositionParams { get; set; }

        public uint Locker { get; set; }

        public uint Wardrobe { get; set; }

        public uint Fridge { get; set; }

        /// <summary>Список FID мебели в доме</summary>
        public List<Furniture> Furniture { get; set; }

        public Light[] LightsStates { get; set; }

        public bool[] DoorsStates { get; set; }

        /// <summary>Стандартная цена дома</summary>
        public int Price { get; set; }

        public uint Dimension { get; set; }

        public ClassTypes Class { get; set; }

        public HouseBase(uint ID, Utils.Vector4 PositionParams, Types Type, Style.RoomTypes RoomType)
        {
            this.Type = Type;

            this.RoomType = RoomType;

            this.Id = ID;

            this.PositionParams = PositionParams;

            this.Class = GetClass(this);
        }

        public void TriggerEventForHouseOwners(string eventName, params object[] args)
        {
            var players = Settlers.Keys.Where(x => x?.PlayerData?.Player.Dimension == Dimension).Select(x => x.PlayerData.Player).ToList();

            if (players.Count > 0)
            {
                if (Owner?.PlayerData?.Player.Dimension == Dimension)
                    players.Add(Owner.PlayerData.Player);

                NAPI.ClientEvent.TriggerClientEventToPlayers(players.ToArray(), eventName, args);
            }
            else
            {
                if (Owner?.PlayerData?.Player.Dimension == Dimension)
                    Owner.PlayerData.Player.TriggerEvent(eventName, args);
            }
        }

        public virtual void UpdateOwner(PlayerData.PlayerInfo pInfo)
        {
            Owner = pInfo;
        }

        public string ToClientJson()
        {
            var data = new JObject();

            data.Add("I", Id);
            data.Add("T", (int)Type);
            data.Add("S", (int)StyleData.Type);
            data.Add("Dim", Dimension);

            data.Add("LI", Locker);
            data.Add("WI", Wardrobe);
            data.Add("FI", Fridge);

            data.Add("DS", DoorsStates.SerializeToJson());
            data.Add("LS", LightsStates.SerializeToJson());
            data.Add("F", Furniture.SerializeToJson());

            return data.SerializeToJson();
        }

        public void SetPlayersInside(params Player[] players)
        {
            NAPI.ClientEvent.TriggerClientEventToPlayers(players, "House::Enter", ToClientJson());
        }

        public void SetPlayersOutside(params Player[] players)
        {

        }

        public abstract bool IsEntityNearEnter(Entity entity);

        public abstract void SetPlayerInside(Player player);

        public abstract void ChangeOwner(PlayerData.PlayerInfo pInfo);

        public void SettlePlayer(PlayerData.PlayerInfo pInfo, bool state, PlayerData pDataInit = null)
        {
            if (state)
            {
                if (Settlers.ContainsKey(pInfo))
                    return;

                Settlers.Add(pInfo, new bool[5]);

                TriggerEventForHouseOwners("HouseMenu::SettlerUpd", $"{pInfo.CID}_{pInfo.Name}_{pInfo.Surname}");

                if (pInfo.PlayerData != null)
                {
                    pInfo.PlayerData.SettledHouseBase = this;

                    if (pDataInit != null)
                        pInfo.PlayerData.Player.TriggerEvent("Player::SettledHB", (int)Type, Id, true, pDataInit.Player);
                    else
                        pInfo.PlayerData.Player.TriggerEvent("Player::SettledHB", (int)Type, Id, true);
                }
            }
            else
            {
                Settlers.Remove(pInfo);

                if (pDataInit?.Info == pInfo)
                {
                    pDataInit.Player.CloseAll(true);
                }

                TriggerEventForHouseOwners("HouseMenu::SettlerUpd", pInfo.CID);

                if (pInfo.PlayerData != null)
                {
                    pInfo.PlayerData.SettledHouseBase = null;

                    if (pDataInit != null)
                        pInfo.PlayerData.Player.TriggerEvent("Player::SettledHB", (int)Type, Id, false, pDataInit.Player);
                    else
                        pInfo.PlayerData.Player.TriggerEvent("Player::SettledHB", (int)Type, Id, false);
                }
            }

            MySQL.HouseUpdateSettlers(this);
        }
    }

    public class House : HouseBase
    {
        /// <summary>Словарь всех домов</summary>
        public static Dictionary<uint, House> All { get; private set; } = new Dictionary<uint, House>();

        public Utils.Vector4 GarageOutside { get; set; }

        /// <summary>Тип гаража</summary>
        public Garage.Style GarageData { get; private set; }

        public House(uint HID, Utils.Vector4 PositionParams, Style.RoomTypes RoomType, int Price, Garage.Types? GarageType = null, Utils.Vector4 GarageOutside = null) : base(HID, PositionParams, Types.House, RoomType)
        {
            this.Price = Price;
            this.Dimension = (uint)(HID + Utils.HouseDimBase);

            this.GarageOutside = GarageOutside;

            if (GarageType is Garage.Types gType)
            {
                this.GarageData = Garage.Style.Get(gType, 0);
            }

            All.Add(HID, this);
        }

        /// <summary>Метод для загрузки всех домов</summary>
        /// <returns>Кол-во загруженных домов</returns>
        public static int LoadAll()
        {
            new House(1, new Utils.Vector4(1724.771f, 4642.161f, 43.8755f, 115f), Style.RoomTypes.Two, 50000, Garage.Types.Two, new Utils.Vector4(1723.976f, 4630.187f, 42.84944f, 116.6f));

            var lines = new List<string>();

            foreach (var x in All.Values)
            {
                MySQL.LoadHouse(x);

                lines.Add($"new House({x.Id}, {x.PositionParams.Position.ToCSharpStr()}, Sync.House.Style.RoomTypes.{x.RoomType}, {(x.GarageData == null ? "null" : $"Garage.Types.{x.GarageData.Type}")}, {(x.GarageOutside == null ? "null" : x.GarageOutside.Position.ToCSharpStr())}, {x.Price}, HouseBase.ClassTypes.{x.Class}, {x.Tax});");
            }

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "HOUSES_TO_REPLACE", lines);

            return All.Count;
        }

        public static House Get(uint id) => All.GetValueOrDefault(id);

        public override void UpdateOwner(PlayerData.PlayerInfo pInfo)
        {
            base.UpdateOwner(pInfo);

            Sync.World.SetSharedData($"House::{Id}::OName", pInfo == null ? null : $"{pInfo.Name} {pInfo.Surname} [#{pInfo.CID}]");
        }

        public override bool IsEntityNearEnter(Entity entity) => entity.Dimension == Utils.Dimensions.Main && entity.Position.DistanceIgnoreZ(PositionParams.Position) <= Settings.ENTITY_INTERACTION_MAX_DISTANCE;

        public bool IsEntityNearVehicleEnter(Entity entity) => GarageOutside == null ? false : entity.Dimension == Utils.Dimensions.Main && entity.Position.DistanceIgnoreZ(GarageOutside.Position) <= Settings.ENTITY_INTERACTION_MAX_DISTANCE;

        public override void SetPlayerInside(Player player)
        {
            var sData = StyleData;

            player.Teleport(sData.Position, false, Dimension, sData.Heading, true);

            player.TriggerEvent("House::Enter", ToClientJson());
        }

        public override void ChangeOwner(PlayerData.PlayerInfo pInfo)
        {
            if (Owner != null)
            {
                Owner.PlayerData?.RemoveHouseProperty(this);
            }

            if (pInfo != null)
            {
                pInfo.PlayerData?.AddHouseProperty(this);
            }

            UpdateOwner(pInfo);

            MySQL.HouseUpdateOwner(this);
        }

        public void SetVehicleToGarage(VehicleData vData, int slot)
        {
            vData.Vehicle.TriggerEventOccupants("House::Enter", ToClientJson());

            vData.EngineOn = false;

            var vPos = GarageData.VehiclePositions[slot];

            vData.Vehicle.Teleport(vPos.Position, Dimension, vPos.RotationZ, true, Additional.AntiCheat.VehicleTeleportTypes.All);

            vData.SetFreezePosition(vPos.Position);
            vData.IsInvincible = true;

            vData.Info.LastData.GarageSlot = slot;
        }

        public void SetVehicleToGarageOnSpawn(VehicleData vData)
        {
            var vPos = GarageData.VehiclePositions[vData.LastData.GarageSlot];

            vData.Vehicle.Position = vPos.Position;
            vData.Vehicle.SetHeading(vPos.RotationZ);

            vData.IsFrozen = true;
            vData.IsInvincible = true;
        }

        public IEnumerable<VehicleData.VehicleInfo> GetVehiclesInGarage()
        {
            if (GarageData == null || Owner == null)
                return null;

            return Owner.OwnedVehicles.Where(x => x.LastData.GarageSlot >= 0 && (x.VehicleData?.Vehicle.Dimension ?? x.LastData.Dimension) == Dimension);
        }
    }

    public class Apartments : HouseBase
    {
        public class ApartmentsRoot : IDimensionable
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

            public static void LoadAll()
            {
                new ApartmentsRoot(Types.Cheap1, new Utils.Vector4(-150.497f, 6416.68f, 31.9159f, 45f), new Utils.Vector4(696.9186f, 1299.008f, -186.5668f, 92f), 10, new Utils.Vector4(697.21f, 1302.987f, -186.57f, 181.7173f), 3.7f, 1);

                var lines = new List<string>();

                foreach (var x in All.Values)
                {
                    lines.Add($"new ApartmentsRoot(ApartmentsRoot.Types.{x.Type.ToString()}, {x.EnterParams.Position.ToCSharpStr()}, {x.ExitParams.Position.ToCSharpStr()}, {x.FloorsAmount}, {x.FloorPosition.Position.ToCSharpStr()}, {x.FloorDistZ}f, {x.StartFloor});");
                }

                Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "AROOTS_TO_REPLACE", lines);
            }

            public static ApartmentsRoot Get(Types type) => All.GetValueOrDefault(type);

            public Vector3 GetFloorPosition(int floor)
            {
                if (floor < StartFloor || floor > FloorsAmount)
                    return null;

                return new Vector3(FloorPosition.X, FloorPosition.Y, FloorPosition.Z + (floor - StartFloor) * FloorDistZ);
            }

            public void SetPlayersInside(params Player[] players)
            {

            }

            public void SetPlayersOutside(params Player[] players)
            {

            }
        }

        /// <summary>Словарь всех квартир</summary>
        public static Dictionary<uint, Apartments> All { get; set; } = new Dictionary<uint, Apartments>();

        public static Dictionary<int, float> TaxCoeffs { get; set; }

        /// <summary>Данные многоквартирного дома</summary>
        public ApartmentsRoot Root { get; private set; }

        /// <summary>Метод для загрузки всех квартир</summary>
        /// <returns>Кол-во загруженных квартир</returns>
        public static int LoadAll()
        {
            ApartmentsRoot.LoadAll();

            new Apartments(1, new Utils.Vector4(692.4949f, 1293.174f, -186.5667f, 273.5f), ApartmentsRoot.Types.Cheap1, Style.RoomTypes.Two, 50000);

            var lines = new List<string>();

            foreach (var x in All.Values)
            {
                MySQL.LoadApartments(x);

                lines.Add($"new Apartments({x.Id}, {x.PositionParams.Position.ToCSharpStr()}, ApartmentsRoot.Types.{x.Root.Type.ToString()}, Sync.House.Style.RoomTypes.{x.RoomType.ToString()}, {x.Price}, HouseBase.ClassTypes.{x.Class}, {x.Tax});");
            }

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "APARTMENTS_TO_REPLACE", lines);

            return All.Count;
        }
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

        public override void SetPlayerInside(Player player)
        {
            var sData = StyleData;

            player.Teleport(sData.Position, false, Dimension, sData.Heading, true);

            player.TriggerEvent("House::Enter", ToClientJson());

            player.TriggerEvent("ARoot::Exit");
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

            UpdateOwner(pInfo);

            MySQL.HouseUpdateOwner(this);
        }
    }

    public class Garage : IDimensionable
    {
        public static Dictionary<uint, Garage> All { get; set; } = new Dictionary<uint, Garage>();

        public enum Types
        {
            Two = 2,
            Six = 6,
            Ten = 10,
        }

        public class Style
        {
            public static Dictionary<Types, Dictionary<byte, Style>> All { get; set; } = new Dictionary<Types, Dictionary<byte, Style>>();

            public Types Type { get; set; }

            public byte Variation { get; set; }

            public List<Utils.Vector4> VehiclePositions { get; set; }

            public Utils.Vector4 EnterPosition { get; set; }

            public int MaxVehicles => VehiclePositions.Count;

            public Style(Types Type, byte Variation, Utils.Vector4 EnterPosition, List<Utils.Vector4> VehiclePositions)
            {
                this.Type = Type;

                this.Variation = Variation;

                this.EnterPosition = EnterPosition;

                this.VehiclePositions = VehiclePositions;

                if (!All.ContainsKey(Type))
                    All.Add(Type, new Dictionary<byte, Style>() { { Variation, this } });
                else
                    All[Type].Add(Variation, this);
            }

            public static void LoadAll()
            {
                new Style(Types.Two, 0, new Utils.Vector4(179.0708f, -1005.729f, -98.99996f, 80.5f), new List<Utils.Vector4>()
                {
                    new Utils.Vector4(171.2562f, -1004.826f, -99.38025f, 182f),
                    new Utils.Vector4(174.7562f, -1004.826f, -99.38025f, 182f),
                });

                new Style(Types.Six, 0, new Utils.Vector4(207.0894f, -998.9854f, -98.99996f, 90f), new List<Utils.Vector4>()
                {
                    new Utils.Vector4(192.987f, -1004.135f, -99.38025f, 182f),
                    new Utils.Vector4(196.487f, -1004.135f, -99.38025f, 182f),

                    new Utils.Vector4(199.987f, -1004.135f, -99.38025f, 182f),
                    new Utils.Vector4(203.487f, -1004.135f, -99.38025f, 182f),

                    new Utils.Vector4(192.987f, -997.135f, -99.38025f, 182f),
                    new Utils.Vector4(196.487f, -997.135f, -99.38025f, 182f),
                });

                new Style(Types.Ten, 0, new Utils.Vector4(238.0103f, -1004.861f, -98.99996f, 78f), new List<Utils.Vector4>()
                {
                    new Utils.Vector4(233.536f, -1001.264f, -99.38025f, 125f),
                    new Utils.Vector4(233.536f, -996.764f, -99.38025f, 125f),
                    new Utils.Vector4(233.536f, -992.264f, -99.38025f, 125f),
                    new Utils.Vector4(233.536f, -987.764f, -99.38025f, 125f),
                    new Utils.Vector4(233.536f, -983.264f, -99.38025f, 125f),

                    new Utils.Vector4(223.536f, -1001.264f, -99.38025f, 250f),
                    new Utils.Vector4(223.536f, -996.764f, -99.38025f, 250f),
                    new Utils.Vector4(223.536f, -992.264f, -99.38025f, 250f),
                    new Utils.Vector4(223.536f, -987.764f, -99.38025f, 250f),
                    new Utils.Vector4(223.536f, -983.264f, -99.38025f, 250f),
                });
            }

            public static Style Get(Types type, byte variation) => All.GetValueOrDefault(type).GetValueOrDefault(variation);
        }

        public class GarageRoot
        {
            private static Dictionary<Types, GarageRoot> All { get; set; } = new Dictionary<Types, GarageRoot>();

            public enum Types
            {
                Complex1 = 0,
            }

            public Types Type { get; set; }

            public Utils.Vector4 EnterPosition { get; set; }

            public List<Utils.Vector4> VehicleExitPositions { get; set; }

            public Vector3 EnterPositionVehicle { get; set; }

            private int LastExitUsed { get; set; }

            public GarageRoot(Types Type, Utils.Vector4 EnterPosition, Vector3 EnterPositionVehicle, List<Utils.Vector4> VehicleExitPositions)
            {
                this.Type = Type;

                this.EnterPosition = EnterPosition;
                this.EnterPositionVehicle = EnterPositionVehicle;
                this.VehicleExitPositions = VehicleExitPositions;

                All.Add(Type, this);
            }

            public static void LoadAll()
            {
                new GarageRoot(Types.Complex1, new Utils.Vector4(-1167.71f, -700.1437f, 21.89413f, 295.6788f), new Vector3(-1204.965f, -715.033f, 21.62106f), new List<Utils.Vector4>()
                {
                    new Utils.Vector4(-1191.297f, -735.8434f, 20.17742f, 307.4758f),
                    new Utils.Vector4(-1189.372f, -738.7911f, 19.98976f, 307.4758f),
                    new Utils.Vector4(-1186.283f, -742.4839f, 19.73591f, 307.4758f),
                    new Utils.Vector4(-1184.012f, -745.5002f, 19.54056f, 307.4758f),
                });

                var lines = new List<string>();

                foreach (var x in All.Values)
                {
                    lines.Add($"new GarageRoot(GarageRoot.Types.{x.Type.ToString()}, {x.EnterPosition.Position.ToCSharpStr()}, {x.EnterPositionVehicle.ToCSharpStr()});");
                }

                Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "GROOTS_TO_REPLACE", lines);
            }

            public Utils.Vector4 GetNextVehicleExit()
            {
                var nextId = LastExitUsed + 1;

                if (nextId >= VehicleExitPositions.Count)
                    nextId = 0;

                LastExitUsed = nextId;

                return VehicleExitPositions[nextId];
            }

            public bool IsEntityNearEnter(Entity entity) => entity.Dimension == Utils.Dimensions.Main && entity.Position.DistanceIgnoreZ(EnterPosition.Position) <= Settings.ENTITY_INTERACTION_MAX_DISTANCE;

            public bool IsEntityNearVehicleEnter(Entity entity) => entity.Dimension == Utils.Dimensions.Main && entity.Position.DistanceIgnoreZ(EnterPositionVehicle) <= Settings.ENTITY_INTERACTION_MAX_DISTANCE;

            public static GarageRoot Get(Types type) => All.GetValueOrDefault(type);
        }

        public PlayerData.PlayerInfo Owner { get; set; }

        public Style StyleData { get; set; }

        public uint Id { get; set; }

        public uint Dimension { get; set; }

        public GarageRoot Root { get; set; }

        public int Price { get; set; }

        public ClassTypes ClassType { get; set; }

        public int Tax => GetTax(ClassType);

        public int Balance { get; set; }

        public bool IsLocked { get; set; }

        public byte Variation { get; set; }

        public enum ClassTypes
        {
            GA = 0,
            GB,
            GC,
            GD,
        }

        private static Dictionary<ClassTypes, int> Taxes = new Dictionary<ClassTypes, int>()
        {
            { ClassTypes.GA, 50 },
            { ClassTypes.GB, 75 },
            { ClassTypes.GC, 90 },
            { ClassTypes.GD, 100 },
        };

        public static int GetTax(ClassTypes cType) => Taxes[cType];

        public static ClassTypes GetClass(Garage garage)
        {
            if (garage.Price <= 50_000)
                return ClassTypes.GA;

            if (garage.Price <= 150_000)
                return ClassTypes.GB;

            if (garage.Price <= 500_000)
                return ClassTypes.GC;

            return ClassTypes.GD;
        }

        public Garage(uint Id, GarageRoot.Types RootType, Types Type, byte Variation, int Price)
        {
            this.Id = Id;

            this.Root = GarageRoot.Get(RootType);

            this.StyleData = Style.Get(Type, Variation);

            this.Price = Price;

            this.ClassType = GetClass(this);

            this.Dimension = (uint)(Id + Utils.GarageDimBase);

            All.Add(Id, this);
        }

        public static void LoadAll()
        {
            GarageRoot.LoadAll();

            new Garage(1, GarageRoot.Types.Complex1, Types.Two, 0, 25_000);

            var lines = new List<string>();

            foreach (var x in All.Values)
            {
                MySQL.LoadGarage(x);

                lines.Add($"new Garage({x.Id}, GarageRoot.Types.{x.Root.Type.ToString()}, Garage.Types.{x.StyleData.Type.ToString()}, {x.Variation}, Garage.ClassTypes.{x.ClassType.ToString()}, {x.Tax}, {x.Price});");
            }

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "GARAGES_TO_REPLACE", lines);
        }

        public static Garage Get(uint id) => All.GetValueOrDefault(id);

        public void UpdateOwner(PlayerData.PlayerInfo pInfo)
        {
            Owner = pInfo;

            Sync.World.SetSharedData($"Garages::{Id}::OName", pInfo == null ? null : $"{pInfo.Name} {pInfo.Surname} [#{pInfo.CID}]");
        }

        public void ChangeOwner(PlayerData.PlayerInfo pInfo)
        {
            if (Owner != null)
            {
                Owner.PlayerData?.RemoveGarageProperty(this);
            }

            if (pInfo != null)
            {
                pInfo.PlayerData?.AddGarageProperty(this);
            }

            UpdateOwner(pInfo);

            MySQL.GarageUpdateOwner(this);
        }

        public void SetVehicleToGarage(VehicleData vData, int slot)
        {
            vData.Vehicle.TriggerEventOccupants("Garage::Enter", Id);

            vData.EngineOn = false;

            var vPos = StyleData.VehiclePositions[slot];

            vData.Vehicle.Teleport(vPos.Position, Dimension, vPos.RotationZ, true, Additional.AntiCheat.VehicleTeleportTypes.All);

            vData.SetFreezePosition(vPos.Position);
            vData.IsInvincible = true;

            vData.Info.LastData.GarageSlot = slot;
        }

        public void SetVehicleToGarageOnSpawn(VehicleData vData)
        {
            var vPos = StyleData.VehiclePositions[vData.LastData.GarageSlot];

            vData.Vehicle.Position = vPos.Position;
            vData.Vehicle.SetHeading(vPos.RotationZ);

            vData.IsFrozen = true;
            vData.IsInvincible = true;
        }

        public IEnumerable<VehicleData.VehicleInfo> GetVehiclesInGarage()
        {
            if (Owner == null)
                return null;

            return Owner.OwnedVehicles.Where(x => x.LastData.GarageSlot >= 0 && (x.VehicleData?.Vehicle.Dimension ?? x.LastData.Dimension) == Dimension);
        }

        public void SetPlayersInside(params Player[] players)
        {

        }

        public void SetPlayersOutside(params Player[] players)
        {

        }
    }
}
