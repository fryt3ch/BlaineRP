using System.Collections.Generic;

namespace BlaineRP.Server.Game.Estates
{
    public partial class Garage
    {
        public static void LoadAll()
        {
            GarageRoot.LoadAll();

            new Garage(1, 3, Types.Two, 0, 25_000);

            var lines = new List<string>();

            foreach (var x in All.Values)
            {
                MySQL.LoadGarage(x);

                lines.Add($"new Garage({x.Id}, {x.Root.Id}, {(int)x.StyleData.Type}, {x.Variation}, {(int)x.ClassType}, {x.Tax}, {x.Price});");
            }

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetPath + @"\Game\Estates\Initialization.cs",
                "GARAGES_TO_REPLACE",
                lines
            );
        }
    }
}