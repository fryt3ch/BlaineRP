using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;

namespace BCRPServer.Events.Players
{
    internal class Casino : Script
    {
        [RemoteProc("Casino::RLTJ")]
        public static object CasinoRouletteJoin(Player player, int casinoId, int rouletteId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return null;

            var casino = Game.Casino.Casino.GetById(casinoId);

            if (casino == null)
                return null;

            var roulette = casino.GetRouletteById(rouletteId);

            if (roulette == null)
                return null;

            if (player.Dimension != Properties.Settings.Static.MainDimension || roulette.Position.DistanceTo(player.Position) > 5f)
                return null;

            var betData = roulette.CurrentPlayers.GetValueOrDefault(pData.CID);

            if (betData == null)
            {
                if (roulette.CurrentPlayers.Count >= roulette.MaxPlayers)
                {
                    player.Notify("Casino::RLTMP", roulette.MaxPlayers);

                    return null;
                }

                return $"{pData.Info.CasinoChips}^{roulette.GetCurrentStateData()}";
            }
            else
            {
                return $"{pData.Info.CasinoChips}^{roulette.GetCurrentStateData()}";
            }
        }

        [RemoteProc("Casino::RLTSB")]
        public static bool CasinoRouletteSetBet(Player player, int casinoId, int rouletteId, byte betTypeNum, uint amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            if (!Enum.IsDefined(typeof(Game.Casino.Roulette.BetTypes), betTypeNum))
                return false;

            var casino = Game.Casino.Casino.GetById(casinoId);

            if (casino == null)
                return false;

            var roulette = casino.GetRouletteById(rouletteId);

            if (roulette == null)
                return false;

            if (player.Dimension != Properties.Settings.Static.MainDimension || roulette.Position.DistanceTo(player.Position) > 5f)
                return false;

            if (!roulette.CanPlaceBet())
                return false;

            if (roulette.MinBet > amount)
                amount = roulette.MinBet;
            else if (roulette.MaxBet < amount)
                amount = roulette.MaxBet;

            uint newBalance;

            if (!Game.Casino.Casino.TryRemoveCasinoChips(pData.Info, amount, out newBalance, true, null))
                return false;

            var betType = (Game.Casino.Roulette.BetTypes)betTypeNum;

            int freeBetIdx = -1;

            var betData = roulette.CurrentPlayers.GetValueOrDefault(pData.CID);

            if (betData == null)
            {
                if (roulette.CurrentPlayers.Count >= roulette.MaxPlayers)
                {
                    player.Notify("Casino::RLTMP", roulette.MaxPlayers);

                    return false;
                }

                betData = roulette.AddPlayer(pData);

                freeBetIdx = 0;
            }
            else
            {
                for (int i = 0; i < betData.Bets.Length; i++)
                {
                    if (freeBetIdx < 0 && betData.Bets[i] == null)
                    {
                        freeBetIdx = i;
                    }
                    else if (betData.Bets[i]?.Type == betType)
                    {
                        return false;
                    }
                }

                if (freeBetIdx < 0)
                {
                    player.Notify("Casino::RLTMB");

                    return false;
                }
            }

            betData.Bets[freeBetIdx] = new Game.Casino.Roulette.BetData.Bet() { Type = betType, Amount = amount, };

            Game.Casino.Casino.SetCasinoChips(pData.Info, newBalance, null);

            var state = roulette.GetCurrentStateData();

            if (state != null && state[0] == 'I')
            {
                roulette.StartGame();
            }

            return true;
        }

        [RemoteEvent("Casino::LCWS")]
        private static void CasinoLuckyWheelSpin(Player player, int casinoId, int luckyWheelId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var casino = Game.Casino.Casino.GetById(casinoId);

            if (casino == null)
                return;

            var luckyWheel = casino.GetLuckyWheelById(luckyWheelId);

            if (luckyWheel == null)
                return;

            if (player.Dimension != Properties.Settings.Static.MainDimension || luckyWheel.Position.DistanceTo(player.Position) > 5f)
                return;

            var freeLuckyWheelCdId = NAPI.Util.GetHashKey("CASINO_LW_FREE_0");

            var curTime = Utils.GetCurrentTime();

            if (pData.HasCooldown(freeLuckyWheelCdId, curTime, Game.Casino.LuckyWheel.SpinDefaultCooldown, out _, out _, out _, 3, true))
                return;

            if (!luckyWheel.IsAvailableNow())
            {
                player.Notify("Casino::LCWAS");

                return;
            }

            foreach (var x in Game.Casino.Casino.All)
            {
                foreach (var y in x.LuckyWheels)
                {
                    if (y.CurrentCID > 0 && y.CurrentCID == pData.CID)
                        return;
                }
            }                

            luckyWheel.Spin(casinoId, luckyWheelId, pData);

            pData.Info.SetCooldown(freeLuckyWheelCdId, curTime, true);
        }

