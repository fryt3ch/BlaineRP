using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Events.Commands
{
    partial class Commands
    {
        [Command("p_mutef", 1)]
        private static void FractionMute(PlayerData pData, params string[] args)
        {
            if (args.Length != 3)
                return;

            if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                return;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (!fData.HasMemberPermission(pData.Info, 8, true))
                return;

            uint pid, mins;

            if (!uint.TryParse(args[0], out pid) || !uint.TryParse(args[1], out mins))
                return;

            if (mins < 1)
                mins = 1;
            else if (mins > Settings.FRACTION_MUTE_MAX_MINUTES)
                mins = Settings.FRACTION_MUTE_MAX_MINUTES;

            var reason = (string)args[2];

            var tInfo = pid < Settings.CurrentProfile.Game.CIDBaseOffset ? PlayerData.All.Values.Where(x => x.Player.Id == pid).FirstOrDefault()?.Info : PlayerData.PlayerInfo.Get(pid);

            if (tInfo == null)
            {
                pData.Player.Notify("Cmd::TargetNotFound");

                return;
            }

            if (tInfo.Fraction != fData.Type)
            {
                pData.Player.Notify("Fraction::TGINYF");

                return;
            }

            if (pData.Info.FractionRank <= tInfo.FractionRank)
            {
                pData.Player.Notify("Fraction::HRIBTY");

                return;
            }

            var allMutes = tInfo.Punishments.Where(x => x.Type == Sync.Punishment.Types.FractionMute).ToList();

            if (allMutes.Where(x => x.IsActive()).Any())
            {
                pData.Player.Notify("ASTUFF::PIAMT");

                return;
            }

            var curTime = Utils.GetCurrentTime();

            var punishment = new Sync.Punishment(Sync.Punishment.GetNextId(), Sync.Punishment.Types.FractionMute, reason, curTime, curTime.AddMinutes(mins), pData.CID);

            if (allMutes.Count >= Settings.MAX_PUNISHMENTS_PER_TYPE_HISTORY)
            {
                var oldMute = allMutes.OrderBy(x => x.StartDate).FirstOrDefault();

                if (oldMute != null)
                {
                    allMutes.Remove(oldMute);

                    tInfo.Punishments.Remove(oldMute);

                    MySQL.RemovePunishment(oldMute.Id);
                }
            }

            tInfo.Punishments.Add(punishment);

            MySQL.AddPunishment(tInfo, pData.Info, punishment);

            if (tInfo.PlayerData != null)
            {
                tInfo.PlayerData.Player.TriggerEvent("Player::Punish", punishment.Id, (int)punishment.Type, pData.Player.Id, punishment.EndDate.GetUnixTimestamp(), reason);

                fData.SendFractionChatMessage($"{pData.Player.Name} ({pData.Player.Id}) выдал мут {tInfo.PlayerData.Player.Name} ({tInfo.PlayerData.Player.Id}) на {mins} мин. Причина: {reason}");
            }
            else
            {
                fData.SendFractionChatMessage($"{pData.Player.Name} ({pData.Player.Id}) выдал мут {tInfo.Name} {tInfo.Surname} #{tInfo.CID} на {mins} мин. Причина: {reason}");
            }
        }

        [Command("p_unmutef", 1)]
        private static void FractionUnmute(PlayerData pData, string[] args)
        {
            if (args.Length != 2)
                return;

            if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                return;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (!fData.HasMemberPermission(pData.Info, 8, true))
                return;

            uint pid;

            if (!uint.TryParse(args[0], out pid))
                return;

            var reason = args[1];

            var tInfo = pid < Settings.CurrentProfile.Game.CIDBaseOffset ? PlayerData.All.Values.Where(x => x.Player.Id == pid).FirstOrDefault()?.Info : PlayerData.PlayerInfo.Get(pid);

            if (tInfo == null)
            {
                pData.Player.Notify("Cmd::TargetNotFound");

                return;
            }

            if (tInfo.Fraction != fData.Type)
            {
                pData.Player.Notify("Fraction::TGINYF");

                return;
            }

            if (pData.Info.FractionRank <= tInfo.FractionRank)
            {
                pData.Player.Notify("Fraction::HRIBTY");

                return;
            }

            var actualMute = tInfo.Punishments.Where(x => x.Type == Sync.Punishment.Types.FractionMute && x.IsActive()).FirstOrDefault();

            if (actualMute == null)
            {
                pData.Player.Notify("ASTUFF::PINMT");

                return;
            }

            actualMute.AmnestyInfo = new Sync.Punishment.Amnesty()
            {
                CID = pData.CID,
                Reason = reason,
                Date = Utils.GetCurrentTime(),
            };

            MySQL.UpdatePunishmentAmnesty(actualMute);

            if (tInfo.PlayerData != null)
            {
                tInfo.PlayerData.Player.TriggerEvent("Player::Punish", actualMute.Id, (int)actualMute.Type, pData.Player.Id, -1, reason);

                fData.SendFractionChatMessage($"{pData.Player.Name} ({pData.Player.Id}) снял мут {tInfo.PlayerData.Player.Name} ({tInfo.PlayerData.Player.Id}). Причина: {reason}");
            }
            else
            {
                fData.SendFractionChatMessage($"{pData.Player.Name} ({pData.Player.Id}) снял мут {tInfo.Name} {tInfo.Surname} #{tInfo.CID}. Причина: {reason}");
            }
        }
    }
}
