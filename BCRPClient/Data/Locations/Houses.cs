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

            public abstract Vector3 Position { get; }

            public Sync.House.Style.RoomTypes RoomType { get; set; }

            public Additional.ExtraColshape Colshape { get; set; }

            public Additional.ExtraLabel InfoText { get; set; }

            public abstract Additional.ExtraBlip OwnerBlip { get; set; }

            public HouseBase(Sync.House.HouseTypes Type, uint Id, uint Price, Sync.House.Style.RoomTypes RoomType, ClassTypes Class, uint Tax)
            {
                this.Type = Type;

                this.Id = Id;
                this.Price = Price;
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

            public uint RootId { get; }

            public ushort FloorIdx { get; set; }

            public ushort SubIdx { get; set; }

            public int NumberInRoot { get; }

            public override Vector3 Position => ApartmentsRoot.All[RootId].Shell.GetApartmentsPosition(FloorIdx, SubIdx);

            public Apartments(uint Id, uint RootId, ushort FloorIdx, ushort SubIdx, Sync.House.Style.RoomTypes RoomType, uint Price, ClassTypes Class, uint Tax) : base(Sync.House.HouseTypes.Apartments, Id, Price, RoomType, Class, Tax)
            {
                this.RootId = RootId;

                All.Add(Id, this);

                NumberInRoot = ApartmentsRoot.All[RootId].Shell.GetApartmentsIdx(FloorIdx, SubIdx);
            }

            public override void ToggleOwnerBlip(bool state)
            {
                var oBlip = OwnerBlip;

                oBlip?.Destroy();

                var aRoot = ApartmentsRoot.All[RootId];

                if (state)
                {
                    var curARoot = Player.LocalPlayer.GetData<ApartmentsRoot>("ApartmentsRoot::Current");

                    if (curARoot == null)
                    {
                        //aRoot.Blip.SetDisplay(0);

                        OwnerBlip = new Additional.ExtraBlip(475, aRoot.PositionEnter, string.Format(Locale.General.Blip.ApartmentsOwnedBlip, aRoot.Name, NumberInRoot + 1), 1f, 5, 255, 0f, false, 0, 0f, Settings.MAIN_DIMENSION);
                    }
                    else if (curARoot.Id == aRoot.Id)
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
                var root = ApartmentsRoot.All[RootId];

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

            public override Vector3 Position { get; }

            public override Additional.ExtraBlip OwnerBlip { get => Player.LocalPlayer.GetData<Additional.ExtraBlip>($"House::{Id}::OBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"House::{Id}::OBlip"); else Player.LocalPlayer.SetData($"House::{Id}::OBlip", value); } }

            public Additional.ExtraColshape OwnerGarageColshape { get => Player.LocalPlayer.GetData<Additional.ExtraColshape>($"House::{Id}::OGCS"); set { if (value == null) Player.LocalPlayer.ResetData($"House::{Id}::OGCS"); else Player.LocalPlayer.SetData($"House::{Id}::OGCS", value); } }

            public Additional.ExtraBlip OwnerGarageBlip { get => Player.LocalPlayer.GetData<Additional.ExtraBlip>($"House::{Id}::OGBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"House::{Id}::OGBlip"); else Player.LocalPlayer.SetData($"House::{Id}::OGBlip", value); } }

            public House(uint Id, Vector3 Position, Sync.House.Style.RoomTypes RoomType, Garage.Types? GarageType, Vector3 GaragePosition, uint Price, ClassTypes Class, uint Tax) : base(Sync.House.HouseTypes.House, Id, Price, RoomType, Class, Tax)
            {
                this.Position = Position;

                this.GarageType = GarageType;

                this.GaragePosition = GaragePosition;

                Colshape = new Additional.Cylinder(new Vector3(Position.X, Position.Y, Position.Z - 1f), 1.5f, 2f, false, new Utils.Colour(255, 0, 0, 125), Settings.MAIN_DIMENSION, null)
                {
                    InteractionType = Additional.ExtraColshape.InteractionTypes.HouseEnter,
                    ActionType = Additional.ExtraColshape.ActionTypes.HouseEnter,

                    Data = this,
                };

                InfoText = new Additional.ExtraLabel(new Vector3(Position.X, Position.Y, Position.Z - 0.5f), "", Utils.WhiteColourRGBA, 25f, 0, false, Settings.MAIN_DIMENSION) { Font = 0 };

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
            public static Dictionary<uint, ApartmentsRoot> All { get; set; } = new Dictionary<uint, ApartmentsRoot>();

            private static Dictionary<ShellTypes, ShellData> ShellDatas { get; set; } = new Dictionary<ShellTypes, ShellData>();

            public enum ShellTypes : byte
            {
                None = 0,

                HighEnd_0 = 1,

                MediumEnd_0 = 2,

                LowEnd_1_0 = 3,
                LowEnd_2_0 = 4,
                LowEnd_3_0 = 5,
                LowEnd_4_0 = 6,
                LowEnd_5_0 = 7,
            }

            public class ShellData
            {
                public Vector3 EnterPosition { get; set; }

                public Vector3[][] ElevatorPositions { get; set; }

                public Vector3[][] ApartmentsPositions { get; set; }

                public ushort FloorsAmount { get; set; }
                public ushort StartFloor { get; set; }

                public ShellData()
                {

                }

                public Vector3 GetFloorPosition(ushort floorIdx, ushort subIdx = 0)
                {
                    if (floorIdx >= ElevatorPositions.Length)
                        return null;

                    if (subIdx >= ElevatorPositions[floorIdx].Length)
                        return null;

                    var d = ElevatorPositions[floorIdx][subIdx];

                    return d;
                }

                public Vector3 GetApartmentsPositionByIdx(int idx)
                {
                    if (idx < 0)
                        return null;

                    for (int i = 0; i < ApartmentsPositions.Length; i++)
                    {
                        if (idx < ApartmentsPositions[i].Length)
                            return ApartmentsPositions[i][idx];

                        idx -= ApartmentsPositions[i].Length;
                    }

                    return null;
                }

                public Vector3 GetApartmentsPosition(ushort floorIdx, ushort subIdx)
                {
                    if (floorIdx >= ApartmentsPositions.Length)
                        return null;

                    if (subIdx >= ApartmentsPositions[floorIdx].Length)
                        return null;

                    var d = ApartmentsPositions[floorIdx][subIdx];

                    return d;
                }

                public int GetApartmentsIdx(ushort floorIdx, ushort subIdx)
                {
                    if (floorIdx >= ApartmentsPositions.Length)
                        return -1;

                    if (subIdx >= ApartmentsPositions[floorIdx].Length)
                        return -1;

                    var totalCount = (int)subIdx;

                    for (ushort i = 0; i < floorIdx; i++)
                    {
                        totalCount += ApartmentsPositions[i].Length;
                    }

                    return totalCount;
                }

                public bool GetClosestElevator(Vector3 pos, out int floorIdx, out int subIdx)
                {
                    var minDist = float.MaxValue;

                    floorIdx = -1;
                    subIdx = -1;

                    for (int i = 0; i < ElevatorPositions.Length; i++)
                    {
                        for (int j = 0; j < ElevatorPositions[i].Length; j++)
                        {
                            var dist = pos.DistanceTo(ElevatorPositions[i][j]);

                            if (dist < minDist)
                            {
                                minDist = dist;

                                floorIdx = i;
                                subIdx = j;
                            }
                        }
                    }

                    return floorIdx >= 0 && subIdx >= 0;
                }
            }

            public uint Id { get; set; }

            public ShellTypes ShellType { get; }

            public ShellData Shell => ShellDatas.GetValueOrDefault(ShellType);

            public Vector3 PositionEnter { get; set; }

            public Additional.ExtraColshape Colshape { get; set; }

            public Additional.ExtraLabel InfoText { get; set; }

            public Additional.ExtraBlip Blip { get; set; }

            public string Name { get; }

            public List<Apartments> AllApartments => Apartments.All.Values.Where(x => x.RootId == Id).ToList();

            public List<Additional.ExtraColshape> LoadedColshapes { get => Player.LocalPlayer.GetData<List<Additional.ExtraColshape>>("ApartmentsRoot::LoadedColshapes"); set { if (value == null) Player.LocalPlayer.ResetData("ApartmentsRoot::LoadedColshapes"); else Player.LocalPlayer.SetData("ApartmentsRoot::LoadedColshapes", value); } }

            public List<Additional.ExtraLabel> LoadedTextLabels { get => Player.LocalPlayer.GetData<List<Additional.ExtraLabel>>("ApartmentsRoot::LoadedTextLabels"); set { if (value == null) Player.LocalPlayer.ResetData("ApartmentsRoot::LoadedTextLabels"); else Player.LocalPlayer.SetData("ApartmentsRoot::LoadedTextLabels", value); } }

            public List<Additional.ExtraBlip> LoadedBlips { get => Player.LocalPlayer.GetData<List<Additional.ExtraBlip>>("ApartmentsRoot::LoadedBlips"); set { if (value == null) Player.LocalPlayer.ResetData("ApartmentsRoot::LoadedBlips"); else Player.LocalPlayer.SetData("ApartmentsRoot::LoadedBlips", value); } }

            public ApartmentsRoot(uint Id, byte ShellTypeNum, Vector3 PositionEnter)
            {
                this.Id = Id;

                this.Name = $"Жилой комплекс #{Id}";

                this.ShellType = (ShellTypes)ShellTypeNum;

                this.PositionEnter = PositionEnter;

                All.Add(Id, this);

                this.Colshape = new Additional.Cylinder(new Vector3(PositionEnter.X, PositionEnter.Y, PositionEnter.Z - 1f), 1f, 1.5f, false, new Utils.Colour(255, 0, 0, 255), Settings.MAIN_DIMENSION, null)
                {
                    ActionType = Additional.ExtraColshape.ActionTypes.ApartmentsRootEnter,
                    InteractionType = Additional.ExtraColshape.InteractionTypes.ApartmentsRootEnter,

                    Data = this,
                };

                InfoText = new Additional.ExtraLabel(new Vector3(PositionEnter.X, PositionEnter.Y, PositionEnter.Z - 0.5f), "", new RGBA(255, 255, 255, 255), 15f, 0, false, Settings.MAIN_DIMENSION) { Font = 0 };

                Blip = new Additional.ExtraBlip(475, PositionEnter, "Жилой комплекс", 1f, 25, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            }

            public void Load()
            {
                var aps = AllApartments;

                var loadedColshapes = new List<Additional.ExtraColshape>();
                var loadedTextLabels = new List<Additional.ExtraLabel>();

                var shell = Shell;

                for (int i = 0; i < aps.Count; i++)
                {
                    var ap = aps[i];

                    var pos = ap.Position;

                    var enterCs = new Additional.Cylinder(new Vector3(pos.X, pos.Y, pos.Z - 1f), 1f, 1.5f, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null)
                    {
                        ActionType = Additional.ExtraColshape.ActionTypes.HouseEnter,
                        InteractionType = Additional.ExtraColshape.InteractionTypes.HouseEnter,

                        Data = ap,
                    };

                    loadedColshapes.Add(enterCs);

                    ap.InfoText = new Additional.ExtraLabel(new Vector3(pos.X, pos.Y, pos.Z - 0.5f), string.Format(Locale.Property.ApartmentsTextLabel, ap.NumberInRoot + 1, ap.OwnerName ?? Locale.Property.NoOwner), Utils.WhiteColourRGBA, 5f, 0, false, Player.LocalPlayer.Dimension);
                }
                var floorCounter = 0;

                for (int i = 0; i < shell.ElevatorPositions.Length; i++)
                {
                    var text = string.Format(Locale.Property.ApartmentsRootElevatorTextLabel, i + shell.StartFloor, floorCounter + 1, floorCounter += shell.ApartmentsPositions[i].Length);

                    for (int j = 0; j < shell.ElevatorPositions[i].Length; j++)
                    {
                        var floorPos = shell.ElevatorPositions[i][j];

                        var elevatorCs = new Additional.Cylinder(new Vector3(floorPos.X, floorPos.Y, floorPos.Z - 1f), 1f, 1.5f, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null)
                        {
                            InteractionType = Additional.ExtraColshape.InteractionTypes.ApartmentsRootElevator,

                            Data = this,
                        };

                        loadedTextLabels.Add(new Additional.ExtraLabel(new Vector3(floorPos.X, floorPos.Y, floorPos.Z - 0.5f), text, Utils.WhiteColourRGBA, 5f, 0, false, Player.LocalPlayer.Dimension) { Font = 0 });

                        loadedColshapes.Add(elevatorCs);
                    }
                }

                var exitCs = new Additional.Cylinder(new Vector3(shell.EnterPosition.X, shell.EnterPosition.Y, shell.EnterPosition.Z - 1f), 1f, 1.5f, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null)
                {
                    ActionType = Additional.ExtraColshape.ActionTypes.ApartmentsRootExit,
                    InteractionType = Additional.ExtraColshape.InteractionTypes.ApartmentsRootExit,

                    Data = this,
                };

                loadedColshapes.Add(exitCs);

                loadedTextLabels.Add(new Additional.ExtraLabel(new Vector3(shell.EnterPosition.X, shell.EnterPosition.Y, shell.EnterPosition.Z - 0.5f), Locale.Property.ApartmentsRootExitTextLabel, Utils.WhiteColourRGBA, 5f, 0, false, Player.LocalPlayer.Dimension) { Font = 0 });

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

                var shell = Shell;

                InfoText.Text = string.Format(Locale.Property.ApartmentsRootTextLabel, Name, shell.FloorsAmount, aps.Where(x => x.OwnerName == null).Count(), aps.Count);
            }

            public static void AddShell(byte typeNum, ushort startFloor, ushort floorsAmount, Vector3 enterPos, string elevPosJs, string apPosJs)
            {
                ShellDatas.TryAdd((ShellTypes)typeNum, new ShellData()
                {
                    StartFloor = startFloor,

                    FloorsAmount = floorsAmount,

                    EnterPosition = enterPos,

                    ElevatorPositions = RAGE.Util.Json.Deserialize<Vector3[][]>(elevPosJs),

                    ApartmentsPositions = RAGE.Util.Json.Deserialize<Vector3[][]>(apPosJs),
                });
            }
        }
    }
}
