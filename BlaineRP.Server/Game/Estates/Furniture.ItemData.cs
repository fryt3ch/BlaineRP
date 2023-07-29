using System.Collections.Generic;
using System.IO;
using BlaineRP.Server.Game.Craft;
using BlaineRP.Server.Game.Craft.Workbenches;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Estates
{
    public partial class Furniture
    {
        public partial class ItemData
        {
            public static Dictionary<string, ItemData> AllData { get; private set; } = new Dictionary<string, ItemData>();

            private static Dictionary<Types, WorkbenchTypes?> WorkbenchTypes { get; set; } = new Dictionary<Types, WorkbenchTypes?>()
            {
                { Types.KitchenSet, Craft.Workbenches.WorkbenchTypes.KitchenSet },
            };

            public Types Type { get; private set; }

            public string Name { get; private set; }

            public uint Model { get; private set; }

            public string ModelStr { get; }

            public WorkbenchTypes? WorkbenchType => WorkbenchTypes.GetValueOrDefault(Type);

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