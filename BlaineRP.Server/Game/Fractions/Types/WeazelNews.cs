using System.Linq;

namespace BlaineRP.Server.Game.Fractions
{
    // -556.5839f, -924.9739f, 23.88925f, 269.3907f
    public class WeazelNews : Fraction
    {
        public WeazelNews(Types Type, string Name) : base(Type, Name)
        {

        }

        public override string ClientData => $"FractionTypes.{Type}, \"{Name}\", {ContainerId}, \"{ContainerPositions.SerializeToJson().Replace('\"', '\'')}\", \"{CreationWorkbenchPositions.SerializeToJson().Replace('\"', '\'')}\", {Ranks.Count - 1}, \"{CreationWorkbenchPrices.SerializeToJson().Replace('\"', '\'')}\", {(uint)MetaFlags}";

        protected override void FractionDataTriggerEvent(PlayerData pData)
        {
            pData.Player.TriggerEvent("Player::SCF", (int)Type, News.SerializeToJson(), AllVehicles.Select(x => $"{x.Key.VID}&{x.Key.VID}&{x.Value.MinimalRank}").ToList(), AllMembers.Select(x => $"{x.CID}&{x.Name} {x.Surname}&{x.FractionRank}&{(x.IsOnline ? 1 : 0)}&{GetMemberStatus(x)}&{x.LastJoinDate.GetUnixTimestamp()}").ToList());
        }
    }
}