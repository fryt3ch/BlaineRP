using BlaineRP.Server.EntitiesData.Vehicles;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Jobs
{
    public partial class BusDriver
    {
        public override void Initialize()
        {
            if (SubId == 0)
            {
                var numberplateText = "BUSBC";

                var vType = Game.Data.Vehicles.GetData("coach");

                var colour1 = new Colour(255, 255, 255, 255);
                var colour2 = new Colour(255, 0, 0, 255);

                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        vType,
                        colour1,
                        colour2,
                        new Vector4(-766.2029f, 5524.796f, 34.31338f, 28.3842f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        vType,
                        colour1,
                        colour2,
                        new Vector4(-762.9031f, 5526.446f, 34.31443f, 29.5885f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        vType,
                        colour1,
                        colour2,
                        new Vector4(-759.7948f, 5528.181f, 34.31483f, 30.55079f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        vType,
                        colour1,
                        colour2,
                        new Vector4(-756.5928f, 5529.695f, 34.31594f, 29.94526f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        vType,
                        colour1,
                        colour2,
                        new Vector4(-753.6445f, 5532.07f, 34.31636f, 29.10272f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        vType,
                        colour1,
                        colour2,
                        new Vector4(-750.3279f, 5533.969f, 34.31674f, 29.43421f),
                        Properties.Settings.Static.MainDimension
                    )
                );
            }
            else
            {
                var numberplateText = "BUSLS";

                var vType = Game.Data.Vehicles.GetData("bus");
            }
        }
    }
}