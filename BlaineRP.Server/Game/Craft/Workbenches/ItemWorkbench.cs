using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Game.Craft.Workbenches
{
    public class ItemWorkbench : Workbench
    {
        public static ItemWorkbench Get(uint uid) => Workbench.Get(Types.ItemWorkbench, uid) as ItemWorkbench;

        public World.Service.ItemOnGround OwnerEntity { get; set; }

        public override bool IsNear(PlayerData pData)
        {
            if (OwnerEntity.Object?.Exists != true)
                return false;

            if (pData.Player.Dimension != OwnerEntity.Object.Dimension)
                return false;

            if (pData.Player.Position.DistanceTo(OwnerEntity.Object.Position) > Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE)
                return false;

            return true;
        }

        public override bool IsAccessableFor(PlayerData pData)
        {
            if (OwnerEntity.Object?.Exists != true)
                return false;

            return OwnerEntity.PlayerHasAccess(pData, true, true);
        }

        public ItemWorkbench(uint Uid, WorkbenchTypes Workbenchtype, World.Service.ItemOnGround OwnerEntity) : base(Uid, Types.ItemWorkbench, Workbenchtype)
        {
            this.OwnerEntity = OwnerEntity;
        }
    }
}