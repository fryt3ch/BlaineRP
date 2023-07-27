using System.Collections.Generic;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Jobs
{
    public partial class BusDriver
    {
        public class RouteData
        {
            public List<Vector3> Positions { get; set; }

            public uint Reward { get; set; }

            public RouteData(uint Reward, List<Vector3> Positions)
            {
                this.Reward = Reward;

                this.Positions = Positions;
            }
        }
    }
}