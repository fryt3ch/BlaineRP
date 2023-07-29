using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Game.Items
{
    public abstract class PlaceableItem : Item
    {
        public new abstract class ItemData : Item.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Model}";

            public ItemData(string name, float weight, string model) : base(name, weight, model)
            {

            }
        }

        [JsonIgnore]
        public new ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        public virtual World.Service.ItemOnGround Install(PlayerData pData, Vector3 pos, Vector3 rot)
        {
            if (OnGroundInstance is World.Service.ItemOnGround iogInstance)
            {
                if (iogInstance.Type == World.Service.ItemOnGround.Types.PlacedItem)
                {
                    iogInstance.Object.Position = pos;
                    iogInstance.Object.Rotation = rot;

                    return iogInstance;
                }

                return null;
            }

            return World.Service.AddItemOnGround(pData, this, pos, rot, pData.Player.Dimension, World.Service.ItemOnGround.Types.PlacedItem);
        }

        public virtual bool Remove(PlayerData pData)
        {
            if (OnGroundInstance is World.Service.ItemOnGround iog)
            {
                if (iog.Type == World.Service.ItemOnGround.Types.PlacedItem)
                {
                    iog.Delete(false);

                    return true;
                }
            }

            return false;
        }

        protected PlaceableItem(string id, Item.ItemData data, Type type) : base(id, data, type)
        {

        }
    }
}
