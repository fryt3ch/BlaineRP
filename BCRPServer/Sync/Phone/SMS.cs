using GTANetworkAPI;
using System;
using System.Net.Mail;

namespace BCRPServer.Sync.Phone
{
	public class SMS
	{
        public string Data { get; set; }

        public SMS(PlayerData.PlayerInfo SenderInfo, PlayerData.PlayerInfo ReceiverInfo, string Text) : this(SenderInfo.PhoneNumber, ReceiverInfo.PhoneNumber, Text)
        {

        }

        public SMS(uint SenderPhone, PlayerData.PlayerInfo ReceiverInfo, string Text) : this(SenderPhone, ReceiverInfo.PhoneNumber, Text)
        {

        }

        public SMS(PlayerData.PlayerInfo SenderInfo, uint ReceiverPhone, string Text) : this(SenderInfo.PhoneNumber, ReceiverPhone, Text)
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

