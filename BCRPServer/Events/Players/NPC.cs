using GTANetworkAPI;
using static BCRPServer.Sync.NPC;

namespace BCRPServer.Events.Players
{
    class NPC : Script
    {
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

            if (!IsNpcAllowedTo(npcId, actionId))
                return;

            var action = GetActionById(actionId);

            if (action == null)
                return;

            action.Invoke(pData, data.Split('&'));
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

            if (!IsNpcAllowedTo(npcId, procId))
                return null;

            var proc = GetProcById(procId);

            if (proc == null)
                return null;

            return proc.Invoke(pData, npcId, data.Split('&'));
        }
    }
}
