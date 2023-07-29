using Newtonsoft.Json;
using System.Collections.Generic;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Sync;

namespace BlaineRP.Server.Game.Items
{
    public partial class CigarettesPack : StatusChanger, IConsumable
    {
        public new class ItemData : StatusChanger.ItemData, Item.ItemData.IConsumable
        {
            public const int UseCigModelIdx = 1;

            public int MaxAmount { get; set; }

            public int MaxPuffs { get; set; }

            public int MaxTime { get; set; }

            public AttachmentType AttachType { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Mood}, {MaxAmount}";

            public ItemData(string name, string[] models, int mood, int maxPuffs, int maxTime, AttachmentType attachType, int maxAmount) : base(name, 0.1f, models, 0, mood, 0)
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

        public CigarettesPack(string id) : base(id, IdList[id], typeof(CigarettesPack))
        {
            Amount = MaxAmount;
        }
    }
}
