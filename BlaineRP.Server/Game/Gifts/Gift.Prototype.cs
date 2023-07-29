namespace BlaineRP.Server.Game.Gifts
{
    public partial class Gift
    {
        public class Prototype
        {
            public GiftTypes Type { get; set; }

            public GiftSourceTypes SourceType { get; set; }

            public string Gid { get; set; }

            public int Variation { get; set; }

            public int Amount { get; set; }

            public Prototype(GiftTypes type, GiftSourceTypes sourceType, string gid, int vatiation, int amount)
            {
                Type = type;
                SourceType = sourceType;
                Gid = gid;
                Variation = Variation;
                Amount = amount;
            }

            public static Prototype CreateAchievement(GiftTypes type, string gid, int variation, int amount) => new Prototype(type, GiftSourceTypes.Achievement, gid, variation, amount);

            public static Prototype CreateCasino(GiftTypes type, string gid, int variation, int amount) => new Prototype(type, GiftSourceTypes.Casino, gid, variation, amount);
        }
    }
}