﻿using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.CEF;
using static BlaineRP.Client.Data.Dialogue;

namespace BlaineRP.Client.Data.NPCs.Dialogues
{
    [Script(int.MaxValue)]
    public class Casino 
    {
        public Casino()
        {
            new Dialogue("casino_cashier_def", null, async (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var npc = NPC.CurrentNPC;

                var casino = npc?.Data as Data.Locations.Casino;

                if (casino == null)
                    return;

                if (args == null)
                {

                }

                var data = args != null && args[0] is string t ? t.Split('_') :((string)await npc.CallRemoteProc("Casino::GCBD"))?.Split('_');

                if (data == null)
                {
                    CloseCurrentDialogue();

                    return;
                }

                var balance = decimal.Parse(data[0]);

                var btns = new List<Dialogue.Button>();

                btns.Add(new Dialogue.Button($"[Купить фишки - ${casino.BuyChipPrice}]", async () =>
                {
                    var cashBalance = pData.Cash;

                    var maxChips = cashBalance / casino.BuyChipPrice;

                    if (maxChips <= 0)
                    {
                        CEF.Notification.ShowError(Locale.Notifications.Money.NotEnough);

                        return;
                    }

                    await CEF.ActionBox.ShowRange
                    (
                        "CasinoChipsBuyRange", "Покупка фишек", 1, maxChips, 1, 1, ActionBox.RangeSubTypes.Default,

                        () =>
                        {
                            CEF.ActionBox.DefaultBindAction.Invoke();

                            CEF.Browser.SwitchTemp(CEF.Browser.IntTypes.NPC, false);

                            Data.NPC.UnbindEsc();
                        },

                        (rType, amountD) =>
                        {
                            CEF.ActionBox.Close(false);

                            if (rType != ActionBox.ReplyTypes.OK)
                                return;

                            if (NPC.CurrentNPC != npc)
                                return;

                            npc.SetTempDialogueData("buy_chips_amount", amountD);

                            npc.ShowDialogue("casino_cashier_cbuy_0", false, null, Locale.Get("GEN_CHIPS_1", amountD), Locale.Get("GEN_CHIPS_1", ((ulong)Math.Floor(amountD * casino.BuyChipPrice))));
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
                }));

                btns.Add(new Dialogue.Button($"[Продать фишки - ${casino.SellChipPrice}]", async () =>
                {
                    if (balance <= 0)
                    {
                        CEF.Notification.ShowError("У Вас нет фишек!");

                        return;
                    }

                    await CEF.ActionBox.ShowRange
                    (
                        "CasinoChipsBuyRange", "Продажа фишек", 1, balance, balance / 2, 1, ActionBox.RangeSubTypes.Default,

                        () =>
                        {
                            CEF.ActionBox.DefaultBindAction.Invoke();

                            CEF.Browser.SwitchTemp(CEF.Browser.IntTypes.NPC, false);

                            Data.NPC.UnbindEsc();
                        },

                        (rType, amountD) =>
                        {
                            CEF.ActionBox.Close(false);

                            if (rType != ActionBox.ReplyTypes.OK)
                                return;

                            if (NPC.CurrentNPC != npc)
                                return;

                            npc.SetTempDialogueData("sell_chips_amount", amountD);

                            npc.ShowDialogue("casino_cashier_csell_0", false, null, Locale.Get("GEN_CHIPS_1", amountD), Locale.Get("GEN_CHIPS_1", ((ulong)Math.Floor(amountD * casino.SellChipPrice))));
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
                }));

                btns.Add(Dialogue.Button.DefaultExitButton);

                var dialogue = Get("casino_cashier_def_0");

                dialogue.Buttons = btns;

                NPC.CurrentNPC.ShowDialogue("casino_cashier_def_0", true, null, Locale.Get("GEN_CHIPS_1", balance));
            });

            new Dialogue("casino_cashier_def_0", "Приветствуем в кассе нашего казино, здесь вы можете купить или продать фишки для игры. У Вас на балансе - {0} фишек", null);

            new Dialogue("casino_cashier_cbuy_0", "Вы действительно хотите приобрести {0} фишек?\nИх стоимость составит ${1}", null,
                new Dialogue.Button("[Купить]", async () =>
                {
                    var npc = NPC.CurrentNPC;

                    var casino = npc?.Data as Data.Locations.Casino;

                    if (casino == null)
                        return;

                    var chipsToBuy = npc.GetTempDialogueData<decimal>("buy_chips_amount");

                    if (chipsToBuy <= 0)
                        return;

                    if (NPC.LastSent.IsSpam(500, false, true))
                        return;

                    NPC.LastSent = Sync.World.ServerTime;

                    var res = (string)await npc.CallRemoteProc("Casino::C", "1", chipsToBuy);

                    if (res == null)
                        return;

                    npc.ShowDialogue("casino_cashier_def", true, new object[] { res });
                }),

                Dialogue.Button.DefaultBackButton,
                Dialogue.Button.DefaultExitButton
            );

            new Dialogue("casino_cashier_csell_0", "Вы действительно хотите продать {0} фишек?\nЗа них Вы получите ${1}", null,
                new Dialogue.Button("[Продать]", async () =>
                {
                    var npc = NPC.CurrentNPC;

                    var casino = npc?.Data as Data.Locations.Casino;

                    if (casino == null)
                        return;

                    var chipsToSell = npc.GetTempDialogueData<decimal>("sell_chips_amount");

                    if (chipsToSell <= 0)
                        return;

                    if (NPC.LastSent.IsSpam(500, false, true))
                        return;

                    NPC.LastSent = Sync.World.ServerTime;

                    var res = (string)await npc.CallRemoteProc("Casino::C", "0", chipsToSell);

                    if (res == null)
                        return;

                    npc.ShowDialogue("casino_cashier_def", true, new object[] { res });
                }),

                Dialogue.Button.DefaultBackButton,
                Dialogue.Button.DefaultExitButton
            );
        }
    }
}