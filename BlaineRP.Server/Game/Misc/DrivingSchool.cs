using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.EntityData.Players;
using BlaineRP.Server.EntityData.Vehicles;

namespace BlaineRP.Server.Game
{
    public class Autoschool
    {
        public const byte MinimalOkAnswersTestPercentage = 80;

        public static List<Autoschool> All { get; private set; } = new List<Autoschool>();

        public int Id => All.IndexOf(this) + 1;

        public Vector3 Position { get; set; }

        public Dictionary<VehicleInfo, LicenseTypes> Vehicles { get; private set; }

        public Dictionary<LicenseTypes, Vector3[]> PracticeRoutes { get; private set; }

        [Properties.Settings.Static.ClientSync("drivingSchoolLicensePrices")]
        public static Dictionary<LicenseTypes, uint> Prices { get; private set; } = new Dictionary<LicenseTypes, uint>()
        {
            { LicenseTypes.B, 1_000 },
            { LicenseTypes.C, 2_000 },
            { LicenseTypes.A, 3_000 },
            { LicenseTypes.D, 4_000 },
            { LicenseTypes.Sea, 4_000 },
            { LicenseTypes.Fly, 5_000 },
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

            new Autoschool(new Vector3(208.6196f, -1382.992f, 29.58354f))
            {
                Vehicles = new Dictionary<VehicleInfo, LicenseTypes>()
                {
                    { VehicleData.NewAutoschool(1, vehicle1, col1, col2, new Utils.Vector4(208.6979f, -1373.918f, 30.17774f, 230.5807f), Properties.Settings.Static.MainDimension), LicenseTypes.B },
                    { VehicleData.NewAutoschool(1, vehicle1, col1, col2, new Utils.Vector4(211.0172f, -1371.398f, 30.17699f, 232.7517f), Properties.Settings.Static.MainDimension), LicenseTypes.B },
                    { VehicleData.NewAutoschool(1, vehicle1, col1, col2, new Utils.Vector4(213.2036f, -1369.059f, 30.17646f, 231.8667f), Properties.Settings.Static.MainDimension), LicenseTypes.B },
                    { VehicleData.NewAutoschool(1, vehicle1, col1, col2, new Utils.Vector4(214.5865f, -1363.528f, 30.17337f, 229.6534f), Properties.Settings.Static.MainDimension), LicenseTypes.B },
                    { VehicleData.NewAutoschool(1, vehicle1, col1, col2, new Utils.Vector4(216.8003f, -1360.368f, 30.17301f, 231.4579f), Properties.Settings.Static.MainDimension), LicenseTypes.B },
                    { VehicleData.NewAutoschool(1, vehicle1, col1, col2, new Utils.Vector4(218.7388f, -1357.639f, 30.17165f, 232.5897f), Properties.Settings.Static.MainDimension), LicenseTypes.B },
                },

                PracticeRoutes = new Dictionary<LicenseTypes, Vector3[]>()
                {
                    {
                        LicenseTypes.B,

                        new Vector3[]
                        {
                            new Vector3(205.2294f, 362.9645f, 106.1131f),
                            new Vector3(100.4652f, 344.27f, 112.2379f),
                            new Vector3(35.4907f, 277.5542f, 109.1398f),
                        }
                    }
                },
            };

            foreach (var x in All)
            {
                var avgVehiclePoses = x.Vehicles.GroupBy(x => x.Value).ToDictionary(x => x.Key, x => { var allPos = x.Select(y => y.Key.LastData.Position).ToList(); var pos = Utils.ZeroVector; foreach (var p in allPos) pos += p; return pos / allPos.Count; });

                lines.Add($"new {nameof(BlaineRP.Client.Game.Misc.Autoschool)}({x.Position.ToCSharpStr()}, \"{x.PracticeRoutes.SerializeToJson().Replace('\"', '\'')}\", \"{avgVehiclePoses.SerializeToJson().Replace('\"', '\'')}\");");
            }

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetPath + @"\Game\Misc\Autoschool.Initialization.cs", "TO_REPLACE", lines);
        }

        public static LicenseTypes GetLicenseTypeForPracticeRoute(LicenseTypes licType) => licType == LicenseTypes.B || licType == LicenseTypes.A || licType == LicenseTypes.C || licType == LicenseTypes.D ? LicenseTypes.B : licType;
    }
}
