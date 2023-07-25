using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Jobs.Enums;
using BlaineRP.Client.Game.Jobs.Types;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Game.Wrappers.Blips;
using BlaineRP.Client.Game.Wrappers.Colshapes;
using BlaineRP.Client.Quests;
using BlaineRP.Client.Quests.Enums;
using RAGE;
using RAGE.Elements;
using NPC = BlaineRP.Client.Game.NPCs.NPC;

namespace BlaineRP.Client.Game.Jobs
{
    public abstract class Job
    {
        public static Dictionary<int, Job> AllJobs { get; private set; } = new Dictionary<int, Job>();

        public static Job Get(int id) => AllJobs.GetValueOrDefault(id);

        private static Dictionary<JobTypes, Action<PlayerData, Job>> ShowJobMenuActions { get; set; } = new Dictionary<JobTypes, Action<PlayerData, Job>>()
            {
                {
                    JobTypes.Trucker,

                    (pData, job) =>
                    {
                        var truckerJob = job as Trucker;

                        if (Quest.GetPlayerQuest(pData, QuestTypes.JTR1)?.Step > 0)
                        {
                            Notification.ShowError(Locale.Notifications.General.JobOrderMenuHasOrder);

                            return;
                        }

                        var jobVehicle = job.GetCurrentData<RAGE.Elements.Vehicle>("JVEH");

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
                    JobTypes.Collector,

                    (pData, job) =>
                    {
                        var truckerJob = job as Collector;

                        if (Quest.GetPlayerQuest(pData, QuestTypes.JCL1)?.Step > 0)
                        {
                            Notification.ShowError(Locale.Notifications.General.JobOrderMenuHasOrder);

                            return;
                        }

                        var jobVehicle = job.GetCurrentData<RAGE.Elements.Vehicle>("JVEH");

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
                    JobTypes.Cabbie,

                    (pData, job) =>
                    {
                        var cabbieJob = job as Cabbie;

                        if (cabbieJob.GetCurrentData<Cabbie.OrderInfo>("CO") != null)
                        {
                            Notification.ShowError(Locale.Notifications.General.JobOrderMenuHasOrder);

                            return;
                        }

                        var jobVehicle = job.GetCurrentData<RAGE.Elements.Vehicle>("JVEH");

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
                    JobTypes.BusDriver,

                    (pData, job) =>
                    {
                        var busJob = job as BusDriver;

                        if (Quest.GetPlayerQuest(pData, QuestTypes.JBD1)?.Step > 0)
                        {
                            Notification.ShowError(Locale.Notifications.General.JobOrderMenuHasOrder);

                            return;
                        }

                        var jobVehicle = job.GetCurrentData<RAGE.Elements.Vehicle>("JVEH");

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

            return default;
        }

        public bool ResetCurrentData(string key) => CurrentData.Remove(key);

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

    [Script(int.MaxValue)]
    public class JobEvents
    {
        public JobEvents()
        {
            Events.Add("Player::SCJ", (args) =>
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (args == null || args.Length < 1)
                {
                    var lastJob = pData.CurrentJob;

                    if (lastJob != null)
                    {
                        lastJob.OnEndJob();

                        pData.CurrentJob = null;
                    }

                }
                else
                {
                    var job = Job.Get((int)args[0]);

                    var lastJob = pData.CurrentJob;

                    if (lastJob != null)
                    {
                        lastJob.OnEndJob();
                    }

                    pData.CurrentJob = job;

                    job.OnStartJob(args.Skip(1).ToArray());
                }
            });

            Events.Add("Job::TSC", (args) =>
            {
                var newBalance = Utils.Convert.ToDecimal(args[0]);

                var oldBalance = Utils.Convert.ToDecimal(args[1]);

                var diff = newBalance > oldBalance ? newBalance - oldBalance : 0;

                if (diff > 0)
                {
                    Notification.Show(Notification.Types.Information, "+" + Locale.Get("GEN_MONEY_0", diff), $"Сумма Вашей зарплаты: {Locale.Get("GEN_MONEY_0", newBalance)}");
                }
            });

            Events.Add("Job::TR::OC", (args) =>
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var job = pData.CurrentJob as Trucker;

                if (job == null)
                    return;

                var activeOrders = job.GetCurrentData<List<Trucker.OrderInfo>>("AOL");

                if (activeOrders == null)
                    return;

                if (args[0] is string str)
                {
                    var oData = str.Split('_');

                    var order = new Trucker.OrderInfo();

                    order.Id = uint.Parse(oData[0]);
                    var businessId = int.Parse(oData[1]);
                    order.MPIdx = int.Parse(oData[2]);
                    order.Reward = uint.Parse(oData[3]);

                    order.TargetBusiness = Client.Data.Locations.Business.All[businessId];

                    var existingOrder = activeOrders.Where(x => x.Id == order.Id).FirstOrDefault();

                    if (existingOrder != null)
                        activeOrders.Remove(existingOrder);

                    activeOrders.Add(order);

                    Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), string.Format(Locale.Notifications.General.JobNewOrder, Input.Core.Get(Input.Enums.BindTypes.Menu).GetKeyString(), Locale.Get(Language.Strings.GetKeyFromTypeByMemberName(typeof(HUD.Menu.Types), nameof(HUD.Menu.Types.Job_Menu), "MENU_I_NAME") ?? "null")));

                    Audio.PlayOnce("JOB_NEWORDER", Audio.TrackTypes.Success0, 0.5f, 0);
                }
                else
                {
                    var id = Utils.Convert.ToUInt32(args[0]);

                    var order = activeOrders.Where(x => x.Id == id).FirstOrDefault();

                    if (order == null)
                        return;

                    activeOrders.Remove(order);
                }

                activeOrders = activeOrders.OrderByDescending(x => x.Reward).ToList();

                job.SetCurrentData("AOL", activeOrders);

                if (ActionBox.CurrentContextStr == "JobTruckerOrderSelect")
                {
                    ActionBox.Close();

                    job.ShowOrderSelection(activeOrders);
                }
            });

            Events.Add("Job::CL::OC", (args) =>
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var job = pData.CurrentJob as Collector;

                if (job == null)
                    return;

                var activeOrders = job.GetCurrentData<List<Collector.OrderInfo>>("AOL");

                if (activeOrders == null)
                    return;

                if (args[0] is string str)
                {
                    var oData = str.Split('_');

                    var order = new Collector.OrderInfo();

                    order.Id = uint.Parse(oData[0]);
                    var businessId = int.Parse(oData[1]);
                    order.Reward = uint.Parse(oData[2]);

                    order.TargetBusiness = Client.Data.Locations.Business.All[businessId];

                    var existingOrder = activeOrders.Where(x => x.Id == order.Id).FirstOrDefault();

                    if (existingOrder != null)
                        activeOrders.Remove(existingOrder);

                    activeOrders.Add(order);

                    Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), string.Format(Locale.Notifications.General.JobNewOrder, Input.Core.Get(Input.Enums.BindTypes.Menu).GetKeyString(), Locale.Get(Language.Strings.GetKeyFromTypeByMemberName(typeof(HUD.Menu.Types), nameof(HUD.Menu.Types.Job_Menu), "MENU_I_NAME") ?? "null")));

                    Audio.PlayOnce("JOB_NEWORDER", Audio.TrackTypes.Success0, 0.5f, 0);
                }
                else
                {
                    var id = Utils.Convert.ToUInt32(args[0]);

                    var order = activeOrders.Where(x => x.Id == id).FirstOrDefault();

                    if (order == null)
                        return;

                    activeOrders.Remove(order);
                }

                activeOrders = activeOrders.OrderByDescending(x => x.Reward).ToList();

                job.SetCurrentData("AOL", activeOrders);

                if (ActionBox.CurrentContextStr == "JobCollectorOrderSelect")
                {
                    ActionBox.Close();

                    job.ShowOrderSelection(activeOrders);
                }
            });

