using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.Game.Quests.Types;
using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Quests
{
    public partial class Quest
    {
        public static Dictionary<QuestType, Quest> GetNewDict() => new Dictionary<QuestType, Quest>()
        {
            //{ QuestData.Types.TQ1, new Quest(QuestData.Types.TQ1) },
        };

        [JsonIgnore]
        public bool IsTemp => IsQuestTemp(Type);

        [JsonIgnore]
        public QuestType Type { get; set; }

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

        public Quest(QuestType Type)
        {
            this.Type = Type;
        }

        public Quest(QuestType Type, bool IsCompleted, byte Step, int StepProgress) : this(Type)
        {
            this.IsCompleted = IsCompleted;
            this.Step = Step;
            this.StepProgress = StepProgress;
        }

        public static void StartQuest(PlayerData pData, QuestType type, byte step = 0, int stepProgress = 0, string currentData = null)
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

        public void Cancel(PlayerInfo pInfo, bool success = false)
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

        public void UpdateStep(PlayerInfo pInfo, byte step, int progress, string currentData = null)
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

        public void UpdateStepKeepOldData(PlayerInfo pInfo, byte step, int progress)
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

        public static bool IsQuestTemp(QuestType type) => type >= QuestType.JTR1 && type <= QuestType.DRSCHOOL0;

        public static void InitializeAll()
        {
            var ns = typeof(JTR1).Namespace;

            foreach (var x in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == ns && t.IsClass))
            {
                var method = x.GetMethod("Initialize");

                method?.Invoke(null, null);
            }
        }
    }
}
