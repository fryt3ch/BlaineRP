using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Data.Jobs
{
    public class Farmer : Job
    {
        private int BusinessId { get; set; }

        public Data.Locations.Farm FarmBusiness => Data.Locations.Business.All[BusinessId] as Data.Locations.Farm;

        public Farmer(int Id, int BuisnessId) : base(Id, Types.Farmer)
        {
            this.BusinessId = BuisnessId;
        }
    }
}
