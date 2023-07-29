using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class WorkbenchTool : Item
    {
        public new class ItemData : Item.ItemData
        {
            public override string ClientData => $"\"{Name}\"";

            public ItemData(string name) : base(name, 0f, new string[] { })
            {

            }
        }

        public WorkbenchTool(string id) : base(id, IdList[id], typeof(WorkbenchTool))
        {

        }
    }
}
