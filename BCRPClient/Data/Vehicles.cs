using RAGE;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Data
{
    public class Vehicles : Events.Script
    {
        public enum Types
        {
            Car = 0,
            Moto,
        }

        public class Vehicle
        {
            public string Name { get; set; }
            public string ID { get; set; }
            public Types Type { get; set; }

            public string BrandName { get; set; }
            public string SubName { get; set; }

            public Vehicle(string ID, string Name, Types Type)
            {
                this.ID = ID;
                this.Name = Name;
                this.Type = Type;

                var divider = Name.IndexOf(' ');

                this.BrandName = Name.Substring(0, divider == -1 ? Name.Length : divider + 1);
                this.SubName = divider == -1 ? "" : Name.Substring(divider + 1);
            }
        }

        private static Dictionary<string, Vehicle> All;

        public Vehicles()
        {
            All = new Dictionary<string, Vehicle>()
            {
                { "buffalo", new Vehicle("buffalo", "Bravado Buffalo", Types.Car) }
            };
        }

        public static Vehicle Get(string id) => id == null ? null : All.ContainsKey(id) ? All[id] : null;
    }
}
