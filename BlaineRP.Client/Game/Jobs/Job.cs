using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Quests;
using BlaineRP.Client.Game.UI.CEF;
using RAGE.Elements;
using NPC = BlaineRP.Client.Game.NPCs.NPC;

namespace BlaineRP.Client.Game.Jobs
{
    public abstract partial class Job
    {
        public static Dictionary<int, Job> AllJobs { get; private set; } = new Dictionary<int, Job>();

        public static Job Get(int id)
        {
            return AllJobs.GetValueOrDefault(id);
        }

        private static Dictionary<JobTypes, Action<PlayerData, Job>> ShowJobMenuActions { get; set; } = new Dictionary<JobTypes, Action<PlayerData, Job>>()
        {
            {
                JobTypes.Trucker, (pData, job) =>
                {
                    var truckerJob = job as Trucker;

                    if (Quest.GetPlayerQuest(pData, QuestTypes.JTR1)?.Step > 0)
                    {
                        Notification.ShowError(Locale.Notifications.General.JobOrderMenuHasOrder);

                        return;
                    }

                    var jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                    if (jobVehicle == null)
                        return;

                    if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                    {
                        Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                        return;
                    }

                    var activeOrders = job.GetCurrentData<List<Trucker.OrderInfo>>("AOL");

                    if (activeOrders == null)
                        return;

                    truckerJob.ShowOrderSelection(activeOrders);
                }
            },
            {
                JobTypes.Collector, (pData, job) =>
                {
                    var truckerJob = job as Collector;

                    if (Quest.GetPlayerQuest(pData, QuestTypes.JCL1)?.Step > 0)
                    {
                        Notification.ShowError(Locale.Notifications.General.JobOrderMenuHasOrder);

                        return;
                    }

                    var jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                    if (jobVehicle == null)
                        return;

                    if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                    {
                        Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                        return;
                    }

                    var activeOrders = job.GetCurrentData<List<Collector.OrderInfo>>("AOL");

                    if (activeOrders == null)
                        return;

                    truckerJob.ShowOrderSelection(activeOrders);
                }
            },
            {
                JobTypes.Cabbie, (pData, job) =>
                {
                    var cabbieJob = job as Cabbie;

                    if (cabbieJob.GetCurrentData<Cabbie.OrderInfo>("CO") != null)
                    {
                        Notification.ShowError(Locale.Notifications.General.JobOrderMenuHasOrder);

                        return;
                    }

                    var jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                    if (jobVehicle == null)
                        return;

                    if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                    {
                        Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                        return;
                    }

                    var activeOrders = job.GetCurrentData<List<Cabbie.OrderInfo>>("AOL");

                    if (activeOrders == null)
                        return;

                    cabbieJob.ShowOrderSelection(activeOrders);
                }
            },
            {
                JobTypes.BusDriver, (pData, job) =>
                {
                    var busJob = job as BusDriver;

                    if (Quest.GetPlayerQuest(pData, QuestTypes.JBD1)?.Step > 0)
                    {
                        Notification.ShowError(Locale.Notifications.General.JobOrderMenuHasOrder);

                        return;
                    }

                    var jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

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

        public static void ShowJobMenu()
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var job = pData.CurrentJob;

            if (job == null)
                return;

            var showAction = ShowJobMenuActions.GetValueOrDefault(job.Type);

            if (showAction == null)
                return;

            showAction.Invoke(pData, job);
        }

        public int Id { get; set; }

        public int SubId => AllJobs.Values.Where(x => x.Type == Type).ToList().IndexOf(this);

        public JobTypes Type { get; set; }

        public string Name => Locale.Property.JobNames.GetValueOrDefault(Type) ?? "null";

        public ExtraBlip Blip { get; set; }

        public NPC JobGiver { get; set; }

        private Dictionary<string, object> CurrentData { get; set; }

        public Job(int Id, JobTypes Type)
        {
            this.Type = Type;
            this.Id = Id;

            AllJobs.Add(Id, this);
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
            var data = CurrentData.GetValueOrDefault(key);

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