            Events.Add("Job::CAB::OC", (args) =>
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var job = pData.CurrentJob as Cabbie;

                if (job == null)
                    return;

                var activeOrders = job.GetCurrentData<List<Cabbie.OrderInfo>>("AOL");

                if (activeOrders == null)
                    return;

                if (args[0] is string str)
                {
                    var oData = str.Split('_');

                    var order = new Cabbie.OrderInfo()
                    {
                        Id = uint.Parse(oData[0]),

                        Position = new Vector3(float.Parse(oData[1]), float.Parse(oData[2]), float.Parse(oData[3])),
                    };

                    var existingOrder = activeOrders.Where(x => x.Id == order.Id).FirstOrDefault();

                    if (existingOrder != null)
                        activeOrders.Remove(existingOrder);

                    activeOrders.Add(order);

                    Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), string.Format(Locale.Notifications.General.JobNewOrder, Input.Core.Get(Input.Enums.BindTypes.Menu).GetKeyString(), Locale.Get(Language.Strings.GetKeyFromTypeByMemberName(typeof(HUD.Menu.Types), nameof(HUD.Menu.Types.Job_Menu), "MENU_I_NAME") ?? "null")));

                    Audio.PlayOnce("JOB_NEWORDER", Audio.TrackTypes.Success0, 0.5f, 0);
                }
                else
                {
                    var id = Utils.Convert.ToUInt32(args[0]);

                    if (args.Length > 1 && args[1] is bool success)
                    {
                        if (job.GetCurrentData<Cabbie.OrderInfo>("CO")?.Id == id)
                        {
                            job.GetCurrentData<ExtraBlip>("Blip")?.Destroy();
                            job.GetCurrentData<ExtraColshape>("CS")?.Destroy();

                            job.ResetCurrentData("CO");
                            job.ResetCurrentData("Blip");
                            job.ResetCurrentData("CS");
                        }

                        if (success)
                        {
                            Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Notifications.General.Taxi4);
                        }
                        else
                        {
                            Notification.Show(Notification.Types.Error, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Notifications.General.JobOrderCancelByCaller);

                            Audio.PlayOnce("JOB_CANCELORDER", Audio.TrackTypes.Error0, 0.5f, 0);
                        }
                    }

                    var order = activeOrders.Where(x => x.Id == id).FirstOrDefault();

                    if (order == null)
                        return;

                    activeOrders.Remove(order);
                }

                job.SetCurrentData("AOL", activeOrders);

                if (ActionBox.CurrentContextStr == "JobCabbieOrderSelect")
                {
                    ActionBox.Close();

                    job.ShowOrderSelection(activeOrders);
                }
            });
        }
    }
}
