﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Events.Players
{
    public class Quests : Script
    {
        private static Dictionary<Sync.Quest.QuestData.Types, Func<PlayerData, Sync.Quest, int, string[], byte>> QuestProgressFuncs = new Dictionary<Sync.Quest.QuestData.Types, Func<PlayerData, Sync.Quest, int, string[], byte>>()
        {
            {
                Sync.Quest.QuestData.Types.JTR1,

                (pData, questData, newProgress, data) =>
                {
                    if (questData.Step < 1)
                        return 0;

                    var player = pData.Player;

                    if (data.Length < 1)
                        return 0;

                    uint orderId;

                    if (!uint.TryParse(data[0], out orderId))
                        return 0;

                    if (player.Dimension != Utils.Dimensions.Main || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                        return 0;

                    var job = pData.CurrentJob as Game.Jobs.Trucker;

                    if (job == null)
                        return 0;

                    var activeOrder = job.ActiveOrders.GetValueOrDefault(orderId);

                    if (activeOrder == null)
                        return 0;

                    if (pData.VehicleSeat != 0)
                        return 0;

                    if (activeOrder.CurrentVehicle != player.Vehicle.GetMainData())
                        return 0;

                    if (questData.Step == 1)
                    {
                        if (activeOrder.GotMaterials)
                            return 0;

                        if (job.MaterialsPositions[activeOrder.MPIdx].DistanceTo(player.Position) > 15f)
                            return 0;

                        activeOrder.GotMaterials = true;

                        questData.UpdateStepKeepOldData(pData.Info, 2);
                    }
                    else
                    {
                        if (!activeOrder.GotMaterials)
                            return 0;

                        if (activeOrder.TargetBusiness.PositionInfo.DistanceTo(player.Position) > 15f)
                            return 0;

                        job.RemoveOrder(orderId);

                        questData.UpdateStep(pData.Info, 0);

                        if (pData.HasBankAccount(true))
                        {
                            ulong newBalance;

                            if (pData.BankAccount.TryAddMoneyDebit(activeOrder.Reward, out newBalance, true))
                                pData.BankAccount.SetDebitBalance(newBalance, null);
                        }

                        if (activeOrder.IsCustom)
                        {
                            uint newMaterialsBalance;

                            if (activeOrder.TargetBusiness.Owner != null && activeOrder.TargetBusiness.TryAddMaterials(activeOrder.TargetBusiness.OrderedMaterials, out newMaterialsBalance, false))
                            {
                                activeOrder.TargetBusiness.SetMaterials(newMaterialsBalance);

                                MySQL.BusinessUpdateBalances(activeOrder.TargetBusiness);

                                // set ordered materials to 0
                            }
                        }
                        else
                        {
                            job.TryAddRandomDefaultOrder();
                        }
                    }

                    return byte.MaxValue;
                }
            }
        };

        [RemoteProc("Quest::PU")]
        private static byte ProgressUpdate(Player player, int questTypeNum, int newProgress, string data)
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

            return func.Invoke(pData, questData, newProgress, data.Split('&'));
        }
    }
}
