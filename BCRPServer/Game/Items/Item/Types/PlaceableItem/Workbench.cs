using BCRPServer.Game.Items.Craft;
using GTANetworkAPI;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BCRPServer.Game.Items
{
    public class Workbench : PlaceableItem
    {
        new public class ItemData : PlaceableItem.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Model}";

            public Craft.Workbench.WorkbenchTypes WorkbenchType { get; set; }

            public ItemData(string Name, float Weight, string Model, Craft.Workbench.WorkbenchTypes WorkbenchType) : base(Name, Weight, Model)
            {
                this.WorkbenchType = WorkbenchType;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "pwb_bbq_0", new ItemData("Гриль (компактный)", 5f, "prop_bbq_4", Craft.Workbench.WorkbenchTypes.Grill) },
            { "pwb_bbq_1", new ItemData("Гриль (большой)", 10f, "prop_bbq_5", Craft.Workbench.WorkbenchTypes.Grill) },

            { "pwb_bbq_2", new ItemData("Костёр", 1f, "prop_beach_fire", Craft.Workbench.WorkbenchTypes.Grill) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        public ItemWorkbench WorkbenchInstance => ItemWorkbench.Get(UID);

        public override void Delete()
        {
            if (WorkbenchInstance is ItemWorkbench wbInstance)
            {
                wbInstance.Delete();
            }

            base.Delete();
        }

        public override Sync.World.ItemOnGround Install(PlayerData pData, Vector3 pos, Vector3 rot)
        {
            var iog = base.Install(pData, pos, rot);

            if (iog != null)
            {
                if (WorkbenchInstance != null)
                    return iog;

                var wrInstance = new Craft.ItemWorkbench(UID, Data.WorkbenchType, iog);
            }

            return iog;
        }

        public override bool Remove(PlayerData pData)
        {
            if (WorkbenchInstance is ItemWorkbench wbInstance)
            {
                if (wbInstance.OwnerEntity.Object?.Exists == true)
                {
                    wbInstance.DropAllItemsToGround(wbInstance.OwnerEntity.Object.Position, wbInstance.OwnerEntity.Object.Rotation, wbInstance.OwnerEntity.Object.Dimension);
                }

                wbInstance.Delete();
            }
            else
            {
                return false;
            }

            base.Remove(pData);

            return true;
        }

        public Workbench(string ID) : base(ID, IDList[ID], typeof(Workbench))
        {

        }
    }
}
