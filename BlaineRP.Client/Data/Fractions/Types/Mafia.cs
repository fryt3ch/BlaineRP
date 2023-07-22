using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlaineRP.Client.Data.Fractions
{
    public class Mafia : Fraction
    {
        public Mafia(Types Type, string Name, uint StorageContainerId, string ContainerPos, string CWbPos, byte MaxRank, string CreationWorkbenchPricesJs, uint MetaFlags) : base(Type, Name, StorageContainerId, ContainerPos, CWbPos, MaxRank, RAGE.Util.Json.Deserialize<Dictionary<string, uint>>(CreationWorkbenchPricesJs), MetaFlags)
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
    public class MafiaEvents 
    {
        public MafiaEvents()
        {

        }
    }
}