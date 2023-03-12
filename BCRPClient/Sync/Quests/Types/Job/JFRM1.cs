﻿using RAGE;
using System.Collections.Generic;

namespace BCRPClient.Sync.Quests.Types.Job
{
    internal class JFRM1 : Events.Script
    {
        public JFRM1()
        {
            new Quest.QuestData(Quest.QuestData.Types.JFRM1, "Сбор пшеницы", "Фермер", new Dictionary<byte, Quest.QuestData.StepData>()
            {
                {
                    0,

                    new Quest.QuestData.StepData("Собирайте урожай, проезжая на тракторе по точкам", 1)
                    {
                        StartAction = (pData, quest) =>
                        {
                            var qData = quest.CurrentData?.Split('&');

                            if (qData == null || qData.Length != 1)
                                return;

                            var job = pData.CurrentJob as Data.Jobs.Farmer;

                            if (job == null)
                                return;

                            var businessData = job.FarmBusiness;

                            if (businessData == null)
                                return;

                            var jobVehicleRId = ushort.Parse(qData[0]);

                            job.SetCurrentData("JVEH", RAGE.Elements.Entities.Vehicles.GetAtRemote(jobVehicleRId));

                            for (int i = 0; i < businessData.CropFields.Count; i++)
                            {
                                var fieldData = businessData.CropFields[i];

                                if (fieldData.Type != Data.Locations.Farm.CropField.Types.Wheat)
                                    continue;

                                var srcCs = fieldData.Colshape as Additional.Cuboid;

                                if (srcCs == null)
                                    continue;

                                var cs = new Additional.Cuboid(srcCs.Position, srcCs.Width, srcCs.Depth, srcCs.Height, srcCs.Heading, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                                {
                                    ApproveType = Additional.ExtraColshape.ApproveTypes.OnlyVehicleDriver,

                                    ActionType = Additional.ExtraColshape.ActionTypes.VehicleSpeedLimit,

                                    Data = Data.Locations.Farm.TRACTOR_MAX_SPEED_KM_H / 3.6f,
                                };

                                quest.SetActualData($"CS_VSL_{i}", cs);
                            }

                            businessData.UpdateTractorTakerData(quest);
                        },

                        EndAction = (pData, quest) =>
                        {
                            quest.ClearAllActualData();
                        }
                    }
                },
            });
        }
    }
}
