using System.Collections.Generic;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Misc
{
    public partial class VehicleDestruction
    {
        public static void InitializeAll()
        {
            All = new List<VehicleDestruction>()
            {
                new VehicleDestruction(new Vector3(2400.494f, 3108.046f, 47.17492f)),
                new VehicleDestruction(new Vector3(-458.2042f, -1715.724f, 17.6409f)),
                new VehicleDestruction(new Vector3(1284.828f, -2560.101f, 43.04144f)),
            };

            var lines = new List<string>();

            foreach (var x in All)
            {
                lines.Add($"new {nameof(BlaineRP.Client.Game.Misc.VehicleDestruction)}({x.Id}, {x.Position.ToCSharpStr()});");
            }

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() +
                                          Properties.Settings.Static.ClientScriptsTargetPath +
                                          @"\Game\Misc\VehicleDestruction.Initialization.cs",
                "TO_REPLACE",
                lines
            );
        }
    }
}