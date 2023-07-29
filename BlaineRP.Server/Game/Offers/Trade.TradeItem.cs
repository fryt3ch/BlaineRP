namespace BlaineRP.Server.Game.Management.Offers
{
    public partial class Trade
    {
        public class TradeItem
        {
            public Items.Item ItemRoot { get; set; }
            public int Amount { get; set; }

            public TradeItem(Items.Item ItemRoot, int Amount)
            {
                this.ItemRoot = ItemRoot;
                this.Amount = Amount;
            }

            public string ToClientJson()
            {
                return ItemRoot == null
                    ? ""
                    : $"{ItemRoot.ID}&{Amount}&{(ItemRoot is Items.IStackable ? ItemRoot.BaseWeight : ItemRoot.Weight)}&{Items.Stuff.GetItemTag(ItemRoot)}";
            }
        }
    }
}