using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace BCRPServer.Sync
{
    public class Quest
    {
        public class QuestData
        {
            public enum Types
            {
                TQ1 = 0,
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
            { QuestData.Types.TQ1, new Quest(QuestData.Types.TQ1) },
        };

        [JsonIgnore]
        public QuestData.Types Type { get; set; }

        [JsonIgnore]
        public QuestData Data => QuestData.All[Type];

        [JsonProperty(PropertyName = "C")]
        public bool IsCompleted { get; set; }

        [JsonProperty(PropertyName = "S")]
        public int Step { get; set; }

        [JsonProperty(PropertyName = "SP")]
        public int StepProgress { get; set; }

        [JsonIgnore]
        public QuestData.StepData CurrentStepData => Data.Steps.GetValueOrDefault(Step);

        public Quest(QuestData.Types Type)
        {
            this.Type = Type;
        }

        public Quest(QuestData.Types Type, bool IsCompleted, int Step, int StepProgress) : this(Type)
        {
            this.IsCompleted = IsCompleted;
            this.Step = Step;
            this.StepProgress = StepProgress;
        }

        public static bool IsStepInfluential(int step) => step % 256 == 0;

        public void UpdateStep(PlayerData.PlayerInfo pInfo, int step)
        {
            var sData = Data.Steps.GetValueOrDefault(step);

            if (sData == null)
                return;

            Step = step;

            if (pInfo.PlayerData != null)
            {
                var player = pInfo.PlayerData.Player;

                player.TriggerEvent("Player::Quest::Upd", (int)Type, Step, StepProgress);
            }

            if (IsStepInfluential(step))
            {
                MySQL.CharacterQuestUpdate(pInfo, this);
            }
        }
    }
}
