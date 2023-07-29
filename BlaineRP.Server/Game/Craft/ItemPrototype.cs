namespace BlaineRP.Server.Game.Craft
{
    public class ItemPrototype
    {
        public string Id { get; private set; }

        public int Amount { get; private set; }

        public ItemPrototype(string Id, int Amount = 1)
        {
            this.Id = Id;
            this.Amount = Amount;
        }
    }
}