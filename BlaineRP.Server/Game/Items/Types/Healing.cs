using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using BlaineRP.Server.Game.Animations;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Sync;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Items
{
    public partial class Healing : StatusChanger, IStackable
    {
        public new class ItemData : StatusChanger.ItemData, Item.ItemData.IStackable
        {
            public int MaxAmount { get; set; }

            public FastType Animation { get; set; }

            public AttachmentType AttachType { get; set; }

            public double ResurrectionChance { get; set; }

            public bool ClearWoundedState { get; set; }

            public TimeSpan ResurrectionTime { get; set; }

            public TimeSpan UsageTime { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Health}, {MaxAmount}";

            public ItemData(string name, float weight, string model, int health, bool clearWoundedState, double resurrectiondChance, int maxAmount, TimeSpan usageTime, FastType animation, AttachmentType attachType) : base(name, weight, new string[] { model }, 0, 0, health)
            {
                MaxAmount = maxAmount;

                Animation = animation;

                AttachType = attachType;

                ClearWoundedState = clearWoundedState;
                ResurrectionChance = resurrectiondChance;

                UsageTime = usageTime;
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

            player.AttachObject(data.Model, data.AttachType, (int)data.UsageTime.TotalMilliseconds, null);

            pData.PlayAnim(data.Animation, data.UsageTime);

            if (data.ClearWoundedState)
            {
                if (pData.IsWounded)
                    pData.IsWounded = false;
            }

            var hp = player.Health;

            var healthDiff = Utils.CalculateDifference(hp, data.Health, 0, Properties.Settings.Static.PlayerMaxHealth);

            if (healthDiff != 0)
            {
                player.SetHealth(hp + healthDiff);
            }
        }

        public void ResurrectPlayer(PlayerData pData, PlayerData tData, double? overrideResurrectChance = null, TimeSpan? overrideResurrectTime = null)
        {
            var data = Data;

            var resurrectionChance = overrideResurrectChance ?? data.ResurrectionChance;

            var result = resurrectionChance <= 0d ? false : SRandom.NextDoubleS() <= resurrectionChance;

            pData.Player.AttachEntity(tData.Player, AttachmentType.PlayerResurrect, $"{(int)data.ResurrectionTime.TotalMilliseconds}_{(result ? 1 : 0)}_{0}");
        }

        public Healing(string id) : base(id, IdList[id], typeof(Healing))
        {

        }
    }
}
