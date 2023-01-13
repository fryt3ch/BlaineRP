using Newtonsoft.Json;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

namespace BCRPClient.Sync
{
    public class Quest
    {
        public static Quest ActualQuest { get; private set; }

        public class QuestData
        {
            public enum Types
            {
                TQ1 = 0,
            }

            public enum ColourTypes
            {
                Dark = 0,
                Red,
            }

            public static Dictionary<Types, QuestData> All { get; private set; } = new Dictionary<Types, QuestData>()
            {
                {
                    Types.TQ1,

                    new QuestData(Types.TQ1, "Пиздилово", "Snow Brawl", new Dictionary<int, StepData>()
                    {
                        {
                            0,
                            
                            new StepData("Попадите снежком в {1-0} игроков", 10)
                            {
                                StartAction = (pData, quest) =>
                                {
                                    var blips = quest.Blips;

                                    if (blips == null)
                                        blips = new List<Blip>();

                                    var mBlip = new Blip(304, new Vector3(0f, 0f, 0f), "asdas", 1f, 5, 255, 0, false, 0, 0, Settings.MAIN_DIMENSION);

                                    mBlip.SetData("IsMain", true);

                                    blips.Add(mBlip);

                                    quest.Blips = blips;
                                },

                                EndAction = null,
                            }
                        }
                    })
                }
            };

            public Types Type { get; set; }

            public ColourTypes ColourType { get; set; }

            public string Name { get; set; }

            public string GiverName { get; set; }

            public Dictionary<int, StepData> Steps { get; set; }

            public QuestData(Types Type, string Name, string GiverName, Dictionary<int, StepData> Steps)
            {
                this.Type = Type;

                this.Name = Name;
                this.GiverName = GiverName;

                this.Steps = Steps;

                this.ColourType = ColourTypes.Dark;
            }

            public class StepData
            {
                public string GoalName { get; set; }

                public int MaxProgress { get; set; }

                public Action<Sync.Players.PlayerData, Quest> StartAction { get; set; }

                public Action<Sync.Players.PlayerData, Quest> EndAction { get; set; }

                public StepData(string GoalName, int MaxProgress = 1)
                {
                    this.GoalName = GoalName;

                    this.MaxProgress = MaxProgress;
                }
            }
        }

        public QuestData.Types Type { get; set; }

        public QuestData Data => QuestData.All[Type];

        public int Step { get; set; }

        public int StepProgress { get; set; }

        public QuestData.StepData CurrentStepData => Data.Steps.GetValueOrDefault(Step);

        public List<Blip> Blips { get => Player.LocalPlayer.GetData<List<Blip>>($"Quests::{Type}::Temp::Blips"); set { if (value == null) Player.LocalPlayer.ResetData($"Quests::{Type}::Temp::Blips"); else Player.LocalPlayer.SetData($"Quests::{Type}::Temp::Blips", value); } }

        public string GoalWithProgress
        {
            get
            {
                var sData = CurrentStepData;

                if (sData == null)
                    return null;

                if (sData.GoalName.Contains("{1-0}"))
                    return sData.GoalName.Replace("{1-0}", (sData.MaxProgress - StepProgress).ToString());

                return string.Format(sData.GoalName, StepProgress, sData.MaxProgress);
            }
        }

        public Quest(QuestData.Types Type, int Step, int StepProgress)
        {
            this.Type = Type;

            this.Step = Step;
            this.StepProgress = StepProgress;
        }

        public static Quest GetPlayerQuest(Sync.Players.PlayerData pData, QuestData.Types type) => pData.Quests.Where(x => x.Type == type).FirstOrDefault();

        public void MenuIconFunc()
        {
            if (ActualQuest != this)
            {
                SetActive(true);
            }

            var mBlip = Blips?.Where(x => x?.GetData<bool>("IsMain") == true).FirstOrDefault();

            if (mBlip != null)
            {
                var coords = mBlip.GetInfoIdCoord();

                RAGE.Game.Ui.SetWaypointOff();

                RAGE.Game.Ui.SetNewWaypoint(coords.X, coords.Y);
            }
        }

        public void UpdateHudQuest()
        {
            var data = Data;

            CEF.HUD.SetQuestParams(data.GiverName, data.Name, GoalWithProgress, data.ColourType);
        }

        public void UpdateProgress(int newProgress)
        {
            StepProgress = newProgress;

            if (ActualQuest == this)
            {
                UpdateHudQuest();
            }
        }

        public void UpdateStep(int newStep)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var sData = CurrentStepData;

            if (sData != null)
            {
                sData.EndAction?.Invoke(pData, this);
            }

            sData = Data.Steps.GetValueOrDefault(newStep);

            if (sData != null)
            {
                sData.StartAction?.Invoke(pData, this);
            }

            Step = newStep;

            if (ActualQuest == this)
            {
                UpdateHudQuest();
            }
        }

        public void SetActive(bool state)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (state)
            {
                var sData = CurrentStepData;

                if (sData == null)
                    return;

                ActualQuest = this;

                if (!Settings.Interface.HideQuest)
                {
                    CEF.HUD.EnableQuest(true);
                }
            }
            else
            {
                ActualQuest = pData.Quests.Where(x => x != this).FirstOrDefault();

                if (ActualQuest != null)
                {
                    ActualQuest.SetActive(true);
                }
                else
                {
                    CEF.HUD.EnableQuest(false);
                }
            }
        }

        public void Initialize()
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (ActualQuest == null)
                SetActive(true);

            var sData = CurrentStepData;

            if (sData != null)
            {
                sData.StartAction?.Invoke(pData, this);
            }
        }

        public void Destroy()
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (ActualQuest == this)
            {
                SetActive(false);
            }

            var sData = CurrentStepData;

            if (sData != null)
            {
                sData.EndAction?.Invoke(pData, this);
            }

            var blips = Blips;

            if (blips != null)
            {
                foreach (var x in blips)
                    x?.Destroy();

                blips.Clear();

                Blips = null;
            }
        }
    }
}
