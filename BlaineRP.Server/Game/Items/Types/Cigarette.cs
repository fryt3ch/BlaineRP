using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Sync;

namespace BlaineRP.Server.Game.Items
{
    public partial class Cigarette : StatusChanger, IStackable
    {
        public static TimeSpan SmokePuffAnimationTime { get; } = TimeSpan.FromMilliseconds(3_000);
        public static TimeSpan SmokeTransitionAnimationTime { get; } = TimeSpan.FromMilliseconds(1_000);
        public static TimeSpan SmokeTransitionTime { get; } = TimeSpan.FromMilliseconds(500);

        public static Dictionary<AttachmentType, AttachmentType> DependentTypes = new Dictionary<AttachmentType, AttachmentType>()
        {
            { AttachmentType.ItemCigMouth, AttachmentType.ItemCigHand },
            { AttachmentType.ItemCig1Mouth, AttachmentType.ItemCig1Hand },
            { AttachmentType.ItemCig2Mouth, AttachmentType.ItemCig2Hand },
            { AttachmentType.ItemCig3Mouth, AttachmentType.ItemCig3Hand },

            { AttachmentType.ItemCigHand, AttachmentType.ItemCigMouth },
            { AttachmentType.ItemCig1Hand, AttachmentType.ItemCig1Mouth },
            { AttachmentType.ItemCig2Hand, AttachmentType.ItemCig2Mouth },
            { AttachmentType.ItemCig3Hand, AttachmentType.ItemCig3Mouth },
        };

        public static List<AttachmentType> AttachTypes { get; set; } = new List<AttachmentType>(DependentTypes.Keys);

        public new class ItemData : StatusChanger.ItemData, Item.ItemData.IStackable
        {
            public const int UseCigModelIdx = 1;

            public int MaxAmount { get; set; }

            public int MaxPuffs { get; set; }

            public int MaxTime { get; set; }

            public AttachmentType AttachType { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Mood}, {MaxAmount}";

            public ItemData(string name, string[] models, int mood, int maxPuffs, int maxTime, AttachmentType attachType, int maxAmount) : base(name, 0.01f, models, 0, mood, 0)
            {
                MaxAmount = maxAmount;

                MaxPuffs = maxPuffs;
                MaxTime = maxTime;

                AttachType = attachType;
            }
        }

        [JsonIgnore]
        public new ItemData Data { get => (ItemData)base.Data; }

        [JsonIgnore]
        public int MaxAmount => Data.MaxAmount;

        [JsonIgnore]
        public override float Weight { get => BaseWeight * Amount; }

        public int Amount { get; set; }

        public override void Apply(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            player.AttachObject(data.GetModelAt(ItemData.UseCigModelIdx), data.AttachType, -1, null, data.MaxTime, data.MaxPuffs);

            var moodDiff = (byte)Utils.CalculateDifference(pData.Mood, data.Mood, 0, Properties.Settings.Static.PlayerMaxMood);

            if (moodDiff != 0)
            {
                pData.Mood += moodDiff;
            }
        }

        public Cigarette(string id) : base(id, IdList[id], typeof(Cigarette))
        {

        }
    }
}
