﻿using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Events.Commands
{
    partial class Commands
    {
        [Command("p_kick", 1)]
        private static void Kick(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            var reason = args[1];

            uint pid;

            if (!uint.TryParse(args[0], out pid))
                return;

            var target = Utils.FindPlayerOnline(pid);

            if (target?.Exists != true)
            {
                pData.Player.Notify("Cmd::TargetNotFound");

                return;
            }

            var tData = target.GetMainData();

            if (tData == null)
            {
                Sync.Chat.SendGlobal(Sync.Chat.Types.Kick, $"{pData.Player.Name} ({pData.Player.Id})", reason, $"NOT_AUTH ({target.Id})");
            }
            else
            {
                Sync.Chat.SendGlobal(Sync.Chat.Types.Kick, $"{pData.Player.Name} ({pData.Player.Id})", reason, $"{tData.Name} {tData.Surname} ({target.Id})");
            }

            target.Notify("KickA", $"{pData.Name} {pData.Surname} #{pData.CID}", reason);

            Utils.Kick(target, null);
        }

        [Command("p_skick", 1)]
        private static void SilentKick(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            var reason = args[1];

            uint pid;

            if (!uint.TryParse(args[0], out pid))
                return;

            var target = Utils.FindPlayerOnline(pid);

            if (target?.Exists != true)
            {
                pData.Player.Notify("Cmd::TargetNotFound");

                return;
            }

            var tData = target.GetMainData();

            if (tData == null)
            {
                Utils.MsgToAdmins(string.Format(Locale.Chat.Admin.SilentKick, $"{pData.Name} {pData.Surname} #{pData.CID}", $"NOT_AUTH ({target.Id})", reason));
            }
            else
            {
                Utils.MsgToAdmins(string.Format(Locale.Chat.Admin.SilentKick, $"{pData.Name} {pData.Surname} #{pData.CID}", $"{tData.Name} {tData.Surname} #{tData.CID}", reason));
            }

            target.Notify("KickA", $"{pData.Name} {pData.Surname} #{pData.CID}", reason);

            Utils.Kick(target, null);
        }

        [Command("p_mute", 1)]
        private static void Mute(PlayerData pData, params string[] args)
        {
            if (args.Length != 3)
                return;

            uint pid, mins;

            if (!uint.TryParse(args[0], out pid) || !uint.TryParse(args[1], out mins))
                return;

            var reason = (string)args[2];

            var tInfo = pid < Utils.FirstCID ? PlayerData.All.Values.Where(x => x.Player.Id == pid).FirstOrDefault()?.Info : PlayerData.PlayerInfo.Get(pid);
            
            if (tInfo == null)
            {
                pData.Player.Notify("Cmd::TargetNotFound");

                return;
            }

            var allMutes = tInfo.Punishments.Where(x => x.Type == Sync.Punishment.Types.Mute).ToList();

            if (allMutes.Where(x => x.IsActive()).Any())
            {
                pData.Player.Notify("ASTUFF::PIAMT");

                return;
            }

            var curTime = Utils.GetCurrentTime();

            var punishment = new Sync.Punishment(Sync.Punishment.GetNextId(), Sync.Punishment.Types.Mute, reason, curTime, curTime.AddMinutes(mins), pData.CID);

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
                tInfo.PlayerData.IsMuted = true;

                tInfo.PlayerData.Player.TriggerEvent("Player::Punish", punishment.Id, (int)punishment.Type, pData.Player.Id, punishment.EndDate.GetUnixTimestamp(), reason);

                Sync.Chat.SendGlobal(Sync.Chat.Types.Mute, $"{pData.Player.Name} ({pData.Player.Id})", reason, $"{tInfo.PlayerData.Player.Name} ({tInfo.PlayerData.Player.Id})", $"{mins}");
            }
            else
            {
                Sync.Chat.SendGlobal(Sync.Chat.Types.Mute, $"{pData.Player.Name} ({pData.Player.Id})", reason, $"{tInfo.Name} {tInfo.Surname} #{tInfo.CID}", $"{mins}");
            }
        }

        [Command("p_unmute", 1)]
        private static void Unmute(PlayerData pData, string[] args)
        {
            if (args.Length != 2)
                return;

            uint pid;

            if (!uint.TryParse(args[0], out pid))
                return;

            var reason = args[1];

            var tInfo = pid < Utils.FirstCID ? PlayerData.All.Values.Where(x => x.Player.Id == pid).FirstOrDefault()?.Info : PlayerData.PlayerInfo.Get(pid);

            if (tInfo == null)
            {
                pData.Player.Notify("Cmd::TargetNotFound");

                return;
            }

            var actualMute = tInfo.Punishments.Where(x => x.Type == Sync.Punishment.Types.Mute && x.IsActive()).FirstOrDefault();

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
                tInfo.PlayerData.IsMuted = false;

                tInfo.PlayerData.Player.TriggerEvent("Player::Punish", actualMute.Id, (int)actualMute.Type, pData.Player.Id, -1, reason);

                Sync.Chat.SendGlobal(Sync.Chat.Types.UnMute, $"{pData.Player.Name} ({pData.Player.Id})", reason, $"{tInfo.PlayerData.Player.Name} ({tInfo.PlayerData.Player.Id})", null);
            }
            else
            {
                Sync.Chat.SendGlobal(Sync.Chat.Types.UnMute, $"{pData.Player.Name} ({pData.Player.Id})", reason, $"{tInfo.Name} {tInfo.Surname} #{tInfo.CID}", null);
            }
        }

        [Command("p_jail", 1)]
        private static void Jail(PlayerData pData, params string[] args)
        {
            if (args.Length != 3)
                return;

            uint pid, mins;

            if (!uint.TryParse(args[0], out pid) || !uint.TryParse(args[1], out mins))
                return;

            var reason = (string)args[2];

            var tInfo = pid < Utils.FirstCID ? PlayerData.All.Values.Where(x => x.Player.Id == pid).FirstOrDefault()?.Info : PlayerData.PlayerInfo.Get(pid);

            if (tInfo == null)
            {
                pData.Player.Notify("Cmd::TargetNotFound");

                return;
            }

            var allJails = tInfo.Punishments.Where(x => x.Type == Sync.Punishment.Types.NRPPrison).ToList();

            if (allJails.Where(x => x.IsActive()).Any())
            {
                pData.Player.Notify("ASTUFF::PIAJL");

                return;
            }

            var curTime = Utils.GetCurrentTime();

            var punishment = new Sync.Punishment(Sync.Punishment.GetNextId(), Sync.Punishment.Types.NRPPrison, reason, curTime, curTime.AddMinutes(mins), pData.CID);

            if (allJails.Count >= Settings.MAX_PUNISHMENTS_PER_TYPE_HISTORY)
            {
                var oldJail = allJails.OrderBy(x => x.StartDate).FirstOrDefault();

                if (oldJail != null)
                {
                    allJails.Remove(oldJail);

                    tInfo.Punishments.Remove(oldJail);

                    MySQL.RemovePunishment(oldJail.Id);
                }
            }

            tInfo.Punishments.Add(punishment);

            MySQL.AddPunishment(tInfo, pData.Info, punishment);

            if (tInfo.PlayerData != null)
            {
                tInfo.PlayerData.Player.TriggerEvent("Player::Punish", punishment.Id, (int)punishment.Type, pData.Player.Id, punishment.EndDate.GetUnixTimestamp(), reason);

                Sync.Chat.SendGlobal(Sync.Chat.Types.Jail, $"{pData.Player.Name} ({pData.Player.Id})", reason, $"{tInfo.PlayerData.Player.Name} ({tInfo.PlayerData.Player.Id})", $"{mins}");

                Utils.Demorgan.SetToDemorgan(tInfo.PlayerData, false);
            }
            else
            {
                Sync.Chat.SendGlobal(Sync.Chat.Types.Jail, $"{pData.Player.Name} ({pData.Player.Id})", reason, $"{tInfo.Name} {tInfo.Surname} #{tInfo.CID}", $"{mins}");
            }

            if (tInfo.Fraction != Game.Fractions.Types.None)
            {
                var fData = Game.Fractions.Fraction.Get(tInfo.Fraction);

                fData.OnMemberStatusChange(tInfo, fData.GetMemberStatus(tInfo));
            }
        }

        [Command("p_unjail", 1)]
        private static void Unjail(PlayerData pData, string[] args)
        {
            if (args.Length != 2)
                return;

            uint pid;

            if (!uint.TryParse(args[0], out pid))
                return;

            var reason = args[1];

            var tInfo = pid < Utils.FirstCID ? PlayerData.All.Values.Where(x => x.Player.Id == pid).FirstOrDefault()?.Info : PlayerData.PlayerInfo.Get(pid);

            if (tInfo == null)
            {
                pData.Player.Notify("Cmd::TargetNotFound");

                return;
            }

            var actualJail = tInfo.Punishments.Where(x => x.Type == Sync.Punishment.Types.NRPPrison && x.IsActive()).FirstOrDefault();

            if (actualJail == null)
            {
                pData.Player.Notify("ASTUFF::PINJL");

                return;
            }

            actualJail.AmnestyInfo = new Sync.Punishment.Amnesty()
            {
                CID = pData.CID,
                Reason = reason,
                Date = Utils.GetCurrentTime(),
            };

            MySQL.UpdatePunishmentAmnesty(actualJail);

            if (tInfo.PlayerData != null)
            {
                tInfo.PlayerData.Player.TriggerEvent("Player::Punish", actualJail.Id, (int)actualJail.Type, pData.Player.Id, -1, reason);

                Sync.Chat.SendGlobal(Sync.Chat.Types.UnJail, $"{pData.Player.Name} ({pData.Player.Id})", reason, $"{tInfo.PlayerData.Player.Name} ({tInfo.PlayerData.Player.Id})", null);

                Utils.Demorgan.SetFromDemorgan(tInfo.PlayerData);
            }
            else
            {
                Sync.Chat.SendGlobal(Sync.Chat.Types.UnJail, $"{pData.Player.Name} ({pData.Player.Id})", reason, $"{tInfo.Name} {tInfo.Surname} #{tInfo.CID}", null);
            }

            if (tInfo.Fraction != Game.Fractions.Types.None)
            {
                var fData = Game.Fractions.Fraction.Get(tInfo.Fraction);

                fData.OnMemberStatusChange(tInfo, fData.GetMemberStatus(tInfo));
            }
        }

        [Command("p_warn", 1)]
        private static void Warn(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            uint pid;

            if (!uint.TryParse(args[0], out pid))
                return;

            var reason = (string)args[1];

            var tInfo = pid < Utils.FirstCID ? PlayerData.All.Values.Where(x => x.Player.Id == pid).FirstOrDefault()?.Info : PlayerData.PlayerInfo.Get(pid);

            if (tInfo == null)
            {
                pData.Player.Notify("Cmd::TargetNotFound");

                return;
            }

            var allWarns = tInfo.Punishments.Where(x => x.Type == Sync.Punishment.Types.Warn).ToList();

            if (allWarns.Count == Settings.MAX_WARNS_BEFORE_BAN)
            {
                pData.Player.Notify("ASTUFF::PAHMW");

                return;
            }

            var curTime = Utils.GetCurrentTime();

            var punishment = new Sync.Punishment(Sync.Punishment.GetNextId(), Sync.Punishment.Types.Warn, reason, curTime, curTime.AddDays(Settings.WARN_DAYS_TO_UNWARN), pData.CID);

            if (allWarns.Count >= Settings.MAX_PUNISHMENTS_PER_TYPE_HISTORY)
            {
                var oldJail = allWarns.OrderBy(x => x.StartDate).FirstOrDefault();

                if (oldJail != null)
                {
                    allWarns.Remove(oldJail);

                    tInfo.Punishments.Remove(oldJail);

                    MySQL.RemovePunishment(oldJail.Id);
                }
            }

            tInfo.Punishments.Add(punishment);

            MySQL.AddPunishment(tInfo, pData.Info, punishment);

            if (tInfo.PlayerData != null)
            {
                tInfo.PlayerData.Player.TriggerEvent("Player::Punish", punishment.Id, (int)punishment.Type, pData.Player.Id, punishment.EndDate.GetUnixTimestamp(), reason);

                Sync.Chat.SendGlobal(Sync.Chat.Types.Warn, $"{pData.Player.Name} ({pData.Player.Id})", reason, $"{tInfo.PlayerData.Player.Name} ({tInfo.PlayerData.Player.Id})", null);
            }
            else
            {
                Sync.Chat.SendGlobal(Sync.Chat.Types.Warn, $"{pData.Player.Name} ({pData.Player.Id})", reason, $"{tInfo.Name} {tInfo.Surname} #{tInfo.CID}", null);
            }
        }

        [Command("p_unwarn", 1)]
        private static void Unwarn(PlayerData pData, string[] args)
        {
            if (args.Length != 3)
                return;

            uint pid, warnId;

            if (!uint.TryParse(args[0], out pid) || !uint.TryParse(args[1], out warnId))
                return;

            var reason = args[2];

            var tInfo = pid < Utils.FirstCID ? PlayerData.All.Values.Where(x => x.Player.Id == pid).FirstOrDefault()?.Info : PlayerData.PlayerInfo.Get(pid);

            if (tInfo == null)
            {
                pData.Player.Notify("Cmd::TargetNotFound");

                return;
            }

            var actualWarn = tInfo.Punishments.Where(x => x.Type == Sync.Punishment.Types.Warn && x.Id == warnId && x.IsActive()).FirstOrDefault();

            if (actualWarn == null)
            {
                pData.Player.Notify("ASTUFF::PWNFOF");

                return;
            }

            actualWarn.AmnestyInfo = new Sync.Punishment.Amnesty()
            {
                CID = pData.CID,
                Reason = reason,
                Date = Utils.GetCurrentTime(),
            };

            MySQL.UpdatePunishmentAmnesty(actualWarn);

            if (tInfo.PlayerData != null)
            {
                tInfo.PlayerData.Player.TriggerEvent("Player::Punish", actualWarn.Id, (int)actualWarn.Type, pData.Player.Id, -1, reason);

                Sync.Chat.SendGlobal(Sync.Chat.Types.UnWarn, $"{pData.Player.Name} ({pData.Player.Id})", reason, $"{tInfo.PlayerData.Player.Name} ({tInfo.PlayerData.Player.Id})", null);
            }
            else
            {
                Sync.Chat.SendGlobal(Sync.Chat.Types.UnWarn, $"{pData.Player.Name} ({pData.Player.Id})", reason, $"{tInfo.Name} {tInfo.Surname} #{tInfo.CID}", null);
            }
        }

        [Command("p_ban", 1)]
        private static void Ban(PlayerData pData, params string[] args)
        {
            if (args.Length != 3)
                return;

            uint pid, days;

            if (!uint.TryParse(args[0], out pid) || !uint.TryParse(args[1], out days))
                return;

            var reason = (string)args[2];

            var tInfo = pid < Utils.FirstCID ? PlayerData.All.Values.Where(x => x.Player.Id == pid).FirstOrDefault()?.Info : PlayerData.PlayerInfo.Get(pid);

            if (tInfo == null)
            {
                pData.Player.Notify("Cmd::TargetNotFound");

                return;
            }

            var allBans = tInfo.Punishments.Where(x => x.Type == Sync.Punishment.Types.Ban).ToList();

            if (allBans.Where(x => x.IsActive()).Any())
            {
                pData.Player.Notify("ASTUFF::PIAB");

                return;
            }

            var curTime = Utils.GetCurrentTime();

            var punishment = new Sync.Punishment(Sync.Punishment.GetNextId(), Sync.Punishment.Types.Ban, reason, curTime, curTime.AddDays(days), pData.CID);

            if (allBans.Count >= Settings.MAX_PUNISHMENTS_PER_TYPE_HISTORY)
            {
                var oldJail = allBans.OrderBy(x => x.StartDate).FirstOrDefault();

                if (oldJail != null)
                {
                    allBans.Remove(oldJail);

                    tInfo.Punishments.Remove(oldJail);

                    MySQL.RemovePunishment(oldJail.Id);
                }
            }

            tInfo.Punishments.Add(punishment);

            MySQL.AddPunishment(tInfo, pData.Info, punishment);

            if (tInfo.PlayerData != null)
            {
                Sync.Chat.SendGlobal(Sync.Chat.Types.Ban, $"{pData.Player.Name} ({pData.Player.Id})", reason, $"{tInfo.PlayerData.Player.Name} ({tInfo.PlayerData.Player.Id})", $"{days}");

                tInfo.PlayerData.Player.Notify("KickB", $"{pData.Player.Name} #{pData.CID}", reason, punishment.EndDate.ToString("dd.MM.yyyy HH:mm:ss"));

                Utils.Kick(tInfo.PlayerData.Player, null);
            }
            else
            {
                Sync.Chat.SendGlobal(Sync.Chat.Types.Ban, $"{pData.Player.Name} ({pData.Player.Id})", reason, $"{tInfo.Name} {tInfo.Surname} #{tInfo.CID}", $"{days}");
            }

            if (tInfo.Fraction != Game.Fractions.Types.None)
            {
                var fData = Game.Fractions.Fraction.Get(tInfo.Fraction);

                fData.OnMemberStatusChange(tInfo, fData.GetMemberStatus(tInfo));
            }
        }

        [Command("p_unban", 1)]
        private static void Unban(PlayerData pData, string[] args)
        {
            if (args.Length != 2)
                return;

            uint pid;

            if (!uint.TryParse(args[0], out pid))
                return;

            var reason = args[1];

            var tInfo = pid < Utils.FirstCID ? PlayerData.All.Values.Where(x => x.Player.Id == pid).FirstOrDefault()?.Info : PlayerData.PlayerInfo.Get(pid);

            if (tInfo == null)
            {
                pData.Player.Notify("Cmd::TargetNotFound");

                return;
            }

            var actualBan = tInfo.Punishments.Where(x => x.Type == Sync.Punishment.Types.Ban && x.IsActive()).FirstOrDefault();

            if (actualBan == null)
            {
                pData.Player.Notify("ASTUFF::PNB");

                return;
            }

            actualBan.AmnestyInfo = new Sync.Punishment.Amnesty()
            {
                CID = pData.CID,
                Reason = reason,
                Date = Utils.GetCurrentTime(),
            };

            MySQL.UpdatePunishmentAmnesty(actualBan);

            Sync.Chat.SendGlobal(Sync.Chat.Types.UnBan, $"{pData.Player.Name} ({pData.Player.Id})", reason, $"{tInfo.Name} {tInfo.Surname} #{tInfo.CID}", null);

            if (tInfo.Fraction != Game.Fractions.Types.None)
            {
                var fData = Game.Fractions.Fraction.Get(tInfo.Fraction);

                fData.OnMemberStatusChange(tInfo, fData.GetMemberStatus(tInfo));
            }
        }
    }
}