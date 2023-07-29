using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Inventory;
using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Items
{
    public partial interface IUsable
    {
        [JsonIgnore]
        public bool InUse { get; set; }

        public bool StartUse(PlayerData pData, GroupTypes group, int slot, bool needUpdate, params object[] args);

        public bool StopUse(PlayerData pData, GroupTypes group, int slot, bool needUpdate, params object[] args);
    }
}