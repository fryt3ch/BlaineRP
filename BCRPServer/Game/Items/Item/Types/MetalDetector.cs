using Newtonsoft.Json;
using System.Collections.Generic;

namespace BCRPServer.Game.Items
{
    public class MetalDetector : Item, IUsable
    {
        new public class ItemData : Item.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f";

            public ItemData(string Name, float Weight) : base(Name, Weight, "w_am_metaldetector")
            {

            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "metaldet_0", new ItemData("Металлоискатель", 2f) },
        };

        [JsonIgnore]
        new public ItemData Data => (ItemData)base.Data;

        [JsonIgnore]
        public bool InUse { get; set; }

        public bool StartUse(PlayerData pData, Inventory.GroupTypes group, int slot, bool needUpdate, params object[] args)
        {
            if (InUse)
                return false;

            InUse = true;

            pData.Player.AttachObject(Model, Sync.AttachSystem.Types.ItemMetalDetector, -1, null);

            pData.PlayAnim(Sync.Animations.GeneralTypes.MetalDetectorProcess0);

            if (needUpdate && slot >= 0)
            {
                pData.Player.InventoryUpdate(group, slot, this.ToClientJson(group));
            }

            return true;
        }

        public bool StopUse(PlayerData pData, Inventory.GroupTypes group, int slot, bool needUpdate, params object[] args)
        {
            if (!InUse)
                return false;

            InUse = false;

            pData.Player.DetachObject(Sync.AttachSystem.Types.ItemMetalDetector);

            pData.StopGeneralAnim();

            if (needUpdate && slot >= 0)
            {
                pData.Player.InventoryUpdate(group, slot, this.ToClientJson(group));
            }

            return true;
        }

        public MetalDetector(string ID) : base(ID, IDList[ID], typeof(MetalDetector))
        {

        }
    }
}