using RAGE;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BCRPClient.Additional
{
    class Discord : Events.Script
    {
		public Discord()
        {
            RAGE.Discord.Update("Проводит время на", "Blaine RP");
		}
    }
}
