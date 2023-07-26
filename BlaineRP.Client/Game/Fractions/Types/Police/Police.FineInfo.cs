using System;

namespace BlaineRP.Client.Game.Fractions
{
    public partial class Police
    {
        public class FineInfo
        {
            public string Member { get; set; }

            public string Target { get; set; }

            public uint Amount { get; set; }

            public string Reason { get; set; }

            public DateTime Time { get; set; }

            public FineInfo()
            {
            }
        }
    }
}