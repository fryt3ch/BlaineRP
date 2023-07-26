using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Estates
{
    public class Garage
    {
        public enum ClassTypes
        {
            GA = 0,
            GB,
            GC,
            GD,
        }

        public enum Types
        {
            Two = 2,
            Six = 6,
            Ten = 10,
        }

        public static Dictionary<uint, Garage> All = new Dictionary<uint, Garage>();

        public Garage(uint Id, uint RootId, int Type, byte Variation, int ClassType, uint Tax, uint Price)
        {
            this.Id = Id;
            this.Type = (Types)Type;
            this.RootId = RootId;

            this.Variation = Variation;

            this.ClassType = (ClassTypes)ClassType;
            this.Tax = Tax;

            this.Price = Price;

            All.Add(Id, this);
        }

        public uint Id { get; set; }

        public Types Type { get; set; }

        public byte Variation { get; set; }

        public uint RootId { get; set; }

        public ClassTypes ClassType { get; set; }

        public uint Tax { get; set; }

        public uint Price { get; set; }

        public string OwnerName => World.Core.GetSharedData<string>($"Garages::{Id}::OName");

        public int NumberInRoot => All.Values.Where(x => x.RootId == RootId).ToList().FindIndex(x => x == this);

        public ExtraBlip OwnerBlip
        {
            get => Player.LocalPlayer.GetData<ExtraBlip>($"Garage::{Id}::OBlip");
            set
            {
                if (value == null)
                    Player.LocalPlayer.ResetData($"Garage::{Id}::OBlip");
                else
                    Player.LocalPlayer.SetData($"Garage::{Id}::OBlip", value);
            }
        }

        public ExtraBlip OwnerGarageBlip
        {
            get => Player.LocalPlayer.GetData<ExtraBlip>($"Garage::{Id}::OGBlip");
            set
            {
                if (value == null)
                    Player.LocalPlayer.ResetData($"Garage::{Id}::OBlip");
                else
                    Player.LocalPlayer.SetData($"Garage::{Id}::OGBlip", value);
            }
        }

        public ExtraColshape OwnerGarageColshape
        {
            get => Player.LocalPlayer.GetData<ExtraColshape>($"Garage::{Id}::OGCS");
            set
            {
                if (value == null)
                    Player.LocalPlayer.ResetData($"Garage::{Id}::OGCS");
                else
                    Player.LocalPlayer.SetData($"Garage::{Id}::OGCS", value);
            }
        }

        public void ToggleOwnerBlip(bool state)
        {
            ExtraBlip oBlip = OwnerBlip;

            oBlip?.Destroy();

            ExtraBlip ogBlip = OwnerGarageBlip;

            ogBlip?.Destroy();

            GarageRoot gRoot = GarageRoot.All[RootId];

            ExtraColshape ogCs = OwnerGarageColshape;

            ogCs?.Destroy();

            if (state)
            {
                //gRoot.Blip.SetDisplay(0);

                OwnerBlip = new ExtraBlip(50,
                    gRoot.EnterColshape.Position,
                    string.Format(Locale.General.Blip.GarageOwnedBlip, GarageRoot.All[RootId].Name, NumberInRoot + 1),
                    1f,
                    5,
                    255,
                    0f,
                    false,
                    0,
                    0f,
                    Settings.App.Static.MainDimension
                );

                OwnerGarageBlip = new ExtraBlip(9,
                    gRoot.VehicleEnterPosition.Position,
                    "",
                    1f,
                    3,
                    125,
                    0f,
                    true,
                    0,
                    gRoot.VehicleEnterPosition.RotationZ,
                    Settings.App.Static.MainDimension
                );

                OwnerGarageColshape = new Sphere(gRoot.VehicleEnterPosition.Position,
                    gRoot.VehicleEnterPosition.RotationZ,
                    false,
                    Utils.Misc.RedColor,
                    Settings.App.Static.MainDimension,
                    null
                )
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

        public class Style
        {
            public Style(Types Type, byte Variation, Vector3 EnterPosition, Action OnAction = null, Action OffAction = null)
            {
                this.Variation = Variation;

                this.EnterPosition = EnterPosition;

                this.OnAction = OnAction;
                this.OffAction = OffAction;

                if (!All.ContainsKey(Type))
                    All.Add(Type,
                        new Dictionary<byte, Style>()
                        {
                            { Variation, this },
                        }
                    );
                else
                    All[Type].Add(Variation, this);
            }

            private static Dictionary<Types, Dictionary<byte, Style>> All { get; set; } = new Dictionary<Types, Dictionary<byte, Style>>();

            public Types Type { get; set; }

            public Vector3 EnterPosition { get; set; }

            public byte Variation { get; set; }

            public Action OnAction { get; set; }

            public Action OffAction { get; set; }

            public static void LoadAll()
            {
                new Style(Types.Two, 0, new Vector3(179.0708f, -1005.729f, -98.99996f), null, null);
                new Style(Types.Six, 0, new Vector3(207.0894f, -998.9854f, -98.99996f), null, null);
                new Style(Types.Ten, 0, new Vector3(238.0103f, -1004.861f, -98.99996f), null, null);

                new Style(Types.Ten,
                    1,
                    new Vector3(238.0103f, -1004.861f, -98.99996f),
                    () =>
                    {
                        int intId = RAGE.Game.Interior.GetInteriorAtCoords(520f, -2625f, -50f);
                        Utils.Game.Misc.ToggleInteriorEntitySet(intId, "entity_set_tint_01", true);
                        Utils.Game.Misc.ToggleInteriorEntitySet(intId, "entity_set_shell_01", true);
                    },
                    () =>
                    {
                        int intId = RAGE.Game.Interior.GetInteriorAtCoords(520f, -2625f, -50f);
                        Utils.Game.Misc.ToggleInteriorEntitySet(intId, "entity_set_tint_01", false);
                        Utils.Game.Misc.ToggleInteriorEntitySet(intId, "entity_set_shell_01", false);
                    }
                );

                new Style(Types.Ten,
                    2,
                    new Vector3(238.0103f, -1004.861f, -98.99996f),
                    () =>
                    {
                        int intId = RAGE.Game.Interior.GetInteriorAtCoords(520f, -2625f, -50f);
                        Utils.Game.Misc.ToggleInteriorEntitySet(intId, "entity_set_tint_01", true);
                        Utils.Game.Misc.ToggleInteriorEntitySet(intId, "entity_set_shell_03", true);
                    },
                    () =>
                    {
                        int intId = RAGE.Game.Interior.GetInteriorAtCoords(520f, -2625f, -50f);
                        Utils.Game.Misc.ToggleInteriorEntitySet(intId, "entity_set_tint_01", false);
                        Utils.Game.Misc.ToggleInteriorEntitySet(intId, "entity_set_shell_03", false);
                    }
                );

                foreach (Dictionary<byte, Style> x in All.Values)
                {
                    foreach (Style y in x.Values)
                    {
                        y.OffAction?.Invoke();
                    }
                }
            }

            public static Style Get(Types type, byte variation)
            {
                return All.GetValueOrDefault(type).GetValueOrDefault(variation);
            }
        }
    }
}