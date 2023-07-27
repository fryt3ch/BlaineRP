using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.Helpers;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.NPCs;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Businesses
{
    public abstract partial class Business
    {
        public static Dictionary<int, Business> All = new Dictionary<int, Business>();

        public Business(int Id, Vector3 PositionInfo, BusinessType Type, uint Price, uint Rent, float Tax)
        {
            this.Type = Type;

            this.Id = Id;

            SubId = All.Where(x => x.Value.Type == Type).Count() + 1;

            this.Price = Price;
            this.Rent = Rent;
            this.Tax = Tax;

            if (PositionInfo != null)
            {
                InfoColshape = new Cylinder(new Vector3(PositionInfo.X, PositionInfo.Y, PositionInfo.Z - 1f),
                    1f,
                    1.5f,
                    false,
                    new Utils.Colour(255, 0, 0, 255),
                    Settings.App.Static.MainDimension,
                    null
                )
                {
                    ActionType = ActionTypes.BusinessInfo,
                    InteractionType = InteractionTypes.BusinessInfo,
                    Data = this,
                };

                InfoText = new ExtraLabel(new Vector3(PositionInfo.X, PositionInfo.Y, PositionInfo.Z - 0.5f),
                    $"{Name} #{SubId}",
                    new RGBA(255, 255, 255, 255),
                    15f,
                    0,
                    false,
                    Settings.App.Static.MainDimension
                )
                {
                    Font = 0,
                };
            }

            All.Add(Id, this);
        }

        public BusinessType Type { get; set; }

        public int Id { get; set; }

        public int SubId { get; set; }

        public string OwnerName => World.Core.GetSharedData<string>($"Business::{Id}::OName");

        public ExtraBlip OwnerBlip
        {
            get => Player.LocalPlayer.GetData<ExtraBlip>($"Business::{Id}::OBlip");
            set
            {
                if (value == null)
                    Player.LocalPlayer.ResetData($"Business::{Id}::OBlip");
                Player.LocalPlayer.SetData($"Business::{Id}::OBlip", value);
            }
        }

        public ExtraBlip Blip { get; set; }

        public ExtraLabel InfoText { get; set; }

        public ExtraColshape InfoColshape { get; set; }

        public NPC Seller { get; set; }

        public string Name => Locale.Property.BusinessNames.GetValueOrDefault(Type) ?? "null";

        public uint Price { get; set; }

        public uint Rent { get; set; }

        public float Tax { get; set; }

        public void UpdateOwnerName(string name)
        {
            if (name == null)
                name = Locale.Property.NoOwner;
        }

        public void ToggleOwnerBlip(bool state)
        {
            if (state)
            {
                Blip.Display = 0;

                ExtraBlip oBlip = OwnerBlip;

                oBlip?.Destroy();

                OwnerBlip = new ExtraBlip(207, InfoColshape.Position, Name, 1f, 5, 255, 0f, false, 0, 0f, Settings.App.Static.MainDimension);
            }
            else
            {
                Blip.Display = 2;

                ExtraBlip oBlip = OwnerBlip;

                if (oBlip != null)
                {
                    oBlip.Destroy();

                    OwnerBlip = null;
                }
            }
        }
    }
}