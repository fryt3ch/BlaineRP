using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;
using System.Collections.Generic;
using BlaineRP.Client.Sync;
using BlaineRP.Client.Game.EntitiesData.Enums;
using BlaineRP.Client.Game.NPCs;
using BlaineRP.Client.Game.Wrappers.Blips;
using BlaineRP.Client.Game.Wrappers.Colshapes;
using BlaineRP.Client.Game.Wrappers.Colshapes.Enums;
using BlaineRP.Client.Game.Wrappers.Colshapes.Types;

namespace BlaineRP.Client.Data
{
    public partial class Locations
    {
        public class Autoschool
        {
            public static Dictionary<LicenseTypes, uint> Prices { get; set; }

            public static List<Autoschool> All { get; private set; } = new List<Autoschool>();

            public static Autoschool Get(int id) => id < 1 || id > All.Count ? null : All[id - 1];

            public Dictionary<LicenseTypes, Vector3[]> PracticeRoutes { get; set; }

            public Dictionary<LicenseTypes, Vector3> VehiclesPositions { get; set; }

            public int Id => All.IndexOf(this) + 1;

            public Autoschool(Vector3 Position, string PracticeRoutesStr, string VehiclesPositionsStr)
            {
                All.Add(this);

                this.PracticeRoutes = RAGE.Util.Json.Deserialize<Dictionary<LicenseTypes, Vector3[]>>(PracticeRoutesStr);

                this.VehiclesPositions = RAGE.Util.Json.Deserialize<Dictionary<LicenseTypes, Vector3>>(VehiclesPositionsStr);

                var id = Id;

                var cs = new Cylinder(new Vector3(Position.X, Position.Y, Position.Z), 1.5f, 2.5f, false, Misc.RedColor, Settings.App.Static.MainDimension, null)
                {
                    InteractionType = InteractionTypes.DrivingSchoolInteract,

                    ActionType = ActionTypes.DrivingSchoolInteract,

                    Data = id,
                };

                var marker = new Marker(32, new Vector3(Position.X, Position.Y, Position.Z + 1f), 1f, Vector3.Zero, Vector3.Zero, new RGBA(255, 255, 255, 255), true, Settings.App.Static.MainDimension);

                var blip = new ExtraBlip(545, Position, "Автошкола", 1f, 3, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                if (id == 1)
                {
                    var npc = new NPC($"drivingschool_{id}", "Оливия", NPC.Types.Talkable, "s_f_y_airhostess_01", new Vector3(214.5508f, -1400.095f, 30.58353f), 318.5795f, Settings.App.Static.MainDimension)
                    {
                        SubName = "NPC_SUBNAME_DRIVINGSCHOOL_WORKER",

                        DefaultDialogueId = "drivingschool_d_0",
                    };
                }
            }

            public static LicenseTypes GetLicenseTypeForPracticeRoute(LicenseTypes licType) => licType == LicenseTypes.B || licType == LicenseTypes.A || licType == LicenseTypes.C || licType == LicenseTypes.D ? LicenseTypes.B : licType;
        }
    }
}
