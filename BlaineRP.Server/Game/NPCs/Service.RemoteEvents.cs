using System;
using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.NPCs
{
    public partial class Service
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("NPC::Action")]
            private static void NPCAction(Player player, string npcId, string actionId, string data)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (data == null)
                    return;

                Vector3 npcPos = GetPositionById(npcId);

                if (npcPos == null)
                    return;

                if (player.Position.DistanceTo(npcPos) > Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE)
                    return;

                Tuple<string[], Delegate> actionData = GetActionDataById(actionId);

                if (actionData == null || !actionData.Item1.Contains(npcId))
                    return;

                if (actionData.Item2 is Action<PlayerData, string, string[]> action)
                    action.Invoke(pData, npcId, data.Split('&'));
            }

            [RemoteProc("NPC::Proc")]
            private static object NPCProc(Player player, string npcId, string procId, string data)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                PlayerData pData = sRes.Data;

                if (data == null)
                    return null;

                Vector3 npcPos = GetPositionById(npcId);

                if (npcPos == null)
                    return null;

                if (player.Position.DistanceTo(npcPos) > Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE)
                    return null;

                Tuple<string[], Delegate> actionData = GetActionDataById(procId);

                if (actionData == null || !actionData.Item1.Contains(npcId))
                    return null;

                if (actionData.Item2 is Func<PlayerData, string, string[], object> action)
                    return action.Invoke(pData, npcId, data.Split('&'));

                return null;
            }
        }
    }
}