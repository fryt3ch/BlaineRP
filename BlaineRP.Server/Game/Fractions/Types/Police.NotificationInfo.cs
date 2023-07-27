using System;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Fractions
{
    public partial class Police
    {
        public class NotificationInfo
        {
            public string Text { get; set; }

            public Vector3 Position { get; set; }

            public DateTime Time { get; set; }

            public FractionType FractionType { get; set; }

            public NotificationInfo()
            {

            }
        }
    }
}