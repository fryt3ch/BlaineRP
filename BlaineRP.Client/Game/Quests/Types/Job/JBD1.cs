﻿using System.Collections.Generic;
using BlaineRP.Client.Game.Helpers;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.Jobs;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Quests
{
    [Script(int.MaxValue)]
    internal class JBD1
    {
        public JBD1()
        {
            new Quest.QuestData(QuestTypes.JBD1,
                "Маршрут - наше всё",
                "Автобусник",
                new Dictionary<byte, Quest.QuestData.StepData>()
                {
                    {
                        0, new Quest.QuestData.StepData("Выберите маршрут", 1)
                        {
                            StartAction = (pData, quest) =>
                            {
                            },
                        }
                    },
                    {
                        1, new Quest.QuestData.StepData("Едьте к началу маршрута", 1)
                        {
                            StartAction = (pData, quest) =>
                            {
                                string[] qData = quest.CurrentData?.Split('&');

                                if (qData == null || qData.Length != 1)
                                    return;

                                var job = pData.CurrentJob as BusDriver;

                                if (job == null)
                                    return;

                                var routeIdx = int.Parse(qData[0]);

                                var destPos = new Vector3(job.Routes[routeIdx].Positions[0].X, job.Routes[routeIdx].Positions[0].Y, job.Routes[routeIdx].Positions[0].Z - 1f);

                                var colshape = new Cylinder(destPos, 5f, 10f, true, new Colour(255, 0, 0, 125), Settings.App.Static.MainDimension, null)
                                {
                                    ApproveType = ApproveTypes.OnlyServerVehicleDriver,
                                    OnEnter = async (cancel) =>
                                    {
                                        Vehicle jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                                        if (jobVehicle == null || Player.LocalPlayer.Vehicle != jobVehicle)
                                        {
                                            Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                                            return;
                                        }

                                        byte res = await quest.CallProgressUpdateProc();
                                    },
                                };

                                var blip = new ExtraBlip(162, destPos, "", 0f, 2, 255, 0f, false, 0, 0f, Settings.App.Static.MainDimension);

                                blip.SetRoute(true);

                                quest.SetActualData("E_BP_0", blip);
                                quest.SetActualData("CS_0", colshape);

                                quest.SetActualData("E_MKR_0",
                                    new Marker(4,
                                        new Vector3(destPos.X, destPos.Y, destPos.Z + 4.5f),
                                        5f,
                                        Vector3.Zero,
                                        Vector3.Zero,
                                        new RGBA(255, 255, 255, 150),
                                        true,
                                        Settings.App.Static.MainDimension
                                    )
                                );
                            },
                            EndAction = (pData, quest) =>
                            {
                                quest.GetActualData<Scaleform>("Scaleform")?.Destroy();
                                quest.GetActualData<AsyncTask>("Task")?.Cancel();

                                quest.ClearAllActualData();
                            },
                        }
                    },
                    {
                        2, new Quest.QuestData.StepData("Едьте к следующей точке [{0}/{1}]", 1)
                        {
                            StartAction = (pData, quest) =>
                            {
                                string[] qData = quest.CurrentData?.Split('&');

                                if (qData == null || qData.Length != 1)
                                    return;

                                var job = pData.CurrentJob as BusDriver;

                                if (job == null)
                                    return;

                                var routeIdx = int.Parse(qData[0]);
                                int posIdx = quest.StepProgress + 1;

                                quest.CurrentStepData.MaxProgress = job.Routes[routeIdx].Positions.Count - 2;

                                var destPos = new Vector3(job.Routes[routeIdx].Positions[posIdx].X,
                                    job.Routes[routeIdx].Positions[posIdx].Y,
                                    job.Routes[routeIdx].Positions[posIdx].Z - 1f
                                );
                                var nextPos = new Vector3(job.Routes[routeIdx].Positions[posIdx + 1].X,
                                    job.Routes[routeIdx].Positions[posIdx + 1].Y,
                                    job.Routes[routeIdx].Positions[posIdx + 1].Z - 1f
                                );

                                var colshape = new Cylinder(destPos, 5f, 10f, true, new Colour(255, 0, 0, 125), Settings.App.Static.MainDimension, null)
                                {
                                    ApproveType = ApproveTypes.OnlyServerVehicleDriver,
                                    OnEnter = (cancel) =>
                                    {
                                        var waitTime = 10000;

                                        if (quest.GetActualData<ExtraColshape>("CS_0") is ExtraColshape cs)
                                            cs.IsVisible = false;

                                        if (quest.GetActualData<Checkpoint>("E_MKR_0") is Checkpoint checkpoint)
                                            checkpoint.Visible = false;

                                        Vehicle jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                                        if (jobVehicle == null || Player.LocalPlayer.Vehicle != jobVehicle)
                                        {
                                            Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                                            return;
                                        }

                                        var task = new AsyncTask(async () =>
                                            {
                                                if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                                {
                                                    Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                                                    return;
                                                }

                                                byte res = await quest.CallProgressUpdateProc();
                                            },
                                            waitTime,
                                            false,
                                            0
                                        );

                                        var scaleform = Scaleform.CreateCounter("job_busdriver_0",
                                            Locale.Get("SCALEFORM_JOB_BUSDRIVER_WAIT_HEADER"),
                                            Locale.Get("SCALEFORM_JOB_TRUCKER_WAIT_CONTENT"),
                                            waitTime / 1000,
                                            Scaleform.CounterSoundTypes.None
                                        );

                                        scaleform.OnRender += () =>
                                        {
                                            if (Player.LocalPlayer.Vehicle != jobVehicle || jobVehicle.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                                            {
                                                Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

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
                                        if (quest.GetActualData<ExtraColshape>("CS_0") is ExtraColshape cs)
                                            cs.IsVisible = true;

                                        if (quest.GetActualData<Checkpoint>("E_MKR_0") is Checkpoint checkpoint)
                                            checkpoint.Visible = true;

                                        quest.GetActualData<Scaleform>("Scaleform")?.Destroy();
                                        quest.GetActualData<AsyncTask>("Task")?.Cancel();

                                        quest.ResetActualData("Scaleform");
                                        quest.ResetActualData("Task");
                                    },
                                };

                                var blip = new ExtraBlip(162, destPos, "", 0f, 2, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                                blip.SetRoute(true);

                                quest.SetActualData("E_BP_0", blip);
                                quest.SetActualData("CS_0", colshape);

                                var p1 = new Vector3(destPos.X, destPos.Y, destPos.Z + 4.5f);
                                var p2 = new Vector3(nextPos.X, nextPos.Y, nextPos.Z + 4.5f);

                                quest.SetActualData("E_MKR_0", new Checkpoint(17, p1, 5f, p2, new RGBA(255, 255, 255, 150), true, Settings.App.Static.MainDimension));
                            },
                            EndAction = (pData, quest) =>
                            {
                                quest.GetActualData<Scaleform>("Scaleform")?.Destroy();
                                quest.GetActualData<AsyncTask>("Task")?.Cancel();

                                quest.ClearAllActualData();
                            },
                        }
                    },
                    {
                        3, new Quest.QuestData.StepData("Едьте к концу маршрута", 1)
                        {
                            StartAction = (pData, quest) =>
                            {
                                string[] qData = quest.CurrentData?.Split('&');

                                if (qData == null || qData.Length != 1)
                                    return;

                                var job = pData.CurrentJob as BusDriver;

                                if (job == null)
                                    return;

                                var routeIdx = int.Parse(qData[0]);

                                int destIdx = job.Routes[routeIdx].Positions.Count - 1;

                                var destPos = new Vector3(job.Routes[routeIdx].Positions[destIdx].X,
                                    job.Routes[routeIdx].Positions[destIdx].Y,
                                    job.Routes[routeIdx].Positions[destIdx].Z - 1f
                                );

                                var colshape = new Cylinder(destPos, 5f, 10f, true, new Colour(255, 0, 0, 125), Settings.App.Static.MainDimension, null)
                                {
                                    ApproveType = ApproveTypes.OnlyServerVehicleDriver,
                                    OnEnter = async (cancel) =>
                                    {
                                        Vehicle jobVehicle = job.GetCurrentData<Vehicle>("JVEH");

                                        if (jobVehicle == null || Player.LocalPlayer.Vehicle != jobVehicle)
                                        {
                                            Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                                            return;
                                        }

                                        byte res = await quest.CallProgressUpdateProc();
                                    },
                                };

                                var blip = new ExtraBlip(162, destPos, "", 0f, 2, 255, 0f, false, 0, 0f, Settings.App.Static.MainDimension);

                                blip.SetRoute(true);

                                quest.SetActualData("E_BP_0", blip);
                                quest.SetActualData("CS_0", colshape);

                                quest.SetActualData("E_MKR_0",
                                    new Marker(4,
                                        new Vector3(destPos.X, destPos.Y, destPos.Z + 4.5f),
                                        5f,
                                        Vector3.Zero,
                                        Vector3.Zero,
                                        new RGBA(255, 255, 255, 150),
                                        true,
                                        Settings.App.Static.MainDimension
                                    )
                                );
                            },
                            EndAction = (pData, quest) =>
                            {
                                quest.GetActualData<Scaleform>("Scaleform")?.Destroy();
                                quest.GetActualData<AsyncTask>("Task")?.Cancel();

                                quest.ClearAllActualData();
                            },
                        }
                    },
                }
            );
        }
    }
}