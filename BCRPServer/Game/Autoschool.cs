using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Game
{
    public class Autoschool
    {
        public const byte MinimalOkAnswersTestPercentage = 80;

        public static List<Autoschool> All { get; private set; } = new List<Autoschool>();

        public int Id => All.IndexOf(this) + 1;

        public Vector3 Position { get; set; }

        public Dictionary<VehicleData.VehicleInfo, PlayerData.LicenseTypes> Vehicles { get; private set; }

        public Dictionary<PlayerData.LicenseTypes, Vector3[]> PracticeRoutes { get; private set; }

        public static Dictionary<PlayerData.LicenseTypes, uint> Prices { get; private set; } = new Dictionary<PlayerData.LicenseTypes, uint>()
        {
            { PlayerData.LicenseTypes.B, 1_000 },
            { PlayerData.LicenseTypes.C, 2_000 },
            { PlayerData.LicenseTypes.A, 3_000 },
            { PlayerData.LicenseTypes.D, 4_000 },
            { PlayerData.LicenseTypes.Sea, 4_000 },
            { PlayerData.LicenseTypes.Fly, 5_000 },
        };

        public Autoschool(Vector3 Position)
        {
            this.Position = Position;

            All.Add(this);
        }

        public static Autoschool Get(int id) => id < 1 || id > All.Count ? null : All[id - 1];

        public static void InitializeAll()
        {
            var lines = new List<string>();

            var col1 = new Utils.Colour(0, 0, 255, 255);
            var col2 = new Utils.Colour(0, 0, 255, 255);

            var vehicle1 = Game.Data.Vehicles.GetData("intruder");

            new Autoschool(new Vector3(225.5257f, 365.0517f, 105.0235f))
            {
                Vehicles = new Dictionary<VehicleData.VehicleInfo, PlayerData.LicenseTypes>()
                {
                    { VehicleData.NewAutoschool(1, vehicle1, col1, col2, new Utils.Vector4(209.1577f, 374.8885f, 106.5878f, 345.0295f), Utils.Dimensions.Main), PlayerData.LicenseTypes.B },
                    { VehicleData.NewAutoschool(1, vehicle1, col1, col2, new Utils.Vector4(204.6213f, 376.2432f, 106.8394f, 345.0295f), Utils.Dimensions.Main), PlayerData.LicenseTypes.B },
                },

                PracticeRoutes = new Dictionary<PlayerData.LicenseTypes, Vector3[]>()
                {
                    {
                        PlayerData.LicenseTypes.B,

                        new Vector3[]
                        {
                            new Vector3(205.2294f, 362.9645f, 106.1131f),
                            new Vector3(100.4652f, 344.27f, 112.2379f),
                            new Vector3(35.4907f, 277.5542f, 109.1398f),
                        }
                    }
                },
            };

            lines.Add($"Autoschool.Prices = RAGE.Util.Json.Deserialize<Dictionary<Sync.Players.LicenseTypes, uint>>(\"{Prices.SerializeToJson().Replace('\"', '\'')}\");");

            foreach (var x in All)
            {
                var avgVehiclePoses = x.Vehicles.GroupBy(x => x.Value).ToDictionary(x => x.Key, x => { var allPos = x.Select(y => y.Key.LastData.Position).ToList(); var pos = Utils.ZeroVector; foreach (var p in allPos) pos += p; return pos / allPos.Count; });

                lines.Add($"new Autoschool({x.Position.ToCSharpStr()}, \"{x.PracticeRoutes.SerializeToJson().Replace('\"', '\'')}\", \"{avgVehiclePoses.SerializeToJson().Replace('\"', '\'')}\");");
            }

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "DRIVINGSCHOOLS_TO_REPLACE", lines);
        }

        public static PlayerData.LicenseTypes GetLicenseTypeForPracticeRoute(PlayerData.LicenseTypes licType) => licType == PlayerData.LicenseTypes.B || licType == PlayerData.LicenseTypes.A || licType == PlayerData.LicenseTypes.C || licType == PlayerData.LicenseTypes.D ? PlayerData.LicenseTypes.B : licType;
    }
}
