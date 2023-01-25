using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Items.Craft
{
    public partial class Craft
    {
        private static ItemPrototype FireStaticPrototype { get; set; } = new ItemPrototype("wbi_0", 0);
        private static ItemPrototype WaterStaticPrototype { get; set; } = new ItemPrototype("wbi_1", 0);

        public static List<Receipt> AllReceipts { get; private set; } = new List<Receipt>()
        {
            new Receipt(new ResultData("f_acod_f", 1, 1000), new ItemPrototype("f_acod", 1), FireStaticPrototype),
        };

        public static WorkbenchTool FireStaticItem { get; private set; } = (WorkbenchTool)Stuff.CreateItem("wbi_0", 0, 0, true);
        public static WorkbenchTool WaterStaticItem { get; private set; } = (WorkbenchTool)Stuff.CreateItem("wbi_1", 0, 0, true);
    }
}
