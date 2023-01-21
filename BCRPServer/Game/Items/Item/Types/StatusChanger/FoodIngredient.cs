using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Items
{
    public class FoodIngredient : Food, ICraftIngredient
    {
        new public class ItemData : Food.ItemData, Item.ItemData.ICraftIngredient
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Satiety}, {Mood}, {Health}, {MaxAmount}";

            public ItemData(string Name, float Weight, string Model, int Satiety, int Mood, int Health, int MaxAmount, Sync.Animations.FastTypes Animation, Sync.AttachSystem.Types AttachType) : base(Name, Weight, Model, Satiety, Mood, Health, MaxAmount, Animation, AttachType)
            {

            }
        }

        new public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "fi_f_acod", new ItemData("Антарктический тунец", 0.15f, "brp_p_fish_acod_0", 25, 0, 0, 64, Sync.Animations.FastTypes.ItemBurger, Sync.AttachSystem.Types.ItemBurger) },
        };

        public FoodIngredient(string ID) : base(ID, IDList[ID], typeof(FoodIngredient))
        {
            this.Amount = MaxAmount;
        }
    }
}
