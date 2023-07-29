using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Misc
{
    public partial class DrivingSchool
    {
        public static void InitializeAll()
        {
            var lines = new List<string>();

            var col1 = new Colour(0, 0, 255, 255);
            var col2 = new Colour(0, 0, 255, 255);

            var vehicle1 = EntitiesData.Vehicles.Static.Service.GetData("intruder");

            new DrivingSchool(new Vector3(208.6196f, -1382.992f, 29.58354f))
            {
                Vehicles = new Dictionary<VehicleInfo, LicenseType>()
                {
                    {
                        VehicleData.NewAutoschool(1,
                            vehicle1,
                            col1,
                            col2,
                            new Vector4(208.6979f, -1373.918f, 30.17774f, 230.5807f),
                            Properties.Settings.Static.MainDimension
                        ),
                        LicenseType.B
                    },
                    {
                        VehicleData.NewAutoschool(1,
                            vehicle1,
                            col1,
                            col2,
                            new Vector4(211.0172f, -1371.398f, 30.17699f, 232.7517f),
                            Properties.Settings.Static.MainDimension
                        ),
                        LicenseType.B
                    },
                    {
                        VehicleData.NewAutoschool(1,
                            vehicle1,
                            col1,
                            col2,
                            new Vector4(213.2036f, -1369.059f, 30.17646f, 231.8667f),
                            Properties.Settings.Static.MainDimension
                        ),
                        LicenseType.B
                    },
                    {
                        VehicleData.NewAutoschool(1,
                            vehicle1,
                            col1,
                            col2,
                            new Vector4(214.5865f, -1363.528f, 30.17337f, 229.6534f),
                            Properties.Settings.Static.MainDimension
                        ),
                        LicenseType.B
                    },
                    {
                        VehicleData.NewAutoschool(1,
                            vehicle1,
                            col1,
                            col2,
                            new Vector4(216.8003f, -1360.368f, 30.17301f, 231.4579f),
                            Properties.Settings.Static.MainDimension
                        ),
                        LicenseType.B
                    },
                    {
                        VehicleData.NewAutoschool(1,
                            vehicle1,
                            col1,
                            col2,
                            new Vector4(218.7388f, -1357.639f, 30.17165f, 232.5897f),
                            Properties.Settings.Static.MainDimension
                        ),
                        LicenseType.B
                    },
                },

                PracticeRoutes = new Dictionary<LicenseType, Vector3[]>()
                {
                    {
                        LicenseType.B, new Vector3[]
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
                var avgVehiclePoses = x.Vehicles.GroupBy(x => x.Value)
                                       .ToDictionary(x => x.Key,
                                            x =>
                                            {
                                                var allPos = x.Select(y => y.Key.LastData.Position).ToList();
                                                var pos = Utils.ZeroVector;
                                                foreach (var p in allPos)
                                                    pos += p;
                                                return pos / allPos.Count;
                                            }
                                        );

                lines.Add(
                    $"new {nameof(BlaineRP.Client.Game.Misc.Autoschool)}({x.Position.ToCSharpStr()}, \"{x.PracticeRoutes.SerializeToJson().Replace('\"', '\'')}\", \"{avgVehiclePoses.SerializeToJson().Replace('\"', '\'')}\");"
                );
            }

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() +
                                          Properties.Settings.Static.ClientScriptsTargetPath +
                                          @"\Game\Misc\Autoschool.Initialization.cs",
                "TO_REPLACE",
                lines
            );
        }
    }
}