using Newtonsoft.Json;
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

            public bool RemovesWounded { get; set; }

            public bool RemovesKnocked { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Health}, {MaxAmount}";

            public ItemData(string Name, float Weight, string Model, int Health, bool RemovesWounded, bool RemovesKnocked, int MaxAmount, Sync.Animations.FastTypes Animation, Sync.AttachSystem.Types AttachType) : base(Name, Weight, new string[] { Model }, 0, 0, Health)
            {
                this.MaxAmount = MaxAmount;

                this.Animation = Animation;

                this.AttachType = AttachType;

                this.RemovesWounded = RemovesWounded;
                this.RemovesKnocked = RemovesKnocked;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "med_b_0", new ItemData("Бинт", 0.1f, "prop_gaffer_arm_bind", 10, true, false, 256, Sync.Animations.FastTypes.ItemBandage, Sync.AttachSystem.Types.ItemBandage) },

            { "med_kit_0", new ItemData("Аптечка", 0.25f, "prop_ld_health_pack", 50, false, false, 128, Sync.Animations.FastTypes.ItemMedKit, Sync.AttachSystem.Types.ItemMedKit) },
            { "med_kit_1", new ItemData("Аптечка ПП", 0.25f, "prop_ld_health_pack", 50, true, true, 128, Sync.Animations.FastTypes.ItemMedKit, Sync.AttachSystem.Types.ItemMedKit) },
            { "med_kit_2", new ItemData("Аптечка EMS", 0.25f, "prop_ld_health_pack", 85, true, true, 128, Sync.Animations.FastTypes.ItemMedKit, Sync.AttachSystem.Types.ItemMedKit) },
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

            player.AttachObject(data.Model, data.AttachType, Sync.Animations.FastTimeouts[data.Animation], null);

            pData.PlayAnim(data.Animation);

            if (data.RemovesWounded)
            {
                if (pData.IsWounded)
                    pData.IsWounded = false;
            }

            var hp = player.Health;

            var healthDiff = Utils.CalculateDifference(hp, data.Health, 0, 100);

            if (healthDiff != 0)
            {
                player.SetHealth(hp + healthDiff);
            }
        }

        public void ApplyToOther(PlayerData pData, PlayerData tData)
        {
            var player = pData.Player;
            var target = tData.Player;

            var data = Data;

            player.AttachObject(data.Model, data.AttachType, Sync.Animations.FastTimeouts[data.Animation], null);

            pData.PlayAnim(data.Animation);

            var hp = target.Health;

            var healthDiff = Utils.CalculateDifference(hp, data.Health, 0, 100);

            if (healthDiff != 0)
            {
                target.SetHealth(hp + healthDiff);
            }
        }

        public Healing(string ID) : base(ID, IDList[ID], typeof(Healing))
        {

        }
    }
}
