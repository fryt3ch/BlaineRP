﻿using System.Collections.Generic;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Game.EntitiesData.Vehicles;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.Misc;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Quests
{
    [Script(int.MaxValue)]
    internal class DRSCHOOL0
    {
        public DRSCHOOL0()
        {
            new Quest.QuestData(QuestTypes.DRSCHOOL0,
                "Практическая часть",
                "Автошкола",
                new Dictionary<byte, Quest.QuestData.StepData>()
                {
                    {
                        0, new Quest.QuestData.StepData("Сядьте в транспорт для экзамена", 1)
                        {
                            StartAction = (pData, quest) =>
                            {
                                string[] qData = quest.CurrentData?.Split('&');

                                if (qData == null)
                                    return;

                                Vector3 pPos = Player.LocalPlayer.Position;

                                int minIdx = -1;
                                float minDist = float.MaxValue;

                                Vector3 pos = null;

                                var licType = (LicenseTypes)int.Parse(qData[0]);

                                for (var i = 0; i < Autoschool.All.Count; i++)
                                {
                                    Autoschool school = Autoschool.All[i];

                                    if (school.VehiclesPositions.TryGetValue(licType, out pos) && pPos.DistanceTo(pos) is float dist && dist <= minDist)
                                    {
                                        minIdx = i;

                                        minDist = dist;
                                    }
                                }

                                if (minIdx < 0)
                                    return;

                                var blip = new ExtraBlip(162, pos, "Экзаменационный транспорт", 1f, 3, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                                quest.SetActualData("E_BP_M", blip);
                            },
                            EndAction = (pData, quest) =>
                            {
                                quest.ClearAllActualData();
                            },
                        }
                    },
                    {
                        1, new Quest.QuestData.StepData("Направляйтесь к следующей точке [{0}/{1}]", 1)
                        {
                            StartAction = (pData, quest) =>
                            {
                                string[] qData = quest.CurrentData?.Split('&');

                                if (qData == null)
                                    return;

                                var school = Autoschool.Get(int.Parse(qData[1]));

                                var licType = (LicenseTypes)int.Parse(qData[2]);

                                Vehicle veh = Entities.Vehicles.GetAtRemote(ushort.Parse(qData[0]));

                                int posIdx = quest.StepProgress;

                                Vector3[] routeData = school.PracticeRoutes.GetValueOrDefault(Autoschool.GetLicenseTypeForPracticeRoute(licType));

                                if (routeData == null)
                                    return;

                                quest.CurrentStepData.MaxProgress = routeData.Length;

                                var destPos = new Vector3(routeData[posIdx].X, routeData[posIdx].Y, routeData[posIdx].Z - 1f);
                                Vector3 nextPos = posIdx < routeData.Length - 1
                                    ? new Vector3(routeData[posIdx + 1].X, routeData[posIdx + 1].Y, routeData[posIdx + 1].Z - 1f)
                                    : null;

                                var colshape = new Cylinder(destPos, 2.5f, 5f, true, new Colour(255, 0, 0, 125), Settings.App.Static.MainDimension, null)
                                {
                                    ApproveType = ApproveTypes.OnlyServerVehicleDriver,
                                    OnEnter = async (cancel) =>
                                    {
                                        if (quest.GetActualData<ExtraColshape>("CS_0") is ExtraColshape cs)
                                            cs.IsVisible = false;

                                        if (quest.GetActualData<Checkpoint>("E_MKR_0") is Checkpoint checkpoint)
                                            checkpoint.Visible = false;

                                        if (veh?.Exists != true || Player.LocalPlayer.Vehicle != veh)
                                        {
                                            Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                                            return;
                                        }

                                        byte res = await quest.CallProgressUpdateProc();
                                    },
                                    OnExit = (cancel) =>
                                    {
                                        if (quest.GetActualData<ExtraColshape>("CS_0") is ExtraColshape cs)
                                            cs.IsVisible = true;

                                        if (quest.GetActualData<Checkpoint>("E_MKR_0") is Checkpoint checkpoint)
                                            checkpoint.Visible = true;
                                    },
                                };

                                var blip = new ExtraBlip(162, destPos, "", 0f, 2, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                                blip.SetRoute(true);

                                quest.SetActualData("E_BP_0", blip);
                                quest.SetActualData("CS_0", colshape);

                                var p1 = new Vector3(destPos.X, destPos.Y, destPos.Z + 2.5f);

                                if (nextPos != null)
                                {
                                    var p2 = new Vector3(nextPos.X, nextPos.Y, nextPos.Z + 2.5f);

                                    quest.SetActualData("E_MKR_0", new Checkpoint(17, p1, 2.5f, p2, new RGBA(255, 255, 255, 150), true, Settings.App.Static.MainDimension));
                                }

                                var faultCheckTask = new AsyncTask(async () =>
                                    {
                                        int faultCode = GetExamFaultCode(veh);

                                        if (faultCode > 0)
                                            if (await quest.CallProgressUpdateProc(faultCode) == byte.MaxValue)
                                                Notification.Show($"DriveS::PEF{faultCode}");
                                    },
                                    250,
                                    true,
                                    0
                                );

                                faultCheckTask.Run();

                                quest.SetActualData("FaultCheckTask", faultCheckTask);
                            },
                            EndAction = (pData, quest) =>
                            {
                                quest.GetActualData<AsyncTask>("FaultCheckTask")?.Cancel();

                                quest.ResetActualData("FaultCheckTask");

                                quest.ClearAllActualData();
                            },
                        }
                    },
                    {
                        2, new Quest.QuestData.StepData("Припаркуйте транспорт на его место", 1)
                        {
                            StartAction = (pData, quest) =>
                            {
                                string[] qData = quest.CurrentData?.Split('&');

                                if (qData == null)
                                    return;

                                var destPos = new Vector3(float.Parse(qData[1]), float.Parse(qData[2]), float.Parse(qData[3]) - 1f);

                                Vehicle veh = Entities.Vehicles.GetAtRemote(ushort.Parse(qData[0]));

                                var colshape = new Cylinder(destPos, 2.5f, 5f, true, new Colour(255, 0, 0, 125), Settings.App.Static.MainDimension, null)
                                {
                                    ApproveType = ApproveTypes.OnlyServerVehicleDriver,
                                    OnEnter = (cancel) =>
                                    {
                                        if (veh?.Exists != true || Player.LocalPlayer.Vehicle != veh)
                                        {
                                            Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                                            return;
                                        }

                                        if (quest.GetActualData<ExtraColshape>("CS_0") is ExtraColshape cs)
                                            cs.IsVisible = false;

                                        if (quest.GetActualData<Marker>("E_MKR_0") is Marker checkpoint)
                                            checkpoint.Visible = false;

                                        Notification.Show(Notification.Types.Information,
                                            Locale.Get("NOTIFICATION_HEADER_DEF"),
                                            "Для того, чтобы закончить экзамен заглушите двигатель транспорта!"
                                        );

                                        AsyncTask engineTask = null;

                                        engineTask = new AsyncTask(async () =>
                                            {
                                                if (veh?.Exists != true)
                                                    return;

                                                if (VehicleData.GetData(veh)?.EngineOn == false)
                                                {
                                                    engineTask?.Cancel();

                                                    byte res = await quest.CallProgressUpdateProc();
                                                }
                                            },
                                            100,
                                            true,
                                            0
                                        );

                                        quest.SetActualData("EngineTask", engineTask);

                                        engineTask.Run();
                                    },
                                    OnExit = (cancel) =>
                                    {
                                        if (quest.GetActualData<ExtraColshape>("CS_0") is ExtraColshape cs)
                                            cs.IsVisible = true;

                                        if (quest.GetActualData<Marker>("E_MKR_0") is Marker checkpoint)
                                            checkpoint.Visible = true;

                                        if (quest.GetActualData<AsyncTask>("EngineTask") is AsyncTask task)
                                        {
                                            task.Cancel();

                                            quest.ResetActualData("EngineTask");
                                        }
                                    },
                                };

                                var blip = new ExtraBlip(162, destPos, "", 0f, 2, 255, 0f, false, 0, 0f, Settings.App.Static.MainDimension);

                                blip.SetRoute(true);

                                quest.SetActualData("E_BP_0", blip);
                                quest.SetActualData("CS_0", colshape);

                                quest.SetActualData("E_MKR_0",
                                    new Marker(4,
                                        new Vector3(destPos.X, destPos.Y, destPos.Z + 2.5f),
                                        2.5f,
                                        Vector3.Zero,
                                        Vector3.Zero,
                                        new RGBA(255, 255, 255, 150),
                                        true,
                                        Settings.App.Static.MainDimension
                                    )
                                );

                                var faultCheckTask = new AsyncTask(async () =>
                                    {
                                        int faultCode = GetExamFaultCode(veh);

                                        if (faultCode > 0)
                                            if (await quest.CallProgressUpdateProc(faultCode) == byte.MaxValue)
                                                Notification.Show($"DriveS::PEF{faultCode}");
                                    },
                                    250,
                                    true,
                                    0
                                );

                                faultCheckTask.Run();

                                quest.SetActualData("FaultCheckTask", faultCheckTask);
                            },
                            EndAction = (pData, quest) =>
                            {
                                quest.GetActualData<AsyncTask>("FaultCheckTask")?.Cancel();
                                quest.GetActualData<AsyncTask>("EngineTask")?.Cancel();

                                quest.ResetActualData("EngineTask");

                                quest.ClearAllActualData();
                            },
                        }
                    },
                }
            );
        }

        private static int GetExamFaultCode(Vehicle veh)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return -1;

            var vData = VehicleData.GetData(veh);

            if (vData == null || veh?.Exists != true)
                return 1;

            if (Player.LocalPlayer.Vehicle != veh || veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                return 2;

            if (vData.Data.Type == Data.Vehicles.VehicleTypes.Car && vData.EngineOn && veh.GetSpeed() > 1f && !pData.BeltOn)
                return 3;

            if (veh.GetEngineHealth() <= 1f)
                return 4;

            if (vData.FuelLevel == 0f)
                return 5;

            return -1;
        }
    }
}