using Google.Protobuf.WellKnownTypes;
using GTANetworkAPI;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BCRPServer.Events.Players
{
    class Commands : Script
    {
        private static Utils.Colour DefColour => new Utils.Colour(0, 0, 0);

        private class Command
        {
            public int PermissionLevel { get; set; }

            public Action<PlayerData, string[]> Action { get; set; }

            public Command(int PermissionLevel, Action<PlayerData, string[]> Action)
            {
                this.PermissionLevel = PermissionLevel;

                this.Action = Action;
            }

            public bool IsAllowed(PlayerData pData) => pData.AdminLevel >= PermissionLevel;
        }

        private static Dictionary<string, Command> All = new Dictionary<string, Command>()
        {
            {
                "p_kick",

                new Command(1, (pData, args) =>
                {
                    if (args.Length != 2)
                        return;

                    var reason = args[1];

                    uint pid;

                    if (!uint.TryParse(args[0], out pid))
                        return;

                    var target = pData.Player;

                    if (pData.CID != pid && pData.Player.Id != pid)
                    {
                        target = Utils.FindPlayerOnline(pid);

                        if (target?.Exists != true)
                        {
                            pData.Player.Notify("Cmd::TargetNotFound");

                            return;
                        }
                    }
                    else
                    {
                        return;
                    }

                    var tData = target.GetMainData();

                    if (tData == null)
                    {
                        Sync.Chat.SendGlobal(Sync.Chat.Types.Kick, pData.Player, reason, $"NOT_AUTH ({target.Id})");
                    }
                    else
                    {
                        Sync.Chat.SendGlobal(Sync.Chat.Types.Kick, pData.Player, reason, $"{tData.Name} {tData.Surname} ({target.Id})");
                    }

                    Utils.Kick(target, $"{pData.Name} {pData.Surname} #{pData.CID})", reason);
                })
            },

            {
                "p_skick",

                new Command(1, (pData, args) =>
                {
                    if (args.Length != 2)
                        return;

                    var reason = args[1];

                    uint pid;

                    if (!uint.TryParse(args[0], out pid))
                        return;

                    var target = pData.Player;

                    if (pData.CID != pid && pData.Player.Id != pid)
                    {
                        target = Utils.FindPlayerOnline(pid);

                        if (target?.Exists != true)
                        {
                            pData.Player.Notify("Cmd::TargetNotFound");

                            return;
                        }
                    }
                    else
                    {
                        return;
                    }

                    var tData = target.GetMainData();

                    if (tData == null)
                    {
                        Utils.MsgToAdmins(string.Format(Locale.Chat.Admin.SilentKick, $"{pData.Name} {pData.Surname} #{pData.CID})", $"NOT_AUTH ({target.Id})", reason));
                    }
                    else
                    {
                        Utils.MsgToAdmins(string.Format(Locale.Chat.Admin.SilentKick, $"{pData.Name} {pData.Surname} #{pData.CID})", $"{tData.Name} {tData.Surname} #{tData.CID})", reason));
                    }

                    Utils.Kick(target, $"{pData.Name} {pData.Surname} #{pData.CID})", reason);
                })
            },

            {
                "p_gm",

                new Command(1, (pData, args) =>
                {
                    if (args.Length != 2)
                        return;

                    uint pid;
                    bool state;

                    if (!uint.TryParse(args[0], out pid))
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

                    if (args[1].Length == 0)
                    {
                        tData.IsInvincible = !tData.IsInvincible;

                        return;
                    }
                    else
                    {
                        if (!bool.TryParse(args[1], out state))
                            return;

                        if (tData.IsInvincible == state)
                            return;

                        tData.IsInvincible = state;
                    }
                })
            },

            {
                "p_invis",

                new Command(1, (pData, args) =>
                {
                    if (args.Length != 2)
                        return;

                    uint pid;
                    bool state;

                    if (!uint.TryParse(args[0], out pid))
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

/*                    if (args[1].Length == 0)
                    {
                        tData.IsInvincible = !tData.IsInvincible;

                        return;
                    }
                    else
                    {
                        if (!bool.TryParse(args[1], out state))
                            return;

                        if (tData.IsInvincible == state)
                            return;

                        tData.IsInvincible = state;
                    }*/
                })
            },

            {
                "p_freeze",

                new Command(1, (pData, args) =>
                {
                    if (args.Length != 2)
                        return;

                    uint pid;
                    bool state;

                    if (!uint.TryParse(args[0], out pid))
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

                    if (args[1].Length == 0)
                    {
                        tData.IsFrozen = !tData.IsFrozen;

                        return;
                    }
                    else
                    {
                        if (!bool.TryParse(args[1], out state))
                            return;

                        if (tData.IsFrozen == state)
                            return;

                        tData.IsFrozen = state;
                    }
                })
            },

            {
                "p_tpp",

                new Command(1, (pData, args) =>
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
                        tData.Player.Vehicle.Teleport(new Vector3(x, y, z), null, null, false, Additional.AntiCheat.VehicleTeleportTypes.All);
                })
            },

            {
                "p_sdim",

                new Command(1, (pData, args) =>
                {
                    if (args.Length != 2)
                        return;

                    uint pid, dim;

                    if (!uint.TryParse(args[0], out pid) || !uint.TryParse(args[1], out dim))
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

                    tData.Player.Teleport(null, false, dim, null, false);
                })
            },

            {
                "p_tppl",

                new Command(1, (pData, args) =>
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
                })
            },

            {
                "p_tpveh",

                new Command(1, (pData, args) =>
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
                })
            },

            {
                "p_hp",

                new Command(1, (pData, args) =>
                {
                    if (args.Length != 2)
                        return;

                    uint pid;
                    int value;

                    if (!uint.TryParse(args[0], out pid) || !int.TryParse(args[1], out value))
                        return;

                    if (value < 0)
                        value = 0;
                    else if (value > 100)
                        value = 100;

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

                    tData.Player.SetHealth(value);
                })
            },

            {
                "p_mood",

                new Command(1, (pData, args) =>
                {
                    if (args.Length != 2)
                        return;

                    uint pid;
                    int value;

                    if (!uint.TryParse(args[0], out pid) || !int.TryParse(args[1], out value))
                        return;

                    if (value < 0)
                        value = 0;
                    else if (value > 100)
                        value = 100;

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

                    tData.Mood = value;
                })
            },

            {
                "p_satiety",

                new Command(1, (pData, args) =>
                {
                    if (args.Length != 2)
                        return;

                    uint pid;
                    int value;

                    if (!uint.TryParse(args[0], out pid) || !int.TryParse(args[1], out value))
                        return;

                    if (value < 0)
                        value = 0;
                    else if (value > 100)
                        value = 100;

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

                    tData.Satiety = value;
                })
            },

            {
                "p_tclothes",

                new Command(1, (pData, args) =>
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
                })
            },

            {
                "p_titem",

                new Command(1, (pData, args) =>
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

                    if (Game.Items.Items.GetType(id) == null)
                    {
                        pData.Player.Notify("Cmd::IdNotFound");

                        return;
                    }

                    Game.Items.Items.GiveItem(tData, id, variation, amount, true);
                })
            },

            {
                "p_item",

                new Command(1, (pData, args) =>
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

                    if (Game.Items.Items.GetType(id) == null)
                    {
                        pData.Player.Notify("Cmd::IdNotFound");

                        return;
                    }

                    Game.Items.Items.GiveItem(tData, id, variation, amount, false);
                })
            },

            {
                "veh_temp",

                new Command(1, (pData, args) =>
                {
                    if (args.Length != 2)
                        return;

                    var id = args[1];

                    uint pid;

                    if (!uint.TryParse(args[0], out pid))
                        return;

                    var vType = Game.Data.Vehicles.GetData(id);

                    if (vType == null)
                    {
                        pData.Player.Notify("Cmd::IdNotFound");

                        return;
                    }

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

                    var vData = VehicleData.NewTemp(tData, vType, DefColour, DefColour, tData.Player.Position, tData.Player.Heading, tData.Player.Dimension);

                    if (vData == null)
                        return;
                })
            },

            {
                "veh_new",

                new Command(1, (pData, args) =>
                {
                    if (args.Length != 2)
                        return;

                    var id = args[1];

                    uint pid;

                    if (!uint.TryParse(args[0], out pid))
                        return;

                    var vType = Game.Data.Vehicles.GetData(id);

                    if (vType == null)
                    {
                        pData.Player.Notify("Cmd::IdNotFound");

                        return;
                    }

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

                    var vData = VehicleData.New(tData, vType, DefColour, DefColour, tData.Player.Position, tData.Player.Heading, tData.Player.Dimension, true);

                    if (vData == null)
                        return;
                })
            },

            {
                "veh_del",

                new Command(1, (pData, args) =>
                {
                    if (args.Length != 2)
                        return;

                    uint vid;
                    bool completely;

                    if (!uint.TryParse(args[0], out vid) || !bool.TryParse(args[1], out completely))
                        return;

                    var vData = Utils.FindVehicleOnline(vid);

                    if (vData == null || vData.Vehicle?.Exists != true)
                    {
                        pData.Player.Notify("Cmd::TargetNotFound");

                        return;
                    }

                    vData.Delete(completely);
                })
            },

            {
                "veh_rs",

                new Command(1, (pData, args) =>
                {
                    if (args.Length != 1)
                        return;

                    uint vid;

                    if (!uint.TryParse(args[0], out vid))
                        return;

                    var vData = Utils.FindVehicleOnline(vid);

                    if (vData == null || vData.Vehicle?.Exists != true)
                    {
                        pData.Player.Notify("Cmd::TargetNotFound");

                        return;
                    }

                    vData.Respawn();
                })
            },

            {
                "w_iog_cl",

                new Command(1, (pData, args) =>
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
                })
            },
        };

        [RemoteEvent("Cmd::Exec")]
        private static void CmdExecute(Player player, string cmdId, string argStr)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (argStr == null)
                return;

            var cmdData = All.GetValueOrDefault(cmdId);

            if (cmdData == null)
                return;

            if (!cmdData.IsAllowed(pData))
                return;

            cmdData.Action?.Invoke(pData, argStr.Split('&'));
        }
    }
}
