using System.Collections.Generic;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Players.Customization.Clothes.Uniforms;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Fractions
{
    public interface IUniformable
    {
        public List<UniformTypes> UniformTypes { get; set; }

        public Vector3[] LockerRoomPositions { get; set; }

        public bool IsPlayerInAnyUniform(PlayerData pData, bool notifyIfNot = false);
    }
}