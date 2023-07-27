using BlaineRP.Server.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Casino.Games
{
    public partial class SlotMachine
    {
        internal class RemoteEvents : Script
        {
            [RemoteProc("Casino::SLME")]
            private static object CasinoSlotMachineEnter(Player player, int casinoId, int slotMachineId)
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

                SlotMachine slotMachine = casino.GetSlotMachineById(slotMachineId);

                if (slotMachine == null)
                    return null;

                if (!slotMachine.IsPlayerNear(player))
                    return null;

                if (!slotMachine.IsAvailableNow())
                {
                    var tInfo = PlayerInfo.Get(slotMachine.CurrentCID);

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

                foreach (CasinoEntity x in CasinoEntity.All)
                {
                    foreach (SlotMachine y in x.SlotMachines)
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
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                PlayerData pData = sRes.Data;

                var casino = CasinoEntity.GetById(casinoId);

                if (casino == null)
                    return false;

                SlotMachine slotMachine = casino.GetSlotMachineById(slotMachineId);

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
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return null;

                var casino = CasinoEntity.GetById(casinoId);

                if (casino == null)
                    return null;

                SlotMachine slotMachine = casino.GetSlotMachineById(slotMachineId);

                if (slotMachine == null)
                    return null;

                if (slotMachine.CurrentCID <= 0 || slotMachine.CurrentCID != pData.CID)
                    return null;

                if (!slotMachine.CanSpinNow())
                    return null;

                if (bet < MinBet || bet > MaxBet)
                    return null;

                uint newBalance;

                if (!CasinoEntity.TryRemoveCasinoChips(pData.Info, bet, out newBalance, true, null))
                    return null;

                CasinoEntity.SetCasinoChips(pData.Info, newBalance, null);

                ReelIconTypes res = slotMachine.Spin(pData, bet);

                return $"{(byte)res}^{slotMachine.Jackpot}";
            }
        }
    }
}