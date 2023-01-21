using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Items
{
    public class Cigarette : StatusChanger, IStackable
    {
        public static Dictionary<Sync.AttachSystem.Types, Sync.AttachSystem.Types> DependentTypes = new Dictionary<Sync.AttachSystem.Types, Sync.AttachSystem.Types>()
        {
            { Sync.AttachSystem.Types.ItemCigMouth, Sync.AttachSystem.Types.ItemCigHand },
            { Sync.AttachSystem.Types.ItemCig1Mouth, Sync.AttachSystem.Types.ItemCig1Hand },
            { Sync.AttachSystem.Types.ItemCig2Mouth, Sync.AttachSystem.Types.ItemCig2Hand },
            { Sync.AttachSystem.Types.ItemCig3Mouth, Sync.AttachSystem.Types.ItemCig3Hand },

            { Sync.AttachSystem.Types.ItemCigHand, Sync.AttachSystem.Types.ItemCigMouth },
            { Sync.AttachSystem.Types.ItemCig1Hand, Sync.AttachSystem.Types.ItemCig1Mouth },
            { Sync.AttachSystem.Types.ItemCig2Hand, Sync.AttachSystem.Types.ItemCig2Mouth },
            { Sync.AttachSystem.Types.ItemCig3Hand, Sync.AttachSystem.Types.ItemCig3Mouth },
        };

        public static List<Sync.AttachSystem.Types> AttachTypes { get; set; } = new List<Sync.AttachSystem.Types>(Cigarette.DependentTypes.Keys);

        new public class ItemData : StatusChanger.ItemData, Item.ItemData.IStackable
        {
            public const int UseCigModelIdx = 1;

            public int MaxAmount { get; set; }

            public int MaxPuffs { get; set; }

            public int MaxTime { get; set; }

            public Sync.AttachSystem.Types AttachType { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Mood}, {MaxAmount}";

            public ItemData(string Name, string[] Models, int Mood, int MaxPuffs, int MaxTime, Sync.AttachSystem.Types AttachType, int MaxAmount) : base(Name, 0.01f, Models, 0, Mood, 0)
            {
                this.MaxAmount = MaxAmount;

                this.MaxPuffs = MaxPuffs;
                this.MaxTime = MaxTime;

                this.AttachType = AttachType;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "cig_0", new ItemData("Сигарета Redwood", new string[] { "prop_cs_ciggy_01", "ng_proc_cigarette01a" }, 25, 15, 300000, Sync.AttachSystem.Types.ItemCigMouth, 20) },

            { "cig_1", new ItemData("Сигарета Chartman", new string[] { "prop_sh_cigar_01", "prop_sh_cigar_01" }, 25, 15, 300000, Sync.AttachSystem.Types.ItemCig1Mouth, 20) },

            { "cig_c_0", new ItemData("Сигара", new string[] { "prop_cigar_02", "prop_cigar_01" }, 25, 15, 300000, Sync.AttachSystem.Types.ItemCig2Mouth, 20) },

            { "cig_j_0", new ItemData("Косяк", new string[] { "p_cs_joint_02", "prop_sh_joint_01" }, 25, 15, 300000, Sync.AttachSystem.Types.ItemCig3Mouth, 20) },
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

            var moodDiff = Utils.GetCorrectDiff(pData.Mood, data.Mood, 0, 100);

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
