using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;
using static BCRPClient.Data.Dialogue;

namespace BCRPClient.Data.NPCDialogues
{
    public class Misc
    {
        public static void Load()
        {
            new Dialogue("vrent_s_preprocess", null,

                async (args) =>
                {
                    if (NPC.CurrentNPC == null)
                        return;

                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    var data = (int?)await NPC.CurrentNPC.CallRemoteProc("vrent_s_d") ?? -1;

                    if (NPC.CurrentNPC == null)
                        return;

                    if (data < 0)
                    {
                        NPC.CurrentNPC.SwitchDialogue(false);
                    }
                    else
                    {
                        var dg = AllDialogues["vrent_s_def_0"];

                        dg.Buttons[0].Text = $"Конечно [{Utils.GetPriceString(data)}]";

                        NPC.CurrentNPC.ShowDialogue("vrent_s_def_0");
                    }
                }

                );

            new Dialogue("vrent_s_def_0", "Привет! Хочешь недорого арендовать простенький мопед, с которым будет проще изучать наш округ?", null,

                new Button(null, async () =>
                {
                    if (NPC.CurrentNPC == null)
                        return;

                    if ((bool?)await NPC.CurrentNPC.CallRemoteProc("vrent_s_p") ?? false)
                        NPC.CurrentNPC?.SwitchDialogue(false);
                }),

                Button.DefaultExitButton

                );
        }
    }
}
