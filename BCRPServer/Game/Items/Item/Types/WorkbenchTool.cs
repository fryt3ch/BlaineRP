﻿using System;
using System.Collections.Generic;
using System.Text;

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
        };

        public WorkbenchTool(string ID) : base(ID, IDList[ID], typeof(WorkbenchTool))
        {

        }
    }
}