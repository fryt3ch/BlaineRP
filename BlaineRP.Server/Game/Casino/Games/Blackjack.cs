using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.Extensions.System;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Casino.Games
{
    public partial class Blackjack
    {
        /// <summary>Является ли карта тузом?</summary>
        /// <param name="cType">Тип карты</param>
        public static bool IsCardTypeAce(CardTypes cType) => cType == CardTypes.Club_Ace || cType == CardTypes.Dia_Ace || cType == CardTypes.Hrt_Ace || cType == CardTypes.Spd_Ace;

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

            Utils.TriggerEventInDistance(Position, Properties.Settings.Static.MainDimension, 50f, "Casino::BLJS", CasinoId, Id, value);
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

                        DealerHand.Add(new CardData() { CardType = cardType, Value = value });

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
                                    var pInfo = PlayerInfo.Get(CurrentPlayers[x].CID);

                                    if (pInfo == null)
                                        continue;

                                    uint chipBalance;

                                    if (CasinoEntity.TryAddCasinoChips(pInfo, CurrentPlayers[x].Bet, out chipBalance, true, null))
                                    {
                                        CasinoEntity.SetCasinoChips(pInfo, chipBalance, null);
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
                                    var pInfo = PlayerInfo.Get(CurrentPlayers[x].CID);

                                    if (pInfo == null)
                                        continue;

                                    uint chipBalance;

                                    if (CasinoEntity.TryAddCasinoChips(pInfo, (uint)Math.Floor(CurrentPlayers[x].Bet * BLACKJACK_COEF), out chipBalance, true, null))
                                    {
                                        CasinoEntity.SetCasinoChips(pInfo, chipBalance, null);
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

                        var pInfo = PlayerInfo.Get(x.CID);

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

                                if (CasinoEntity.TryAddCasinoChips(pInfo, totalWin, out newBalance, true, null))
                                {
                                    CasinoEntity.SetCasinoChips(pInfo, newBalance, null);
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

                var pInfo = PlayerInfo.Get(x.CID);

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

        public bool IsPlayerNearTable(Player player) => player.Dimension == Properties.Settings.Static.MainDimension && player.Position.DistanceTo(Position) <= 5f;
    }
}
