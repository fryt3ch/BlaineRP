using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Reflection;
using BlaineRP.Server.EntitiesData.Players;

namespace BlaineRP.Server.Events.Commands
{
    partial class Commands : Script
    {
        [AttributeUsage(AttributeTargets.Method)]
        public class CommandAttribute : Attribute
        {
            public int PermissionLevel { get; set; }

            public string Id { get; set; }

            public CommandAttribute(string Id, int PermissionLevel)
            {
                this.Id = Id;

                this.PermissionLevel = PermissionLevel;
            }
        }

        public class CommandData
        {
            public int PermissionLevel { get; set; }

            public Action<PlayerData, string[]> Action { get; set; }

            public CommandData(int PermissionLevel, Action<PlayerData, string[]> Action)
            {
                this.PermissionLevel = PermissionLevel;

                this.Action = Action;
            }

            public bool IsAllowed(PlayerData pData, bool notify)
            {
                if (pData.AdminLevel >= PermissionLevel)
                    return true;

                if (notify)
                {
                    pData.Player.Notify("ACMD::NA");
                }

                return false;
            }
        }

        public static Dictionary<string, CommandData> All { get; private set; }

        public static void LoadAll()
        {
            if (All != null)
                return;

            All = new Dictionary<string, CommandData>();

            foreach (var method in typeof(Commands).GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
            {
                var attr = method.GetCustomAttribute<CommandAttribute>();

                if (attr == null)
                    continue;

                var deleg = (Action<PlayerData, string[]>)method.CreateDelegate(typeof(Action<PlayerData, string[]>));

                var cmdData = new CommandData(attr.PermissionLevel, deleg);

                All.TryAdd(attr.Id, cmdData);
            }
        }

        [RemoteEvent("Cmd::Exec")]
        private static void CmdExecute(Player player, string cmdId, string argStr)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (argStr == null)
                return;

            var cmdData = All.GetValueOrDefault(cmdId);

            if (cmdData == null)
                return;

            if (!cmdData.IsAllowed(pData, true))
                return;

            cmdData.Action?.Invoke(pData, argStr.Split('&'));
        }
    }
}
