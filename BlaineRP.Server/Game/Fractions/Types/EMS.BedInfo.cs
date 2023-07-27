using System.Threading;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Fractions
{
    public partial class EMS
    {
        public class BedInfo
        {
            public ushort RID { get; set; }

            public Timer Timer { get; set; }

            public Vector3 Position { get; set; }

            public BedInfo()
            {

            }
        }
    }
}