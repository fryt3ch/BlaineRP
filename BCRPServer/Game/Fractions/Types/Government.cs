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
    public class Government : Fraction, IUniformable
    {
        public Government(Types Type, string Name) : base(Type, Name)
        {

        }

        public override string ClientData => $"Fractions.Types.{Type}, \"{Name}\", {ContainerId}, \"{ContainerPositions.SerializeToJson().Replace('\"', '\'')}\", \"{CreationWorkbenchPositions.SerializeToJson().Replace('\"', '\'')}\", {Ranks.Count - 1}, \"{LockerRoomPositions.SerializeToJson().Replace('\"', '\'')}\", \"{CreationWorkbenchPrices.SerializeToJson().Replace('"', '\'')}\"";

        public List<Customization.UniformTypes> UniformTypes { get; set; }

        public Vector3[] LockerRoomPositions { get; set; }

        public bool IsPlayerInAnyUniform(PlayerData pData)
        {
            if (pData.CurrentUniform is Customization.UniformTypes uType)
                return UniformTypes.Contains(uType);

            return false;
        }

        protected override void FractionDataTriggerEvent(PlayerData pData)
        {
            pData.Player.TriggerEvent("Player::SCF", (int)Type, News.SerializeToJson(), AllVehicles.Select(x => $"{x.Key.VID}&{x.Key.VID}&{x.Value.MinimalRank}").ToList(), AllMembers.Select(x => $"{x.CID}&{x.Name} {x.Surname}&{x.FractionRank}&{(x.IsOnline ? 1 : 0)}&{GetMemberStatus(x)}&{x.LastJoinDate.GetUnixTimestamp()}").ToList());
        }
    }
}