using System.Collections.Generic;
using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Game.Craft.Workbenches
{
    public class FurnitureWorkbench : Workbench
    {
        public static FurnitureWorkbench Get(uint uid) => Workbench.Get(Types.FurnitureWorkbench, uid) as FurnitureWorkbench;

        public Estates.HouseBase HouseBase { get; private set; }

        public override bool IsNear(PlayerData pData)
        {
            if (pData.CurrentHouseBase is Estates.HouseBase houseBase)
            {
                if (houseBase != HouseBase)
                    return false;

                return true;
            }

            return false;
        }

        public override bool IsAccessableFor(PlayerData pData)
        {
            if (!HouseBase.ContainersLocked)
                return true;

            if (HouseBase.Owner == pData.Info)
                return true;

            if (HouseBase.Settlers.GetValueOrDefault(pData.Info) != null)
                return true;

            pData.Player.Notify("House::NotAllowed");

            return false;
        }

        public FurnitureWorkbench(uint Uid, Estates.HouseBase HouseBase, WorkbenchTypes Workbenchtype) : base(Uid, Types.FurnitureWorkbench, Workbenchtype)
        {
            this.HouseBase = HouseBase;
        }
    }
}