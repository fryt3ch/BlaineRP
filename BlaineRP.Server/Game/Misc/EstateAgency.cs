using GTANetworkAPI;
using System.Collections.Generic;

namespace BlaineRP.Server.Game.Misc
{
    public partial class EstateAgency
    {
        private static EstateAgency[] All { get; set; }

        public static EstateAgency Get(int idx) => idx < 0 || idx >= All.Length ? null : All[idx];

        public int Id { get { for (int i = 0; i < All.Length; i++) if (All[i] == this) return i; return -1; } }

        public Vector3[] Positions { get; set; }

        public ushort HouseGPSPrice { get; set; }

        public EstateAgency()
        {

        }
    }
}