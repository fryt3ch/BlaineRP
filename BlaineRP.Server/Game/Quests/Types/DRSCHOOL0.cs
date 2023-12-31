﻿using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using BlaineRP.Server.Game.Misc;

namespace BlaineRP.Server.Game.Quests.Types
{
    public class DRSCHOOL0
    {
        public static void Initialize()
        {
            new Quest.QuestData(QuestType.DRSCHOOL0)
            {
                ProgressUpdateFunc = (pData, questData, data) =>
                {
                    if (questData.Step == 0)
                        return 0;

                    var questCurrentData = questData.CurrentData.Split('&');

                    if (data.Length == 1)
                    {
                        int faultIdx;

                        if (int.TryParse(data[0], out faultIdx))
                        {
                            var vehicleRemoteId = ushort.Parse(questCurrentData[0]);

                            var vDataT = VehicleData.All.Values.Where(x => x.Vehicle.Id == vehicleRemoteId).FirstOrDefault();

                            questData.Cancel(pData.Info, false);

                            vDataT?.Delete(false);

                            return byte.MaxValue;
                        }
                    }

                    var vData = pData.Player.Vehicle.GetMainData();

                    if (vData == null || pData.VehicleSeat != 0 || vData.OwnerID != pData.CID || vData.OwnerType != OwnerTypes.PlayerDrivingSchool)
                        return 0;

                    if (questData.Step == 1)
                    {
                        var school = DrivingSchool.Get(int.Parse(questCurrentData[1]));

                        var licType = (LicenseType)int.Parse(questCurrentData[2]);

                        var rLicType = DrivingSchool.GetLicenseTypeForPracticeRoute(licType);

                        var routeData = school.PracticeRoutes.GetValueOrDefault(rLicType);

                        if (routeData == null)
                            return 0;

                        if (pData.Player.Position.DistanceTo(routeData[questData.StepProgress]) > 10f)
                            return 0;

                        var nextProgress = questData.StepProgress + 1;

                        if (nextProgress >= routeData.Length)
                        {
                            questData.UpdateStep(pData.Info, 2, 0, $"{vData.Vehicle.Id}&{vData.LastData.Position.X}&{vData.LastData.Position.Y}&{vData.LastData.Position.Z}&{(int)licType}");
                        }
                        else
                        {
                            questData.UpdateStepKeepOldData(pData.Info, 1, nextProgress);
                        }
                    }
                    else if (questData.Step == 2)
                    {
                        if (pData.Player.Position.DistanceTo(vData.LastData.Position) > 10f)
                            return 0;

                        var licType = (LicenseType)int.Parse(questCurrentData[4]);

                        questData.Cancel(pData.Info, true);

                        pData.Info.AddLicense(licType);

                        pData.Player.Notify("DriveS::PES", licType.ToString());

                        vData.Delete(false);
                    }

                    return 0;
                }
            };
        }
    }
}