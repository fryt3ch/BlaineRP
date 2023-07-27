namespace BlaineRP.Server.Game.Casino.Games
{
    public partial class Blackjack
    {
        /// <summary>Типы карт</summary>
        public enum CardTypes : byte
        {
            None = 0,

            Club_Ace,
            Club_02,
            Club_03,
            Club_04,
            Club_05,
            Club_06,
            Club_07,
            Club_08,
            Club_09,
            Club_10,
            Club_Jack,
            Club_Queen,
            Club_King,

            Dia_Ace,
            Dia_02,
            Dia_03,
            Dia_04,
            Dia_05,
            Dia_06,
            Dia_07,
            Dia_08,
            Dia_09,
            Dia_10,
            Dia_Jack,
            Dia_Queen,
            Dia_King,

            Hrt_Ace,
            Hrt_02,
            Hrt_03,
            Hrt_04,
            Hrt_05,
            Hrt_06,
            Hrt_07,
            Hrt_08,
            Hrt_09,
            Hrt_10,
            Hrt_Jack,
            Hrt_Queen,
            Hrt_King,

            Spd_Ace,
            Spd_02,
            Spd_03,
            Spd_04,
            Spd_05,
            Spd_06,
            Spd_07,
            Spd_08,
            Spd_09,
            Spd_10,
            Spd_Jack,
            Spd_Queen,
            Spd_King,
        }
    }
}