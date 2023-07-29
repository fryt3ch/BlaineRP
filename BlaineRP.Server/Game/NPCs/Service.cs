using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BlaineRP.Server.Game.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.NPCs
{
    public class NPC : Script
    {
        internal class RemoteEvents
        {
            
        }
        private static Dictionary<string, Vector3> Positions = new Dictionary<string, Vector3>()
        {
            { "vpound_w_0", new Vector3(485.6506f, -54.18661f, 78.30058f) },

            { "vrent_s_0", new Vector3(-718.6724f, 5821.765f, 17.21804f) },

            { "Casino@Cashier_0_0", new Vector3(978.074f, 38.62385f, 74.88191f) },
        };

        private static readonly Dictionary<string, Tuple<string[], Delegate>> _actions = new Dictionary<string, Tuple<string[], Delegate>>();

        public static Vector3 GetPositionById(string npcId) => Positions.GetValueOrDefault(npcId);

        public static Tuple<string[], Delegate> GetActionDataById(string actionId) => _actions.GetValueOrDefault(actionId);

        public static void LoadAll()
        {
            foreach (var method in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsClass).SelectMany(x => x.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)))
            {
                var attr = method.GetCustomAttribute<RemoteActionAttribute>();

                if (attr == null)
                    continue;

                var deleg = method.CreateDelegate(Expression.GetDelegateType(method.GetParameters().Select(parm => parm.ParameterType).Append(method.ReturnType).ToArray()));

                _actions.TryAdd(attr.Id, new Tuple<string[], Delegate>(attr.AllowedNpcIds, deleg));
            }
        }

        public static bool AddNpc(string npcId, Vector3 pos) => Positions.TryAdd(npcId, pos);
    }
}
