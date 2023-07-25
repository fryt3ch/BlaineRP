using System;

namespace BlaineRP.Client.Game.Fractions
{
    public class MemberData
    {
        public bool IsOnline { get; set; }

        public byte SubStatus { get; set; }

        public string Name { get; set; }

        public byte Rank { get; set; }

        public DateTime LastSeenDate { get; set; }
    }
}