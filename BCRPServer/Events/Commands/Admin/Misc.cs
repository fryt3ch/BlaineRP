using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Events.Commands
{
    partial class Commands
    {
        [Command("p_tclothes", 1)]
        private static void TempClothes(PlayerData pData, params string[] args)
        {
            if (args.Length != 5)
                return;

            uint pid;
            int slot, drawable, texture;
            bool clothes;

            if (!uint.TryParse(args[0], out pid) || !int.TryParse(args[1], out slot) || !int.TryParse(args[2], out drawable) || !int.TryParse(args[3], out texture) || !bool.TryParse(args[4], out clothes))
                return;

            var tData = pData;

            if (pData.CID != pid && pData.Player.Id != pid)
            {
                tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    pData.Player.Notify("Cmd::TargetNotFound");

                    return;
                }
            }

            if (slot < 0 || drawable < 0 || texture < 0)
            {
                tData.UpdateClothes();
            }
            else
            {
                if (clothes)
                    tData.Player.SetClothes(slot, drawable, texture);
                else
                    tData.Player.SetAccessories(slot, drawable, texture);
            }
        }

        [Command("s_payday", 1)]
        private static void DoPayday(PlayerData pData, params string[] args)
        {
            Server.DoPayDay(false);
        }

        [Command("s_paydayX", 1)]
        private static void SetPaydayX(PlayerData pData, params string[] args)
        {
            if (args.Length != 1)
                return;

            byte x;

            if (!byte.TryParse(args[0], out x))
                return;

            if (x < 0)
                x = 1;
            else if (x > 10)
                x = 10;

            Server.PayDayX = x;
        }
    }
}
