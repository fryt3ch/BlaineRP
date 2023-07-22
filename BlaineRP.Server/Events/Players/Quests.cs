using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Server.Events.Players
{
    public class Quests : Script
    {
        [RemoteProc("Quest::PU")]
        private static byte ProgressUpdate(Player player, int questTypeNum, string data)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (data == null)
                return 0;

            if (!Enum.IsDefined(typeof(Sync.Quest.QuestData.Types), questTypeNum))
                return 0;

            var questType = (Sync.Quest.QuestData.Types)questTypeNum;

            var questData = pData.Info.Quests.GetValueOrDefault(questType);

            if (questData == null)
                return 0;

            var func = Sync.Quest.QuestData.All.GetValueOrDefault(questType)?.ProgressUpdateFunc;

            if (func == null)
                return 0;

            return func.Invoke(pData, questData, data.Split('&'));
        }
    }
}
