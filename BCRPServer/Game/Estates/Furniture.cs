using GTANetworkAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace BCRPServer.Game.Estates
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

        [JsonProperty(PropertyName = "U")]
        public uint UID { get; set; }

        [JsonProperty(PropertyName = "I")]
        public string ID { get; set; }

        [JsonIgnore]
        public Types Type { get; set; }

        [JsonProperty(PropertyName = "D")]
        public Utils.Vector4 Data { get; set; }

        public Furniture(uint UID, string ID, Utils.Vector4 Data)
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

            Data = new Utils.Vector4(0f, 0f, 0f, 0f);

            Add(this);
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
