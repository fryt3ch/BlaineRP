using System.Collections.Generic;

namespace BlaineRP.Server.Game.Casino.Games
{
    public partial class Blackjack
    {
        public class SeatData
        {
            public uint CID { get; set; }

            public uint Bet { get; set; }

            public List<CardData> Hand { get; set; }

            public SeatData()
            {

            }
        }
    }
}