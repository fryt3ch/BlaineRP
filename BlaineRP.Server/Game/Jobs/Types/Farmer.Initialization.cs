using System.Linq;
using BlaineRP.Server.EntitiesData.Vehicles;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Jobs
{
    public partial class Farmer
    {
        public override void Initialize()
        {
            var subId = SubId;

            var numberplate = $"FARM{FarmBusiness.ID}";

            var tractorType = TractorVehicleData;
            var planeType = PlaneVehicleData;

            if (subId == 0)
            {
                var tractorCol1 = new Colour(255, 0, 0, 255);
                var tractorCol2 = new Colour(0, 0, 0, 255);

                //2133.948f, 4783.06f, 41.31395f, 23.95123f - plane pos

                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplate,
                        tractorType,
                        tractorCol1,
                        tractorCol2,
                        new Vector4(1865.232f, 4872.092f, 44.38249f, 206.4608f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplate,
                        tractorType,
                        tractorCol1,
                        tractorCol2,
                        new Vector4(1870.744f, 4874.888f, 44.66641f, 206.8733f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplate,
                        tractorType,
                        tractorCol1,
                        tractorCol2,
                        new Vector4(1876.944f, 4878.157f, 45.11294f, 202.0746f),
                        Properties.Settings.Static.MainDimension
                    )
                );
                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplate,
                        tractorType,
                        tractorCol1,
                        tractorCol2,
                        new Vector4(1859.166f, 4868.78f, 44.17879f, 210.0185f),
                        Properties.Settings.Static.MainDimension
                    )
                );

                Vehicles.Add(VehicleData.NewJob(Id,
                        numberplate,
                        planeType,
                        tractorCol1,
                        tractorCol2,
                        new Vector4(2143.947f, 4813.993f, 41.57311f, 114.3835f),
                        Properties.Settings.Static.MainDimension
                    )
                );
            }

            foreach (var x in Vehicles.Where(x => x.Data == tractorType))
            {
                AttachHarvTrailOnTractor(x.VehicleData);
            }
        }
    }
}