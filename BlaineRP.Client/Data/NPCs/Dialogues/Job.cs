using BlaineRP.Client.Extensions.System;
using RAGE;
using RAGE.Elements;
using System.Linq;
using static BlaineRP.Client.Data.Dialogue;

namespace BlaineRP.Client.Data.NPCs.Dialogues
{
    [Script(int.MaxValue)]
    public class Job
    {
        public Job()
        {
            new Dialogue("job_trucker_g_0", "О, привет, я как раз ищу кого то, кто справится с доставкой грузов. Звучит просто, правда?", null,
                new Dialogue.Button("Да, всегда хотел", () => NPC.CurrentNPC?.ShowDialogue("job_trucker_g_1")),
                //new Button("Ты давно тут всем заправляешь?", () => NPC.CurrentNPC?.ShowDialogue("job_trucker_g_2"), false),
                Dialogue.Button.DefaultExitButton
            );

            new Dialogue("job_trucker_g_1", "Значит смотри, берешь в аренду любой из грузовиков, стоящих неподалеку, выбираешь в системе заинстересовавший тебя заказ и едешь за самим грузом, а дальше - доставляешь его заказчику", null,
                new Dialogue.Button("Так, а что по оплате?", () => NPC.CurrentNPC?.ShowDialogue("job_debitcard_g_0")),
                Dialogue.Button.DefaultExitButton);

            /*            new Dialogue("job_trucker_g_2", "Да сколько себя помню, у меня и брат владеет частью нашего семейного бизнеса, но в Лос-Сантосе. Давно его не видел, мне нужно передать ему одну вещь. Можешь передать ему ее, если будешь там? Он тебя хорошо отблагодарит.", null,
                            Button.DefaultBackButton,
                            Button.DefaultExitButton);*/

            new Dialogue("job_debitcard_g_0", "С этим у нас все официально, поэтому никаких конвертов. Если у тебя нет карты, получи ее в любом банковском отделении, базовый тариф стоит копейки. Без нее работать у нас не сможешь. А так, начислим тебе все, что ты заработал, даже если тебя вдруг не будет в округе, кхе-кхе", null,
                Dialogue.Button.DefaultBackButton,
                Dialogue.Button.DefaultExitButton
            );

            new Dialogue("job_farm_pl_0", null, async (args) =>
            {
                if (NPC.CurrentNPC == null)
                    return;

                var pData = Sync.Players.GetData(Player.LocalPlayer);

                var farmJobData = (Data.Jobs.Farmer)Data.Jobs.Job.AllJobs.Values.Where(x => x is Data.Jobs.Farmer farm && farm.FarmBusiness == NPC.CurrentNPC.Data).FirstOrDefault();

                if (pData.CurrentJob == farmJobData)
                {
                    var salary = Utils.Convert.ToDecimal(await Events.CallRemoteProc("Job::GTCSI"));

                    if (salary <= 0)
                    {
                        AllDialogues["job_farm_aj_0"].Text = "Эй, работник, я вижу, что ты вообще ничего не сделал, можешь, конечно закончить рабочий день, но денег не получишь";
                    }
                    else
                    {
                        AllDialogues["job_farm_aj_0"].Text = $"Хочешь закончить рабочий день и получить зарплату? Если что, ты заработал {Locale.Get("GEN_MONEY_0", salary)}";
                    }

                    NPC.CurrentNPC.ShowDialogue("job_farm_aj_0");
                }
                else
                {
                    var margin = Utils.Convert.ToSingle(await Events.CallRemoteProc("Business::GMI", farmJobData.FarmBusiness.Id));

                    if (NPC.CurrentNPC == null || margin < 0f)
                        return;

                    NPC.CurrentNPC.ShowDialogue("job_farm_g_0", true, null, margin * 100 - 100);
                }
            });

            new Dialogue("job_farm_g_0", "Приветствую на нашей ферме! Хочешь поработать у нас? Платим неплохо, владельцу фермы достается {0}% от твоего заработка", null,
                new Dialogue.Button("Ага, давай попробуем", async () =>
                {
                    if (NPC.CurrentNPC == null)
                        return;

                    var farmJobData = (Data.Jobs.Farmer)Data.Jobs.Job.AllJobs.Values.Where(x => x is Data.Jobs.Farmer farm && farm.FarmBusiness == NPC.CurrentNPC.Data).FirstOrDefault();

                    if (NPC.LastSent.IsSpam(500, false, false))
                        return;

                    if ((bool)await Events.CallRemoteProc("Job::FARM::TJ", farmJobData.Id))
                    {
                        NPC.CurrentNPC?.SwitchDialogue(false);
                    }
                }),

                Dialogue.Button.DefaultExitButton
            );

            new Dialogue("job_farm_aj_0", "", null,
                new Dialogue.Button("[Завершить работу]", async () =>
                {
                    if (NPC.CurrentNPC == null)
                        return;

                    var farmJobData = (Data.Jobs.Farmer)Data.Jobs.Job.AllJobs.Values.Where(x => x is Data.Jobs.Farmer farm && farm.FarmBusiness == NPC.CurrentNPC.Data).FirstOrDefault();

                    if (NPC.LastSent.IsSpam(500, false, false))
                        return;

                    if ((bool)await Events.CallRemoteProc("Job::FARM::FJ"))
                    {
                        NPC.CurrentNPC?.SwitchDialogue(false);
                    }
                }),
                Dialogue.Button.DefaultExitButton
            );

            new Dialogue("job_farm_d_aero", "Чтобы труд работников нашей фермы был еще более продуктивным, необходимо с воздуха обрабатывать почву специальным средством. Можешь попробовать, но для этого тебе нужна лицензия категории Fly (лётная). Если разобьешь самолет, за нас не беспокойся - страховка все покроет, однако тогда плакала твоя лицензия!", null,
                new Dialogue.Button("Ага, давай попробуем", () => { }),
                Dialogue.Button.DefaultExitButton
            );
        }
    }
}
