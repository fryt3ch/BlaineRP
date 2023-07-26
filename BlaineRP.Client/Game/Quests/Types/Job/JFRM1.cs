using System.Collections.Generic;
using BlaineRP.Client.Game.Businesses;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.Jobs;

namespace BlaineRP.Client.Game.Quests
{
    [Script(int.MaxValue)]
    internal class JFRM1
    {
        public JFRM1()
        {
            new Quest.QuestData(QuestTypes.JFRM1, "Сбор пшеницы", "Фермер", new Dictionary<byte, Quest.QuestData.StepData>()
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

                            var job = pData.CurrentJob as Farmer;

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

                                if (fieldData.Type != Farm.CropField.Types.Wheat)
                                    continue;

                                var srcCs = fieldData.Colshape as Cuboid;

                                if (srcCs == null)
                                    continue;

                                var cs = new Cuboid(srcCs.Position, srcCs.Width, srcCs.Depth, srcCs.Height, srcCs.Heading, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                                {
                                    ApproveType = ApproveTypes.OnlyVehicleDriver,

                                    ActionType = ActionTypes.VehicleSpeedLimit,

                                    Data = Farm.TRACTOR_MAX_SPEED_KM_H / 3.6f,
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
