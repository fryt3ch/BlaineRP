namespace BlaineRP.Server.Game.Casino.Games
{
    public partial class Roulette
    {
        public class BetData
        {
            public class Bet
            {
                public BetTypes Type { get; set; }

                public uint Amount { get; set; }
            }

            public Bet[] Bets { get; set; } = new Bet[MAX_BETS];
        }
    }
}