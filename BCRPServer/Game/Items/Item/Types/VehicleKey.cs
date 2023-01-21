using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Items
{
    public class VehicleKey : Item, ITagged
    {
        new public class ItemData : Item.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f";

            public ItemData(string Name, float Weight, string Model) : base(Name, Weight, Model) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "vk_0", new ItemData("Ключ", 0.01f, "p_car_keys_01") },
        };

        [JsonIgnore]
        public VehicleData.VehicleInfo VehicleInfo => VehicleData.VehicleInfo.Get(VID);

        public bool IsKeyValid(VehicleData.VehicleInfo vInfo) => (vInfo == null || vInfo.VID != VID) ? false : vInfo.AllKeys.Contains(UID);

        public string Tag { get; set; }

        public uint VID { get; set; }

        public VehicleKey(string ID) : base(ID, IDList[ID], typeof(VehicleKey))
        {

        }
    }
}
