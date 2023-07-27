namespace BlaineRP.Server.Game.Casino.Games
{
    public partial class Blackjack
    {
        public class CardData
        {
            public CardTypes CardType { get; set; }

            public byte Value { get; set; }

            public CardData()
            {

            }
        }
    }
}