        [RemoteProc("Casino::SLME")]
        private static object CasinoSlotMachineEnter(Player player, int casinoId, int slotMachineId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return null;

            var casino = Game.Casino.Casino.GetById(casinoId);

            if (casino == null)
                return null;

            var slotMachine = casino.GetSlotMachineById(slotMachineId);

            if (slotMachine == null)
                return null;

            if (!slotMachine.IsPlayerNear(player))
                return null;

            if (!slotMachine.IsAvailableNow())
            {
                var tInfo = PlayerData.PlayerInfo.Get(slotMachine.CurrentCID);

                var tData = tInfo?.PlayerData;

                if (tData == null || !slotMachine.IsPlayerNear(tData.Player))
                {
                    slotMachine.CurrentCID = 0;

                    tData?.Player.CloseAll(true);
                }
                else
                {
                    player.Notify("Casino::SLMAS");

                    return null;
                }
            }

            foreach (var x in Game.Casino.Casino.All)
            {
                foreach (var y in x.SlotMachines)
                {
                    if (y.CurrentCID > 0 && y.CurrentCID == pData.CID)
                        return null;
                }
            }

            slotMachine.SetPlayerTo(pData);

            return $"{pData.Info.CasinoChips}^{slotMachine.Jackpot}";
        }

        [RemoteProc("Casino::SLML")]
        private static bool CasinoSlotMachineLeave(Player player, int casinoId, int slotMachineId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            var casino = Game.Casino.Casino.GetById(casinoId);

            if (casino == null)
                return false;

            var slotMachine = casino.GetSlotMachineById(slotMachineId);

            if (slotMachine == null)
                return false;

            if (slotMachine.CurrentCID <= 0 || slotMachine.CurrentCID != pData.CID)
                return false;

            slotMachine.SetPlayerFrom(pData);

            return true;
        }

        [RemoteProc("Casino::SLMB")]
        private static object CasinoSlotMachineBet(Player player, int casinoId, int slotMachineId, uint bet)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return null;

            var casino = Game.Casino.Casino.GetById(casinoId);

            if (casino == null)
                return null;

            var slotMachine = casino.GetSlotMachineById(slotMachineId);

            if (slotMachine == null)
                return null;

            if (slotMachine.CurrentCID <= 0 || slotMachine.CurrentCID != pData.CID)
                return null;

            if (!slotMachine.CanSpinNow())
                return null;

            if (bet < Game.Casino.SlotMachine.MinBet || bet > Game.Casino.SlotMachine.MaxBet)
                return null; 

            uint newBalance;

            if (!Game.Casino.Casino.TryRemoveCasinoChips(pData.Info, bet, out newBalance, true, null))
                return null;

            Game.Casino.Casino.SetCasinoChips(pData.Info, newBalance, null);

            var res = slotMachine.Spin(pData, bet);

            return $"{(byte)res}^{slotMachine.Jackpot}";
        }

