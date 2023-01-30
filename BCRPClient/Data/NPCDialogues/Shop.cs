using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static BCRPClient.Data.Dialogue;

namespace BCRPClient.Data.NPCDialogues
{
    public class Shop
    {
        public static void Load()
        {
            new Dialogue("seller_bags_preprocess", null, (args) =>
            {
                if (NPC.CurrentNPC == null)
                    return;

                if (Player.LocalPlayer.GetDrawableVariation(5) > 0)
                    NPC.CurrentNPC.ShowDialogue("seller_bags_b_0");
                else
                    NPC.CurrentNPC.ShowDialogue("seller_bags_n_0");

            });

            new Dialogue("seller_bags_b_0", "Привет, вижу у вас уже есть сумка, не у меня покупали случаем? Но не думайте, скидку я никому не даю!", null,
                Button.DefaultShopEnterButton,
                Button.DefaultExitButton
            );

            new Dialogue("seller_furn_g_0", "Здравствуйте, добро пожаловать в наш магазин мебели! Хотите ознакомиться с каталогом?", null,
                new Button("Да, давайте", () => NPC.CurrentNPC?.ShowDialogue("seller_furn_c_0")),

                Button.DefaultExitButton
            );

            new Dialogue("seller_furn_c_0", "Пожалуйста, вот все типы мебели, которые у нас сейчас есть в наличии, выберите желаемый", null,

                Button.DefaultBackButton,
                Button.DefaultExitButton
            );

            AllDialogues["seller_furn_c_0"].Buttons.InsertRange(0, Locale.Property.FurnitureSubTypeNames.Select(x => new Button($"[{x.Value}]", () => { if (NPC.CurrentNPC == null || NPC.LastSent.IsSpam(500, false, false)) return; NPC.LastSent = DateTime.Now; Events.CallRemote("Business::Furn::Enter", (NPC.CurrentNPC.Data as Data.Locations.Business)?.Id ?? -1, (int)x.Key); }, true)));

            new Dialogue("seller_clothes_greeting_0", "Приветствуем в нашем магазине!\nЖелаете ознакомиться с ассортиментом? У нас есть новые поступления, уверена, вам понравится!", null,

                Button.DefaultShopEnterButton,

                new Button("Есть ли работа для меня?", () => { }, true),

                Button.DefaultExitButton

            );

            new Dialogue("seller_shop_greeting_0", "Здравствуйте, хорошо, что вы заглянули к нам сегодня, как раз привезли свежайшие продукты!\n", null,

                Button.DefaultShopEnterButton,

                new Button("Есть ли работа для меня?", () => { }, true),

                Button.DefaultExitButton

            );

            new Dialogue("seller_no_job_0", "К сожалению, пока что ваша помощь ни в чем не требуется, магазин с работой справляется.", null,

                new Button("[Назад]", () => { NPC.CurrentNPC?.ShowDialogue("seller_greeting_0"); }, true),

                new Button("[Выйти]", CloseCurrentDialogue, false)

            );
        }
    }
}
