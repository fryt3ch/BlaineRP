using BCRPServer.Game.Data;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BCRPServer.Game.Fractions
{
    public class Gang : Fraction
    {
        public Gang(Types Type, string Name) : base(Type, Name)
        {

        }

        public override string ClientData => $"Fractions.Types.{Type}, \"{Name}\", {ContainerId}, \"{ContainerPositions.SerializeToJson().Replace('\"', '\'')}\", \"{CreationWorkbenchPositions.SerializeToJson().Replace('\"', '\'')}\", {Ranks.Count - 1}, \"{CreationWorkbenchPrices.SerializeToJson().Replace('"', '\'')}\"";

        private ushort[] PermanentGangZones { get; set; }

        protected override void FractionDataTriggerEvent(PlayerData pData)
        {
            pData.Player.TriggerEvent("Player::SCF", (int)Type, News.SerializeToJson(), AllVehicles.Select(x => $"{x.Key.VID}&{x.Key.VID}&{x.Value.MinimalRank}").ToList(), AllMembers.Select(x => $"{x.CID}&{x.Name} {x.Surname}&{x.FractionRank}&{(x.IsOnline ? 1 : 0)}&{GetMemberStatus(x)}&{x.LastJoinDate.GetUnixTimestamp()}").ToList());
        }

        public class GangZone
        {
            public static List<GangZone> All { get; private set; }

            public const uint ZONE_INCOME = 1_000;
            
            public Utils.Vector2 Position { get; set; }

            public Types OwnerType { get; set; }

            public ushort Id { get; set; }

            public GangZone(ushort Id, float PosX, float PosY)
            {
                this.Id = Id;

                this.Position = new Utils.Vector2(PosX, PosY);
            }

            public void UpdateOwner(bool updateDb)
            {
                if (OwnerType == Types.None)
                {
                    Sync.World.ResetSharedData($"GZONE_{Id}_O");
                }
                else
                {
                    Sync.World.SetSharedData($"GZONE_{Id}_O", (int)OwnerType);
                }
            }

            public void UpdateBlipFlash(int interval)
            {
                if (interval <= 0)
                {
                    Sync.World.ResetSharedData($"GZONE_{Id}_FI");
                }
                else
                {
                    Sync.World.SetSharedData($"GZONE_{Id}_FI", interval);
                }
            }

            public static GangZone GetZoneById(ushort id) => All.Where(x => x.Id == id).FirstOrDefault();

            public static List<GangZone> GetAllOwnedBy(Types type) => All.Where(x => x.OwnerType == type).ToList();

            public static void Initialize()
            {
                if (All != null)
                    return;

                All = new List<GangZone>()
                {
                    new GangZone(1, -150f, -1350f),
                    new GangZone(2, -50f, -1350f),
                    new GangZone(3, 50f, -1350f),
                    new GangZone(4, 150f, -1350f),
                    new GangZone(5, 250f, -1350f),

                    new GangZone(6, -150f, -1450f),
                    new GangZone(7, -50f, -1450f),
                    new GangZone(8, 50f, -1450f),
                    new GangZone(9, 150f, -1450f),
                    new GangZone(10, 250f, -1450f),

                    new GangZone(11, -150f, -1550f),
                    new GangZone(12, -50f, -1550f),
                    new GangZone(13, 50f, -1550f),
                    new GangZone(14, 150f, -1550f),
                    new GangZone(15, 250f, -1550f),

                    new GangZone(16, -150f, -1650f),
                    new GangZone(17, -50f, -1650f),
                    new GangZone(18, 50f, -1650f),
                    new GangZone(19, 150f, -1650f),
                    new GangZone(20, 250f, -1650f),
                    new GangZone(21, 350f, -1650f),
                    new GangZone(22, 450f, -1650f),
                    new GangZone(23, 550f, -1650f),
                    new GangZone(24, 650f, -1650f),

                    new GangZone(25, -150f, -1750f),
                    new GangZone(26, -50f, -1750f),
                    new GangZone(27, 50f, -1750f),
                    new GangZone(28, 150f, -1750f),
                    new GangZone(29, 250f, -1750f),
                    new GangZone(30, 350f, -1750f),
                    new GangZone(31, 450f, -1750f),
                    new GangZone(32, 550f, -1750f),
                    new GangZone(33, 650f, -1750f),

                    new GangZone(34, -150f, -1850f),
                    new GangZone(35, -50f, -1850f),
                    new GangZone(36, 50f, -1850f),
                    new GangZone(37, 150f, -1850f),
                    new GangZone(38, 250f, -1850f),
                    new GangZone(39, 350f, -1850f),
                    new GangZone(40, 450f, -1850f),
                    new GangZone(41, 550f, -1850f),
                    new GangZone(42, 650f, -1850f),

                    new GangZone(43, -50f, -1950f),
                    new GangZone(44, 50f, -1950f),
                    new GangZone(45, 150f, -1950f),
                    new GangZone(46, 250f, -1950f),
                    new GangZone(47, 350f, -1950f),
                    new GangZone(48, 450f, -1950f),
                    new GangZone(49, 550f, -1950f),
                    new GangZone(50, 650f, -1950f),

                    new GangZone(51, 50f, -2050f),
                    new GangZone(52, 150f, -2050f),
                    new GangZone(53, 250f, -2050f),
                    new GangZone(54, 350f, -2050f),
                    new GangZone(55, 450f, -2050f),
                    new GangZone(56, 550f, -2050f),
                    new GangZone(57, 650f, -2050f),

                    new GangZone(58, 50f, -2150f),
                    new GangZone(59, 150f, -2150f),
                    new GangZone(60, 250f, -2150f),
                    new GangZone(61, 350f, -2150f),
                    new GangZone(62, 450f, -2150f),
                    new GangZone(63, 550f, -2150f),
                };
            }
        }
    }
}