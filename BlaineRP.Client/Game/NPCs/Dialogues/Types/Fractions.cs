using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Game.World;
using Newtonsoft.Json.Linq;
using RAGE.Elements;

namespace BlaineRP.Client.Game.NPCs.Dialogues
{
    [Script(int.MaxValue)]
    public class Fraction
    {
        public Fraction()
        {
            new Dialogue("cop_0_g",
                "Здравствуйте, я - дежурный полиции, чем могу помочь?",
                null,
                new Dialogue.Button("[Купить гос. номер для транспорта]",
                    async () =>
                    {
                        NPC npc = NPC.CurrentNPC;

                        if (npc == null)
                            return;

                        npc.SwitchDialogue(false);

                        await Numberplates.Show(1m, npc.Id);
                    }
                ),
                new Dialogue.Button("[Поставить транспорт на учёт]", () => NPC.CurrentNPC?.ShowDialogue("cop_a_regv", true, null)),
                new Dialogue.Button("[Снять транспорт с учёта]", () => NPC.CurrentNPC?.ShowDialogue("cop_a_unregv", true, null)),
                Dialogue.Button.DefaultExitButton
            );

            new Dialogue("cop_a_regv",
                "Постановка транспорта на учёт заключается в регистрации гос. номера на Ваш транспорт, это стандартная юридическая процедура после любой фактической смены гос. номера.\nЧтобы сделать это, на Вашем транспорте должен быть установлен гос. номер. Хотите выбрать конкретный транспорт и продолжить?\n",
                null,
                new Dialogue.Button("Да, давайте",
                    async () =>
                    {
                        var pData = PlayerData.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        NPC npc = NPC.CurrentNPC;

                        if (npc == null)
                            return;

                        if (NPC.LastSent.IsSpam(1000, false, true))
                            return;

                        NPC.LastSent = Core.ServerTime;

                        var res = ((JArray)await npc.CallRemoteProc("cop_np_vreg_g", 1))?.ToObject<List<string>>()
                                                                                         .Select(x =>
                                                                                              {
                                                                                                  string[] d = x.Split('&');
                                                                                                  var vid = uint.Parse(d[0]);
                                                                                                  return (vid, d[1],
                                                                                                      pData.OwnedVehicles.Where(y => y.VID == vid)
                                                                                                           .Select(x => x.Data)
                                                                                                           .FirstOrDefault());
                                                                                              }
                                                                                          )
                                                                                         .Where(x => x.Item3 != null)
                                                                                         .ToList();

                        if (res == null)
                            return;

                        if (res.Count == 0)
                        {
                            Notification.ShowError("Вы не владеете ни одним транспортом, который не стоит на учёте и на который установлен какой-либо гос. номер!");

                            return;
                        }

                        npc = NPC.CurrentNPC;

                        if (npc == null)
                            return;

                        npc.SwitchDialogue(false);

                        await ActionBox.ShowSelect("NumberplateRegSelect",
                            "Выберите транспорт",
                            res.Select(x => ((decimal)x.vid, $"{x.Item3.Name} | [{x.Item2}]")).ToArray(),
                            null,
                            null,
                            ActionBox.DefaultBindAction,
                            async (rType, id) =>
                            {
                                ActionBox.Close(true);

                                if (rType == ActionBox.ReplyTypes.OK)
                                {
                                    object priceT = await npc.CallRemoteProc("cop_np_vreg_p", 1);

                                    if (priceT == null)
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        (uint vid, string, Data.Vehicles.Vehicle) vData = res.Where(x => x.vid == id).FirstOrDefault();

                                        var price = Utils.Convert.ToDecimal(priceT);

                                        await ActionBox.ShowMoney("NumberplateRegMoney",
                                            "Постановка транспорта на учёт",
                                            $"Вы действительно хотите поставить транспорт {vData.Item3.Name} [#{vData.vid}] на государственный учёт?\n\nОн будет зарегистрирован под гос. номером [{vData.Item2}], стоимость услуги - {Locale.Get("GEN_MONEY_0", price)}",
                                            ActionBox.DefaultBindAction,
                                            async (rType) =>
                                            {
                                                if (rType == ActionBox.ReplyTypes.OK || rType == ActionBox.ReplyTypes.Cancel)
                                                {
                                                    if (NPC.LastSent.IsSpam(1000, false, true))
                                                        return;

                                                    NPC.LastSent = Core.ServerTime;

                                                    bool useCash = rType == ActionBox.ReplyTypes.OK;

                                                    var res = (string)await npc.CallRemoteProc("cop_np_vreg", vData.vid, useCash ? 1 : 0);

                                                    if (res != null)
                                                    {
                                                        Notification.Show(Notification.Types.Success,
                                                            Locale.Get("NOTIFICATION_HEADER_DEF"),
                                                            $"Вы поставили {vData.Item3.Name} на гос. учёт, он зарегистрирован под гос. номером [{res}]"
                                                        );

                                                        ActionBox.Close(true);
                                                    }
                                                }
                                                else
                                                {
                                                    ActionBox.Close(true);
                                                }
                                            },
                                            null
                                        );
                                    }
                                }
                            },
                            null
                        );
                    }
                ),
                Dialogue.Button.DefaultBackButton,
                Dialogue.Button.DefaultExitButton
            );

