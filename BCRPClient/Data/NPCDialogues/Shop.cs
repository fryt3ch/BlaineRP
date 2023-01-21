using RAGE.Elements;
using System;
using System.Collections.Generic;
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

            new Dialogue("seller_bags_n_0", "Приветствую, вижу, вам чего-то не хватает... Как насчёт новенькой сумочки или рюкзака?", null,
                Button.DefaultShopEnterButton,
                Button.DefaultExitButton
            );

            new Dialogue("seller_clothes_greeting_0", "Приветствуем в нашем магазине!\nЖелаете ознакомиться с ассортиментом? У нас есть новые поступления, уверена, вам понравится!", null,

                Button.DefaultShopEnterButton,

                new Button("Есть ли работа для меня?", () => { }, true),

                Button.DefaultExitButton

            );

            new Dialogue("seller_shop_greeting_0", "Здравствуйте, хорошо, что вы заглянули к нам сегодня, как раз привезли свежайшие продукты!\n", null,

                new Button("[Смотреть товары]", () => { }, true),

                new Button("Есть ли работа для меня?", () => { }, true),

                new Button("[Выйти]", CloseCurrentDialogue, false)

            );

            new Dialogue("seller_gas_greeting_0", "Здравствуйте, хотите что-то приобрести?", null,

                new Button("[Смотреть товары]", () => { }, true),

                new Button("Я хочу заправить транспорт", () => { }, true),

                new Button("Нет, спасибо", () => { CEF.Interaction.CloseMenu(); }, false)

            );

            new Dialogue("seller_gas_info_0", "Не, это не ко мне, у меня можно купить разные приблуды для дороги, например, ремонтный набор или канистру с топливом, а заправиться вы можете самостоятельно, поставив транспорт возле бензоколонки.", null,

                new Button("[Назад]", () => { NPC.CurrentNPC?.ShowDialogue("seller_gas_greeting_0"); }, true),

                new Button("[Выйти]", CloseCurrentDialogue, false)

            );

            new Dialogue("seller_no_job_0", "К сожалению, пока что ваша помощь ни в чем не требуется, магазин с работой справляется.", null,

                new Button("[Назад]", () => { NPC.CurrentNPC?.ShowDialogue("seller_greeting_0"); }, true),

                new Button("[Выйти]", CloseCurrentDialogue, false)

            );
        }
    }
}