        [RemoteProc("Casino::BLJE")]
        private static object CasinoBlackjackEnter(Player player, int casinoId, int tableId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return null;

            var casino = Game.Casino.Casino.GetById(casinoId);

            if (casino == null)
                return null;

            var table = casino.GetBlackjackById(tableId);

            if (table == null)
                return null;

            if (!table.IsPlayerNearTable(player))
                return null;

            int freeIdx = -1;

            for (int i = 0; i < table.CurrentPlayers.Length; i++)
            {
                if (table.CurrentPlayers[i] == null || (table.CurrentPlayers[i].CID > 0 && table.CurrentPlayers[i].CID == pData.CID))
                {
                    freeIdx = i;

                    break;
                }
                else if (table.CurrentPlayers[i].CID > 0 && table.CurrentPlayers[i].Bet <= 0)
                {
                    var tInfo = PlayerData.PlayerInfo.Get(table.CurrentPlayers[i].CID);

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

            var pTableData = table.CurrentPlayers[freeIdx];

            pData.PlayAnim(Sync.Animations.GeneralTypes.CasinoBlackjackIdle0);

            if (pTableData == null)
            {
                table.CurrentPlayers[freeIdx] = new Game.Casino.Blackjack.SeatData()
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
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            var casino = Game.Casino.Casino.GetById(casinoId);

            if (casino == null)
                return false;

            var table = casino.GetBlackjackById(tableId);

            if (table == null)
                return false;

            int tableIdx = -1;

            for (int i = 0; i < table.CurrentPlayers.Length; i++)
            {
                if (table.CurrentPlayers[i] != null && table.CurrentPlayers[i].CID == pData.CID)
                {
                    tableIdx = i;

                    break;
                }
            }

            if (tableIdx < 0)
                return false;

            var pTableData = table.CurrentPlayers[tableIdx];

            if (pData.GeneralAnim == Sync.Animations.GeneralTypes.CasinoBlackjackIdle0)
                pData.StopGeneralAnim();

            if (pTableData.Bet <= 0)
                table.CurrentPlayers[tableIdx] = null;

            return true;
        }

        [RemoteProc("Casino::BLJSB")]
        public static bool CasinoBlackjackSetBet(Player player, int casinoId, int tableId, uint amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            var casino = Game.Casino.Casino.GetById(casinoId);

            if (casino == null)
                return false;

            var table = casino.GetBlackjackById(tableId);

            if (table == null)
                return false;

            if (!table.IsPlayerNearTable(player))
                return false;

            int tableIdx = -1;

            for (int i = 0; i < table.CurrentPlayers.Length; i++)
            {
                if (table.CurrentPlayers[i] != null && table.CurrentPlayers[i].CID > 0 && table.CurrentPlayers[i].CID == pData.CID)
                {
                    tableIdx = i;

                    break;
                }
            }

            if (tableIdx < 0)
                return false;

            var pTableData = table.CurrentPlayers[tableIdx];

            var curState = table.GetCurrentStateData();

            if (pTableData.Bet > 0 || curState == null || (curState[0] != 'I' && curState[0] != 'S'))
            {
                player.Notify("Casino::CSB");

                return false;
            }

            if (table.MinBet > amount)
                amount = table.MinBet;
            else if (table.MaxBet < amount)
                amount = table.MaxBet;

            uint newBalance;

            if (!Game.Casino.Casino.TryRemoveCasinoChips(pData.Info, amount, out newBalance, true, null))
                return false;

            Game.Casino.Casino.SetCasinoChips(pData.Info, newBalance, null);

            pTableData.Bet = amount;

            var state = table.GetCurrentStateData();

            if (state != null && state[0] == 'I')
            {
                table.StartGame();
            }

            Utils.TriggerEventInDistance(table.Position, Properties.Settings.Static.MainDimension, 10f, "Casino::BLJM", 1, casinoId, tableId, tableIdx, amount, player.Id);

            return true;
        }

        [RemoteEvent("Casino::BLJD")]
        private static void CasinoBlackjackDecide(Player player, int casinoId, int tableId, byte decision)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var casino = Game.Casino.Casino.GetById(casinoId);

            if (casino == null)
                return;

            var table = casino.GetBlackjackById(tableId);

            if (table == null)
                return;

            if (!table.IsPlayerNearTable(player))
                return;

            int tableIdx = -1;

            for (int i = 0; i < table.CurrentPlayers.Length; i++)
            {
                if (table.CurrentPlayers[i] != null && table.CurrentPlayers[i].CID > 0 && table.CurrentPlayers[i].CID == pData.CID)
                {
                    tableIdx = i;

                    break;
                }
            }

            if (tableIdx < 0)
                return;

            var pTableData = table.CurrentPlayers[tableIdx];

            var curState = table.GetCurrentStateData()?.Split('*');

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
