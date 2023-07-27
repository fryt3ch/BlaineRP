using GTANetworkAPI;

namespace BlaineRP.Server.Game.Fractions
{
    public partial class EMS
    {
        public class CallInfo
        {
            public byte Type { get; set; }

            public Vector3 Position { get; set; }

            public CallInfo()
            {

            }
        }
    }
}