using BlaineRP.Client.Game.Businesses;

namespace BlaineRP.Client.Game.Jobs
{
    public partial class Trucker
    {
        public class OrderInfo
        {
            public OrderInfo()
            {
            }

            public uint Id { get; set; }

            public uint Reward { get; set; }

            public int MPIdx { get; set; }

            public Business TargetBusiness { get; set; }
        }
    }
}