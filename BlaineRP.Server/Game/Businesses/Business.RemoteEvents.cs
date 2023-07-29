using System.Linq;
using BlaineRP.Server.Game.Phone;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;
using Newtonsoft.Json.Linq;

namespace BlaineRP.Server.Game.Businesses
{
    public abstract partial class Business
    {
        internal class RemoteEvents : Script
        {
            [RemoteProc("Business::BuyGov")]
            private static bool BuyGov(Player player, int id)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                var pData = sRes.Data;

                if (player.Dimension != Properties.Settings.Static.MainDimension)
                    return false;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return false;

                var business = Game.Businesses.Business.Get(id);

                if (business == null || !business.IsBuyable)
                    return false;

                if (!business.IsPlayerNearInfoPosition(pData))
                    return false;

                if (business.Owner != null)
                {
                    player.Notify("Business::AB");

                    return true;
                }

                var res = business.BuyFromGov(pData);

                return res;
            }

            [RemoteProc("Business::SellGov")]
            private static bool SellGov(Player player, int id)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                var pData = sRes.Data;

                if (player.Dimension != Properties.Settings.Static.MainDimension)
                    return false;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return false;

                var business = Game.Businesses.Business.Get(id);

                if (business == null || !business.IsBuyable || business.Owner != pData.Info)
                    return false;

                if (!business.IsPlayerNearInfoPosition(pData))
                    return false;

                business.SellToGov(true);

                return true;
            }

            [RemoteProc("Business::TCash")]
            private static ulong? TakeCash(Player player, int id, int amountI)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                var pData = sRes.Data;

                if (amountI <= 0)
                    return null;

                if (player.Dimension != Properties.Settings.Static.MainDimension)
                    return null;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return null;

                var business = Game.Businesses.Business.Get(id);

                if (business == null || business.Owner != pData.Info)
                    return null;

                if (!business.IsPlayerNearInfoPosition(pData))
                    return null;

                var amount = (ulong)amountI;

                ulong newBalance;

                if (!business.TryRemoveMoneyCash(amount, out newBalance, true))
                    return null;

                ulong newPlayerBalance;

                if (!pData.TryAddCash(amount, out newPlayerBalance, true))
                    return null;

                business.SetCash(newBalance);

                pData.SetCash(newPlayerBalance);

