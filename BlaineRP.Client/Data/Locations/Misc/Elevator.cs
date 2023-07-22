using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlaineRP.Client.Data
{
    public partial class Locations
    {
        public class Elevator
        {
            public static Dictionary<uint, Elevator> All { get; set; } = new Dictionary<uint, Elevator>();

            private static Dictionary<HashSet<uint>, string> Names { get; set; } = new Dictionary<HashSet<uint>, string>()
            {
                { new HashSet<uint>() { 1, 4 }, "Служебный гараж" },
                { new HashSet<uint>() { 2 }, "Главный этаж" },
                { new HashSet<uint>() { 3, 8 }, "Вертолётная площадка" },
                { new HashSet<uint>() { 5, }, "Холл" },
                { new HashSet<uint>() { 6, }, "Этаж 49" },
                { new HashSet<uint>() { 7, }, "Этаж 53" },
                { new HashSet<uint>() { 9, }, "Этаж 47" },
            };

            public static Elevator Get(uint id) => All.GetValueOrDefault(id);

            public static string GetName(uint id) => Names.Where(x => x.Key.Contains(id)).Select(x => x.Value).FirstOrDefault() ?? "null";

            public uint[] LinkedElevators { get; set; }

            public Elevator(uint Id, Utils.Vector4 Position, float Range, uint Dimension, string LinkedElevatorsJs)
            {
                this.LinkedElevators = RAGE.Util.Json.Deserialize<uint[]>(LinkedElevatorsJs);

                if (All.TryAdd(Id, this))
                {
                    var cs = new Additional.Cylinder(new Vector3(Position.Position.X, Position.Position.Y, Position.Position.Z - 1f), Range, 1.5f, false, Utils.RedColor, Dimension, null)
                    {
                        Data = Id,

                        InteractionType = Additional.ExtraColshape.InteractionTypes.ElevatorInteract,

                        ActionType = Additional.ExtraColshape.ActionTypes.ElevatorInteract,
                    };
                }
            }
        }
    }
}