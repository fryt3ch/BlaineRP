using System;
using RAGE;

namespace BlaineRP.Client.Game.Fractions
{
    public partial class Police
    {
        public class NotificationInfo
        {
            public ushort Id { get; set; }

            public string Text { get; set; }

            public Vector3 Position { get; set; }

            public DateTime Time { get; set; }

            public NotificationInfo()
            {
            }
        }
    }
}