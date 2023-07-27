using Newtonsoft.Json;
using System.Collections.Generic;
using BlaineRP.Server.UtilsT;
using BlaineRP.Server.UtilsT.UidHandlers;

namespace BlaineRP.Server.Game.Estates
{
    public partial class Furniture
    {
        public static Dictionary<uint, Furniture> All { get; private set; } = new Dictionary<uint, Furniture>();

        public static UInt32 UidHandler { get; private set; } = new UInt32(1);

        public static void AddOnLoad(Furniture f)
        {
            if (f == null)
                return;

            All.Add(f.UID, f);

            UidHandler.TryUpdateLastAddedMaxUid(f.UID);
        }

        public static void Add(Furniture f)
        {
            if (f == null)
                return;

            f.UID = UidHandler.MoveNextUid();

            All.Add(f.UID, f);

            MySQL.FurnitureAdd(f);
        }

        public static void Remove(Furniture f)
        {
            if (f == null)
                return;

            var id = f.UID;

            UidHandler.SetUidAsFree(id);

            All.Remove(id);

            MySQL.FurnitureDelete(f);
        }

        public static Furniture Get(uint id) => All.GetValueOrDefault(id);

        [JsonProperty(PropertyName = "U")]
        public uint UID { get; set; }

        [JsonProperty(PropertyName = "I")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "D")]
        public Vector4 Data { get; set; }

        [JsonIgnore]
        public ItemData FurnitureData => ItemData.Get(ID);

        [JsonIgnore]
        public Game.Items.Craft.FurnitureWorkbench WorkbenchInstance => Game.Items.Craft.FurnitureWorkbench.Get(UID);

        public Furniture(uint UID, string ID, Vector4 Data)
        {
            this.UID = UID;

            this.ID = ID;

            this.Data = Data;
        }

        public Furniture(string ID)
        {
            this.ID = ID;

            Data = new Vector4(0f, 0f, 0f, 0f);

            Add(this);
        }

        public void Setup(Game.Estates.HouseBase houseBase)
        {
            if (FurnitureData.WorkbenchType is Items.Craft.Workbench.WorkbenchTypes wbType)
            {
                if (WorkbenchInstance != null)
                    return;

                var wb = new Game.Items.Craft.FurnitureWorkbench(UID, houseBase, wbType);
            }
        }

        public void Delete(Game.Estates.HouseBase houseBase)
        {
            if (FurnitureData.WorkbenchType is Items.Craft.Workbench.WorkbenchTypes wbType)
            {
                if (WorkbenchInstance is Game.Items.Craft.FurnitureWorkbench furnWb)
                {
                    furnWb.DropAllItemsToGround(Data.Position, Utils.ZeroVector, houseBase.Dimension);

                    furnWb.Delete();
                }
            }
        }
    }
}
