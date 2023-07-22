using GTANetworkAPI;
using System;
using System.Linq;

namespace BlaineRP.Server.Events.Players
{
    class Chat : Script
    {
        [RemoteProc("Chat::Send")]
        private static byte ChatSend(Player player, int typeNum, string message)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Sync.Chat.MessageTypes), typeNum))
                return 0;

            if (message == null)
                return 0;

            message = message.Trim();

            var type = (Sync.Chat.MessageTypes)typeNum;

            if (type > Sync.Chat.MessageTypes.Admin)
                return 0;

            if (pData.IsMuted)
                return 1;

            if (type <= Sync.Chat.MessageTypes.Try)
            {
                if (type == Sync.Chat.MessageTypes.Todo)
                {
                    if (!Sync.Chat.MessageTodoRegex.IsMatch(message))
                        return 0;
                }

                if (!Sync.Chat.MessageRegex.IsMatch(message))
                    return 77;

                Sync.Chat.SendLocal(type, player, message, null);

                return 255;
            }
            else if (type == Sync.Chat.MessageTypes.Fraction)
            {
                if (!Sync.Chat.MessageRegex.IsMatch(message))
                    return 77;

                if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                    return 1;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction);

                if (fData == null)
                    return 0;

                if (!fData.HasMemberPermission(pData.Info, 7, true))
                    return 2;

                if (pData.Info.Punishments.Where(x => x.Type == Sync.Punishment.Types.FractionMute && x.IsActive()).Any())
                    return 3;

                fData.TriggerEventToMembers("Chat::SFM", pData.CID, pData.Player.Id, message);

                return 255;
            }
            else if (type == Sync.Chat.MessageTypes.Department)
            {
                if (!Sync.Chat.MessageRegex.IsMatch(message))
                    return 77;

                if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                    return 1;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction);

                if (fData == null)
                    return 0;

                if (!fData.MetaFlags.HasFlag(Game.Fractions.MetaFlagTypes.IsGov))
                    return 4;

                if (!fData.HasMemberPermission(pData.Info, 9, true))
                    return 2;

                foreach (var x in Game.Fractions.Fraction.All)
                {
                    if (!x.Value.MetaFlags.HasFlag(Game.Fractions.MetaFlagTypes.IsGov))
                        continue;

                    x.Value.TriggerEventToMembers("Chat::SDM", pData.CID, player.Id, message, fData.Type, pData.Info.FractionRank);
                }

                return 255;
            }
            else if (type == Sync.Chat.MessageTypes.Goverment)
            {
                if (!Sync.Chat.MessageRegex.IsMatch(message))
                    return 77;

                if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                    return 1;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction);

                if (fData == null)
                    return 0;

                if (!fData.MetaFlags.HasFlag(Game.Fractions.MetaFlagTypes.IsGov))
                    return 4;

/*                if (!fData.IsLeaderOrWarden(pData.Info, true))
                    return 5;*/

                Sync.Chat.SendGlobal(type, $"{fData.Name} | {fData.Ranks[pData.Info.FractionRank].Name} | {pData.Player.Name}", message, null, null);

                return 0;
            }
            else if (type == Sync.Chat.MessageTypes.Admin)
            {
                if (!Sync.Chat.MessageRegex.IsMatch(message))
                    return 77;

                Sync.Chat.SendGlobal(type, "todo", message);

                return 255;
            }
            else
            {
                return 0;
            }
        }
    }
}
