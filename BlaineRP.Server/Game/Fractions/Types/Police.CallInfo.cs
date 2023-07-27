using System;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Fractions
{
    public partial class Police
    {
        public class CallInfo
        {
            public byte Type { get; set; }

            public Vector3 Position { get; set; }

            public string Message { get; set; }

            public DateTime Time { get; set; }

            public FractionType FractionType { get; set; }

            public CallInfo()
            {

            }
        }
    }
}