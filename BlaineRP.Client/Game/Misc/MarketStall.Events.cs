using System.Linq;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.UI.CEF;

namespace BlaineRP.Client.Game.Misc
{
    public partial class MarketStall
    {
        [Script]
        public class Events
        {
            public Events()
            {
                RAGE.Events.Add("MarketStall::UPD",
                    (args) =>
                    {
                        var id = Utils.Convert.ToInt32(args[0]);

                        var cs = ExtraColshape.All.Where(x => x.Data is MarketStall marketStall && marketStall.Id == id).FirstOrDefault();

                        if (cs == null)
                            return;

                        if (PlayerMarket.CurrentContext == null)
                            return;

                        if (PlayerMarket.CurrentContext == $"MARKETSTALL@SELLER_{id}")
                        {
                        }
                        else if (PlayerMarket.CurrentContext == $"MARKETSTALL@BUYER_{id}")
                        {
                            PlayerMarket.Close();

                            Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Get("MARKETSTALL_B_SERROR_6"));
                        }
                    });

                RAGE.Events.Add("MarketStall::ATBH",
                    (args) =>
                    {
                        var curRentedStall = GetCurrentRentedMarketStall(out _);

                        if (curRentedStall == null)
                            return;

                        var sellHist = SellHistory;

                        if (sellHist == null)
                            return;

                        var itemUid = Utils.Convert.ToUInt32(args[0]);
                        var itemId = (string)args[1];

                        var itemAmount = Utils.Convert.ToUInt32(args[2]);
                        var itemPrice = Utils.Convert.ToDecimal(args[3]);

                        var histItemIdx = -1;

                        for (var i = 0; i < sellHist.Count; i++)
                        {
                            var x = sellHist[i];

                            if (x.Item1 == itemUid && x.Item2 == itemId)
                            {
                                histItemIdx = i;

                                break;
                            }
                        }

                        if (histItemIdx < 0)
                            sellHist.Add((itemUid, itemId, itemAmount, itemPrice));
                        else
                            sellHist[histItemIdx] = (itemUid, itemId, sellHist[histItemIdx].Item3 + itemAmount, sellHist[histItemIdx].Item4 + itemPrice);
                    });
            }
        }
    }
}