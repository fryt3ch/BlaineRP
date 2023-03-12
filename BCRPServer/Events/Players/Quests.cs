using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Events.Players
{
    public class Quests : Script
    {
        private static Dictionary<Sync.Quest.QuestData.Types, Func<PlayerData, Sync.Quest, string[], byte>> QuestProgressFuncs = new Dictionary<Sync.Quest.QuestData.Types, Func<PlayerData, Sync.Quest, string[], byte>>()
        {
            {
                Sync.Quest.QuestData.Types.JTR1,

                (pData, questData, data) =>
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
            },

            {
                Sync.Quest.QuestData.Types.JBD1,

                (pData, questData, data) =>
                {
                    var job = pData.CurrentJob as Game.Jobs.BusDriver;

                    if (job == null)
                        return 0;

                    var vData = pData.Player.Vehicle.GetMainData();

                    if (vData == null || pData.VehicleSeat != 0 || vData.OwnerID != pData.CID || vData.Job != job)
                        return 0;

                    if (questData.Step == 0)
                    {
                        if (data.Length != 1)
                            return 0;

                        int routeIdx;

                        if (!int.TryParse(data[0], out routeIdx))
                            return 0;

                        if (routeIdx < 0 || routeIdx >= job.Routes.Count)
                            return 0;

                        questData.UpdateStep(pData.Info, 1, 0, $"{routeIdx}");
                    }
                    else if (questData.Step == 1)
                    {
                        if (data.Length > 1)
                            return 0;

                        int routeIdx;

                        if (!int.TryParse(questData.CurrentData, out routeIdx))
                            return 0;

                        if (pData.Player.Position.DistanceTo(job.Routes[routeIdx].Positions[0]) > 10f)
                            return 0;

                        questData.UpdateStepKeepOldData(pData.Info, 2, 0);
                    }
                    else if (questData.Step == 2)
                    {
                        if (data.Length > 1)
                            return 0;

                        int routeIdx;

                        if (!int.TryParse(questData.CurrentData, out routeIdx))
                            return 0;

                        var nextProgress = questData.StepProgress + 1;

                        if (pData.Player.Position.DistanceTo(job.Routes[routeIdx].Positions[nextProgress]) > 10f)
                            return 0;

                        if (nextProgress + 1 >= job.Routes[routeIdx].Positions.Count - 1)
                            questData.UpdateStepKeepOldData(pData.Info, 3, 0);
                        else
                            questData.UpdateStepKeepOldData(pData.Info, 2, nextProgress);
                    }
                    else
                    {
                        if (data.Length > 1)
                            return 0;

                        int routeIdx;

                        if (!int.TryParse(questData.CurrentData, out routeIdx))
                            return 0;

                        if (pData.Player.Position.DistanceTo(job.Routes[routeIdx].Positions[job.Routes[routeIdx].Positions.Count - 1]) > 10f)
                            return 0;

                        if (pData.HasBankAccount(true))
                        {
                            ulong newBalance;

                            if (pData.BankAccount.TryAddMoneyDebit(job.Routes[routeIdx].Reward, out newBalance, true))
                                pData.BankAccount.SetDebitBalance(newBalance, null);
                        }

                        questData.UpdateStep(pData.Info, 0, 0, null);
                    }

                    return byte.MaxValue;
                }
            },

            {
                Sync.Quest.QuestData.Types.JCL1,

                (pData, questData, data) =>
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
            },

            {
                Sync.Quest.QuestData.Types.JFRM1,

                (pData, questData, data) =>
                {
                    if (data.Length != 3)
                        return 0;

                    var job = pData.CurrentJob as Game.Jobs.Farmer;

                    if (job == null)
                        return 0;

                    var vData = pData.Player.Vehicle.GetMainData();

                    if (vData == null || pData.VehicleSeat != 0 || vData.OwnerID != pData.CID || vData.Job != job)
                        return 0;

                    int fieldIdx; byte col, row;

                    if (!int.TryParse(data[0], out fieldIdx) || !byte.TryParse(data[1], out col) || !byte.TryParse(data[2], out row))
                        return 0;

                    var farmBusiness = job.FarmBusiness;

                    var cropData = Game.Businesses.Farm.CropField.GetData(farmBusiness, fieldIdx, col, row);

                    if (cropData == null || cropData.CTS != null)
                        return 0;

                    if (Game.Businesses.Farm.CropField.CropData.GetGrowTime(farmBusiness, fieldIdx, col, row) != 0)
                        return 0;

                    cropData.UpdateGrowTime(farmBusiness, fieldIdx, col, row, null, true);

                    uint newMats, playerTotalSalary;
                    ulong newBizBalance;

                    if (farmBusiness.TryProceedPayment(pData, $"crop_{(int)farmBusiness.CropFields[fieldIdx].Type}_1", Game.Jobs.Farmer.GetPlayerSalaryCoef(pData.Info), out newMats, out newBizBalance, out playerTotalSalary))
                        farmBusiness.ProceedPayment(pData, newMats, newBizBalance, playerTotalSalary);

                    return byte.MaxValue;
                }
            },
        };

        [RemoteProc("Quest::PU")]
        private static byte ProgressUpdate(Player player, int questTypeNum, string data)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (data == null)
                return 0;

            if (!Enum.IsDefined(typeof(Sync.Quest.QuestData.Types), questTypeNum))
                return 0;

            var questType = (Sync.Quest.QuestData.Types)questTypeNum;

            var questData = pData.Info.Quests.GetValueOrDefault(questType);

            if (questData == null)
                return 0;

            var func = QuestProgressFuncs.GetValueOrDefault(questType);

            if (func == null)
                return 0;

            return func.Invoke(pData, questData, data.Split('&'));
        }
    }
}
