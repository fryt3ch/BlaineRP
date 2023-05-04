using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Casino
{
    public class LuckyWheel
    {
        public Vector3 Position { get; set; }

        public LuckyWheel(float PosX, float PosY, float PosZ)
        {
            this.Position = new Vector3(PosX, PosY, PosZ);
        }
    }
}
