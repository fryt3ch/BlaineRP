using BlaineRP.Server.Game.Items.Craft;
using GTANetworkAPI;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public class Tent : PlaceableItem
    {
        new public class ItemData : PlaceableItem.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Model}";

            public ItemData(string Name, float Weight, string Model) : base(Name, Weight, Model)
            {

            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "tent_0", new ItemData("Палатка (серая)", 2f, "brp_p_tent_0_grey") },
            { "tent_1", new ItemData("Палатка (синяя)", 2f, "brp_p_tent_0_blue") },
            { "tent_2", new ItemData("Палатка (жёлтая)", 2f, "brp_p_tent_0_yellow") },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        public override void Delete()
        {
            base.Delete();
        }

        public override Sync.World.ItemOnGround Install(PlayerData pData, Vector3 pos, Vector3 rot)
        {
            var iog = base.Install(pData, pos, rot);

            return iog;
        }

        public override bool Remove(PlayerData pData)
        {
            base.Remove(pData);

            return true;
        }

        public Tent(string ID) : base(ID, IDList[ID], typeof(Tent))
        {

        }
    }
}