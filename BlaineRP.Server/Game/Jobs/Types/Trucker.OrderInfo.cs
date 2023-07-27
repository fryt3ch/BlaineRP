using BlaineRP.Server.EntitiesData.Players;

namespace BlaineRP.Server.Game.Jobs
{
    public partial class Trucker
    {
        public class OrderInfo
        {
            public bool IsCustom => Reward >= MinimalRewardX;

            public PlayerInfo CurrentWorker { get; set; }

            public uint Reward { get; set; }

            public int MPIdx { get; set; }

            public Game.Businesses.Business TargetBusiness { get; set; }

            public OrderInfo(Game.Businesses.Business TargetBusiness)
            {
                this.TargetBusiness = TargetBusiness;
            }
        }
    }
}