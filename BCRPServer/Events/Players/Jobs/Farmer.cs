using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

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

            if (player.Dimension != Utils.Dimensions.Main || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return false;

            var job = pData.CurrentJob as Game.Jobs.Farmer;

            if (job == null)
                return false;

            if (job.Position.Position.DistanceTo(player.Position) > 10f)
                return false;

            job.SetPlayerNoJob(pData.Info);

            return true;
        }

        [RemoteProc("Job::FARM::CP")]
        private static byte CropProcess(Player player, int fieldIdx, byte row, byte col)
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

            var cropData = Game.Businesses.Farm.CropField.GetData(farmBusiness, fieldIdx, row, col);

            if (cropData == null)
                return 0;

            if (cropData.CTS != null)
                return 1;

            var cropPos = farmBusiness.CropFields[fieldIdx].GetCropPosition3D(row, col);

            if (cropPos == null)
                return 0;

            if (cropPos.DistanceTo(player.Position) > 5f)
                return 0;

            var growTimeT = Game.Businesses.Farm.CropField.CropData.GetGrowTime(farmBusiness, fieldIdx, row, col);

            cropData.CTS = new System.Threading.CancellationTokenSource();

            if (growTimeT is long growTime)
            {
                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        await System.Threading.Tasks.Task.Delay(10000, cropData.CTS.Token);

                        NAPI.Task.Run(() =>
                        {
                            if (pData.Player?.Exists == true)
                                pData.Player.DetachObject(Sync.AttachSystem.Types.FarmPlantSmallShovel);

                            if (cropPos.DistanceTo(player.Position) <= 5f)
                            {
                                Game.Businesses.Farm.CropField.CropData.SetGrowTime(farmBusiness, fieldIdx, row, col, null);
                            }
                            else
                            {

                            }
                        });
                    }
                    catch (Exception ex) { }
                });
            }
            else
            {
                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        await System.Threading.Tasks.Task.Delay(10000, cropData.CTS.Token);

                        NAPI.Task.Run(() =>
                        {
                            if (pData.Player?.Exists == true)
                                pData.Player.DetachObject(Sync.AttachSystem.Types.FarmPlantSmallShovel);

                            if (cropPos.DistanceTo(player.Position) <= 5f)
                            {
                                Game.Businesses.Farm.CropField.CropData.SetGrowTime(farmBusiness, fieldIdx, row, col, Utils.GetCurrentTime().GetUnixTimestamp() + 500);
                            }
                            else
                            {

                            }
                        });
                    }
                    catch (Exception ex) { }
                });
            }

            Game.Jobs.Farmer.SetPlayerCurrentCropInfo(pData, fieldIdx, row, col);

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

            var curJob = pData.CurrentJob as Game.Jobs.Farmer;

            if (curJob == null)
                return;

            pData.Player.DetachObject(Sync.AttachSystem.Types.FarmPlantSmallShovel);
        }
    }
}
