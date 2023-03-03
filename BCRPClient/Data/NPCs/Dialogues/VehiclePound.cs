using BCRPClient.CEF;
using RAGE;
using RAGE.Elements;
using System.Collections.Generic;
using System.Linq;
using static BCRPClient.Data.Dialogue;

namespace BCRPClient.Data.NPCDialogues
{
    public class VehiclePound : Events.Script
    {
        public VehiclePound()
        {
            new Dialogue("vpound_preprocess", null,

                async (args) =>
                {
                    if (NPC.CurrentNPC == null)
                        return;

                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    var data = (string)await NPC.CurrentNPC.CallRemoteProc("vpound_d");

                    if (NPC.CurrentNPC == null)
                        return;

                    if (data == null)
                    {
                        NPC.CurrentNPC.ShowDialogue("vpound_no_vehicles_0");
                    }
                    else
                    {
                        var dataList = data.Split('_').ToList();

                        var price = Utils.GetPriceString(int.Parse(dataList[0]));

                        var vehs = dataList.Skip(1).Select(x => uint.Parse(x)).ToList();

                        if (vehs.Count == 1)
                        {
                            var vid = vehs[0];

                            NPC.CurrentNPC.TempDialogueData = vid;

                            NPC.CurrentNPC.ShowDialogue("vpound_def_dg_0", true, null, pData.OwnedVehicles.Where(x => x.VID == vid).Select(x => x.Data.Name).FirstOrDefault() ?? "null", price);
                        }
                        else
                        {
                            NPC.CurrentNPC.TempDialogueData = vehs;

                            NPC.CurrentNPC.ShowDialogue("vpound_def_dg_1", true, null, price);
                        }
                    }
                }

                );

            new Dialogue("vpound_no_vehicles_0", "Так-с, не вижу ни одного вашего транспорта в нашей системе.\nВот когда он у нас окажется - тогда и приходите!", null,

                Button.DefaultExitButton

                )
            {
                TimedTexts = new Dictionary<TimeTypes, string>()
                {
                    { TimeTypes.Morning, "Доброе утро! `ddg`" },
                    { TimeTypes.Night, "[зевает]\n\n`ddg` И желательно - не ночью..." },
                }
            };


            new Dialogue("vpound_def_dg_0", "Здравствуйте, {0} находится на нашей штрафстоянке, чтобы забрать его оплатите штраф - {1}\nЕсли что, мы принимаем только наличные!", null,

                new Button("[Оплатить]", async () =>
                {
                    var vid = NPC.CurrentNPC?.TempDialogueData as uint?;

                    if (vid == null || NPC.LastSent.IsSpam(500, false, false))
                    {
                        return;
                    }
                    else
                    {
                        if ((bool?)await NPC.CurrentNPC.CallRemoteProc("vpound_p", vid) == true)
                            NPC.CurrentNPC?.SwitchDialogue(false);
                    }
                }),

                Button.DefaultExitButton

                );


            new Dialogue("vpound_def_dg_1", "Здравствуйте, Ваш транспорт находится на нашей штрафстоянке, выберите нужный и оплатите штраф - {0}\nЕсли что, мы принимаем только наличные!", null,

                new Button("[Перейти к выбору]", () =>
                {
                    if (NPC.CurrentNPC == null)
                        return;

                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    var npcId = NPC.CurrentNPC.Id;

                    var vids = NPC.CurrentNPC.TempDialogueData as List<uint>;

                    if (vids == null)
                        return;

                    NPC.CurrentNPC.SwitchDialogue(false);

                    var counter = 0;

                    CEF.ActionBox.ShowSelect(ActionBox.Contexts.VehiclePoundSelect, Locale.Actions.VehiclePoundSelectHeader, vids.Select(x => (counter++, pData.OwnedVehicles.Where(y => y.VID == x).Select(x => $"{x.Data.Name} [#{x.VID}]").FirstOrDefault() ?? "null")).ToArray(), null, null, vids, npcId);
                }),

                Button.DefaultExitButton

                );
        }
    }
}
