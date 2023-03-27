using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Sync
{
    public static class Cooldowns
    {
        public enum Types
        {
            ShootingRange = 0,

            FractionJoined,
        }

        public static bool IsTemp(Types type) => false;

        public const int CD_SHOOTING_RANGE = 3_600;
    }
}
