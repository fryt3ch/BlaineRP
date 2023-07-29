using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.Game.Management.Animations;
using BlaineRP.Server.Game.Management.Attachments;
using BlaineRP.Server.Sync;

namespace BlaineRP.Server.Game.Items
{
    public class Food : StatusChanger, IStackable
    {
        new public class ItemData : StatusChanger.ItemData, Item.ItemData.IStackable
        {
            public int MaxAmount { get; set; }

            public FastType Animation { get; set; }

            public AttachmentType AttachType { get; set; }

            public TimeSpan UsageTime { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Satiety}, {Mood}, {Health}, {MaxAmount}";

            public ItemData(string Name, float Weight, string Model, int Satiety, int Mood, int Health, int MaxAmount, TimeSpan UsageTime, FastType Animation, AttachmentType AttachType) : base(Name, Weight, new string[] { Model }, Satiety, Mood, Health)
            {
                this.MaxAmount = MaxAmount;

                this.Animation = Animation;

                this.AttachType = AttachType;

                this.UsageTime = UsageTime;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "f_burger", new ItemData("Бургер", 0.15f, "prop_cs_burger_01", 25, 0, 0, 64, TimeSpan.FromMilliseconds(6_000), FastType.ItemBurger, AttachmentType.ItemBurger) },
            { "f_chips", new ItemData("Чипсы",0.15f, "prop_food_bs_chips", 15, 0, 0, 64, TimeSpan.FromMilliseconds(6_000), FastType.ItemChips, AttachmentType.ItemChips) },
            { "f_pizza", new ItemData("Пицца", 0.15f, "v_res_tt_pizzaplate", 50, 15, 0, 64, TimeSpan.FromMilliseconds(6_000), FastType.ItemPizza, AttachmentType.ItemPizza) },
            { "f_chocolate", new ItemData("Шоколадка", 0.15f,  "prop_candy_pqs", 10, 20, 0, 64, TimeSpan.FromMilliseconds(6_000), FastType.ItemChocolate, AttachmentType.ItemChocolate) },
            { "f_hotdog", new ItemData("Хот-дог", 0.15f, "prop_cs_hotdog_01", 10, 20, 0, 64, TimeSpan.FromMilliseconds(6_000), FastType.ItemChocolate, AttachmentType.ItemChocolate) },

            { "f_cola", new ItemData("Кола", 0.15f, "prop_food_juice01", 5, 20, 0, 64, TimeSpan.FromMilliseconds(6_000), FastType.ItemCola, AttachmentType.ItemCola) },

            { "f_beer", new ItemData("Пиво", 0.15f, "prop_sh_beer_pissh_01", 5, 50, 0, 64, TimeSpan.FromMilliseconds(6_000), FastType.ItemBeer, AttachmentType.ItemBeer) },

            { "f_acod_f", new ItemData("Антарктический тунец (ж.)", 0.1f, "brp_p_fish_meat_c_0", 25, 15, 0, 128, TimeSpan.FromMilliseconds(6_000), FastType.ItemBurger, AttachmentType.ItemBurger) },
            { "f_acod", new ItemData("Антарктический тунец", 0.15f, "brp_p_fish_acod_0", 5, 0, 0, 64, TimeSpan.FromMilliseconds(6_000), FastType.ItemBurger, AttachmentType.ItemBurger) },
        };

        [JsonIgnore]
        new public ItemData Data => (ItemData)base.Data;

        [JsonIgnore]
        public int MaxAmount => Data.MaxAmount;

        [JsonIgnore]
        public override float Weight => BaseWeight * Amount;

        public int Amount { get; set; }

        public override void Apply(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            player.AttachObject(data.Model, data.AttachType, (int)data.UsageTime.TotalMilliseconds, null);

            pData.PlayAnim(data.Animation, data.UsageTime);

            if (Data.Satiety > 0)
            {
                var satietyDiff = (byte)Utils.CalculateDifference(pData.Satiety, data.Satiety, 0, Properties.Settings.Static.PlayerMaxSatiety);

                if (satietyDiff != 0)
                {
                    pData.Satiety += satietyDiff;
                }
            }

            if (Data.Mood > 0)
            {
                var moodDiff = (byte)Utils.CalculateDifference(pData.Mood, data.Mood, 0, Properties.Settings.Static.PlayerMaxMood);

                if (moodDiff != 0)
                {
                    pData.Mood += moodDiff;
                }
            }
        }

        [JsonConstructor]
        public Food(string ID) : base(ID, IDList[ID], typeof(Food))
        {
            this.Amount = MaxAmount;
        }

        public Food(string ID, Item.ItemData ItemData, System.Type Type) : base(ID, ItemData, Type)
        {
            this.Amount = MaxAmount;
        }
    }
}
