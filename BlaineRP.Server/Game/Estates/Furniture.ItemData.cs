using System.Collections.Generic;
using System.IO;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Estates
{
    public partial class Furniture
    {
        public partial class ItemData
        {
            public static Dictionary<string, ItemData> AllData { get; private set; } = new Dictionary<string, ItemData>();

            private static Dictionary<Types, Game.Items.Craft.Workbench.WorkbenchTypes?> WorkbenchTypes { get; set; } = new Dictionary<Types, Items.Craft.Workbench.WorkbenchTypes?>()
            {
                { Types.KitchenSet, Game.Items.Craft.Workbench.WorkbenchTypes.KitchenSet },
            };

            public Types Type { get; private set; }

            public string Name { get; private set; }

            public uint Model { get; private set; }

            public string ModelStr { get; }

            public Game.Items.Craft.Workbench.WorkbenchTypes? WorkbenchType => WorkbenchTypes.GetValueOrDefault(Type);

            public ItemData(string Id, Types Type, string Name, string Model)
            {
                this.ModelStr = Model;

                this.Type = Type;

                this.Name = Name;

                this.Model = NAPI.Util.GetHashKey(Model);

                AllData.Add(Id, this);
            }

            public static ItemData Get(string id) => AllData.GetValueOrDefault(id);
        }
    }
}