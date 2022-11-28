using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BCRPClient.Data
{
    public class Locations : Events.Script
    {
        public class Garage
        {
            public static Dictionary<uint, Garage> All = new Dictionary<uint, Garage>();

            public enum Types
            {
                Two = 2,
                Six = 6,
                Ten = 10,
            }

            public class Style
            {
                private static Dictionary<Types, Style> All { get; set; } = new Dictionary<Types, Style>();

                public Types Type { get; set; }

                public Vector3 EnterPosition { get; set; }

                public Style(Types Type, Vector3 EnterPosition)
                {
                    this.EnterPosition = EnterPosition;

                    All.Add(Type, this);
                }

                public static void LoadAll()
                {
                    new Style(Types.Two, new Vector3(179.0708f, -1005.729f, -98.99996f));
                    new Style(Types.Six, new Vector3(207.0894f, -998.9854f, -98.99996f));
                    new Style(Types.Ten, new Vector3(238.0103f, -1004.861f, -98.99996f));
                }

                public static Style Get(Types type) => All.GetValueOrDefault(type);
            }
        }

        public class House
        {
            public static Dictionary<uint, House> All = new Dictionary<uint, House>();

            public uint Id { get; set; }

            public string OwnerName => Sync.World.GetSharedData<string>($"House::{Id}::OName");

            public int Price { get; set; }

            public Vector3 Position { get; set; }

            public Sync.House.Style.RoomTypes RoomType { get; set; }

            public Garage.Types? GarageType { get; set; }

            public Vector3 GaragePosition { get; set; }

            public Additional.ExtraColshape Colshape { get; set; }

            public RAGE.Elements.TextLabel InfoText { get; set; }

            public Blip OwnerBlip { get => Player.LocalPlayer.GetData<Blip>($"House::{Id}::OBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"House::{Id}::OBlip"); else Player.LocalPlayer.SetData($"House::{Id}::OBlip", value); } }

            public Additional.ExtraColshape OwnerGarageColshape { get => Player.LocalPlayer.GetData<Additional.ExtraColshape>($"House::{Id}::OGCS"); set { if (value == null) Player.LocalPlayer.ResetData($"House::{Id}::OGCS"); else Player.LocalPlayer.SetData($"House::{Id}::OGCS", value); } }
           
            public Blip OwnerGarageBlip { get => Player.LocalPlayer.GetData<Blip>($"House::{Id}::OGBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"House::{Id}::OGBlip"); else Player.LocalPlayer.SetData($"House::{Id}::OGBlip", value); } }

            public House(uint Id, Vector3 Position, Sync.House.Style.RoomTypes RoomType, Garage.Types? GarageType, Vector3 GaragePosition, int Price)
            {
                this.Id = Id;

                this.Price = Price;

                this.Position = Position;

                this.RoomType = RoomType;

                this.GarageType = GarageType;

                this.GaragePosition = GaragePosition;

                Colshape = new Additional.Cylinder(Position, 1.5f, 2f, false, new Utils.Colour(255, 0, 0, 125), Settings.MAIN_DIMENSION, null);

                Colshape.InteractionType = Additional.ExtraColshape.InteractionTypes.HouseEnter;
                Colshape.ActionType = Additional.ExtraColshape.ActionTypes.HouseEnter;

                Colshape.Data = this;

                InfoText = new TextLabel(Position, $"Дом #{Id}", new RGBA(255, 255, 255, 255), 25f, 0, false, Settings.MAIN_DIMENSION) { Font = 0 };

                All.Add(Id, this);
            }

            public void UpdateOwnerName(string name)
            {
                if (name == null)
                    name = Locale.Property.NoOwner;
            }

            public void ToggleOwnerBlip(bool state)
            {
                if (state)
                {
                    var oBlip = OwnerBlip;

                    oBlip?.Destroy();

                    OwnerBlip = new Blip(40, Position, $"Дом #{Id}", 1f, 5, 255, 0f, false, 0, 0f, Settings.MAIN_DIMENSION);

                    if (GaragePosition != null)
                    {
                        OwnerGarageColshape = new Additional.Sphere(GaragePosition, 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null);

                        OwnerGarageColshape.Data = this;

                        OwnerGarageColshape.ActionType = Additional.ExtraColshape.ActionTypes.HouseEnter;

                        OwnerGarageBlip = new Blip(1, GaragePosition, "", 1f, 1, 125, 0f, true, 0, 25f, Settings.MAIN_DIMENSION);
                    }
                }
                else
                {
                    var oBlip = OwnerBlip;

                    if (oBlip != null)
                    {
                        oBlip.Destroy();

                        OwnerBlip = null;
                    }

                    var ogCs = OwnerGarageColshape;

                    if (ogCs != null)
                    {
                        ogCs.Delete();

                        OwnerGarageColshape = null;
                    }

                    var ogBlip = OwnerGarageBlip;

                    if (ogBlip != null)
                    {
                        ogBlip.Destroy();

                        OwnerGarageBlip = null;
                    }
                }
            }
        }

        public static class CayoPerico
        {
            public static Blip MainBlip { get; set; }

            public static bool IslandLoaded { get; set; }

            public static Additional.ExtraColshape MainColshape { get; set; }

            private static AsyncTask LoadTask { get; set; }

            public static void Initialize()
            {
                MainColshape = new Additional.Circle(new Vector3(4840.571f, -5174.425f, 0f), 2374f, false, new Utils.Colour(0, 0, 255, 125), uint.MaxValue, null);

                MainColshape.OnEnter += (cancel) =>
                {
                    if (IslandLoaded)
                        return;

                    ToggleCayoPericoIsland(true, true);
                };

                MainColshape.OnExit += (cancel) =>
                {
                    if (!IslandLoaded)
                        return;

                    ToggleCayoPericoIsland(false, true);
                };

                ToggleCayoPericoIsland(false, false);

                MainColshape.Name = "CayoPerico_Loader";

                MainBlip = new Blip(836, new Vector3(4900.16f, -5192.03f, 2.44f), "Cayo Perico", 1.1f, 49, 255, 0f, true, 0, 0f, uint.MaxValue);
            }

            public static void ToggleCayoPericoIsland(bool state, bool updateCustomWeather)
            {
                RAGE.Game.Invoker.Invoke(0x9A9D1BA639675CF1, "HeistIsland", state); // SetIslandHopperEnabled
                RAGE.Game.Invoker.Invoke(0x5E1460624D194A38, state); // SetToggleMinimapHeistIsland

                if (updateCustomWeather)
                    Sync.World.SetSpecialWeather(state ? (Sync.World.WeatherTypes?)Sync.World.WeatherTypes.EXTRASUNNY : null);

                LoadTask?.Cancel();

                if (state)
                {
                    LoadTask = new AsyncTask(() =>
                    {
                        RAGE.Game.Streaming.RemoveIpl("h4_islandx_sea_mines");

                        LoadTask = null;
                    }, 2000, false, 0);
                }
                else
                {
                    LoadTask = new AsyncTask(() =>
                    {
                        RAGE.Game.Streaming.RequestIpl("h4_islandx_terrain_01_slod");
                        RAGE.Game.Streaming.RequestIpl("h4_islandx_terrain_02_slod");
                        RAGE.Game.Streaming.RequestIpl("h4_islandx_terrain_03_lod");
                        RAGE.Game.Streaming.RequestIpl("h4_islandx_terrain_04_slod");
                        RAGE.Game.Streaming.RequestIpl("h4_islandx_terrain_05_slod");
                        RAGE.Game.Streaming.RequestIpl("h4_islandx_terrain_06_slod");

                        var intid = RAGE.Game.Interior.GetInteriorAtCoords(4840.571f, -5174.425f, 2f);

                        RAGE.Game.Interior.RefreshInterior(intid);

                        LoadTask = null;
                    }, 1550, false, 0);
                }

                LoadTask.Run();

                IslandLoaded = state;
            }
        }

        public class Bank
        {
            public static Dictionary<int, Bank> All = new Dictionary<int, Bank>();

            public int Id { get; set; }

            public List<NPC> Workers { get; set; }

            public Blip Blip { get; set; }

            public Bank(int id, Vector3 BlipPosition, params (Vector3 NpcPosition, float NpcHeading)[] NPCs)
            {
                Id = id;

                Blip = new Blip(605, BlipPosition, "Банковское отделение", 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                Workers = new List<NPC>();

                for (int i = 0; i < NPCs.Length; i++)
                {
                    var npc = new NPC($"bank_w_{Id}_{i}", "Эмили", NPC.Types.Talkable, "csb_anita", NPCs[i].NpcPosition, NPCs[i].NpcHeading, Settings.MAIN_DIMENSION);

                    npc.DefaultDialogueId = "bank_preprocess";

                    npc.Data = this;

                    Workers.Add(npc);
                }
            }
        }

        public class ATM
        {
            public static Dictionary<int, ATM> All = new Dictionary<int, ATM>();

            public int Id { get; set; }

            public Additional.ExtraColshape Colshape { get; set; }

            public Blip Blip { get; set; }

            public ATM(int Id, Vector3 Position, float Range)
            {
                this.Id = Id;

                All.Add(Id, this);

                Colshape = new Additional.Sphere(Position, Range, false, new Utils.Colour(255, 0, 0, 255), Settings.MAIN_DIMENSION, null);

                Colshape.Data = this;

                Colshape.InteractionType = Additional.ExtraColshape.InteractionTypes.ATM;
                Colshape.ActionType = Additional.ExtraColshape.ActionTypes.ATM;

                Colshape.Name = $"atm_{Id}";

                Blip = new Blip(108, Position, "Банкомат", 0.4f, 25, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            }
        }

        public abstract class Business
        {
            public static Dictionary<int, Business> All = new Dictionary<int, Business>();

            public enum Types
            {
                ClothesShop1 = 0,
                ClothesShop2,
                ClothesShop3,

                CarShop1,
                CarShop2,
                CarShop3,

                MotoShop,

                BoatShop,

                AeroShop,

                Market,

                GasStation,
            }

            public Types Type { get; set; }

            public int Id { get; set; }

            public int SubId { get; set; }

            public string OwnerName => Sync.World.GetSharedData<string>($"Business::{Id}::OName");

            public Blip OwnerBlip { get => Player.LocalPlayer.GetData<Blip>($"Business::{Id}::OBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"Business::{Id}::OBlip"); Player.LocalPlayer.SetData($"Business::{Id}::OBlip", value); } }

            public Blip Blip { get; set; }

            public TextLabel InfoText { get; set; }

            public Additional.ExtraColshape InfoColshape { get; set; }

            public Additional.ExtraColshape AdditionalColshape { get; set; }

            public NPC Seller { get; set; }

            public string Name => Locale.Property.BusinessNames.GetValueOrDefault(Type) ?? "null";

            public Business(int Id, Vector3 PositionInfo, Types Type)
            {
                this.Type = Type;

                this.Id = Id;

                InfoColshape = new Additional.Cylinder(PositionInfo, 1f, 1.5f, false, new Utils.Colour(255, 0, 0, 255), Settings.MAIN_DIMENSION, null);

                InfoColshape.ActionType = Additional.ExtraColshape.ActionTypes.BusinessInfo;
                InfoColshape.InteractionType = Additional.ExtraColshape.InteractionTypes.BusinessInfo;

                InfoColshape.Data = Id;

                InfoText = new TextLabel(new Vector3(PositionInfo.X, PositionInfo.Y, PositionInfo.Z + 0.5f), $"{Name} #{SubId}", new RGBA(255, 255, 255, 255), 25f, 0, false, Settings.MAIN_DIMENSION) { Font = 0 };

                All.Add(Id, this);
            }

            public void UpdateOwnerName(string name)
            {
                if (name == null)
                    name = Locale.Property.NoOwner;
            }

            public void ToggleOwnerBlip(bool state)
            {
                if (state)
                {
                    Blip.SetDisplay(0);

                    var oBlip = OwnerBlip;

                    oBlip?.Destroy();

                    OwnerBlip = new Blip(207, InfoColshape.Position, Name, 1f, 5, 255, 0f, false, 0, 0f, Settings.MAIN_DIMENSION);
                }
                else
                {
                    Blip.SetDisplay(2);

                    var oBlip = OwnerBlip;

                    if (oBlip != null)
                    {
                        oBlip.Destroy();

                        OwnerBlip = null;
                    }
                }
            }
        }

        public class ClothesShop1 : Business
        {
            private static int Counter = 1;

            public ClothesShop1(int Id, Vector3 PositionInfo, Vector3 PositionPed, float HeadingPed, string NamePed, string ModelPed) : base(Id, PositionInfo, Types.ClothesShop1)
            {
                this.Blip = new Blip(73, PositionInfo, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                this.Seller = new NPC($"seller_{Id}", NamePed, NPC.Types.Talkable, ModelPed, PositionPed, HeadingPed, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;

                SubId = Counter++;
            }
        }

        public class ClothesShop2 : Business
        {
            private static int Counter = 1;

            public ClothesShop2(int Id, Vector3 PositionInfo, Vector3 PositionPed, float HeadingPed, string NamePed, string ModelPed) : base(Id, PositionInfo, Types.ClothesShop2)
            {
                this.Blip = new Blip(366, PositionInfo, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                this.Seller = new NPC($"seller_{Id}", NamePed, NPC.Types.Talkable, ModelPed, PositionPed, HeadingPed, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;

                SubId = Counter++;
            }
        }

        public class ClothesShop3 : Business
        {
            private static int Counter = 1;

            public ClothesShop3(int Id, Vector3 PositionInfo, Vector3 PositionPed, float HeadingPed, string NamePed, string ModelPed) : base(Id, PositionInfo, Types.ClothesShop3)
            {
                this.Blip = new Blip(439, PositionInfo, Name, 1f, 5, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                this.Seller = new NPC($"seller_{Id}", NamePed, NPC.Types.Talkable, ModelPed, PositionPed, HeadingPed, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;

                SubId = Counter++;
            }
        }

        public class Market : Business
        {
            private static int Counter = 1;

            public Market(int Id, Vector3 PositionInfo, Vector3 PositionPed, float HeadingPed, string NamePed, string ModelPed) : base(Id, PositionInfo, Types.Market)
            {
                this.Blip = new Blip(52, PositionInfo, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                this.Seller = new NPC($"seller_{Id}", NamePed, NPC.Types.Talkable, ModelPed, PositionPed, HeadingPed, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;

                SubId = Counter++;
            }
        }

        public class GasStation : Business
        {
            private static int Counter = 1;

            public GasStation(int Id, Vector3 PositionInfo, Vector3 PositionGas, Vector3 PositionPed, float HeadingPed, string NamePed, string ModelPed) : base(Id, PositionInfo, Types.GasStation)
            {
                this.Blip = new Blip(361, PositionInfo, Name, 0.75f, 47, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var cs = new Additional.Sphere(PositionGas, 5f, false, new Utils.Colour(255, 0, 0, 125), Settings.MAIN_DIMENSION, null);

                cs.ActionType = Additional.ExtraColshape.ActionTypes.GasStation;
                cs.Data = this.Id;

                //this.Seller = new NPC($"seller_{Id}", NamePed, NPC.Types.Seller, ModelPed, PositionPed, HeadingPed, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                //this.Seller.Data = this;

                SubId = Counter++;
            }
        }

        public class CarShop1 : Business
        {
            private static int Counter = 1;

            public CarShop1(int Id, Vector3 PositionInfo, Vector3 PositionPed, float HeadingPed, string NamePed, string ModelPed) : base(Id, PositionInfo, Types.CarShop1)
            {
                this.Blip = new Blip(225, PositionInfo, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                this.Seller = new NPC($"seller_{Id}", NamePed, NPC.Types.Talkable, ModelPed, PositionPed, HeadingPed, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;

                SubId = Counter++;
            }
        }

        public class CarShop2 : Business
        {
            private static int Counter = 1;

            public CarShop2(int Id, Vector3 PositionInfo, Vector3 PositionPed, float HeadingPed, string NamePed, string ModelPed) : base(Id, PositionInfo, Types.CarShop2)
            {
                this.Blip = new Blip(530, PositionInfo, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                this.Seller = new NPC($"seller_{Id}", NamePed, NPC.Types.Talkable, ModelPed, PositionPed, HeadingPed, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;

                SubId = Counter++;
            }
        }

        public class CarShop3 : Business
        {
            private static int Counter = 1;

            public CarShop3(int Id, Vector3 PositionInfo, Vector3 PositionPed, float HeadingPed, string NamePed, string ModelPed) : base(Id, PositionInfo, Types.CarShop3)
            {
                this.Blip = new Blip(523, PositionInfo, Name, 1f, 5, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                this.Seller = new NPC($"seller_{Id}", NamePed, NPC.Types.Talkable, ModelPed, PositionPed, HeadingPed, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;

                SubId = Counter++;
            }
        }

        public class MotoShop : Business
        {
            private static int Counter = 1;

            public MotoShop(int Id, Vector3 PositionInfo, Vector3 PositionPed, float HeadingPed, string NamePed, string ModelPed) : base(Id, PositionInfo, Types.MotoShop)
            {
                this.Blip = new Blip(522, PositionInfo, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                this.Seller = new NPC($"seller_{Id}", NamePed, NPC.Types.Talkable, ModelPed, PositionPed, HeadingPed, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;

                SubId = Counter++;
            }
        }

        public class BoatShop : Business
        {
            private static int Counter = 1;

            public BoatShop(int Id, Vector3 PositionInfo, Vector3 PositionPed, float HeadingPed, string NamePed, string ModelPed) : base(Id, PositionInfo, Types.BoatShop)
            {
                this.Blip = new Blip(410, PositionInfo, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                this.Seller = new NPC($"seller_{Id}", NamePed, NPC.Types.Talkable, ModelPed, PositionPed, HeadingPed, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;

                SubId = Counter++;
            }
        }

        public class AeroShop : Business
        {
            private static int Counter = 1;

            public AeroShop(int Id, Vector3 PositionInfo, Vector3 PositionPed, float HeadingPed, string NamePed, string ModelPed) : base(Id, PositionInfo, Types.AeroShop)
            {
                this.Blip = new Blip(602, PositionInfo, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                this.Seller = new NPC($"seller_{Id}", NamePed, NPC.Types.Talkable, ModelPed, PositionPed, HeadingPed, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;

                SubId = Counter++;
            }
        }

        public Locations()
        {
            CayoPerico.Initialize();

            Garage.Style.LoadAll();

            #region Clothes (Cheap)
            new ClothesShop1(1, new Vector3(1200f, 2701f, 37f), new Vector3(1201.885f, 2710.143f, 38.2226f), 105f, "Лана", "csb_anita");
            new ClothesShop1(3, new Vector3(-1096f, 2702.9f, 18f), new Vector3(-1097.523f, 2714.026f, 19.108f), 150f, "Лана", "csb_anita");
            new ClothesShop1(4, new Vector3(1683.5f, 4822.3f, 41f), new Vector3(1694.727f, 4817.582f, 42.06f), 20f, "Лана", "csb_anita");
            new ClothesShop1(5, new Vector3(-0.26f, 6519.46f, 30.5f), new Vector3(1f, 6508.753f, 31.87f), 325, "Лана", "csb_anita");

            new ClothesShop1(11, new Vector3(-815.1346f, -1079.327f, 10.13754f), new Vector3(-817.8808f, -1070.944f, 11.32811f), 135f, "Лана", "csb_anita");
            new ClothesShop1(12, new Vector3(83.63f, -1389.665f, 28.4166f), new Vector3(75.42346f, -1387.689f, 29.37614f), 195.8f, "Лана", "csb_anita");
            new ClothesShop1(13, new Vector3(416.326f, -805.2744f, 28.37296f), new Vector3(425.6321f, -811.4822f, 29.49114f), 11f, "Лана", "csb_anita");
            #endregion

            #region Clothes (Expensive)
            new ClothesShop2(2, new Vector3(622f, 2744.5f, 41f), new Vector3(613.035f, 2761.843f, 42.088f), 265f, "Лана", "csb_anita");
            new ClothesShop2(14, new Vector3(-3164.073f, 1059.789f, 19.84639f), new Vector3(-3169.008f, 1044.211f, 20.86322f), 48.6f, "Лана", "csb_anita");

            new ClothesShop2(9, new Vector3(127.03f, -205.91f, 53.55547f), new Vector3(127.3073f, -223.18f, 54.55783f), 66f, "Лана", "csb_anita");
            new ClothesShop2(10, new Vector3(-1203.283f, -781.6449f, 16.3305f), new Vector3(-1194.725f, -767.6141f, 17.31629f), 208f, "Лана", "csb_anita");

            #endregion

            #region Clothes (Brand)
            new ClothesShop3(6, new Vector3(-1455.7f, -228.9f, 48.25f), new Vector3(-1448.824f, -237.893f, 49.81332f), 45f, "Лана", "csb_anita");
            new ClothesShop3(7, new Vector3(-718f, -160f, 36f), new Vector3(-708.95f, -151.6612f, 37.415f), 114f, "Лана", "csb_anita");
            new ClothesShop3(8, new Vector3(-152.62f, -304f, 37.91f), new Vector3(-165f, -303.2f, 39.73328f), 251f, "Лана", "csb_anita");
            #endregion

            new Market(15, new Vector3(546.2691f, 2674.628f, 42f), new Vector3(549.1185f, 2671.407f, 42.1565f), 91.55f, "Лана", "csb_anita");

            new GasStation(16, new Vector3(270.1317f, 2601.239f, 44.64737f), new Vector3(263.9698f, 2607.402f, 44.98298f), null, 0f, "", "");

            new CarShop1(17, new Vector3(-62.48621f, -1089.3f, 26.69341f), new Vector3(-54.68786f, -1088.418f, 26.42234f), 155.8f, "Лана", "csb_anita");

            new BoatShop(18, new Vector3(-813.3688f, -1336.428f, 5.150263f), new Vector3(-813.8713f, -1343.797f, 5.150264f), 49.62344f, "Лана", "csb_anita");

            new AeroShop(19, new Vector3(1757.495f, 3239.969f, 41.94524f), new Vector3(1760.724f, 3234.819f, 42.13989f), 314.5554f, "Лана", "csb_anita");

            #region ATM
            new ATM(0, new Vector3(-301.65726f, -829.5886f, 32.419765f), 1f);
            new ATM(1, new Vector3(-303.2257f, -829.3121f, 32.419765f), 1f);
            new ATM(2, new Vector3(-204.0193f, -861.0091f, 30.271332f), 1f);
            new ATM(3, new Vector3(118.64156f, -883.56946f, 31.13945f), 1f);
            new ATM(4, new Vector3(24.5933f, -945.543f, 29.333046f), 1f);
            new ATM(5, new Vector3(5.686035f, -919.9551f, 29.48088f), 1f);
            new ATM(6, new Vector3(296.17563f, -896.2318f, 29.290146f), 1f);
            new ATM(7, new Vector3(296.8775f, -894.3196f, 29.261478f), 1f);
            new ATM(8, new Vector3(147.47305f, -1036.2175f, 29.367783f), 1f);
            new ATM(9, new Vector3(145.83922f, -1035.6254f, 29.367783f), 1f);
            new ATM(10, new Vector3(112.47614f, -819.80804f, 31.339552f), 1f);
            new ATM(11, new Vector3(111.38856f, -774.84015f, 31.437658f), 1f);
            new ATM(12, new Vector3(114.54742f, -775.9721f, 31.417364f), 1f);
            new ATM(13, new Vector3(-256.6386f, -715.88983f, 33.7883f), 1f);
            new ATM(14, new Vector3(-259.27673f, -723.2652f, 33.701546f), 1f);
            new ATM(15, new Vector3(-254.52185f, -692.8869f, 33.578255f), 1f);
            new ATM(16, new Vector3(-27.890343f, -724.10895f, 44.22287f), 1f);
            new ATM(17, new Vector3(-30.099571f, -723.2863f, 44.22287f), 1f);
            new ATM(18, new Vector3(228.03244f, 337.85013f, 105.50133f), 1f);
            new ATM(19, new Vector3(158.79654f, 234.74516f, 106.643265f), 1f);
            new ATM(20, new Vector3(527.77765f, -160.66086f, 57.136715f), 1f);
            new ATM(21, new Vector3(-57.170288f, -92.37918f, 57.750687f), 1f);
            new ATM(22, new Vector3(89.813385f, 2.880325f, 68.35214f), 1f);
            new ATM(23, new Vector3(285.3485f, 142.97507f, 104.16232f), 1f);
            new ATM(24, new Vector3(357.12845f, 174.08362f, 103.059654f), 1f);
            new ATM(25, new Vector3(1137.8113f, -468.86255f, 66.698654f), 1f);
            new ATM(26, new Vector3(1167.06f, -455.6541f, 66.818565f), 1f);
            new ATM(27, new Vector3(1077.7786f, -776.96643f, 58.256516f), 1f);
            new ATM(28, new Vector3(289.52997f, -1256.7876f, 29.440575f), 1f);
            new ATM(29, new Vector3(289.26785f, -1282.3204f, 29.65519f), 1f);
            new ATM(30, new Vector3(-165.58443f, 234.76587f, 94.92897f), 1f);
            new ATM(31, new Vector3(-165.58443f, 232.69547f, 94.92897f), 1f);
            new ATM(32, new Vector3(-1044.466f, -2739.6414f, 9.12406f), 1f);
            new ATM(33, new Vector3(-1205.3783f, -326.5286f, 37.85104f), 1f);
            new ATM(34, new Vector3(-1206.1417f, -325.03165f, 37.85104f), 1f);
            new ATM(35, new Vector3(-846.6537f, -341.50903f, 38.668503f), 1f);
            new ATM(36, new Vector3(-847.204f, -340.42908f, 38.6793f), 1f);
            new ATM(37, new Vector3(-720.6288f, -415.52432f, 34.97996f), 1f);
            new ATM(38, new Vector3(-867.013f, -187.99278f, 37.882175f), 1f);
            new ATM(39, new Vector3(-867.97455f, -186.34193f, 37.882175f), 1f);
            new ATM(40, new Vector3(-1415.4801f, -212.33244f, 46.49542f), 1f);
            new ATM(41, new Vector3(-1430.6633f, -211.35867f, 46.47162f), 1f);
            new ATM(42, new Vector3(-1410.7357f, -98.927895f, 52.39701f), 1f);
            new ATM(43, new Vector3(-1410.183f, -100.64539f, 52.396523f), 1f);
            new ATM(44, new Vector3(-1282.0983f, -210.55992f, 42.43031f), 1f);
            new ATM(45, new Vector3(-1286.7037f, -213.78275f, 42.43031f), 1f);
            new ATM(46, new Vector3(-1289.742f, -227.16498f, 42.43031f), 1f);
            new ATM(47, new Vector3(-1285.1365f, -223.94215f, 42.43031f), 1f);
            new ATM(48, new Vector3(-712.93567f, -818.4827f, 23.740658f), 1f);
            new ATM(49, new Vector3(-710.08276f, -818.4756f, 23.736336f), 1f);
            new ATM(50, new Vector3(-617.80347f, -708.8591f, 30.043213f), 1f);
            new ATM(51, new Vector3(-617.80347f, -706.8521f, 30.043213f), 1f);
            new ATM(52, new Vector3(-614.5187f, -705.5981f, 31.223999f), 1f);
            new ATM(53, new Vector3(-611.8581f, -705.5981f, 31.223999f), 1f);
            new ATM(54, new Vector3(-660.67633f, -854.48816f, 24.456635f), 1f);
            new ATM(55, new Vector3(-537.8052f, -854.93567f, 29.275429f), 1f);
            new ATM(56, new Vector3(-594.61444f, -1160.8519f, 22.333511f), 1f);
            new ATM(57, new Vector3(-596.12506f, -1160.8503f, 22.3336f), 1f);
            new ATM(58, new Vector3(-526.7791f, -1223.3737f, 18.45272f), 1f);
            new ATM(59, new Vector3(-1569.8396f, -547.0309f, 34.932163f), 1f);
            new ATM(60, new Vector3(-1570.7653f, -547.7035f, 34.932163f), 1f);
            new ATM(61, new Vector3(-1305.7078f, -706.6881f, 25.314468f), 1f);
            new ATM(62, new Vector3(-1315.416f, -834.431f, 16.952328f), 1f);
            new ATM(63, new Vector3(-1314.466f, -835.6913f, 16.952328f), 1f);
            new ATM(64, new Vector3(-2071.9285f, -317.2862f, 13.318085f), 1f);
            new ATM(65, new Vector3(-821.89355f, -1081.5546f, 11.136639f), 1f);
            new ATM(66, new Vector3(-1110.2284f, -1691.1538f, 4.378483f), 1f);
            new ATM(67, new Vector3(-2956.8481f, 487.21576f, 15.478001f), 1f);
            new ATM(68, new Vector3(-2958.977f, 487.30713f, 15.478001f), 1f);
            new ATM(69, new Vector3(-2974.5864f, 380.12692f, 15f), 1f);
            new ATM(70, new Vector3(-1091.8875f, 2709.0535f, 18.919415f), 1f);
            new ATM(71, new Vector3(-2295.8525f, 357.93475f, 174.60143f), 1f);
            new ATM(72, new Vector3(-2295.0693f, 356.2556f, 174.60143f), 1f);
            new ATM(73, new Vector3(-2294.2998f, 354.6056f, 174.60143f), 1f);
            new ATM(74, new Vector3(-3144.8875f, 1127.811f, 20.838036f), 1f);
            new ATM(75, new Vector3(-3043.8347f, 594.16394f, 7.732796f), 1f);
            new ATM(76, new Vector3(-3241.4546f, 997.9085f, 12.548369f), 1f);
            new ATM(77, new Vector3(2563.9995f, 2584.553f, 38.06807f), 1f);
            new ATM(78, new Vector3(2558.3242f, 350.988f, 108.597466f), 1f);
            new ATM(79, new Vector3(156.18863f, 6643.2f, 31.59372f), 1f);
            new ATM(80, new Vector3(173.8246f, 6638.2173f, 31.59372f), 1f);
            new ATM(81, new Vector3(-282.7141f, 6226.43f, 31.496475f), 1f);
            new ATM(82, new Vector3(-95.870285f, 6457.462f, 31.473938f), 1f);
            new ATM(83, new Vector3(-97.63721f, 6455.732f, 31.467934f), 1f);
            new ATM(84, new Vector3(-132.66629f, 6366.8765f, 31.47258f), 1f);
            new ATM(85, new Vector3(-386.4596f, 6046.4106f, 31.473991f), 1f);
            new ATM(86, new Vector3(1687.3951f, 4815.9f, 42.006466f), 1f);
            new ATM(87, new Vector3(1700.6941f, 6426.762f, 32.632965f), 1f);
            new ATM(88, new Vector3(1822.9714f, 3682.5771f, 34.267452f), 1f);
            new ATM(89, new Vector3(1171.523f, 2703.1394f, 38.147697f), 1f);
            new ATM(90, new Vector3(1172.4573f, 2703.1394f, 38.147697f), 1f);
            new ATM(91, new Vector3(238.26779f, 217.10918f, 106.40615f), 1f);
            new ATM(92, new Vector3(238.69781f, 216.18698f, 106.40615f), 1f);
            new ATM(93, new Vector3(237.83775f, 218.03137f, 106.40615f), 1f);
            new ATM(94, new Vector3(237.40773f, 218.95358f, 106.40615f), 1f);
            new ATM(95, new Vector3(236.9777f, 219.87578f, 106.40615f), 1f);
            new ATM(96, new Vector3(264.86896f, 209.94864f, 106.40615f), 1f);
            new ATM(97, new Vector3(265.21695f, 210.9048f, 106.40615f), 1f);
            new ATM(98, new Vector3(265.56497f, 211.86098f, 106.40615f), 1f);
            new ATM(99, new Vector3(265.913f, 212.81714f, 106.40615f), 1f);
            new ATM(100, new Vector3(266.26102f, 213.77332f, 106.40615f), 1f);
            new ATM(101, new Vector3(380.65576f, 322.8424f, 103.56634f), 1f);
            new ATM(102, new Vector3(1153.1111f, -326.90186f, 69.20503f), 1f);
            new ATM(103, new Vector3(33.19432f, -1348.8058f, 29.49696f), 1f);
            new ATM(104, new Vector3(130.57912f, -1292.3688f, 29.271421f), 1f);
            new ATM(105, new Vector3(130.15036f, -1291.6261f, 29.271421f), 1f);
            new ATM(106, new Vector3(129.69753f, -1290.8418f, 29.271421f), 1f);
            new ATM(107, new Vector3(-57.402237f, -1751.7471f, 29.420937f), 1f);
            new ATM(108, new Vector3(-718.26135f, -915.71277f, 19.21553f), 1f);
            new ATM(109, new Vector3(-273.36655f, -2024.2079f, 30.169643f), 1f);
            new ATM(110, new Vector3(-262.36078f, -2012.054f, 30.169643f), 1f);
            new ATM(111, new Vector3(-1391.3445f, -589.86273f, 30.315836f), 1f);
            new ATM(112, new Vector3(-1827.6887f, 784.465f, 138.31522f), 1f);
            new ATM(113, new Vector3(-3040.2046f, 593.29694f, 7.908859f), 1f);
            new ATM(114, new Vector3(-3240.028f, 1008.5453f, 12.830639f), 1f);
            new ATM(115, new Vector3(2559.0522f, 389.47443f, 108.62291f), 1f);
            new ATM(116, new Vector3(1703.3152f, 4934.0527f, 42.063587f), 1f);
            new ATM(117, new Vector3(1735.0105f, 6410.01f, 35.03717f), 1f);
            new ATM(118, new Vector3(2683.592f, 3286.3f, 55.241077f), 1f);
            new ATM(119, new Vector3(1968.3923f, 3743.0784f, 32.343689f), 1f);
            new ATM(120, new Vector3(540.22064f, 2671.683f, 42.15644f), 1f);
            #endregion

            new Bank(0, new Vector3(-111.6786f, 6462.01f, 31.64078f), (new Vector3(-111.2246f, 6469.992f, 31.62671f), 133.712f));
            new Bank(1, new Vector3(1175.001f, 2708.205f, 38.08792f), (new Vector3(1175.001f, 2708.205f, 38.08792f), 176.3602f));
            new Bank(2, new Vector3(246.737f, 218.0641f, 106.2868f), (new Vector3(243.6818f, 226.22f, 106.2876f), 154.0384f), (new Vector3(248.7591f, 224.3721f, 106.2876f), 154.0384f), (new Vector3(253.9547f, 222.5404f, 106.2876f), 154.0384f));
            new Bank(3, new Vector3(-349.6931f, -45.84351f, 49.03683f), (new Vector3(-351.3526f, -51.27759f, 49.0365f), 338.2035f));
            new Bank(4, new Vector3(315.5918f, -275.0276f, 53.9234f), (new Vector3(313.8149f, -280.5039f, 54.16468f), 338.7193f));
            new Bank(5, new Vector3(-1215.04f, -326.2117f, 37.67439f), (new Vector3(-1211.996f, -332.0042f, 37.78094f), 25.14937f));
            new Bank(6, new Vector3(-2968.591f, 482.9666f, 15.4687f), (new Vector3(-2961.119f, 482.9693f, 15.697f), 86.52053f));
            new Bank(7, new Vector3(151.3286f, -1036.054f, 29.33932f), (new Vector3(149.432f, -1042.05f, 29.36801f), 337.0007f));

            new House(1, new Vector3(1724.771f, 4642.161f, 42.8755f), Sync.House.Style.RoomTypes.Two, Garage.Types.Two, new Vector3(1723.976f, 4630.187f, 42.84944f), 50000);
        }
    }
}
