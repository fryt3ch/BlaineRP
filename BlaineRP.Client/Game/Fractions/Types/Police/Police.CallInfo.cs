using System;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Fractions
{
    public partial class Police
    {
        public class CallInfo
        {
            public CallInfo()
            {
            }

            public byte Type { get; set; }

            public Player Player { get; set; }

            public Vector3 Position { get; set; }

            public string Message { get; set; }

            public DateTime Time { get; set; }
        }
    }
}