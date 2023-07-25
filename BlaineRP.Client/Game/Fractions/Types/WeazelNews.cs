using System.Collections.Generic;
using BlaineRP.Client.Game.Fractions.Enums;

namespace BlaineRP.Client.Game.Fractions.Types
{
    public class WeazelNews : Fraction
    {
        public WeazelNews(FractionTypes type, string name, uint storageContainerId, string containerPos, string cWbPos, byte maxRank, string creationWorkbenchPricesJs, uint metaFlags) : base(type, name, storageContainerId, containerPos, cWbPos, maxRank, RAGE.Util.Json.Deserialize<Dictionary<string, uint>>(creationWorkbenchPricesJs), metaFlags)
        {
            if (type == FractionTypes.MEDIA_LS)
            {

            }
        }

        public override void OnStartMembership(params object[] args)
        {
            base.OnStartMembership(args);

            //CEF.HUD.Menu.UpdateCurrentTypes(true, CEF.HUD.Menu.Types.Fraction_Police_TabletPC);

            //KeyBinds.CurrentExtraAction0 = () => CuffPlayer(null, null);
        }

        public override void OnEndMembership()
        {
            //CEF.HUD.Menu.UpdateCurrentTypes(false, CEF.HUD.Menu.Types.Fraction_Police_TabletPC);

            Input.Core.CurrentExtraAction0 = null;

            base.OnEndMembership();
        }
    }

    [Script(int.MaxValue)]
    public class WeazelNewsEvents
    {
        public WeazelNewsEvents()
        {

        }
    }
}