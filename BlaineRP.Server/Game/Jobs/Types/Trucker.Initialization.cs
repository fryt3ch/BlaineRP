using BlaineRP.Server.EntitiesData.Vehicles;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Jobs
{
    public partial class Trucker
    {
        public override void Initialize()
        {
            var numberplateText = "TRUCK";

            Vehicles.Add(VehicleData.NewJob(Id,
                    numberplateText,
                    Data.Vehicles.GetData("pounder"),
                    Colour.DefWhite,
                    Colour.DefBlack,
                    new Vector4(36.48936f, 6342.64f, 31.30971f, 14.86628f),
                    Properties.Settings.Static.MainDimension
                )
            );
            Vehicles.Add(VehicleData.NewJob(Id,
                    numberplateText,
                    Data.Vehicles.GetData("pounder"),
                    Colour.DefWhite,
                    Colour.DefBlack,
                    new Vector4(30.00755f, 6338.54f, 31.3096f, 15.64089f),
                    Properties.Settings.Static.MainDimension
                )
            );
            Vehicles.Add(VehicleData.NewJob(Id,
                    numberplateText,
                    Data.Vehicles.GetData("pounder"),
                    Colour.DefWhite,
                    Colour.DefBlack,
                    new Vector4(23.15289f, 6334.313f, 31.30952f, 15.82415f),
                    Properties.Settings.Static.MainDimension
                )
            );
            Vehicles.Add(VehicleData.NewJob(Id,
                    numberplateText,
                    Data.Vehicles.GetData("pounder"),
                    Colour.DefWhite,
                    Colour.DefBlack,
                    new Vector4(16.22741f, 6331.058f, 31.30931f, 16.68959f),
                    Properties.Settings.Static.MainDimension
                )
            );
            Vehicles.Add(VehicleData.NewJob(Id,
                    numberplateText,
                    Data.Vehicles.GetData("pounder"),
                    Colour.DefWhite,
                    Colour.DefBlack,
                    new Vector4(9.375045f, 6326.348f, 31.30978f, 16.85452f),
                    Properties.Settings.Static.MainDimension
                )
            );

            Vehicles.Add(VehicleData.NewJob(Id,
                    numberplateText,
                    Data.Vehicles.GetData("pounder"),
                    Colour.DefBlack,
                    Colour.DefBlack,
                    new Vector4(13.45169f, 6349.37f, 31.30666f, 211.8596f),
                    Properties.Settings.Static.MainDimension
                )
            );
            Vehicles.Add(VehicleData.NewJob(Id,
                    numberplateText,
                    Data.Vehicles.GetData("pounder"),
                    Colour.DefBlack,
                    Colour.DefBlack,
                    new Vector4(18.80654f, 6355.293f, 31.30764f, 213.5229f),
                    Properties.Settings.Static.MainDimension
                )
            );
            Vehicles.Add(VehicleData.NewJob(Id,
                    numberplateText,
                    Data.Vehicles.GetData("pounder"),
                    Colour.DefBlack,
                    Colour.DefBlack,
                    new Vector4(24.34281f, 6360.932f, 31.30667f, 213.5152f),
                    Properties.Settings.Static.MainDimension
                )
            );
            Vehicles.Add(VehicleData.NewJob(Id,
                    numberplateText,
                    Data.Vehicles.GetData("pounder"),
                    Colour.DefBlack,
                    Colour.DefBlack,
                    new Vector4(29.61494f, 6366.637f, 31.30571f, 214.6733f),
                    Properties.Settings.Static.MainDimension
                )
            );
        }
    }
}