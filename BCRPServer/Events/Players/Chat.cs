using GTANetworkAPI;
using System;
using System.Linq;

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

            var type = (Sync.Chat.Types)typeNum;

            if (type > Sync.Chat.Types.Admin)
                return;

            if (pData.IsMuted)
                return;

            if (type <= Sync.Chat.Types.Try)
            {
                Sync.Chat.SendLocal(type, player, message, null);
            }
            else if (type == Sync.Chat.Types.Fraction)
            {
                if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                    return;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction);

                if (fData == null)
                    return;

                if (!fData.HasMemberPermission(pData.Info, 6, true))
                    return;

                if (pData.Info.Punishments.Where(x => x.Type == Sync.Punishment.Types.FractionMute && x.IsActive()).Any())
                    return;

                fData.TriggerEventToMembers("Chat::SFM", pData.CID, pData.Player.Id, message);
            }
            else if (type == Sync.Chat.Types.Goverment || type == Sync.Chat.Types.Admin) // add if of who can call
            {
                Sync.Chat.SendGlobal(type, "todo", message);
            }
        }
    }
}
