using RAGE.Elements;

namespace BlaineRP.Client.Game.Casino.Games
{
    public partial class Blackjack
    {
        public class CardData
        {
            public CardType CardType { get; set; }

            public byte Value { get; set; }

            public MapObject MapObject { get; set; }
        }
    }
}