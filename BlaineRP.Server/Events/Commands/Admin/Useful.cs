namespace BlaineRP.Server.Events.Commands
{
    partial class Commands
    {
        [Command("p_invis", 1)]
        private static void Invisibility(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            uint pid;
            bool state;

            if (!uint.TryParse(args[0], out pid))
                return;

            var tData = pData;

            if (pData.CID != pid && pData.Player.Id != pid)
            {
                tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    pData.Player.Notify("Cmd::TargetNotFound");

                    return;
                }
            }

            if (args[1].Length == 0)
            {
                tData.IsInvisible = !tData.IsInvisible;

                return;
            }
            else
            {
                if (!bool.TryParse(args[1], out state))
                    return;

                if (tData.IsInvisible == state)
                    return;

                tData.IsInvisible = state;
            }
        }

        [Command("p_fly", 1)]
        private static void Fly(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            uint pid;
            bool state;

            if (!uint.TryParse(args[0], out pid))
                return;

            var tData = pData;

            if (pData.CID != pid && pData.Player.Id != pid)
            {
                tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    pData.Player.Notify("Cmd::TargetNotFound");

                    return;
                }
            }

            if (args[1].Length == 0)
            {
                tData.IsFlyOn = !tData.IsFlyOn;

                return;
            }
            else
            {
                if (!bool.TryParse(args[1], out state))
                    return;

                if (tData.IsFlyOn == state)
                    return;

                tData.IsFlyOn = state;
            }
        }

        [Command("p_gm", 1)]
        private static void GodMode(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            uint pid;
            bool state;

            if (!uint.TryParse(args[0], out pid))
                return;

            var tData = pData;

            if (pData.CID != pid && pData.Player.Id != pid)
            {
                tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    pData.Player.Notify("Cmd::TargetNotFound");

                    return;
                }
            }

            if (args[1].Length == 0)
            {
                tData.IsInvincible = !tData.IsInvincible;

                return;
            }
            else
            {
                if (!bool.TryParse(args[1], out state))
                    return;

                if (tData.IsInvincible == state)
                    return;

                tData.IsInvincible = state;
            }
        }

        [Command("p_hp", 1)]
        private static void SetHealth(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            uint pid;
            int value;

            if (!uint.TryParse(args[0], out pid) || !int.TryParse(args[1], out value))
                return;

            if (value < 0)
                value = 0;
            else if (value > Properties.Settings.Static.PlayerMaxHealth)
                value = Properties.Settings.Static.PlayerMaxHealth;

            var tData = pData;

            if (pData.CID != pid && pData.Player.Id != pid)
            {
                tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    pData.Player.Notify("Cmd::TargetNotFound");

                    return;
                }
            }

            tData.Player.SetHealth(value);
        }

        [Command("p_mood", 1)]
        private static void SetMood(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            uint pid;
            byte value;

            if (!uint.TryParse(args[0], out pid) || !byte.TryParse(args[1], out value))
                return;

            if (value < 0)
                value = 0;
            else if (value > Properties.Settings.Static.PlayerMaxMood)
                value = Properties.Settings.Static.PlayerMaxMood;

            var tData = pData;

            if (pData.CID != pid && pData.Player.Id != pid)
            {
                tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    pData.Player.Notify("Cmd::TargetNotFound");

                    return;
                }
            }

            tData.Mood = value;
        }

        [Command("p_satiety", 1)]
        private static void SetSatiety(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            uint pid;
            byte value;

            if (!uint.TryParse(args[0], out pid) || !byte.TryParse(args[1], out value))
                return;

            if (value < 0)
                value = 0;
            else if (value > Properties.Settings.Static.PlayerMaxSatiety)
                value = Properties.Settings.Static.PlayerMaxSatiety;

            var tData = pData;

            if (pData.CID != pid && pData.Player.Id != pid)
            {
                tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    pData.Player.Notify("Cmd::TargetNotFound");

                    return;
                }
            }

            tData.Satiety = value;
        }

        [Command("p_freeze", 1)]
        private static void Freeze(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            uint pid;
            bool state;

            if (!uint.TryParse(args[0], out pid))
                return;

            var tData = pData;

            if (pData.CID != pid && pData.Player.Id != pid)
            {
                tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    pData.Player.Notify("Cmd::TargetNotFound");

                    return;
                }
            }

            if (args[1].Length == 0)
            {
                tData.IsFrozen = !tData.IsFrozen;

                return;
            }
            else
            {
                if (!bool.TryParse(args[1], out state))
                    return;

                if (tData.IsFrozen == state)
                    return;

                tData.IsFrozen = state;
            }
        }
    }
}
