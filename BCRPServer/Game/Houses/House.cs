using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Houses
{
    public abstract class HouseBase
    {
        public static Color DefaultLightColour = new Color(255, 187, 96);

        public class Style
        {
            /// <summary>Типы планировки</summary>
            public enum Types
            {
                Room2_1 = 0,
                Room2_2,
            }

            public Types Type { get; private set; }
            public Vector3 Position { get; private set; }
            public float Heading { get; private set; }

            public int LightsCount { get; private set; }
            public int DoorsCount { get; private set; }

            /// <summary>Словарь планировок</summary>
            private static Dictionary<Style.Types, Style> All = new Dictionary<Style.Types, Style>()
            {
                { Types.Room2_1, new Style(Types.Room2_1, new Vector3(67.955511f, 70.03592f, -9f), 272f, 6, 5) },
                { Types.Room2_2, new Style(Types.Room2_2, new Vector3(67.955511f, 70.03592f, -19f), 272f, 6, 5) },
            };

            public static Style Get(Types type) => All[type];

            public Style(Types Type, Vector3 Position, float Heading, int LightsCount = 0, int DoorsCount = 0)
            {
                this.Type = Type;

                this.Position = Position;
                this.Heading = Heading;

                this.LightsCount = LightsCount;
                this.DoorsCount = DoorsCount;
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
        /// <summary>Тип планировки</summary>
        public Style.Types StyleType { get; set; }
        /// <summary>CID владельца</summary>
        /// <value>0 - если нет владельца</value>
        public uint Owner { get; set; }
        /// <summary>Список CID сожителей</summary>
        public List<uint> Settlers { get; set; }
        /// <summary>Баланс дома</summary>
        public int Balance { get; set; }
        /// <summary>Заблокированы ли двери?</summary>
        public bool IsLocked { get; set; }
        public bool ContainersLocked { get; set; }

        /// <summary>Дневной налог</summary>
        public int DayTax { get; set; }
        /// <summary>Ночной налог</summary>
        public int NightTax { get; set; }

        /// <summary>Глобальная позиция</summary>
        public Vector3 GlobalPosition { get; set; }
        /// <summary>Информация о планировке</summary>
        public Style StyleData { get => Style.Get(StyleType); }
        /// <summary>Поворот игрока при выходе из дома</summary>
        public float ExitHeading { get; set; }

        /// <summary>Тип гаража</summary>
        public Garage.Types GarageType { get; set; }
        /// <summary>Список VID транспорта в гараже</summary>
        public List<int> Vehicles { get; set; }

        public uint Locker { get; set; }
        public uint Wardrobe { get; set; }
        public uint Fridge { get; set; }

        /// <summary>Список FID мебели в доме</summary>
        public List<uint> Furniture { get; set; }

        public (Color Colour, bool State)[] LightsStates { get; set; }
        public bool[] DoorsStates { get; set; }

        /// <summary>Стандартная цена дома</summary>
        public int Price { get; set; }

        /// <summary>Сущность колшейпа входа</summary>
        public Additional.ExtraColshape EnterColshape { get; set; }
        /// <summary>Сущность маркера входа</summary>
        public Marker EnterMarker { get; set; }
        /// <summary>Сущность текста входа</summary>
        public TextLabel EnterText { get; set; }

        public uint Dimension { get; set; }

        public HouseBase(uint ID, Vector3 GlobalPosition, float ExitHeading, Style.Types StyleType)
        {
            this.ID = ID;

            this.GlobalPosition = GlobalPosition;
            this.StyleType = StyleType;

            this.ExitHeading = ExitHeading;
        }
    }

    public class House : HouseBase
    {
        /// <summary>Словарь всех домов</summary>
        public static Dictionary<int, House> All;

        /// <summary></summary>
        private static Dictionary<int, float> TaxCoeffs { get; set; }

        public Garage.Outside GarageOutside { get; set; }

        public House(uint HID, Vector3 GlobalPosition, float ExitHeading, Style.Types StyleType, int Price, Garage.Outside GarageOutside = null) : base(HID, GlobalPosition, ExitHeading, StyleType)
        {
            this.Price = Price;
            this.Dimension = (uint)(HID + Utils.HouseDimBase);

            this.GarageOutside = GarageOutside;

            EnterColshape = new Additional.ExtraColshape.Cylinder(GlobalPosition, 1f, 2f, false, new Color(255, 0, 0, 255), Utils.Dimensions.Main);

            EnterColshape.ActionType = Additional.ExtraColshape.ActionTypes.HouseEnter;
            EnterColshape.InteractionType = Additional.ExtraColshape.InteractionTypes.HouseEnter;

            EnterColshape.Data = ID;

            //EnterMarker = NAPI.Marker.CreateMarker(MarkerType.VerticalCylinder, GlobalPosition, Utils.ZeroVector, Utils.ZeroVector, 0.75f, Utils.YellowColor, false, Utils.Dimensions.Main);
            EnterText = NAPI.TextLabel.CreateTextLabel(string.Format(Locale.Houses.HouseInfo, HID), new Vector3(GlobalPosition.X, GlobalPosition.Y, GlobalPosition.Z + 0.5f), 5f, 0.1f, 0, Utils.WhiteColor, true, Utils.Dimensions.Main);

            var sdata = StyleData;
        }

        /// <summary>Метод для загрузки всех домов</summary>
        /// <returns>Кол-во загруженных домов</returns>
        public static int LoadAll()
        {
            if (All != null)
                return All.Count;

            Game.Items.Container.AllSIDs.Add("h_locker", new Items.Container.Data(50, 150f, Items.Container.AllowedItemTypes.All, Items.Container.ContainerTypes.Locker));
            Game.Items.Container.AllSIDs.Add("h_wardrobe", new Items.Container.Data(50, 150f, Items.Container.AllowedItemTypes.Wardrobe, Items.Container.ContainerTypes.Wardrobe));
            Game.Items.Container.AllSIDs.Add("h_fridge", new Items.Container.Data(50, 150f, Items.Container.AllowedItemTypes.All, Items.Container.ContainerTypes.Fridge));

            TaxCoeffs = new Dictionary<int, float>()
            {
                { 0, 0.00075f }, { 100000, 0.0001f }, { 500000, 0.00005f },
            };

            All = new Dictionary<int, House>()
            {
                { 1, new House(1, new Vector3(1724.771f, 4642.161f, 42.8755f), 115f, Style.Types.Room2_2, 50000, null) },
            };

            for (int i = 1; i < All.Count + 1; i++)
                All[i] = MySQL.GetHouse(All[i]);

            return All.Count;
        }

        public static House Get(int id) => All.ContainsKey(id) ? All[id] : null;
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

            public ColShape EnterColshape { get; set; }
            public Marker EnterMarker { get; set; }
            public TextLabel EnterText { get; set; }

            public ColShape ExitColshape { get; set; }
            public Marker ExitMarker { get; set; }
            public TextLabel ExitText { get; set; }

            public Blip Blip { get; set; }

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

                EnterColshape = NAPI.ColShape.CreateCylinderColShape(PositionEnter, 0.75f, 2f, Utils.Dimensions.Main);
                EnterMarker = NAPI.Marker.CreateMarker(MarkerType.VerticalCylinder, PositionEnter, Utils.ZeroVector, Utils.ZeroVector, 0.75f, Utils.YellowColor, false, Utils.Dimensions.Main);
                EnterText = NAPI.TextLabel.CreateTextLabel(string.Format(Locale.Houses.ApartmentsRootInfo, Name), new Vector3(PositionEnter.X, PositionEnter.Y, PositionEnter.Z + 0.5f), 5f, 0.1f, 0, Utils.WhiteColor, true, Utils.Dimensions.Main);

                ExitColshape = NAPI.ColShape.CreateCylinderColShape(PositionEnter, 0.75f, 2f, this.Dimension);
                ExitMarker = NAPI.Marker.CreateMarker(MarkerType.VerticalCylinder, PositionEnter, Utils.ZeroVector, Utils.ZeroVector, 0.75f, Utils.RedColor, false, this.Dimension);
                ExitText = NAPI.TextLabel.CreateTextLabel(Locale.Houses.Exit, new Vector3(PositionEnter.X, PositionEnter.Y, PositionEnter.Z + 0.5f), 5f, 0.1f, 0, Utils.WhiteColor, true, Utils.Dimensions.Main);

                Blip = NAPI.Blip.CreateBlip(475, PositionEnter, 1f, 25, string.Format(Locale.Houses.ApartmentsRootBlip, Name), 255, 0, true);

                EnterColshape.SetSharedData("ApartmentsRootEnter", true);
                EnterColshape.SetSharedData("ID", (int)Type);

                ExitColshape.SetSharedData("ApartmentsRootExit", true);
                ExitColshape.SetSharedData("ID", (int)Type);
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
        public Apartments(uint HID, Vector3 GlobalPosition, float ExitHeading, ApartmentsRoot.Types RootType, Style.Types StyleType, int Price) : base(HID, GlobalPosition, ExitHeading, StyleType)
        {
            this.RootType = RootType;

            this.Price = (int)Math.Floor(Price * Roots[RootType].PriceCoeff);

            this.Dimension = (uint)(HID + Utils.ApartmentsDimBase);

            var root = Root;

            EnterColshape = new Additional.ExtraColshape.Cylinder(GlobalPosition, 0.75f, 2f, false, new Color(255, 0, 0, 255), Root.Dimension);

            EnterColshape.ActionType = Additional.ExtraColshape.ActionTypes.HouseEnter;
            EnterColshape.InteractionType = Additional.ExtraColshape.InteractionTypes.HouseEnter;

            //EnterMarker = NAPI.Marker.CreateMarker(MarkerType.VerticalCylinder, GlobalPosition, Utils.ZeroVector, Utils.ZeroVector, 0.75f, Utils.YellowColor, false, Root.Dimension);
            EnterText = NAPI.TextLabel.CreateTextLabel(string.Format(Locale.Houses.HouseInfo, HID), new Vector3(GlobalPosition.X, GlobalPosition.Y, GlobalPosition.Z + 0.5f), 5f, 0.1f, 0, Utils.WhiteColor, true, Root.Dimension);

            var sdata = StyleData;
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
