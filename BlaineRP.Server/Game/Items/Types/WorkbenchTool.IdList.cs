using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class WorkbenchTool
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            { "wbi_0", new ItemData("Огонь") },
            { "wbi_1", new ItemData("Вода") },

            { "wbi_2", new ItemData("Нож") },
            { "wbi_3", new ItemData("Венчик") },
            { "wbi_4", new ItemData("Скалка") },
        };
    }
}