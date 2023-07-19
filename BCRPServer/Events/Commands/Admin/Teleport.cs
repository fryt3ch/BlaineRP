using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Events.Commands
{
    partial class Commands
    {
        [Command("p_tpp", 1)]
        private static void TeleportToPosition(PlayerData pData, params string[] args)
        {
            if (args.Length != 5)
                return;

            uint pid;
            float x, y, z;
            bool toGround;

            if (!uint.TryParse(args[0], out pid) || !float.TryParse(args[1], out x) || !float.TryParse(args[2], out y) || !float.TryParse(args[3], out z) || !bool.TryParse(args[4], out toGround))
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

            if (tData.Player.Vehicle == null)
                tData.Player.Teleport(new Vector3(x, y, z), toGround, null, null, false);
            else
                tData.Player.Vehicle.Teleport(new Vector3(x, y, z), null, null, false, Additional.AntiCheat.VehicleTeleportTypes.All, toGround);
        }

        [Command("p_tppl", 1)]
        private static void TeleportPlayer(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            uint pid;
            bool here;

            if (!uint.TryParse(args[0], out pid) || !bool.TryParse(args[1], out here))
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
            else
            {
                return;
            }

            if (here)
            {
                tData.Player.Teleport(pData.Player.Position, false, pData.Player.Dimension, null, false);
            }
            else
            {
                pData.Player.Teleport(tData.Player.Position, false, tData.Player.Dimension, null, false);
            }
        }

        [Command("p_tpveh", 1)]
        private static void TeleportVehicle(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            uint vid;
            bool here;

            if (!uint.TryParse(args[0], out vid) || !bool.TryParse(args[1], out here))
                return;

            var vData = Utils.FindVehicleOnline(vid);

            if (vData == null || vData.Vehicle?.Exists != true)
            {
                pData.Player.Notify("Cmd::TargetNotFound");

                return;
            }

            if (here)
            {
                vData.Vehicle.Teleport(pData.Player.Position, pData.Player.Dimension, null, false, Additional.AntiCheat.VehicleTeleportTypes.Default);
            }
            else
            {
                pData.Player.Teleport(vData.Vehicle.Position, false, vData.Vehicle.Dimension, null, false);
            }
        }

        [Command("p_sdim", 1)]
        private static void SetDimension(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            uint pid, dim;

            if (!uint.TryParse(args[0], out pid) || !uint.TryParse(args[1], out dim))
                return;

            if (dim == 0)
                dim = Properties.Settings.Profile.Current.Game.MainDimension;

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

            tData.Player.Teleport(null, false, dim, null, false);
        }
    }
}
