using RAGE;
using RAGE.Elements;
using System.Collections.Generic;

namespace BCRPClient.Sync.Quests.Types.Main
{
    internal class TQ1 : Events.Script
    {
        public TQ1()
        {
            new Quest.QuestData(Quest.QuestData.Types.TQ1, "Пиздилово", "Snow Brawl", new Dictionary<byte, Quest.QuestData.StepData>()
            {
                {
                    0,

                    new Quest.QuestData.StepData("Попадите снежком в {1-0} игроков", 10)
                    {
                        StartAction = (pData, quest) =>
                        {
                            var mBlip = new Blip(304, new Vector3(0f, 0f, 0f), "asdas", 1f, 5, 255, 0, false, 0, 0, Settings.MAIN_DIMENSION);

                            quest.SetActualData("E_BP_M", mBlip);
                        },

                        EndAction = null,
                    }
                }
            });
        }
    }
}
