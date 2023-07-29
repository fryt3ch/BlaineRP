using BlaineRP.Server.Game.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Businesses
{
    public abstract partial class Shop
    {
        internal class RemoteEvents : Script
        {
            [RemoteProc("Shop::Buy")]
            public static bool ShopBuy(Player player, string id, bool useCash)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                PlayerData pData = sRes.Data;

                if (id == null)
                    return false;

                var shop = pData.CurrentBusiness as Shop;

                if (shop == null)
                    return false;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return false;

                if (!(shop is IEnterable))
                {
                    if (shop is GasStation gs)
                    {
                        if (!gs.IsPlayerNearGasolinesPosition(pData))
                            return false;
                    }
                    else
                    {
                        if (!shop.IsPlayerNearInteractPosition(pData))
                            return false;
                    }
                }

                bool res = shop.TryBuyItem(pData, useCash, id);

                return res;
            }
        }
    }
}