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
        private static void TempVehicle(Player player, int pid, string id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!IsAllowed(pData, "Cmd::Vehicle::Temp"))
                return;

            var vType = Game.Data.Vehicles.GetData(id);

            if (vType == null)
            {
                player.Notify("Cmd::IdNotFound");

                return;
            }

            var tData = pData;

            if (pData.CID != pid && player.Id != pid)
            {
                tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    player.Notify("Cmd::TargetNotFound");

                    return;
                }
            }

            var vData = VehicleData.NewTemp(tData, vType, DefColour, DefColour, tData.Player.Position, tData.Player.Heading, tData.Player.Dimension);

            if (vData == null)
                return;
        }

        [RemoteEvent("Cmd::Vehicle")]
        private static void Vehicle(Player player, int pid, string id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!IsAllowed(pData, "Cmd::Vehicle"))
                return;

            var vType = Game.Data.Vehicles.GetData(id);

            if (vType == null)
            {
                player.Notify("Cmd::IdNotFound");

                return;
            }

            var tData = pData;

            if (pData.CID != pid && player.Id != pid)
            {
                tData = Utils.FindReadyPlayerOnline(pid);

                if (tData == null || tData.Player?.Exists != true)
                {
                    player.Notify("Cmd::TargetNotFound");

                    return;
                }
            }

            var vData = VehicleData.New(tData, vType, DefColour, DefColour, tData.Player.Position, tData.Player.Heading, tData.Player.Dimension, true);

            if (vData == null)
                return;
        }

        [RemoteEvent("Cmd::Vehicle::Delete")]
        private static void VehicleDelete(Player player, int vid, bool completely)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!IsAllowed(pData, "Cmd::Vehicle::Delete"))
                return;

            var vData = Utils.FindVehicleOnline(vid);

            if (vData == null || vData.Vehicle?.Exists != true)
            {
                player.Notify("Cmd::TargetNotFound");

                return;
            }

            vData.Delete(completely);
        }

        [RemoteEvent("Cmd::Vehicle::Respawn")]
        private static void VehicleRespawn(Player player, int vid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!IsAllowed(pData, "Cmd::Vehicle::Respawn"))
                return;

            var vData = Utils.FindVehicleOnline(vid);

            if (vData == null || vData.Vehicle?.Exists != true)
            {
                player.Notify("Cmd::TargetNotFound");

                return;
            }

            vData.Respawn();
        }
        #endregion

        #region Weapon
        [RemoteEvent("Cmd::Weapon::Temp")]
        private static void GiveTempWeapon(Player player, int pid, string id, int ammo)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!IsAllowed(pData, "Cmd::Weapon::Temp"))
                return;

            var tData = Utils.FindReadyPlayerOnline(pid);

            if (tData == null || tData.Player?.Exists != true)
            {
                player.Notify("Cmd::IdNotFound");

                return;
            }

            var target = tData.Player;

            Game.Items.Items.GiveItem(pData, id, 0, ammo, true);
        }
        #endregion

        #region Items
        [RemoteEvent("Cmd::Item::Temp")]
        private static void GiveTempItem(Player player, int pid, string id, int amount, int variation)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!IsAllowed(pData, "Cmd::Item::Temp"))
                return;

            if (Game.Items.Items.GetType(id) == null)
            {
                player.Notify("Cmd::IdNotFound");

                return;
            }

            var tData = Utils.FindReadyPlayerOnline(pid);

            var target = tData.Player;

            Game.Items.Items.GiveItem(pData, id, variation, amount, true);
        }

        [RemoteEvent("Cmd::Item")]
        private static void GiveItem(Player player, int pid, string id, int amount, int variation)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!IsAllowed(pData, "Cmd::Item::Temp"))
                return;

            if (Game.Items.Items.GetType(id) == null)
            {
                player.Notify("Cmd::IdNotFound");

                return;
            }

            var tData = Utils.FindReadyPlayerOnline(pid);

            var target = tData.Player;

            Game.Items.Items.GiveItem(pData, id, variation, amount, false);
        }

        [RemoteEvent("Cmd::Item::Info")]
        private static void ItemInfo(Player player, string id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!IsAllowed(pData, "Cmd::Item::Info"))
                return;

            var iClass = Game.Items.Items.GetType(id);

            if (iClass == null)
            {
                player.Notify("Cmd::IdNotFound");

                return;
            }

            string interfaces = string.Join(", ", iClass.GetInterfaces().Select(x => x.Name));

            player.TriggerEvent("Item::Info", id, iClass.BaseType.Name, iClass.Name, interfaces);
        }

        [RemoteEvent("Cmd::Items::Clear")]
        private static void ClearItems(Player player, int delay)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

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
        }
        #endregion

        [RemoteEvent("Cmd::TP::Pos")]
        private static void TeleportPosition(Player player, float x, float y, float z, bool toGround)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!IsAllowed(pData, "Cmd::TP::Pos"))
                return;

            player.Teleport(new Vector3(x, y, z), toGround);
        }

        [RemoteEvent("Cmd::TP::To")]
        private static void TeleportTo(Player player, int pid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

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
        }

        [RemoteEvent("Cmd::TP::To::Veh")]
        private static void TeleportToVeh(Player player, int vid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

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
        }

        [RemoteEvent("Cmd::TP::Get")]
        private static void TeleportGetHere(Player player, int pid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

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
        }

        [RemoteEvent("Cmd::TP::Get::Veh")]
        private static void TeleportGetHereVeh(Player player, int vid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

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
        }

        [RemoteEvent("Cmd::TP::Dim")]
        private static void SetDimension(Player player, int pid, uint dimension)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

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
        }

        [RemoteEvent("Cmd::Invis")]
        private static void Invisibility(Player player, bool toggle, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!IsAllowed(pData, "Cmd::Invis"))
                return;

            if (toggle)
            {
                if (player.Transparency == 0)
                    player.SetAlpha(255);
                else
                    player.SetAlpha(0);
            }
            else
                player.SetAlpha(state ? 0 : 255);

            pData.IsInvisible = player.Transparency == 0;
        }

        [RemoteEvent("Cmd::GM")]
        private static void GodMode(Player player, bool toggle, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

        }

        [RemoteEvent("Cmd::Clothes")]
        private static void SetClothes(Player player, int slot, int drawable, int texture, bool clothes)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!IsAllowed(pData, "Cmd::Clothes"))
                return;

            if (slot == -1)
                pData.UpdateClothes();
            else
            {
                if (clothes)
                    player.SetClothes(slot, drawable, texture);
                else
                    player.SetAccessories(slot, drawable, texture);
            }
        }

        [RemoteEvent("Cmd::Health")]
        private static void SetHealth(Player player, int pid, int value)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (value < 0)
                value = 0;
            else if (value > 100)
                value = 100;

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
        }

        [RemoteEvent("Cmd::Mood")]
        private static void Mood(Player player, int pid, int value)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (value < 0)
                value = 0;
            else if (value > 100)
                value = 100;

            if (!IsAllowed(pData, "Cmd::Mood"))
                return;

            var tData = Utils.FindReadyPlayerOnline(pid);

            if (tData == null || tData.Player?.Exists != true)
            {
                player.Notify("Cmd::TargetNotFound");

                return;
            }

            tData.Mood = value;
        }

        [RemoteEvent("Cmd::Satiety")]
        private static void Satiety(Player player, int pid, int value)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (value < 0)
                value = 0;
            else if (value > 100)
                value = 100;

            if (!IsAllowed(pData, "Cmd::Satiety"))
                return;

            var tData = Utils.FindReadyPlayerOnline(pid);

            if (tData == null || tData.Player?.Exists != true)
            {
                player.Notify("Cmd::TargetNotFound");

                return;
            }

            tData.Satiety = value;
        }

        [RemoteEvent("Cmd::Kick")]
        private static void Kick(Player player, int pid, string reason, bool silent)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

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
                Chat.SendGlobal(Chat.Types.Kick, player, reason, target.Name + $" ({target.Id})");
            else
                Utils.MsgToAdmins(string.Format(Locale.Chat.Admin.SilentKick, player.Name + $" #{pData.CID})", target.Name + $" ({target.Id})", reason));

            Utils.Kick(target, player.Name + $" #{pData.CID})", reason);
        }

        #region Freeze
        [RemoteEvent("Cmd::Freeze")]
        private static void Freeze(Player player, int pid, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

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
        }
        #endregion
    }
}
