using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BCRPServer.Game.Items
{
    public class Healing : StatusChanger, IStackable
    {
        new public class ItemData : StatusChanger.ItemData, Item.ItemData.IStackable
        {
            public int MaxAmount { get; set; }

            public Sync.Animations.FastTypes Animation { get; set; }

            public Sync.AttachSystem.Types AttachType { get; set; }

            public double ResurrectionChance { get; set; }

            public bool ClearWoundedState { get; set; }

            public TimeSpan ResurrectionTime { get; set; }

            public TimeSpan UsageTime { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Health}, {MaxAmount}";

            public ItemData(string Name, float Weight, string Model, int Health, bool ClearWoundedState, double ResurrectiondChance, int MaxAmount, TimeSpan UsageTime, Sync.Animations.FastTypes Animation, Sync.AttachSystem.Types AttachType) : base(Name, Weight, new string[] { Model }, 0, 0, Health)
            {
                this.MaxAmount = MaxAmount;

                this.Animation = Animation;

                this.AttachType = AttachType;

                this.ClearWoundedState = ClearWoundedState;
                this.ResurrectionChance = ResurrectiondChance;

                this.UsageTime = UsageTime;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "med_b_0", new ItemData("Бинт", 0.1f, "prop_gaffer_arm_bind", 10, true, -1.0d, 64, TimeSpan.FromMilliseconds(4_000), Sync.Animations.FastTypes.ItemBandage, Sync.AttachSystem.Types.ItemBandage) },

            { "med_kit_0", new ItemData("Аптечка", 0.25f, "prop_ld_health_pack", 50, false, 0.50d, 3, TimeSpan.FromMilliseconds(7_000), Sync.Animations.FastTypes.ItemMedKit, Sync.AttachSystem.Types.ItemMedKit) { ResurrectionTime = TimeSpan.FromSeconds(10), } },
            { "med_kit_ems_0", new ItemData("Аптечка EMS", 0.25f, "prop_ld_health_pack", Properties.Settings.Static.PlayerMaxHealth, true, 0.75d, 64, TimeSpan.FromMilliseconds(7_000), Sync.Animations.FastTypes.ItemMedKit, Sync.AttachSystem.Types.ItemMedKit) { ResurrectionTime = TimeSpan.FromSeconds(8), } },
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

            pData.Player.AttachEntity(tData.Player, Sync.AttachSystem.Types.PlayerResurrect, $"{(int)data.ResurrectionTime.TotalMilliseconds}_{(result ? 1 : 0)}_{0}");
        }

        public Healing(string ID) : base(ID, IDList[ID], typeof(Healing))
        {

        }
    }
}
