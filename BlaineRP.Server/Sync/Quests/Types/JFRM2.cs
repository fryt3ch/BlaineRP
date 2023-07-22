using System;
using System.Collections.Generic;
using System.Text;

namespace BlaineRP.Server.Sync.Quests.Types
{
    public class JFRM2
    {
        public static void Initialize()
        {
            new Quest.QuestData(Quest.QuestData.Types.JFRM2)
            {
                ProgressUpdateFunc = (pData, questData, data) =>
                {
                    if (data.Length != 2)
                        return 0;

                    var job = pData.CurrentJob as Game.Jobs.Farmer;

                    if (job == null)
                        return 0;

                    var vData = pData.Player.Vehicle.GetMainData();

                    if (vData == null || pData.VehicleSeat != 0 || vData.OwnerID != pData.CID || vData.Job != job)
                        return 0;

                    int fieldIdx; int pointIdx;

                    if (!int.TryParse(data[0], out fieldIdx) || !int.TryParse(data[1], out pointIdx))
                        return 0;

                    var farmBusiness = job.FarmBusiness;

                    if (fieldIdx < 0 || pointIdx < 0 || farmBusiness.CropFields == null || fieldIdx >= farmBusiness.CropFields.Count || pointIdx >= farmBusiness.CropFields[fieldIdx].IrrigationPoints.Count)
                        return 0;

                    if (Game.Businesses.Farm.CropField.GetIrrigationEndTime(farmBusiness, fieldIdx) != null)
                        return 0;

                    if (pData.Player.Position.DistanceTo(farmBusiness.CropFields[fieldIdx].IrrigationPoints[pointIdx]) > 15f)
                        return 0;

                    var irrigatedData = Game.Jobs.Farmer.GetPlayerFieldsIrrigationData(pData);

                    if (irrigatedData == null)
                        return 0;

                    HashSet<int> pointsData;

                    if (!irrigatedData.TryGetValue(fieldIdx, out pointsData))
                    {
                        pointsData = new HashSet<int>();

                        irrigatedData.Add(fieldIdx, pointsData);
                    }

                    if (!pointsData.Add(pointIdx))
                        return 0;

                    if (pointsData.Count >= farmBusiness.CropFields[fieldIdx].IrrigationPoints.Count)
                    {
                        irrigatedData.Remove(fieldIdx);

                        var curTimeSecs = Utils.GetCurrentTime().GetUnixTimestamp();

                        farmBusiness.CropFields[fieldIdx].UpdateIrrigationEndTime(farmBusiness, fieldIdx, curTimeSecs + Game.Jobs.Farmer.CropFieldIrrigationDuration, true);

                        for (byte j = 0; j < farmBusiness.CropFields[fieldIdx].CropsData.Count; j++)
                        {
                            for (byte k = 0; k < farmBusiness.CropFields[fieldIdx].CropsData[j].Count; k++)
                            {
                                var cropData = farmBusiness.CropFields[fieldIdx].CropsData[j][k];

                                if (cropData.WasIrrigated)
                                    continue;

                                if (Game.Businesses.Farm.CropField.CropData.GetGrowTime(farmBusiness, fieldIdx, j, k) is long growTime && growTime > 0)
                                {
                                    var secsLeft = growTime - curTimeSecs;

                                    if (secsLeft <= 0)
                                        continue;

                                    secsLeft = curTimeSecs + (int)Math.Floor(secsLeft * Game.Jobs.Farmer.CropFieldIrrigationTimeCoef);

                                    cropData.WasIrrigated = true;

                                    cropData.UpdateGrowTime(farmBusiness, fieldIdx, j, k, secsLeft, true);
                                }
                            }
                        }

                        uint newMats, playerTotalSalary;
                        ulong newBizBalance;

                        if (farmBusiness.TryProceedPayment(pData, $"crop_{(int)farmBusiness.CropFields[fieldIdx].Type}_2", Game.Jobs.Farmer.GetPlayerSalaryCoef(pData.Info), out newMats, out newBizBalance, out playerTotalSalary))
                            farmBusiness.ProceedPayment(pData, newMats, newBizBalance, playerTotalSalary);

                        return 254;
                    }

                    return byte.MaxValue;
                }
            };
        }
    }
}
