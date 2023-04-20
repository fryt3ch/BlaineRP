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
    public class Mafia : Fraction
    {
        public Mafia(Types Type, string Name) : base(Type, Name)
        {

        }

        public override string ClientData => $"Fractions.Types.{Type}, \"{Name}\", {ContainerId}, \"{ContainerPositions.SerializeToJson().Replace('\"', '\'')}\", \"{CreationWorkbenchPositions.SerializeToJson().Replace('\"', '\'')}\", {Ranks.Count - 1}, \"{CreationWorkbenchPrices.SerializeToJson().Replace('"', '\'')}\"";

        protected override void FractionDataTriggerEvent(PlayerData pData)
        {
            pData.Player.TriggerEvent("Player::SCF", (int)Type, News.SerializeToJson(), AllVehicles.Select(x => $"{x.Key.VID}&{x.Key.VID}&{x.Value.MinimalRank}").ToList(), AllMembers.Select(x => $"{x.CID}&{x.Name} {x.Surname}&{x.FractionRank}&{(x.IsOnline ? 1 : 0)}&{GetMemberStatus(x)}&{x.LastJoinDate.GetUnixTimestamp()}").ToList());
        }
    }
}