﻿using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Game.UI.CEF;
using RAGE.Elements;

namespace BlaineRP.Client.Game.NPCs.Dialogues
{
    [Script(int.MaxValue)]
    public class VehiclePound
    {
        public VehiclePound()
        {
            new Dialogue("vpound_preprocess",
                null,
                async (args) =>
                {
                    if (NPC.CurrentNPC == null)
                        return;

                    var pData = PlayerData.GetData(Player.LocalPlayer);

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

                        string price = Locale.Get("GEN_MONEY_0", int.Parse(dataList[0]));

                        var vehs = dataList.Skip(1).Select(x => uint.Parse(x)).ToList();

                        if (vehs.Count == 1)
                        {
                            uint vid = vehs[0];

                            NPC.CurrentNPC.SetTempDialogueData("vid", vid);

                            NPC.CurrentNPC.ShowDialogue("vpound_def_dg_0",
                                true,
                                null,
                                pData.OwnedVehicles.Where(x => x.VID == vid).Select(x => x.Data.Name).FirstOrDefault() ?? "null",
                                price
                            );
                        }
                        else
                        {
                            NPC.CurrentNPC.SetTempDialogueData("vid", vehs);

                            NPC.CurrentNPC.ShowDialogue("vpound_def_dg_1", true, null, price);
                        }
                    }
                }
            );

            new Dialogue("vpound_no_vehicles_0",
                "Так-с, не вижу ни одного вашего транспорта в нашей системе.\nВот когда он у нас окажется - тогда и приходите!",
                null,
                Dialogue.Button.DefaultExitButton
            )
            {
                TimedTexts = new Dictionary<Dialogue.TimeTypes, string>()
                {
                    { Dialogue.TimeTypes.Morning, "Доброе утро! `ddg`" },
                    { Dialogue.TimeTypes.Night, "[зевает]\n\n`ddg` И желательно - не ночью..." },
                },
            };


            new Dialogue("vpound_def_dg_0",
                "Здравствуйте, {0} находится на нашей штрафстоянке, чтобы забрать его оплатите штраф - {1}\nЕсли что, мы принимаем только наличные!",
                null,
                new Dialogue.Button("[Оплатить]",
                    async () =>
                    {
                        uint? vid = NPC.CurrentNPC.GetTempDialogueData<uint?>("vid");

                        if (vid == null || NPC.LastSent.IsSpam(500, false, false))
                        {
                            return;
                        }
                        else
                        {
                            if ((bool?)await NPC.CurrentNPC.CallRemoteProc("vpound_p", vid) == true)
                                NPC.CurrentNPC?.SwitchDialogue(false);
                        }
                    }
                ),
                Dialogue.Button.DefaultExitButton
            );


            new Dialogue("vpound_def_dg_1",
                "Здравствуйте, Ваш транспорт находится на нашей штрафстоянке, выберите нужный и оплатите штраф - {0}\nЕсли что, мы принимаем только наличные!",
                null,
                new Dialogue.Button("[Перейти к выбору]",
                    async () =>
                    {
                        if (NPC.CurrentNPC == null)
                            return;

                        var pData = PlayerData.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        string npcId = NPC.CurrentNPC.Id;

                        List<uint> vids = NPC.CurrentNPC.GetTempDialogueData<List<uint>>("vehs");

                        if (vids == null)
                            return;

                        NPC.CurrentNPC.SwitchDialogue(false);

                        var counter = 0;

                        await ActionBox.ShowSelect("VehiclePoundSelect",
                            Locale.Actions.VehiclePoundSelectHeader,
                            vids.Select(x => ((decimal)counter++, pData.OwnedVehicles.Where(y => y.VID == x).Select(x => $"{x.Data.Name} [#{x.VID}]").FirstOrDefault() ?? "null"))
                                .ToArray(),
                            null,
                            null,
                            ActionBox.DefaultBindAction,
                            async (rType, idD) =>
                            {
                                var id = (int)idD;

                                if (rType == ActionBox.ReplyTypes.OK)
                                {
                                    uint vid = vids[id];

                                    if (ActionBox.LastSent.IsSpam(500, false, true))
                                        return;

                                    var npcData = NPC.GetData(npcId);

                                    if (npcData == null)
                                        return;

                                    if ((bool?)await npcData.CallRemoteProc("vpound_p", vid) ?? false)
                                        ActionBox.Close(true);
                                }
                                else if (rType == ActionBox.ReplyTypes.Cancel)
                                {
                                    ActionBox.Close(true);
                                }
                                else
                                {
                                    return;
                                }
                            },
                            null
                        );
                    }
                ),
                Dialogue.Button.DefaultExitButton
            );
        }
    }
}