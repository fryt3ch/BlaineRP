using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Jobs
{
    public abstract partial class Job
    {
        public static int InitializeAll()
        {
            new Trucker(new Vector4(50.92614f, 6337.754f, 31.38093f, 21.58484f))
            {
                VehicleRentPrice = 1_500,

                MaterialsPositions = new List<Vector3>()
                {
                    new Vector3(142.8727f, 6364.274f, 30.5f),
                }
            };

            new Cabbie(new Vector4(-271.4561f, 6074.177f, 31.68299f, 183.1857f))
            {
                VehicleRentPrice = 750,
            };

            new Cabbie(new Vector4(895.5431f, -178.9546f, 74.70026f, 254.4035f))
            {
                VehicleRentPrice = 750,
            };

            new BusDriver(new Vector4(-744.4896f, 5546.173f, 33.60594f, 121.2667f))
            {
                VehicleRentPrice = 1_000,

                Routes = new List<BusDriver.RouteData>()
                {
                    new BusDriver.RouteData(2_500,
                        new List<Vector3>()
                        {
                            new Vector3(-782.8536f, 5500.472f, 34.38135f),
                            new Vector3(-945.3662f, 5418.015f, 38.22468f),
                            new Vector3(-773.1383f, 5482.208f, 34.35715f),
                            new Vector3(-785.9555f, 5536.9139f, 33.65596f),
                        }
                    ),
                }
            };

            new Collector(new Vector4(-90.54147f, 6471.977f, 31.29943f, 0f), 0)
            {
                VehicleRentPrice = 2_000,
            };

            new Collector(new Vector4(132.968f, -1056.943f, 29.19235f, 0f), 7)
            {
                VehicleRentPrice = 2_000,
            };

            new Farmer(Game.Businesses.Business.Get(38) as Game.Businesses.Farm)
            {
                VehicleRentPrice = 500,
            };

            var lines = new List<string>();

            foreach (var x in AllJobs.Values)
            {
                x.Initialize();

                lines.Add($"new {x.GetType().Name}({x.ClientData});");
            }

            Trucker.AllTruckerJobs = AllJobs.Values.Select(x => x as Trucker).Where(x => x != null).ToList();
            Collector.AllCollectorJobs = AllJobs.Values.Select(x => x as Collector).Where(x => x != null).ToList();

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetPath + @"\Game\Jobs\Job.Initialization.cs",
                "TO_REPLACE",
                lines
            );

            return AllJobs.Count;
        }
    }
}