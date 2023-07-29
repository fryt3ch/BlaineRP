using System;
using System.Collections.Generic;
using BlaineRP.Server.Game.Animations;
using BlaineRP.Server.Game.Attachments;

namespace BlaineRP.Server.Game.Items
{
    public partial class Food
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            { "f_burger", new ItemData("Бургер", 0.15f, "prop_cs_burger_01", 25, 0, 0, 64, TimeSpan.FromMilliseconds(6_000), FastType.ItemBurger, AttachmentType.ItemBurger) },
            { "f_chips", new ItemData("Чипсы", 0.15f, "prop_food_bs_chips", 15, 0, 0, 64, TimeSpan.FromMilliseconds(6_000), FastType.ItemChips, AttachmentType.ItemChips) },
            { "f_pizza", new ItemData("Пицца", 0.15f, "v_res_tt_pizzaplate", 50, 15, 0, 64, TimeSpan.FromMilliseconds(6_000), FastType.ItemPizza, AttachmentType.ItemPizza) },
            {
                "f_chocolate",
                new ItemData("Шоколадка", 0.15f, "prop_candy_pqs", 10, 20, 0, 64, TimeSpan.FromMilliseconds(6_000), FastType.ItemChocolate, AttachmentType.ItemChocolate)
            },
            {
                "f_hotdog",
                new ItemData("Хот-дог", 0.15f, "prop_cs_hotdog_01", 10, 20, 0, 64, TimeSpan.FromMilliseconds(6_000), FastType.ItemChocolate, AttachmentType.ItemChocolate)
            },

            { "f_cola", new ItemData("Кола", 0.15f, "prop_food_juice01", 5, 20, 0, 64, TimeSpan.FromMilliseconds(6_000), FastType.ItemCola, AttachmentType.ItemCola) },

            { "f_beer", new ItemData("Пиво", 0.15f, "prop_sh_beer_pissh_01", 5, 50, 0, 64, TimeSpan.FromMilliseconds(6_000), FastType.ItemBeer, AttachmentType.ItemBeer) },

            {
                "f_acod_f",
                new ItemData("Антарктический тунец (ж.)",
                    0.1f,
                    "brp_p_fish_meat_c_0",
                    25,
                    15,
                    0,
                    128,
                    TimeSpan.FromMilliseconds(6_000),
                    FastType.ItemBurger,
                    AttachmentType.ItemBurger
                )
            },
            {
                "f_acod",
                new ItemData("Антарктический тунец", 0.15f, "brp_p_fish_acod_0", 5, 0, 0, 64, TimeSpan.FromMilliseconds(6_000), FastType.ItemBurger, AttachmentType.ItemBurger)
            },
        };
    }
}