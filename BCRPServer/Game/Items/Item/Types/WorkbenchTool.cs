using System.Collections.Generic;

namespace BCRPServer.Game.Items
{
    public class WorkbenchTool : Item
    {
        new public class ItemData : Item.ItemData
        {
            public override string ClientData => $"\"{Name}\"";

            public ItemData(string Name) : base(Name, 0f, new string[] { })
            {

            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "wbi_0", new ItemData("Огонь") },
            { "wbi_1", new ItemData("Вода") },

            { "wbi_2", new ItemData("Нож") },
            { "wbi_3", new ItemData("Венчик") },
            { "wbi_4", new ItemData("Скалка") },
        };

        public WorkbenchTool(string ID) : base(ID, IDList[ID], typeof(WorkbenchTool))
        {

        }
    }
}
