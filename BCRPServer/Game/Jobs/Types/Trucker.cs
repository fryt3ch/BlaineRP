using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Jobs
{
    public class Trucker : Job, IVehicles
    {
        public List<VehicleData> Vehicles { get; set; } = new List<VehicleData>();

        public string NumberplateText { get; set; } = "TRUCK";

        public uint VehicleRentPrice { get; set; }

        public override string ClientData => $"{Id}, {Position.ToCSharpStr()}";

        public Trucker(Utils.Vector4 Position) : base(Types.Trucker, Position)
        {

        }

        public override void Initialize()
        {
            Vehicles.Add(VehicleData.NewJob(Id, Data.Vehicles.GetData("pounder"), Utils.Colour.DefWhite, Utils.Colour.DefBlack, new Utils.Vector4(36.48936f, 6342.64f, 31.30971f, 14.86628f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, Data.Vehicles.GetData("pounder"), Utils.Colour.DefWhite, Utils.Colour.DefBlack, new Utils.Vector4(30.00755f, 6338.54f, 31.3096f, 15.64089f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, Data.Vehicles.GetData("pounder"), Utils.Colour.DefWhite, Utils.Colour.DefBlack, new Utils.Vector4(23.15289f, 6334.313f, 31.30952f, 15.82415f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, Data.Vehicles.GetData("pounder"), Utils.Colour.DefWhite, Utils.Colour.DefBlack, new Utils.Vector4(16.22741f, 6331.058f, 31.30931f, 16.68959f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, Data.Vehicles.GetData("pounder"), Utils.Colour.DefWhite, Utils.Colour.DefBlack, new Utils.Vector4(9.375045f, 6326.348f, 31.30978f, 16.85452f), Utils.Dimensions.Main));

            Vehicles.Add(VehicleData.NewJob(Id, Data.Vehicles.GetData("pounder"), Utils.Colour.DefBlack, Utils.Colour.DefBlack, new Utils.Vector4(13.45169f, 6349.37f, 31.30666f, 211.8596f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, Data.Vehicles.GetData("pounder"), Utils.Colour.DefBlack, Utils.Colour.DefBlack, new Utils.Vector4(18.80654f, 6355.293f, 31.30764f, 213.5229f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, Data.Vehicles.GetData("pounder"), Utils.Colour.DefBlack, Utils.Colour.DefBlack, new Utils.Vector4(24.34281f, 6360.932f, 31.30667f, 213.5152f), Utils.Dimensions.Main));
            Vehicles.Add(VehicleData.NewJob(Id, Data.Vehicles.GetData("pounder"), Utils.Colour.DefBlack, Utils.Colour.DefBlack, new Utils.Vector4(29.61494f, 6366.637f, 31.30571f, 214.6733f), Utils.Dimensions.Main));
        }
    }
}
