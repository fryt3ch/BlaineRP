﻿using System.Linq;
using BlaineRP.Server.Extensions.System;
using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Game.Fractions
{
    public class Mafia : Fraction
    {
        public Mafia(FractionType Type, string Name) : base(Type, Name)
        {

        }

        public override string ClientData => $"FractionTypes.{Type}, \"{Name}\", {ContainerId}, \"{ContainerPositions.SerializeToJson().Replace('\"', '\'')}\", \"{CreationWorkbenchPositions.SerializeToJson().Replace('\"', '\'')}\", {Ranks.Count - 1}, \"{CreationWorkbenchPrices.SerializeToJson().Replace('"', '\'')}\", {(uint)MetaFlags}";

        protected override void FractionDataTriggerEvent(PlayerData pData)
        {
            pData.Player.TriggerEvent("Player::SCF", (int)Type, News.SerializeToJson(), AllVehicles.Select(x => $"{x.Key.VID}&{x.Key.VID}&{x.Value.MinimalRank}").ToList(), AllMembers.Select(x => $"{x.CID}&{x.Name} {x.Surname}&{x.FractionRank}&{(x.IsOnline ? 1 : 0)}&{GetMemberStatus(x)}&{x.LastJoinDate.GetUnixTimestamp()}").ToList());
        }
    }
}