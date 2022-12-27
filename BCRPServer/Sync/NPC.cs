using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

namespace BCRPServer.Sync
{
    public static class NPC
    {
        private static Dictionary<string, Vector3> Positions = new Dictionary<string, Vector3>()
        {
            { "vpound_w_0", new Vector3(485.6506f, -54.18661f, 78.30058f) }
        };

        private static Dictionary<List<string>, List<string>> AllowedActionsProcs = new Dictionary<List<string>, List<string>>()
        {
            {
                new List<string>()
                {
                    "vpound_w_0",
                },

                new List<string>()
                {
                    "vpound_d",
                }
            }
        };

        private static Dictionary<string, Action<PlayerData, string>> Actions = new Dictionary<string, Action<PlayerData, string>>()
        {

        };

        private static Dictionary<string, Func<PlayerData, string, object>> Procedures = new Dictionary<string, Func<PlayerData, string, object>>()
        {
            {
                "vpound_d",

                (pData, data) =>
                {
                    var vehsOnPound = pData.Info.VehiclesOnPound.ToList();

                    if (vehsOnPound.Count == 0)
                        return null;

                    return $"{Settings.VEHICLEPOUND_PAY_PRICE}_{string.Join('_', vehsOnPound.Select(x => x.VID))}";
                }
            }
        };

        public static Vector3 GetPositionById(string npcId) => Positions.GetValueOrDefault(npcId);

        public static Action<PlayerData, string> GetActionById(string actionId) => Actions.GetValueOrDefault(actionId);

        public static Func<PlayerData, string, object> GetProcById(string procId) => Procedures.GetValueOrDefault(procId);

        public static bool IsNpcAllowedTo(string npcId, string actionProcId) => AllowedActionsProcs.Where(x => x.Key.Contains(npcId) && x.Value.Contains(actionProcId)).Any();
    }
}
