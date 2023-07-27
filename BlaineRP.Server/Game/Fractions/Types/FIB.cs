using BlaineRP.Server.Game.Data;
using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.Extensions.System;

namespace BlaineRP.Server.Game.Fractions
{
    public class FIB : Fraction, IUniformable
    {
        public FIB(FractionType Type, string Name) : base(Type, Name)
        {

        }

        public override string ClientData => $"FractionTypes.{Type}, \"{Name}\", {ContainerId}, \"{ContainerPositions.SerializeToJson().Replace('\"', '\'')}\", \"{CreationWorkbenchPositions.SerializeToJson().Replace('\"', '\'')}\", {Ranks.Count - 1}, \"{LockerRoomPositions.SerializeToJson().Replace('\"', '\'')}\", \"{CreationWorkbenchPrices.SerializeToJson().Replace('"', '\'')}\", {(uint)MetaFlags}";

        public List<Customization.UniformTypes> UniformTypes { get; set; }

        public Vector3[] LockerRoomPositions { get; set; }

        public bool IsPlayerInAnyUniform(PlayerData pData, bool notifyIfNot = false)
        {
            var res = UniformTypes.Contains(pData.CurrentUniform);

            if (res)
                return true;

            if (notifyIfNot)
            {
                pData.Player.Notify("Fraction::NIUF");
            }

            return false;
        }

        protected override void FractionDataTriggerEvent(PlayerData pData)
        {
            pData.Player.TriggerEvent("Player::SCF", (int)Type, News.SerializeToJson(), AllVehicles.Select(x => $"{x.Key.VID}&{x.Key.VID}&{x.Value.MinimalRank}").ToList(), AllMembers.Select(x => $"{x.CID}&{x.Name} {x.Surname}&{x.FractionRank}&{(x.IsOnline ? 1 : 0)}&{GetMemberStatus(x)}&{x.LastJoinDate.GetUnixTimestamp()}").ToList());
        }
    }
}