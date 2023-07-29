namespace BlaineRP.Server.Game.Craft
{
    public class ResultData
    {
        public ItemPrototype ResultItem { get; private set; }

        public int CraftTime { get; private set; }

        public ResultData(string Id, int Amount = 1, int CraftTime = 0)
        {
            this.ResultItem = new ItemPrototype(Id, Amount);

            this.CraftTime = CraftTime;
        }
    }
}