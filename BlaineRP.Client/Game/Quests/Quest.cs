using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.UI.CEF;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Quests
{
    public partial class Quest
    {
        public Quest(QuestTypes Type, byte Step, int StepProgress, string CurrentData)
        {
            this.Type = Type;

            this.Step = Step;
            this.StepProgress = StepProgress;

            this.CurrentData = CurrentData;
        }

        public static Quest ActualQuest { get; private set; }

        public QuestTypes Type { get; set; }

        public QuestData Data => QuestData.All[Type];

        public byte Step { get; set; }

        public int StepProgress { get; set; }

        public string CurrentData { get; set; }

        public QuestData.StepData CurrentStepData => Data.Steps.GetValueOrDefault(Step);

        private Dictionary<string, object> ActualData { get; set; } = new Dictionary<string, object>();

        public string GoalWithProgress
        {
            get
            {
                QuestData.StepData sData = CurrentStepData;

                if (sData == null)
                    return null;

                if (sData.GoalName.Contains("{1-0}"))
                    return sData.GoalName.Replace("{1-0}", (sData.MaxProgress - StepProgress).ToString());

                return string.Format(sData.GoalName, StepProgress, sData.MaxProgress);
            }
        }

        public void SetActualData(string key, object data)
        {
            if (ActualData == null)
                return;

            if (!ActualData.TryAdd(key, data))
                ActualData[key] = data;
        }

        public T GetActualData<T>(string key)
        {
            object data = ActualData.GetValueOrDefault(key);

            if (data is T dataT)
                return dataT;

            return default(T);
        }

        public bool ResetActualData(string key)
        {
            return ActualData.Remove(key);
        }

        public static Quest GetPlayerQuest(PlayerData pData, QuestTypes type)
        {
            return pData.Quests.Where(x => x.Type == type).FirstOrDefault();
        }

        public void MenuIconFunc()
        {
            SetActive(true);
        }

        public void UpdateHudQuest()
        {
            QuestData data = Data;

            HUD.SetQuestParams(data.GiverName, data.Name, GoalWithProgress, data.ColourType);
        }

        public void UpdateProgress(int newProgress)
        {
            StepProgress = newProgress;

            if (ActualQuest == this)
                UpdateHudQuest();
        }

        public void UpdateStep(byte newStep)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            QuestData.StepData sData = CurrentStepData;

            if (sData != null)
                sData.EndAction?.Invoke(pData, this);

            sData = Data.Steps.GetValueOrDefault(newStep);

            Step = newStep;

            if (sData != null)
                sData.StartAction?.Invoke(pData, this);

            //SetActive(true);
        }

        public void SetActive(bool state, bool route = true)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (state)
            {
                QuestData.StepData sData = CurrentStepData;

                if (sData == null)
                    return;

                ActualQuest = this;

                if (!Settings.User.Interface.HideQuest)
                    HUD.EnableQuest(true);

                if (route)
                {
                    ExtraBlip mBlip = GetActualData<ExtraBlip>("E_BP_M");

                    if (mBlip != null)
                    {
                        Vector3 coords = mBlip.Position;
                        Utils.Game.Misc.SetWaypoint(coords.X, coords.Y);
                    }
                }
            }
            else
            {
                ActualQuest = pData.Quests.Where(x => x != this).FirstOrDefault();

                if (ActualQuest != null)
                    ActualQuest.SetActive(true);
                else
                    HUD.EnableQuest(false);
            }
        }

        public void Initialize()
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            QuestData.StepData sData = CurrentStepData;

            if (sData != null)
                sData.StartAction?.Invoke(pData, this);

            if (ActualQuest == null)
                SetActive(true, false);
        }

        public void Destroy()
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (ActualQuest == this)
                SetActive(false);

            QuestData.StepData sData = CurrentStepData;

            if (sData != null)
                sData.EndAction?.Invoke(pData, this);

            ClearAllActualData();
        }

        public void ClearAllActualData()
        {
            foreach (string x in ActualData.Keys.ToList())
            {
                object value;

                if (!ActualData.TryGetValue(x, out value))
                    return;

                if (x.StartsWith("E_"))
                    ((dynamic)value)?.Destroy();
                else if (x.StartsWith("CS_"))
                    (value as ExtraColshape)?.Destroy();
            }

            ActualData.Clear();
        }

        public async System.Threading.Tasks.Task<byte> CallProgressUpdateProc(params object[] args)
        {
            return (byte)(int)await Events.CallRemoteProc("Quest::PU", (int)Type, string.Join('&', args));
        }

        public void SetQuestAsCompleted(bool success, bool notify)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (notify)
                Notification.Show(Notification.Types.Quest, Data.Name, success ? Locale.Notifications.General.QuestFinishedText : Locale.Notifications.General.QuestCancelledText);

            pData.Quests.Remove(this);

            Destroy();

            Menu.UpdateQuests(pData);
        }

        public void SetQuestAsStarted(bool notify)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            pData.Quests.Add(this);

            Initialize();

            if (notify)
                Notification.Show(Notification.Types.Quest, Data.Name, string.Format(Locale.Notifications.General.QuestStartedText, GoalWithProgress));

            Menu.UpdateQuests(pData);
        }

        public void SetQuestAsUpdated(byte step, int stepProgress, string data, bool notify)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            CurrentData = data;

            StepProgress = stepProgress;

            UpdateStep(step);

            UpdateProgress(stepProgress);

            if (notify)
                Notification.Show(Notification.Types.Quest, Data.Name, string.Format(Locale.Notifications.General.QuestUpdatedText, GoalWithProgress));

            Menu.UpdateQuests(pData);
        }

        public void SetQuestAsUpdatedKeepOldData(byte step, int stepProgress, bool notify)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            StepProgress = stepProgress;

            UpdateStep(step);

            UpdateProgress(stepProgress);

            if (notify)
                Notification.Show(Notification.Types.Quest, Data.Name, string.Format(Locale.Notifications.General.QuestUpdatedText, GoalWithProgress));

            Menu.UpdateQuests(pData);
        }
    }
}