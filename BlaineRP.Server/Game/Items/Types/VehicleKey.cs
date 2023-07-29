using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using BlaineRP.Server.Game.EntitiesData.Vehicles;

namespace BlaineRP.Server.Game.Items
{
    public partial class VehicleKey : Item, ITaggedFull
    {
        public new class ItemData : Item.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f";

            public ItemData(string name, float weight, string model) : base(name, weight, model) { }
        }

        [JsonIgnore]
        public VehicleInfo VehicleInfo => VehicleInfo.Get(Vid);

        public bool IsKeyValid(VehicleInfo vInfo) => KeysUid != Guid.Empty && vInfo?.VID == Vid && vInfo.KeysUid == KeysUid;

        public string Tag { get; set; }

        [JsonProperty(PropertyName = "VID")]
        public uint Vid { get; set; }

        [JsonProperty(PropertyName = "KUID")]
        public Guid KeysUid { get; set; }

        public VehicleKey(string id) : base(id, IdList[id], typeof(VehicleKey))
        {

        }
    }
}
