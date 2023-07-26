using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Server.Game.Misc
{
    public class Elevator
    {
        private static Sync.AttachSystem.Types[] AllowedEntityToEntityAttachTypes { get; } = new Sync.AttachSystem.Types[]
        {
            Sync.AttachSystem.Types.Carry,
            Sync.AttachSystem.Types.PoliceEscort,
        };

        public static Elevator Get(uint id) => All.GetValueOrDefault(id);

        private static Dictionary<uint, Elevator> All { get; set; } = new Dictionary<uint, Elevator>();

        public Utils.Vector4 Position { get; set; }

        public float Range { get; set; }

        public uint Dimension { get; set; }

        public uint[] LinkedElevators { get; set; }

        private Func<PlayerData, bool, bool> CheckFunction { get; set; }

        public Elevator(uint Id, float PositionX, float PositionY, float PositionZ, float Heading, float Range, uint Dimension)
        {
            if (All.TryAdd(Id, this))
            {
                this.Position = new Utils.Vector4(PositionX, PositionY, PositionZ, Heading);
                this.Range = Range;
                this.Dimension = Dimension;
            }
        }

        public static void InitializeAll()
        {
            #region EMS LS
            new Elevator(1, 346.4775f, -582.795f, 28.79683f, 252.7726f, 1.5f, Properties.Settings.Static.MainDimension)
            {
                LinkedElevators = new uint[] { 2, 3, },
            };

            new Elevator(2, 331.8203f, -595.4133f, 43.28408f, 66.18335f, 1.5f, Properties.Settings.Static.MainDimension)
            {
                LinkedElevators = new uint[] { 3, 1, },
            };

            new Elevator(3, 339.6073f, -584.1365f, 74.16172f, 251.6294f, 1.5f, Properties.Settings.Static.MainDimension)
            {
                LinkedElevators = new uint[] { 2, 1, },
            };
            #endregion

            #region FIB LS
            new Elevator(4, 125.0529f, -741.1408f, 33.13321f, 337.5505f, 1.5f, Properties.Settings.Static.MainDimension)
            {
                LinkedElevators = new uint[] { 5, 9, 6, 7, 8, },
            };

            new Elevator(5, 140.3531f, -768.4006f, 45.7520f, 60.07933f, 1.5f, Properties.Settings.Static.MainDimension)
            {
                LinkedElevators = new uint[] { 9, 6, 7, 8, 4, },
            };

            new Elevator(6, 136.0483f, -761.7639f, 242.152f, 160.531f, 1.5f, Properties.Settings.Static.MainDimension)
            {
                LinkedElevators = new uint[] { 7, 8, 9, 5, 4, },
            };

            new Elevator(7, 115.3614f, -741.3292f, 258.1522f, 332.9151f, 1.5f, Properties.Settings.Static.MainDimension)
            {
                LinkedElevators = new uint[] { 8, 6, 9, 5, 4, },
            };

            new Elevator(8, 141.0281f, -735.806f, 262.851f, 160.9292f, 1.5f, Properties.Settings.Static.MainDimension)
            {
                LinkedElevators = new uint[] { 7, 6, 9, 5, 4, },
            };

            new Elevator(9, 136.0041f, -761.8313f, 234.152f, 157.3688f, 1.5f, Properties.Settings.Static.MainDimension)
            {
                LinkedElevators = new uint[] { 6, 7, 8, 5, 4, },
            };
            #endregion

            var lines = new List<string>();

            foreach (var x in All)
            {
                lines.Add($"new {nameof(BlaineRP.Client.Game.Misc.Elevator)}({x.Key}, {x.Value.Position.ToCSharpStr()}, {x.Value.Range}f, {x.Value.Dimension}, \"{x.Value.LinkedElevators.SerializeToJson().Replace('\"', '\'')}\");");
            }

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetPath + @"\Game\Misc\Elevator.Initialization.cs", "TO_REPLACE", lines);
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
                    Additional.AntiCheat.TeleportPeds(elevatorTo.Position.Position, false, elevatorTo.Dimension, elevatorTo.Position.RotationZ, null, ped);
                }

                pData.Player.AttachEntity(entity, x.Type, x.SyncData);
            }
        }
    }
}
