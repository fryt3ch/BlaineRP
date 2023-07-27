using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.Game.Management.Animations;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Casino.Games
{
    public partial class Blackjack
    {
        internal class RemoteEvents : Script
        {
            [RemoteProc("Casino::BLJE")]
            private static object CasinoBlackjackEnter(Player player, int casinoId, int tableId)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return null;

                var casino = CasinoEntity.GetById(casinoId);

                if (casino == null)
                    return null;

                Blackjack table = casino.GetBlackjackById(tableId);

                if (table == null)
                    return null;

                if (!table.IsPlayerNearTable(player))
                    return null;

                int freeIdx = -1;

                for (var i = 0; i < table.CurrentPlayers.Length; i++)
                {
                    if (table.CurrentPlayers[i] == null || table.CurrentPlayers[i].CID > 0 && table.CurrentPlayers[i].CID == pData.CID)
                    {
                        freeIdx = i;

                        break;
                    }
                    else if (table.CurrentPlayers[i].CID > 0 && table.CurrentPlayers[i].Bet <= 0)
                    {
                        var tInfo = PlayerInfo.Get(table.CurrentPlayers[i].CID);

                        var tData = tInfo?.PlayerData;

                        if (tData == null || !table.IsPlayerNearTable(tData.Player))
                        {
                            table.CurrentPlayers[i] = null;

                            freeIdx = i;

                            tData?.Player.CloseAll(true);

                            break;
                        }
                    }
                }

                if (freeIdx < 0)
                {
                    player.Notify("Casino::RLTMP", table.CurrentPlayers.Length);

                    return null;
                }

                SeatData pTableData = table.CurrentPlayers[freeIdx];

                pData.PlayAnim(GeneralType.CasinoBlackjackIdle0);

                if (pTableData == null)
                {
                    table.CurrentPlayers[freeIdx] = new SeatData()
                    {
                        CID = pData.CID,
                    };
                }
                else
                {
                }

                return $"{pData.Info.CasinoChips}^{freeIdx}^{table.GetCurrentStateData()}";
            }

            [RemoteProc("Casino::BLJL")]
            private static bool CasinoBlackjackLeave(Player player, int casinoId, int tableId)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                PlayerData pData = sRes.Data;

                var casino = CasinoEntity.GetById(casinoId);

                if (casino == null)
                    return false;

                Blackjack table = casino.GetBlackjackById(tableId);

                if (table == null)
                    return false;

                int tableIdx = -1;

                for (var i = 0; i < table.CurrentPlayers.Length; i++)
                {
                    if (table.CurrentPlayers[i] != null && table.CurrentPlayers[i].CID == pData.CID)
                    {
                        tableIdx = i;

                        break;
                    }
                }

                if (tableIdx < 0)
                    return false;

                SeatData pTableData = table.CurrentPlayers[tableIdx];

                if (pData.GeneralAnim == GeneralType.CasinoBlackjackIdle0)
                    pData.StopGeneralAnim();

                if (pTableData.Bet <= 0)
                    table.CurrentPlayers[tableIdx] = null;

                return true;
            }

            [RemoteProc("Casino::BLJSB")]
            public static bool CasinoBlackjackSetBet(Player player, int casinoId, int tableId, uint amount)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return false;

                var casino = CasinoEntity.GetById(casinoId);

                if (casino == null)
                    return false;

                Blackjack table = casino.GetBlackjackById(tableId);

                if (table == null)
                    return false;

                if (!table.IsPlayerNearTable(player))
                    return false;

                int tableIdx = -1;

                for (var i = 0; i < table.CurrentPlayers.Length; i++)
                {
                    if (table.CurrentPlayers[i] != null && table.CurrentPlayers[i].CID > 0 && table.CurrentPlayers[i].CID == pData.CID)
                    {
                        tableIdx = i;

                        break;
                    }
                }

                if (tableIdx < 0)
                    return false;

                SeatData pTableData = table.CurrentPlayers[tableIdx];

                string curState = table.GetCurrentStateData();

                if (pTableData.Bet > 0 || curState == null || curState[0] != 'I' && curState[0] != 'S')
                {
                    player.Notify("Casino::CSB");

                    return false;
                }

                if (table.MinBet > amount)
                    amount = table.MinBet;
                else if (table.MaxBet < amount)
                    amount = table.MaxBet;

                uint newBalance;

                if (!CasinoEntity.TryRemoveCasinoChips(pData.Info, amount, out newBalance, true, null))
                    return false;

                CasinoEntity.SetCasinoChips(pData.Info, newBalance, null);

                pTableData.Bet = amount;

                string state = table.GetCurrentStateData();

                if (state != null && state[0] == 'I')
                    table.StartGame();

                Utils.TriggerEventInDistance(table.Position, Properties.Settings.Static.MainDimension, 10f, "Casino::BLJM", 1, casinoId, tableId, tableIdx, amount, player.Id);

                return true;
            }

            [RemoteEvent("Casino::BLJD")]
            private static void CasinoBlackjackDecide(Player player, int casinoId, int tableId, byte decision)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                var casino = CasinoEntity.GetById(casinoId);

                if (casino == null)
                    return;

                Blackjack table = casino.GetBlackjackById(tableId);

                if (table == null)
                    return;

                if (!table.IsPlayerNearTable(player))
                    return;

                int tableIdx = -1;

                for (var i = 0; i < table.CurrentPlayers.Length; i++)
                {
                    if (table.CurrentPlayers[i] != null && table.CurrentPlayers[i].CID > 0 && table.CurrentPlayers[i].CID == pData.CID)
                    {
                        tableIdx = i;

                        break;
                    }
                }

                if (tableIdx < 0)
                    return;

                SeatData pTableData = table.CurrentPlayers[tableIdx];

                string[] curState = table.GetCurrentStateData()?.Split('*');

                if (curState == null || curState[0] != "D" || byte.Parse(curState[1]) != tableIdx)
                    return;

                if (decision == 0)
                {
                    table.Timer?.Dispose();

                    table.SetPlayerToDecisionState((byte)(tableIdx + 1));

                    Utils.TriggerEventInDistance(table.Position, Properties.Settings.Static.MainDimension, 10f, "Casino::BLJM", 0, 1, player.Id);
                }
                else if (decision == 1)
                {
                    table.OnPlayerChooseAnother((byte)tableIdx);

                    Utils.TriggerEventInDistance(table.Position, Properties.Settings.Static.MainDimension, 10f, "Casino::BLJM", 0, 2, player.Id);
                }
            }
        }
    }
}