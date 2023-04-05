using BCRPServer.Game.Data;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Fractions
{
    public class Police : Fraction, IUniformable
    {
        public Police(Types Type, string Name) : base(Type, Name)
        {

        }

        public override string ClientData => $"Fractions.Types.{Type}, \"{Name}\", {ContainerId}, {ContainerPosition.ToCSharpStr()}, {CreationWorkbenchPosition.ToCSharpStr()}, {Ranks.Count - 1}, {LockerRoomPosition.ToCSharpStr()}, \"{CreationWorkbenchPrices.SerializeToJson().Replace('"', '\'')}\", \"{ArrestCellsPositions.SerializeToJson().Replace('"', '\'')}\", {ArrestMenuPosition.ToCSharpStr()}";

        public static Dictionary<string, uint[]> NumberplatePrices { get; private set; } = new Dictionary<string, uint[]>()
        {
            {
                "np_0",

                new uint[]
                {
                    500_000, 450_000, 400_000, 150_000, 50_000, 15_000, 5_500, 1_500,
                }
            },

            {
                "np_1",

                new uint[]
                {
                    500_000, 450_000, 400_000, 150_000, 50_000, 15_000, 5_500, 1_500,
                }
            },

            {
                "np_2",

                new uint[]
                {
                    500_000, 450_000, 400_000, 150_000, 50_000, 15_000, 5_500, 1_500,
                }
            },

            {
                "np_3",

                new uint[]
                {
                    500_000, 450_000, 400_000, 150_000, 50_000, 15_000, 5_500, 1_500,
                }
            },

            {
                "np_4",

                new uint[]
                {
                    500_000, 450_000, 400_000, 150_000, 50_000, 15_000, 5_500, 1_500,
                }
            },
        };

        public static uint VehicleNumberplateRegPrice { get; set; } = 1_000;
        public static uint VehicleNumberplateUnRegPrice { get; set; } = 500;

        public List<Customization.UniformTypes> UniformTypes { get; set; }

        public Vector3 LockerRoomPosition { get; set; }

        public Utils.Vector4[] ArrestCellsPositions { get; set; }

        public Vector3 ArrestMenuPosition { get; set; }

        public Utils.Vector4 ArrestFreePosition { get; set; }

        private static int LastArrestCellPositionUsed { get; set; }

        public bool IsPlayerInAnyUniform(PlayerData pData)
        {
            if (pData.CurrentUniform is Customization.UniformTypes uType)
                return UniformTypes.Contains(uType);

            return false;
        }

        public Vector3 GetNextArrestCellPosition()
        {
            var pos = LastArrestCellPositionUsed >= ArrestCellsPositions.Length ? ArrestCellsPositions[LastArrestCellPositionUsed = 0] : ArrestCellsPositions[LastArrestCellPositionUsed++];

            return new Vector3(pos.X, pos.Y, pos.Z);
        }

        public void SetPlayerToPrison(PlayerData pData, bool justTeleport)
        {
            if (!justTeleport)
            {
                Utils.RemoveAllWeapons(pData, true, true);
            }

            var pos = GetNextArrestCellPosition();

            pData.Player.Teleport(pos, false, Utils.Dimensions.Main, null, false);
        }

        public void SetPlayerFromPrison(PlayerData pData)
        {
            var pos = new Utils.Vector4(ArrestFreePosition.X, ArrestFreePosition.Y, ArrestFreePosition.Z, ArrestFreePosition.RotationZ);

            pData.Player.Teleport(pos.Position, false, Utils.Dimensions.Main, pos.RotationZ, false);
        }
    }
}
