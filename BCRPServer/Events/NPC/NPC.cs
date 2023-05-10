using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BCRPServer.Events.NPC
{
    public class NPC : Script
    {
        [AttributeUsage(AttributeTargets.Method)]
        public class ActionAttribute : Attribute
        {
            public string Id { get; set; }

            public string[] AllowedNpcIds { get; set; }

            public ActionAttribute(string Id, params string[] AllowedNpcIds)
            {
                this.Id = Id;

                this.AllowedNpcIds = AllowedNpcIds;
            }
        }

        private static Dictionary<string, Vector3> Positions = new Dictionary<string, Vector3>()
        {
            { "vpound_w_0", new Vector3(485.6506f, -54.18661f, 78.30058f) },

            { "vrent_s_0", new Vector3(-718.6724f, 5821.765f, 17.21804f) },

            { "Casino@Cashier_0_0", new Vector3(978.074f, 38.62385f, 74.88191f) },
        };

        private static Dictionary<string, Tuple<string[], Delegate>> Actions;

        public static Vector3 GetPositionById(string npcId) => Positions.GetValueOrDefault(npcId);

        public static Tuple<string[], Delegate> GetActionDataById(string actionId) => Actions.GetValueOrDefault(actionId);

        public static void LoadAll()
        {
            if (Actions != null)
                return;

            Actions = new Dictionary<string, Tuple<string[], Delegate>>();

            foreach (var method in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsClass).SelectMany(x => x.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)))
            {
                var attr = method.GetCustomAttribute<ActionAttribute>();

                if (attr == null)
                    continue;

                var deleg = method.CreateDelegate(Expression.GetDelegateType(method.GetParameters().Select(parm => parm.ParameterType).Append(method.ReturnType).ToArray()));

                Actions.TryAdd(attr.Id, new Tuple<string[], Delegate>(attr.AllowedNpcIds, deleg));
            }
        }

        public static bool AddNpc(string npcId, Vector3 pos) => Positions.TryAdd(npcId, pos);

        [RemoteEvent("NPC::Action")]
        private static void NPCAction(Player player, string npcId, string actionId, string data)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (data == null)
                return;

            var npcPos = GetPositionById(npcId);

            if (npcPos == null)
                return;

            if (player.Position.DistanceTo(npcPos) > Settings.ENTITY_INTERACTION_MAX_DISTANCE)
                return;

            var actionData = GetActionDataById(actionId);

            if (actionData == null || !actionData.Item1.Contains(npcId))
                return;

            if (actionData.Item2 is Action<PlayerData, string, string[]> action)
            {
                action.Invoke(pData, npcId, data.Split('&'));
            }
        }

        [RemoteProc("NPC::Proc")]
        private static object NPCProc(Player player, string npcId, string procId, string data)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (data == null)
                return null;

            var npcPos = GetPositionById(npcId);

            if (npcPos == null)
                return null;

            if (player.Position.DistanceTo(npcPos) > Settings.ENTITY_INTERACTION_MAX_DISTANCE)
                return null;

            var actionData = GetActionDataById(procId);

            if (actionData == null || !actionData.Item1.Contains(npcId))
                return null;

            if (actionData.Item2 is Func<PlayerData, string, string[], object> action)
            {
                return action.Invoke(pData, npcId, data.Split('&'));
            }

            return null;
        }
    }
}
