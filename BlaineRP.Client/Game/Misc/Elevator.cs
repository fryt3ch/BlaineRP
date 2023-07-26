using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using RAGE;

namespace BlaineRP.Client.Game.Misc
{
    public partial class Elevator
    {
        public static Dictionary<uint, Elevator> All { get; set; } = new Dictionary<uint, Elevator>();

        private static Dictionary<HashSet<uint>, string> Names { get; set; } = new Dictionary<HashSet<uint>, string>()
        {
            { new HashSet<uint>() { 1, 4, }, "Служебный гараж" },
            { new HashSet<uint>() { 2, }, "Главный этаж" },
            { new HashSet<uint>() { 3, 8, }, "Вертолётная площадка" },
            { new HashSet<uint>() { 5, }, "Холл" },
            { new HashSet<uint>() { 6, }, "Этаж 49" },
            { new HashSet<uint>() { 7, }, "Этаж 53" },
            { new HashSet<uint>() { 9, }, "Этаж 47" },
        };

        public static Elevator Get(uint id)
        {
            return All.GetValueOrDefault(id);
        }

        public static string GetName(uint id)
        {
            return Names.Where(x => x.Key.Contains(id)).Select(x => x.Value).FirstOrDefault() ?? "null";
        }

        public uint[] LinkedElevators { get; set; }

        public Elevator(uint id, Utils.Vector4 position, float range, uint dimension, string linkedElevatorsJs)
        {
            LinkedElevators = RAGE.Util.Json.Deserialize<uint[]>(linkedElevatorsJs);

            if (All.TryAdd(id, this))
            {
                var cs = new Cylinder(new Vector3(position.Position.X, position.Position.Y, position.Position.Z - 1f), range, 1.5f, false, Utils.Misc.RedColor, dimension, null)
                {
                    Data = id, InteractionType = InteractionTypes.ElevatorInteract, ActionType = ActionTypes.ElevatorInteract,
                };
            }
        }
    }
}