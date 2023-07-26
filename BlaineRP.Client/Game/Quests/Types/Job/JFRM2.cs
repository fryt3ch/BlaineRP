using System.Collections.Generic;
using BlaineRP.Client.Game.Jobs;
using BlaineRP.Client.Game.World;
using BlaineRP.Client.Utils;

namespace BlaineRP.Client.Game.Quests
{
    [Script(int.MaxValue)]
    internal class JFRM2
    {
        private const int PLANE_IRRIGATION_WATER_FX_TIME = 7_500;

        public JFRM2()
        {
            new Quest.QuestData(QuestTypes.JFRM2, "Орошение полей", "Фермер", new Dictionary<byte, Quest.QuestData.StepData>()
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

                            var job = pData.CurrentJob as Farmer;

                            if (job == null)
                                return;

                            var businessData = job.FarmBusiness;

                            if (businessData == null)
                                return;

                            var jobVehicleRId = ushort.Parse(qData[0]);

                            job.SetCurrentData("JVEH", RAGE.Elements.Entities.Vehicles.GetAtRemote(jobVehicleRId));

                            businessData.UpdatePlaneIrrigatorData(quest);

                            quest.SetActualData("FARMJOBTEMPFX::PW", new List<int>());

                            var task = new AsyncTask(() =>
                            {
                                var effects = quest.GetActualData<List<int>>("FARMJOBTEMPFX::PW");

                                if (effects == null || effects.Count == 0)
                                    return;

                                if (Core.ServerTimestampMilliseconds - quest.GetActualData<long>("FARMJOBTEMPFXT::PW") >= PLANE_IRRIGATION_WATER_FX_TIME)
                                {
                                    for (int i = 0; i < effects.Count; i++)
                                    {
                                        RAGE.Game.Graphics.RemoveParticleFx(effects[i], false);
                                    }

                                    effects.Clear();
                                }
                            }, 1000, true, 0);

                            quest.SetActualData("FxTask", task);

                            task.Run();
                        },

                        EndAction = (pData, quest) =>
                        {
                            quest.GetActualData<AsyncTask>("FxTask")?.Cancel();

                            var tempFxList = quest.GetActualData<List<int>>("FARMJOBTEMPFX::PW");

                            if (tempFxList != null)
                            {
                                for (int i = 0; i < tempFxList.Count; i++)
                                {
                                    RAGE.Game.Graphics.RemoveParticleFx(tempFxList[i], false);
                                }
                            }

                            quest.ClearAllActualData();
                        }
                    }
                },
            });
        }
    }
}