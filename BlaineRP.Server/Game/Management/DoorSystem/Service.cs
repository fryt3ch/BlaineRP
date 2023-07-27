using System.Collections.Generic;
using System.IO;
using BlaineRP.Server.Game.World;

namespace BlaineRP.Server.Game.Management.DoorSystem
{
    public static partial class Service
    {
        public static Dictionary<uint, Door> AllDoors { get; set; }

        public static Door GetDoorById(uint id) => AllDoors.GetValueOrDefault(id);
    }
}
