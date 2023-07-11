using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BCRPServer.Game.Casino
{
    public class Blackjack
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

        /// <summary>Является ли карта тузом?</summary>
        /// <param name="cType">Тип карты</param>
        public static bool IsCardTypeAce(CardTypes cType) => cType == CardTypes.Club_Ace || cType == CardTypes.Dia_Ace || cType == CardTypes.Hrt_Ace || cType == CardTypes.Spd_Ace;

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

        public class CardData
        {
            public CardTypes CardType { get; set; }

            public byte Value { get; set; }

            public CardData()
            {

            }
        }

        public class SeatData
        {
            public uint CID { get; set; }

            public uint Bet { get; set; }

            public List<CardData> Hand { get; set; }

            public SeatData()
            {

            }
        }

        public SeatData[] CurrentPlayers { get; set; }

        public uint MinBet { get; set; }

        public uint MaxBet { get; set; }

        public Vector3 Position { get; set; }

        public Timer Timer { get; set; }

        private string CurrentStateData { get; set; }

        private List<CardData> DealerHand { get; set; }

        private List<CardTypes> CurrentDeck { get; set; }

        private static CardTypes[] UnlimitedDeck { get; } = (CardTypes[])Enum.GetValues(typeof(CardTypes));

        private CardData DealerHiddenCard { get; set; }

        public readonly byte CasinoId;
        public readonly byte Id;

        public Blackjack(byte CasinoId, byte Id, float PosX, float PosY, float PosZ, byte MaxPlayers)
        {
            this.Position = new Vector3(PosX, PosY, PosZ);

            this.CurrentPlayers = new SeatData[MaxPlayers];

            this.CasinoId = CasinoId;
            this.Id = Id;
        }

        public void SetCurrentStateData(string value)
        {
            CurrentStateData = value;

            Utils.TriggerEventInDistance(Position, Settings.MAIN_DIMENSION, 50f, "Casino::BLJS", CasinoId, Id, value);
        }

        public string GetCurrentStateData()
        {
            return CurrentStateData;
        }

        public void StartGame()
        {
            Timer?.Dispose();

            var startDate = Utils.GetCurrentTime().AddMilliseconds(BET_WAIT_TIME);

            SetCurrentStateData($"S{startDate.GetUnixTimestamp()}*");

            if (DECKS_PER_GAME > 0)
            {
                CurrentDeck = GetNewDeck(DECKS_PER_GAME);
            }

            DealerHand = new List<CardData>();

            Timer = new Timer((obj) =>
            {
                NAPI.Task.Run(() =>
                {
                    Timer?.Dispose();

                    var strBuilder = new StringBuilder("R*");

                    byte dealerHandSum = 0;

                    for (int i = 0; i < 2; i++)
                    {
                        var cardType = GetNextCardFromDeck();

                        var value = CardValues.GetValueOrDefault(cardType);

                        if (i == 1 && IsCardTypeAce(cardType) && (dealerHandSum + value > LOOSE_AFTER))
                            value = 1;

                        DealerHand.Add(new CardData() { CardType = cardType, Value = value});

                        CurrentDeck?.Remove(cardType);

                        dealerHandSum += value;
                    }

                    var dealerBlackjack = dealerHandSum == BLACKJACK_ON;

                    var dealerCheckCard = false;

                    string dealerStr = null;

                    if (dealerBlackjack)
                    {
                        dealerStr = $"{(byte)DealerHand[0].CardType}-{DealerHand[0].Value}!{(byte)DealerHand[1].CardType}-{DealerHand[1].Value}";
                    }
                    else
                    {
                        dealerStr = $"{(byte)DealerHand[0].CardType}-{DealerHand[0].Value}!0-0";

                        DealerHiddenCard = new CardData() { CardType = DealerHand[1].CardType, Value = DealerHand[1].Value };

                        DealerHand[1].CardType = CardTypes.None;
                        DealerHand[1].Value = 0;

                        if (DealerHand[0].Value == 10 || DealerHand[0].Value == 11)
                            dealerCheckCard = true;
                    }

                    strBuilder.Append(dealerStr);

                    var blackjackPlayers = new List<int>();

                    var playersCount = 0;

                    for (int j = 0; j < CurrentPlayers.Length; j++)
                    {
                        var x = CurrentPlayers[j];

                        if (x == null || x.Bet <= 0)
                        {
                            strBuilder.Append('*');

                            continue;
                        }

                        playersCount++;

                        x.Hand = new List<CardData>();

                        byte handSum = 0;

                        for (int i = 0; i < 2; i++)
                        {
                            var cardType = GetNextCardFromDeck();

                            var value = CardValues.GetValueOrDefault(cardType);

                            if (i == 1 && IsCardTypeAce(cardType) && (handSum + value > LOOSE_AFTER))
                                value = 1;

                            x.Hand.Add(new CardData() { CardType = cardType, Value = value });

                            handSum += value;

                            CurrentDeck?.Remove(cardType);
                        }

                        if (handSum == BLACKJACK_ON)
                        {
                            blackjackPlayers.Add(j);
                        }

                        var playerStr = $"{(byte)x.Hand[0].CardType}-{x.Hand[0].Value}!{(byte)x.Hand[1].CardType}-{x.Hand[1].Value}";

                        strBuilder.Append($"*{playerStr}");
                    }

                    SetCurrentStateData(strBuilder.ToString());

                    Timer = new Timer((obj) =>
                    {
                        NAPI.Task.Run(() =>
                        {
                            Timer?.Dispose();

                            if (dealerBlackjack)
                            {
                                foreach (var x in blackjackPlayers)
                                {
                                    var pInfo = PlayerData.PlayerInfo.Get(CurrentPlayers[x].CID);

                                    if (pInfo == null)
                                        continue;

                                    uint chipBalance;

                                    if (Casino.TryAddCasinoChips(pInfo, CurrentPlayers[x].Bet, out chipBalance, true, null))
                                    {
                                        Casino.SetCasinoChips(pInfo, chipBalance, null);
                                    }
                                }

                                for (int i = 0; i < CurrentPlayers.Length; i++)
                                {
                                    var x = CurrentPlayers[i];

                                    if (x == null)
                                        continue;

                                    CurrentPlayers[i].Bet = 0;
                                    CurrentPlayers[i].Hand = null;
                                }

                                if (CurrentDeck != null)
                                    CurrentDeck = null;

                                if (DealerHiddenCard != null)
                                    DealerHiddenCard = null;

                                if (DealerHand != null)
                                    DealerHand = null;

                                Timer = null;

                                SetCurrentStateData("I");

                                KickAllWrongPlayers();
                            }
                            else
                            {
                                foreach (var x in blackjackPlayers)
                                {
                                    var pInfo = PlayerData.PlayerInfo.Get(CurrentPlayers[x].CID);

                                    if (pInfo == null)
                                        continue;

                                    uint chipBalance;

                                    if (Casino.TryAddCasinoChips(pInfo, (uint)Math.Floor(CurrentPlayers[x].Bet * BLACKJACK_COEF), out chipBalance, true, null))
                                    {
                                        Casino.SetCasinoChips(pInfo, chipBalance, null);
                                    }

                                    CurrentPlayers[x].Bet = 0;
                                    CurrentPlayers[x].Hand = null;
                                }

                                SetPlayerToDecisionState(0);
                            }
                        });
                    }, null, 500 + CARD_GIVE_TIME * 2 + playersCount * CARD_GIVE_TIME * 2, Timeout.Infinite);
                });
            }, null, BET_WAIT_TIME, Timeout.Infinite);
        }

        public void SetPlayerToDecisionState(byte seatIdx)
        {
            Timer?.Dispose();

            if (seatIdx >= CurrentPlayers.Length)
            {
                FinishGame();

                return;
            }
            
            if (CurrentPlayers[seatIdx] == null || CurrentPlayers[seatIdx].Bet <= 0 || CurrentPlayers[seatIdx].Hand == null)
            {
                SetPlayerToDecisionState((byte)(seatIdx + 1));

                return;
            }

            var startDate = Utils.GetCurrentTime().AddMilliseconds(DECISION_WAIT_TIME);

            var curCardsStrBuilder = GetCurrentCardsStrBuilder();

            curCardsStrBuilder.Insert(0, $"D*{seatIdx}*{startDate.GetUnixTimestamp()}");

            SetCurrentStateData(curCardsStrBuilder.ToString());

            Timer = new Timer((obj) =>
            {
                NAPI.Task.Run(() =>
                {
                    Timer?.Dispose();

                    SetPlayerToDecisionState((byte)(seatIdx + 1));
                });
            }, null, DECISION_WAIT_TIME, Timeout.Infinite);
        }

        private void FinishGame()
        {
            Timer?.Dispose();

            if (DealerHiddenCard != null)
                DealerHand[1] = DealerHiddenCard;

            var handSum = DealerHand[0].Value + DealerHand[1].Value;

            var anyPlayerLeft = false;

            for (int i = 0; i < CurrentPlayers.Length; i++)
            {
                if (CurrentPlayers[i] != null && CurrentPlayers[i].Hand != null)
                {
                    anyPlayerLeft = true;

                    break;
                }
            }

            if (anyPlayerLeft)
            {
                while (handSum < DEALER_STOPS_ON)
                {
                    var cardType = GetNextCardFromDeck();

                    var value = CardValues.GetValueOrDefault(cardType);

                    DealerHand.Add(new CardData() { CardType = cardType, Value = value });

                    CurrentDeck?.Remove(cardType);

                    handSum += value;
                }
            }

            var curCardsStrBuilder = GetCurrentCardsStrBuilder();

            curCardsStrBuilder.Insert(0, $"F");

            SetCurrentStateData(curCardsStrBuilder.ToString());

            var cardsAdded = DealerHand.Count - 2;

            Timer = new Timer((obj) =>
            {
                NAPI.Task.Run(() =>
                {
                    Timer?.Dispose();

                    var totalBetted = 0m;
                    var totalPayed = 0m;

                    for (int i = 0; i < CurrentPlayers.Length; i++)
                    {
                        var x = CurrentPlayers[i];

                        if (x == null)
                            continue;

                        var pInfo = PlayerData.PlayerInfo.Get(x.CID);

                        if (pInfo == null)
                            continue;

                        if (x.Bet > 0)
                        {
                            totalBetted += x.Bet;

                            var pHandsum = x.Hand.Select(x => (int)x.Value).Sum();

                            uint totalWin = 0;

                            if (handSum <= LOOSE_AFTER)
                            {
                                if (pHandsum > handSum)
                                {
                                    totalWin = (uint)Math.Floor(x.Bet * WIN_COEF);
                                }
                                else if (pHandsum == handSum)
                                {
                                    totalWin = x.Bet;
                                }
                                else
                                {

                                }
                            }
                            else
                            {
                                totalWin = (uint)Math.Floor(x.Bet * WIN_COEF);
                            }

                            if (totalWin > 0)
                            {
                                uint newBalance;

                                if (Casino.TryAddCasinoChips(pInfo, totalWin, out newBalance, true, null))
                                {
                                    Casino.SetCasinoChips(pInfo, newBalance, null);
                                }

                                totalPayed += totalWin;
                            }
                        }

                        x.Bet = 0;
                        x.Hand = null;
                    }

                    if (CurrentDeck != null)
                        CurrentDeck = null;

                    if (DealerHiddenCard != null)
                        DealerHiddenCard = null;

                    if (DealerHand != null)
                        DealerHand = null;

                    Timer = null;

                    SetCurrentStateData("I");

                    KickAllWrongPlayers();
                });
            }, null, cardsAdded * CARD_GIVE_TIME + 4000, -1);
        }

        public void OnPlayerChooseAnother(byte seatIdx)
        {
            Timer?.Dispose();

            var handSum = CurrentPlayers[seatIdx].Hand.Select(x => (int)x.Value).Sum();

            var cardType = GetNextCardFromDeck();

            var value = CardValues.GetValueOrDefault(cardType);

            if (handSum + value > LOOSE_AFTER && IsCardTypeAce(cardType))
            {
                value = 1;
            }

            var cardData = new CardData() { CardType = cardType, Value = value };

            CurrentPlayers[seatIdx].Hand.Add(cardData);

            handSum += value;

            CurrentDeck?.Remove(cardType);

            var curCardsStrBuilder = GetCurrentCardsStrBuilder();

            curCardsStrBuilder.Insert(0, $"H*{seatIdx}*0");

            SetCurrentStateData(curCardsStrBuilder.ToString());

            Timer = new Timer((obj) =>
            {
                NAPI.Task.Run(() =>
                {
                    Timer?.Dispose();

                    if (handSum > LOOSE_AFTER)
                    {
                        CurrentPlayers[seatIdx].Bet = 0;
                        CurrentPlayers[seatIdx].Hand = null;

                        curCardsStrBuilder = GetCurrentCardsStrBuilder();

                        curCardsStrBuilder.Insert(0, $"L*{seatIdx}*0");

                        SetCurrentStateData(curCardsStrBuilder.ToString());

                        Timer = new Timer((obj) =>
                        {
                            NAPI.Task.Run(() =>
                            {
                                Timer?.Dispose();

                                SetPlayerToDecisionState((byte)(seatIdx + 1));
                            });
                        }, null, 2_500, -1);
                    }
                    else if (handSum == LOOSE_AFTER)
                    {
                        SetPlayerToDecisionState((byte)(seatIdx + 1));
                    }
                    else
                    {
                        SetPlayerToDecisionState(seatIdx);
                    }
                });
            }, null, CARD_GIVE_TIME + 500, Timeout.Infinite);
        }

        public StringBuilder GetCurrentCardsStrBuilder()
        {
            var strBuilder = new StringBuilder();

            var dealerHand = DealerHand.Select(x => $"{(byte)x.CardType}-{x.Value}").ToList();

            if (dealerHand.Count == 1)
                dealerHand.Add("0-0");

            strBuilder.Append('*');
            strBuilder.Append(string.Join('!', dealerHand));

            for (int i = 0; i < CurrentPlayers.Length; i++)
            {
                var x = CurrentPlayers[i];

                if (x == null || x.Hand == null)
                {
                    strBuilder.Append('*');

                    continue;
                }

                var hand = x.Hand.Select(x => $"{(byte)x.CardType}-{x.Value}").ToList();

                strBuilder.Append('*');
                strBuilder.Append(string.Join('!', hand));
            }

            return strBuilder;
        }

        public CardTypes GetNextCardFromDeck()
        {
            if (DECKS_PER_GAME == 0)
            {
                return UnlimitedDeck[SRandom.GetInt32S(1, UnlimitedDeck.Length)];
            }
            else
            {
                return CurrentDeck[SRandom.GetInt32S(0, CurrentDeck.Count)];
            }
        }

        private static List<CardTypes> GetNewDeck(byte decksInAmount)
        {
            var deck = ((CardTypes[])Enum.GetValues(typeof(CardTypes))).ToList();

            deck.Remove(CardTypes.None);

            var nDeck = new List<CardTypes>();

            for (byte i = 0; i < decksInAmount; i++)
                nDeck.AddRange(deck);

            return nDeck;
        }

        public void KickAllWrongPlayers()
        {
            for (int i = 0; i < CurrentPlayers.Length; i++)
            {
                var x = CurrentPlayers[i];

                if (x == null)
                    continue;

                var pInfo = PlayerData.PlayerInfo.Get(x.CID);

                var pData = pInfo?.PlayerData;

                if (pData == null || !IsPlayerNearTable(pData.Player))
                {
                    CurrentPlayers[i] = null;

                    if (pData != null)
                    {
                        pData.Player.CloseAll(true);
                    }
                }
            }
        }

        public bool IsPlayerNearTable(Player player) => player.Dimension == Settings.MAIN_DIMENSION && player.Position.DistanceTo(Position) <= 5f;
    }
}
