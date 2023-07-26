using System;

namespace BlaineRP.Client.Game.Fractions
{
    public partial class Police
    {
        public class ArrestInfo
        {
            public uint Id { get; set; }

            public DateTime Time { get; set; }

            public string TargetName { get; set; }
            public string MemberName { get; set; }

            public ArrestInfo()
            {
            }
        }
    }
}