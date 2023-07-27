using BlaineRP.Server.EntitiesData.Vehicles;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Jobs
{
    public partial class Collector
    {
        public override void Initialize()
        {
            var numberplateText = $"BANK{BankId}";

            var subId = SubId;

            var vType = Game.Data.Vehicles.GetData("stockade");

            if (subId == 0)
            {
                var colour1 = new Colour(255, 255, 255, 255);
                var colour2 = new Colour(255, 0, 0, 255);

                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        vType,
                        colour1,
                        colour2,
                        new Vector4(-125.2996f, 6476.03f, 31.06822f, 134.7541f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        vType,
                        colour1,
                        colour2,
                        new Vector4(-128.2522f, 6479.262f, 31.07171f, 134.5773f),
                        Properties.Settings.Static.MainDimension
                    )
                );
            }
            else if (subId == 1)
            {
                var colour1 = new Colour(255, 255, 255, 255);
                var colour2 = new Colour(158, 186, 91, 255);

                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        vType,
                        colour1,
                        colour2,
                        new Vector4(162.2404f, -1081.599f, 28.79756f, 1.330183f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        vType,
                        colour1,
                        colour2,
                        new Vector4(158.5186f, -1081.227f, 28.79802f, 1.025265f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        vType,
                        colour1,
                        colour2,
                        new Vector4(154.839f, -1081.596f, 28.79737f, 1.333282f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        vType,
                        colour1,
                        colour2,
                        new Vector4(150.9679f, -1081.523f, 28.79806f, 1f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        vType,
                        colour1,
                        colour2,
                        new Vector4(147.2328f, -1081.582f, 28.79786f, 1f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        vType,
                        colour1,
                        colour2,
                        new Vector4(143.5911f, -1081.642f, 28.79869f, 1f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplateText,
                        vType,
                        colour1,
                        colour2,
                        new Vector4(139.8136f, -1081.303f, 28.7997f, 1f),
                        Properties.Settings.Static.MainDimension
                    )
                );
            }
        }
    }
}