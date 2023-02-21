using GTANetworkAPI;
using Newtonsoft.Json;
using System;

namespace BCRPServer.Game.Items
{
    public abstract class PlaceableItem : Item
    {
        new public abstract class ItemData : Item.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Model}";

            public ItemData(string Name, float Weight, string Model) : base(Name, Weight, Model)
            {

            }
        }

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        public virtual Sync.World.ItemOnGround Install(PlayerData pData, Vector3 pos, Vector3 rot)
        {
            if (OnGroundInstance is Sync.World.ItemOnGround iogInstance)
            {
                if (iogInstance.Type == Sync.World.ItemOnGround.Types.PlacedItem)
                {
                    iogInstance.Object.Position = pos;
                    iogInstance.Object.Rotation = rot;

                    return iogInstance;
                }

                return null;
            }

            return Sync.World.AddItemOnGround(pData, this, pos, rot, pData.Player.Dimension, Sync.World.ItemOnGround.Types.PlacedItem);
        }

        public virtual bool Remove(PlayerData pData)
        {
            if (OnGroundInstance is Sync.World.ItemOnGround iog)
            {
                if (iog.Type == Sync.World.ItemOnGround.Types.PlacedItem)
                {
                    iog.Delete(false);

                    return true;
                }
            }

            return false;
        }

        protected PlaceableItem(string ID, Item.ItemData Data, Type Type) : base(ID, Data, Type)
        {

        }
    }
}
