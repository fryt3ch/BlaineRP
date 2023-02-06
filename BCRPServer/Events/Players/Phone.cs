using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Events.Players
{
    public class Phone : Script
    {
        private static uint VisualClientPhoneCallPrice { get; } = (60_000 / Settings.PHONE_CALL_X) * Settings.PHONE_CALL_COST_X;

        [RemoteProc("Phone::GPD")]
        private static string GetPhoneData(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            return $"{pData.Info.PhoneNumber}_{pData.Info.PhoneBalance}_{VisualClientPhoneCallPrice}_{Settings.PHONE_SMS_COST_PER_CHAR}";
        }

        [RemoteProc("Phone::AB")]
        private static uint? AddBalance(Player player, int amountI)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (amountI <= 0)
                return null;

            if (!pData.HasBankAccount(true))
                return null;

            var amount = (uint)amountI;

            ulong newBankBalance;

            if (!pData.BankAccount.TryRemoveMoneyDebit((ulong)amountI, out newBankBalance, true))
                return null;

            uint newPhoneBalance;

            if (!pData.Info.TryAddPhoneBalance(amount, out newPhoneBalance, true))
                return null;

            if (newPhoneBalance > Settings.PHONE_MAX_BALANCE)
            {
                player.Notify("Phone::MBA", Settings.PHONE_MAX_BALANCE);

                return null;
            }

            pData.BankAccount.SetDebitBalance(newBankBalance, null);

            pData.Info.SetPhoneBalance(newPhoneBalance);

            return pData.Info.PhoneBalance;
        }

        [RemoteProc("Phone::CP")]
        private static bool CallPlayer(Player player, uint phoneNumber)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return false;

            if (pData.ActiveCall != null)
                return false;

            var tData = PlayerData.All.Values.FirstOrDefault(x => x.Info.PhoneNumber == phoneNumber);

            if (tData == null || tData == pData)
                return false;

            if (pData.Info.PhoneBalance < Settings.PHONE_CALL_COST_X)
            {
                // todo notify

                return false;
            }

            if (tData.Info.PhoneBlacklist.Contains(pData.Info.PhoneNumber) || tData.Player.Dimension == Utils.GetPrivateDimension(tData.Player) || pData.IsFrozen)
                return false;

            if (tData.ActiveCall != null)
            {
                // todo notify

                return false;
            }

            var newCall = new Sync.Phone.Call(pData, tData);

            return true;
        }

        [RemoteEvent("Phone::CC")]
        private static void CancelCall(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var activeCall = pData.ActiveCall;

            if (activeCall == null)
                return;

            activeCall.Cancel(activeCall.Caller == pData ? Sync.Phone.Call.CancelTypes.Caller : Sync.Phone.Call.CancelTypes.Receiver);
        }

        [RemoteProc("Phone::AC")]
        private static bool AddContact(Player player, uint phoneNumber, string name)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.Info.Contacts.ContainsKey(phoneNumber))
                return false;

            // add name length check and max contacts check

            pData.Info.Contacts.Add(phoneNumber, name);

            MySQL.CharacterContactsUpdate(pData.Info);

            return true;
        }

        [RemoteProc("Phone::RC")]
        private static bool RemoveContact(Player player, uint phoneNumber)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (!pData.Info.Contacts.Remove(phoneNumber))
                return false;

            MySQL.CharacterContactsUpdate(pData.Info);

            return true;
        }

        [RemoteProc("Phone::BLC")]
        private static bool BlacklistChange(Player player, uint phoneNumber, bool add)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (add)
            {
                if (pData.Info.PhoneBlacklist.Contains(phoneNumber))
                    return false;

                // add size check

                pData.Info.PhoneBlacklist.Add(phoneNumber);
            }
            else
            {
                if (!pData.Info.PhoneBlacklist.Remove(phoneNumber))
                    return false;
            }

            MySQL.CharacterPhoneBlacklistUpdate(pData.Info);

            return true;
        }

        [RemoteProc("Phone::SSMS")]
        private static bool SendSms(Player player, uint phoneNumber, string text)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (!pData.Info.Contacts.ContainsKey(phoneNumber))
                return false;

            pData.Info.Contacts.Remove(phoneNumber);

            MySQL.CharacterContactsUpdate(pData.Info);

            return true;
        }
    }
}
