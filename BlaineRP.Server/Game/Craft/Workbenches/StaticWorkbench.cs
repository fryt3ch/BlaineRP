using BlaineRP.Server.Game.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Craft.Workbenches
{
    public class StaticWorkbench : Workbench
    {
        public uint Dimension { get; set; }

        public Vector3 Position { get; set; }

        public float InteractionRange { get; set; }

        public override bool IsNear(PlayerData pData)
        {
            if (pData.Player.Dimension != Dimension)
                return false;

            if (pData.Player.Position.DistanceTo(Position) > InteractionRange)
                return false;

            return true;
        }

        public override bool IsAccessableFor(PlayerData pData)
        {
            return true;
        }

        public StaticWorkbench(uint Uid, WorkbenchTypes Workbenchtype) : base(Uid, Types.StaticWorkbench, Workbenchtype)
        {

        }
    }
}