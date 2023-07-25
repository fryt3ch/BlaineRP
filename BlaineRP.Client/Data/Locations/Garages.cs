using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.World;
using BlaineRP.Client.Game.Wrappers;
using BlaineRP.Client.Game.Wrappers.Blips;
using BlaineRP.Client.Game.Wrappers.Colshapes;
using BlaineRP.Client.Game.Wrappers.Colshapes.Enums;
using BlaineRP.Client.Game.Wrappers.Colshapes.Types;
using Core = BlaineRP.Client.Game.World.Core;

namespace BlaineRP.Client.Data
{
    public partial class Locations
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
                            Misc.ToggleInteriorEntitySet(intId, "entity_set_tint_01", true);
                            Misc.ToggleInteriorEntitySet(intId, "entity_set_shell_01", true);
                        },

                        () =>
                        {
                            var intId = RAGE.Game.Interior.GetInteriorAtCoords(520f, -2625f, -50f);
                            Misc.ToggleInteriorEntitySet(intId, "entity_set_tint_01", false);
                            Misc.ToggleInteriorEntitySet(intId, "entity_set_shell_01", false);
                        });

                    new Style(Types.Ten, 2, new Vector3(238.0103f, -1004.861f, -98.99996f),
                        () =>
                        {
                            var intId = RAGE.Game.Interior.GetInteriorAtCoords(520f, -2625f, -50f);
                            Misc.ToggleInteriorEntitySet(intId, "entity_set_tint_01", true);
                            Misc.ToggleInteriorEntitySet(intId, "entity_set_shell_03", true);
                        },

                        () =>
                        {
                            var intId = RAGE.Game.Interior.GetInteriorAtCoords(520f, -2625f, -50f);
                            Misc.ToggleInteriorEntitySet(intId, "entity_set_tint_01", false);
                            Misc.ToggleInteriorEntitySet(intId, "entity_set_shell_03", false);
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

            public uint RootId { get; set; }

            public ClassTypes ClassType { get; set; }

            public uint Tax { get; set; }

            public uint Price { get; set; }

            public string OwnerName => Core.GetSharedData<string>($"Garages::{Id}::OName");

            public int NumberInRoot => Garage.All.Values.Where(x => x.RootId == RootId).ToList().FindIndex(x => x == this);

            public ExtraBlip OwnerBlip { get => Player.LocalPlayer.GetData<ExtraBlip>($"Garage::{Id}::OBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"Garage::{Id}::OBlip"); else Player.LocalPlayer.SetData($"Garage::{Id}::OBlip", value); } }

            public ExtraBlip OwnerGarageBlip { get => Player.LocalPlayer.GetData<ExtraBlip>($"Garage::{Id}::OGBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"Garage::{Id}::OBlip"); else Player.LocalPlayer.SetData($"Garage::{Id}::OGBlip", value); } }

            public ExtraColshape OwnerGarageColshape { get => Player.LocalPlayer.GetData<ExtraColshape>($"Garage::{Id}::OGCS"); set { if (value == null) Player.LocalPlayer.ResetData($"Garage::{Id}::OGCS"); else Player.LocalPlayer.SetData($"Garage::{Id}::OGCS", value); } }

            public Garage(uint Id, uint RootId, Types Type, byte Variation, ClassTypes ClassType, uint Tax, uint Price)
            {
                this.Id = Id;
                this.Type = Type;
                this.RootId = RootId;

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

                var gRoot = GarageRoot.All[RootId];

                var ogCs = OwnerGarageColshape;

                ogCs?.Destroy();

                if (state)
                {
                    //gRoot.Blip.SetDisplay(0);

                    OwnerBlip = new ExtraBlip(50, gRoot.EnterColshape.Position, string.Format(Locale.General.Blip.GarageOwnedBlip, GarageRoot.All[RootId].Name, NumberInRoot + 1), 1f, 5, 255, 0f, false, 0, 0f, Settings.App.Static.MainDimension);

                    OwnerGarageBlip = new ExtraBlip(9, gRoot.VehicleEnterPosition.Position, "", 1f, 3, 125, 0f, true, 0, gRoot.VehicleEnterPosition.RotationZ, Settings.App.Static.MainDimension);

                    OwnerGarageColshape = new Sphere(gRoot.VehicleEnterPosition.Position, gRoot.VehicleEnterPosition.RotationZ, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                    {
                        ActionType = ActionTypes.GarageRootEnter,

                        ApproveType = ApproveTypes.OnlyServerVehicleDriver,

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
            public static Dictionary<uint, GarageRoot> All { get; set; } = new Dictionary<uint, GarageRoot>();

            public uint Id { get; }

            public ExtraBlip Blip { get; set; }

            public ExtraColshape EnterColshape { get; set; }

            public ExtraLabel TextLabel { get; set; }

            public Utils.Vector4 VehicleEnterPosition { get; set; }

            public List<Garage> AllGarages => Garage.All.Values.Where(x => x.RootId == Id).ToList();

            public string Name { get; }

            public GarageRoot(uint Id, Vector3 EnterPosition, Utils.Vector4 VehicleEnterPosition)
            {
                this.Id = Id;

                this.Name = string.Format(Locale.Property.GarageRootName, Id);

                this.VehicleEnterPosition = VehicleEnterPosition;

                this.EnterColshape = new Cylinder(new Vector3(EnterPosition.X, EnterPosition.Y, EnterPosition.Z - 1f), 1f, 1.5f, false, new Utils.Colour(255, 0, 0, 255), Settings.App.Static.MainDimension, null)
                {
                    ActionType = ActionTypes.GarageRootEnter,
                    InteractionType = InteractionTypes.GarageRootEnter,

                    Data = this,
                };

                this.Blip = new ExtraBlip(50, EnterPosition, Locale.Property.GarageRootNameDef, 1f, 3, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                this.TextLabel = new ExtraLabel(new Vector3(EnterPosition.X, EnterPosition.Y, EnterPosition.Z - 0.5f), Name, new RGBA(255, 255, 255, 255), 15f, 0, false, Settings.App.Static.MainDimension) { Font = 0 };

                All.Add(Id, this);
            }
        }
    }
}
