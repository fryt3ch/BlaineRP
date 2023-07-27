using GTANetworkAPI;

namespace BlaineRP.Server.Game.Businesses
{
    public partial class FurnitureShop
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("Business::Furn::Enter")]
            public static void BusinessFurnitureEnter(Player player, int id, int subTypeNum)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                var pData = sRes.Data;

                if (pData.CurrentBusiness != null)
                    return;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                var business = Game.Businesses.Business.Get(id) as Game.Businesses.FurnitureShop;

                if (business == null)
                    return;

                if (!business.IsPlayerNearInteractPosition(pData))
                    return;

                player.CloseAll(true);

                player.TriggerEvent("Shop::Show", (int)business.Type, (float)business.Margin, null, subTypeNum);

                pData.CurrentBusiness = business;
            }
        }
    }
}