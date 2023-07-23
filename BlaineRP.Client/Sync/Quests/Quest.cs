using BlaineRP.Client.CEF;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Client.Sync
{
    public class Quest
    {
        public static Quest ActualQuest { get; private set; }

        public class QuestData
        {
            public enum Types
            {
                TQ1 = 0,

                /// <summary>Job Trucker 1</summary>
                JTR1,
                /// <summary>Job Bus Driver 1</summary>
                JBD1,

                JCL1,

                JFRM1,
                JFRM2,

                DRSCHOOL0,
            }

            public enum ColourTypes
            {
                Dark = 0,
                Red,
            }

            public static Dictionary<Types, QuestData> All { get; private set; } = new Dictionary<Types, QuestData>();

            public Types Type { get; set; }

            public ColourTypes ColourType { get; set; }

            public string Name { get; set; }

            public string GiverName { get; set; }

            public Dictionary<byte, StepData> Steps { get; set; }

            public QuestData(Types Type, string Name, string GiverName, Dictionary<byte, StepData> Steps)
            {
                this.Type = Type;

                this.Name = Name;
                this.GiverName = GiverName;

                this.Steps = Steps;

                this.ColourType = ColourTypes.Dark;

                All.TryAdd(Type, this);
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

        public byte Step { get; set; }

        public int StepProgress { get; set; }

        public string CurrentData { get; set; }

        public QuestData.StepData CurrentStepData => Data.Steps.GetValueOrDefault(Step);

        private Dictionary<string, object> ActualData { get; set; } = new Dictionary<string, object>();

        public void SetActualData(string key, object data)
        {
            if (ActualData == null)
                return;

            if (!ActualData.TryAdd(key, data))
                ActualData[key] = data;
        }

        public T GetActualData<T>(string key)
        {
            var data = ActualData.GetValueOrDefault(key);

            if (data is T dataT)
                return dataT;

            return default(T);
        }

        public bool ResetActualData(string key) => ActualData.Remove(key);

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

        public Quest(QuestData.Types Type, byte Step, int StepProgress, string CurrentData)
        {
            this.Type = Type;

            this.Step = Step;
            this.StepProgress = StepProgress;

            this.CurrentData = CurrentData;
        }

        public static Quest GetPlayerQuest(Sync.Players.PlayerData pData, QuestData.Types type) => pData.Quests.Where(x => x.Type == type).FirstOrDefault();

        public void MenuIconFunc()
        {
            SetActive(true);
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

        public void UpdateStep(byte newStep)
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

            Step = newStep;

            if (sData != null)
            {
                sData.StartAction?.Invoke(pData, this);
            }

            //SetActive(true);
        }

        public void SetActive(bool state, bool route = true)
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

                if (!Settings.User.Interface.HideQuest)
                {
                    CEF.HUD.EnableQuest(true);
                }

                if (route)
                {
                    var mBlip = GetActualData<Additional.ExtraBlip>("E_BP_M");

                    if (mBlip != null)
                    {
                        var coords = mBlip.Position;
                        Misc.SetWaypoint(coords.X, coords.Y);
                    }
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

            var sData = CurrentStepData;

            if (sData != null)
            {
                sData.StartAction?.Invoke(pData, this);
            }

            if (ActualQuest == null)
                SetActive(true, false);
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

            ClearAllActualData();
        }

        public void ClearAllActualData()
        {
            foreach (var x in ActualData.Keys.ToList())
            {
                object value;

                if (!ActualData.TryGetValue(x, out value))
                    return;

                if (x.StartsWith("E_"))
                {
                    ((dynamic)value)?.Destroy();
                }
                else if (x.StartsWith("CS_"))
                {
                    (value as Additional.ExtraColshape)?.Destroy();
                }
            }

            ActualData.Clear();
        }

        public async System.Threading.Tasks.Task<byte> CallProgressUpdateProc(params object[] args) => (byte)(int)await Events.CallRemoteProc("Quest::PU", (int)Type, string.Join('&', args));

        public void SetQuestAsCompleted(bool success, bool notify)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (notify)
                CEF.Notification.Show(Notification.Types.Quest, Data.Name, success ? Locale.Notifications.General.QuestFinishedText : Locale.Notifications.General.QuestCancelledText);

            pData.Quests.Remove(this);

            Destroy();

            CEF.Menu.UpdateQuests(pData);
        }

        public void SetQuestAsStarted(bool notify)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            pData.Quests.Add(this);

            Initialize();

            if (notify)
                CEF.Notification.Show(Notification.Types.Quest, Data.Name, string.Format(Locale.Notifications.General.QuestStartedText, GoalWithProgress));

            CEF.Menu.UpdateQuests(pData);
        }

        public void SetQuestAsUpdated(byte step, int stepProgress, string data, bool notify)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            CurrentData = data;

            StepProgress = stepProgress;

            UpdateStep(step);

            UpdateProgress(stepProgress);

            if (notify)
                CEF.Notification.Show(Notification.Types.Quest, Data.Name, string.Format(Locale.Notifications.General.QuestUpdatedText, GoalWithProgress));

            CEF.Menu.UpdateQuests(pData);
        }

        public void SetQuestAsUpdatedKeepOldData(byte step, int stepProgress, bool notify)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            StepProgress = stepProgress;

            UpdateStep(step);

            UpdateProgress(stepProgress);

            if (notify)
                CEF.Notification.Show(Notification.Types.Quest, Data.Name, string.Format(Locale.Notifications.General.QuestUpdatedText, GoalWithProgress));

            CEF.Menu.UpdateQuests(pData);
        }
    }
}
