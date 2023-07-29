using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using RAGE.Elements;
using static BlaineRP.Client.Game.NPCs.Dialogues.Dialogue;

namespace BlaineRP.Client.Game.NPCs.Dialogues
{
    [Script(int.MaxValue)]
    public class Misc
    {
        public Misc()
        {
            new Dialogue("vrent_s_preprocess",
                null,
                async (args) =>
                {
                    if (NPC.CurrentNPC == null)
                        return;

                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    int data = (int?)await NPC.CurrentNPC.CallRemoteProc("vrent_s_d") ?? -1;

                    if (NPC.CurrentNPC == null)
                        return;

                    if (data < 0)
                    {
                        NPC.CurrentNPC.SwitchDialogue(false);
                    }
                    else
                    {
                        Dialogue dg = AllDialogues["vrent_s_def_0"];

                        dg.Buttons[0].Text = $"Конечно [{Locale.Get("GEN_MONEY_0", data)}]";

                        NPC.CurrentNPC.ShowDialogue("vrent_s_def_0");
                    }
                }
            );

            new Dialogue("vrent_s_def_0",
                "Привет! Хочешь недорого арендовать простенький мопед, с которым будет проще изучать наш округ?",
                null,
                new Button(null,
                    async () =>
                    {
                        if (NPC.CurrentNPC == null)
                            return;

                        if ((bool?)await NPC.CurrentNPC.CallRemoteProc("vrent_s_p") ?? false)
                            NPC.CurrentNPC?.SwitchDialogue(false);
                    }
                ),
                Button.DefaultExitButton
            );

            new Dialogue("drivingschool_d_0",
                "Здравствуйте, хотите получить лицензию на управление каким-то типом транспорта?",
                null,
                new Button("Да, хотелось бы",
                    async () =>
                    {
                        NPC.CurrentNPC?.ShowDialogue("drivingschool_d_1", true, null);
                    }
                ),
                Button.DefaultExitButton
            );

            new Dialogue("drivingschool_d_1",
                "Тогда пройдите в экзаменационный класс и выберите нужный Вам тип лицензии!\n\nКогда оплатите сдачу теоретического теста и пройдете его, набрав не менее 80% правильных ответов, сможете пройти практику в любое удобное для вас время в течение дня\n\n[сдача практического экзамена доступна до перезапуска сервера, если не пройдете его до этого момента, Вам придется снова сдавать теорию]",
                null,
                Button.DefaultBackButton,
                Button.DefaultExitButton
            );
        }
    }
}