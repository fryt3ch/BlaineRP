using System;

namespace BlaineRP.Client.Game.Fractions
{
    public partial class Police
    {
        public class APBInfo
        {
            public uint Id { get; set; }

            public string TargetName { get; set; }

            public string Details { get; set; }

            public string Member { get; set; }

            public DateTime Time { get; set; }

            public APBInfo()
            {
            }
        }
    }
}