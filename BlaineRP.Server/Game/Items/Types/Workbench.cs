using GTANetworkAPI;
using Newtonsoft.Json;
using System.Collections.Generic;
using BlaineRP.Server.Game.Craft;
using BlaineRP.Server.Game.Craft.Workbenches;
using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Game.Items
{
    public partial class Workbench : PlaceableItem
    {
        public new class ItemData : PlaceableItem.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Model}";

            public WorkbenchTypes WorkbenchType { get; set; }

            public ItemData(string name, float weight, string model, WorkbenchTypes workbenchType) : base(name, weight, model)
            {
                WorkbenchType = workbenchType;
            }
        }

        [JsonIgnore]
        public new ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

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

        public override World.Service.ItemOnGround Install(PlayerData pData, Vector3 pos, Vector3 rot)
        {
            var iog = base.Install(pData, pos, rot);

            if (iog != null)
            {
                if (WorkbenchInstance != null)
                    return iog;

                var wrInstance = new ItemWorkbench(UID, Data.WorkbenchType, iog);
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

        public Workbench(string id) : base(id, IdList[id], typeof(Workbench))
        {

        }
    }
}
