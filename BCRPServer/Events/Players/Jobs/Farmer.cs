using GTANetworkAPI;
using System;

namespace BCRPServer.Events.Players.Jobs
{
    public class Farmer : Script
    {
        [RemoteProc("Job::FARM::TJ")]
        private static bool TakeJob(Player player, int jobId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (player.Dimension != Utils.Dimensions.Main || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return false;

            if (pData.HasJob(true))
                return false;

            var job = Game.Jobs.Job.Get(jobId) as Game.Jobs.Farmer;

            if (job == null)
                return false;

            if (job.Position.Position.DistanceTo(player.Position) > 10f)
                return false;

            if (!job.CanPlayerDoThisJob(pData))
                return false;

            job.SetPlayerJob(pData);

            return true;
        }

        [RemoteProc("Job::FARM::FJ")]
        private static bool FinishJob(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            var job = pData.CurrentJob as Game.Jobs.Farmer;

            if (job == null)
                return false;

            /*            if (job.Position.Position.DistanceTo(player.Position) > 10f)
                            return false;*/

            job.SetPlayerNoJob(pData.Info);

            return true;
        }

        [RemoteProc("Job::FARM::CP")]
        private static byte CropProcess(Player player, int fieldIdx, byte col, byte row)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (player.Dimension != Utils.Dimensions.Main || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked || player.Vehicle != null || pData.HasAnyHandAttachedObject || pData.IsAnyAnimOn())
                return 0;

            var job = pData.CurrentJob as Game.Jobs.Farmer;

            if (job == null)
                return 0;

            var farmBusiness = job.FarmBusiness;

            var cropData = Game.Businesses.Farm.CropField.GetData(farmBusiness, fieldIdx, col, row);

            if (cropData == null)
                return 0;

            if (cropData.CTS != null)
                return 1;

            var cropField = farmBusiness.CropFields[fieldIdx];

            var cropPos = cropField.GetCropPosition3D(col, row);

            if (cropPos == null)
                return 0;

            if (cropPos.DistanceTo(player.Position) > 5f)
                return 0;

            var growTimeT = Game.Businesses.Farm.CropField.CropData.GetGrowTime(farmBusiness, fieldIdx, col, row);

            if (growTimeT is long growTime)
            {
                if (cropField.Type == Game.Businesses.Farm.CropField.Types.Wheat)
                    return 0;

                cropData.CTS = new System.Threading.CancellationTokenSource();

                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        await System.Threading.Tasks.Task.Delay(10000, cropData.CTS.Token);

                        NAPI.Task.Run(() =>
                        {
                            if (pData.Player?.Exists == true)
                            {
                                if (pData.Player.DetachObject(Sync.AttachSystem.Types.FarmPlantSmallShovel))
                                {
                                    cropData.UpdateGrowTime(farmBusiness, fieldIdx, col, row, null, true);

                                    uint newMats, playerTotalSalary;
                                    ulong newBizBalance;

                                    if (farmBusiness.TryProceedPayment(pData, $"crop_{(int)cropField.Type}_1", Game.Jobs.Farmer.GetPlayerSalaryCoef(pData.Info), out newMats, out newBizBalance, out playerTotalSalary))
                                        farmBusiness.ProceedPayment(pData, newMats, newBizBalance, playerTotalSalary);
                                }
                            }
                        });
                    }
                    catch (Exception ex) { }
                });
            }
            else
            {
                cropData.CTS = new System.Threading.CancellationTokenSource();

                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        await System.Threading.Tasks.Task.Delay(10000, cropData.CTS.Token);

                        NAPI.Task.Run(() =>
                        {
                            if (pData.Player?.Exists == true)
                            {
                                if (pData.Player.DetachObject(Sync.AttachSystem.Types.FarmPlantSmallShovel))
                                {
                                    cropData.UpdateGrowTime(farmBusiness, fieldIdx, col, row, Utils.GetCurrentTime().GetUnixTimestamp() + Game.Jobs.Farmer.GetGrowTimeForCrop(cropField.Type), true);

                                    uint newMats, playerTotalSalary;
                                    ulong newBizBalance;

                                    if (farmBusiness.TryProceedPayment(pData, $"crop_{(int)cropField.Type}_0", Game.Jobs.Farmer.GetPlayerSalaryCoef(pData.Info), out newMats, out newBizBalance, out playerTotalSalary))
                                        farmBusiness.ProceedPayment(pData, newMats, newBizBalance, playerTotalSalary);
                                }
                            }
                        });
                    }
                    catch (Exception ex) { }
                });
            }

            Game.Jobs.Farmer.SetPlayerCurrentCropInfo(pData, fieldIdx, col, row);

            pData.Player.AttachObject(Game.Jobs.Farmer.LittleShovelModel, Sync.AttachSystem.Types.FarmPlantSmallShovel, -1, null);

            return byte.MaxValue;
        }

        [RemoteEvent("Job::FARM::SCP")]
        private static void StopCropProcess(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            pData.Player.DetachObject(Sync.AttachSystem.Types.FarmPlantSmallShovel);
        }

        [RemoteProc("Job::FARM::OTP")]
        private static byte OrangeTreeProcess(Player player, int idx)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (player.Dimension != Utils.Dimensions.Main || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked || player.Vehicle != null || pData.HasAnyHandAttachedObject || pData.IsAnyAnimOn())
                return 0;

            var job = pData.CurrentJob as Game.Jobs.Farmer;

            if (job == null)
                return 0;

            var farmBusiness = job.FarmBusiness;

            var cropData = Game.Businesses.Farm.OrangeTreeData.GetData(farmBusiness, idx);

            if (cropData == null)
                return 0;

            if (cropData.CTS != null)
                return 1;

            if (cropData.Position.DistanceTo(player.Position) > 5f)
                return 0;

            var growTimeT = Game.Businesses.Farm.OrangeTreeData.GetGrowTime(farmBusiness, idx);

            if (growTimeT is long growTime)
            {
                cropData.CTS = new System.Threading.CancellationTokenSource();

                pData.PlayAnim(Sync.Animations.GeneralTypes.TreeCollect0);

                player.TriggerEvent("MG::OTC::S");
            }
            else
            {
                cropData.CTS = new System.Threading.CancellationTokenSource();

                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        await System.Threading.Tasks.Task.Delay(10000, cropData.CTS.Token);

                        NAPI.Task.Run(() =>
                        {
                            if (pData.Player?.Exists == true)
                            {
                                if (pData.Player.DetachObject(Sync.AttachSystem.Types.FarmWateringCan))
                                {
                                    cropData.UpdateGrowTime(farmBusiness, idx, Utils.GetCurrentTime().GetUnixTimestamp() + Game.Jobs.Farmer.GetGrowTimeForCrop(Game.Businesses.Farm.CropField.Types.OrangeTree), true);

                                    uint newMats, playerTotalSalary;
                                    ulong newBizBalance;

                                    if (farmBusiness.TryProceedPayment(pData, $"crop_{(int)Game.Businesses.Farm.CropField.Types.OrangeTree}_0", Game.Jobs.Farmer.GetPlayerSalaryCoef(pData.Info), out newMats, out newBizBalance, out playerTotalSalary))
                                        farmBusiness.ProceedPayment(pData, newMats, newBizBalance, playerTotalSalary);
                                }
                            }
                        });
                    }
                    catch (Exception ex) { }
                });

                pData.Player.AttachObject(Game.Jobs.Farmer.WateringCanModel, Sync.AttachSystem.Types.FarmWateringCan, -1, null);
            }

            Game.Jobs.Farmer.SetPlayerCurrentOrangeTreeInfo(pData, idx);

            return byte.MaxValue;
        }

        [RemoteEvent("Job::FARM::SOTP")]
        private static void StopOrangeTreeProcess(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!pData.Player.DetachObject(Sync.AttachSystem.Types.FarmWateringCan))
            {
                if (!pData.Player.DetachObject(Sync.AttachSystem.Types.FarmOrangeBoxCarry))
                {
                    var job = pData.CurrentJob as Game.Jobs.Farmer;

                    if (job == null)
                        return;

                    int treeIdx;

                    if (!Game.Jobs.Farmer.TryGetPlayerCurrentOrangeTreeInfo(pData, out treeIdx))
                        return;

                    var business = job.FarmBusiness;

                    if (business.OrangeTrees[treeIdx].CTS != null)
                    {
                        business.OrangeTrees[treeIdx].CTS.Cancel();
                        business.OrangeTrees[treeIdx].CTS.Dispose();

                        business.OrangeTrees[treeIdx].CTS = null;
                    }

                    if (pData.GeneralAnim == Sync.Animations.GeneralTypes.TreeCollect0)
                        pData.StopGeneralAnim();

                    Game.Jobs.Farmer.ResetPlayerCurrentOrangeTreeInfo(pData);
                }
            }
        }

        [RemoteEvent("Job::FARM::OTFC")]
        private static void OrangeTreeFinishCollect(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var job = pData.CurrentJob as Game.Jobs.Farmer;

            if (job == null)
                return;

            int treeIdx;

            if (!Game.Jobs.Farmer.TryGetPlayerCurrentOrangeTreeInfo(pData, out treeIdx))
                return;

            var business = job.FarmBusiness;

            if (Game.Businesses.Farm.OrangeTreeData.GetGrowTime(business, treeIdx) != 0)
                return;

            if (pData.GeneralAnim != Sync.Animations.GeneralTypes.TreeCollect0 || player.Dimension != Utils.Dimensions.Main || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked || player.Vehicle != null || business.OrangeTrees[treeIdx].Position.DistanceTo(player.Position) > 10f)
            {
                if (business.OrangeTrees[treeIdx].CTS != null)
                {
                    business.OrangeTrees[treeIdx].CTS.Cancel();
                    business.OrangeTrees[treeIdx].CTS.Dispose();

                    business.OrangeTrees[treeIdx].CTS = null;
                }

                if (pData.GeneralAnim == Sync.Animations.GeneralTypes.TreeCollect0)
                    pData.StopGeneralAnim();

                Game.Jobs.Farmer.ResetPlayerCurrentOrangeTreeInfo(pData);

                return;
            }

            pData.Player.AttachObject(Game.Jobs.Farmer.BoxModel, Sync.AttachSystem.Types.FarmOrangeBoxCarry, -1, null);
        }

        [RemoteEvent("Job::FARM::OTF")]
        private static void OrangeTreeFinish(Player player, int boxIdx)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var job = pData.CurrentJob as Game.Jobs.Farmer;

            if (job == null)
                return;

            int treeIdx;

            if (!Game.Jobs.Farmer.TryGetPlayerCurrentOrangeTreeInfo(pData, out treeIdx))
                return;

            var farmBusiness = job.FarmBusiness;

            if (Game.Businesses.Farm.OrangeTreeData.GetGrowTime(farmBusiness, treeIdx) != 0)
                return;

            if (boxIdx < 0 || boxIdx >= farmBusiness.OrangeTreeBoxPositions.Count)
                return;

            if (player.DetachObject(Sync.AttachSystem.Types.FarmOrangeBoxCarry))
            {
                if (player.Dimension != Utils.Dimensions.Main || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked || player.Vehicle != null || farmBusiness.OrangeTreeBoxPositions[boxIdx].DistanceTo(player.Position) > 5f)
                    return;

                farmBusiness.OrangeTrees[treeIdx].UpdateGrowTime(farmBusiness, treeIdx, null, true);

                uint newMats, playerTotalSalary;
                ulong newBizBalance;

                if (farmBusiness.TryProceedPayment(pData, $"crop_{(int)Game.Businesses.Farm.CropField.Types.OrangeTree}_1", Game.Jobs.Farmer.GetPlayerSalaryCoef(pData.Info), out newMats, out newBizBalance, out playerTotalSalary))// give salary
                    farmBusiness.ProceedPayment(pData, newMats, newBizBalance, playerTotalSalary);
            }
        }

        [RemoteProc("Job::FARM::COWP")]
        private static byte CowProcess(Player player, int idx)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (player.Dimension != Utils.Dimensions.Main || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked || player.Vehicle != null || pData.HasAnyHandAttachedObject || pData.IsAnyAnimOn())
                return 0;

            var job = pData.CurrentJob as Game.Jobs.Farmer;

            if (job == null)
                return 0;

            var farmBusiness = job.FarmBusiness;

            var cropData = Game.Businesses.Farm.CowData.GetData(farmBusiness, idx);

            Game.Businesses.Farm.CowData.GetData(farmBusiness, idx);

            if (cropData == null)
                return 0;

            if (cropData.CTS != null)
                return 1;

            if (cropData.Position.Position.DistanceTo(player.Position) > 5f)
                return 0;

            var growTimeT = Game.Businesses.Farm.CowData.GetGrowTime(farmBusiness, idx);

            if (growTimeT is long growTime)
            {
                cropData.CTS = new System.Threading.CancellationTokenSource();

                if (!pData.CrouchOn)
                    pData.CrouchOn = true;

                pData.PlayAnim(Sync.Animations.GeneralTypes.MilkCow0);

                player.TriggerEvent("MG::COWC::S");
            }
            else
            {
                return 0;
            }

            Game.Jobs.Farmer.SetPlayerCurrentCowInfo(pData, idx);

            return byte.MaxValue;
        }

        [RemoteEvent("Job::FARM::SCOWP")]
        private static void StopCowProcess(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!pData.Player.DetachObject(Sync.AttachSystem.Types.FarmMilkBucketCarry))
            {
                var job = pData.CurrentJob as Game.Jobs.Farmer;

                if (job == null)
                    return;

                int cowIdx;

                if (!Game.Jobs.Farmer.TryGetPlayerCurrentCowInfo(pData, out cowIdx))
                    return;

                var business = job.FarmBusiness;

                if (business.Cows[cowIdx].CTS != null)
                {
                    business.Cows[cowIdx].CTS.Cancel();
                    business.Cows[cowIdx].CTS.Dispose();

                    business.Cows[cowIdx].CTS = null;
                }

                if (pData.GeneralAnim == Sync.Animations.GeneralTypes.MilkCow0)
                    pData.StopGeneralAnim();

                Game.Jobs.Farmer.ResetPlayerCurrentCowInfo(pData);
            }
        }

        //2133.948f, 4783.06f, 41.31395f, 23.95123f | 2143.947f, 4813.993f, 41.57311f, 114.3835f - planes pos

        [RemoteEvent("Job::FARM::COWFC")]
        private static void CowFinishCollect(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var job = pData.CurrentJob as Game.Jobs.Farmer;

            if (job == null)
                return;

            int cowIdx;

            if (!Game.Jobs.Farmer.TryGetPlayerCurrentCowInfo(pData, out cowIdx))
                return;

            var business = job.FarmBusiness;

            if (Game.Businesses.Farm.CowData.GetGrowTime(business, cowIdx) != 0)
                return;

            if (pData.GeneralAnim != Sync.Animations.GeneralTypes.MilkCow0 || player.Dimension != Utils.Dimensions.Main || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked || player.Vehicle != null || business.Cows[cowIdx].Position.Position.DistanceTo(player.Position) > 10f)
            {
                if (business.Cows[cowIdx].CTS != null)
                {
                    business.Cows[cowIdx].CTS.Cancel();
                    business.Cows[cowIdx].CTS.Dispose();

                    business.Cows[cowIdx].CTS = null;
                }

                if (pData.GeneralAnim == Sync.Animations.GeneralTypes.MilkCow0)
                    pData.StopGeneralAnim();

                Game.Jobs.Farmer.ResetPlayerCurrentCowInfo(pData);

                return;
            }

            pData.Player.AttachObject(Game.Jobs.Farmer.MilkBucketModel, Sync.AttachSystem.Types.FarmMilkBucketCarry, -1, null);
        }

        [RemoteEvent("Job::FARM::COWF")]
        private static void CowFinish(Player player, int bucketIdx)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var job = pData.CurrentJob as Game.Jobs.Farmer;

            if (job == null)
                return;

            int cowIdx;

            if (!Game.Jobs.Farmer.TryGetPlayerCurrentCowInfo(pData, out cowIdx))
                return;

            var farmBusiness = job.FarmBusiness;

            if (Game.Businesses.Farm.CowData.GetGrowTime(farmBusiness, cowIdx) != 0)
                return;

            if (bucketIdx < 0 || bucketIdx >= farmBusiness.CowBucketPositions.Count)
                return;

            if (player.DetachObject(Sync.AttachSystem.Types.FarmMilkBucketCarry))
            {
                if (player.Dimension != Utils.Dimensions.Main || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked || player.Vehicle != null || farmBusiness.CowBucketPositions[bucketIdx].DistanceTo(player.Position) > 5f)
                    return;

                farmBusiness.Cows[cowIdx].UpdateGrowTime(farmBusiness, cowIdx, Utils.GetCurrentTime().GetUnixTimestamp() + Game.Jobs.Farmer.GetGrowTimeForCrop(Game.Businesses.Farm.CropField.Types.Cow), true);

                uint newMats, playerTotalSalary;
                ulong newBizBalance;

                if (farmBusiness.TryProceedPayment(pData, $"crop_{(int)Game.Businesses.Farm.CropField.Types.Cow}_1", Game.Jobs.Farmer.GetPlayerSalaryCoef(pData.Info), out newMats, out newBizBalance, out playerTotalSalary))// give salary
                    farmBusiness.ProceedPayment(pData, newMats, newBizBalance, playerTotalSalary);
            }
        }
    }
}
