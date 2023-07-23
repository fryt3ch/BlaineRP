using RAGE;
using System.Collections.Generic;

namespace BlaineRP.Client.Sync.Quests.Types.Main
{
    [Script(int.MaxValue)]
    internal class TQ1
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
                            var mBlip = new Additional.ExtraBlip(304, new Vector3(0f, 0f, 0f), "asdas", 1f, 5, 255, 0, false, 0, 0, Settings.App.Static.MainDimension);

                            quest.SetActualData("E_BP_M", mBlip);
                        },

                        EndAction = null,
                    }
                }
            });
        }
    }
}
