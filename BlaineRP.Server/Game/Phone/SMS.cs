﻿using BlaineRP.Server.Extensions.System;
using BlaineRP.Server.Game.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Phone
{
    public partial class SMS
    {
        public static string GetDefaultSmsMessage(PredefinedTypes type) => Language.Strings.Get(Language.Strings.GetKeyFromTypeByMemberName(type.GetType(), type.ToString(), "SMS_TEXT") ?? "null");

        public string Data { get; set; }

        public SMS(PlayerInfo SenderInfo, PlayerInfo ReceiverInfo, string Text) : this(SenderInfo.PhoneNumber, ReceiverInfo.PhoneNumber, Text)
        {

        }

        public SMS(uint SenderPhone, PlayerInfo ReceiverInfo, string Text) : this(SenderPhone, ReceiverInfo.PhoneNumber, Text)
        {

        }

        public SMS(PlayerInfo SenderInfo, uint ReceiverPhone, string Text) : this(SenderInfo.PhoneNumber, ReceiverPhone, Text)
        {

        }

        public SMS(uint SenderPhone, uint ReceiverPhone, string Text)
        {
            Data = $"{SenderPhone}_{ReceiverPhone}_{Utils.GetCurrentTime().GetUnixTimestamp()}_{Text}";
        }

        public static void TriggerRemove(Player player, int idx) => player.TriggerEvent("Phone::CSMS", idx);

        public void TriggerAdd(Player player) => player.TriggerEvent("Phone::CSMS", Data);
    }
}

