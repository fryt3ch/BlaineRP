using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlaineRP.Server.Sync.Quests.Types
{
    public class JCL1
    {
        public static void Initialize()
        {
            new Quest.QuestData(Quest.QuestData.Types.JCL1)
            {
                ProgressUpdateFunc = (pData, questData, data) =>
                {
                    var job = pData.CurrentJob as Game.Jobs.Collector;

                    if (job == null)
                        return 0;

                    var vData = pData.Player.Vehicle.GetMainData();

                    if (vData == null || pData.VehicleSeat != 0 || vData.OwnerID != pData.CID || vData.Job != job)
                        return 0;

                    if (questData.Step == 0)
                    {
                        if (data.Length != 1)
                            return 0;

                        uint orderId;

                        if (!uint.TryParse(data[0], out orderId))
                            return 0;

                        var order = job.ActiveOrders.GetValueOrDefault(orderId);

                        if (order == null)
                            return 1;

                        if (order.CurrentWorker != null)
                            return 2;

                        if (job.ActiveOrders.Where(x => x.Value.CurrentWorker == pData.Info).Any())
                            return 1;

                        job.SetOrderAsTaken(orderId, order, pData);

                        questData.UpdateStep(pData.Info, 1, 0, $"{orderId}&{order.TargetBusiness.ID}&{order.Reward}");
                    }
                    else if (questData.Step == 1)
                    {
                        if (data.Length > 1)
                            return 0;

                        uint orderId;

                        if (!uint.TryParse(questData.CurrentData.Split('&')[0], out orderId))
                            return 0;

                        var order = job.ActiveOrders.GetValueOrDefault(orderId);

                        if (order == null)
                            return 0;

                        if (pData.Player.Position.DistanceTo(order.TargetBusiness.PositionInfo) > 10f)
                            return 0;

                        questData.UpdateStepKeepOldData(pData.Info, 2, 0);
                    }
                    else if (questData.Step == 2)
                    {
                        if (data.Length > 1)
                            return 0;

                        uint orderId;

                        if (!uint.TryParse(questData.CurrentData.Split('&')[0], out orderId))
                            return 0;

                        var order = job.ActiveOrders.GetValueOrDefault(orderId);

                        if (order == null)
                            return 0;

                        if (pData.Player.Position.DistanceTo(job.Position.Position) > 10f)
                            return 0;

                        if (pData.HasBankAccount(true))
                        {
                            ulong newBalance;

                            if (pData.BankAccount.TryAddMoneyDebit(order.Reward, out newBalance, true))
                                pData.BankAccount.SetDebitBalance(newBalance, null);
                        }

                        job.RemoveOrder(orderId, order);

                        questData.UpdateStep(pData.Info, 0, 0, null);
                    }

                    return byte.MaxValue;
                }
            };
        }
    }
}
