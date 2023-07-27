using GTANetworkAPI;
using System;
using System.Collections.Generic;
using BlaineRP.Server.EntityData.Players;

namespace BlaineRP.Server.Game.Businesses
{
    public abstract class Shop : Business
    {
        /// <summary>Все цены в магазинах (бизнесах)</summary>
        /// <remarks>Цены - в материалах, не в долларах</remarks>
        public static Dictionary<BusinessTypes, MaterialsData> AllPrices { get; private set; } = new Dictionary<BusinessTypes, MaterialsData>();

        public Dictionary<string, uint> Prices => AllPrices[Type].Prices;

        public bool TryProceedPayment(PlayerData pData, bool useCash, string itemId, uint amount, out uint newMats, out ulong newBalance, out ulong newPlayerBalance)
        {
            try
            {
                var matData = MaterialsData;

                var matPrice = matData.Prices[itemId];

                matPrice *= amount;

                if (Owner != null)
                {
                    if (!TryRemoveMaterials(matPrice, out newMats, true, pData))
                    {
                        newBalance = 0;
                        newPlayerBalance = 0;

                        return false;
                    }
                }
                else
                {
                    newMats = 0;
                }

                var realPrice = (ulong)Math.Floor((decimal)matPrice * matData.RealPrice * Margin);

                if (useCash && !IncassationState)
                {
                    if (!pData.TryRemoveCash(realPrice, out newPlayerBalance, true))
                    {
                        newBalance = 0;

                        return false;
                    }

                    var bizPrice = GetBusinessPrice(matPrice, false);

                    if (!TryAddMoneyCash(bizPrice, out newBalance, true, pData))
                        return false;
                }
                else
                {
                    if (useCash)
                    {
                        if (!pData.TryRemoveCash(realPrice, out newPlayerBalance, true))
                        {
                            newBalance = 0;

                            return false;
                        }

                        var bizPrice = GetBusinessPrice(matPrice, true);

                        if (!TryAddMoneyBank(bizPrice, out newBalance, true, pData))
                            return false;
                    }
                    else
                    {
                        if (!pData.HasBankAccount(true))
                        {
                            newBalance = 0;
                            newPlayerBalance = 0;

                            return false;
                        }

                        ulong cb;

                        if (!pData.BankAccount.TryRemoveMoneyDebitUseCashback(realPrice, out newPlayerBalance, out cb, true))
                        {
                            newBalance = 0;

                            return false;
                        }

                        var bizPrice = GetBusinessPrice(matPrice, false);

                        if (!TryAddMoneyBank(bizPrice, out newBalance, true, pData))
                            return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                newMats = 0;
                newBalance = 0;
                newPlayerBalance = 0;

                return false;
            }
        }

        public bool TryProceedPaymentByFraction(PlayerData pData, Game.Fractions.Fraction fData, string itemId, uint amount, out uint newMats, out ulong newBalance, out ulong newFractionBalance)
        {
            try
            {
                var matData = MaterialsData;

                var matPrice = matData.Prices[itemId];

                matPrice *= amount;

                if (Owner != null)
                {
                    if (!TryRemoveMaterials(matPrice, out newMats, true, pData))
                    {
                        newBalance = 0;
                        newFractionBalance = 0;

                        return false;
                    }
                }
                else
                {
                    newMats = 0;
                }

                var realPrice = (ulong)Math.Floor((decimal)matPrice * matData.RealPrice * Margin);

                if (!fData.TryRemoveMoney(realPrice, out newFractionBalance, true, pData))
                {
                    newBalance = 0;

                    return false;
                }

                var bizPrice = GetBusinessPrice(matPrice, false);

                if (!TryAddMoneyBank(bizPrice, out newBalance, true, pData))
                    return false;

                return true;
            }
            catch (Exception)
            {
                newMats = 0;
                newBalance = 0;
                newFractionBalance = 0;

                return false;
            }
        }

        public virtual bool TryBuyItem(PlayerData pData, bool useCash, string itemId)
        {
            var iData = itemId.Split('&');

            if (iData.Length != 3)
                return false;

            int variation, amount;

            if (!int.TryParse(iData[1], out variation) || !int.TryParse(iData[2], out amount))
                return false;

            if (variation < 0 || amount <= 0)
                return false;

            uint newMats;
            ulong newBalance, newPlayerBalance;

            if (!TryProceedPayment(pData, useCash, iData[0], (uint)amount, out newMats, out newBalance, out newPlayerBalance))
                return false;

            if (!pData.GiveItem(out _, iData[0], variation, amount, true, true))
                return false;

            ProceedPayment(pData, useCash, newMats, newBalance, newPlayerBalance);

            return true;
        }

        public Shop(int ID, Vector3 PositionInfo, Utils.Vector4 PositionInteract, BusinessTypes Type) : base(ID, PositionInfo, PositionInteract, Type)
        {

        }
    }
}
