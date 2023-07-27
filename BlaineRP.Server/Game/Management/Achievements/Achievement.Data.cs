namespace BlaineRP.Server.Game.Management.Achievements
{
    public partial class Achievement
    {
        public class Data
        {
            public uint Goal { get; set; }

            public bool IsHidden { get; set; }

            public Game.Items.Gift.Prototype Reward { get; set; }

            public Data(uint Goal, Game.Items.Gift.Prototype Reward)
            {
                this.Goal = Goal;
                this.Reward = Reward;
            }
        }
    }
}