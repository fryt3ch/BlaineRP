using System.Collections.Generic;
using BlaineRP.Server.Extensions.GTANetworkAPI;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Estates
{
    public partial class GarageRoot
    {
        private static Dictionary<uint, GarageRoot> All { get; set; } = new Dictionary<uint, GarageRoot>();

        public uint Id { get; }

        public Vector4 EnterPosition { get; set; }

        public List<Vector4> VehicleExitPositions { get; set; }

        public Vector4 EnterPositionVehicle { get; set; }

        private int LastExitUsed { get; set; }

        public GarageRoot(uint Id, Vector4 EnterPosition, Vector4 EnterPositionVehicle, List<Vector4> VehicleExitPositions)
        {
            this.Id = Id;

            this.EnterPosition = EnterPosition;
            this.EnterPositionVehicle = EnterPositionVehicle;
            this.VehicleExitPositions = VehicleExitPositions;

            All.Add(Id, this);
        }

        public Vector4 GetNextVehicleExit()
        {
            var nextId = LastExitUsed + 1;

            if (nextId >= VehicleExitPositions.Count)
                nextId = 0;

            LastExitUsed = nextId;

            return VehicleExitPositions[nextId];
        }

        public bool IsEntityNearEnter(Entity entity) => entity.Dimension == Properties.Settings.Static.MainDimension && entity.Position.DistanceIgnoreZ(EnterPosition.Position) <= Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE;

        public bool IsEntityNearVehicleEnter(Entity entity) => entity.Dimension == Properties.Settings.Static.MainDimension && entity.Position.DistanceTo(EnterPositionVehicle.Position) <= EnterPositionVehicle.RotationZ + 2.5f;

        public static GarageRoot Get(uint id) => All.GetValueOrDefault(id);
    }
}