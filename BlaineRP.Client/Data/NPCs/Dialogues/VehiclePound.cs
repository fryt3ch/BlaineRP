using BlaineRP.Client.CEF;
using RAGE;
using RAGE.Elements;
using System.Collections.Generic;
using System.Linq;
using static BlaineRP.Client.Data.Dialogue;

namespace BlaineRP.Client.Data.NPCDialogues
{
    [Script(int.MaxValue)]
    public class VehiclePound 
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

                            NPC.CurrentNPC.SetTempDialogueData("vid", vid);

                            NPC.CurrentNPC.ShowDialogue("vpound_def_dg_0", true, null, pData.OwnedVehicles.Where(x => x.VID == vid).Select(x => x.Data.Name).FirstOrDefault() ?? "null", price);
                        }
                        else
                        {
                            NPC.CurrentNPC.SetTempDialogueData("vid", vehs);

                            NPC.CurrentNPC.ShowDialogue("vpound_def_dg_1", true, null, price);
                        }
                    }
                }

                );

            new Dialogue("vpound_no_vehicles_0", "Так-с, не вижу ни одного вашего транспорта в нашей системе.\nВот когда он у нас окажется - тогда и приходите!", null,

                Dialogue.Button.DefaultExitButton

                )
            {
                TimedTexts = new Dictionary<Dialogue.TimeTypes, string>()
                {
                    { Dialogue.TimeTypes.Morning, "Доброе утро! `ddg`" },
                    { Dialogue.TimeTypes.Night, "[зевает]\n\n`ddg` И желательно - не ночью..." },
                }
            };


            new Dialogue("vpound_def_dg_0", "Здравствуйте, {0} находится на нашей штрафстоянке, чтобы забрать его оплатите штраф - {1}\nЕсли что, мы принимаем только наличные!", null,

                new Dialogue.Button("[Оплатить]", async () =>
                {
                    var vid = NPC.CurrentNPC.GetTempDialogueData<uint?>("vid");

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

                Dialogue.Button.DefaultExitButton

                );


            new Dialogue("vpound_def_dg_1", "Здравствуйте, Ваш транспорт находится на нашей штрафстоянке, выберите нужный и оплатите штраф - {0}\nЕсли что, мы принимаем только наличные!", null,

                new Dialogue.Button("[Перейти к выбору]", async () =>
                {
                    if (NPC.CurrentNPC == null)
                        return;

                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    var npcId = NPC.CurrentNPC.Id;

                    var vids = NPC.CurrentNPC.GetTempDialogueData<List<uint>>("vehs");

                    if (vids == null)
                        return;

                    NPC.CurrentNPC.SwitchDialogue(false);

                    var counter = 0;

                    await CEF.ActionBox.ShowSelect
                    (
                        "VehiclePoundSelect", Locale.Actions.VehiclePoundSelectHeader, vids.Select(x => ((decimal)counter++, pData.OwnedVehicles.Where(y => y.VID == x).Select(x => $"{x.Data.Name} [#{x.VID}]").FirstOrDefault() ?? "null")).ToArray(), null, null,

                        CEF.ActionBox.DefaultBindAction,

                        async (rType, idD) =>
                        {
                            var id = (int)idD;

                            if (rType == CEF.ActionBox.ReplyTypes.OK)
                            {
                                var vid = vids[id];

                                if (CEF.ActionBox.LastSent.IsSpam(500, false, true))
                                    return;

                                var npcData = Data.NPC.GetData(npcId);

                                if (npcData == null)
                                    return;

                                if ((bool?)await npcData.CallRemoteProc("vpound_p", vid) ?? false)
                                {
                                    CEF.ActionBox.Close(true);
                                }
                            }
                            else if (rType == CEF.ActionBox.ReplyTypes.Cancel)
                            {
                                CEF.ActionBox.Close(true);
                            }
                            else
                                return;
                        },

                        null
                    );
                }),

                Dialogue.Button.DefaultExitButton

                );
        }
    }
}
