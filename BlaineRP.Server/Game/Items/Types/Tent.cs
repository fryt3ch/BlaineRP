using GTANetworkAPI;
using Newtonsoft.Json;
using System.Collections.Generic;
using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Game.Items
{
    public partial class Tent : PlaceableItem
    {
        public new class ItemData : PlaceableItem.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Model}";

            public ItemData(string name, float weight, string model) : base(name, weight, model)
            {

            }
        }

        [JsonIgnore]
        public new ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        public override void Delete()
        {
            base.Delete();
        }

        public override World.Service.ItemOnGround Install(PlayerData pData, Vector3 pos, Vector3 rot)
        {
            var iog = base.Install(pData, pos, rot);

            return iog;
        }

        public override bool Remove(PlayerData pData)
        {
            base.Remove(pData);

            return true;
        }

        public Tent(string id) : base(id, IdList[id], typeof(Tent))
        {

        }
    }
}