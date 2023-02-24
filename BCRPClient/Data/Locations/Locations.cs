using BCRPClient.Additional;
using RAGE;
using RAGE.Elements;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Data
{
    public partial class Locations : Events.Script
    {
        public Locations()
        {
            CayoPerico.Initialize();

            Garage.Style.LoadAll();

            #region BIZS_TO_REPLACE

            #endregion

            #region JOBS_TO_REPLACE

            #endregion

            #region ATM_TO_REPLACE

            #endregion

            #region BANKS_TO_REPLACE

            #endregion

            #region AROOTS_TO_REPLACE

            #endregion

            #region APARTMENTS_TO_REPLACE

            #endregion

            #region HOUSES_TO_REPLACE

            #endregion

            #region GROOTS_TO_REPLACE

            #endregion

            #region GARAGES_TO_REPLACE

            #endregion

            new NPC("vpound_w_0", "Джон", NPC.Types.Talkable, "ig_trafficwarden", new Vector3(485.6506f, -54.18661f, 78.30058f), 55.38f, Settings.MAIN_DIMENSION)
            {
                Blip = new Blip(832, new Vector3(485.6506f, -54.18661f, 78.30058f), "Штрафстоянка", 1f, 47, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION),

                DefaultDialogueId = "vpound_preprocess",
            };

            new NPC("vrent_s_0", "Джон", NPC.Types.Talkable, "s_m_m_trucker_01", new Vector3(-718.6724f, 5821.765f, 17.21804f), 106.9247f, Settings.MAIN_DIMENSION)
            {
                Blip = new Blip(76, new Vector3(-718.6724f, 5821.765f, 17.21804f), "Аренда мопедов", 0.85f, 47, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION),

                DefaultDialogueId = "vrent_s_preprocess",
            };

            Events.Add("Job::TR::OC", (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var job = pData.CurrentJob as Data.Locations.Trucker;

                if (job == null)
                    return;

                var activeOrders = job.GetCurrentData<List<Data.Locations.Trucker.OrderInfo>>("AOL");

                if (activeOrders == null)
                    return;

                if (args[0] is string str)
                {
                    var oData = str.Split('_');

                    var order = new Data.Locations.Trucker.OrderInfo();

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

                    if (job.GetCurrentData<Data.Locations.Trucker.OrderInfo>("CO")?.Id == id)
                    {
                        job.ResetCurrentData("CO");
                    }

                    var order = activeOrders.Where(x => x.Id == id).FirstOrDefault();

                    if (order == null)
                        return;

                    activeOrders.Remove(order);
                }

                activeOrders = activeOrders.OrderByDescending(x => x.Reward).ToList();

                job.SetCurrentData("AOL", activeOrders);

                if (CEF.ActionBox.CurrentContext == CEF.ActionBox.Contexts.JobTruckerOrderSelect)
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

                var job = pData.CurrentJob as Data.Locations.Cabbie;

                if (job == null)
                    return;

                var activeOrders = job.GetCurrentData<List<Data.Locations.Cabbie.OrderInfo>>("AOL");

                if (activeOrders == null)
                    return;

                if (args[0] is string str)
                {
                    var oData = str.Split('_');

                    var order = new Data.Locations.Cabbie.OrderInfo();

                    order.Id = uint.Parse(oData[0]);

                    var pos = new Vector3(float.Parse(oData[1]), float.Parse(oData[2]), float.Parse(oData[3]));

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
                        if (job.GetCurrentData<Data.Locations.Cabbie.OrderInfo>("CO")?.Id == id)
                        {
                            job.GetCurrentData<Blip>("Blip")?.Destroy();
                            job.GetCurrentData<ExtraColshape>("CS")?.Destroy();

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

                if (CEF.ActionBox.CurrentContext == CEF.ActionBox.Contexts.JobTruckerOrderSelect)
                {
                    CEF.ActionBox.Close();

                    job.ShowOrderSelection(activeOrders);
                }
            });
        }
    }
}