using Newtonsoft.Json;
using System.Collections.Generic;

namespace BCRPServer.Game.Items
{
    public class Ring : Clothes, Clothes.IToggleable, Clothes.IProp
    {
        new public class ItemData : Clothes.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Model}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, string Model, string SexAlternativeID = null) : base(Name, 0.01f, Model, Sex, 1, new int[] { 0 }, SexAlternativeID)
            {

            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "ring_m_0", new ItemData("Золотое кольцо с бриллиантами", true, "brp_p_ring_0_0", "ring_f_0") },
            { "ring_m_1", new ItemData("Золотое кольцо с красным камнем", true, "brp_p_ring_1_0", "ring_f_1") },

            { "ring_f_0", new ItemData("Золотое кольцо с бриллиантами", false, "brp_p_ring_0_0", "ring_m_0") },
            { "ring_f_1", new ItemData("Золотое кольцо с красным камнем", false, "brp_p_ring_1_0", "ring_m_1") },
        };

        public const int Slot = int.MinValue;

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; set => base.SexAlternativeData = value; }

        public bool Toggled { get; set; }

        public void Action(PlayerData pData)
        {
            Unwear(pData);

            Toggled = !Toggled;

            Wear(pData);
        }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            player.AttachObject(Model, Toggled ? Sync.AttachSystem.Types.PedRingLeft3 : Sync.AttachSystem.Types.PedRingRight3, -1, null);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.DetachObject(Toggled ? Sync.AttachSystem.Types.PedRingLeft3 : Sync.AttachSystem.Types.PedRingRight3);
        }

        public Ring(string ID, int Variation = 0) : base(ID, IDList[ID], typeof(Ring), Variation)
        {

        }
    }
}
