using System;
using System.Collections.Generic;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;

namespace BlaineRP.Client.Game.Quests
{
    public partial class Quest
    {
        public class QuestData
        {
            public enum ColourTypes
            {
                Dark = 0,
                Red,
            }

            public QuestData(QuestTypes Type, string Name, string GiverName, Dictionary<byte, StepData> Steps)
            {
                this.Type = Type;

                this.Name = Name;
                this.GiverName = GiverName;

                this.Steps = Steps;

                ColourType = ColourTypes.Dark;

                All.TryAdd(Type, this);
            }

            public static Dictionary<QuestTypes, QuestData> All { get; private set; } = new Dictionary<QuestTypes, QuestData>();

            public QuestTypes Type { get; set; }

            public ColourTypes ColourType { get; set; }

            public string Name { get; set; }

            public string GiverName { get; set; }

            public Dictionary<byte, StepData> Steps { get; set; }

            public class StepData
            {
                public StepData(string GoalName, int MaxProgress = 1)
                {
                    this.GoalName = GoalName;

                    this.MaxProgress = MaxProgress;
                }

                public string GoalName { get; set; }

                public int MaxProgress { get; set; }

                public Action<PlayerData, Quest> StartAction { get; set; }

                public Action<PlayerData, Quest> EndAction { get; set; }
            }
        }
    }
}