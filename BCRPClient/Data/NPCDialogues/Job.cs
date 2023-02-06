using System;
using System.Collections.Generic;
using System.Text;
using static BCRPClient.Data.Dialogue;

namespace BCRPClient.Data.NPCDialogues
{
    public class Job
    {
        public static void Load()
        {
            new Dialogue("job_trucker_g_0", "О, привет, я как раз ищу кого то, кто справится с доставкой грузов. Звучит просто, правда?", null,
                new Button("Да, всегда хотел", () => NPC.CurrentNPC?.ShowDialogue("job_trucker_g_1")),
                //new Button("Ты давно тут всем заправляешь?", () => NPC.CurrentNPC?.ShowDialogue("job_trucker_g_2"), false),
                Button.DefaultExitButton);

            new Dialogue("job_trucker_g_1", "Значит смотри, берешь в аренду любой из грузовиков, стоящих неподалеку, выбираешь в системе заинстересовавший тебя заказ и едешь за самим грузом, а дальше - доставляешь его заказчику", null,
                new Button("Так, а что по оплате?", () => NPC.CurrentNPC?.ShowDialogue("job_debitcard_g_0")),
                Button.DefaultExitButton);

/*            new Dialogue("job_trucker_g_2", "Да сколько себя помню, у меня и брат владеет частью нашего семейного бизнеса, но в Лос-Сантосе. Давно его не видел, мне нужно передать ему одну вещь. Можешь передать ему ее, если будешь там? Он тебя хорошо отблагодарит.", null,
                Button.DefaultBackButton,
                Button.DefaultExitButton);*/

            new Dialogue("job_debitcard_g_0", "С этим у нас все официально, поэтому никаких конвертов. Если у тебя нет карты, получи ее в любом банковском отделении, базовый тариф стоит копейки. Без нее работать у нас не сможешь. А так, начислим тебе все, что ты заработал, даже если тебя вдруг не будет в округе, кхе-кхе", null,
                Button.DefaultBackButton,
                Button.DefaultExitButton);
        }
    }
}
