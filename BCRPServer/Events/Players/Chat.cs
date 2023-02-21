using GTANetworkAPI;
using System;

namespace BCRPServer.Events.Players
{
    class Chat : Script
    {
        [RemoteEvent("Chat::Send")]
        private static void ChatSend(Player player, int typeNum, string message)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Sync.Chat.Types), typeNum))
                return;

            Sync.Chat.Types type = (Sync.Chat.Types)typeNum;

            if (type > Sync.Chat.Types.Admin)
                return;

            if (type <= Sync.Chat.Types.Fraction)
            {
                Sync.Chat.SendLocal(type, player, message, null);
            }
            else if (type == Sync.Chat.Types.Goverment || type == Sync.Chat.Types.Admin) // add if of who can call
            {
                Sync.Chat.SendGlobal(type, player, message);
            }
        }
    }
}
