using BlaineRP.Server.Game.Gifts;

namespace BlaineRP.Server.Game.Achievements
{
    public partial class Achievement
    {
        public class Data
        {
            public uint Goal { get; set; }

            public bool IsHidden { get; set; }

            public Gift.Prototype Reward { get; set; }

            public Data(uint Goal, Gift.Prototype Reward)
            {
                this.Goal = Goal;
                this.Reward = Reward;
            }
        }
    }
}