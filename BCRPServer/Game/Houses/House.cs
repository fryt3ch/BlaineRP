using GTANetworkAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Game.Houses
{
    public abstract class HouseBase
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

                Game.Items.Container.AllSIDs.Add("h_locker", new Items.Container.Data(50, 150f, Items.Container.AllowedItemTypes.All, Items.Container.ContainerTypes.Locker));
                Game.Items.Container.AllSIDs.Add("h_wardrobe", new Items.Container.Data(50, 80f, Items.Container.AllowedItemTypes.Wardrobe, Items.Container.ContainerTypes.Wardrobe));
                Game.Items.Container.AllSIDs.Add("h_fridge", new Items.Container.Data(50, 100f, Items.Container.AllowedItemTypes.Fridge, Items.Container.ContainerTypes.Fridge));
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

        /// <summary>ID дома</summary>
        /// <value>У домов, квартир - разные!</value>
        public uint ID { get; set; }

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
        public int Tax { get; set; }

        /// <summary>Глобальная позиция</summary>
        public Vector3 GlobalPosition { get; set; }

        /// <summary>Поворот игрока при выходе из дома</summary>
        public float ExitHeading { get; set; }

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

        public HouseBase(uint ID, Vector3 GlobalPosition, float ExitHeading, Types Type, Style.RoomTypes RoomType)
        {
            this.Type = Type;

            this.RoomType = RoomType;

            this.ID = ID;

            this.GlobalPosition = GlobalPosition;
            this.ExitHeading = ExitHeading;
        }
    }

    public class House : HouseBase
    {
        /// <summary>Словарь всех домов</summary>
        public static Dictionary<uint, House> All { get; private set; } = new Dictionary<uint, House>();

        public Utils.Vector4 GarageOutside { get; set; }

        /// <summary>Тип гаража</summary>
        public Garage.Style GarageData { get; private set; }

        /// <summary>Список транспорта в гараже</summary>
        public VehicleData.VehicleInfo[] Vehicles { get; set; }

        public House(uint HID, Vector3 GlobalPosition, float ExitHeading, Style.RoomTypes RoomType, int Price, Garage.Types? GarageType = null, Utils.Vector4 GarageOutside = null) : base(HID, GlobalPosition, ExitHeading, Types.House, RoomType)
        {
            this.Price = Price;
            this.Dimension = (uint)(HID + Utils.HouseDimBase);

            this.GarageOutside = GarageOutside;

            this.GarageData = GarageType == null ? null : Garage.Style.Get((Garage.Types)GarageType);

            if (this.GarageData != null)
                this.Vehicles = new VehicleData.VehicleInfo[this.GarageData.MaxVehicles];

            All.Add(HID, this);
        }

        /// <summary>Метод для загрузки всех домов</summary>
        /// <returns>Кол-во загруженных домов</returns>
        public static int LoadAll()
        {
            new House(1, new Vector3(1724.771f, 4642.161f, 42.8755f), 115f, Style.RoomTypes.Two, 50000, Garage.Types.Two, new Utils.Vector4(1723.976f, 4630.187f, 42.84944f, 116.6f));

            foreach (var x in All.Values)
            {
                MySQL.LoadHouse(x);
            }

            return All.Count;
        }

        public static House Get(uint id) => All.GetValueOrDefault(id);

        public void UpdateOwner(PlayerData.PlayerInfo pInfo)
        {
            Owner = pInfo;

            Game.World.SetSharedData($"House::{ID}::OName", pInfo == null ? null : $"{pInfo.Name} {pInfo.Surname} [#{pInfo.CID}]");
        }

        public string ToClientJson()
        {
            var data = new JObject();

            data.Add("I", ID);
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
    }

    public class Apartments : HouseBase
    {
        public class ApartmentsRoot
        {
            public static Dictionary<Types, ApartmentsRoot> All { get; set; } = new Dictionary<Types, ApartmentsRoot>();

            /// <summary>Типы многоквартирных домов</summary>
            public enum Types
            {
                Cheap1 = 0,
            }

            /// <summary>Тип многоквартирного дома</summary>
            public Types Type { get; set; }

            /// <summary>Позиция входа</summary>
            public Vector3 PositionEnter { get; set; }

            /// <summary>Позиция выхода</summary>
            public Vector3 PositionExit { get; set; }

            /// <summary>Поворот игрока при входе</summary>
            public float EnterHeading { get; set; }

            /// <summary>Поворот игрока при выходе</summary>
            public float ExitHeading { get; set; }

            /// <summary>Измерение многоквартирного дома</summary>
            public uint Dimension { get; set; }

            public ApartmentsRoot(Types Type, Vector3 PositionEnter, float EnterHeading, Vector3 PositionExit, float ExitHeading)
            {
                this.Type = Type;

                this.PositionEnter = PositionEnter;
                this.PositionExit = PositionExit;

                this.Dimension = (uint)(Utils.ApartmentsRootDimBase + (int)Type);

                this.EnterHeading = EnterHeading;
                this.ExitHeading = ExitHeading;

                All.Add(Type, this);
            }

            public static void LoadAll()
            {
                new ApartmentsRoot(Types.Cheap1, new Vector3(0, 0, 0), 0f, new Vector3(0, 0, 0), 0f);
            }

            public static ApartmentsRoot Get(Types type) => All.GetValueOrDefault(type);
        }

        /// <summary>Словарь всех квартир</summary>
        public static Dictionary<int, Apartments> All { get; set; } = new Dictionary<int, Apartments>();

        public static Dictionary<int, float> TaxCoeffs { get; set; }

        /// <summary>Данные многоквартирного дома</summary>
        public ApartmentsRoot Root { get; private set; }

        /// <summary>Метод для загрузки всех квартир</summary>
        /// <returns>Кол-во загруженных квартир</returns>
        public static int LoadAll()
        {
            ApartmentsRoot.LoadAll();

            return All.Count;
        }
        public Apartments(uint HID, Vector3 GlobalPosition, float ExitHeading, ApartmentsRoot.Types RootType, Style.RoomTypes RoomType, int Price) : base(HID, GlobalPosition, ExitHeading, Types.Apartments, RoomType)
        {
            this.Root = ApartmentsRoot.Get(RootType);

            this.Price = Price;

            this.Dimension = (uint)(HID + Utils.ApartmentsDimBase);
        }
    }

    public class Garage
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
            public static Dictionary<Types, Style> All { get; set; } = new Dictionary<Types, Style>();

            public Types Type { get; set; }

            public List<(Vector3 Position, float Heading)> VehiclePositions { get; set; }

            public Vector3 EnterPosition { get; set; }

            public float EnterHeading { get; set; }

            public int MaxVehicles => VehiclePositions.Count;

            public Style(Types Type, Vector3 EnterPosition, float EnterHeading, params (Vector3, float)[] VehiclePositions)
            {
                this.EnterPosition = EnterPosition;
                this.EnterHeading = EnterHeading;

                this.VehiclePositions = VehiclePositions.ToList();

                All.Add(Type, this);
            }

            public static void LoadAll()
            {
                new Style(Types.Two, new Vector3(179.0708f, -1005.729f, -98.99996f), 80.5f,
                    (new Vector3(171.2562f, -1004.826f, -99.38025f), 182f),
                    (new Vector3(174.7562f, -1004.826f, -99.38025f), 182f));

                new Style(Types.Six, new Vector3(207.0894f, -998.9854f, -98.99996f), 90f,
                    (new Vector3(192.987f, -1004.135f, -99.38025f), 182f),
                    (new Vector3(196.487f, -1004.135f, -99.38025f), 182f),

                    (new Vector3(199.987f, -1004.135f, -99.38025f), 182f),
                    (new Vector3(203.487f, -1004.135f, -99.38025f), 182f),

                    (new Vector3(192.987f, -997.135f, -99.38025f), 182f),
                    (new Vector3(196.487f, -997.135f, -99.38025f), 182f));

                new Style(Types.Ten, new Vector3(238.0103f, -1004.861f, -98.99996f), 78f,
                    (new Vector3(233.536f, -1001.264f, -99.38025f), 125f),
                    (new Vector3(233.536f, -996.764f, -99.38025f), 125f),
                    (new Vector3(233.536f, -992.264f, -99.38025f), 125f),
                    (new Vector3(233.536f, -987.764f, -99.38025f), 125f),
                    (new Vector3(233.536f, -983.264f, -99.38025f), 125f),

                    (new Vector3(223.536f, -1001.264f, -99.38025f), 250f),
                    (new Vector3(223.536f, -996.764f, -99.38025f), 250f),
                    (new Vector3(223.536f, -992.264f, -99.38025f), 250f),
                    (new Vector3(223.536f, -987.764f, -99.38025f), 250f),
                    (new Vector3(223.536f, -983.264f, -99.38025f), 250f));
            }

            public static Style Get(Types type) => All.GetValueOrDefault(type);
        }

        public class GarageRoot
        {
            private static Dictionary<Types, GarageRoot> All { get; set; } = new Dictionary<Types, GarageRoot>();

            public enum Types
            {
                Complex1 = 0,
            }

            public Types Type { get; set; }

            public Vector3 Position { get; set; }

            public GarageRoot(Types Type, Vector3 Position)
            {
                this.Type = Type;

                this.Position = Position;
            }

            public static void LoadAll()
            {

            }

            public static GarageRoot Get(Types type) => All.GetValueOrDefault(type);
        }

        public Style StyleData { get; set; }

        public uint Id { get; set; }

        public uint Dimension { get; set; }

        public GarageRoot Root { get; set; }

        public int Price { get; set; }

        public Garage(uint Id, GarageRoot.Types RootType, Types Type, int Price)
        {
            this.Id = Id;

            this.Root = GarageRoot.Get(RootType);

            this.StyleData = Style.Get(Type);

            this.Price = Price;

            All.Add(Id, this);
        }

        public static void LoadAll()
        {
            GarageRoot.LoadAll();
        }

        public static Garage Get(uint id) => All.GetValueOrDefault(id);
    }
}
