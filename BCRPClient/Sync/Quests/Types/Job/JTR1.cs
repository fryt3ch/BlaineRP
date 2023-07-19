using RAGE;
using RAGE.Elements;
using System.Collections.Generic;

namespace BCRPClient.Sync.Quests.Types.Job
{
    [Script(int.MaxValue)]
    internal class JTR1 
    {
        public JTR1()
        {
            new Quest.QuestData(Quest.QuestData.Types.JTR1, "Доставка груза", "Дальнобойщик", new Dictionary<byte, Quest.QuestData.StepData>()
            {
                {
                    0,

                    new Quest.QuestData.StepData("Возьмите новый заказ", 1)
                    {
                        StartAction = (pData, quest) =>
                        {

                        }
                    }
                },

                {
                    1,

                    new Quest.QuestData.StepData("Заберите груз со склада", 1)
                    {
                        StartAction = (pData, quest) =>
                        {
                            var qData = quest.CurrentData?.Split('&');

                            if (qData == null || qData.Length != 4)
                                return;

                            var job = pData.CurrentJob as Data.Jobs.Trucker;

                            if (job == null)
                                return;

                            var currentOrder = new Data.Jobs.Trucker.OrderInfo() { Id = uint.Parse(qData[0]), MPIdx = int.Parse(qData[1]), TargetBusiness = BCRPClient.Data.Locations.Business.All[int.Parse(qData[2])], Reward = uint.Parse(qData[3]) };

                            var destPos = new Vector3(job.MaterialsPositions[currentOrder.MPIdx].X, job.MaterialsPositions[currentOrder.MPIdx].Y, job.MaterialsPositions[currentOrder.MPIdx].Z - 1f);

                            var colshape = new Additional.Cylinder(destPos, 10f, 10f, true, new Utils.Colour(255, 0, 0, 125), Settings.App.Static.MainDimension, null)
                            {
                                ApproveType = Additional.ExtraColshape.ApproveTypes.OnlyServerVehicleDriver,

                                OnEnter = (cancel) =>
                                {
                                    var waitTime = 5000;

                                    if (quest.GetActualData<Additional.ExtraColshape>("CS_0") is Additional.ExtraColshape cs)
                                        cs.IsVisible = false;

                                    var jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                                    if (jobVehicle == null || Player.LocalPlayer.Vehicle != jobVehicle)
                                    {
                                        CEF.Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                                        return;
                                    }

                                    var task = new AsyncTask(async () =>
                                    {
                                        if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                        {
                                            CEF.Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                                            return;
                                        }

                                        var res = await quest.CallProgressUpdateProc();
                                    }, waitTime, false, 0);

                                    var scaleform = Additional.Scaleform.CreateCounter("job_trucker_0", Locale.Get("SCALEFORM_JOB_TRUCKER_WAIT_0_HEADER"), Locale.Get("SCALEFORM_JOB_TRUCKER_WAIT_CONTENT"), waitTime / 1000, Additional.Scaleform.CounterSoundTypes.None);

                                    scaleform.OnRender += () =>
                                    {
                                        if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                        {
                                            CEF.Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                                            scaleform.Destroy();
                                            task.Cancel();

                                            return;
                                        }
                                    };

                                    quest.SetActualData("Scaleform", scaleform);
                                    quest.SetActualData("Task", task);

                                    task.Run();
                                },

                                OnExit = (cancel) =>
                                {
                                    if (quest.GetActualData<Additional.ExtraColshape>("CS_0") is Additional.ExtraColshape cs)
                                        cs.IsVisible = true;

                                    quest.GetActualData<Additional.Scaleform>("Scaleform")?.Destroy();
                                    quest.GetActualData<AsyncTask>("Task")?.Cancel();

                                    quest.ResetActualData("Scaleform");
                                    quest.ResetActualData("Task");
                                }
                            };

                            var blip = new Additional.ExtraBlip(478, destPos, Locale.General.Blip.JobTruckerPointAText, 1f, 3, 255, 0f, false, 0, 0f, Settings.App.Static.MainDimension);

                            blip.SetRoute(true);

                            quest.SetActualData("E_BP_M", blip);
                            quest.SetActualData("CS_0", colshape);

                            quest.SetActualData("E_TXL_0", new Additional.ExtraLabel(new Vector3(destPos.X, destPos.Y, destPos.Z + 2f), Locale.General.Blip.JobTruckerPointAText, new RGBA(255, 255, 255, 255), 25f, 0, true, Settings.App.Static.MainDimension) { Font = 4, LOS = false });
                        },

                        EndAction = (pData, quest) =>
                        {
                            quest.GetActualData<Additional.Scaleform>("Scaleform")?.Destroy();
                            quest.GetActualData<AsyncTask>("Task")?.Cancel();

                            quest.ClearAllActualData();
                        }
                    }
                },

                {
                    2,

                    new Quest.QuestData.StepData("Доставьте груз получателю", 1)
                    {
                        StartAction = (pData, quest) =>
                        {
                            var qData = quest.CurrentData?.Split('&');

                            if (qData == null || qData.Length != 4)
                                return;

                            var job = pData.CurrentJob as Data.Jobs.Trucker;

                            if (job == null)
                                return;

                            var currentOrder = new Data.Jobs.Trucker.OrderInfo() { Id = uint.Parse(qData[0]), MPIdx = int.Parse(qData[1]), TargetBusiness = BCRPClient.Data.Locations.Business.All[int.Parse(qData[2])], Reward = uint.Parse(qData[3]) };

                            var destPos = new Vector3(currentOrder.TargetBusiness.InfoColshape.Position.X, currentOrder.TargetBusiness.InfoColshape.Position.Y, currentOrder.TargetBusiness.InfoColshape.Position.Z);

                            var colshape = new Additional.Cylinder(destPos, 10f, 10f, true, new Utils.Colour(255, 0, 0, 125), Settings.App.Static.MainDimension, null)
                            {
                                ApproveType = Additional.ExtraColshape.ApproveTypes.OnlyServerVehicleDriver,

                                OnEnter = (cancel) =>
                                {
                                    var waitTime = 5000;

                                    if (quest.GetActualData<Additional.ExtraColshape>("CS_0") is Additional.ExtraColshape cs)
                                        cs.IsVisible = false;

                                    var jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                                    if (jobVehicle == null || Player.LocalPlayer.Vehicle != jobVehicle)
                                    {
                                        CEF.Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                                        return;
                                    }

                                    var task = new AsyncTask(async () =>
                                    {
                                        if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                        {
                                            CEF.Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                                            return;
                                        }

                                        var res = await quest.CallProgressUpdateProc();
                                    }, waitTime, false, 0);

                                    var scaleform = Additional.Scaleform.CreateCounter("job_trucker_0", Locale.Get("SCALEFORM_JOB_TRUCKER_WAIT_1_HEADER"), Locale.Get("SCALEFORM_JOB_TRUCKER_WAIT_CONTENT"), waitTime / 1000, Additional.Scaleform.CounterSoundTypes.None);

                                    scaleform.OnRender += () =>
                                    {
                                        if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                        {
                                            CEF.Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                                            scaleform.Destroy();
                                            task.Cancel();

                                            return;
                                        }
                                    };

                                    quest.SetActualData("Scaleform", scaleform);
                                    quest.SetActualData("Task", task);

                                    task.Run();
                                },

                                OnExit = (cancel) =>
                                {
                                    if (quest.GetActualData<Additional.ExtraColshape>("CS_0") is Additional.ExtraColshape cs)
                                        cs.IsVisible = true;

                                    quest.GetActualData<Additional.Scaleform>("Scaleform")?.Destroy();
                                    quest.GetActualData<AsyncTask>("Task")?.Cancel();

                                    quest.ResetActualData("Scaleform");
                                    quest.ResetActualData("Task");
                                }
                            };

                            var blip = new Additional.ExtraBlip(478, destPos, "", 0f, 3, 255, 0f, true, 0, 10f, Settings.App.Static.MainDimension);

                            blip.SetRoute(true);

                            quest.SetActualData("E_BP_M", blip);
                            quest.SetActualData("CS_0", colshape);
                        },

                        EndAction = (pData, quest) =>
                        {
                            quest.GetActualData<Additional.Scaleform>("Scaleform")?.Destroy();
                            quest.GetActualData<AsyncTask>("Task")?.Cancel();

                            quest.ClearAllActualData();
                        }
                    }
                }
            });
        }
    }
}
