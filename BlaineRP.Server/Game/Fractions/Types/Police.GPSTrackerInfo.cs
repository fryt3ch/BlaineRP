namespace BlaineRP.Server.Game.Fractions
{
    public partial class Police
    {
        public class GPSTrackerInfo
        {
            public uint VID { get; set; }

            public string VehicleStr { get; set; }

            public string InstallerStr { get; set; }

            public FractionType FractionType { get; set; }

            public GPSTrackerInfo()
            {

            }
        }
    }
}