using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Data.Fractions
{
    public class WeazelNews : Fraction
    {
        public WeazelNews(Types Type, string Name, uint StorageContainerId, string ContainerPos, string CWbPos, byte MaxRank, string CreationWorkbenchPricesJs, uint MetaFlags) : base(Type, Name, StorageContainerId, ContainerPos, CWbPos, MaxRank, RAGE.Util.Json.Deserialize<Dictionary<string, uint>>(CreationWorkbenchPricesJs), MetaFlags)
        {
            if (Type == Types.MEDIA_LS)
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

            KeyBinds.CurrentExtraAction0 = null;

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