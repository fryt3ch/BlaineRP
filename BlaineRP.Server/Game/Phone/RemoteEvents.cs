﻿using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Management.Chat;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Phone
{
    public class RemoteEvents : Script
    {
        private static uint VisualClientPhoneCallPrice { get; } = (60_000 / Properties.Settings.Static.PHONE_CALL_X) * Properties.Settings.Static.PHONE_CALL_COST_X;
        
        [RemoteEvent("Players::SPST")]
        public static void SetPhoneStateType(Player player, byte stateNum)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(PlayerPhoneState), stateNum))
                return;

            var stateType = (PlayerPhoneState)stateNum;

            var curStateType = pData.PhoneStateType;

            if (curStateType == stateType)
                return;

            if (stateType != PlayerPhoneState.Off)
            {
                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                pData.PhoneStateType = stateType;

                if (curStateType == PlayerPhoneState.Off)
                {
                    Game.Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_PHONE_ON"));

                    player.AttachObject(Game.Attachments.Service.Models.Phone, AttachmentType.PhoneSync, -1, null);
                }
            }
            else
            {
                Sync.Players.StopUsePhone(pData);
            }
        }

        [RemoteProc("Phone::GPD")]
        private static string GetPhoneData(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            return $"{pData.Info.PhoneBalance}_{VisualClientPhoneCallPrice}_{Properties.Settings.Static.PHONE_SMS_COST_PER_CHAR}";
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

            if (newPhoneBalance > Properties.Settings.Static.PHONE_MAX_BALANCE)
            {
                player.Notify("Phone::MBA", Properties.Settings.Static.PHONE_MAX_BALANCE);

                return null;
            }

            pData.BankAccount.SetDebitBalance(newBankBalance, null);

            pData.Info.SetPhoneBalance(newPhoneBalance);

            return pData.Info.PhoneBalance;
        }

        [RemoteProc("Phone::CP")]
        private static byte CallPlayer(Player player, uint phoneNumber)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return 0;

            if (phoneNumber < 1_000)
                return 1;

            if (pData.ActiveCall != null)
                return 0;

            var tData = PlayerData.All.Values.Where(x => x.Info.PhoneNumber == phoneNumber).FirstOrDefault();

            if (tData == null || tData == pData)
                return 1;

            if (!pData.Info.TryRemovePhoneBalance(Properties.Settings.Static.PHONE_CALL_COST_X, out _, true))
                return 0;

            if (tData.Info.PhoneBlacklist.Contains(pData.Info.PhoneNumber) || tData.Player.Dimension == Utils.GetPrivateDimension(tData.Player) || pData.IsFrozen)
                return 1;

            if (tData.ActiveCall != null)
                return 2;

            var newCall = new Call(pData, tData);

            return byte.MaxValue;
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
                var activeCall = Call.GetByReceiver(pData);

                if (activeCall == null || activeCall.StatusType == Call.StatusTypes.Process)
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

                activeCall.Cancel(activeCall.Caller == pData ? Call.CancelTypes.Caller : Call.CancelTypes.Receiver);
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

            if (pData.Info.Contacts.Count >= Properties.Settings.Static.PHONE_CONTACTS_MAX_AMOUNT)
            {
                player.Notify("Phone::CMA", Properties.Settings.Static.PHONE_CONTACTS_MAX_AMOUNT);

                return false;
            }

            name = name.Trim();

            if (name.Length > Properties.Settings.Static.PHONE_CONTACT_NAME_MAX_LENGTH)
                return false;

            foreach (var x in name)
                if (!char.IsLetterOrDigit(x) && !char.IsWhiteSpace(x))
                    return false;

            pData.Info.AddOrUpdateContact(phoneNumber, name);

            return true;
        }

        [RemoteProc("Phone::RC")]
        private static bool RemoveContact(Player player, uint phoneNumber)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (!pData.Info.RemoveContact(phoneNumber))
                return false;

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

                if (pData.Info.PhoneBlacklist.Count >= Properties.Settings.Static.PHONE_BLACKLIST_MAX_AMOUNT)
                {
                    player.Notify("Phone::BLMA", Properties.Settings.Static.PHONE_BLACKLIST_MAX_AMOUNT);

                    return false;
                }

                pData.Info.AddPhoneToBlacklist(phoneNumber);
            }
            else
            {
                if (!pData.Info.RemovePhoneFromBlacklist(phoneNumber))
                    return false;
            }

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

            if (phoneNumber < 1_000)
                return string.Empty;

            text = text.Trim();

            var symbolsCount = (uint)text.Length;

            if (symbolsCount < Properties.Settings.Static.PHONE_SMS_MIN_LENGTH || symbolsCount > Properties.Settings.Static.PHONE_SMS_MAX_LENGTH)
                return null;

            uint newPhoneBalance;

            if (!pData.Info.TryRemovePhoneBalance(symbolsCount * Properties.Settings.Static.PHONE_SMS_COST_PER_CHAR, out newPhoneBalance, true))
                return null;

            if (Call.GetByCaller(pData) != null)
                return null;

            var tData = PlayerData.All.Values.Where(x => x.Info.PhoneNumber == phoneNumber).FirstOrDefault();

            if (tData == null || pData == tData)
                return string.Empty;

            if (attachPos)
                text += $"<GEOL>{player.Position.X}_{player.Position.Y}</GEOL>";

            var smsData = new SMS(pData.Info, tData.Info, text);

            pData.Info.AddSms(smsData, false);

            if (!tData.Info.PhoneBlacklist.Contains(pData.Info.PhoneNumber))
                tData.Info.AddSms(smsData, true);

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

            var toDelList = new List<SMS>();

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
    }
}