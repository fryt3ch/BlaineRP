using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Data
{
    public partial class Locations : Events.Script
    {
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

            public uint Price { get; set; }

            public uint Tax { get; set; }

            public ClassTypes Class { get; set; }

            public abstract string OwnerName { get; }

            public Vector3 Position { get; set; }

            public Sync.House.Style.RoomTypes RoomType { get; set; }

            public Additional.ExtraColshape Colshape { get; set; }

            public RAGE.Elements.TextLabel InfoText { get; set; }

            public abstract Additional.ExtraBlip OwnerBlip { get; set; }

            public HouseBase(Sync.House.HouseTypes Type, uint Id, uint Price, Vector3 Position, Sync.House.Style.RoomTypes RoomType, ClassTypes Class, uint Tax)
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

            public override Additional.ExtraBlip OwnerBlip { get => Player.LocalPlayer.GetData<Additional.ExtraBlip>($"Apartments::{Id}::OBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"Apartments::{Id}::OBlip"); else Player.LocalPlayer.SetData($"Apartments::{Id}::OBlip", value); } }

            public ApartmentsRoot.Types RootType { get; set; }

            public int NumberInRoot => Apartments.All.Values.Where(x => x.RootType == RootType).ToList().FindIndex(x => x == this);

            public Apartments(uint Id, Vector3 Position, ApartmentsRoot.Types RootType, Sync.House.Style.RoomTypes RoomType, uint Price, ClassTypes Class, uint Tax) : base(Sync.House.HouseTypes.Apartments, Id, Price, Position, RoomType, Class, Tax)
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

                        OwnerBlip = new Additional.ExtraBlip(475, aRoot.PositionEnter, string.Format(Locale.General.Blip.ApartmentsOwnedBlip, aRoot.Name, NumberInRoot + 1), 1f, 5, 255, 0f, false, 0, 0f, Settings.MAIN_DIMENSION);
                    }
                    else if (curARoot.Type == aRoot.Type)
                    {
                        //aRoot.Blip.SetDisplay(2);

                        OwnerBlip = new Additional.ExtraBlip(475, Position, string.Format(Locale.General.Blip.ApartmentsOwnedBlip, aRoot.Name, NumberInRoot + 1), 1f, 5, 255, 0f, false, 0, 0f, Player.LocalPlayer.Dimension);
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

            public override Additional.ExtraBlip OwnerBlip { get => Player.LocalPlayer.GetData<Additional.ExtraBlip>($"House::{Id}::OBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"House::{Id}::OBlip"); else Player.LocalPlayer.SetData($"House::{Id}::OBlip", value); } }

            public Additional.ExtraColshape OwnerGarageColshape { get => Player.LocalPlayer.GetData<Additional.ExtraColshape>($"House::{Id}::OGCS"); set { if (value == null) Player.LocalPlayer.ResetData($"House::{Id}::OGCS"); else Player.LocalPlayer.SetData($"House::{Id}::OGCS", value); } }

            public Additional.ExtraBlip OwnerGarageBlip { get => Player.LocalPlayer.GetData<Additional.ExtraBlip>($"House::{Id}::OGBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"House::{Id}::OGBlip"); else Player.LocalPlayer.SetData($"House::{Id}::OGBlip", value); } }

            public House(uint Id, Vector3 Position, Sync.House.Style.RoomTypes RoomType, Garage.Types? GarageType, Vector3 GaragePosition, uint Price, ClassTypes Class, uint Tax) : base(Sync.House.HouseTypes.House, Id, Price, Position, RoomType, Class, Tax)
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

                ogCs?.Destroy();

                var ogBlip = OwnerGarageBlip;

                ogBlip?.Destroy();

                if (state)
                {
                    if (GarageType == null)
                    {
                        OwnerBlip = new Additional.ExtraBlip(40, Position, $"Дом #{Id}", 1f, 5, 255, 0f, false, 0, 0f, Settings.MAIN_DIMENSION);
                    }
                    else
                    {
                        OwnerBlip = new Additional.ExtraBlip(492, Position, $"Дом #{Id}", 1.2f, 5, 255, 0f, false, 0, 0f, Settings.MAIN_DIMENSION);
                    }

                    if (GaragePosition != null)
                    {
                        OwnerGarageColshape = new Additional.Sphere(GaragePosition, 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                        {
                            ApproveType = Additional.ExtraColshape.ApproveTypes.OnlyServerVehicleDriver,

                            ActionType = Additional.ExtraColshape.ActionTypes.HouseEnter,

                            Data = this,
                        };

                        OwnerGarageBlip = new Additional.ExtraBlip(9, GaragePosition, "", 1f, 3, 125, 0f, true, 0, 2.5f, Settings.MAIN_DIMENSION);
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

            public Additional.ExtraBlip Blip { get; set; }

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

                Blip = new Additional.ExtraBlip(475, PositionEnter, Name, 1f, 25, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            }

            public float GetFloorZ(int floor) => FloorPosition.Z + (floor - StartFloor) * FloorDistZ;

            public int? GetFloor(Vector3 pos)
            {
                if (FloorsAmount <= 0)
                    return null;

                var minDistZ = 10f;
                int? minFloor = null;

                for (int i = StartFloor; i < StartFloor + FloorsAmount; i++)
                {
                    var distZ = Math.Abs(pos.Z - GetFloorZ(i));

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
                        x?.Destroy();

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
    }
}
