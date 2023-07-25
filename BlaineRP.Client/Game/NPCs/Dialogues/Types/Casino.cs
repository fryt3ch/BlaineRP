using System.Collections.Generic;
using BlaineRP.Client.Data;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Game.World;
using RAGE.Elements;
using static BlaineRP.Client.Game.NPCs.Dialogues.Dialogue;

namespace BlaineRP.Client.Game.NPCs.Dialogues.Types
{
    [Script(int.MaxValue)]
    public class Casino
    {
        public Casino()
        {
            new Dialogue("casino_cashier_def", null, async (args) =>
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var npc = NPC.CurrentNPC;

                var casino = npc?.Data as Locations.Casino;

                if (casino == null)
                    return;

                if (args == null)
                {

                }

                var data = args != null && args[0] is string t ? t.Split('_') : ((string)await npc.CallRemoteProc("Casino::GCBD"))?.Split('_');

                if (data == null)
                {
                    CloseCurrentDialogue();

                    return;
                }

                var balance = decimal.Parse(data[0]);

                var btns = new List<Button>();

                btns.Add(new Button($"[Купить фишки - ${casino.BuyChipPrice}]", async () =>
                {
                    var cashBalance = pData.Cash;

                    var maxChips = cashBalance / casino.BuyChipPrice;

                    if (maxChips <= 0)
                    {
                        Notification.ShowError(Locale.Notifications.Money.NotEnough);

                        return;
                    }

                    await ActionBox.ShowRange
                    (
                        "CasinoChipsBuyRange", "Покупка фишек", 1, maxChips, 1, 1, ActionBox.RangeSubTypes.Default,

                        () =>
                        {
                            ActionBox.DefaultBindAction.Invoke();

                            Browser.SwitchTemp(Browser.IntTypes.NPC, false);

                            NPC.UnbindEsc();
                        },

                        (rType, amountD) =>
                        {
                            ActionBox.Close(false);

                            if (rType != ActionBox.ReplyTypes.OK)
                                return;

                            if (NPC.CurrentNPC != npc)
                                return;

                            npc.SetTempDialogueData("buy_chips_amount", amountD);

                            npc.ShowDialogue("casino_cashier_cbuy_0", false, null, Locale.Get("GEN_CHIPS_1", amountD), Locale.Get("GEN_CHIPS_1", (ulong)System.Math.Floor(amountD * casino.BuyChipPrice)));
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
                }));

                btns.Add(new Button($"[Продать фишки - ${casino.SellChipPrice}]", async () =>
                {
                    if (balance <= 0)
                    {
                        Notification.ShowError("У Вас нет фишек!");

                        return;
                    }

                    await ActionBox.ShowRange
                    (
                        "CasinoChipsBuyRange", "Продажа фишек", 1, balance, balance / 2, 1, ActionBox.RangeSubTypes.Default,

                        () =>
                        {
                            ActionBox.DefaultBindAction.Invoke();

                            Browser.SwitchTemp(Browser.IntTypes.NPC, false);

                            NPC.UnbindEsc();
                        },

                        (rType, amountD) =>
                        {
                            ActionBox.Close(false);

                            if (rType != ActionBox.ReplyTypes.OK)
                                return;

                            if (NPC.CurrentNPC != npc)
                                return;

                            npc.SetTempDialogueData("sell_chips_amount", amountD);

                            npc.ShowDialogue("casino_cashier_csell_0", false, null, Locale.Get("GEN_CHIPS_1", amountD), Locale.Get("GEN_CHIPS_1", (ulong)System.Math.Floor(amountD * casino.SellChipPrice)));
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
                }));

                btns.Add(Button.DefaultExitButton);

                var dialogue = Get("casino_cashier_def_0");

                dialogue.Buttons = btns;

                NPC.CurrentNPC.ShowDialogue("casino_cashier_def_0", true, null, Locale.Get("GEN_CHIPS_1", balance));
            });

            new Dialogue("casino_cashier_def_0", "Приветствуем в кассе нашего казино, здесь вы можете купить или продать фишки для игры. У Вас на балансе - {0} фишек", null);

            new Dialogue("casino_cashier_cbuy_0", "Вы действительно хотите приобрести {0} фишек?\nИх стоимость составит ${1}", null,
                new Button("[Купить]", async () =>
                {
                    var npc = NPC.CurrentNPC;

                    var casino = npc?.Data as Locations.Casino;

                    if (casino == null)
                        return;

                    var chipsToBuy = npc.GetTempDialogueData<decimal>("buy_chips_amount");

                    if (chipsToBuy <= 0)
                        return;

                    if (NPC.LastSent.IsSpam(500, false, true))
                        return;

                    NPC.LastSent = Core.ServerTime;

                    var res = (string)await npc.CallRemoteProc("Casino::C", "1", chipsToBuy);

                    if (res == null)
                        return;

                    npc.ShowDialogue("casino_cashier_def", true, new object[] { res });
                }),

                Button.DefaultBackButton,
                Button.DefaultExitButton
            );

            new Dialogue("casino_cashier_csell_0", "Вы действительно хотите продать {0} фишек?\nЗа них Вы получите ${1}", null,
                new Button("[Продать]", async () =>
                {
                    var npc = NPC.CurrentNPC;

                    var casino = npc?.Data as Locations.Casino;

                    if (casino == null)
                        return;

                    var chipsToSell = npc.GetTempDialogueData<decimal>("sell_chips_amount");

                    if (chipsToSell <= 0)
                        return;

                    if (NPC.LastSent.IsSpam(500, false, true))
                        return;

                    NPC.LastSent = Core.ServerTime;

                    var res = (string)await npc.CallRemoteProc("Casino::C", "0", chipsToSell);

                    if (res == null)
                        return;

                    npc.ShowDialogue("casino_cashier_def", true, new object[] { res });
                }),

                Button.DefaultBackButton,
                Button.DefaultExitButton
            );
        }
    }
}