using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.UI.CEF;
using static BlaineRP.Client.Game.NPCs.Dialogues.Dialogue;

namespace BlaineRP.Client.Game.NPCs.Dialogues
{
    [Script(int.MaxValue)]
    public class FishBuyer
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

                    if (npc.Data is Game.Misc.FishBuyer fb)
                        npc.ShowDialogue("fishbuyer_a_p", true, null, string.Join("\n", Game.Misc.FishBuyer.BasePrices.Select(x => $"{Items.Core.GetName(x.Key) ?? "null"} - {Locale.Get("GEN_MONEY_0", fb.GetPrice(x.Key))}")));
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

                    var fb = npc.Data as Game.Misc.FishBuyer;

                    if (fb == null)
                        return;

                    var dict = new Dictionary<string, int>();

                    foreach (var x in Game.Misc.FishBuyer.BasePrices)
                    {
                        for (int i = 0; i < Inventory.ItemsParams.Length; i++)
                        {
                            if (Inventory.ItemsParams[i]?.Id == x.Key)
                            {
                                var amount = (int)((object[])Inventory.ItemsData[i][0])[3];

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

                        var name = Items.Core.GetName(x.Key);

                        dg.Buttons.Add(new Button($"{name} - {amount} шт.", () =>
                        {
                            npc.SetTempDialogueData("fish_id", id);
                            npc.SetTempDialogueData("fish_amount_s", amount);

                            npc.ShowDialogue("fishbuyer_a_fs2", true, null, name, Locale.Get("GEN_MONEY_0", fb.GetPrice(id)), amount, Locale.Get("GEN_MONEY_0", amount * price));
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

                    var fb = npc.Data as Game.Misc.FishBuyer;

                    if (fb == null)
                        return;

                    var curFishId = npc.GetTempDialogueData<string>("fish_id");
                    var curFishAmountA = npc.GetTempDialogueData<int>("fish_amount_s");

                    if (curFishAmountA <= 1)
                    {
                        SellFishApprove(npc, curFishId, curFishAmountA);

                        return;
                    }

                    await ActionBox.ShowRange
                    (
                        "FishBuyerRange", "Выберите кол-во рыбы для продажи", 1, curFishAmountA, curFishAmountA, 1, ActionBox.RangeSubTypes.Default,

                        () =>
                        {
                            ActionBox.DefaultBindAction.Invoke();

                            Browser.SwitchTemp(Browser.IntTypes.NPC, false);

                            NPC.UnbindEsc();
                        },

                        (rType, amountD) =>
                        {
                            var amount = (int)amountD;

                            ActionBox.Close(false);

                            if (NPC.CurrentNPC != npc)
                                return;

                            if (rType == ActionBox.ReplyTypes.OK)
                                SellFishApprove(npc, curFishId, amount);
                        },

                        () =>
                        {
                            if (NPC.CurrentNPC == npc)
                            {
                                Cursor.Show(true, true);

                                NPC.BindEsc();

                                Browser.SwitchTemp(Browser.IntTypes.NPC, true);
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

            var fb = npc.Data as Game.Misc.FishBuyer;

            if (fb == null)
                return;

            npc.SetTempDialogueData("fish_amount", amount);

            if (fishId == null)
            {
                var allFish = npc.GetTempDialogueData<Dictionary<string, int>>("all_fish");

                if (allFish == null)
                    return;

                var t = $"Я правильно понял, что ты хочешь продать всю рыбу, что у тебя есть в кол-ве {allFish.Values.Sum()} шт.? За все это я дам тебе {Locale.Get("GEN_MONEY_0", allFish.Select(x => x.Value * (decimal)fb.GetPrice(x.Key)).Sum())}, согласен?";

                var dg = AllDialogues["fishbuyer_a_fs3"].Text = t;

                npc.ShowDialogue("fishbuyer_a_fs3", true, null);
            }
            else
            {
                var t = $"Я правильно понял, что ты хочешь продать мне {Items.Core.GetName(fishId)} в кол-ве {amount} шт.? За все это я дам тебе {Locale.Get("GEN_MONEY_0", amount * fb.GetPrice(fishId))}, согласен?";

                var dg = AllDialogues["fishbuyer_a_fs3"].Text = t;

                npc.ShowDialogue("fishbuyer_a_fs3", true, null);
            }
        }
    }
}