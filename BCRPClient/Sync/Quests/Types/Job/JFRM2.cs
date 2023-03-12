using RAGE;
using System.Collections.Generic;

namespace BCRPClient.Sync.Quests.Types.Job
{
    internal class JFRM2 : Events.Script
    {
        public JFRM2()
        {
            new Quest.QuestData(Quest.QuestData.Types.JFRM2, "Орошение полей", "Фермер", new Dictionary<byte, Quest.QuestData.StepData>()
            {
                {
                    0,

                    new Quest.QuestData.StepData("Орошайте поля, пролетая по точкам", 1)
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