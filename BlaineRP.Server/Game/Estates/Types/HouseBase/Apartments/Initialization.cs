using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Server.Game.Estates
{
    public partial class Apartments
    {
        public partial class ApartmentsRoot
        {
            public static void LoadAll()
            {
                new ApartmentsRoot(1, ShellTypes.MediumEnd_0, new Utils.Vector4(-150.497f, 6416.68f, 31.9159f, 45f));

                new ApartmentsRoot(2, ShellTypes.HighEnd_0, new Utils.Vector4(-777.3051f, 312.1409f, 85.69813f, 176.3586f));
                new ApartmentsRoot(3, ShellTypes.HighEnd_0, new Utils.Vector4(-1442.538f, -545.4138f, 34.74182f, 213.3717f));
                new ApartmentsRoot(4, ShellTypes.HighEnd_0, new Utils.Vector4(-47.95924f, -585.7656f, 37.95282f, 69.66584f));
                new ApartmentsRoot(5, ShellTypes.HighEnd_0, new Utils.Vector4(-619.2709f, 36.8526f, 43.57647f, 176.5841f));

                new ApartmentsRoot(6, ShellTypes.MediumEnd_0, new Utils.Vector4(-827.6624f, -696.8757f, 28.05656f, 85.35796f));

                new ApartmentsRoot(7, ShellTypes.LowEnd_2_0, new Utils.Vector4(-35.08994f, -1554.494f, 30.67673f, 319.0496f));

                new ApartmentsRoot(8, ShellTypes.LowEnd_2_0, new Utils.Vector4(-1255.122f, -1330.628f, 4.080747f, 291.2249f));

                new ApartmentsRoot(9, ShellTypes.LowEnd_1_0, new Utils.Vector4(1658.132f, 4851.235f, 41.97267f, 277.1076f));

                new ApartmentsRoot(10, ShellTypes.LowEnd_5_0, new Utils.Vector4(278.7079f, -1119.274f, 29.41967f, 176.6636f));

                var lines = new List<string>();

                foreach (var x in ApartmentsRoot.Shells)
                {
                    lines.Add($"ApartmentsRoot.AddShell({(byte)x.Key}, {x.Value.StartFloor}, {x.Value.FloorsAmount}, {x.Value.EnterPosition.Position.ToCSharpStr()}, \"{x.Value.ElevatorPositions.Select(x => x.Select(y => y.Position).ToList()).ToList().SerializeToJson().Replace('\"', '\'')}\", \"{x.Value.ApartmentsPositions.Select(x => x.Select(y => y.Position).ToList()).ToList().SerializeToJson().Replace('\"', '\'')}\");");
                }

                foreach (var x in All.Values)
                {
                    lines.Add($"new ApartmentsRoot({x.Id}, {(byte)x.ShellType}, {x.EnterParams.Position.ToCSharpStr()});");
                }

                Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetLocationsLoaderPath, "AROOTS_TO_REPLACE", lines);
            }
        }

        /// <summary>Метод для загрузки всех квартир</summary>
        /// <returns>Кол-во загруженных квартир</returns>
        public static int LoadAll()
        {
            ApartmentsRoot.LoadAll();

            new Apartments(1, 1, 0, 0, Style.RoomTypes.Two, 50000);

            var lines = new List<string>();

            foreach (var x in All.Values)
            {
                MySQL.LoadApartments(x);

                lines.Add($"new Apartments({x.Id}, {x.RootId}, {x.FloorIdx}, {x.SubIdx}, Sync.House.Style.RoomTypes.{x.RoomType.ToString()}, {x.Price}, HouseBase.ClassTypes.{x.Class}, {x.Tax});");
            }

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetLocationsLoaderPath, "APARTMENTS_TO_REPLACE", lines);

            return All.Count;
        }
    }
}
