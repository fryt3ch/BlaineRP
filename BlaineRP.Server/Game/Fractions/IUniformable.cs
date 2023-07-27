using System.Collections.Generic;
using BlaineRP.Server.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Fractions
{
    public interface IUniformable
    {
        public List<Game.Data.Customization.UniformTypes> UniformTypes { get; set; }

        public Vector3[] LockerRoomPositions { get; set; }

        public bool IsPlayerInAnyUniform(PlayerData pData, bool notifyIfNot = false);
    }
}