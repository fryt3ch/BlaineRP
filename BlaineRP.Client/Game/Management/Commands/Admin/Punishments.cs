using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Utils;

namespace BlaineRP.Client.Game.Management.Commands
{
    partial class Service
    {
        [Command("kick", true, "Кикнуть игрока")]
        public static void Kick(uint pid, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            if (!reason.IsTextLengthValid(1, 24, true))
                return;

            CallRemote("p_kick", pid, reason);

            LastSent = World.Core.ServerTime;
        }

        [Command("silentkick", true, "Кикнуть игрока (тихо)", "skick", "kicks", "kicksilent")]
        public static void SilentKick(uint pid, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            if (!reason.IsTextLengthValid(1, 24, true))
                return;

            CallRemote("p_skick", pid, reason);

            LastSent = World.Core.ServerTime;
        }

        [Command("mute", true, "Выдать мут игроку")]
        public static void Mute(uint pid, uint mins, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            if (!Convert.ToDecimal(mins).IsNumberValid<uint>(1, uint.MaxValue, out _, true))
                return;

            if (!reason.IsTextLengthValid(1, 24, true))
                return;

            CallRemote("p_mute", pid, mins, reason);

            LastSent = World.Core.ServerTime;
        }

        [Command("unmute", true, "Снять мут с игрока")]
        public static void Unmute(uint pid, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            if (!reason.IsTextLengthValid(1, 24, true))
                return;

            CallRemote("p_unmute", pid, reason);

            LastSent = World.Core.ServerTime;
        }

        [Command("warn", true, "Выдать предупреждение игроку")]
        public static void Warn(uint pid, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            if (!reason.IsTextLengthValid(1, 24, true))
                return;

            CallRemote("p_warn", pid, reason);

            LastSent = World.Core.ServerTime;
        }

        [Command("unwarn", true, "Снять предупреждения с игрока")]
        public static void Unwarn(uint pid, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            if (!reason.IsTextLengthValid(1, 24, true))
                return;

            CallRemote("p_uwarn", pid, reason);

            LastSent = World.Core.ServerTime;
        }

        [Command("ban", true, "Выдать бан игроку")]
        public static void Ban(uint pid, uint days, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            if (!Convert.ToDecimal(days).IsNumberValid<uint>(1, uint.MaxValue, out _, true))
                return;

            if (!reason.IsTextLengthValid(1, 24, true))
                return;

            CallRemote("p_ban", pid, days, reason);

            LastSent = World.Core.ServerTime;
        }

        [Command("unban", true, "Снять бан с игрока")]
        public static void Unban(uint pid, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            if (!reason.IsTextLengthValid(1, 24, true))
                return;

            CallRemote("p_uban", pid, reason);

            LastSent = World.Core.ServerTime;
        }

        [Command("hardban", true, "Выдать тяжёлый бан игроку")]
        public static void HardBan(uint pid, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            if (!reason.IsTextLengthValid(1, 24, true))
                return;

            CallRemote("p_hban", pid, reason);

            LastSent = World.Core.ServerTime;
        }

        [Command("unhardban", true, "Снять тяжёлый бан с игрока")]
        public static void Unhardban(uint pid, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            if (!reason.IsTextLengthValid(1, 24, true))
                return;

            CallRemote("p_uhban", pid, reason);

            LastSent = World.Core.ServerTime;
        }

        [Command("jail", true, "Посадить игрока в NonRP-тюрьму")]
        public static void Jail(uint pid, uint mins, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            if (!Convert.ToDecimal(mins).IsNumberValid<uint>(1, uint.MaxValue, out _, true))
                return;

            if (!reason.IsTextLengthValid(1, 24, true))
                return;

            CallRemote("p_jail", pid, mins, reason);

            LastSent = World.Core.ServerTime;
        }

        [Command("unjail", true, "Выпустить игрока из NonRP-тюрьмы")]
        public static void Unjail(uint pid, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            if (!reason.IsTextLengthValid(1, 24, true))
                return;

            CallRemote("p_unjail", pid, reason);

            LastSent = World.Core.ServerTime;
        }
    }
}