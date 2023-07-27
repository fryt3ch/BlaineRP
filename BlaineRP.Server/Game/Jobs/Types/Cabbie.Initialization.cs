using BlaineRP.Server.EntitiesData.Vehicles;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Jobs
{
    public partial class Cabbie
    {
        public override void Initialize()
        {
            var taxiVehData = Data.Vehicles.GetData("taxi");

            var subId = SubId;

            if (subId == 0)
            {
                var taxiColour1 = new Colour(255, 207, 32, 255);
                var taxiColour2 = new Colour(255, 207, 32, 255);

                var numberplateText = $"TAXI{subId}";

                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        taxiVehData,
                        taxiColour1,
                        taxiColour2,
                        new Vector4(-255.9184f, 6056.99f, 31.54631f, 124.0276f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        taxiVehData,
                        taxiColour1,
                        taxiColour2,
                        new Vector4(-258.7205f, 6059.436f, 31.34031f, 126.3335f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        taxiVehData,
                        taxiColour1,
                        taxiColour2,
                        new Vector4(-261.5727f, 6062.245f, 31.17303f, 125.3396f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        taxiVehData,
                        taxiColour1,
                        taxiColour2,
                        new Vector4(-264.7574f, 6064.779f, 31.07093f, 126.9705f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        taxiVehData,
                        taxiColour1,
                        taxiColour2,
                        new Vector4(-267.6993f, 6067.127f, 31.07048f, 126.3737f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        taxiVehData,
                        taxiColour1,
                        taxiColour2,
                        new Vector4(-270.1129f, 6069.721f, 31.07076f, 123.6445f),
                        Properties.Settings.Static.MainDimension
                    )
                );
            }
            else
            {
                var taxiColour1 = new Colour(255, 207, 32, 255);
                var taxiColour2 = new Colour(255, 207, 32, 255);

                var numberplateText = $"TAXI{subId}";

                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        taxiVehData,
                        taxiColour1,
                        taxiColour2,
                        new Vector4(913.8994f, -159.8919f, 74.38394f, 194.9475f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        taxiVehData,
                        taxiColour1,
                        taxiColour2,
                        new Vector4(911.4471f, -163.2334f, 73.98677f, 193.3457f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        taxiVehData,
                        taxiColour1,
                        taxiColour2,
                        new Vector4(920.908f, -163.4761f, 74.43394f, 100.5343f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        taxiVehData,
                        taxiColour1,
                        taxiColour2,
                        new Vector4(918.376f, -166.9907f, 74.22795f, 101.7324f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        taxiVehData,
                        taxiColour1,
                        taxiColour2,
                        new Vector4(916.8076f, -170.4563f, 74.07147f, 102.9674f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        taxiVehData,
                        taxiColour1,
                        taxiColour2,
                        new Vector4(908.4425f, -183.159f, 73.77179f, 59.08912f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        taxiVehData,
                        taxiColour1,
                        taxiColour2,
                        new Vector4(904.7427f, -188.8681f, 73.42956f, 60.56298f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        taxiVehData,
                        taxiColour1,
                        taxiColour2,
                        new Vector4(906.9313f, -186.1521f, 73.63885f, 58.95269f),
                        Properties.Settings.Static.MainDimension
                    )
                );
            }
        }
    }
}