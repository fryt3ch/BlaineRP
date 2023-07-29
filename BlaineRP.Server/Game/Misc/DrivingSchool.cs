using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Misc
{
    public partial class DrivingSchool
    {
        public const byte MinimalOkAnswersTestPercentage = 80;

        public static List<DrivingSchool> All { get; private set; } = new List<DrivingSchool>();

        public int Id => All.IndexOf(this) + 1;

        public Vector3 Position { get; set; }

        public Dictionary<VehicleInfo, LicenseType> Vehicles { get; private set; }

        public Dictionary<LicenseType, Vector3[]> PracticeRoutes { get; private set; }

        [Properties.Settings.Static.ClientSync("drivingSchoolLicensePrices")]
        public static Dictionary<LicenseType, uint> Prices { get; private set; } = new Dictionary<LicenseType, uint>()
        {
            { LicenseType.B, 1_000 },
            { LicenseType.C, 2_000 },
            { LicenseType.A, 3_000 },
            { LicenseType.D, 4_000 },
            { LicenseType.Sea, 4_000 },
            { LicenseType.Fly, 5_000 },
        };

        public DrivingSchool(Vector3 Position)
        {
            this.Position = Position;

            All.Add(this);
        }

        public static DrivingSchool Get(int id) => id < 1 || id > All.Count ? null : All[id - 1];

        public static LicenseType GetLicenseTypeForPracticeRoute(LicenseType licType) => licType == LicenseType.B || licType == LicenseType.A || licType == LicenseType.C || licType == LicenseType.D ? LicenseType.B : licType;
    }
}
