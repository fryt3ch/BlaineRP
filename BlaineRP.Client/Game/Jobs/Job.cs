using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Quests;
using BlaineRP.Client.Game.UI.CEF;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Jobs
{
    public abstract partial class Job
    {
        public Job(int Id, JobType Type)
        {
            this.Type = Type;
            this.Id = Id;

            AllJobs.Add(Id, this);
        }

        public static Dictionary<int, Job> AllJobs { get; private set; } = new Dictionary<int, Job>();

        private static Dictionary<JobType, Action<PlayerData, Job>> ShowJobMenuActions { get; set; } = new Dictionary<JobType, Action<PlayerData, Job>>()
        {
            {
                JobType.Trucker, (pData, job) =>
                {
                    var truckerJob = job as Trucker;

                    if (Quest.GetPlayerQuest(pData, QuestTypes.JTR1)?.Step > 0)
                    {
                        Notification.ShowError(Locale.Notifications.General.JobOrderMenuHasOrder);

                        return;
                    }

                    Vehicle jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                    if (jobVehicle == null)
                        return;

                    if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                    {
                        Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                        return;
                    }

                    List<Trucker.OrderInfo> activeOrders = job.GetCurrentData<List<Trucker.OrderInfo>>("AOL");

                    if (activeOrders == null)
                        return;

                    truckerJob.ShowOrderSelection(activeOrders);
                }
            },
            {
                JobType.Collector, (pData, job) =>
                {
                    var truckerJob = job as Collector;

                    if (Quest.GetPlayerQuest(pData, QuestTypes.JCL1)?.Step > 0)
                    {
                        Notification.ShowError(Locale.Notifications.General.JobOrderMenuHasOrder);

                        return;
                    }

                    Vehicle jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                    if (jobVehicle == null)
                        return;

                    if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                    {
                        Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                        return;
                    }

                    List<Collector.OrderInfo> activeOrders = job.GetCurrentData<List<Collector.OrderInfo>>("AOL");

                    if (activeOrders == null)
                        return;

                    truckerJob.ShowOrderSelection(activeOrders);
                }
            },
            {
                JobType.Cabbie, (pData, job) =>
                {
                    var cabbieJob = job as Cabbie;

                    if (cabbieJob.GetCurrentData<Cabbie.OrderInfo>("CO") != null)
                    {
                        Notification.ShowError(Locale.Notifications.General.JobOrderMenuHasOrder);

                        return;
                    }

                    Vehicle jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                    if (jobVehicle == null)
                        return;

                    if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                    {
                        Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                        return;
                    }

                    List<Cabbie.OrderInfo> activeOrders = job.GetCurrentData<List<Cabbie.OrderInfo>>("AOL");

                    if (activeOrders == null)
                        return;

                    cabbieJob.ShowOrderSelection(activeOrders);
                }
            },
            {
                JobType.BusDriver, (pData, job) =>
                {
                    var busJob = job as BusDriver;

                    if (Quest.GetPlayerQuest(pData, QuestTypes.JBD1)?.Step > 0)
                    {
                        Notification.ShowError(Locale.Notifications.General.JobOrderMenuHasOrder);

                        return;
                    }

                    Vehicle jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                    if (jobVehicle == null)
                        return;

                    if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                    {
                        Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                        return;
                    }

                    busJob.ShowRouteSelection();
                }
            },
        };

        public int Id { get; set; }

        public int SubId => AllJobs.Values.Where(x => x.Type == Type).ToList().IndexOf(this);

        public JobType Type { get; set; }

        public string Name => Locale.Get(Language.Strings.GetKeyFromTypeByMemberName(Type.GetType(), Type.ToString(), "NAME_0") ?? "null");

        public ExtraBlip Blip { get; set; }

        public NPCs.NPC JobGiver { get; set; }

        private Dictionary<string, object> CurrentData { get; set; }

        public static Job Get(int id)
        {
            return AllJobs.GetValueOrDefault(id);
        }

        public static void ShowJobMenu()
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            Job job = pData.CurrentJob;

            if (job == null)
                return;

            Action<PlayerData, Job> showAction = ShowJobMenuActions.GetValueOrDefault(job.Type);

            if (showAction == null)
                return;

            showAction.Invoke(pData, job);
        }

        public void SetCurrentData(string key, object data)
        {
            if (CurrentData == null)
                return;

            if (!CurrentData.TryAdd(key, data))
                CurrentData[key] = data;
        }

        public T GetCurrentData<T>(string key)
        {
            object data = CurrentData.GetValueOrDefault(key);

            if (data is T dataT)
                return dataT;

            return default(T);
        }

        public bool ResetCurrentData(string key)
        {
            return CurrentData.Remove(key);
        }

        public virtual void OnStartJob(object[] data)
        {
            CurrentData = new Dictionary<string, object>();

            HUD.Menu.UpdateCurrentTypes(true, HUD.Menu.Types.Job_Menu);
        }

        public virtual void OnEndJob()
        {
            if (CurrentData != null)
            {
                CurrentData.Clear();

                CurrentData = null;
            }

            HUD.Menu.UpdateCurrentTypes(false, HUD.Menu.Types.Job_Menu);
        }
    }
}