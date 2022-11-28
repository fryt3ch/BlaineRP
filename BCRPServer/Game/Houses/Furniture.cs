using GTANetworkAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace BCRPServer.Game.Houses
{
    public class Furniture
    {
        private static Queue<uint> FreeIDs { get; set; } = new Queue<uint>();

        public static Dictionary<uint, Furniture> All { get; private set; } = new Dictionary<uint, Furniture>();

        private static uint LastAddedMaxId { get; set; }

        public static uint MoveNextId()
        {
            uint id;

            if (!FreeIDs.TryDequeue(out id))
            {
                id = ++LastAddedMaxId;
            }

            return id;
        }

        public static void AddFreeId(uint id) => FreeIDs.Enqueue(id);

        public static void AddOnLoad(Furniture f)
        {
            if (f == null)
                return;

            All.Add(f.UID, f);

            if (f.UID > LastAddedMaxId)
                LastAddedMaxId = f.UID;
        }

        public static void Add(Furniture f)
        {
            if (f == null)
                return;

            All.Add(f.UID, f);

            MySQL.FurnitureAdd(f);
        }

        public static void Remove(Furniture f)
        {
            if (f == null)
                return;

            var id = f.UID;

            AddFreeId(id);

            All.Remove(id);

            MySQL.FurnitureDelete(f);
        }

        public static Furniture Get(uint id) => All.GetValueOrDefault(id);

        public enum Types
        {
            Default = 0,

            KitchenSet,
        }

        private static List<string> KitchenSetIDs { get; set; } = new List<string>()
        {

        };

        public uint UID { get; set; }

        public string ID { get; set; }

        public Types Type { get; set; }

        public FurnitureData Data { get; set; }

        public class FurnitureData
        {
            [JsonProperty(PropertyName = "P")]
            public Vector3 Position { get; set; }

            [JsonProperty(PropertyName = "R")]
            public Vector3 Rotation { get; set; }

            public FurnitureData() { }
        }

        public Furniture(uint UID, string ID, FurnitureData Data)
        {
            this.UID = UID;

            this.ID = ID;

            this.Data = Data;

            this.Type = GetType(ID);
        }

        public Furniture(string ID)
        {
            this.UID = MoveNextId();

            this.ID = ID;

            this.Type = GetType(ID);

            Data = new FurnitureData();

            Data.Position = new Vector3(0f, 0f, 0f);
            Data.Rotation = new Vector3(0f, 0f, 0f);

            Add(this);
        }

        public JObject ToClientJObject()
        {
            var obj = new JObject();

            obj.Add("U", UID);
            obj.Add("I", ID);

            obj.Add("PX", Data.Position.X);
            obj.Add("PY", Data.Position.Y);
            obj.Add("PZ", Data.Position.Z);

            obj.Add("RX", Data.Rotation.X);
            obj.Add("RY", Data.Rotation.Y);
            obj.Add("RZ", Data.Rotation.Z);

            return obj;
        }

        private static Types GetType(string id)
        {
            if (KitchenSetIDs.Contains(id))
            {
                return Types.KitchenSet;
            }

            return Types.Default;
        }
    }
}
