using RAGE;
using RAGE.Elements;
using System;
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

                    order.Id = int.Parse(oData[0]);
                    order.MPIdx = int.Parse(oData[1]);
                    order.Reward = uint.Parse(oData[2]);

                    order.TargetBusiness = Data.Locations.Business.All[order.Id < 0 ? -order.Id : order.Id];

                    var existingOrder = activeOrders.Where(x => x.Id == order.Id).FirstOrDefault();

                    if (existingOrder != null)
                        activeOrders.Remove(existingOrder);

                    activeOrders.Add(order);
                }
                else
                {
                    var id = (int)args[0];

                    var order = activeOrders.Where(x => x.Id == id).FirstOrDefault();

                    if (order == null)
                        return;

                    activeOrders.Remove(order);
                }

                if (CEF.ActionBox.CurrentContext == CEF.ActionBox.Contexts.JobTruckerOrderSelect)
                {
                    CEF.ActionBox.Close();

                    job.ShowOrderSelection(activeOrders);
                }
            });
        }
    }
}