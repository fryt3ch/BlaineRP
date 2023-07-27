using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.Game.Fractions;
using BlaineRP.Server.Game.Management.Punishments;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Management.Chat
{
    internal partial class Service
    {
        internal class RemoteEvents : Script
        {
            [RemoteProc("Chat::Send")]
            private static byte ChatSend(Player player, int typeNum, string message)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return 0;

                PlayerData pData = sRes.Data;

                if (!Enum.IsDefined(typeof(MessageType), typeNum))
                    return 0;

                if (message == null)
                    return 0;

                message = message.Trim();

                var type = (MessageType)typeNum;

                if (type > MessageType.Admin)
                    return 0;

                if (pData.IsMuted)
                    return 1;

                if (type <= MessageType.Try)
                {
                    if (type == MessageType.Todo)
                        if (!MessageTodoRegex.IsMatch(message))
                            return 0;

                    if (!MessageRegex.IsMatch(message))
                        return 77;

                    SendLocal(type, player, message, null);

                    return 255;
                }
                else if (type == MessageType.Fraction)
                {
                    if (!MessageRegex.IsMatch(message))
                        return 77;

                    if (!Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                        return 1;

                    var fData = Fractions.Fraction.Get(pData.Fraction);

                    if (fData == null)
                        return 0;

                    if (!fData.HasMemberPermission(pData.Info, 7, true))
                        return 2;

                    if (pData.Info.Punishments.Where(x => x.Type == PunishmentType.FractionMute && x.IsActive()).Any())
                        return 3;

                    fData.TriggerEventToMembers("Chat::SFM", pData.CID, pData.Player.Id, message);

                    return 255;
                }
                else if (type == MessageType.Department)
                {
                    if (!MessageRegex.IsMatch(message))
                        return 77;

                    if (!Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                        return 1;

                    var fData = Fractions.Fraction.Get(pData.Fraction);

                    if (fData == null)
                        return 0;

                    if (!fData.MetaFlags.HasFlag(Fractions.Fraction.FlagTypes.IsGov))
                        return 4;

                    if (!fData.HasMemberPermission(pData.Info, 9, true))
                        return 2;

                    foreach (KeyValuePair<FractionType, Fraction> x in Fractions.Fraction.All)
                    {
                        if (!x.Value.MetaFlags.HasFlag(Fractions.Fraction.FlagTypes.IsGov))
                            continue;

                        x.Value.TriggerEventToMembers("Chat::SDM", pData.CID, player.Id, message, fData.Type, pData.Info.FractionRank);
                    }

                    return 255;
                }
                else if (type == MessageType.Goverment)
                {
                    if (!MessageRegex.IsMatch(message))
                        return 77;

                    if (!Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                        return 1;

                    var fData = Fractions.Fraction.Get(pData.Fraction);

                    if (fData == null)
                        return 0;

                    if (!fData.MetaFlags.HasFlag(Fractions.Fraction.FlagTypes.IsGov))
                        return 4;

                    /*                if (!fData.IsLeaderOrWarden(pData.Info, true))
                                        return 5;*/

                    SendGlobal(type, $"{fData.Name} | {fData.Ranks[pData.Info.FractionRank].Name} | {pData.Player.Name}", message, null, null);

                    return 0;
                }
                else if (type == MessageType.Admin)
                {
                    if (!MessageRegex.IsMatch(message))
                        return 77;

                    SendGlobal(type, "todo", message);

                    return 255;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}