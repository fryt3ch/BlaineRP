using System.Collections.Generic;
using BlaineRP.Client.Game.Helpers;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Estates
{
    public class House : HouseBase
    {
        public static Dictionary<uint, House> All = new Dictionary<uint, House>();

        public House(uint id, Vector3 position, int roomType, int? garageType, Vector3 garagePosition, uint price, int @class, uint tax) : base(HouseBase.Types.House,
            id,
            price,
            roomType,
            @class,
            tax
        )
        {
            Position = position;

            GarageType = garageType is int garageTypeI ? (Garage.Types?)garageTypeI : null;

            GaragePosition = garagePosition;

            Colshape = new Cylinder(new Vector3(position.X, position.Y, position.Z - 1f),
                1.5f,
                2f,
                false,
                new Utils.Colour(255, 0, 0, 125),
                Settings.App.Static.MainDimension,
                null
            )
            {
                InteractionType = InteractionTypes.HouseEnter,
                ActionType = ActionTypes.HouseEnter,
                Data = this,
            };

            InfoText = new ExtraLabel(new Vector3(position.X, position.Y, position.Z - 0.5f), "", Utils.Misc.WhiteColourRGBA, 25f, 0, false, Settings.App.Static.MainDimension)
            {
                Font = 0,
            };

            All.Add(id, this);
        }

        public override string OwnerName => World.Core.GetSharedData<string>($"House::{Id}::OName");

        public Garage.Types? GarageType { get; set; }

        public Vector3 GaragePosition { get; set; }

        public override Vector3 Position { get; }

        public override ExtraBlip OwnerBlip
        {
            get => Player.LocalPlayer.GetData<ExtraBlip>($"House::{Id}::OBlip");
            set
            {
                if (value == null)
                    Player.LocalPlayer.ResetData($"House::{Id}::OBlip");
                else
                    Player.LocalPlayer.SetData($"House::{Id}::OBlip", value);
            }
        }

        public ExtraColshape OwnerGarageColshape
        {
            get => Player.LocalPlayer.GetData<ExtraColshape>($"House::{Id}::OGCS");
            set
            {
                if (value == null)
                    Player.LocalPlayer.ResetData($"House::{Id}::OGCS");
                else
                    Player.LocalPlayer.SetData($"House::{Id}::OGCS", value);
            }
        }

        public ExtraBlip OwnerGarageBlip
        {
            get => Player.LocalPlayer.GetData<ExtraBlip>($"House::{Id}::OGBlip");
            set
            {
                if (value == null)
                    Player.LocalPlayer.ResetData($"House::{Id}::OGBlip");
                else
                    Player.LocalPlayer.SetData($"House::{Id}::OGBlip", value);
            }
        }

        public override void UpdateOwnerName(string name)
        {
            InfoText.Text = string.Format(Locale.Property.HouseTextLabel, Id, name ?? Locale.Property.NoOwner);
        }

        public override void ToggleOwnerBlip(bool state)
        {
            ExtraBlip oBlip = OwnerBlip;

            oBlip?.Destroy();

            ExtraColshape ogCs = OwnerGarageColshape;

            ogCs?.Destroy();

            ExtraBlip ogBlip = OwnerGarageBlip;

            ogBlip?.Destroy();

            if (state)
            {
                if (GarageType == null)
                    OwnerBlip = new ExtraBlip(40, Position, $"Дом #{Id}", 1f, 5, 255, 0f, false, 0, 0f, Settings.App.Static.MainDimension);
                else
                    OwnerBlip = new ExtraBlip(492, Position, $"Дом #{Id}", 1.2f, 5, 255, 0f, false, 0, 0f, Settings.App.Static.MainDimension);

                if (GaragePosition != null)
                {
                    OwnerGarageColshape = new Sphere(GaragePosition, 2.5f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                    {
                        ApproveType = ApproveTypes.OnlyServerVehicleDriver,
                        ActionType = ActionTypes.HouseEnter,
                        Data = this,
                    };

                    OwnerGarageBlip = new ExtraBlip(9, GaragePosition, "", 1f, 3, 125, 0f, true, 0, 2.5f, Settings.App.Static.MainDimension);
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
}