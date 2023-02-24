using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Data
{
    public partial class Locations : Events.Script
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

            public uint Tax { get; set; }

            public uint Price { get; set; }

            public string OwnerName => Sync.World.GetSharedData<string>($"Garages::{Id}::OName");

            public int NumberInRoot => Garage.All.Values.Where(x => x.RootType == RootType).ToList().FindIndex(x => x == this);

            public Blip OwnerBlip { get => Player.LocalPlayer.GetData<Blip>($"Garage::{Id}::OBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"Garage::{Id}::OBlip"); else Player.LocalPlayer.SetData($"Garage::{Id}::OBlip", value); } }

            public Blip OwnerGarageBlip { get => Player.LocalPlayer.GetData<Blip>($"Garage::{Id}::OGBlip"); set { if (value == null) Player.LocalPlayer.ResetData($"Garage::{Id}::OBlip"); else Player.LocalPlayer.SetData($"Garage::{Id}::OGBlip", value); } }

            public Additional.ExtraColshape OwnerGarageColshape { get => Player.LocalPlayer.GetData<Additional.ExtraColshape>($"Garage::{Id}::OGCS"); set { if (value == null) Player.LocalPlayer.ResetData($"Garage::{Id}::OGCS"); else Player.LocalPlayer.SetData($"Garage::{Id}::OGCS", value); } }

            public Garage(uint Id, GarageRoot.Types RootType, Types Type, byte Variation, ClassTypes ClassType, uint Tax, uint Price)
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

                ogCs?.Destroy();

                if (state)
                {
                    //gRoot.Blip.SetDisplay(0);

                    OwnerBlip = new Blip(50, gRoot.EnterColshape.Position, string.Format(Locale.General.Blip.GarageOwnedBlip, GarageRoot.All[RootType].Name, NumberInRoot + 1), 1f, 5, 255, 0f, false, 0, 0f, Settings.MAIN_DIMENSION);

                    OwnerGarageBlip = new Blip(9, gRoot.VehicleEnterPosition, "", 1f, 3, 125, 0f, true, 0, 2.5f, Settings.MAIN_DIMENSION);

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
    }
}
