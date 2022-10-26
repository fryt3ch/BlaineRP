using Google.Protobuf.WellKnownTypes;
using GTANetworkAPI;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.Sync
{
    class Commands : Script
    {
        private static Color DefColour = new Color(0, 0, 0);

        private static Dictionary<string, int> PermissionLevels = new Dictionary<string, int>()
        {
            { "Cmd::Vehicle::Temp", 1},
            { "Cmd::Vehicle", 1},
            { "Cmd::Vehicle::Delete", 1},
            { "Cmd::Vehicle::Respawn", 1},

            { "Cmd::Weapon", 1},
            { "Cmd::Weapon::Temp", 1},

            { "Cmd::Item", 1},
            { "Cmd::Item::Temp", 1},

            { "Cmd::Item::Info", 1},

            { "Cmd::Items::Clear", 1 },

            { "Cmd::TP::Pos", 1},
            { "Cmd::TP::Dim", 1},

            { "Cmd::Invis", 1},

            { "Cmd::Clothes", 1},

            { "Cmd::Health", 1},

            { "Cmd::Mood", 1},
            { "Cmd::Satiety", 1},
        };

        private static bool IsAllowed(PlayerData pData, string cmd)
        {
            if (pData.AdminLevel < (PermissionLevels.ContainsKey(cmd) ? PermissionLevels[cmd] : 1))
                return false;

            return true;
        }

        #region Vehicles
        [RemoteEvent("Cmd::Vehicle::Temp")]
        private static async Task TempVehicle(Player player, int pid, string id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::Vehicle::Temp"))
                    return;

                if (!Game.Data.Vehicles.All.ContainsKey(id))
                {
                    player.Notify("Cmd::IdNotFound");

                    return;
                }

                var tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    player.Notify("Cmd::TargetNotFound");

                    return;
                }

                var target = tData.Player;

                bool otherPlayer = false;

                var pos = target.Position;
                var rot = target.Rotation;
                var dim = target.Dimension;

                if (tData.CID != pData.CID)
                {
                    otherPlayer = true;

                    if (!await tData.WaitAsync())
                        return;
                }

                await Task.Run(async () =>
                {
                    var vData = await VehicleData.NewTemp(tData, id, DefColour, pos, rot, dim);

                    if (vData == null)
                        return;
                });

                if (otherPlayer)
                    tData.Release();
            });

            pData.Release();
        }

        [RemoteEvent("Cmd::Vehicle")]
        private static async Task Vehicle(Player player, int pid, string id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::Vehicle"))
                    return;

                if (!Game.Data.Vehicles.All.ContainsKey(id))
                {
                    player.Notify("Cmd::IdNotFound");

                    return;
                }

                var tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    player.Notify("Cmd::TargetNotFound");

                    return;
                }

                var target = tData.Player;

                bool otherPlayer = false;

                var pos = target.Position;
                var rot = target.Rotation;
                var dim = target.Dimension;


                if (tData.CID != pData.CID)
                {
                    otherPlayer = true;

                    if (!await tData.WaitAsync())
                        return;
                }

                await Task.Run(async () =>
                {
                    var vData = await VehicleData.New(tData, id, DefColour, pos, rot, dim);

                    if (vData == null)
                        return;
                });

                if (otherPlayer)
                    tData.Release();
            });

            pData.Release();
        }

        [RemoteEvent("Cmd::Vehicle::Delete")]
        private static async Task VehicleDelete(Player player, int vid, bool completely)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::Vehicle::Delete"))
                    return;

                var vData = Utils.FindVehicleOnline(vid);

                if (vData == null || vData.Vehicle?.Exists != true)
                {
                    player.Notify("Cmd::TargetNotFound");

                    return;
                }

                await Task.Run(() =>
                {
                    vData.Delete(false);
                });
            });

            pData.Release();
        }

        [RemoteEvent("Cmd::Vehicle::Respawn")]
        private static async Task VehicleRespawn(Player player, int vid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::Vehicle::Respawn"))
                    return;

                var vData = Utils.FindVehicleOnline(vid);

                if (vData == null || vData.Vehicle?.Exists != true)
                {
                    player.Notify("Cmd::TargetNotFound");

                    return;
                }

                await Task.Run(() =>
                {
                    vData.Respawn();
                });
            });

            pData.Release();
        }
        #endregion

        #region Weapon
        [RemoteEvent("Cmd::Weapon::Temp")]
        private static async Task GiveTempWeapon(Player player, int pid, string id, int ammo)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::Weapon::Temp"))
                    return;

                var tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    player.Notify("Cmd::IdNotFound");

                    return;
                }


                var target = tData.Player;

                bool otherPlayer = false;

                if (tData.CID != pData.CID)
                {
                    otherPlayer = true;

                    if (!await tData.WaitAsync())
                        return;
                }

                await Game.Items.Items.GiveItem(pData, id, 0, ammo, true);

                if (otherPlayer)
                    tData.Release();
            });

            pData.Release();
        }
        #endregion

        #region Items
        [RemoteEvent("Cmd::Item::Temp")]
        private static async Task GiveTempItem(Player player, int pid, string id, int amount, int variation)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::Item::Temp"))
                    return;

                if (Game.Items.Items.GetClass(id) == null)
                {
                    player.Notify("Cmd::IdNotFound");

                    return;
                }

                var tData = Utils.FindReadyPlayerOnline(pid);

                var target = tData.Player;

                bool otherPlayer = false;

                if (tData.CID != pData.CID)
                {
                    otherPlayer = true;

                    if (!await tData.WaitAsync())
                        return;
                }

                await Game.Items.Items.GiveItem(pData, id, variation, amount, true);

                if (otherPlayer)
                    tData.Release();
            });

            pData.Release();
        }

        [RemoteEvent("Cmd::Item")]
        private static async Task GIveItem(Player player, int pid, string id, int amount, int variation)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::Item::Temp"))
                    return;

                if (Game.Items.Items.GetClass(id) == null)
                {
                    player.Notify("Cmd::IdNotFound");

                    return;
                }

                var tData = Utils.FindReadyPlayerOnline(pid);

                var target = tData.Player;

                bool otherPlayer = false;

                if (tData.CID != pData.CID)
                {
                    otherPlayer = true;

                    if (!await tData.WaitAsync())
                        return;
                }

                await Game.Items.Items.GiveItem(pData, id, variation, amount, false);

                if (otherPlayer)
                    tData.Release();
            });

            pData.Release();
        }

        [RemoteEvent("Cmd::Item::Info")]
        private static async Task ItemInfo(Player player, string id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::Item::Info"))
                    return;

                var iClass = Game.Items.Items.GetClass(id);
                var iType = Game.Items.Items.GetType(id);

                if (iClass == null || iType == Game.Items.Item.Types.NotAssigned)
                {
                    player.Notify("Cmd::IdNotFound");

                    return;
                }

                string interfaces = string.Join(", ", iClass.GetInterfaces().Select(x => x.Name));

                player.TriggerEvent("Item::Info", id, iClass.Name, iType.ToString(), interfaces);
            });

            pData.Release();
        }

        [RemoteEvent("Cmd::Items::Clear")]
        private static async Task ClearItems(Player player, int delay)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::Items::Clear"))
                    return;

                if (delay > 60)
                    delay = 60;
                else if (delay < 0)
                {
                    Game.World.ClearAllItemsCancel();

                    return;
                }

                Game.World.ClearAllItems(delay);
            });

            pData.Release();
        }
        #endregion

        [RemoteEvent("Cmd::TP::Pos")]
        private static async Task TeleportPosition(Player player, float x, float y, float z, bool toGround)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::TP::Pos"))
                    return;

                player.Teleport(new Vector3(x, y, z), toGround);
            });

            pData.Release();
        }

        [RemoteEvent("Cmd::TP::To")]
        private static async Task TeleportTo(Player player, int pid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::TP::To"))
                    return;

                var tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    player.Notify("Cmd::TargetNotFound");

                    return;
                }

                var target = tData.Player;

                player.Teleport(target.Position, false, target.Dimension);
            });

            pData.Release();
        }

        [RemoteEvent("Cmd::TP::To::Veh")]
        private static async Task TeleportToVeh(Player player, int vid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::TP::To::Veh"))
                    return;

                var vData = Utils.FindVehicleOnline(vid);

                if (vData == null || vData.Vehicle?.Exists != true)
                {
                    player.Notify("Cmd::TargetNotFound");

                    return;
                }

                var target = vData.Vehicle;

                player.Teleport(target.Position, false, target.Dimension);
            });

            pData.Release();
        }

        [RemoteEvent("Cmd::TP::Get")]
        private static async Task TeleportGetHere(Player player, int pid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::TP::Get"))
                    return;

                var tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    player.Notify("Cmd::TargetNotFound");

                    return;
                }

                var target = tData.Player;

                target.Teleport(player.Position, false, player.Dimension);
            });

            pData.Release();
        }

        [RemoteEvent("Cmd::TP::Get::Veh")]
        private static async Task TeleportGetHereVeh(Player player, int vid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::TP::Get::Veh"))
                    return;

                var vData = Utils.FindVehicleOnline(vid);

                if (vData == null || vData.Vehicle?.Exists != true)
                {
                    player.Notify("Cmd::TargetNotFound");

                    return;
                }

                var target = vData.Vehicle;

                target.Position = player.Position;
                target.Dimension = player.Dimension;
            });

            pData.Release();
        }

        [RemoteEvent("Cmd::TP::Dim")]
        private static async Task SetDimension(Player player, int pid, uint dimension)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::TP::Dim"))
                    return;

                var tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    player.Notify("Cmd::TargetNotFound");

                    return;
                }

                var target = tData.Player;

                target.Teleport(null, false, dimension);
            });

            pData.Release();
        }

        [RemoteEvent("Cmd::Invis")]
        private static async Task Invisibility(Player player, bool toggle, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::Invis"))
                    return;

                if (toggle)
                {
                    if (player.Transparency == 0)
                        player.SetTransparency(255);
                    else
                        player.SetTransparency(0);
                }
                else
                    player.SetTransparency(state ? 0 : 255);

                pData.IsInvisible = player.Transparency == 0;
            });

            pData.Release();
        }

        [RemoteEvent("Cmd::GM")]
        private static async Task GodMode(Player player, bool toggle, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;



            pData.Release();
        }

        [RemoteEvent("Cmd::Clothes")]
        private static async Task SetClothes(Player player, int slot, int drawable, int texture, bool clothes)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::Clothes"))
                    return;

                if (slot == -1)
                    player.GetMainData()?.UpdateClothes();
                else
                {
                    if (clothes)
                        player.SetClothes(slot, drawable, texture);
                    else
                        player.SetAccessories(slot, drawable, texture);
                }
            });

            pData.Release();
        }

        [RemoteEvent("Cmd::Health")]
        private static async Task SetHealth(Player player, int pid, int value)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            if (value < 0)
                value = 0;
            else if (value > 100)
                value = 100;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::Health"))
                    return;

                var tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    player.Notify("Cmd::TargetNotFound");

                    return;
                }

                var target = tData.Player;

                target.SetHealth(value);
            });

            pData.Release();
        }

        [RemoteEvent("Cmd::Mood")]
        private static async Task Mood(Player player, int pid, int value)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            if (value < 0)
                value = 0;
            else if (value > 100)
                value = 100;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::Mood"))
                    return;

                var tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    player.Notify("Cmd::TargetNotFound");

                    return;
                }

                tData.Mood = value;
            });

            pData.Release();
        }

        [RemoteEvent("Cmd::Satiety")]
        private static async Task Satiety(Player player, int pid, int value)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            if (value < 0)
                value = 0;
            else if (value > 100)
                value = 100;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::Satiety"))
                    return;

                var tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    player.Notify("Cmd::TargetNotFound");

                    return;
                }

                tData.Satiety = value;
            });

            pData.Release();
        }

        [RemoteEvent("Cmd::Kick")]
        private static async Task Kick(Player player, int pid, string reason, bool silent)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::Kick"))
                    return;

                var target = Utils.FindPlayerOnline(pid);

                if (target?.Exists != true)
                {
                    player.Notify("Cmd::TargetNotFound");
                    return;
                }

                var tData = target.GetMainData();

                if (!silent)
                    Chat.SendGlobal(Chat.Type.Kick, player, reason, target.Name + $" ({target.Id})");
                else
                    Utils.MsgToAdmins(string.Format(Locale.Chat.Admin.SilentKick, player.Name + $" #{pData.CID})", target.Name + $" ({target.Id})", reason));

                Utils.Kick(target, player.Name + $" #{pData.CID})", reason);
            });

            pData.Release();
        }

        #region Freeze
        [RemoteEvent("Cmd::Freeze")]
        private static async Task Freeze(Player player, int pid, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (!IsAllowed(pData, "Cmd::Freeze"))
                    return;

                var target = Utils.FindPlayerOnline(pid);

                if (target?.Exists != true)
                {
                    player.Notify("Cmd::TargetNotFound");

                    return;
                }

                if (state)
                {
                    if (target.Vehicle != null)
                        target.WarpOutOfVehicle();
                }

                target.TriggerEvent("Players::Freeze", state, pData.CID);
            });

            pData.Release();
        }
        #endregion
    }
}