            new Dialogue("cop_a_unregv",
                "Снятие транспорта с учёта заключается в откреплении зарегистрированного гос. номера от Вашего транспорта. После выполнения данной процедуры Вы не сможете легально пользоваться этим транспортом без гос. номера или с любым фактически установленным на него гос. номером, для этого Вам необходимо будет снова поставить транспорт на учёт. Хотите выбрать конкретный транспорт и продолжить?\n",
                null,
                new Dialogue.Button("Да, давайте",
                    async () =>
                    {
                        var pData = PlayerData.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        NPC npc = NPC.CurrentNPC;

                        if (npc == null)
                            return;

                        if (NPC.LastSent.IsSpam(1000, false, true))
                            return;

                        NPC.LastSent = Core.ServerTime;

                        var res = ((JArray)await npc.CallRemoteProc("cop_np_vreg_g", 0))?.ToObject<List<string>>()
                                                                                         .Select(x =>
                                                                                              {
                                                                                                  string[] d = x.Split('&');
                                                                                                  var vid = uint.Parse(d[0]);
                                                                                                  return (vid, d[1],
                                                                                                      pData.OwnedVehicles.Where(y => y.VID == vid)
                                                                                                           .Select(x => x.Data)
                                                                                                           .FirstOrDefault());
                                                                                              }
                                                                                          )
                                                                                         .Where(x => x.Item3 != null)
                                                                                         .ToList();

                        if (res == null)
                            return;

                        if (res.Count == 0)
                        {
                            Notification.ShowError("Вы не владеете ни одним транспортом, который стоит на гос. учёте!");

                            return;
                        }

                        npc = NPC.CurrentNPC;

                        if (npc == null)
                            return;

                        npc.SwitchDialogue(false);

                        await ActionBox.ShowSelect("NumberplateUnRegSelect",
                            "Выберите транспорт",
                            res.Select(x => ((decimal)x.vid, $"{x.Item3.Name} | [{x.Item2}]")).ToArray(),
                            null,
                            null,
                            ActionBox.DefaultBindAction,
                            async (rType, id) =>
                            {
                                ActionBox.Close(true);

                                if (rType == ActionBox.ReplyTypes.OK)
                                {
                                    object priceT = await npc.CallRemoteProc("cop_np_vreg_p", 0);

                                    if (priceT == null)
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        (uint vid, string, Data.Vehicles.Vehicle) vData = res.Where(x => x.vid == id).FirstOrDefault();

                                        var price = Utils.Convert.ToDecimal(priceT);

                                        await ActionBox.ShowMoney("NumberplateUnRegMoney",
                                            "Снятие транспорта с учёта",
                                            $"Вы действительно хотите снять транспорт {vData.Item3.Name} [#{vData.vid}] с государственного учёта?\n\nСейчас он зарегистрирован под гос. номером [{vData.Item2}], стоимость услуги - {Locale.Get("GEN_MONEY_0", price)}",
                                            ActionBox.DefaultBindAction,
                                            async (rType) =>
                                            {
                                                if (rType == ActionBox.ReplyTypes.OK || rType == ActionBox.ReplyTypes.Cancel)
                                                {
                                                    if (NPC.LastSent.IsSpam(1000, false, true))
                                                        return;

                                                    NPC.LastSent = Core.ServerTime;

                                                    bool useCash = rType == ActionBox.ReplyTypes.OK;

                                                    var res = (string)await npc.CallRemoteProc("cop_np_vunreg", vData.vid, useCash ? 1 : 0);

                                                    if (res != null)
                                                    {
                                                        Notification.Show(Notification.Types.Success,
                                                            Locale.Get("NOTIFICATION_HEADER_DEF"),
                                                            $"Вы сняли {vData.Item3.Name} с гос. учёта!"
                                                        );

                                                        if (res.Length > 0)
                                                            Notification.ShowHint(
                                                                $"На транспорте, который вы только что сняли с учёта, сейчас фактически установлен гос. номер - [{res}], если Вы не хотите получать штрафы за езду на транспорте с ложным гос. номером - поставьте его на учёт!",
                                                                true
                                                            );

                                                        ActionBox.Close(true);
                                                    }
                                                }
                                                else
                                                {
                                                    ActionBox.Close(true);
                                                }
                                            },
                                            null
                                        );
                                    }
                                }
                            },
                            null
                        );
                    }
                ),
                Dialogue.Button.DefaultBackButton,
                Dialogue.Button.DefaultExitButton
            );
        }
    }
}