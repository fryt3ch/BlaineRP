using System.Collections.Generic;

namespace BlaineRP.Server.Game.Casino.Games
{
    public partial class Blackjack
    {
        /// <summary>Время (в мс.) ожидания во время фазы ставок</summary>
        public const int BET_WAIT_TIME = 5_000;
        /// <summary>Время (в мс.) ожидания во время хода игрока</summary>
        public const int DECISION_WAIT_TIME = 15_000;
        /// <summary>Время (в мс.) раздачи 1 ед. карт</summary>
        private const int CARD_GIVE_TIME = 1_750;

        /// <summary>Коэфициент выплаты игроку при блэкджеке</summary>
        private const decimal BLACKJACK_COEF = 3m;
        /// <summary>Коэфициент выплаты игроку при победе</summary>
        private const decimal WIN_COEF = 2m;

        /// <summary>Кол-во колод для каждой игры</summary>
        /// <remarks>0 - бесконечное кол-во</remarks>
        public const byte DECKS_PER_GAME = 0;

        /// <summary>Сумма значений карт, при достижении которой дилер перестает брать новые по завершению игры</summary>
        private const byte DEALER_STOPS_ON = 17;
        /// <summary>Сумма значений карт, при превышении которой следует проигрыш</summary>
        private const byte LOOSE_AFTER = 21;
        /// <summary>Сумма значений карт, дающей блэкджек</summary>
        /// <remarks>Имеет значение только при раздаче карт</remarks>
        private const byte BLACKJACK_ON = 21;
        
        /// <summary>Базовые значения карт</summary>
        /// <remarks>По правилам, если игрок получает туз и сумма его карт превысит 21, то туз считается не за 11, а за 1</remarks>
        private static Dictionary<CardTypes, byte> CardValues = new Dictionary<CardTypes, byte>()
        {
            { CardTypes.Club_Ace, 11 },
            { CardTypes.Club_02, 2 },
            { CardTypes.Club_03, 3 },
            { CardTypes.Club_04, 4 },
            { CardTypes.Club_05, 5 },
            { CardTypes.Club_06, 6 },
            { CardTypes.Club_07, 7 },
            { CardTypes.Club_08, 8 },
            { CardTypes.Club_09, 9 },
            { CardTypes.Club_10, 10 },
            { CardTypes.Club_Jack, 10 },
            { CardTypes.Club_Queen, 10 },
            { CardTypes.Club_King, 10 },

            { CardTypes.Dia_Ace, 11 },
            { CardTypes.Dia_02, 2 },
            { CardTypes.Dia_03, 3 },
            { CardTypes.Dia_04, 4 },
            { CardTypes.Dia_05, 5 },
            { CardTypes.Dia_06, 6 },
            { CardTypes.Dia_07, 7 },
            { CardTypes.Dia_08, 8 },
            { CardTypes.Dia_09, 9 },
            { CardTypes.Dia_10, 10 },
            { CardTypes.Dia_Jack, 10 },
            { CardTypes.Dia_Queen, 10 },
            { CardTypes.Dia_King, 10 },

            { CardTypes.Hrt_Ace, 11 },
            { CardTypes.Hrt_02, 2 },
            { CardTypes.Hrt_03, 3 },
            { CardTypes.Hrt_04, 4 },
            { CardTypes.Hrt_05, 5 },
            { CardTypes.Hrt_06, 6 },
            { CardTypes.Hrt_07, 7 },
            { CardTypes.Hrt_08, 8 },
            { CardTypes.Hrt_09, 9 },
            { CardTypes.Hrt_10, 10 },
            { CardTypes.Hrt_Jack, 10 },
            { CardTypes.Hrt_Queen, 10 },
            { CardTypes.Hrt_King, 10 },

            { CardTypes.Spd_Ace, 11 },
            { CardTypes.Spd_02, 2 },
            { CardTypes.Spd_03, 3 },
            { CardTypes.Spd_04, 4 },
            { CardTypes.Spd_05, 5 },
            { CardTypes.Spd_06, 6 },
            { CardTypes.Spd_07, 7 },
            { CardTypes.Spd_08, 8 },
            { CardTypes.Spd_09, 9 },
            { CardTypes.Spd_10, 10 },
            { CardTypes.Spd_Jack, 10 },
            { CardTypes.Spd_Queen, 10 },
            { CardTypes.Spd_King, 10 },
        };
    }
}