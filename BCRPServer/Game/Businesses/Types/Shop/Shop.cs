using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Businesses
{
    public abstract class Shop : Business
    {
        /// <summary>Все цены в магазинах (бизнесах)</summary>
        /// <remarks>Цены - в материалах, не в долларах</remarks>
        public static Dictionary<Types, MaterialsData> AllPrices { get; private set; } = new Dictionary<Types, MaterialsData>();

        public Dictionary<string, int> Prices => AllPrices[Type].Prices;

        public (int MatPrice, int RealPrice)? CanBuy(PlayerData pData, bool useCash, string itemId, int amount)
        {
            var priceData = MaterialsData;

            if (priceData == null)
                return null;

            int matPrice;

            if (!priceData.Prices.TryGetValue(itemId, out matPrice))
                return null;

            matPrice *= amount;

            if (Owner != null)
            {
                if (!HasEnoughMaterials(matPrice, pData))
                    return null;
            }

            var realPrice = (int)Math.Floor(matPrice * priceData.RealPrice * Margin);

            if (useCash)
            {
                if (!pData.HasEnoughCash(realPrice, true))
                    return null;
            }
            else
            {
                if (!pData.HasBankAccount(true))
                    return null;

                var cb = pData.BankAccount.HasEnoughMoneyDebit(realPrice, true, true);

                if (cb < 0)
                    return null;

                realPrice -= cb;
            }

            return (matPrice, realPrice);
        }

        public virtual bool BuyItem(PlayerData pData, bool useCash, string itemId)
        {
            var iData = itemId.Split('&');

            if (iData.Length != 3)
                return false;

            int variation, amount;

            if (!int.TryParse(iData[1], out variation) || !int.TryParse(iData[2], out amount))
                return false;

            if (variation < 0 || amount <= 0)
                return false;

            var res = CanBuy(pData, useCash, iData[0], amount);

            if (res == null)
                return false;

            if (!pData.GiveItem(iData[0], variation, amount, true, true))
                return false;

            PaymentProceed(pData, useCash, res.Value.MatPrice, res.Value.RealPrice);

            return true;
        }

        public Shop(int ID, Vector3 PositionInfo, Utils.Vector4 PositionInteract, Types Type) : base(ID, PositionInfo, PositionInteract, Type)
        {

        }
    }
}
