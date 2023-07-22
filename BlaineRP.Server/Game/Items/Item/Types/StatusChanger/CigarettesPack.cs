using Newtonsoft.Json;
using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public class CigarettesPack : StatusChanger, IConsumable
    {
        new public class ItemData : StatusChanger.ItemData, Item.ItemData.IConsumable
        {
            public const int UseCigModelIdx = 1;

            public int MaxAmount { get; set; }

            public int MaxPuffs { get; set; }

            public int MaxTime { get; set; }

            public Sync.AttachSystem.Types AttachType { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Mood}, {MaxAmount}";

            public ItemData(string Name, string[] Models, int Mood, int MaxPuffs, int MaxTime, Sync.AttachSystem.Types AttachType, int MaxAmount) : base(Name, 0.1f, Models, 0, Mood, 0)
            {
                this.MaxAmount = MaxAmount;

                this.MaxPuffs = MaxPuffs;
                this.MaxTime = MaxTime;

                this.AttachType = AttachType;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "cigs_0", new ItemData("Сигареты Redwood", new string[] { "v_ret_ml_cigs", "ng_proc_cigarette01a" }, 25, 15, 300000, Sync.AttachSystem.Types.ItemCigMouth, 20) },

            { "cigs_1", new ItemData("Сигареты Chartman", new string[] { "prop_cigar_pack_01", "prop_sh_cigar_01" }, 25, 15, 300000, Sync.AttachSystem.Types.ItemCig1Mouth, 20) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; }

        [JsonIgnore]
        public int MaxAmount => Data.MaxAmount;

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

        public CigarettesPack(string ID) : base(ID, IDList[ID], typeof(CigarettesPack))
        {
            this.Amount = MaxAmount;
        }
    }
}
