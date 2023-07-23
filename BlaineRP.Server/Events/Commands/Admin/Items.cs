namespace BlaineRP.Server.Events.Commands
{
    partial class Commands
    {
        [Command("p_item", 1)]
        private static void GiveItem(PlayerData pData, params string[] args)
        {
            if (args.Length != 4)
                return;

            var id = args[1];

            uint pid;
            int amount, variation;

            if (!uint.TryParse(args[0], out pid) || !int.TryParse(args[2], out amount) || !int.TryParse(args[3], out variation))
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

            if (Game.Items.Stuff.GetType(id) == null)
            {
                pData.Player.Notify("Cmd::IdNotFound");

                return;
            }

            tData.GiveItem(out _, id, variation, amount);
        }

        [Command("p_titem", 1)]
        private static void GiveTempItem(PlayerData pData, params string[] args)
        {
            if (args.Length != 4)
                return;

            var id = args[1];

            uint pid;
            int amount, variation;

            if (!uint.TryParse(args[0], out pid) || !int.TryParse(args[2], out amount) || !int.TryParse(args[3], out variation))
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

            if (Game.Items.Stuff.GetType(id) == null)
            {
                pData.Player.Notify("Cmd::IdNotFound");

                return;
            }

            tData.GiveItem(out _, id, variation, amount); // not temp actually
        }

        [Command("w_iog_cl", 1)]
        private static void WorldItemsOnGroundClear(PlayerData pData, params string[] args)
        {
            if (args.Length != 1)
                return;

            int delay;

            if (!int.TryParse(args[0], out delay))
                return;

            if (delay > 60)
            {
                delay = 60;
            }
            else if (delay < 0)
            {
                Sync.World.ClearAllItemsCancel();

                return;
            }

            Sync.World.ClearAllItems(delay);
        }
    }
}
