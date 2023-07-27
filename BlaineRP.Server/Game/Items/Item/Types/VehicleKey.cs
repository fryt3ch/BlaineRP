using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using BlaineRP.Server.EntitiesData.Vehicles;

namespace BlaineRP.Server.Game.Items
{
    public class VehicleKey : Item, ITaggedFull
    {
        new public class ItemData : Item.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f";

            public ItemData(string Name, float Weight, string Model) : base(Name, Weight, Model) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "vk_0", new ItemData("Ключ от транспорта", 0.01f, "p_car_keys_01") },
        };

        [JsonIgnore]
        public VehicleInfo VehicleInfo => VehicleInfo.Get(VID);

        public bool IsKeyValid(VehicleInfo vInfo) => KeysUid != Guid.Empty && vInfo?.VID == VID && vInfo.KeysUid == KeysUid;

        public string Tag { get; set; }

        [JsonProperty(PropertyName = "VID")]
        public uint VID { get; set; }

        [JsonProperty(PropertyName = "KUID")]
        public Guid KeysUid { get; set; }

        public VehicleKey(string ID) : base(ID, IDList[ID], typeof(VehicleKey))
        {

        }
    }
}
