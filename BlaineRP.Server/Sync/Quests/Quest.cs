using BlaineRP.Server.Game.Businesses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BlaineRP.Server.Sync
{
    public class Quest
    {
        public class QuestData
        {
            public enum Types
            {
                TQ1 = 0,

                #region Temp Quests
                JTR1,

                JBD1,

                JCL1,

                JFRM1,
                JFRM2,

                DRSCHOOL0,
                #endregion
            }

            public static Dictionary<Types, QuestData> All { get; private set; } = new Dictionary<Types, QuestData>();

            public Func<PlayerData, Sync.Quest, string[], byte> ProgressUpdateFunc { get; set; }

            public QuestData(Types Type)
            {
                All.Add(Type, this);
            }
        }

        public static Dictionary<QuestData.Types, Quest> GetNewDict() => new Dictionary<QuestData.Types, Quest>()
        {
            //{ QuestData.Types.TQ1, new Quest(QuestData.Types.TQ1) },
        };

        [JsonIgnore]
        public bool IsTemp => IsQuestTemp(Type);

        [JsonIgnore]
        public QuestData.Types Type { get; set; }

        [JsonIgnore]
        public QuestData Data => QuestData.All.GetValueOrDefault(Type);

        [JsonProperty(PropertyName = "C")]
        public bool IsCompleted { get; set; }

        [JsonProperty(PropertyName = "S")]
        public byte Step { get; set; }

        [JsonProperty(PropertyName = "SP")]
        public int StepProgress { get; set; }

        [JsonIgnore]
        public string CurrentData { get; set; }

        public Quest(QuestData.Types Type)
        {
            this.Type = Type;
        }

        public Quest(QuestData.Types Type, bool IsCompleted, byte Step, int StepProgress) : this(Type)
        {
            this.IsCompleted = IsCompleted;
            this.Step = Step;
            this.StepProgress = StepProgress;
        }

        public static void StartQuest(PlayerData pData, QuestData.Types type, byte step = 0, int stepProgress = 0, string currentData = null)
        {
            var quest = new Quest(type, false, step, stepProgress);

            if (pData.Info.Quests.TryAdd(type, quest))
            {
                quest.CurrentData = currentData;

                pData.Player.TriggerEvent("Player::Quest::Upd", (int)type, step, stepProgress, currentData);

                if (!quest.IsTemp)
                {
                    MySQL.CharacterQuestUpdate(pData.Info, type, quest);
                }
            }
        }

        public void Cancel(PlayerData.PlayerInfo pInfo, bool success = false)
        {
            if (pInfo.Quests.Remove(Type))
            {
                if (pInfo.PlayerData != null)
                {
                    pInfo.PlayerData.Player.TriggerEvent("Player::Quest::Upd", (int)Type, success);
                }

                if (!IsTemp)
                {
                    MySQL.CharacterQuestUpdate(pInfo, Type, null);
                }
            }
        }

        public void UpdateStep(PlayerData.PlayerInfo pInfo, byte step, int progress, string currentData = null)
        {
            Step = step;

            StepProgress = progress;

            CurrentData = currentData;

            if (pInfo.PlayerData != null)
            {
                var player = pInfo.PlayerData.Player;

                player.TriggerEvent("Player::Quest::Upd", (int)Type, Step, StepProgress, currentData);
            }

            if (!IsTemp)
            {
                MySQL.CharacterQuestUpdate(pInfo, Type, this);
            }
        }

        public void UpdateStepKeepOldData(PlayerData.PlayerInfo pInfo, byte step, int progress)
        {
            Step = step;

            StepProgress = progress;

            if (pInfo.PlayerData != null)
            {
                var player = pInfo.PlayerData.Player;

                player.TriggerEvent("Player::Quest::Upd", (int)Type, Step, StepProgress);
            }

            if (!IsTemp)
            {
                MySQL.CharacterQuestUpdate(pInfo, Type, this);
            }
        }

        public static bool IsQuestTemp(QuestData.Types type) => type >= QuestData.Types.JTR1 && type <= QuestData.Types.DRSCHOOL0;

        public static void InitializeAll()
        {
            var ns = typeof(Sync.Quests.Types.JTR1).Namespace;

            foreach (var x in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == ns && t.IsClass))
            {
                var method = x.GetMethod("Initialize");

                method?.Invoke(null, null);
            }
        }
    }
}
