using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Houses
{
    public abstract class HouseBase
    {
        public static Color DefaultLightColour = new Color(255, 187, 96, 255);

        public class SettlerPermissions
        {
            [JsonProperty(PropertyName = "L")]
            public bool Lights { get; set; }

            [JsonProperty(PropertyName = "D")]
            public bool Doors { get; set; }

            [JsonProperty(PropertyName = "C")]
            public bool Closet { get; set; }

            [JsonProperty(PropertyName = "W")]
            public bool Wardrobe { get; set; }

            [JsonProperty(PropertyName = "F")]
            public bool Fridge { get; set; }

            public SettlerPermissions() { }
        }

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

            public static void Load()
            {
                new Style(HouseBase.Types.House, RoomTypes.Two, Types.First, new Vector3(67.955511f, 70.03592f, -9f), 272f, 6, 5);
                new Style(HouseBase.Types.House, RoomTypes.Two, Types.Second, new Vector3(67.955511f, 70.03592f, -19f), 272f, 6, 5);

                Game.Items.Container.AllSIDs.Add("h_locker", new Items.Container.Data(50, 150f, Items.Container.AllowedItemTypes.All, Items.Container.ContainerTypes.Locker));
                Game.Items.Container.AllSIDs.Add("h_wardrobe", new Items.Container.Data(50, 150f, Items.Container.AllowedItemTypes.Wardrobe, Items.Container.ContainerTypes.Wardrobe));
                Game.Items.Container.AllSIDs.Add("h_fridge", new Items.Container.Data(50, 150f, Items.Container.AllowedItemTypes.All, Items.Container.ContainerTypes.Fridge));
            }
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
        public Dictionary<PlayerData.PlayerInfo, SettlerPermissions> Settlers { get; set; }

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

        /// <summary>Тип гаража</summary>
        public Garage.Types? GarageType { get; set; }

        /// <summary>Список транспорта в гараже</summary>
        public List<VehicleData.VehicleInfo> Vehicles { get; set; }

        public uint Locker { get; set; }

        public uint Wardrobe { get; set; }

        public uint Fridge { get; set; }

        /// <summary>Список FID мебели в доме</summary>
        public List<uint> Furniture { get; set; }

        public (Color Colour, bool State)[] LightsStates { get; set; }

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

        public Garage.Outside GarageOutside { get; set; }

        public House(uint HID, Vector3 GlobalPosition, float ExitHeading, Style.RoomTypes RoomType, int Price, Garage.Outside GarageOutside = null) : base(HID, GlobalPosition, ExitHeading, Types.House, RoomType)
        {
            this.Price = Price;
            this.Dimension = (uint)(HID + Utils.HouseDimBase);

            this.GarageOutside = GarageOutside;

            All.Add(HID, this);
        }

        /// <summary>Метод для загрузки всех домов</summary>
        /// <returns>Кол-во загруженных домов</returns>
        public static int LoadAll()
        {
            new House(1, new Vector3(1724.771f, 4642.161f, 42.8755f), 115f, Style.RoomTypes.Two, 50000, null);

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
    }

    public class Apartments : HouseBase
    {
        public class ApartmentsRoot
        {
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

            /// <summary>Коэффициент цены квартиры</summary>
            public float PriceCoeff { get; set; }

            /// <summary>Название многоквартирного дома</summary>
            public string Name { get; set; }

            /// <summary>Измерение многоквартирного дома</summary>
            public uint Dimension { get; set; }

            public ApartmentsRoot(Types Type, Vector3 PositionEnter, float EnterHeading, Vector3 PositionExit, float ExitHeading, string Name, float PriceCoeff = 1f)
            {
                this.Type = Type;

                this.PositionEnter = PositionEnter;
                this.PositionExit = PositionExit;

                this.Name = Name;

                this.PriceCoeff = PriceCoeff;

                this.Dimension = (uint)(Utils.ApartmentsRootDimBase + (int)Type);

                this.EnterHeading = EnterHeading;
                this.ExitHeading = ExitHeading;
            }
        }

        /// <summary>Словарь всех многоквартирных домов</summary>
        public static Dictionary<ApartmentsRoot.Types, ApartmentsRoot> Roots;
        /// <summary>Словарь всех квартир</summary>
        public static Dictionary<int, Apartments> All;

        public static Dictionary<int, float> TaxCoeffs { get; set; }

        /// <summary>Тип многоквартирного дома</summary>
        public ApartmentsRoot.Types RootType { get; set; }
        /// <summary>Данные многоквартирного дома</summary>
        public ApartmentsRoot Root { get => Roots[RootType]; }

        /// <summary>Метод для загрузки всех квартир</summary>
        /// <returns>Кол-во загруженных квартир</returns>
        public static int LoadAll()
        {
            if (All != null)
                return All.Count;

            Roots = new Dictionary<ApartmentsRoot.Types, ApartmentsRoot>()
            {
                { ApartmentsRoot.Types.Cheap1, new ApartmentsRoot(ApartmentsRoot.Types.Cheap1, new Vector3(0, 0, 0), 0f, new Vector3(0, 0, 0), 0f, "", 100) }
            };

            All = new Dictionary<int, Apartments>()
            {

            };

            return All.Count;
        }
        public Apartments(uint HID, Vector3 GlobalPosition, float ExitHeading, ApartmentsRoot.Types RootType, Style.RoomTypes RoomType, int Price) : base(HID, GlobalPosition, ExitHeading, Types.Apartments, RoomType)
        {
            this.RootType = RootType;

            this.Price = (int)Math.Floor(Price * Roots[RootType].PriceCoeff);

            this.Dimension = (uint)(HID + Utils.ApartmentsDimBase);
        }
    }

    public class Garage
    {
        public enum Types
        {
            None = 0,
            Two = 2,
            Six = 6,
            Ten = 10,
        }

        public class Outside
        {
            public Vector3 Position { get; set; }
            public float Heading { get; set; }

            public Outside(Vector3 Position, float Heading)
            {
                this.Position = Position;
                this.Heading = Heading;
            }
        }
    }
}
