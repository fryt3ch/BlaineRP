using System;
using System.Collections.Generic;
using BlaineRP.Server.EntitiesData.Players;

namespace BlaineRP.Server.Game.Quests
{
    public partial class Quest
    {
        public class QuestData
        {
            public static Dictionary<QuestType, QuestData> All { get; private set; } = new Dictionary<QuestType, QuestData>();

            public Func<PlayerData, Quest, string[], byte> ProgressUpdateFunc { get; set; }

            public QuestData(QuestType Type)
            {
                All.Add(Type, this);
            }
        }
    }
}