using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.Helpers;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Estates
{
    public partial class ApartmentsRoot
    {
        public ApartmentsRoot(uint Id, byte ShellTypeNum, Vector3 PositionEnter)
        {
            this.Id = Id;

            Name = $"Жилой комплекс #{Id}";

            ShellType = (ShellTypes)ShellTypeNum;

            this.PositionEnter = PositionEnter;

            All.Add(Id, this);

            Colshape = new Cylinder(new Vector3(PositionEnter.X, PositionEnter.Y, PositionEnter.Z - 1f),
                1f,
                1.5f,
                false,
                new Utils.Colour(255, 0, 0, 255),
                Settings.App.Static.MainDimension,
                null
            )
            {
                ActionType = ActionTypes.ApartmentsRootEnter,
                InteractionType = InteractionTypes.ApartmentsRootEnter,
                Data = this,
            };

            InfoText = new ExtraLabel(new Vector3(PositionEnter.X, PositionEnter.Y, PositionEnter.Z - 0.5f),
                "",
                new RGBA(255, 255, 255, 255),
                15f,
                0,
                false,
                Settings.App.Static.MainDimension
            )
            {
                Font = 0,
            };

            Blip = new ExtraBlip(475, PositionEnter, "Жилой комплекс", 1f, 25, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
        }

        public static Dictionary<uint, ApartmentsRoot> All { get; set; } = new Dictionary<uint, ApartmentsRoot>();

        private static Dictionary<ShellTypes, ShellData> ShellDatas { get; set; } = new Dictionary<ShellTypes, ShellData>();

        public uint Id { get; set; }

        public ShellTypes ShellType { get; }

        public ShellData Shell => ShellDatas.GetValueOrDefault(ShellType);

        public Vector3 PositionEnter { get; set; }

        public ExtraColshape Colshape { get; set; }

        public ExtraLabel InfoText { get; set; }

        public ExtraBlip Blip { get; set; }

        public string Name { get; }

        public List<Apartments> AllApartments => Apartments.All.Values.Where(x => x.RootId == Id).ToList();

        public List<ExtraColshape> LoadedColshapes
        {
            get => Player.LocalPlayer.GetData<List<ExtraColshape>>("ApartmentsRoot::LoadedColshapes");
            set
            {
                if (value == null)
                    Player.LocalPlayer.ResetData("ApartmentsRoot::LoadedColshapes");
                else
                    Player.LocalPlayer.SetData("ApartmentsRoot::LoadedColshapes", value);
            }
        }

        public List<ExtraLabel> LoadedTextLabels
        {
            get => Player.LocalPlayer.GetData<List<ExtraLabel>>("ApartmentsRoot::LoadedTextLabels");
            set
            {
                if (value == null)
                    Player.LocalPlayer.ResetData("ApartmentsRoot::LoadedTextLabels");
                else
                    Player.LocalPlayer.SetData("ApartmentsRoot::LoadedTextLabels", value);
            }
        }

        public List<ExtraBlip> LoadedBlips
        {
            get => Player.LocalPlayer.GetData<List<ExtraBlip>>("ApartmentsRoot::LoadedBlips");
            set
            {
                if (value == null)
                    Player.LocalPlayer.ResetData("ApartmentsRoot::LoadedBlips");
                else
                    Player.LocalPlayer.SetData("ApartmentsRoot::LoadedBlips", value);
            }
        }

        public void Load()
        {
            List<Apartments> aps = AllApartments;

            var loadedColshapes = new List<ExtraColshape>();
            var loadedTextLabels = new List<ExtraLabel>();

            ShellData shell = Shell;

            for (var i = 0; i < aps.Count; i++)
            {
                Apartments ap = aps[i];

                Vector3 pos = ap.Position;

                var enterCs = new Cylinder(new Vector3(pos.X, pos.Y, pos.Z - 1f), 1f, 1.5f, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null)
                {
                    ActionType = ActionTypes.HouseEnter,
                    InteractionType = InteractionTypes.HouseEnter,
                    Data = ap,
                };

                loadedColshapes.Add(enterCs);

                ap.InfoText = new ExtraLabel(new Vector3(pos.X, pos.Y, pos.Z - 0.5f),
                    string.Format(Locale.Property.ApartmentsTextLabel, ap.NumberInRoot + 1, ap.OwnerName ?? Locale.Property.NoOwner),
                    Utils.Misc.WhiteColourRGBA,
                    5f,
                    0,
                    false,
                    Player.LocalPlayer.Dimension
                );
            }

            var floorCounter = 0;

            for (var i = 0; i < shell.ElevatorPositions.Length; i++)
            {
                var text = string.Format(Locale.Property.ApartmentsRootElevatorTextLabel,
                    i + shell.StartFloor,
                    floorCounter + 1,
                    floorCounter += shell.ApartmentsPositions[i].Length
                );

                for (var j = 0; j < shell.ElevatorPositions[i].Length; j++)
                {
                    Vector3 floorPos = shell.ElevatorPositions[i][j];

                    var elevatorCs = new Cylinder(new Vector3(floorPos.X, floorPos.Y, floorPos.Z - 1f),
                        1f,
                        1.5f,
                        false,
                        new Utils.Colour(255, 0, 0, 255),
                        Player.LocalPlayer.Dimension,
                        null
                    )
                    {
                        InteractionType = InteractionTypes.ApartmentsRootElevator,
                        Data = this,
                    };

                    loadedTextLabels.Add(new ExtraLabel(new Vector3(floorPos.X, floorPos.Y, floorPos.Z - 0.5f),
                            text,
                            Utils.Misc.WhiteColourRGBA,
                            5f,
                            0,
                            false,
                            Player.LocalPlayer.Dimension
                        )
                        {
                            Font = 0,
                        }
                    );

                    loadedColshapes.Add(elevatorCs);
                }
            }

            var exitCs = new Cylinder(new Vector3(shell.EnterPosition.X, shell.EnterPosition.Y, shell.EnterPosition.Z - 1f),
                1f,
                1.5f,
                false,
                new Utils.Colour(255, 0, 0, 255),
                Player.LocalPlayer.Dimension,
                null
            )
            {
                ActionType = ActionTypes.ApartmentsRootExit,
                InteractionType = InteractionTypes.ApartmentsRootExit,
                Data = this,
            };

            loadedColshapes.Add(exitCs);

            loadedTextLabels.Add(new ExtraLabel(new Vector3(shell.EnterPosition.X, shell.EnterPosition.Y, shell.EnterPosition.Z - 0.5f),
                    Locale.Property.ApartmentsRootExitTextLabel,
                    Utils.Misc.WhiteColourRGBA,
                    5f,
                    0,
                    false,
                    Player.LocalPlayer.Dimension
                )
                {
                    Font = 0,
                }
            );

            LoadedColshapes = loadedColshapes;
            LoadedTextLabels = loadedTextLabels;
        }

        public void Unload()
        {
            List<Apartments> aps = AllApartments;

            for (var i = 0; i < aps.Count; i++)
            {
                Apartments ap = aps[i];

                ap.InfoText?.Destroy();
                ap.InfoText = null;
            }

            List<ExtraColshape> loadedColshapes = LoadedColshapes;

            if (loadedColshapes != null)
            {
                foreach (ExtraColshape x in loadedColshapes)
                {
                    x?.Destroy();
                }

                loadedColshapes.Clear();

                LoadedColshapes = null;
            }

            List<ExtraLabel> loadedTextLabels = LoadedTextLabels;

            if (loadedTextLabels != null)
            {
                foreach (ExtraLabel x in loadedTextLabels)
                {
                    x?.Destroy();
                }

                loadedTextLabels.Clear();

                LoadedTextLabels = null;
            }

            List<ExtraBlip> loadedBlips = LoadedBlips;

            if (loadedBlips != null)
            {
                foreach (ExtraBlip x in loadedBlips)
                {
                    x?.Destroy();
                }

                loadedBlips.Clear();

                LoadedBlips = null;
            }
        }

        public void UpdateTextLabel()
        {
            List<Apartments> aps = AllApartments;

            ShellData shell = Shell;

            InfoText.Text = string.Format(Locale.Property.ApartmentsRootTextLabel, Name, shell.FloorsAmount, aps.Where(x => x.OwnerName == null).Count(), aps.Count);
        }

        public static void AddShell(byte typeNum, ushort startFloor, ushort floorsAmount, Vector3 enterPos, string elevPosJs, string apPosJs)
        {
            ShellDatas.TryAdd((ShellTypes)typeNum,
                new ShellData()
                {
                    StartFloor = startFloor,
                    FloorsAmount = floorsAmount,
                    EnterPosition = enterPos,
                    ElevatorPositions = RAGE.Util.Json.Deserialize<Vector3[][]>(elevPosJs),
                    ApartmentsPositions = RAGE.Util.Json.Deserialize<Vector3[][]>(apPosJs),
                }
            );
        }
    }
}