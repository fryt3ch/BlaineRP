using System.Collections.Generic;

namespace BCRPServer.Game.Estates
{
    public partial class Apartments
    {
        public partial class ApartmentsRoot
        {
            public static void LoadAll()
            {
                new ApartmentsRoot(Types.Cheap1, new Utils.Vector4(-150.497f, 6416.68f, 31.9159f, 45f), new Utils.Vector4(696.9186f, 1299.008f, -186.5668f, 92f), 10, new Utils.Vector4(697.21f, 1302.987f, -186.57f, 181.7173f), 3.7f, 1);

                var lines = new List<string>();

                foreach (var x in All.Values)
                {
                    lines.Add($"new ApartmentsRoot(ApartmentsRoot.Types.{x.Type.ToString()}, {x.EnterParams.Position.ToCSharpStr()}, {x.ExitParams.Position.ToCSharpStr()}, {x.FloorsAmount}, {x.FloorPosition.Position.ToCSharpStr()}, {x.FloorDistZ}f, {x.StartFloor});");
                }

                Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "AROOTS_TO_REPLACE", lines);
            }
        }

        /// <summary>Метод для загрузки всех квартир</summary>
        /// <returns>Кол-во загруженных квартир</returns>
        public static int LoadAll()
        {
            ApartmentsRoot.LoadAll();

            new Apartments(1, new Utils.Vector4(692.4949f, 1293.174f, -186.5667f, 273.5f), ApartmentsRoot.Types.Cheap1, Style.RoomTypes.Two, 50000);

            var lines = new List<string>();

            foreach (var x in All.Values)
            {
                MySQL.LoadApartments(x);

                lines.Add($"new Apartments({x.Id}, {x.PositionParams.Position.ToCSharpStr()}, ApartmentsRoot.Types.{x.Root.Type.ToString()}, Sync.House.Style.RoomTypes.{x.RoomType.ToString()}, {x.Price}, HouseBase.ClassTypes.{x.Class}, {x.Tax});");
            }

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "APARTMENTS_TO_REPLACE", lines);

            return All.Count;
        }
    }
}
