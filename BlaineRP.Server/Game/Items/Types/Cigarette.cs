using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.Game.Management.Attachments;
using BlaineRP.Server.Sync;

namespace BlaineRP.Server.Game.Items
{
    public class Cigarette : StatusChanger, IStackable
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

        public static List<AttachmentType> AttachTypes { get; set; } = new List<AttachmentType>(Cigarette.DependentTypes.Keys);

        new public class ItemData : StatusChanger.ItemData, Item.ItemData.IStackable
        {
            public const int UseCigModelIdx = 1;

            public int MaxAmount { get; set; }

            public int MaxPuffs { get; set; }

            public int MaxTime { get; set; }

            public AttachmentType AttachType { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Mood}, {MaxAmount}";

            public ItemData(string Name, string[] Models, int Mood, int MaxPuffs, int MaxTime, AttachmentType AttachType, int MaxAmount) : base(Name, 0.01f, Models, 0, Mood, 0)
            {
                this.MaxAmount = MaxAmount;

                this.MaxPuffs = MaxPuffs;
                this.MaxTime = MaxTime;

                this.AttachType = AttachType;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "cig_0", new ItemData("Сигарета Redwood", new string[] { "prop_cs_ciggy_01", "ng_proc_cigarette01a" }, 25, 15, 300000, AttachmentType.ItemCigMouth, 20) },

            { "cig_1", new ItemData("Сигарета Chartman", new string[] { "prop_sh_cigar_01", "prop_sh_cigar_01" }, 25, 15, 300000, AttachmentType.ItemCig1Mouth, 20) },

            { "cig_c_0", new ItemData("Сигара", new string[] { "prop_cigar_02", "prop_cigar_01" }, 25, 15, 300000, AttachmentType.ItemCig2Mouth, 20) },

            { "cig_j_0", new ItemData("Косяк", new string[] { "p_cs_joint_02", "prop_sh_joint_01" }, 25, 15, 300000, AttachmentType.ItemCig3Mouth, 20) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; }

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

        public Cigarette(string ID) : base(ID, IDList[ID], typeof(Cigarette))
        {

        }
    }
}
