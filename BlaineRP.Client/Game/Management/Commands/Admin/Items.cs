﻿using System.Linq;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.UI.CEF;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management.Commands
{
    partial class Service
    {
        [Command("clearitems", true, "Удалить все выброшенные предметы", "cleariog", "ciog")]
        public static void ClearItems(uint delay = 30)
        {
            if (delay > 60)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("w_iog_cl", delay);

            LastSent = World.Core.ServerTime;
        }

        [Command("clearitems_cancel", true, "Отменить удаление выброшенных предметов", "cleariog_cancel", "ciog_cancel")]
        public static void ClearItemsCancel()
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("w_iog_cl", -1);

            LastSent = World.Core.ServerTime;
        }


        [Command("tempitem", true, "Выдать себе предмет (временный)", "titem")]
        public static void TempItem(string id, uint amount = 1, uint variation = 0)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_titem", Player.LocalPlayer.RemoteId, id, amount, variation);

            LastSent = World.Core.ServerTime;
        }

        [Command("give_tempitem", true, "Выдать предмет игроку (временный)", "give_titem")]
        public static void GiveTempItem(uint pid, string id, uint amount = 1, uint variation = 0)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_titem", pid, id, amount, variation);

            LastSent = World.Core.ServerTime;
        }

        [Command("item", true, "Выдать себе предмет")]
        public static void Item(string id, uint amount = 1, uint variation = 0)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_item", Player.LocalPlayer.RemoteId, id, amount, variation);

            LastSent = World.Core.ServerTime;
        }

        [Command("give_item", true, "Выдать предмет игроку")]
        public static void GiveItem(uint pid, string id, uint amount = 1, uint variation = 0)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_item", pid, id, amount, variation);

            LastSent = World.Core.ServerTime;
        }

        [Command("iteminfo", true, "Запросить информацию о предмете", "iinfo", "itemi")]
        public static void ItemInfo(string id)
        {
            if (id == null)
                return;

            System.Type type = Items.Core.GetType(id, true);

            if (type == null)
            {
            }
            else
            {
                Notification.Show(Notification.Types.Information,
                    Locale.Notifications.Commands.Item.Header,
                    string.Format(Locale.Notifications.Commands.Item.Info,
                        id,
                        Items.Core.GetName(id),
                        type.BaseType.Name,
                        type.Name,
                        string.Join(", ", type.GetInterfaces().Select(x => x.Name))
                    ),
                    10000
                );
            }
        }

        [Command("tempweapon", true, "Выдать себе оружие (временное)", "tweapon", "tgun", "tempgun")]
        public static void TempWeapon(string id, uint ammo = 0)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_titem", Player.LocalPlayer.RemoteId, id.StartsWith("w_") ? id : "w_" + id, ammo, 0);

            LastSent = World.Core.ServerTime;
        }

        [Command("give_tempweapon", true, "Выдать оружие игроку (временное)", "give_tweapon", "give_tgun", "give_tempgun")]
        public static void GiveTempWeapon(uint pid, string id, uint ammo = 0)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            CallRemote("p_titem", pid, id.StartsWith("w_") ? id : "w_" + id, ammo, 0);

            LastSent = World.Core.ServerTime;
        }
    }
}