using Newtonsoft.Json;
using System.Collections.Generic;
using BlaineRP.Server.Game.Animations;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Inventory;
using BlaineRP.Server.Sync;

namespace BlaineRP.Server.Game.Items
{
    public partial class MetalDetector : Item, IUsable
    {
        public new class ItemData : Item.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f";

            public ItemData(string name, float weight) : base(name, weight, "w_am_metaldetector")
            {

            }
        }

        [JsonIgnore]
        public new ItemData Data => (ItemData)base.Data;

        [JsonIgnore]
        public bool InUse { get; set; }

        public bool StartUse(PlayerData pData, GroupTypes group, int slot, bool needUpdate, params object[] args)
        {
            if (InUse)
                return false;

            InUse = true;

            pData.Player.AttachObject(Model, AttachmentType.ItemMetalDetector, -1, null);

            pData.PlayAnim(GeneralType.MetalDetectorProcess0);

            if (needUpdate && slot >= 0)
            {
                pData.Player.InventoryUpdate(group, slot, ToClientJson(group));
            }

            return true;
        }

        public bool StopUse(PlayerData pData, GroupTypes group, int slot, bool needUpdate, params object[] args)
        {
            if (!InUse)
                return false;

            InUse = false;

            pData.Player.DetachObject(AttachmentType.ItemMetalDetector);

            pData.StopGeneralAnim();

            if (needUpdate && slot >= 0)
            {
                pData.Player.InventoryUpdate(group, slot, ToClientJson(group));
            }

            return true;
        }

        public MetalDetector(string id) : base(id, IdList[id], typeof(MetalDetector))
        {

        }
    }
}