using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.Businesses;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.UI.CEF;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Jobs
{
    public abstract partial class Job
    {
        [Script(int.MaxValue)]
        public class Events
        {
            public Events()
            {
                RAGE.Events.Add("Player::SCJ",
                    (args) =>
                    {
                        var pData = PlayerData.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        if (args == null || args.Length < 1)
                        {
                            Job lastJob = pData.CurrentJob;

                            if (lastJob != null)
                            {
                                lastJob.OnEndJob();

                                pData.CurrentJob = null;
                            }
                        }
                        else
                        {
                            Job job = Get((int)args[0]);

                            Job lastJob = pData.CurrentJob;

                            if (lastJob != null)
                                lastJob.OnEndJob();

                            pData.CurrentJob = job;

                            job.OnStartJob(args.Skip(1).ToArray());
                        }
                    }
                );

                RAGE.Events.Add("Job::TSC",
                    (args) =>
                    {
                        var newBalance = Utils.Convert.ToDecimal(args[0]);

                        var oldBalance = Utils.Convert.ToDecimal(args[1]);

                        decimal diff = newBalance > oldBalance ? newBalance - oldBalance : 0;

                        if (diff > 0)
                            Notification.Show(Notification.Types.Information,
                                "+" + Locale.Get("GEN_MONEY_0", diff),
                                $"Сумма Вашей зарплаты: {Locale.Get("GEN_MONEY_0", newBalance)}"
                            );
                    }
                );

                RAGE.Events.Add("Job::TR::OC",
                    (args) =>
                    {
                        var pData = PlayerData.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        var job = pData.CurrentJob as Trucker;

                        if (job == null)
                            return;

                        List<Trucker.OrderInfo> activeOrders = job.GetCurrentData<List<Trucker.OrderInfo>>("AOL");

                        if (activeOrders == null)
                            return;

                        if (args[0] is string str)
                        {
                            string[] oData = str.Split('_');

                            var order = new Trucker.OrderInfo();

                            order.Id = uint.Parse(oData[0]);
                            var businessId = int.Parse(oData[1]);
                            order.MPIdx = int.Parse(oData[2]);
                            order.Reward = uint.Parse(oData[3]);

                            order.TargetBusiness = Business.All[businessId];

                            Trucker.OrderInfo existingOrder = activeOrders.Where(x => x.Id == order.Id).FirstOrDefault();

                            if (existingOrder != null)
                                activeOrders.Remove(existingOrder);

                            activeOrders.Add(order);

                            Notification.Show(Notification.Types.Information,
                                Locale.Get("NOTIFICATION_HEADER_DEF"),
                                string.Format(Locale.Notifications.General.JobNewOrder,
                                    Input.Core.Get(Input.Enums.BindTypes.Menu).GetKeyString(),
                                    Locale.Get(Language.Strings.GetKeyFromTypeByMemberName(typeof(HUD.Menu.Types), nameof(HUD.Menu.Types.Job_Menu), "MENU_I_NAME") ?? "null")
                                )
                            );

                            Audio.PlayOnce("JOB_NEWORDER", Audio.TrackTypes.Success0, 0.5f, 0);
                        }
                        else
                        {
                            var id = Utils.Convert.ToUInt32(args[0]);

                            Trucker.OrderInfo order = activeOrders.Where(x => x.Id == id).FirstOrDefault();

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
                    }
                );

                RAGE.Events.Add("Job::CL::OC",
                    (args) =>
                    {
                        var pData = PlayerData.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        var job = pData.CurrentJob as Collector;

                        if (job == null)
                            return;

                        List<Collector.OrderInfo> activeOrders = job.GetCurrentData<List<Collector.OrderInfo>>("AOL");

                        if (activeOrders == null)
                            return;

                        if (args[0] is string str)
                        {
                            string[] oData = str.Split('_');

                            var order = new Collector.OrderInfo();

                            order.Id = uint.Parse(oData[0]);
                            var businessId = int.Parse(oData[1]);
                            order.Reward = uint.Parse(oData[2]);

                            order.TargetBusiness = Business.All[businessId];

                            Collector.OrderInfo existingOrder = activeOrders.Where(x => x.Id == order.Id).FirstOrDefault();

                            if (existingOrder != null)
                                activeOrders.Remove(existingOrder);

                            activeOrders.Add(order);

                            Notification.Show(Notification.Types.Information,
                                Locale.Get("NOTIFICATION_HEADER_DEF"),
                                string.Format(Locale.Notifications.General.JobNewOrder,
                                    Input.Core.Get(Input.Enums.BindTypes.Menu).GetKeyString(),
                                    Locale.Get(Language.Strings.GetKeyFromTypeByMemberName(typeof(HUD.Menu.Types), nameof(HUD.Menu.Types.Job_Menu), "MENU_I_NAME") ?? "null")
                                )
                            );

                            Audio.PlayOnce("JOB_NEWORDER", Audio.TrackTypes.Success0, 0.5f, 0);
                        }
                        else
                        {
                            var id = Utils.Convert.ToUInt32(args[0]);

                            Collector.OrderInfo order = activeOrders.Where(x => x.Id == id).FirstOrDefault();

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
                    }
                );

                RAGE.Events.Add("Job::CAB::OC",
                    (args) =>
                    {
                        var pData = PlayerData.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        var job = pData.CurrentJob as Cabbie;

                        if (job == null)
                            return;

                        List<Cabbie.OrderInfo> activeOrders = job.GetCurrentData<List<Cabbie.OrderInfo>>("AOL");

                        if (activeOrders == null)
                            return;

                        if (args[0] is string str)
                        {
                            string[] oData = str.Split('_');

                            var order = new Cabbie.OrderInfo()
                            {
                                Id = uint.Parse(oData[0]),
                                Position = new Vector3(float.Parse(oData[1]), float.Parse(oData[2]), float.Parse(oData[3])),
                            };

                            Cabbie.OrderInfo existingOrder = activeOrders.Where(x => x.Id == order.Id).FirstOrDefault();

                            if (existingOrder != null)
                                activeOrders.Remove(existingOrder);

                            activeOrders.Add(order);

                            Notification.Show(Notification.Types.Information,
                                Locale.Get("NOTIFICATION_HEADER_DEF"),
                                string.Format(Locale.Notifications.General.JobNewOrder,
                                    Input.Core.Get(Input.Enums.BindTypes.Menu).GetKeyString(),
                                    Locale.Get(Language.Strings.GetKeyFromTypeByMemberName(typeof(HUD.Menu.Types), nameof(HUD.Menu.Types.Job_Menu), "MENU_I_NAME") ?? "null")
                                )
                            );

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

                            Cabbie.OrderInfo order = activeOrders.Where(x => x.Id == id).FirstOrDefault();

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
                    }
                );
            }
        }
    }
}