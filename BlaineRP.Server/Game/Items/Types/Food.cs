using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using BlaineRP.Server.Game.Animations;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Sync;

namespace BlaineRP.Server.Game.Items
{
    public partial class Food : StatusChanger, IStackable
    {
        public new class ItemData : StatusChanger.ItemData, Item.ItemData.IStackable
        {
            public int MaxAmount { get; set; }

            public FastType Animation { get; set; }

            public AttachmentType AttachType { get; set; }

            public TimeSpan UsageTime { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Satiety}, {Mood}, {Health}, {MaxAmount}";

            public ItemData(string name, float weight, string model, int satiety, int mood, int health, int maxAmount, TimeSpan usageTime, FastType animation, AttachmentType attachType) : base(name, weight, new string[] { model }, satiety, mood, health)
            {
                MaxAmount = maxAmount;

                Animation = animation;

                AttachType = attachType;

                UsageTime = usageTime;
            }
        }

        [JsonIgnore]
        public new ItemData Data => (ItemData)base.Data;

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
        public Food(string id) : base(id, IdList[id], typeof(Food))
        {
            Amount = MaxAmount;
        }

        public Food(string id, Item.ItemData itemData, Type type) : base(id, itemData, type)
        {
            Amount = MaxAmount;
        }
    }
}
