using BlaineRP.Client.Game.Businesses;

namespace BlaineRP.Client.Game.Jobs
{
    public class Farmer : Job
    {
        public Farmer(int Id, int BuisnessId) : base(Id, JobType.Farmer)
        {
            BusinessId = BuisnessId;
        }

        private int BusinessId { get; set; }

        public Farm FarmBusiness => Business.All[BusinessId] as Farm;

        public override void OnEndJob()
        {
            Scripts.Farm.StopCowMilk(false);
            Scripts.Farm.StopOrangeTreeCollect(false);

            base.OnEndJob();
        }
    }
}