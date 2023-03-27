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

        public override string ClientData => $"Fractions.Types.{Type}, \"{Name}\", {ContainerId}, {ContainerPosition.ToCSharpStr()}, {CreationWorkbenchPosition.ToCSharpStr()}, {Ranks.Count - 1}, {LockerRoomPosition.ToCSharpStr()}, \"{CreationWorkbenchPrices.SerializeToJson().Replace('"', '\'')}\"";

        public List<Customization.UniformTypes> UniformTypes { get; set; }

        public Vector3 LockerRoomPosition { get; set; }

        public bool IsPlayerInAnyUniform(PlayerData pData)
        {
            if (pData.CurrentUniform is Customization.UniformTypes uType)
                return UniformTypes.Contains(uType);

            return false;
        }
    }
}
