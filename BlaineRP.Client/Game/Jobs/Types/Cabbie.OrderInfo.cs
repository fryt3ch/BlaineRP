using RAGE;

namespace BlaineRP.Client.Game.Jobs
{
    public partial class Cabbie
    {
        public class OrderInfo
        {
            public OrderInfo()
            {
            }

            public uint Id { get; set; }

            public Vector3 Position { get; set; }
        }
    }
}