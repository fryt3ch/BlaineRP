using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCRPServer;

namespace BCRPServer.Events.NPC.Types
{
    internal class Police
    {
        [NPC.Action("cop_np_buy", "cop0_1")]
        private static object PoliceNumberplateBuy(PlayerData pData, string npcId, string[] data)
        {
            if (data.Length != 3)
                return null;

            var npcIdData = npcId.Split('_');

            var fraction = Game.Fractions.Fraction.Get((Game.Fractions.Types)int.Parse(npcIdData[1])) as Game.Fractions.Police;

            if (fraction == null)
                return null;

            var npId = data[0];

            byte signsAmount;

            if (!byte.TryParse(data[1], out signsAmount))
                return null;

            var useCash = data[2] == "1";

            var prices = Game.Fractions.Police.NumberplatePrices.GetValueOrDefault(npId);

            if (prices == null)
                return null;

            if (signsAmount <= 0 || signsAmount > prices.Length)
                return null;

            var price = prices[signsAmount - 1];

            ulong newBalance;

            if (useCash)
            {
                if (!pData.TryRemoveCash(price, out newBalance, true))
                    return null;
            }
            else
            {
                if (!pData.HasBankAccount(true) || !pData.BankAccount.TryRemoveMoneyDebit(price, out newBalance, true))
                    return null;
            }

            var tag = Game.Items.Numberplate.GenerateTag(signsAmount);

            if (tag == null)
            {
                pData.Player.Notify("NP::NFNRN");

                return null;
            }

            Game.Items.Item item;

            if (!pData.GiveItem(out item, npId, 0, 1, true, true))
                return null;

            var numberplateItem = item as Game.Items.Numberplate;

            if (numberplateItem == null)
                return null;

            if (useCash)
            {
                pData.SetCash(newBalance);
            }
            else
            {
                pData.BankAccount.SetDebitBalance(newBalance, null);
            }

            numberplateItem.Tag = tag;

            numberplateItem.AddTagToUsed();

            numberplateItem.Update();

            for (int i = 0; i < pData.Items.Length; i++)
            {
                if (pData.Items[i] == numberplateItem)
                {
                    pData.Player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, i, numberplateItem.ToClientJson(Game.Items.Inventory.GroupTypes.Items));
                }
            }

            return tag;
        }

        [NPC.Action("cop_np_vreg_p", "cop0_1")]
        private static object PoliceVehicleRegistrationGetPrice(PlayerData pData, string npcId, string[] data)
        {
            if (data.Length != 1)
                return null;

            return data[0] == "1" ? Game.Fractions.Police.VehicleNumberplateRegPrice : Game.Fractions.Police.VehicleNumberplateUnRegPrice;
        }

        [NPC.Action("cop_np_vreg_g", "cop0_1")]
        private static object PoliceVehicleRegistrationGetData(PlayerData pData, string npcId, string[] data)
        {
            if (data.Length != 1)
                return null;

            return data[0] == "1" ? pData.OwnedVehicles.Where(x => x.RegisteredNumberplate == null && x.Numberplate != null).Select(x => $"{x.VID}&{x.Numberplate.Tag}").ToList() : pData.OwnedVehicles.Where(x => x.RegisteredNumberplate != null).Select(x => $"{x.VID}&{x.RegisteredNumberplate}").ToList();
        }

        [NPC.Action("cop_np_vreg", "cop0_1")]
        private static object PoliceVehicleRegistration(PlayerData pData, string npcId, string[] data)
        {
            if (data.Length != 2)
                return null;

            uint vid;

            if (!uint.TryParse(data[0], out vid))
                return null;

            var useCash = data[1] == "1";

            var vInfo = pData.OwnedVehicles.Where(x => x.VID == vid).FirstOrDefault();

            if (vInfo == null)
                return null;

            if (vInfo.RegisteredNumberplate != null)
            {
                return null;
            }

            if (vInfo.Numberplate == null)
            {
                return null;
            }

            ulong newBalance;

            if (useCash)
            {
                if (!pData.TryRemoveCash(Game.Fractions.Police.VehicleNumberplateRegPrice, out newBalance, true))
                    return null;

                pData.SetCash(newBalance);
            }
            else
            {
                if (!pData.HasBankAccount(true) || !pData.BankAccount.TryRemoveMoneyDebit(Game.Fractions.Police.VehicleNumberplateRegPrice, out newBalance, true))
                    return null;

                pData.BankAccount.SetDebitBalance(newBalance, null);
            }

            var npTag = vInfo.Numberplate.Tag;

            var aRegisteredVehInfo = VehicleData.VehicleInfo.All.Values.Where(x => x.RegisteredNumberplate == npTag).FirstOrDefault();

            if (aRegisteredVehInfo != null)
            {
                aRegisteredVehInfo.RegisteredNumberplate = null;

                MySQL.VehicleRegisteredNumberplateUpdate(aRegisteredVehInfo);
            }

            vInfo.RegisteredNumberplate = npTag;

            MySQL.VehicleRegisteredNumberplateUpdate(vInfo);

            return npTag;
        }

        [NPC.Action("cop_np_vunreg", "cop0_1")]
        private static object PoliceVehicleUnRegistration(PlayerData pData, string npcId, string[] data)
        {
            if (data.Length != 2)
                return null;

            uint vid;

            if (!uint.TryParse(data[0], out vid))
                return null;

            var useCash = data[1] == "1";

            var vInfo = pData.OwnedVehicles.Where(x => x.VID == vid).FirstOrDefault();

            if (vInfo == null)
                return null;

            if (vInfo.RegisteredNumberplate == null)
            {
                return null;
            }

            ulong newBalance;

            if (useCash)
            {
                if (!pData.TryRemoveCash(Game.Fractions.Police.VehicleNumberplateUnRegPrice, out newBalance, true))
                    return null;

                pData.SetCash(newBalance);
            }
            else
            {
                if (!pData.HasBankAccount(true) || !pData.BankAccount.TryRemoveMoneyDebit(Game.Fractions.Police.VehicleNumberplateUnRegPrice, out newBalance, true))
                    return null;

                pData.BankAccount.SetDebitBalance(newBalance, null);
            }

            vInfo.RegisteredNumberplate = null;

            MySQL.VehicleRegisteredNumberplateUpdate(vInfo);

            return vInfo.Numberplate?.Tag ?? string.Empty;
        }
    }
}
