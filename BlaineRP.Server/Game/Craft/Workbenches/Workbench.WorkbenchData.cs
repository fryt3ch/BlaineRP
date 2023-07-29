using BlaineRP.Server.Game.Items;

namespace BlaineRP.Server.Game.Craft.Workbenches
{
    public abstract partial class Workbench
    {
        public class WorkbenchData
        {
            public WorkbenchTool[] Tools { get; private set; }

            public Item[] DefaultCraftItems { get; private set; }

            public WorkbenchData(WorkbenchTool[] Tools, Item[] DefaultCraftItems)
            {
                this.Tools = Tools;

                this.DefaultCraftItems = DefaultCraftItems;
            }
        }
    }
}