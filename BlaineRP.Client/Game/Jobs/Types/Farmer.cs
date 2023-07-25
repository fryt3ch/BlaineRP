using BlaineRP.Client.Game.Jobs.Enums;

namespace BlaineRP.Client.Game.Jobs.Types
{
    public class Farmer : Job
    {
        private int BusinessId { get; set; }

        public Client.Data.Locations.Farm FarmBusiness => Client.Data.Locations.Business.All[BusinessId] as Client.Data.Locations.Farm;

        public Farmer(int Id, int BuisnessId) : base(Id, JobTypes.Farmer)
        {
            this.BusinessId = BuisnessId;
        }

        public override void OnEndJob()
        {
            Client.Data.Minigames.Farm.StopCowMilk(false);
            Client.Data.Minigames.Farm.StopOrangeTreeCollect(false);

            base.OnEndJob();
        }
    }
}
