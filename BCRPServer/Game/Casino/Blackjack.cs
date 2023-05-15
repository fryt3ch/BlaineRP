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
        public const int BET_WAIT_TIME = 5_000;
        public const int DECISION_WAIT_TIME = 15_000;

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

        public static bool IsCardTypeAce(CardTypes cType) => cType == CardTypes.Club_Ace || cType == CardTypes.Dia_Ace || cType == CardTypes.Hrt_Ace || cType == CardTypes.Spd_Ace;

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

        public class PlayerData
        {
            public uint CID { get; set; }

            public uint Bet { get; set; }

            public List<CardData> Hand { get; set; }

            public PlayerData()
            {

            }
        }

        public PlayerData[] CurrentPlayers { get; set; }

        public uint MinBet { get; set; }

        public uint MaxBet { get; set; }

        public Vector3 Position { get; set; }

        public Timer Timer { get; set; }

        private string CurrentStateData { get; set; }

        private List<CardData> DealerHand { get; set; }

        public readonly byte CasinoId;
        public readonly byte Id;

        public Blackjack(byte CasinoId, byte Id, float PosX, float PosY, float PosZ, byte MaxPlayers)
        {
            this.Position = new Vector3(PosX, PosY, PosZ);

            this.CurrentPlayers = new PlayerData[MaxPlayers];

            this.CasinoId = CasinoId;
            this.Id = Id;
        }

        public void SetCurrentStateData(string value)
        {
            CurrentStateData = value;

            Utils.TriggerEventInDistance(Position, Utils.Dimensions.Main, 50f, "Casino::BLJS", CasinoId, Id, value);
        }

        public string GetCurrentStateData()
        {
            return CurrentStateData;
        }

        public void StartGame()
        {
            Timer?.Dispose();

            var startDate = Utils.GetCurrentTime().AddMilliseconds(BET_WAIT_TIME);

            SetCurrentStateData($"S{startDate.GetUnixTimestamp()}*{string.Join('*', CurrentPlayers.Select(x => x?.Bet ?? 0).ToList())}");

            var allCards = ((CardTypes[])Enum.GetValues(typeof(CardTypes))).ToList();

            allCards.Remove(CardTypes.None);

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
                        var cardType = allCards[SRandom.GetInt32S(0, allCards.Count)];

                        var value = CardValues.GetValueOrDefault(cardType);

                        if (i == 1 && IsCardTypeAce(cardType) && (dealerHandSum + value > 21))
                            value = 1;

                        DealerHand.Add(new CardData() { CardType = cardType, Value = value});

                        allCards.Remove(cardType);
                    }

                    var dealerBlackjack = dealerHandSum == 21;

                    string dealerStr = null;

                    if (dealerBlackjack)
                    {
                        dealerStr = $"{(byte)DealerHand[0].CardType}-{DealerHand[0].Value}!{(byte)DealerHand[1].CardType}-{DealerHand[1].Value}";
                    }
                    else
                    {
                        dealerStr = $"{(byte)DealerHand[0].CardType}-{DealerHand[0].Value}!0-0";

                        DealerHand.RemoveAt(1);
                    }

                    strBuilder.Append(dealerStr);

                    var blackjackPlayers = new List<int>();

                    var playersCount = 0;

                    for (int j = 0; j < CurrentPlayers.Length; j++)
                    {
                        var x = CurrentPlayers[j];

                        if (x == null)
                        {
                            strBuilder.Append('*');

                            continue;
                        }

                        playersCount++;

                        x.Hand = new List<CardData>();

                        byte handSum = 0;

                        for (int i = 0; i < 2; i++)
                        {
                            var cardType = allCards[SRandom.GetInt32S(0, allCards.Count)];

                            var value = CardValues.GetValueOrDefault(cardType);

                            if (i == 1 && IsCardTypeAce(cardType) && (handSum + value > 21))
                                value = 1;

                            x.Hand.Add(new CardData() { CardType = cardType, Value = value });

                            handSum += value;

                            allCards.Remove(cardType);
                        }

                        if (handSum == 21)
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

                            for (int i = 0; i < CurrentPlayers.Length; i++)
                            {
                                var x = CurrentPlayers[i];

                                if (x == null)
                                    continue;

                                SetPlayerToDecisionState((byte)i);

                                return;
                            }
                        });
                    }, null, 1500 * 2 + playersCount * 1500 * 2, Timeout.Infinite);
                });
            }, null, BET_WAIT_TIME, Timeout.Infinite);
        }

        public void SetPlayerToDecisionState(byte seatIdx)
        {
            Timer?.Dispose();

            var startDate = Utils.GetCurrentTime().AddMilliseconds(BET_WAIT_TIME);

            var curCardsStrBuilder = GetCurrentCardsStrBuilder();

            curCardsStrBuilder.Insert(0, $"D*{seatIdx}*{startDate.GetUnixTimestamp()}*");

            SetCurrentStateData(curCardsStrBuilder.ToString());

            Timer = new Timer((obj) =>
            {
                NAPI.Task.Run(() =>
                {
                    Timer?.Dispose();

                    for (int i = seatIdx; i < CurrentPlayers.Length; i++)
                    {
                        if (CurrentPlayers[i] == null)
                            continue;

                        SetPlayerToDecisionState((byte)i);

                        return;
                    }
                });
            }, null, DECISION_WAIT_TIME, Timeout.Infinite);
        }

        public void OnPlayerChooseAnother()
        {
            Timer?.Dispose();
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

                if (x == null)
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
    }
}
