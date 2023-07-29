using Newtonsoft.Json;
using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class WeaponSkin : Item
    {
        public new class ItemData : Item.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, WeaponSkin.ItemData.Types.{Type}";

            public enum Types
            {
                UniDef = 0,
                UniMk2,
            }

            public Types Type { get; set; }

            public int Variation { get; set; }

            public ItemData(string name, float weight, string model, Types type, int variation) : base(name, weight, model)
            {
                Type = type;
                Variation = variation;
            }
        }

        public static ItemData GetData(string id) => (ItemData)IdList[id];

        [JsonIgnore]
        public new ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        public override float Weight { get => Amount * BaseWeight; }

        public int Amount { get; set; }

        public WeaponSkin(string id) : base(id, IdList[id], typeof(WeaponSkin))
        {

        }
    }
}
