using BCRPClient.CEF;
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
                private static Dictionary<Types, Dictionary<byte, Style>> All { get; set; } = new Dictionary<Types, Dictionary<byte, Style>>();

                public Types Type { get; set; }

                public Vector3 EnterPosition { get; set; }

                public byte Variation { get; set; }

                public Action OnAction { get; set; }

                public Action OffAction { get; set; }

                public Style(Types Type, byte Variation, Vector3 EnterPosition, Action OnAction = null, Action OffAction = null)
                {
                    this.Variation = Variation;

                    this.EnterPosition = EnterPosition;

                    this.OnAction = OnAction;
                    this.OffAction = OffAction;

                    if (!All.ContainsKey(Type))
                        All.Add(Type, new Dictionary<byte, Style>() { { Variation, this } });
                    else
                        All[Type].Add(Variation, this);
                }

                public static void LoadAll()
                {
                    new Style(Types.Two, 0, new Vector3(179.0708f, -1005.729f, -98.99996f), null, null);
                    new Style(Types.Six, 0, new Vector3(207.0894f, -998.9854f, -98.99996f), null, null);
                    new Style(Types.Ten, 0, new Vector3(238.0103f, -1004.861f, -98.99996f), null, null);

                    new Style(Types.Ten, 1, new Vector3(238.0103f, -1004.861f, -98.99996f),
                        () =>
                        {
                            var intId = RAGE.Game.Interior.GetInteriorAtCoords(520f, -2625f, -50f);

                            Utils.ToggleInteriorEntitySet(intId, "entity_set_tint_01", true);
                            Utils.ToggleInteriorEntitySet(intId, "entity_set_shell_01", true);
                        },

                        () =>
                        {
                            var intId = RAGE.Game.Interior.GetInteriorAtCoords(520f, -2625f, -50f);

                            Utils.ToggleInteriorEntitySet(intId, "entity_set_tint_01", false);
                            Utils.ToggleInteriorEntitySet(intId, "entity_set_shell_01", false);
                        });

                    new Style(Types.Ten, 2, new Vector3(238.0103f, -1004.861f, -98.99996f),
                        () =>
                        {
                            var intId = RAGE.Game.Interior.GetInteriorAtCoords(520f, -2625f, -50f);

                            Utils.ToggleInteriorEntitySet(intId, "entity_set_tint_01", true);
                            Utils.ToggleInteriorEntitySet(intId, "entity_set_shell_03", true);
                        },

                        () =>
                        {
                            var intId = RAGE.Game.Interior.GetInteriorAtCoords(520f, -2625f, -50f);

                            Utils.ToggleInteriorEntitySet(intId, "entity_set_tint_01", false);
                            Utils.ToggleInteriorEntitySet(intId, "entity_set_shell_03", false);
                        });

                    foreach (var x in All.Values)
                        foreach (var y in x.Values)
                            y.OffAction?.Invoke();
                }

                public static Style Get(Types type, byte variation) => All.GetValueOrDefault(type).GetValueOrDefault(variation);
            }

            public enum ClassTypes
            {
                GA = 0,
                GB,
                GC,
                GD,
            }

            public uint Id { get; set; }

            public Types Type { get; set; }

            public byte Variation { get; set; }

            public GarageRoot.Types RootType { get; set; }

            public ClassTypes ClassType { get; set; }

            public int Tax { get; set; }

            public int Price { get; set; }

            public string OwnerName => Sync.World.GetSharedData<string>($"Garages::{Id}::OName");

            public int NumberInRoot => Garage.All.Values.Where(x => x.RootType == RootType).ToList().FindIndex(x => x == this);

            public Blip OwnerBlip { get => Player.LocalPlayer.GetData<Blip>($"Garage::{Id}::OBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"Garage::{Id}::OBlip"); else Player.LocalPlayer.SetData($"Garage::{Id}::OBlip", value); } }

            public Blip OwnerGarageBlip { get => Player.LocalPlayer.GetData<Blip>($"Garage::{Id}::OGBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"Garage::{Id}::OBlip"); else Player.LocalPlayer.SetData($"Garage::{Id}::OGBlip", value); } }

            public Additional.ExtraColshape OwnerGarageColshape { get => Player.LocalPlayer.GetData<Additional.ExtraColshape>($"Garage::{Id}::OGCS"); set { if (value == null) Player.LocalPlayer.ResetData($"Garage::{Id}::OGCS"); else Player.LocalPlayer.SetData($"Garage::{Id}::OGCS", value); } }

            public Garage(uint Id, GarageRoot.Types RootType, Types Type, byte Variation, ClassTypes ClassType, int Tax, int Price)
            {
                this.Id = Id;
                this.Type = Type;
                this.RootType = RootType;

                this.Variation = Variation;

                this.ClassType = ClassType;
                this.Tax = Tax;

                this.Price = Price;

                All.Add(Id, this);
            }

            public void ToggleOwnerBlip(bool state)
            {
                var oBlip = OwnerBlip;

                oBlip?.Destroy();

                var ogBlip = OwnerGarageBlip;

                ogBlip?.Destroy();

                var gRoot = GarageRoot.All[RootType];

                var ogCs = OwnerGarageColshape;

                ogCs?.Delete();

                if (state)
                {
                    //gRoot.Blip.SetDisplay(0);

                    OwnerBlip = new Blip(50, gRoot.EnterColshape.Position, string.Format(Locale.General.Blip.GarageOwnedBlip, GarageRoot.All[RootType].Name, NumberInRoot + 1), 1f, 5, 255, 0f, false, 0, 0f, Settings.MAIN_DIMENSION);

                    OwnerGarageBlip = new Blip(1, gRoot.VehicleEnterPosition, "", 1f, 3, 125, 0f, true, 0, 25f, Settings.MAIN_DIMENSION);

                    OwnerGarageColshape = new Additional.Sphere(gRoot.VehicleEnterPosition, 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                    {
                        ActionType = Additional.ExtraColshape.ActionTypes.GarageRootEnter,

                        ApproveType = Additional.ExtraColshape.ApproveTypes.OnlyServerVehicleDriver,

                        Data = gRoot,
                    };
                }
                else
                {
                    //gRoot.Blip.SetDisplay(2);

                    OwnerBlip = null;
                    OwnerGarageBlip = null;
                    OwnerGarageColshape = null;
                }
            }

            public void UpdateOwnerName(string name)
            {

            }
        }

        public class GarageRoot
        {
            public static Dictionary<Types, GarageRoot> All { get; set; } = new Dictionary<Types, GarageRoot>();

            public enum Types
            {
                Complex1 = 0,
            }

            public Types Type { get; set; }

            public Blip Blip { get; set; }

            public Additional.ExtraColshape EnterColshape { get; set; }

            public Vector3 VehicleEnterPosition { get; set; }

            public List<Garage> AllGarages => Garage.All.Values.Where(x => x.RootType == Type).ToList();

            public string Name => string.Format(Locale.Property.GarageRootName, (int)Type + 1);

            public GarageRoot(Types Type, Vector3 EnterPosition, Vector3 VehicleEnterPosition)
            {
                this.Type = Type;

                EnterPosition.Z -= 1f;

                this.VehicleEnterPosition = VehicleEnterPosition;

                this.EnterColshape = new Additional.Cylinder(EnterPosition, 1f, 1.5f, false, new Utils.Colour(255, 0, 0, 255), Settings.MAIN_DIMENSION, null)
                {
                    ActionType = Additional.ExtraColshape.ActionTypes.GarageRootEnter,
                    InteractionType = Additional.ExtraColshape.InteractionTypes.GarageRootEnter,

                    Data = this,
                };

                this.Blip = new Blip(50, EnterPosition, Locale.Property.GarageRootNameDef, 1f, 3, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                All.Add(Type, this);
            }
        }

        public class ApartmentsRoot
        {
            public static Dictionary<Types, ApartmentsRoot> All { get; set; } = new Dictionary<Types, ApartmentsRoot>();

            public enum Types
            {
                Cheap1 = 0,
            }

            public Types Type { get; set; }

            public Vector3 PositionEnter { get; set; }

            public Vector3 PositionExit { get; set; }

            public int FloorsAmount { get; set; }

            public Vector3 FloorPosition { get; set; }

            public float FloorDistZ { get; set; }

            public int StartFloor { get; set; }

            public Additional.ExtraColshape Colshape { get; set; }

            public TextLabel InfoText { get; set; }

            public Blip Blip { get; set; }

            public string Name => Locale.Property.ApartmentsRootNames.GetValueOrDefault(Type) ?? "null";

            public List<Apartments> AllApartments => Apartments.All.Values.Where(x => x.RootType == Type).ToList();

            public List<Additional.ExtraColshape> LoadedColshapes { get => Player.LocalPlayer.GetData<List<Additional.ExtraColshape>>("ApartmentsRoot::LoadedColshapes"); set { if (value == null) Player.LocalPlayer.ResetData("ApartmentsRoot::LoadedColshapes"); else Player.LocalPlayer.SetData("ApartmentsRoot::LoadedColshapes", value); } }
            
            public List<TextLabel> LoadedTextLabels { get => Player.LocalPlayer.GetData<List<TextLabel>>("ApartmentsRoot::LoadedTextLabels"); set { if (value == null) Player.LocalPlayer.ResetData("ApartmentsRoot::LoadedTextLabels"); else Player.LocalPlayer.SetData("ApartmentsRoot::LoadedTextLabels", value); } }
            
            public List<Blip> LoadedBlips { get => Player.LocalPlayer.GetData<List<Blip>>("ApartmentsRoot::LoadedBlips"); set { if (value == null) Player.LocalPlayer.ResetData("ApartmentsRoot::LoadedBlips"); else Player.LocalPlayer.SetData("ApartmentsRoot::LoadedBlips", value); } }

            public ApartmentsRoot(Types Type, Vector3 PositionEnter, Vector3 PositionExit, int FloorsAmount, Vector3 FloorPosition, float FloorDistZ, int StartFloor)
            {
                this.Type = Type;

                this.PositionEnter = PositionEnter;
                this.PositionExit = PositionExit;

                this.PositionEnter.Z -= 1f;
                this.PositionExit.Z -= 1f;

                All.Add(Type, this);

                this.FloorsAmount = FloorsAmount;
                this.FloorPosition = FloorPosition;
                this.FloorDistZ = FloorDistZ;
                this.StartFloor = StartFloor;

                this.FloorPosition.Z -= 1f;

                this.Colshape = new Additional.Cylinder(PositionEnter, 1f, 1.5f, false, new Utils.Colour(255, 0, 0, 255), Settings.MAIN_DIMENSION, null)
                {
                    ActionType = Additional.ExtraColshape.ActionTypes.ApartmentsRootEnter,
                    InteractionType = Additional.ExtraColshape.InteractionTypes.ApartmentsRootEnter,

                    Data = this,
                };

                InfoText = new TextLabel(new Vector3(PositionEnter.X, PositionEnter.Y, PositionEnter.Z + 0.5f), "", new RGBA(255, 255, 255, 255), 15f, 0, false, Settings.MAIN_DIMENSION) { Font = 0 };

                Blip = new Blip(475, PositionEnter, Name, 1f, 25, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            }

            public float GetFloorZ(int floor) => FloorPosition.Z + (floor - StartFloor) * FloorDistZ;

            public int? GetCurrentFloor()
            {
                if (FloorsAmount <= 0)
                    return null;

                var posZ = Utils.GetGroundZCoord(Player.LocalPlayer.Position, false);

                var minDistZ = 10f;
                int? minFloor = null;

                for (int i = StartFloor; i < StartFloor + FloorsAmount; i++)
                {
                    var distZ = Math.Abs(posZ - GetFloorZ(i));

                    if (distZ < minDistZ)
                    {
                        minDistZ = distZ;
                        minFloor = i;
                    }
                }

                return minFloor;
            }

            public void Load()
            {
                var aps = AllApartments;

                var loadedColshapes = new List<Additional.ExtraColshape>();
                var loadedTextLabels = new List<TextLabel>();

                for (int i = 0; i < aps.Count; i++)
                {
                    var ap = aps[i];

                    var enterCs = new Additional.Cylinder(ap.Position, 1f, 1.5f, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null)
                    {
                        ActionType = Additional.ExtraColshape.ActionTypes.HouseEnter,
                        InteractionType = Additional.ExtraColshape.InteractionTypes.HouseEnter,

                        Data = ap,
                    };

                    loadedColshapes.Add(enterCs);

                    ap.InfoText = new TextLabel(new Vector3(ap.Position.X, ap.Position.Y, ap.Position.Z + 0.5f), string.Format(Locale.Property.ApartmentsTextLabel, i + 1, ap.OwnerName ?? Locale.Property.NoOwner), Utils.WhiteColourRGBA, 5f, 0, false, Player.LocalPlayer.Dimension);
                }

                for (int i = StartFloor; i < StartFloor + FloorsAmount; i++)
                {
                    var floorPos = new Vector3(FloorPosition.X, FloorPosition.Y, GetFloorZ(i));

                    var elevatorCs = new Additional.Cylinder(floorPos, 1f, 1.5f, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null)
                    {
                        ActionType = Additional.ExtraColshape.ActionTypes.ApartmentsRootElevator,
                        InteractionType = Additional.ExtraColshape.InteractionTypes.ApartmentsRootElevator,

                        Data = this,
                    };

                    loadedTextLabels.Add(new TextLabel(new Vector3(floorPos.X, floorPos.Y, floorPos.Z + 0.5f), string.Format(Locale.Property.ApartmentsRootElevatorTextLabel, i), Utils.WhiteColourRGBA, 5f, 0, false, Player.LocalPlayer.Dimension) { Font = 0 });

                    loadedColshapes.Add(elevatorCs);
                }

                var exitCs = new Additional.Cylinder(PositionExit, 1f, 1.5f, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null)
                {
                    ActionType = Additional.ExtraColshape.ActionTypes.ApartmentsRootExit,
                    InteractionType = Additional.ExtraColshape.InteractionTypes.ApartmentsRootExit,

                    Data = this,
                };

                loadedColshapes.Add(exitCs);

                loadedTextLabels.Add(new TextLabel(new Vector3(PositionExit.X, PositionExit.Y, PositionExit.Z + 0.5f), Locale.Property.ApartmentsRootExitTextLabel, Utils.WhiteColourRGBA, 5f, 0, false, Player.LocalPlayer.Dimension) { Font = 0 });

                LoadedColshapes = loadedColshapes;
                LoadedTextLabels = loadedTextLabels;
            }

            public void Unload()
            {
                var aps = AllApartments;

                for (int i = 0; i < aps.Count; i++)
                {
                    var ap = aps[i];

                    ap.InfoText?.Destroy();
                    ap.InfoText = null;
                }

                var loadedColshapes = LoadedColshapes;

                if (loadedColshapes != null)
                {
                    foreach (var x in loadedColshapes)
                        x?.Delete();

                    loadedColshapes.Clear();

                    LoadedColshapes = null;
                }

                var loadedTextLabels = LoadedTextLabels;

                if (loadedTextLabels != null)
                {
                    foreach (var x in loadedTextLabels)
                        x?.Destroy();

                    loadedTextLabels.Clear();

                    LoadedTextLabels = null;
                }

                var loadedBlips = LoadedBlips;

                if (loadedBlips != null)
                {
                    foreach (var x in loadedBlips)
                        x?.Destroy();

                    loadedBlips.Clear();

                    LoadedBlips = null;
                }
            }

            public void UpdateTextLabel()
            {
                var aps = AllApartments;

                InfoText.Text = string.Format(Locale.Property.ApartmentsRootTextLabel, Name, FloorsAmount, aps.Where(x => x.OwnerName == null).Count(), aps.Count);
            }
        }

        public abstract class HouseBase
        {
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

            public Sync.House.HouseTypes Type { get; set; }

            public uint Id { get; set; }

            public int Price { get; set; }

            public int Tax { get; set; }

            public ClassTypes Class { get; set; }

            public abstract string OwnerName { get; }

            public Vector3 Position { get; set; }

            public Sync.House.Style.RoomTypes RoomType { get; set; }

            public Additional.ExtraColshape Colshape { get; set; }

            public RAGE.Elements.TextLabel InfoText { get; set; }

            public abstract Blip OwnerBlip { get; set; }

            public HouseBase(Sync.House.HouseTypes Type, uint Id, int Price, Vector3 Position, Sync.House.Style.RoomTypes RoomType, ClassTypes Class, int Tax)
            {
                this.Type = Type;

                Position.Z -= 1f;

                this.Id = Id;
                this.Price = Price;
                this.Position = Position;
                this.RoomType = RoomType;
                this.Class = Class;
                this.Tax = Tax;
            }

            public abstract void ToggleOwnerBlip(bool state);

            public abstract void UpdateOwnerName(string name);
        }

        public class Apartments : HouseBase
        {
            public static Dictionary<uint, Apartments> All = new Dictionary<uint, Apartments>();

            public override string OwnerName => Sync.World.GetSharedData<string>($"Apartments::{Id}::OName");

            public override Blip OwnerBlip { get => Player.LocalPlayer.GetData<Blip>($"Apartments::{Id}::OBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"Apartments::{Id}::OBlip"); else Player.LocalPlayer.SetData($"Apartments::{Id}::OBlip", value); } }

            public ApartmentsRoot.Types RootType { get; set; }

            public int NumberInRoot => Apartments.All.Values.Where(x => x.RootType == RootType).ToList().FindIndex(x => x == this);

            public Apartments(uint Id, Vector3 Position, ApartmentsRoot.Types RootType, Sync.House.Style.RoomTypes RoomType, int Price, ClassTypes Class, int Tax) : base(Sync.House.HouseTypes.Apartments, Id, Price, Position, RoomType, Class, Tax)
            {
                this.RootType = RootType;

                All.Add(Id, this);
            }

            public override void ToggleOwnerBlip(bool state)
            {
                var oBlip = OwnerBlip;

                oBlip?.Destroy();

                var aRoot = ApartmentsRoot.All[RootType];

                if (state)
                {
                    var curARoot = Player.LocalPlayer.GetData<ApartmentsRoot>("ApartmentsRoot::Current");

                    if (curARoot == null)
                    {
                        //aRoot.Blip.SetDisplay(0);

                        OwnerBlip = new Blip(475, aRoot.PositionEnter, string.Format(Locale.General.Blip.ApartmentsOwnedBlip, aRoot.Name, NumberInRoot + 1), 1f, 5, 255, 0f, false, 0, 0f, Settings.MAIN_DIMENSION);
                    }
                    else if (curARoot.Type == aRoot.Type)
                    {
                        //aRoot.Blip.SetDisplay(2);

                        OwnerBlip = new Blip(475, Position, string.Format(Locale.General.Blip.ApartmentsOwnedBlip, aRoot.Name, NumberInRoot + 1), 1f, 5, 255, 0f, false, 0, 0f, Player.LocalPlayer.Dimension);
                    }
                }
                else
                {
                    //aRoot.Blip.SetDisplay(2);

                    OwnerBlip = null;
                }
            }

            public override void UpdateOwnerName(string name)
            {
                var root = ApartmentsRoot.All[RootType];

                root.UpdateTextLabel();

                if (InfoText != null)
                {
                    InfoText.Text = string.Format(Locale.Property.ApartmentsTextLabel, NumberInRoot + 1, name ?? Locale.Property.NoOwner);
                }
            }
        }

        public class House : HouseBase
        {
            public static Dictionary<uint, House> All = new Dictionary<uint, House>();

            public override string OwnerName => Sync.World.GetSharedData<string>($"House::{Id}::OName");

            public Garage.Types? GarageType { get; set; }

            public Vector3 GaragePosition { get; set; }

            public override Blip OwnerBlip { get => Player.LocalPlayer.GetData<Blip>($"House::{Id}::OBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"House::{Id}::OBlip"); else Player.LocalPlayer.SetData($"House::{Id}::OBlip", value); } }

            public Additional.ExtraColshape OwnerGarageColshape { get => Player.LocalPlayer.GetData<Additional.ExtraColshape>($"House::{Id}::OGCS"); set { if (value == null) Player.LocalPlayer.ResetData($"House::{Id}::OGCS"); else Player.LocalPlayer.SetData($"House::{Id}::OGCS", value); } }
           
            public Blip OwnerGarageBlip { get => Player.LocalPlayer.GetData<Blip>($"House::{Id}::OGBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"House::{Id}::OGBlip"); else Player.LocalPlayer.SetData($"House::{Id}::OGBlip", value); } }

            public House(uint Id, Vector3 Position, Sync.House.Style.RoomTypes RoomType, Garage.Types? GarageType, Vector3 GaragePosition, int Price, ClassTypes Class, int Tax) : base(Sync.House.HouseTypes.House, Id, Price, Position, RoomType, Class, Tax)
            {
                this.GarageType = GarageType;

                this.GaragePosition = GaragePosition;

                Colshape = new Additional.Cylinder(Position, 1.5f, 2f, false, new Utils.Colour(255, 0, 0, 125), Settings.MAIN_DIMENSION, null)
                {
                    InteractionType = Additional.ExtraColshape.InteractionTypes.HouseEnter,
                    ActionType = Additional.ExtraColshape.ActionTypes.HouseEnter,

                    Data = this,
                };

                InfoText = new TextLabel(new Vector3(Position.X, Position.Y, Position.Z + 0.5f), "", Utils.WhiteColourRGBA, 25f, 0, false, Settings.MAIN_DIMENSION) { Font = 0 };

                All.Add(Id, this);
            }

            public override void UpdateOwnerName(string name)
            {
                InfoText.Text = string.Format(Locale.Property.HouseTextLabel, Id, name ?? Locale.Property.NoOwner);
            }

            public override void ToggleOwnerBlip(bool state)
            {
                var oBlip = OwnerBlip;

                oBlip?.Destroy();

                var ogCs = OwnerGarageColshape;

                ogCs?.Delete();

                var ogBlip = OwnerGarageBlip;

                ogBlip?.Destroy();

                if (state)
                {
                    if (GarageType == null)
                    {
                        OwnerBlip = new Blip(40, Position, $"Дом #{Id}", 1f, 5, 255, 0f, false, 0, 0f, Settings.MAIN_DIMENSION);
                    }
                    else
                    {
                        OwnerBlip = new Blip(492, Position, $"Дом #{Id}", 1.2f, 5, 255, 0f, false, 0, 0f, Settings.MAIN_DIMENSION);
                    }

                    if (GaragePosition != null)
                    {
                        OwnerGarageColshape = new Additional.Sphere(GaragePosition, 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                        {
                            ApproveType = Additional.ExtraColshape.ApproveTypes.OnlyServerVehicleDriver,

                            ActionType = Additional.ExtraColshape.ActionTypes.HouseEnter,

                            Data = this,
                        };

                        OwnerGarageBlip = new Blip(1, GaragePosition, "", 1f, 3, 125, 0f, true, 0, 25f, Settings.MAIN_DIMENSION);
                    }
                }
                else
                {
                    OwnerBlip = null;

                    OwnerGarageBlip = null;

                    OwnerGarageColshape = null;
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
                MainColshape = new Additional.Circle(new Vector3(4840.571f, -5174.425f, 0f), 2374f, false, new Utils.Colour(0, 0, 255, 125), uint.MaxValue, null)
                {
                    Name = "CayoPerico_Loader",
                };

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

                    var npc = new NPC($"bank_{Id}_{i}", "Эмили", NPC.Types.Talkable, "csb_anita", NPCs[i].Position, NPCs[i].RotationZ, Settings.MAIN_DIMENSION)
                    {
                        DefaultDialogueId = "bank_preprocess",

                        Data = this,
                    };

                    Workers.Add(npc);
                }

                Blip = new Blip(605, posBlip / NPCs.Length, Locale.Property.BankNameDef, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
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

                Colshape = new Additional.Sphere(PositionParams.Position, PositionParams.RotationZ, false, new Utils.Colour(255, 0, 0, 255), Settings.MAIN_DIMENSION, null)
                {
                    Data = this,

                    InteractionType = Additional.ExtraColshape.InteractionTypes.ATM,
                    ActionType = Additional.ExtraColshape.ActionTypes.ATM,

                    Name = $"atm_{Id}",
                };

                Blip = new Blip(108, PositionParams.Position, Locale.Property.AtmNameDef, 0.4f, 25, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
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

                WeaponShop,
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

                InfoColshape = new Additional.Cylinder(PositionInfo, 1f, 1.5f, false, new Utils.Colour(255, 0, 0, 255), Settings.MAIN_DIMENSION, null)
                {
                    ActionType = Additional.ExtraColshape.ActionTypes.BusinessInfo,
                    InteractionType = Additional.ExtraColshape.InteractionTypes.BusinessInfo,

                    Data = this,
                };

                InfoText = new TextLabel(new Vector3(PositionInfo.X, PositionInfo.Y, PositionInfo.Z + 0.5f), $"{Name} #{SubId}", new RGBA(255, 255, 255, 255), 15f, 0, false, Settings.MAIN_DIMENSION) { Font = 0 };

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

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
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

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
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

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
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

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
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

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
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

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
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

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
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

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
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

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
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

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };
            }
        }

        public class TuningShop : Business
        {
            public Additional.ExtraColshape EnteranceColshape { get; set; }

            public TuningShop(int Id, Vector3 PositionInfo, int Price, int Rent, float Tax, Utils.Vector4 PositionInteract) : base(Id, PositionInfo, Types.TuningShop, Price, Rent, Tax)
            {
                this.Blip = new Blip(72, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                this.EnteranceColshape = new Additional.Sphere(PositionInteract.Position, 2.5f, false, new Utils.Colour(255, 0, 0, 125), Settings.MAIN_DIMENSION, null)
                {
                    ApproveType = Additional.ExtraColshape.ApproveTypes.OnlyServerVehicleDriver,

                    InteractionType = Additional.ExtraColshape.InteractionTypes.TuningEnter,
                    ActionType = Additional.ExtraColshape.ActionTypes.TuningEnter,

                    Data = this,
                };

                new Marker(44, PositionInteract.Position, 1f, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new RGBA(255, 255, 255), true, Settings.MAIN_DIMENSION);
            }
        }

        public class WeaponShop : Business
        {
            private static (string Model, string Name)[] NPCs { get; set; } = new (string, string)[]
            {
                ("csb_anita", "Анита"),
            };

            public static int ShootingRangePrice => Sync.World.GetSharedData<int>("SRange::Price", 0);

            public WeaponShop(int Id, Vector3 PositionInfo, int Price, int Rent, float Tax, Utils.Vector4 PositionInteract, Vector3 ShootingRangePosition) : base(Id, PositionInfo, Types.WeaponShop, Price, Rent, Tax)
            {
                this.Blip = new Blip(110, PositionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                var npcParams = SubId >= NPCs.Length ? NPCs[0] : NPCs[SubId];

                this.Seller = new NPC($"seller_{Id}", npcParams.Name, NPC.Types.Talkable, npcParams.Model, PositionInteract.Position, PositionInteract.RotationZ, Settings.MAIN_DIMENSION)
                {
                    Data = this,

                    DefaultDialogueId = "seller_clothes_greeting_0",
                };

                var shootingRangeEnterCs = new Additional.Cylinder(ShootingRangePosition, 1.5f, 2f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                {
                    Data = this,

                    ActionType = Additional.ExtraColshape.ActionTypes.ShootingRangeEnter,
                    InteractionType = Additional.ExtraColshape.InteractionTypes.ShootingRangeEnter,
                };

                var shootingRangeText = new TextLabel(new Vector3(ShootingRangePosition.X, ShootingRangePosition.Y, ShootingRangePosition.Z + 0.5f), Locale.General.Business.ShootingRangeTitle, new RGBA(255, 255, 255, 255), 10f, 0, true, Settings.MAIN_DIMENSION);
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

            #region AROOTS_TO_REPLACE

            #endregion

            #region APARTMENTS_TO_REPLACE

            #endregion

            #region HOUSES_TO_REPLACE

            #endregion

            #region GROOTS_TO_REPLACE

            #endregion

            #region GARAGES_TO_REPLACE

            #endregion

            new NPC("vpound_w_0", "Джон", NPC.Types.Talkable, "s_m_y_airworker", new Vector3(485.6506f, -54.18661f, 78.30058f), 55.38f, Settings.MAIN_DIMENSION)
            {
                Blip = new Blip(832, new Vector3(485.6506f, -54.18661f, 78.30058f), "Штрафстоянка", 1f, 47, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION),

                DefaultDialogueId = "vpound_preprocess",
            };

            new NPC("vrent_s_0", "Джон", NPC.Types.Talkable, "s_m_y_airworker", new Vector3(-718.6724f, 5821.765f, 17.21804f), 106.9247f, Settings.MAIN_DIMENSION)
            {
                Blip = new Blip(76, new Vector3(-718.6724f, 5821.765f, 17.21804f), "Аренда мопедов", 0.85f, 47, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION),

                DefaultDialogueId = "vrent_s_preprocess",
            };
        }
    }
}