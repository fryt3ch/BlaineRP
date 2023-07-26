using System.Collections.Generic;

namespace BlaineRP.Client.Game.Fractions
{
    public partial class Gang : Fraction
    {
        public Gang(FractionTypes type, string name, uint storageContainerId, string containerPos, string cWbPos, byte maxRank, string creationWorkbenchPricesJs, uint metaFlags) : base(type, name, storageContainerId, containerPos, cWbPos, maxRank, RAGE.Util.Json.Deserialize<Dictionary<string, uint>>(creationWorkbenchPricesJs), metaFlags)
        {

        }

        public override void OnStartMembership(params object[] args)
        {
            base.OnStartMembership(args);
        }

        public override void OnEndMembership()
        {
            base.OnEndMembership();
        }
    }

    [Script(int.MaxValue)]
    public class GangEvents
    {
        public GangEvents()
        {

        }
    }
}