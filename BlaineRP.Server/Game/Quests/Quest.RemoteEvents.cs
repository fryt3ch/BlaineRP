using System;
using System.Collections.Generic;
using BlaineRP.Server.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Quests
{
    public partial class Quest
    {
        internal class RemoteEvents : Script
        {
            [RemoteProc("Quest::PU")]
            private static byte ProgressUpdate(Player player, int questTypeNum, string data)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return 0;

                PlayerData pData = sRes.Data;

                if (data == null)
                    return 0;

                if (!Enum.IsDefined(typeof(QuestType), questTypeNum))
                    return 0;

                var questType = (QuestType)questTypeNum;

                Quest questData = pData.Info.Quests.GetValueOrDefault(questType);

                if (questData == null)
                    return 0;

                Func<PlayerData, Quest, string[], byte> func = QuestData.All.GetValueOrDefault(questType)?.ProgressUpdateFunc;

                if (func == null)
                    return 0;

                return func.Invoke(pData, questData, data.Split('&'));
            }
        }
    }
}