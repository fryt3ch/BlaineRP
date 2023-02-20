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

            return $"{pData.Info.PhoneBalance}_{VisualClientPhoneCallPrice}_{Settings.PHONE_SMS_COST_PER_CHAR}";
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

            var tData = PlayerData.All.Values.Where(x => x.Info.PhoneNumber == phoneNumber).FirstOrDefault();

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

        [RemoteEvent("Phone::CA")]
        private static void CallAns(Player player, bool ans)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (ans)
            {
                var activeCall = Sync.Phone.Call.GetByReceiver(pData);

                if (activeCall == null || activeCall.StatusType == Sync.Phone.Call.StatusTypes.Process)
                    return;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                activeCall.SetAsProcess();
            }
            else
            {
                var activeCall = pData.ActiveCall;

                if (activeCall == null)
                    return;

                activeCall.Cancel(activeCall.Caller == pData ? Sync.Phone.Call.CancelTypes.Caller : Sync.Phone.Call.CancelTypes.Receiver);
            }
        }

        [RemoteProc("Phone::CC")]
        private static bool ChangeContact(Player player, uint phoneNumber, string name)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (name == null)
                return false;

            if (pData.Info.Contacts.Count >= Settings.PHONE_CONTACTS_MAX_AMOUNT)
            {
                // todo notify

                return false;
            }

            name = name.Trim();

            if (name.Length > Settings.PHONE_CONTACT_NAME_MAX_LENGTH)
                return false;

            foreach (var x in name)
                if (!char.IsLetterOrDigit(x) && !char.IsWhiteSpace(x))
                    return false;

            if (!pData.Info.Contacts.TryAdd(phoneNumber, name))
                pData.Info.Contacts[phoneNumber] = name;

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

                if (pData.Info.PhoneBlacklist.Count >= Settings.PHONE_BLACKLIST_MAX_AMOUNT)
                {
                    // todo notify

                    return false;
                }

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
        private static string SendSms(Player player, uint phoneNumber, string text, bool attachPos)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (text == null)
                return null;

            if (text.Length > Settings.PHONE_SMS_MAX_LENGTH)
            {
                return null;
            }

            uint newPhoneBalance;

            var symbolsCount = (uint)text.Length;

            if (!pData.Info.TryRemovePhoneBalance(symbolsCount * Settings.PHONE_SMS_COST_PER_CHAR, out newPhoneBalance, true))
                return null;

            if (Sync.Phone.Call.GetByCaller(pData) != null)
                return null;

            var tData = PlayerData.All.Values.Where(x => x.Info.PhoneNumber == phoneNumber).FirstOrDefault();

            if (tData == null || pData == tData)
                return null;

            if (tData.Info.PhoneBlacklist.Contains(pData.Info.PhoneNumber))
                return null;

            if (pData.Info.AllSMS.Count >= Settings.PHONE_SMS_MAX_COUNT)
            {
                pData.Info.AllSMS.RemoveAt(0);

                Sync.Phone.SMS.TriggerRemove(player, 0);
            }

            if (attachPos)
                text += $"<GEOL>{player.Position.X}_{player.Position.Y}</GEOL>";

            var smsData = new Sync.Phone.SMS(pData.Info, tData.Info, text);

            pData.Info.AllSMS.Add(smsData);
            tData.Info.AllSMS.Add(smsData);

            smsData.TriggerAdd(tData.Player);

            return smsData.Data;
        }

        [RemoteProc("Phone::DSMS")]
        private static bool DeleteSms(Player player, byte[] nums)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (nums == null || nums.Length == 0 || nums.Length > pData.Info.AllSMS.Count)
                return false;

            var toDelList = new List<Sync.Phone.SMS>();

            for (int i = 0; i < nums.Length; i++)
            {
                if (nums[i] > pData.Info.AllSMS.Count)
                    return false;

                toDelList.Add(pData.Info.AllSMS[nums[i]]);
            }

            toDelList.ForEach(x =>
            {
                pData.Info.AllSMS.Remove(x);
            });

            return true;
        }

        [RemoteProc("Taxi::NO")]
        private static byte TaxiNewOrder(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return 0;

            if (player.Dimension != Utils.Dimensions.Main)
            {
                return 0;
            }

            if (pData.CurrentTaxiOrder != null)
                return 0;

            Game.Jobs.Cabbie.AddPlayerOrder(pData);

            return byte.MaxValue;
        }

        [RemoteEvent("Taxi::CO")]
        private static void TaxiCancelOrder(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var curOrderPair = Game.Jobs.Cabbie.ActiveOrders.Where(x => x.Value.Entity == player).FirstOrDefault();

            if (curOrderPair.Value == null)
                return;

            Game.Jobs.Cabbie.RemoveOrder(curOrderPair.Key, curOrderPair.Value);
        }
    }
}
