using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using static BCRPClient.Data.Dialogue;

namespace BCRPClient.Data.NPCs.Dialogues
{
    public class FishBuyer : Events.Script
    {
        public FishBuyer()
        {
            new Dialogue("fishbuyer_0_g", "Приветствую! Я занимаюсь скупкой рыбы, у тебя есть что-нибудь для меня? Готов предложить неплохие деньги", null,
                new Button("Сейчас посмотрю", () => NPC.CurrentNPC?.ShowDialogue("fishbuyer_a_fs0", false, null)),

                new Button("Так, а что по ценам?", () =>
                {
                    var npc = NPC.CurrentNPC;

                    if (npc == null)
                        return;

                    if (npc.Data is Data.Locations.FishBuyer fb)
                    {
                        npc.ShowDialogue("fishbuyer_a_p", true, null, string.Join("\n", Data.Locations.FishBuyer.BasePrices.Select(x => $"{Data.Items.GetName(x.Key) ?? "null"} - {Utils.GetPriceString(fb.GetPrice(x.Key))}")));
                    }
                }),

                Button.DefaultExitButton
            );

            new Dialogue("fishbuyer_a_p", "Вот мои расценки:\n\n{0}\n\nУчти, что раз в какое-то время цены могут изменяться в большую или меньшую сторону, все зависит от воли Посейдона *смеётся*", null,
                Button.DefaultBackButton,

                Button.DefaultExitButton
            );

            new Dialogue("fishbuyer_a_nf", "Где посмотришь? В карманах что-ли? Кхе, и без того вижу, что нет у тебя с собой рыбы, возвращайся, когда наловишь, если сможешь, конечно *посмеивается*", null,
                Button.DefaultBackButton,

                Button.DefaultExitButton
            );

            new Dialogue("fishbuyer_a_fs0", null,
                (args) =>
                {
                    var npc = NPC.CurrentNPC;

                    if (npc == null)
                        return;

                    var fb = npc.Data as Data.Locations.FishBuyer;

                    if (fb == null)
                        return;

                    var dict = new Dictionary<string, int>();

                    foreach (var x in Data.Locations.FishBuyer.BasePrices)
                    {
                        for (int i = 0; i < CEF.Inventory.ItemsParams.Length; i++)
                        {
                            if (CEF.Inventory.ItemsParams[i]?.Id == x.Key)
                            {
                                var amount = ((int)(((object[])CEF.Inventory.ItemsData[i][0])[3]));

                                if (!dict.TryAdd(x.Key, amount))
                                    dict[x.Key] += amount;
                            }
                        }
                    }

                    if (dict.Count == 0)
                    {
                        var curDg = npc.LastDialogues.Where(x => x.Dialogue.Id == "fishbuyer_a_fs0").FirstOrDefault();

                        if (curDg != null)
                            npc.LastDialogues.Remove(curDg);

                        npc.ShowDialogue("fishbuyer_a_nf", true, null);

                        return;
                    }

                    npc.SetTempDialogueData("all_fish", dict);

                    var dg = AllDialogues["fishbuyer_a_fs1"];

                    dg.Buttons.Clear();

                    ulong sum = 0;

                    foreach (var x in dict)
                    {
                        var id = x.Key;
                        var amount = x.Value;

                        var price = fb.GetPrice(id);

                        var name = Data.Items.GetName(x.Key);

                        dg.Buttons.Add(new Button($"{name} - {amount} шт.", () =>
                        {
                            npc.SetTempDialogueData("fish_id", id);
                            npc.SetTempDialogueData("fish_amount_s", amount);

                            npc.ShowDialogue("fishbuyer_a_fs2", true, null, name, Utils.GetPriceString(fb.GetPrice(id)), amount, Utils.GetPriceString(amount * price));
                        }));

                        sum += (uint)amount;
                    }

                    dg.Buttons.Add(new Button($"Вся рыба - {sum} шт.", () =>
                    {
                        npc.ResetTempDialogueData("fish_id");
                        npc.ResetTempDialogueData("fish_amount_s");

                        SellFishApprove(npc, null, 0);
                    }));

                    dg.Buttons.Add(Button.DefaultBackButton);
                    dg.Buttons.Add(Button.DefaultExitButton);

                    if (sum < 20)
                        dg.Text = "Хм, рыбы у тебя немного, но я все равно куплю её, скажи, какую хочешь продать и сколько штук? Или всю сразу, чтоб не мучаться?";
                    else
                        dg.Text = "Неплохой улов, дружище! Скажи, какую рыбу хочешь продать и сколько штук? Могу и сразу всю скупить, если хочешь";

                    npc.ShowDialogue(dg.Id, true, null);
                }
            );

            new Dialogue("fishbuyer_a_fs1", "", null);

            new Dialogue("fishbuyer_a_fs3", null, null,
                new Button("[Продать]", async () =>
                {
                    var npc = NPC.CurrentNPC;

                    if (npc == null)
                        return;

                    var fishId = npc.GetTempDialogueData<string>("fish_id");
                    var amount = npc.GetTempDialogueData<int>("fish_amount");

                    var res = (int)await npc.CallRemoteProc("fishbuyer_s", fishId ?? "", amount);

                    npc.SwitchDialogue(false);
                }),

                Button.DefaultBackButton,
                Button.DefaultExitButton
            );

            new Dialogue("fishbuyer_a_fs2", "Так, {0}, хорошо, сколько штук? За 1 шт. я дам {1}, следовательно, за всю, что у тебя есть, а это {2} шт. - {3}", null,
                new Button("[Выбрать кол-во]", async () =>
                {
                    var npc = NPC.CurrentNPC;

                    if (npc == null)
                        return;

                    var fb = npc.Data as Data.Locations.FishBuyer;

                    if (fb == null)
                        return;

                    var curFishId = npc.GetTempDialogueData<string>("fish_id");
                    var curFishAmountA = npc.GetTempDialogueData<int>("fish_amount_s");

                    if (curFishAmountA <= 1)
                    {
                        SellFishApprove(npc, curFishId, curFishAmountA);

                        return;
                    }

                    await CEF.ActionBox.ShowRange
                    (
                        "FishBuyerRange", "Выберите кол-во рыбы для продажи", 1, curFishAmountA, curFishAmountA, 1, CEF.ActionBox.RangeSubTypes.Default,

                        () =>
                        {
                            CEF.ActionBox.DefaultBindAction.Invoke();

                            CEF.Browser.SwitchTemp(CEF.Browser.IntTypes.NPC, false);

                            Data.NPC.UnbindEsc();
                        },

                        (rType, amountD) =>
                        {
                            var amount = (int)amountD;

                            CEF.ActionBox.Close(false);

                            if (NPC.CurrentNPC != npc)
                                return;

                            if (rType == CEF.ActionBox.ReplyTypes.OK)
                            {
                                SellFishApprove(npc, curFishId, amount);
                            }
                        },

                        () =>
                        {
                            if (NPC.CurrentNPC == npc)
                            {
                                CEF.Cursor.Show(true, true);

                                Data.NPC.BindEsc();

                                CEF.Browser.SwitchTemp(CEF.Browser.IntTypes.NPC, true);
                            }
                        }
                    );
                }),

                new Button("[Продать всю]", () =>
                {
                    var npc = NPC.CurrentNPC;

                    if (npc == null)
                        return;

                    var curFishId = npc.GetTempDialogueData<string>("fish_id");
                    var curFishAmountA = npc.GetTempDialogueData<int>("fish_amount_s");

                    SellFishApprove(npc, curFishId, curFishAmountA);
                }),

                Button.DefaultBackButton,
                Button.DefaultExitButton
            );
        }

