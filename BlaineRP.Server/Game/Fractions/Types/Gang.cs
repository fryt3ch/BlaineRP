using BlaineRP.Server.Game.Data;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BlaineRP.Server.Game.Fractions
{
    public class Gang : Fraction
    {
        public Gang(Types Type, string Name) : base(Type, Name)
        {

        }

        public override string ClientData => $"Fractions.Types.{Type}, \"{Name}\", {ContainerId}, \"{ContainerPositions.SerializeToJson().Replace('\"', '\'')}\", \"{CreationWorkbenchPositions.SerializeToJson().Replace('\"', '\'')}\", {Ranks.Count - 1}, \"{CreationWorkbenchPrices.SerializeToJson().Replace('"', '\'')}\", {(uint)MetaFlags}";

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
                    new GangZone(1, -250, -1250),
                    new GangZone(2, -250, -1350),
                    new GangZone(3, -250, -1450),
                    new GangZone(4, -250, -1550),
                    new GangZone(5, -250, -1650),
                    new GangZone(6, -250, -1750),
                    new GangZone(7, -250, -1850),
                    new GangZone(8, -250, -1950),
                    new GangZone(9, -250, -2050),
                    new GangZone(10, -250, -2150),
                    new GangZone(11, -250, -2250),
                    new GangZone(12, -250, -2350),
                    new GangZone(13, -250, -2450),
                    new GangZone(14, -250, -2550),
                    new GangZone(15, -250, -2650),
                    new GangZone(16, -150, -1250),
                    new GangZone(17, -150, -1350),
                    new GangZone(18, -150, -1450),
                    new GangZone(19, -150, -1550),
                    new GangZone(20, -150, -1650),
                    new GangZone(21, -150, -1750),
                    new GangZone(22, -150, -1850),
                    new GangZone(23, -150, -1950),
                    new GangZone(24, -150, -2050),
                    new GangZone(25, -150, -2150),
                    new GangZone(26, -150, -2250),
                    new GangZone(27, -150, -2350),
                    new GangZone(28, -150, -2450),
                    new GangZone(29, -150, -2550),
                    new GangZone(30, -150, -2650),
                    new GangZone(31, -50, -1250),
                    new GangZone(32, -50, -1350),
                    new GangZone(33, -50, -1450),
                    new GangZone(34, -50, -1550),
                    new GangZone(35, -50, -1650),
                    new GangZone(36, -50, -1750),
                    new GangZone(37, -50, -1850),
                    new GangZone(38, -50, -1950),
                    new GangZone(39, -50, -2050),
                    new GangZone(40, -50, -2150),
                    new GangZone(41, -50, -2250),
                    new GangZone(42, -50, -2350),
                    new GangZone(43, -50, -2450),
                    new GangZone(44, -50, -2550),
                    new GangZone(45, -50, -2650),
                    new GangZone(46, 50, -1250),
                    new GangZone(47, 50, -1350),
                    new GangZone(48, 50, -1450),
                    new GangZone(49, 50, -1550),
                    new GangZone(50, 50, -1650),
                    new GangZone(51, 50, -1750),
                    new GangZone(52, 50, -1850),
                    new GangZone(53, 50, -1950),
                    new GangZone(54, 50, -2050),
                    new GangZone(55, 50, -2150),
                    new GangZone(56, 50, -2250),
                    new GangZone(57, 50, -2350),
                    new GangZone(58, 50, -2450),
                    new GangZone(59, 50, -2550),
                    new GangZone(60, 50, -2650),
                    new GangZone(61, 150, -1250),
                    new GangZone(62, 150, -1350),
                    new GangZone(63, 150, -1450),
                    new GangZone(64, 150, -1550),
                    new GangZone(65, 150, -1650),
                    new GangZone(66, 150, -1750),
                    new GangZone(67, 150, -1850),
                    new GangZone(68, 150, -1950),
                    new GangZone(69, 150, -2050),
                    new GangZone(70, 150, -2150),
                    new GangZone(71, 150, -2250),
                    new GangZone(72, 150, -2350),
                    new GangZone(73, 150, -2450),
                    new GangZone(74, 150, -2550),
                    new GangZone(75, 150, -2650),
                    new GangZone(76, 250, -1250),
                    new GangZone(77, 250, -1350),
                    new GangZone(78, 250, -1450),
                    new GangZone(79, 250, -1550),
                    new GangZone(80, 250, -1650),
                    new GangZone(81, 250, -1750),
                    new GangZone(82, 250, -1850),
                    new GangZone(83, 250, -1950),
                    new GangZone(84, 250, -2050),
                    new GangZone(85, 250, -2150),
                    new GangZone(86, 250, -2250),
                    new GangZone(87, 250, -2350),
                    new GangZone(88, 250, -2450),
                    new GangZone(89, 250, -2550),
                    new GangZone(90, 250, -2650),
                    new GangZone(91, 350, -1250),
                    new GangZone(92, 350, -1350),
                    new GangZone(93, 350, -1450),
                    new GangZone(94, 350, -1550),
                    new GangZone(95, 350, -1650),
                    new GangZone(96, 350, -1750),
                    new GangZone(97, 350, -1850),
                    new GangZone(98, 350, -1950),
                    new GangZone(99, 350, -2050),
                    new GangZone(100, 350, -2150),
                    new GangZone(101, 350, -2250),
                    new GangZone(102, 350, -2350),
                    new GangZone(103, 350, -2450),
                    new GangZone(104, 350, -2550),
                    new GangZone(105, 350, -2650),
                    new GangZone(106, 450, -1250),
                    new GangZone(107, 450, -1350),
                    new GangZone(108, 450, -1450),
                    new GangZone(109, 450, -1550),
                    new GangZone(110, 450, -1650),
                    new GangZone(111, 450, -1750),
                    new GangZone(112, 450, -1850),
                    new GangZone(113, 450, -1950),
                    new GangZone(114, 450, -2050),
                    new GangZone(115, 450, -2150),
                    new GangZone(116, 450, -2250),
                    new GangZone(117, 450, -2350),
                    new GangZone(118, 450, -2450),
                    new GangZone(119, 450, -2550),
                    new GangZone(120, 450, -2650),
                    new GangZone(121, 550, -1250),
                    new GangZone(122, 550, -1350),
                    new GangZone(123, 550, -1450),
                    new GangZone(124, 550, -1550),
                    new GangZone(125, 550, -1650),
                    new GangZone(126, 550, -1750),
                    new GangZone(127, 550, -1850),
                    new GangZone(128, 550, -1950),
                    new GangZone(129, 550, -2050),
                    new GangZone(130, 550, -2150),
                    new GangZone(131, 550, -2250),
                    new GangZone(132, 550, -2350),
                    new GangZone(133, 550, -2450),
                    new GangZone(134, 550, -2550),
                    new GangZone(135, 550, -2650),
                    new GangZone(136, 650, -1250),
                    new GangZone(137, 650, -1350),
                    new GangZone(138, 650, -1450),
                    new GangZone(139, 650, -1550),
                    new GangZone(140, 650, -1650),
                    new GangZone(141, 650, -1750),
                    new GangZone(142, 650, -1850),
                    new GangZone(143, 650, -1950),
                    new GangZone(144, 650, -2050),
                    new GangZone(145, 650, -2150),
                    new GangZone(146, 650, -2250),
                    new GangZone(147, 650, -2350),
                    new GangZone(148, 650, -2450),
                    new GangZone(149, 650, -2550),
                    new GangZone(150, 650, -2650),
                    new GangZone(151, 750, -1250),
                    new GangZone(152, 750, -1350),
                    new GangZone(153, 750, -1450),
                    new GangZone(154, 750, -1550),
                    new GangZone(155, 750, -1650),
                    new GangZone(156, 750, -1750),
                    new GangZone(157, 750, -1850),
                    new GangZone(158, 750, -1950),
                    new GangZone(159, 750, -2050),
                    new GangZone(160, 750, -2150),
                    new GangZone(161, 750, -2250),
                    new GangZone(162, 750, -2350),
                    new GangZone(163, 750, -2450),
                    new GangZone(164, 750, -2550),
                    new GangZone(165, 750, -2650),
                    new GangZone(166, 850, -1250),
                    new GangZone(167, 850, -1350),
                    new GangZone(168, 850, -1450),
                    new GangZone(169, 850, -1550),
                    new GangZone(170, 850, -1650),
                    new GangZone(171, 850, -1750),
                    new GangZone(172, 850, -1850),
                    new GangZone(173, 850, -1950),
                    new GangZone(174, 850, -2050),
                    new GangZone(175, 850, -2150),
                    new GangZone(176, 850, -2250),
                    new GangZone(177, 850, -2350),
                    new GangZone(178, 850, -2450),
                    new GangZone(179, 850, -2550),
                    new GangZone(180, 850, -2650),
                    new GangZone(181, 950, -1250),
                    new GangZone(182, 950, -1350),
                    new GangZone(183, 950, -1450),
                    new GangZone(184, 950, -1550),
                    new GangZone(185, 950, -1650),
                    new GangZone(186, 950, -1750),
                    new GangZone(187, 950, -1850),
                    new GangZone(188, 950, -1950),
                    new GangZone(189, 950, -2050),
                    new GangZone(190, 950, -2150),
                    new GangZone(191, 950, -2250),
                    new GangZone(192, 950, -2350),
                    new GangZone(193, 950, -2450),
                    new GangZone(194, 950, -2550),
                    new GangZone(195, 950, -2650),
                    new GangZone(196, 1050, -1250),
                    new GangZone(197, 1050, -1350),
                    new GangZone(198, 1050, -1450),
                    new GangZone(199, 1050, -1550),
                    new GangZone(200, 1050, -1650),
                    new GangZone(201, 1050, -1750),
                    new GangZone(202, 1050, -1850),
                    new GangZone(203, 1050, -1950),
                    new GangZone(204, 1050, -2050),
                    new GangZone(205, 1050, -2150),
                    new GangZone(206, 1050, -2250),
                    new GangZone(207, 1050, -2350),
                    new GangZone(208, 1050, -2450),
                    new GangZone(209, 1050, -2550),
                    new GangZone(210, 1050, -2650),
                    new GangZone(211, 1150, -1250),
                    new GangZone(212, 1150, -1350),
                    new GangZone(213, 1150, -1450),
                    new GangZone(214, 1150, -1550),
                    new GangZone(215, 1150, -1650),
                    new GangZone(216, 1150, -1750),
                    new GangZone(217, 1150, -1850),
                    new GangZone(218, 1150, -1950),
                    new GangZone(219, 1150, -2050),
                    new GangZone(220, 1150, -2150),
                    new GangZone(221, 1150, -2250),
                    new GangZone(222, 1150, -2350),
                    new GangZone(223, 1150, -2450),
                    new GangZone(224, 1150, -2550),
                    new GangZone(225, 1150, -2650),
                };
            }
        }
    }
}