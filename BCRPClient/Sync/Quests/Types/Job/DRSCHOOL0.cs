using RAGE;
using RAGE.Elements;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Sync.Quests.Types.Job
{
    internal class DRSCHOOL0 : Events.Script
    {
        public DRSCHOOL0()
        {
            new Quest.QuestData(Quest.QuestData.Types.DRSCHOOL0, "Практическая часть", "Автошкола", new Dictionary<byte, Quest.QuestData.StepData>()
            {
                {
                    0,

                    new Quest.QuestData.StepData("Сядьте в транспорт для экзамена", 1)
                    {
                        StartAction = (pData, quest) =>
                        {
                            var qData = quest.CurrentData?.Split('&');

                            if (qData == null)
                                return;

                            var pPos = Player.LocalPlayer.Position;

                            var minIdx = -1;
                            var minDist = float.MaxValue;

                            Vector3 pos = null;

                            var licType = (Sync.Players.LicenseTypes)int.Parse(qData[0]);

                            for (int i = 0; i < Data.Locations.Autoschool.All.Count; i++)
                            {
                                var school = Data.Locations.Autoschool.All[i];

                                if (school.VehiclesPositions.TryGetValue(licType, out pos) && pPos.DistanceTo(pos) is float dist && dist <= minDist)
                                {
                                    minIdx = i;

                                    minDist = dist;
                                }
                            }

                            if (minIdx < 0)
                                return;

                            var blip = new Additional.ExtraBlip(162, pos, "Экзаменационный транспорт", 1f, 3, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                            quest.SetActualData("E_BP_M", blip);
                        },

                        EndAction = (pData, quest) =>
                        {
                            quest.ClearAllActualData();
                        }
                    }
                },

                {
                    1,

                    new Quest.QuestData.StepData("Направляйтесь к следующей точке [{0}/{1}]", 1)
                    {
                        StartAction = (pData, quest) =>
                        {
                            var qData = quest.CurrentData?.Split('&');

                            if (qData == null)
                                return;

                            var school = Data.Locations.Autoschool.Get(int.Parse(qData[1]));

                            var licType = (Sync.Players.LicenseTypes)int.Parse(qData[2]);

                            var veh = RAGE.Elements.Entities.Vehicles.GetAtRemote(ushort.Parse(qData[0]));

                            var posIdx = quest.StepProgress;

                            var routeData = school.PracticeRoutes.GetValueOrDefault(Data.Locations.Autoschool.GetLicenseTypeForPracticeRoute(licType));

                            if (routeData == null)
                                return;

                            quest.CurrentStepData.MaxProgress = routeData.Length;

                            var destPos = new Vector3(routeData[posIdx].X, routeData[posIdx].Y, routeData[posIdx].Z - 1f);
                            var nextPos = posIdx < routeData.Length - 1 ? new Vector3(routeData[posIdx + 1].X, routeData[posIdx + 1].Y, routeData[posIdx + 1].Z - 1f) : null;

                            var colshape = new Additional.Cylinder(destPos, 2.5f, 5f, true, new Utils.Colour(255, 0, 0, 125), Settings.MAIN_DIMENSION, null)
                            {
                                ApproveType = Additional.ExtraColshape.ApproveTypes.OnlyServerVehicleDriver,

                                OnEnter = async (cancel) =>
                                {
                                    if (quest.GetActualData<Additional.ExtraColshape>("CS_0") is Additional.ExtraColshape cs)
                                        cs.IsVisible = false;

                                    if (quest.GetActualData<Checkpoint>("E_MKR_0") is Checkpoint checkpoint)
                                        checkpoint.Visible = false;

                                    if (veh?.Exists != true || Player.LocalPlayer.Vehicle != veh)
                                    {
                                        CEF.Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                                        return;
                                    }

                                    var res = await quest.CallProgressUpdateProc();
                                },

                                OnExit = (cancel) =>
                                {
                                    if (quest.GetActualData<Additional.ExtraColshape>("CS_0") is Additional.ExtraColshape cs)
                                        cs.IsVisible = true;

                                    if (quest.GetActualData<Checkpoint>("E_MKR_0") is Checkpoint checkpoint)
                                        checkpoint.Visible = true;
                                }
                            };

                            var blip = new Additional.ExtraBlip(162, destPos, "", 0f, 2, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

                            blip.SetRoute(true);

                            quest.SetActualData("E_BP_0", blip);
                            quest.SetActualData("CS_0", colshape);

                            var p1 = new Vector3(destPos.X, destPos.Y, destPos.Z + 2.5f);

                            if (nextPos != null)
                            {
                                var p2 = new Vector3(nextPos.X, nextPos.Y, nextPos.Z + 2.5f);

                                quest.SetActualData("E_MKR_0", new Checkpoint(17, p1, 2.5f, p2, new RGBA(255, 255, 255, 150), true, Settings.MAIN_DIMENSION));
                            }

                            var faultCheckTask = new AsyncTask(async () =>
                            {
                                var faultCode = GetExamFaultCode(veh);

                                if (faultCode > 0)
                                {
                                    if (await quest.CallProgressUpdateProc(faultCode) == byte.MaxValue)
                                        CEF.Notification.Show($"DriveS::PEF{faultCode}");
                                }
                            }, 250, true, 0);

                            faultCheckTask.Run();

                            quest.SetActualData("FaultCheckTask", faultCheckTask);
                        },

                        EndAction = (pData, quest) =>
                        {
                            quest.GetActualData<AsyncTask>("FaultCheckTask")?.Cancel();

                            quest.ResetActualData("FaultCheckTask");

                            quest.ClearAllActualData();
                        }
                    }
                },

                {
                    2,

                    new Quest.QuestData.StepData("Припаркуйте транспорт на его место", 1)
                    {
                        StartAction = (pData, quest) =>
                        {
                            var qData = quest.CurrentData?.Split('&');

                            if (qData == null)
                                return;

                            var destPos = new Vector3(float.Parse(qData[1]), float.Parse(qData[2]), float.Parse(qData[3]) - 1f);

                            var veh = RAGE.Elements.Entities.Vehicles.GetAtRemote(ushort.Parse(qData[0]));

                            var colshape = new Additional.Cylinder(destPos, 2.5f, 5f, true, new Utils.Colour(255, 0, 0, 125), Settings.MAIN_DIMENSION, null)
                            {
                                ApproveType = Additional.ExtraColshape.ApproveTypes.OnlyServerVehicleDriver,

                                OnEnter = (cancel) =>
                                {
                                    if (veh?.Exists != true || Player.LocalPlayer.Vehicle != veh)
                                    {
                                        CEF.Notification.ShowError(Locale.Notifications.General.JobVehicleNotInVeh);

                                        return;
                                    }

                                    if (quest.GetActualData<Additional.ExtraColshape>("CS_0") is Additional.ExtraColshape cs)
                                        cs.IsVisible = false;

                                    if (quest.GetActualData<Marker>("E_MKR_0") is Marker checkpoint)
                                        checkpoint.Visible = false;

                                    CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), "Для того, чтобы закончить экзамен заглушите двигатель транспорта!");

                                    AsyncTask engineTask = null;

                                    engineTask = new AsyncTask(async () =>
                                    {
                                        if (veh?.Exists != true)
                                            return;

                                        if (Sync.Vehicles.GetData(veh)?.EngineOn == false)
                                        {
                                            engineTask?.Cancel();

                                            var res = await quest.CallProgressUpdateProc();
                                        }
                                    }, 100, true, 0);

                                    quest.SetActualData("EngineTask", engineTask);

                                    engineTask.Run();
                                },

                                OnExit = (cancel) =>
                                {
                                    if (quest.GetActualData<Additional.ExtraColshape>("CS_0") is Additional.ExtraColshape cs)
                                        cs.IsVisible = true;

                                    if (quest.GetActualData<Marker>("E_MKR_0") is Marker checkpoint)
                                        checkpoint.Visible = true;

                                    if (quest.GetActualData<AsyncTask>("EngineTask") is AsyncTask task)
                                    {
                                        task.Cancel();

                                        quest.ResetActualData("EngineTask");
                                    }
                                }
                            };

                            var blip = new Additional.ExtraBlip(162, destPos, "", 0f, 2, 255, 0f, false, 0, 0f, Settings.MAIN_DIMENSION);

                            blip.SetRoute(true);

                            quest.SetActualData("E_BP_0", blip);
                            quest.SetActualData("CS_0", colshape);

                            quest.SetActualData("E_MKR_0", new Marker(4, new Vector3(destPos.X, destPos.Y, destPos.Z + 2.5f), 2.5f, Vector3.Zero, Vector3.Zero, new RGBA(255, 255, 255, 150), true, Settings.MAIN_DIMENSION));

                            var faultCheckTask = new AsyncTask(async () =>
                            {
                                var faultCode = GetExamFaultCode(veh);

                                if (faultCode > 0)
                                {
                                    if (await quest.CallProgressUpdateProc(faultCode) == byte.MaxValue)
                                        CEF.Notification.Show($"DriveS::PEF{faultCode}");
                                }
                            }, 250, true, 0);

                            faultCheckTask.Run();

                            quest.SetActualData("FaultCheckTask", faultCheckTask);
                        },

                        EndAction = (pData, quest) =>
                        {
                            quest.GetActualData<AsyncTask>("FaultCheckTask")?.Cancel();
                            quest.GetActualData<AsyncTask>("EngineTask")?.Cancel();

                            quest.ResetActualData("EngineTask");

                            quest.ClearAllActualData();
                        }
                    }
                },
            });
        }

        private static int GetExamFaultCode(Vehicle veh)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return -1;

            var vData = Sync.Vehicles.GetData(veh);

            if (vData == null || veh?.Exists != true)
                return 1;

            if (Player.LocalPlayer.Vehicle != veh || veh.GetPedInSeat(-1, 0) != Player.LocalPlayer.Handle)
                return 2;

            if (vData.Data.Type == Data.Vehicles.Vehicle.Types.Car && vData.EngineOn && veh.GetSpeed() > 1f && !pData.BeltOn)
                return 3;

            if (veh.GetEngineHealth() <= 1f)
                return 4;

            if (vData.FuelLevel == 0f)
                return 5;

            return -1;
        }
    }
}