        private static void SellFishApprove(NPC npc, string fishId, int amount)
        {
            if (npc == null)
                return;

            var fb = npc.Data as Data.Locations.FishBuyer;

            if (fb == null)
                return;

            npc.SetTempDialogueData("fish_amount", amount);

            if (fishId == null)
            {
                var allFish = npc.GetTempDialogueData<Dictionary<string, int>>("all_fish");

                if (allFish == null)
                    return;

                var t = $"Я правильно понял, что ты хочешь продать всю рыбу, что у тебя есть в кол-ве {allFish.Values.Sum()} шт.? За все это я дам тебе {Utils.GetPriceString(allFish.Select(x => x.Value * (decimal)fb.GetPrice(x.Key)).Sum())}, согласен?";

                var dg = Dialogue.AllDialogues["fishbuyer_a_fs3"].Text = t;

                npc.ShowDialogue("fishbuyer_a_fs3", true, null);
            }
            else
            {
                var t = $"Я правильно понял, что ты хочешь продать мне {Data.Items.GetName(fishId)} в кол-ве {amount} шт.? За все это я дам тебе {Utils.GetPriceString(amount * fb.GetPrice(fishId))}, согласен?";

                var dg = Dialogue.AllDialogues["fishbuyer_a_fs3"].Text = t;

                npc.ShowDialogue("fishbuyer_a_fs3", true, null);
            }
        }
    }
}