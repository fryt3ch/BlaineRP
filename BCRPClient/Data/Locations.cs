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

            public uint Id { get; set; }

            public string OwnerName => Sync.World.GetSharedData<string>($"House::{Id}::OName");

            public int Price { get; set; }

            public int Tax { get; set; }

            public ClassTypes Class { get; set; }

            public Vector3 Position { get; set; }

            public Sync.House.Style.RoomTypes RoomType { get; set; }

            public Garage.Types? GarageType { get; set; }

            public Vector3 GaragePosition { get; set; }

            public Additional.ExtraColshape Colshape { get; set; }

            public RAGE.Elements.TextLabel InfoText { get; set; }

            public Blip OwnerBlip { get => Player.LocalPlayer.GetData<Blip>($"House::{Id}::OBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"House::{Id}::OBlip"); else Player.LocalPlayer.SetData($"House::{Id}::OBlip", value); } }

            public Additional.ExtraColshape OwnerGarageColshape { get => Player.LocalPlayer.GetData<Additional.ExtraColshape>($"House::{Id}::OGCS"); set { if (value == null) Player.LocalPlayer.ResetData($"House::{Id}::OGCS"); else Player.LocalPlayer.SetData($"House::{Id}::OGCS", value); } }
           
            public Blip OwnerGarageBlip { get => Player.LocalPlayer.GetData<Blip>($"House::{Id}::OGBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"House::{Id}::OGBlip"); else Player.LocalPlayer.SetData($"House::{Id}::OGBlip", value); } }

            public House(uint Id, Vector3 Position, Sync.House.Style.RoomTypes RoomType, Garage.Types? GarageType, Vector3 GaragePosition, int Price, ClassTypes Class, int Tax)
            {
                this.Id = Id;

                this.Price = Price;

                this.Position = Position;

                this.RoomType = RoomType;

                this.GarageType = GarageType;

                this.GaragePosition = GaragePosition;

                this.Tax = Tax;

                Colshape = new Additional.Cylinder(Position, 1.5f, 2f, false, new Utils.Colour(255, 0, 0, 125), Settings.MAIN_DIMENSION, null);

                Colshape.InteractionType = Additional.ExtraColshape.InteractionTypes.HouseEnter;
                Colshape.ActionType = Additional.ExtraColshape.ActionTypes.HouseEnter;

                Colshape.Data = this;

                InfoText = new TextLabel(Position, $"Дом #{Id}", new RGBA(255, 255, 255, 255), 25f, 0, false, Settings.MAIN_DIMENSION) { Font = 0 };

                this.Class = Class;

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

            public Bank(int id, Utils.Vector4[] NPCs)
            {
                Id = id;

                Workers = new List<NPC>();

                Vector3 posBlip = new Vector3(0f, 0f, 0f);

                for (int i = 0; i < NPCs.Length; i++)
                {
                    posBlip += NPCs[i].Position;

                    var npc = new NPC($"bank_{Id}_{i}", "Эмили", NPC.Types.Talkable, "csb_anita", NPCs[i].Position, NPCs[i].RotationZ, Settings.MAIN_DIMENSION);

                    npc.DefaultDialogueId = "bank_preprocess";

                    npc.Data = this;

                    Workers.Add(npc);
                }

                Blip = new Blip(605, posBlip / NPCs.Length, "Банковское отделение", 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            }
        }

        public class ATM
        {
            public static Dictionary<int, ATM> All = new Dictionary<int, ATM>();

            public int Id { get; set; }

            public Additional.ExtraColshape Colshape { get; set; }

            public Blip Blip { get; set; }

            public ATM(int Id, Utils.Vector4 PositionParams)
            {
                this.Id = Id;

                All.Add(Id, this);

                Colshape = new Additional.Sphere(PositionParams.Position, PositionParams.RotationZ, false, new Utils.Colour(255, 0, 0, 255), Settings.MAIN_DIMENSION, null);

                Colshape.Data = this;

                Colshape.InteractionType = Additional.ExtraColshape.InteractionTypes.ATM;
                Colshape.ActionType = Additional.ExtraColshape.ActionTypes.ATM;

                Colshape.Name = $"atm_{Id}";

                Blip = new Blip(108, PositionParams.Position, "Банкомат", 0.4f, 25, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
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

                TuningShop,
            }

            public Types Type { get; set; }

            public int Id { get; set; }

            public int SubId { get; set; }

            public string OwnerName => Sync.World.GetSharedData<string>($"Business::{Id}::OName");

            public Blip OwnerBlip { get => Player.LocalPlayer.GetData<Blip>($"Business::{Id}::OBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"Business::{Id}::OBlip"); Player.LocalPlayer.SetData($"Business::{Id}::OBlip", value); } }

            public Blip Blip { get; set; }

            public TextLabel InfoText { get; set; }

            public Additional.ExtraColshape InfoColshape { get; set; }

            public NPC Seller { get; set; }

            public string Name => Locale.Property.BusinessNames.GetValueOrDefault(Type) ?? "null";

            public int Price { get; set; }

            public int Rent { get; set; }

            public float Tax { get; set; }

            public Business(int Id, Vector3 PositionInfo, Types Type, int Price, int Rent, float Tax)
            {
                this.Type = Type;

                this.Id = Id;

                this.SubId = All.Where(x => x.Value.Type == Type).Count() + 1;

                this.Price = Price;
                this.Rent = Rent;
                this.Tax = Tax;

                InfoColshape = new Additional.Cylinder(PositionInfo, 1f, 1.5f, false, new Utils.Colour(255, 0, 0, 255), Settings.MAIN_DIMENSION, null);

                InfoColshape.ActionType = Additional.ExtraColshape.ActionTypes.BusinessInfo;
                InfoColshape.InteractionType = Additional.ExtraColshape.InteractionTypes.BusinessInfo;

                InfoColshape.Data = this;

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
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public ClothesShop1(int Id, Vector3 PositionInfo, int Price, int Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.ClothesShop1, Price, Rent, Tax)
            {
                this.Blip = new Blip(73, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;
            }
        }

        public class ClothesShop2 : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public ClothesShop2(int Id, Vector3 PositionInfo, int Price, int Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.ClothesShop2, Price, Rent, Tax)
            {
                this.Blip = new Blip(366, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;
            }
        }

        public class ClothesShop3 : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public ClothesShop3(int Id, Vector3 PositionInfo, int Price, int Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.ClothesShop3, Price, Rent, Tax)
            {
                this.Blip = new Blip(439, PositionInteract.Position, Name, 1f, 5, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;
            }
        }

        public class Market : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public Market(int Id, Vector3 PositionInfo, int Price, int Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.Market, Price, Rent, Tax)
            {
                this.Blip = new Blip(52, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;
            }
        }

        public class GasStation : Business
        {
            public GasStation(int Id, Vector3 PositionInfo, int Price, int Rent, float Tax, Vector3 PositionGas, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.GasStation, Price, Rent, Tax)
            {
                this.Blip = new Blip(361, PositionGas, Name, 0.75f, 47, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var cs = new Additional.Sphere(PositionGas, 5f, false, new Utils.Colour(255, 0, 0, 125), Settings.MAIN_DIMENSION, null);

                cs.ActionType = Additional.ExtraColshape.ActionTypes.GasStation;
                cs.Data = this.Id;

                //this.Seller = new NPC($"seller_{Id}", NamePed, NPC.Types.Seller, ModelPed, PositionPed, HeadingPed, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                //this.Seller.Data = this;
            }
        }

        public class CarShop1 : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public CarShop1(int Id, Vector3 PositionInfo, int Price, int Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.CarShop1, Price, Rent, Tax)
            {
                this.Blip = new Blip(225, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;
            }
        }

        public class CarShop2 : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };
            
            public CarShop2(int Id, Vector3 PositionInfo, int Price, int Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.CarShop2, Price, Rent, Tax)
            {
                this.Blip = new Blip(530, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;
            }
        }

        public class CarShop3 : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public CarShop3(int Id, Vector3 PositionInfo, int Price, int Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.CarShop3, Price, Rent, Tax)
            {
                this.Blip = new Blip(523, PositionInteract.Position, Name, 1f, 5, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;
            }
        }

        public class MotoShop : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public MotoShop(int Id, Vector3 PositionInfo, int Price, int Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.MotoShop, Price, Rent, Tax)
            {
                this.Blip = new Blip(522, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;
            }
        }

        public class BoatShop : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public BoatShop(int Id, Vector3 PositionInfo, int Price, int Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.BoatShop, Price, Rent, Tax)
            {
                this.Blip = new Blip(410, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;
            }
        }

        public class AeroShop : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public AeroShop(int Id, Vector3 PositionInfo, int Price, int Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.AeroShop, Price, Rent, Tax)
            {
                this.Blip = new Blip(602, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION, "seller_clothes_greeting_0");

                this.Seller.Data = this;
            }
        }

        public class TuningShop : Business
        {
            public Additional.ExtraColshape EnteranceColshape { get; set; }

            public TuningShop(int Id, Vector3 PositionInfo, int Price, int Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.TuningShop, Price, Rent, Tax)
            {
                this.Blip = new Blip(72, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                this.EnteranceColshape = new Additional.Sphere(PositionInteract.Position, 2.5f, false, new Utils.Colour(255, 0, 0, 125), Settings.MAIN_DIMENSION, null);

                this.EnteranceColshape.InteractionType = Additional.ExtraColshape.InteractionTypes.TuningEnter;
                this.EnteranceColshape.ActionType = Additional.ExtraColshape.ActionTypes.TuningEnter;

                this.EnteranceColshape.Data = this;

                new Marker(44, PositionInteract.Position, 1f, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new RGBA(255, 255, 255), true, Settings.MAIN_DIMENSION);
            }
        }

        public class WeaponShop : Business
        {
            public WeaponShop(int Id, Vector3 PositionInfo, int Price, int Rent, float Tax, Utils.Vector4 PositionInteract, Vector3 ShootingRangePosition) : base(Id, PositionInfo, Types.TuningShop, Price, Rent, Tax)
            {
                this.Blip = new Blip(110, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            }
        }

        public Locations()
        {
            CayoPerico.Initialize();

            Garage.Style.LoadAll();

            #region BIZS_TO_REPLACE

            #endregion

            #region ATM_TO_REPLACE

            #endregion

            #region BANKS_TO_REPLACE

            #endregion

            #region HOUSES_TO_REPLACE

            #endregion
        }
    }
}