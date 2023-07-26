using BlaineRP.Client.Game.Businesses;

namespace BlaineRP.Client.Game.Jobs
{
    public class Farmer : Job
    {
        private int BusinessId { get; set; }

        public Farm FarmBusiness => Business.All[BusinessId] as Farm;

        public Farmer(int Id, int BuisnessId) : base(Id, JobTypes.Farmer)
        {
            this.BusinessId = BuisnessId;
        }

        public override void OnEndJob()
        {
            Scripts.Farm.StopCowMilk(false);
            Scripts.Farm.StopOrangeTreeCollect(false);

            base.OnEndJob();
        }
    }
}
