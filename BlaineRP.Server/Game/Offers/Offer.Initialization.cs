using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BlaineRP.Server.Game.Offers
{
    public partial class Offer
    {
        public static void Load()
        {
            if (_offerBases != null)
                return;

            _offerBases = new Dictionary<OfferType, OfferBase>();

            foreach (var x in Assembly.GetExecutingAssembly()
                                      .GetTypes()
                                      .Where(x => x.IsClass && x.BaseType == typeof(OfferBase) && x.Namespace?.StartsWith("BCRPServer.Sync.Offers") == true))
            {
                var attr = x.GetCustomAttribute<OfferAttribute>();

                if (attr == null)
                    continue;

                var obj = (OfferBase)Activator.CreateInstance(x);

                if (!_offerBases.TryAdd(attr.Type, obj))
                    _offerBases[attr.Type] = obj;

                //Console.WriteLine(x.Name);
            }
        }
    }
}