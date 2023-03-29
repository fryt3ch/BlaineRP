using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Sync.Quests.Types
{
    public class JBD1
    {
        public static void Initialize()
        {
            new Quest.QuestData(Quest.QuestData.Types.JBD1)
            {
                ProgressUpdateFunc = (pData, questData, data) =>
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
            };
        }
    }
}
