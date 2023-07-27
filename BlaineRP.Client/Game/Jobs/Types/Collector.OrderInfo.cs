using BlaineRP.Client.Game.Businesses;

namespace BlaineRP.Client.Game.Jobs
{
    public partial class Collector
    {
        public class OrderInfo
        {
            public OrderInfo()
            {
            }

            public uint Id { get; set; }

            public uint Reward { get; set; }

            public Business TargetBusiness { get; set; }
        }
    }
}