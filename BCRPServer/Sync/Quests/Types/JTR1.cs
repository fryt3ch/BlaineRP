using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Sync.Quests.Types
{
    public class JTR1
    {
        public static void Initialize()
        {
            new Quest.QuestData(Quest.QuestData.Types.JTR1)
            {
                ProgressUpdateFunc = (pData, questData, data) =>
                {
                    var player = pData.Player;

                    if (player.Dimension != Utils.Dimensions.Main || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                        return 0;

                    var job = pData.CurrentJob as Game.Jobs.Trucker;

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

                        questData.UpdateStep(pData.Info, 1, 0, $"{orderId}&{order.MPIdx}&{order.TargetBusiness.ID}&{order.Reward}");
                    }
                    else if (questData.Step == 1)
                    {
                        uint orderId;

                        if (!uint.TryParse(questData.CurrentData.Split('&')[0], out orderId))
                            return 0;

                        var activeOrder = job.ActiveOrders.GetValueOrDefault(orderId);

                        if (activeOrder == null)
                            return 0;

                        if (job.MaterialsPositions[activeOrder.MPIdx].DistanceTo(player.Position) > 15f)
                            return 0;

                        questData.UpdateStepKeepOldData(pData.Info, 2, 0);
                    }
                    else
                    {
                        uint orderId;

                        if (!uint.TryParse(questData.CurrentData.Split('&')[0], out orderId))
                            return 0;

                        var activeOrder = job.ActiveOrders.GetValueOrDefault(orderId);

                        if (activeOrder == null)
                            return 0;

                        if (activeOrder.TargetBusiness.PositionInfo.DistanceTo(player.Position) > 15f)
                            return 0;

                        job.RemoveOrder(orderId, activeOrder);

                        if (pData.HasBankAccount(true))
                        {
                            ulong newBalance;

                            if (pData.BankAccount.TryAddMoneyDebit(activeOrder.Reward, out newBalance, true))
                                pData.BankAccount.SetDebitBalance(newBalance, null);
                        }

                        questData.UpdateStep(pData.Info, 0, 0);

                        if (activeOrder.IsCustom)
                        {
                            uint newMaterialsBalance;

                            if (activeOrder.TargetBusiness.Owner != null && activeOrder.TargetBusiness.TryAddMaterials(activeOrder.TargetBusiness.OrderedMaterials, out newMaterialsBalance, false))
                            {
                                activeOrder.TargetBusiness.SetMaterials(newMaterialsBalance);

                                activeOrder.TargetBusiness.OrderedMaterials = 0;

                                MySQL.BusinessUpdateBalances(activeOrder.TargetBusiness, true);

                                var sms = new Sync.Phone.SMS((uint)Sync.Phone.SMS.DefaultNumbers.Delivery, activeOrder.TargetBusiness.Owner, string.Format(Sync.Phone.SMS.GetDefaultSmsMessage(Sync.Phone.SMS.DefaultTypes.DeliveryBusinessFinishOrder), orderId));

                                Sync.Phone.SMS.Add(activeOrder.TargetBusiness.Owner, sms, true);
                            }
                        }
                        else
                        {
                            job.TryAddRandomDefaultOrder();
                        }
                    }

                    return byte.MaxValue;
                }
            };
        }
    }
}
