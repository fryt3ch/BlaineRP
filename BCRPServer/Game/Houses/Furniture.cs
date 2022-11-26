using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Houses
{
    class Furniture : Script
    {
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

        public class Data
        {
            [JsonProperty(PropertyName = "P")]
            public Vector3 Position { get; set; }

            [JsonProperty(PropertyName = "R")]
            public Vector3 Rotation { get; set; }

            public Data() { }
        }

        public Furniture(string ID)
        {
            this.ID = ID;

            if (KitchenSetIDs.Contains(ID))
            {
                Type = Types.KitchenSet;
            }
            else
            {
                Type = Types.Default;
            }
        }
    }
}
