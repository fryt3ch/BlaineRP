using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Estates
{
    public partial class House
    {
        /// <summary>Метод для загрузки всех домов</summary>
        /// <returns>Кол-во загруженных домов</returns>
        public static int LoadAll()
        {
            new House(1, new Utils.Vector4(1724.771f, 4642.161f, 43.8755f, 115f), Style.RoomTypes.Two, 50000, Garage.Types.Two, new Utils.Vector4(1723.976f, 4630.187f, 42.84944f, 116.6f));

            var lines = new List<string>();

            foreach (var x in All.Values)
            {
                MySQL.LoadHouse(x);

                lines.Add($"new House({x.Id}, {x.PositionParams.Position.ToCSharpStr()}, Sync.House.Style.RoomTypes.{x.RoomType}, {(x.GarageData == null ? "null" : $"Garage.Types.{x.GarageData.Type}")}, {(x.GarageOutside == null ? "null" : x.GarageOutside.Position.ToCSharpStr())}, {x.Price}, HouseBase.ClassTypes.{x.Class}, {x.Tax});");
            }

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "HOUSES_TO_REPLACE", lines);

            return All.Count;
        }
    }
}
