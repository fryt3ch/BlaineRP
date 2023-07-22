using System;
using System.Collections.Generic;
using System.Text;

namespace BlaineRP.Server.Sync.Quests.Types
{
    public class JFRM1
    {
        public static void Initialize()
        {
            new Quest.QuestData(Quest.QuestData.Types.JFRM1)
            {
                ProgressUpdateFunc = (pData, questData, data) =>
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

                    if (cropData == null || cropData.Timer != null)
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
            };
        }
    }
}
