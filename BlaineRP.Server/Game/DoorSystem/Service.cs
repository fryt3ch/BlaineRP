using System.Collections.Generic;

namespace BlaineRP.Server.Game.DoorSystem
{
    public static partial class Service
    {
        public static Dictionary<uint, Door> AllDoors { get; set; }

        public static Door GetDoorById(uint id) => AllDoors.GetValueOrDefault(id);
    }
}
