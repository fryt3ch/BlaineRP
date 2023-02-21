using System.Collections.Generic;

namespace BCRPServer.Game.Items.Craft
{
    public partial class Craft
    {
        private static ItemPrototype FireStaticPrototype { get; set; } = new ItemPrototype("wbi_0", 0);
        private static ItemPrototype WaterStaticPrototype { get; set; } = new ItemPrototype("wbi_1", 0);
        private static ItemPrototype KnifeStaticPrototype { get; set; } = new ItemPrototype("wbi_2", 0);
        private static ItemPrototype WhiskStaticPrototype { get; set; } = new ItemPrototype("wbi_3", 0);
        private static ItemPrototype RollingPinStaticPrototype { get; set; } = new ItemPrototype("wbi_4", 0);

        public static List<Receipt> AllReceipts { get; private set; } = new List<Receipt>()
        {
            new Receipt(new ResultData("f_acod_f", 1, 1000), new ItemPrototype("f_acod", 1), FireStaticPrototype),
        };

        public static WorkbenchTool FireStaticItem { get; private set; } = (WorkbenchTool)Stuff.CreateItem("wbi_0", 0, 0, true);
        public static WorkbenchTool WaterStaticItem { get; private set; } = (WorkbenchTool)Stuff.CreateItem("wbi_1", 0, 0, true);
        public static WorkbenchTool KnifeStaticItem { get; private set; } = (WorkbenchTool)Stuff.CreateItem("wbi_2", 0, 0, true);
        public static WorkbenchTool WhishStaticItem { get; private set; } = (WorkbenchTool)Stuff.CreateItem("wbi_3", 0, 0, true);
        public static WorkbenchTool RollingPinStaticItem { get; private set; } = (WorkbenchTool)Stuff.CreateItem("wbi_4", 0, 0, true);
    }
}
