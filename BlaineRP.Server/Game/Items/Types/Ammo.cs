using Newtonsoft.Json;
using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class Ammo : Item, IStackable
    {
        public new class ItemData : Item.ItemData, Item.ItemData.IStackable
        {
            public int MaxAmount { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {MaxAmount}";

            public ItemData(string name, float weight, string model, int maxAmount = 1024) : base(name, weight, model)
            {
                MaxAmount = maxAmount;
            }
        }

        public static ItemData GetData(string id) => (ItemData)IdList[id];

        [JsonIgnore]
        public new ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        public override float Weight { get => Amount * BaseWeight; }

        [JsonIgnore]
        public int MaxAmount => Data.MaxAmount;

        public int Amount { get; set; }

        public Ammo(string id) : base(id, IdList[id], typeof(Ammo))
        {

        }
    }
}
