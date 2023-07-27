using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Server.Game.Estates
{
    public partial class Apartments
    {
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

                lines.Add($"new Apartments({x.Id}, {x.RootId}, {x.FloorIdx}, {x.SubIdx}, {(int)x.RoomType}, {x.Price}, {(int)x.Class}, {x.Tax});");
            }

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetPath + @"\Game\Estates\Initialization.cs", "APARTMENTS_TO_REPLACE", lines);

            return All.Count;
        }
    }
}
