using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BCRPServer.Sync
{
    public class Quest
    {
        public class QuestData
        {
            public enum Types
            {
                TQ1 = 0,

                JTR1,

                JBD1,

                JCL1,
            }

            public static Dictionary<Types, QuestData> All { get; private set; } = new Dictionary<Types, QuestData>()
            {
                {
                    Types.TQ1,

                    new QuestData(Types.TQ1, new Dictionary<int, StepData>()
                    {
                        {
                            0,

                            new StepData(1)
                            {
                                StartAction = null,
                                EndAction = null,
                            }
                        }
                    })
                }
            };

            public Types Type { get; set; }

            public Action CompleteAction { get; set; }

            public Dictionary<int, StepData> Steps { get; set; }

            public QuestData(Types Type, Dictionary<int, StepData> Steps)
            {
                this.Type = Type;

                this.Steps = Steps;
            }

            public class StepData
            {
                public int MaxProgress { get; set; }

                public Action StartAction { get; set; }

                public Action EndAction { get; set; }

                public StepData(int MaxProgress = 1)
                {
                    this.MaxProgress = MaxProgress;
                }
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

        [JsonIgnore]
        public QuestData.StepData CurrentStepData => Data?.Steps.GetValueOrDefault(Step);

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

            if (pData.Info.Quests.TryAdd(type, new Quest(type, false, step, stepProgress)))
            {
                quest.CurrentData = currentData;

                pData.Player.TriggerEvent("Player::Quest::Upd", (int)type, step, stepProgress, currentData);

                if (!quest.IsTemp)
                {
                    MySQL.CharacterQuestUpdate(pData.Info, type, quest);
                }
            }
        }

        public void Cancel(PlayerData.PlayerInfo pInfo)
        {
            if (pInfo.Quests.Remove(Type))
            {
                if (pInfo.PlayerData != null)
                {
                    pInfo.PlayerData.Player.TriggerEvent("Player::Quest::Upd", (int)Type, false);
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

            var sData = CurrentStepData;

            if (sData != null)
            {

            }

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

            var sData = CurrentStepData;

            if (sData != null)
            {

            }

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

        public static bool IsQuestTemp(QuestData.Types type) => type >= QuestData.Types.JTR1;
    }
}
