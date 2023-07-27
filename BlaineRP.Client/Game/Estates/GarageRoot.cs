using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.Helpers;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using RAGE;

namespace BlaineRP.Client.Game.Estates
{
    public class GarageRoot
    {
        public GarageRoot(uint Id, Vector3 EnterPosition, Utils.Vector4 VehicleEnterPosition)
        {
            this.Id = Id;

            Name = string.Format(Locale.Property.GarageRootName, Id);

            this.VehicleEnterPosition = VehicleEnterPosition;

            EnterColshape = new Cylinder(new Vector3(EnterPosition.X, EnterPosition.Y, EnterPosition.Z - 1f),
                1f,
                1.5f,
                false,
                new Utils.Colour(255, 0, 0, 255),
                Settings.App.Static.MainDimension,
                null
            )
            {
                ActionType = ActionTypes.GarageRootEnter,
                InteractionType = InteractionTypes.GarageRootEnter,
                Data = this,
            };

            Blip = new ExtraBlip(50, EnterPosition, Locale.Property.GarageRootNameDef, 1f, 3, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            TextLabel = new ExtraLabel(new Vector3(EnterPosition.X, EnterPosition.Y, EnterPosition.Z - 0.5f),
                Name,
                new RGBA(255, 255, 255, 255),
                15f,
                0,
                false,
                Settings.App.Static.MainDimension
            )
            {
                Font = 0,
            };

            All.Add(Id, this);
        }

        public static Dictionary<uint, GarageRoot> All { get; set; } = new Dictionary<uint, GarageRoot>();

        public uint Id { get; }

        public ExtraBlip Blip { get; set; }

        public ExtraColshape EnterColshape { get; set; }

        public ExtraLabel TextLabel { get; set; }

        public Utils.Vector4 VehicleEnterPosition { get; set; }

        public List<Garage> AllGarages => Garage.All.Values.Where(x => x.RootId == Id).ToList();

        public string Name { get; }
    }
}