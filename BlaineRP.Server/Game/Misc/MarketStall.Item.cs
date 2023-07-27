namespace BlaineRP.Server.Game.Misc
{
    public partial class MarketStall
    {
        public class Item
        {
            public Game.Items.Item ItemRoot { get; set; }
            public int Amount { get; set; }
            public uint Price { get; set; }

            public Item(Game.Items.Item ItemRoot, int Amount, uint Price)
            {
                this.ItemRoot = ItemRoot;
                this.Amount = Amount;
                this.Price = Price;
            }
        }
    }
}