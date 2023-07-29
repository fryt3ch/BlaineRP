using BlaineRP.Server.Game.Casino;
using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Game.NPCs.Types
{
    internal class Casino
    {
        [RemoteAction("Casino::GCBD", "Casino@Cashier_0_0")]
        private static object CasinoGetChipsBalanceData(PlayerData pData, string npcId, string[] data)
        {
            return $"{pData.Info.CasinoChips}";
        }

        [RemoteAction("Casino::C", "Casino@Cashier_0_0")]
        private static object CasinoChips(PlayerData pData, string npcId, string[] data)
        {
            if (data.Length != 2)
                return null;

            var buy = data[0] == "1";

            uint amount;

            if (!uint.TryParse(data[1], out amount))
                return null;

            if (amount <= 0)
                return null;

            var npcIdD = npcId.Split('_');

            var casinoId = int.Parse(npcIdD[1]);

            var casino = CasinoEntity.GetById(casinoId);

            if (casino == null)
                return null;

            if (buy)
            {
                var totalPrice = (ulong)casino.BuyChipPrice * amount;

                ulong newBalance;

                if (!pData.TryRemoveCash(totalPrice, out newBalance, true, null))
                    return null;

                uint newChipsBalance;

                if (!CasinoEntity.TryAddCasinoChips(pData.Info, amount, out newChipsBalance, true, null))
                    return null;

                pData.SetCash(newBalance);

                CasinoEntity.SetCasinoChips(pData.Info, newChipsBalance, null);
            }
            else
            {
                var totalPrice = casino.SellChipPrice * amount;

                uint newChipsBalance;

                if (!CasinoEntity.TryRemoveCasinoChips(pData.Info, amount, out newChipsBalance, true, null))
                    return null;

                ulong newBalance;

                if (!pData.TryAddCash(totalPrice, out newBalance, true, null))
                    return null;

                CasinoEntity.SetCasinoChips(pData.Info, newChipsBalance, null);

                pData.SetCash(newBalance);
            }

            return $"{pData.Info.CasinoChips}";
        }
    }
}
