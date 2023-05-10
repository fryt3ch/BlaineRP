using BCRPClient.CEF;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Data.Jobs
{
    public enum Types
    {
        /// <summary>Работа дальнобойщика</summary>
        Trucker = 0,

        Cabbie,

        BusDriver,

        Collector,

        Farmer,
    }

    public abstract class Job
    {
        public static Dictionary<int, Job> AllJobs { get; private set; } = new Dictionary<int, Job>();

        public static Job Get(int id) => AllJobs.GetValueOrDefault(id);

        private static Dictionary<Types, Action<Sync.Players.PlayerData, Job>> ShowJobMenuActions { get; set; } = new Dictionary<Types, Action<Sync.Players.PlayerData, Job>>()
            {
                {
                    Types.Trucker,

                    (pData, job) =>
                    {
                        var truckerJob = job as Trucker;

                        if (Sync.Quest.GetPlayerQuest(pData, Sync.Quest.QuestData.Types.JTR1)?.Step > 0)
                        {
                            CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.JobOrderMenuHasOrder);

                            return;
                        }

                        var jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                        if (jobVehicle == null)
                            return;

                        if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                        {
                            CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.JobVehicleNotInVeh);

                            return;
                        }

                        var activeOrders = job.GetCurrentData<List<Trucker.OrderInfo>>("AOL");

                        if (activeOrders == null)
                            return;

                        truckerJob.ShowOrderSelection(activeOrders);
                    }
                },

                {
                    Types.Collector,

                    (pData, job) =>
                    {
                        var truckerJob = job as Collector;

                        if (Sync.Quest.GetPlayerQuest(pData, Sync.Quest.QuestData.Types.JCL1)?.Step > 0)
                        {
                            CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.JobOrderMenuHasOrder);

                            return;
                        }

                        var jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                        if (jobVehicle == null)
                            return;

                        if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                        {
                            CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.JobVehicleNotInVeh);

                            return;
                        }

                        var activeOrders = job.GetCurrentData<List<Collector.OrderInfo>>("AOL");

                        if (activeOrders == null)
                            return;

                        truckerJob.ShowOrderSelection(activeOrders);
                    }
                },

                {
                    Types.Cabbie,

                    (pData, job) =>
                    {
                        var cabbieJob = job as Cabbie;

                        if (cabbieJob.GetCurrentData<Cabbie.OrderInfo>("CO") != null)
                        {
                            CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.JobOrderMenuHasOrder);

                            return;
                        }

                        var jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                        if (jobVehicle == null)
                            return;

                        if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                        {
                            CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.JobVehicleNotInVeh);

                            return;
                        }

                        var activeOrders = job.GetCurrentData<List<Cabbie.OrderInfo>>("AOL");

                        if (activeOrders == null)
                            return;

                        cabbieJob.ShowOrderSelection(activeOrders);
                    }
                },

                {
                    Types.BusDriver,

                    (pData, job) =>
                    {
                        var busJob = job as BusDriver;

                        if (Sync.Quest.GetPlayerQuest(pData, Sync.Quest.QuestData.Types.JBD1)?.Step > 0)
                        {
                            CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.JobOrderMenuHasOrder);

                            return;
                        }

                        var jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                        if (jobVehicle == null)
                            return;

                        if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                        {
                            CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.JobVehicleNotInVeh);

                            return;
                        }

                        busJob.ShowRouteSelection();
                    }
                },
            };

        public static void ShowJobMenu()
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

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

        public Types Type { get; set; }

        public string Name => Locale.Property.JobNames.GetValueOrDefault(Type) ?? "null";

        public Blip Blip { get; set; }

        public NPC JobGiver { get; set; }

        private Dictionary<string, object> CurrentData { get; set; }

        public Job(int Id, Types Type)
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

            CEF.HUD.Menu.UpdateCurrentTypes(true, HUD.Menu.Types.Job_Menu);
        }

        public virtual void OnEndJob()
        {
            if (CurrentData != null)
            {
                CurrentData.Clear();

                CurrentData = null;
            }

            CEF.HUD.Menu.UpdateCurrentTypes(false, HUD.Menu.Types.Job_Menu);
        }
    }

    public class JobEvents : Events.Script
    {
        public JobEvents()
        {
            Events.Add("Player::SCJ", (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

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
                    var job = Data.Jobs.Job.Get((int)args[0]);

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
                var newBalance = args[0].ToDecimal();

                var oldBalance = args[1].ToDecimal();

                var diff = newBalance > oldBalance ? newBalance - oldBalance : 0;

                if (diff > 0)
                {
                    CEF.Notification.Show(CEF.Notification.Types.Information, "+" + Utils.GetPriceString(diff), $"Сумма Вашей зарплаты: {Utils.GetPriceString(newBalance)}");
                }
            });

            Events.Add("Job::TR::OC", (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

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

                    order.TargetBusiness = Data.Locations.Business.All[businessId];

                    var existingOrder = activeOrders.Where(x => x.Id == order.Id).FirstOrDefault();

                    if (existingOrder != null)
                        activeOrders.Remove(existingOrder);

                    activeOrders.Add(order);

                    CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Notifications.DefHeader, string.Format(Locale.Notifications.General.JobNewOrder, KeyBinds.Get(KeyBinds.Types.Menu).GetKeyString(), Locale.HudMenu.Names.GetValueOrDefault(CEF.HUD.Menu.Types.Job_Menu) ?? "null"));

                    CEF.Audio.PlayOnce("JOB_NEWORDER", CEF.Audio.TrackTypes.Success0, 0.5f, 0);
                }
                else
                {
                    var id = args[0].ToUInt32();

                    var order = activeOrders.Where(x => x.Id == id).FirstOrDefault();

                    if (order == null)
                        return;

                    activeOrders.Remove(order);
                }

                activeOrders = activeOrders.OrderByDescending(x => x.Reward).ToList();

                job.SetCurrentData("AOL", activeOrders);

                if (CEF.ActionBox.CurrentContextStr == "JobTruckerOrderSelect")
                {
                    CEF.ActionBox.Close();

                    job.ShowOrderSelection(activeOrders);
                }
            });

            Events.Add("Job::CL::OC", (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

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

                    order.TargetBusiness = Data.Locations.Business.All[businessId];

                    var existingOrder = activeOrders.Where(x => x.Id == order.Id).FirstOrDefault();

                    if (existingOrder != null)
                        activeOrders.Remove(existingOrder);

                    activeOrders.Add(order);

                    CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Notifications.DefHeader, string.Format(Locale.Notifications.General.JobNewOrder, KeyBinds.Get(KeyBinds.Types.Menu).GetKeyString(), Locale.HudMenu.Names.GetValueOrDefault(CEF.HUD.Menu.Types.Job_Menu) ?? "null"));

                    CEF.Audio.PlayOnce("JOB_NEWORDER", CEF.Audio.TrackTypes.Success0, 0.5f, 0);
                }
                else
                {
                    var id = args[0].ToUInt32();

                    var order = activeOrders.Where(x => x.Id == id).FirstOrDefault();

                    if (order == null)
                        return;

                    activeOrders.Remove(order);
                }

                activeOrders = activeOrders.OrderByDescending(x => x.Reward).ToList();

                job.SetCurrentData("AOL", activeOrders);

                if (CEF.ActionBox.CurrentContextStr == "JobCollectorOrderSelect")
                {
                    CEF.ActionBox.Close();

                    job.ShowOrderSelection(activeOrders);
                }
            });

            Events.Add("Job::CAB::OC", (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

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

                    CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Notifications.DefHeader, string.Format(Locale.Notifications.General.JobNewOrder, KeyBinds.Get(KeyBinds.Types.Menu).GetKeyString(), Locale.HudMenu.Names.GetValueOrDefault(CEF.HUD.Menu.Types.Job_Menu) ?? "null"));

                    CEF.Audio.PlayOnce("JOB_NEWORDER", CEF.Audio.TrackTypes.Success0, 0.5f, 0);
                }
                else
                {
                    var id = args[0].ToUInt32();

                    if (args.Length > 1 && args[1] is bool success)
                    {
                        if (job.GetCurrentData<Cabbie.OrderInfo>("CO")?.Id == id)
                        {
                            job.GetCurrentData<Blip>("Blip")?.Destroy();
                            job.GetCurrentData<Additional.ExtraColshape>("CS")?.Destroy();

                            job.ResetCurrentData("CO");
                            job.ResetCurrentData("Blip");
                            job.ResetCurrentData("CS");
                        }

                        if (success)
                        {
                            CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Notifications.DefHeader, Locale.Notifications.General.Taxi4);
                        }
                        else
                        {
                            CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.DefHeader, Locale.Notifications.General.JobOrderCancelByCaller);

                            CEF.Audio.PlayOnce("JOB_CANCELORDER", CEF.Audio.TrackTypes.Error0, 0.5f, 0);
                        }
                    }

                    var order = activeOrders.Where(x => x.Id == id).FirstOrDefault();

                    if (order == null)
                        return;

                    activeOrders.Remove(order);
                }

                job.SetCurrentData("AOL", activeOrders);

                if (CEF.ActionBox.CurrentContextStr == "JobCabbieOrderSelect")
                {
                    CEF.ActionBox.Close();

                    job.ShowOrderSelection(activeOrders);
                }
            });
        }
    }
}
