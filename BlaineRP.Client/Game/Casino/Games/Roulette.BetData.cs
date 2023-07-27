using RAGE.Elements;

namespace BlaineRP.Client.Game.Casino.Games
{
    public partial class Roulette
    {
        public class BetData
        {
            public BetType BetType { get; set; }

            public uint Amount { get; set; }

            public MapObject MapObject { get; set; }
        }
    }
}