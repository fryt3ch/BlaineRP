using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Sync;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Misc
{
    public partial class Elevator
    {
        private static AttachmentType[] AllowedEntityToEntityAttachTypes { get; } = new AttachmentType[]
        {
            AttachmentType.Carry,
            AttachmentType.PoliceEscort,
        };

        public static Elevator Get(uint id) => All.GetValueOrDefault(id);

        private static Dictionary<uint, Elevator> All { get; set; } = new Dictionary<uint, Elevator>();

        public Vector4 Position { get; set; }

        public float Range { get; set; }

        public uint Dimension { get; set; }

        public uint[] LinkedElevators { get; set; }

        private Func<PlayerData, bool, bool> CheckFunction { get; set; }

        public Elevator(uint Id, float PositionX, float PositionY, float PositionZ, float Heading, float Range, uint Dimension)
        {
            if (All.TryAdd(Id, this))
            {
                this.Position = new Vector4(PositionX, PositionY, PositionZ, Heading);
                this.Range = Range;
                this.Dimension = Dimension;
            }
        }

        public bool GetCheckFunctionResult(PlayerData pData, bool from)
        {
            if (CheckFunction == null)
                return true;

            return CheckFunction.Invoke(pData, from);
        }

        public static void Teleport(PlayerData pData, Elevator elevatorFrom, Elevator elevatorTo, bool fade)
        {
            if (elevatorFrom == null || elevatorTo == null)
                return;

            var attaches = pData.AttachedEntities.Where(x => AllowedEntityToEntityAttachTypes.Contains(x.Type)).ToList();

            pData.Player.Teleport(elevatorTo.Position.Position, false, elevatorTo.Dimension, elevatorTo.Position.RotationZ, fade);

            foreach (var x in attaches)
            {
                var entity = Utils.GetEntityById(x.EntityType, x.Id);

                if (entity?.Exists != true)
                    continue;

                if (entity is Player player)
                {
                    player.Teleport(elevatorTo.Position.Position, false, elevatorTo.Dimension, elevatorTo.Position.RotationZ, fade);
                }
                else if (entity is Ped ped)
                {
                    Management.AntiCheat.Service.TeleportPeds(elevatorTo.Position.Position, false, elevatorTo.Dimension, elevatorTo.Position.RotationZ, null, ped);
                }

                pData.Player.AttachEntity(entity, x.Type, x.SyncData);
            }
        }
    }
}
