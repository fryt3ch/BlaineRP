using System.Collections.Generic;
using System.IO;

namespace BlaineRP.Server.Game.DoorSystem
{
    public static partial class Service
    {
        public static void InitializeAll()
        {
            if (AllDoors != null)
                return;

            AllDoors = new Dictionary<uint, Door>();

            new Door(1, "prison_prop_door2", 1780.352f, 2596.023f, 50.83891f, Properties.Settings.Static.MainDimension);

            var lines = new List<string>();

            foreach (var x in AllDoors)
            {
                lines.Add($"new Door({x.Key}, {x.Value.Model}, {x.Value.Position.ToCSharpStr()}, {x.Value.Dimension});");
            }

            Utils.FillFileToReplaceRegion(Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetPath + @"\Game\Management\Doors\Core.cs",
                "DOORS_TO_REPLACE",
                lines
            );
        }
    }
}