                return newBalance;
            }

            [RemoteProc("Business::SSIS")]
            private static bool SetIncassationState(Player player, int id, bool state)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                var pData = sRes.Data;

                if (player.Dimension != Properties.Settings.Static.MainDimension)
                    return false;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return false;

                var business = Game.Businesses.Business.Get(id);

                if (business == null || business.Owner != pData.Info)
                    return false;

                if (!business.IsPlayerNearInfoPosition(pData))
                    return false;

                if (business.IncassationState == state)
                    return false;

                business.IncassationState = state;

                return true;
            }

            [RemoteProc("Business::SSMA")]
            private static bool SetMargin(Player player, int id, ushort marginC)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                var pData = sRes.Data;

                if (player.Dimension != Properties.Settings.Static.MainDimension)
                    return false;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return false;

                var business = Game.Businesses.Business.Get(id);

                if (business == null || business.Owner != pData.Info)
                    return false;

                if (!business.IsPlayerNearInfoPosition(pData))
                    return false;

                if (marginC > (business.Type == BusinessType.Farm ? Game.Businesses.Business.MAX_MARGIN_CLIENT_FARM : Game.Businesses.Business.MAX_MARGIN_CLIENT))
                    return false;

                var margin = marginC / 100m + 1;

                if (business.Margin == margin)
                    return false;

                business.SetMargin(margin);

                return true;
            }

            [RemoteProc("Business::NDO")]
            private static ulong? NewDeliveryOrder(Player player, int id, int amountI)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                var pData = sRes.Data;

                if (amountI <= 0)
                    return null;

                if (player.Dimension != Properties.Settings.Static.MainDimension)
                    return null;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return null;

                var business = Game.Businesses.Business.Get(id);

                if (business == null || business.Owner != pData.Info)
                    return null;

                if (!business.IsPlayerNearInfoPosition(pData))
                    return null;

                if (business.OrderedMaterials > 0)
                    return null;

                var amount = (ulong)amountI;

                var matData = business.MaterialsData;

                if (amount > matData.MaxMaterialsPerOrder)
                {
                    player.Notify("Business::MMPO", matData.MaxMaterialsPerOrder);

                    return null;
                }

                if ((ulong)business.Materials + amount > matData.MaxMaterialsBalance)
                {
                    player.Notify("Business::MMB", matData.MaxMaterialsBalance, business.Materials);

                    return null;
                }

                var totalPrice = amount * matData.BuyPrice + Game.Businesses.Business.MATS_DELIVERY_PRICE;

                ulong newBalance;

                if (!business.TryRemoveMoneyBank(totalPrice, out newBalance, true))
                    return null;

                business.OrderedMaterials = (uint)amount;

                business.SetBank(newBalance);

                MySQL.BusinessUpdateBalances(business, true);

                var orderId = business.ClosestTruckerJob.AddCustomOrder(business);

                var sms = new SMS((uint)DefaultNumbers.Delivery, pData.Info, string.Format(SMS.GetDefaultSmsMessage(SMS.PredefinedTypes.DeliveryBusinessOrderNew), orderId, amount, totalPrice));

                pData.Info.AddSms(sms, true);

                return business.Bank;
            }

            [RemoteProc("Business::CAO")]
            private static ulong? CancelActiveOrder(Player player, int id)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                var pData = sRes.Data;

                if (player.Dimension != Properties.Settings.Static.MainDimension)
                    return null;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return null;

                var business = Game.Businesses.Business.Get(id);

                if (business == null || business.Owner != pData.Info)
                    return null;

                if (!business.IsPlayerNearInfoPosition(pData))
                    return null;

                if (business.OrderedMaterials <= 0)
                    return null;

                var truckerJob = business.ClosestTruckerJob;

                var currentOrderPair = truckerJob.ActiveOrders.Where(x => x.Value.TargetBusiness == business && x.Value.IsCustom).FirstOrDefault();

                if (currentOrderPair.Value == null)
                    return null;

                if (currentOrderPair.Value.CurrentWorker != null)
                {
                    player.Notify("Business::COIT");

                    return null;
                }

                var totalPrice = (ulong)business.OrderedMaterials * business.MaterialsData.BuyPrice;

                business.OrderedMaterials = 0;

                truckerJob.RemoveOrder(currentOrderPair.Key, currentOrderPair.Value);

                ulong newBalance;

                if (business.TryAddMoneyBank(totalPrice, out newBalance, true))
                {
                    business.SetBank(newBalance);
                }

                MySQL.BusinessUpdateBalances(business, true);

                var sms = new SMS((uint)DefaultNumbers.Delivery, pData.Info, string.Format(SMS.GetDefaultSmsMessage(SMS.PredefinedTypes.DeliveryBusinessOrderCancel), currentOrderPair.Key, totalPrice));

                pData.Info.AddSms(sms, true);

                return business.Bank;
            }

            [RemoteProc("Business::ShowMenu")]
            private static JObject ShowMenu(Player player, int id)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                var pData = sRes.Data;

                if (player.Dimension != Properties.Settings.Static.MainDimension)
                    return null;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return null;

                var business = Game.Businesses.Business.Get(id);

                if (business == null || business.Owner != pData.Info)
                    return null;

                if (!business.IsPlayerNearInfoPosition(pData))
                    return null;

                return business.ToClientMenuObject();
            }

            [RemoteProc("Business::GMI")]
            private static float GetMarginInfo(Player player, int id)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return float.MinValue;

                var business = Game.Businesses.Business.Get(id);

                if (business == null)
                    return float.MinValue;

                return (float)business.Margin;
            }

            [RemoteEvent("Business::Enter")]
            public static void BusinessEnter(Player player, int id)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                var pData = sRes.Data;

                if (pData.CurrentBusiness != null)
                    return;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                var business = Game.Businesses.Business.Get(id);

                if (business == null)
                    return;

                if (!business.IsPlayerNearInteractPosition(pData))
                    return;

                if (business is Game.Businesses.IEnterable enterable)
                {
                    pData.Info.LastData.UpdatePosition(new Vector4(player.Position, player.Heading), player.Dimension, false);

                    pData.StopUseCurrentItem();
                    player.DetachAllObjectsInHand();
                    pData.StopAllAnims();

                    pData.UnequipActiveWeapon();

                    pData.IsInvincible = true;

                    player.Teleport(enterable.EnterProperties.Position, false, Utils.GetPrivateDimension(player), enterable.EnterProperties.RotationZ, true);

                    if (business.Type == BusinessType.BarberShop)
                    {
                        player.TriggerEvent("Shop::Show", (int)business.Type, (float)business.Margin, enterable.EnterProperties.RotationZ, pData.Info.HairStyle, pData.Info.HeadOverlays[1], pData.Info.HeadOverlays[10], pData.Info.HeadOverlays[2], pData.Info.HeadOverlays[8], pData.Info.HeadOverlays[5], pData.Info.HeadOverlays[4]);
                    }
                    else
                    {
                        player.TriggerEvent("Shop::Show", (int)business.Type, (float)business.Margin, enterable.EnterProperties.RotationZ);
                    }

                    pData.CurrentBusiness = business;
                }
                else
                {
                    player.CloseAll(true);

                    player.TriggerEvent("Shop::Show", (int)business.Type, (float)business.Margin);

                    pData.CurrentBusiness = business;
                }
            }

            [RemoteEvent("Business::Exit")]
            public static void BusinessExit(Player player)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                var pData = sRes.Data;

                var business = pData.CurrentBusiness;

                if (business == null)
                    return;

                Sync.Players.ExitFromBusiness(pData, true);
            }
        }
    }
}