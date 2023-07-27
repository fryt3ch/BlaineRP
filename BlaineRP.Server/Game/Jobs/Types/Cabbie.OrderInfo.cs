using System.Threading;
using BlaineRP.Server.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Jobs
{
    public partial class Cabbie
    {
        public class OrderInfo
        {
            public bool Exists => ActiveOrders.ContainsValue(this);

            public Entity Entity { get; set; }

            public Vector3 Position { get; set; }

            public PlayerInfo CurrentWorker { get; set; }

            public Timer GPSTrackerTimer { get; set; }

            public OrderInfo(Entity Entity, Vector3 Position)
            {
                this.Entity = Entity;
                this.Position = Position;
            }
        }
    }
}