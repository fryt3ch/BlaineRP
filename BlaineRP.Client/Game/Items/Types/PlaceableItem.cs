namespace BlaineRP.Client.Game.Items
{
    public abstract class PlaceableItem : Item
    {
        public new abstract class ItemData : Item.ItemData
        {
            public ItemData(string name, float weight, uint model) : base(name, weight)
            {
                Model = model;
            }

            public uint Model { get; set; }
        }
    }
}