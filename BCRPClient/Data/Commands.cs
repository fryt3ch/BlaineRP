using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BCRPClient.Data
{
    class Commands : Events.Script
    {
        private static DateTime LastSent;

        private static List<Instance> All;

        #region Classes
        [AttributeUsage(AttributeTargets.Method)]
        /// <summary>Класс, служащий для хранения информации о команде</summary>
        private class CommandAttribute : Attribute
        {
            /// <summary>Основное название команды</summary>
            public string Name { get; set; }
            /// <summary>Псевдонимы</summary>
            public string[] Aliases { get; set; }
            /// <summary>Доступна ли эта команда только администраторам?</summary>
            public bool AdminOnly { get; set; }
            /// <summary>Описание команды</summary>
            public string Description { get; set; }

            /// <summary>Информация о новой команде<br/><br/>Параметры метода, который содержит этот аттрибут, должны быть IConvertable!<br/>В противном случае, команда не будет загружена</summary>
            /// <param name="Name">Название</param>
            /// <param name="AdminOnly">Доступна ли только администраторам?</param>
            /// <param name="Aliases">Псевдонимы</param>
            public CommandAttribute(string Name, bool AdminOnly, string Description, params string[] Aliases)
            {
                this.Name = Name;
                this.AdminOnly = AdminOnly;
                this.Aliases = Aliases;

                this.Description = Description;
            }
        }

        /// <summary>Класс, служащий для хранения информации о команде и её методе</summary>
        private class Instance
        {
            /// <summary>Данные команды</summary>
            public CommandAttribute Attribute { get; }
            /// <summary>Параметры команды</summary>
            public ParameterInfo[] Parameters { get; }
            /// <summary>Данные метода команды</summary>
            public MethodInfo MethodInfo { get; }

            public Instance(MethodInfo MethodInfo)
            {
                this.MethodInfo = MethodInfo;

                this.Parameters = MethodInfo.GetParameters();
                this.Attribute = MethodInfo.GetCustomAttribute<CommandAttribute>();
            }
        }
        #endregion

        #region All Commands

        #region Admin Commands

        #region Vehicle
        [Command("tempvehicle", true, "Выдать себе транспорт (временный)", "tveh", "tvehicle")]
        public static void TempVehicle(string id)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Vehicle::Temp", Player.LocalPlayer.RemoteId, id);

            LastSent = DateTime.Now;
        }

        [Command("give_tempvehicle", true, "Выдать себе транспорт (временный)", "give_tveh", "give_tvehile")]
        public static void GiveTempVehicle(uint pid, string id)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Vehicle::Temp", pid, id);

            LastSent = DateTime.Now;
        }

        [Command("vehicle", true, "Выдать себе транспорт", "veh")]
        public static void Vehicle(string id)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Vehicle", Player.LocalPlayer.RemoteId, id);

            LastSent = DateTime.Now;
        }

        [Command("give_vehicle", true, "Выдать транспорт игроку", "give_veh")]
        public static void GiveVehicle(uint pid, string id)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Vehicle", pid, id);

            LastSent = DateTime.Now;
        }

        [Command("respawnvehicle", true, "Респавн транспорта", "respawnveh")]
        public static void RespawnVehicle(uint vid)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Vehicle::Respawn", vid);

            LastSent = DateTime.Now;
        }

        [Command("deletevehicle", true, "Удалить транспорт (с сервера)", "delveh")]
        public static void DeleteVehicle(uint vid)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Vehicle::Delete", vid, false);

            LastSent = DateTime.Now;
        }

        [Command("deletevehicle_full", true, "Удалить транспорт (полностью!)", "delveh_full")]
        public static void DeleteVehicleFull(uint vid)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Vehicle::Delete", vid, true);

            LastSent = DateTime.Now;
        }
        #endregion

        #region Items

        [Command("clearitems", true, "Удалить все выброшенные предметы", "cleariog", "ciog")]
        public static void ClearItems(uint delay = 30)
        {
            if (delay > 60)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Items::Clear", delay);

            LastSent = DateTime.Now;
        }

        [Command("clearitems_cancel", true, "Отменить удаление выброшенных предметов", "cleariog_cancel", "ciog_cancel")]
        public static void ClearItemsCancel()
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Items::Clear", -1);

            LastSent = DateTime.Now;
        }

        #region Weapon
        [Command("tempweapon", true, "Выдать себе оружие (временное)", "tweapon", "tgun", "tempgun")]
        public static void TempWeapon(string id, uint ammo = 0)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Weapon::Temp", Player.LocalPlayer.RemoteId, id.StartsWith("w_") ? id : "w_" + id, ammo);

            LastSent = DateTime.Now;
        }

        [Command("give_tempweapon", true, "Выдать оружие игроку (временное)", "give_tweapon", "give_tgun", "give_tempgun")]
        public static void GiveTempWeapon(uint pid, string id, uint ammo = 0)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Weapon::Temp", pid, id.StartsWith("w_") ? id : "w_" + id, ammo);

            LastSent = DateTime.Now;
        }
        #endregion

        [Command("tempitem", true, "Выдать себе предмет (временный)", "titem")]
        public static void TempItem(string id, uint amount = 1, uint variation = 0)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Item::Temp", Player.LocalPlayer.RemoteId, id, amount, variation);

            LastSent = DateTime.Now;
        }

        [Command("give_tempitem", true, "Выдать предмет игроку (временный)", "give_titem")]
        public static void GiveTempItem(uint pid, string id, uint amount = 1, uint variation = 0)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Item::Temp", pid, id, amount, variation);

            LastSent = DateTime.Now;
        }

        [Command("item", true, "Выдать себе предмет")]
        public static void Item(string id, uint amount = 1, uint variation = 0)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Item", Player.LocalPlayer.RemoteId, id, amount, variation);

            LastSent = DateTime.Now;
        }

        [Command("give_item", true, "Выдать предмет игроку")]
        public static void GiveItem(uint pid, string id, uint amount = 1, uint variation = 0)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Item", pid, id, amount, variation);

            LastSent = DateTime.Now;
        }

        [Command("iteminfo", true, "Запросить информацию о предмете", "iinfo", "itemi")]
        public static void ItemInfo(string id)
        {
            if (id == null)
                return;

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Item::Info", id);

            LastSent = DateTime.Now;
        }

        #endregion

        #region Teleports

        #region Teleport Marker
        [Command("teleportmarker", true, "Телепорт по метке", "tpmarker", "tpm")]
        public static void TeleportMarker()
        {
            var position = GameEvents.WaypointPosition;

            if (position == null)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.Commands.Header, Locale.Notifications.Commands.Teleport.NoWaypoint);

                return;
            }

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::TP::Pos", position.X, position.Y, position.Z, true);

            LastSent = DateTime.Now;
        }

        [Command("auto_tpm", true, "Автоматически телепортироваться по метке", "autotpm")]
        public static void AutoTeleportMarker(bool? state = null)
        {
            if (state == null)
                Settings.Other.AutoTeleportMarker = !Settings.Other.AutoTeleportMarker;
            else
                Settings.Other.AutoTeleportMarker = (bool)state;

            CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Notifications.Commands.Header, string.Format(Settings.Other.AutoTeleportMarker ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "AutoTPMarker"));
        }
        #endregion

        #region Teleport Position
        [Command("teleportpos", true, "Телепорт по координатам", "tppos", "tpp")]
        public static void TeleportPosition(float x, float y, float z, bool toGround = false)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::TP::Pos", x, y, z, toGround);

            LastSent = DateTime.Now;
        }
        #endregion

        [Command("tpto", true, "Телепорт к игроку", "goto", "tpp")]
        public static void TeleportToPlayer(uint pid)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::TP::To", pid);

            LastSent = DateTime.Now;
        }

        [Command("tptoveh", true, "Телепорт к транспорту", "gotoveh", "tpv")]
        public static void TeleportToVehicle(uint vid)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::TP::To::Veh", vid);

            LastSent = DateTime.Now;
        }

        [Command("tphere", true, "Телепорт игрока к себе", "gethere", "thp")]
        public static void GetHerePlayer(uint pid)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::TP::Get", pid);

            LastSent = DateTime.Now;
        }

        [Command("tphereveh", true, "Телепорт транспорта к себе", "gethereveh", "thv")]
        public static void GetHereVehicle(uint vid)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::TP::Get::Veh", vid);

            LastSent = DateTime.Now;
        }

        [Command("setdim", true, "Смена измерения", "sdim")]
        public static void SetDimension(uint pid, uint dimension)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::TP::Dim", pid, dimension);

            LastSent = DateTime.Now;
        }

        [Command("anim", true, "Смена измерения", "playanim")]
        public static void PlayAnim(string dict, string name, float sp, float spMult, int dur, int fg, float pRate, bool p1 = false, bool p2 = false, bool p3 = false)
        {
            Utils.RequestAnimDict(dict).GetAwaiter().GetResult();

            Player.LocalPlayer.TaskPlayAnim(dict, name, sp, spMult, dur, fg, pRate, p1, p2, p3);

            LastSent = DateTime.Now;
        }

        [Command("anim_stop", true, "Смена измерения", "stopanim")]
        public static void StopAnim()
        {
            Player.LocalPlayer.ClearTasks();
            Player.LocalPlayer.ClearTasksImmediately();

            LastSent = DateTime.Now;
        }

        #endregion

        #region Invisibility && GM
        [Command("invisibility", true, "Включить/выключить невидимость", "inv", "invis")]
        public static void Invisibility(bool? state = null)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Invis", state == null, state ?? false);
        }

        public static void GodMode(bool? state = null)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::GM", state == null, state ?? false);
        }
        #endregion

        #region Punishments
        [Command("kick", true, "Кикнуть игрока")]
        public static void Kick(uint pid, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Kick", pid, reason, false);

            LastSent = DateTime.Now;
        }

        [Command("silentkick", true, "Кикнуть игрока (тихо)", "skick", "kicks", "kicksilent")]
        public static void SilentKick(uint pid, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Kick", pid, reason, true);

            LastSent = DateTime.Now;
        }

        [Command("mute", true, "Выдать мут игроку")]
        public static void Mute(uint pid, uint mins, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Mute", pid, mins, reason);

            LastSent = DateTime.Now;
        }

        [Command("unmute", true, "Снять мут с игрока")]
        public static void Unmute(uint pid, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Unmute", pid, reason);

            LastSent = DateTime.Now;
        }

        [Command("warn", true, "Выдать предупреждение игроку")]
        public static void Warn(uint pid, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Warn", pid, reason);

            LastSent = DateTime.Now;
        }

        [Command("unwarn", true, "Снять предупреждения с игрока")]
        public static void Unwarn(uint pid, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Unwarn", pid, reason);

            LastSent = DateTime.Now;
        }

        [Command("ban", true, "Выдать бан игроку")]
        public static void Ban(uint pid, uint days, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Ban", pid, days, reason);

            LastSent = DateTime.Now;
        }

        [Command("unban", true, "Снять бан с игрока")]
        public static void Unban(uint pid, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Unban", pid, reason);

            LastSent = DateTime.Now;
        }

        [Command("hardban", true, "Выдать тяжёлый бан игроку")]
        public static void HardBan(uint pid, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Ban::Hard", pid, reason);

            LastSent = DateTime.Now;
        }

        [Command("unhardban", true, "Снять тяжёлый бан с игрока")]
        public static void Unhardban(uint pid, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Unban::Hard", pid, reason);

            LastSent = DateTime.Now;
        }

        [Command("jail", true, "Посадить игрока в NonRP-тюрьму")]
        public static void Jail(uint pid, uint mins, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Jail", pid, mins, reason);

            LastSent = DateTime.Now;
        }

        [Command("unjail", true, "Выпустить игрока из NonRP-тюрьмы")]
        public static void Unjail(uint pid, string reason)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Unjail", pid, reason);

            LastSent = DateTime.Now;
        }
        #endregion

        [Command("setclothes", true, "Надеть временную одежду", "sc", "sclothes")]
        public static void SetClothes(uint slot, uint drawable, uint texture = 0)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Clothes", slot, drawable, texture, true);

            LastSent = DateTime.Now;
        }

        [Command("setaccs", true, "Надеть временный аксессуар", "sa", "saccs")]
        public static void SetAccs(uint slot, uint drawable, uint texture = 0)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Clothes", slot, drawable, texture, false);

            LastSent = DateTime.Now;
        }

        [Command("resetclothes", true, "Сбросить временную одежду (и аксессуары)", "rsc", "rsclothes")]
        public static void ResetClothes()
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Clothes", -1, -1, -1, true);

            LastSent = DateTime.Now;
        }

        [Command("sethealth", true, "Установить здоровье (игроку)", "sethp", "shp")]
        public static void SetHealth(uint pid, uint value)
        {
            if (value > 100)
                value = 100;

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Health", pid, value);

            LastSent = DateTime.Now;
        }

        [Command("health", true, "Установить здоровье (себе)", "hp")]
        public static void Health(uint value)
        {
            if (value > 100)
                value = 100;

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Health", Player.LocalPlayer.RemoteId, value);

            LastSent = DateTime.Now;
        }

        [Command("mood", true, "Установить настроение (себе)")]
        public static void Mood(uint value)
        {
            if (value > 100)
                value = 100;

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Mood", Player.LocalPlayer.RemoteId,value);

            LastSent = DateTime.Now;
        }

        [Command("satiety", true, "Установить сытость (себе)")]
        public static void Satiety(uint value)
        {
            if (value > 100)
                value = 100;

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::Satiety", Player.LocalPlayer.RemoteId, value);

            LastSent = DateTime.Now;
        }

        [Command("setmood", true, "Установить настроение игроку")]
        public static void SetMood(uint pid, uint value)
        {
            if (value > 100)
                value = 100;

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::SetMood", pid, value);

            LastSent = DateTime.Now;
        }

        [Command("setsatiety", true, "Установить сытость игроку")]
        public static void SetSatiety(uint pid, uint value)
        {
            if (value > 100)
                value = 100;

            if (LastSent.IsSpam(1000, false, true))
                return;

            Events.CallRemote("Cmd::SetSatiety", pid, value);

            LastSent = DateTime.Now;
        }

        #endregion

        #region Debug Labels (DL)
        [Command("dl", false, "Показ дополнительных сведений о всех сущностях")]
        public static void DebugLabels(bool? state = null)
        {
            if (state == null)
                Settings.Other.DebugLabels = !Settings.Other.DebugLabels;
            else
                Settings.Other.DebugLabels = (bool)state;

            CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Notifications.Commands.Header, string.Format(Settings.Other.DebugLabels ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "DL"));
        }

        [Command("raytrace", false, "Показ дополнительных сведений о всех сущностях")]
        public static void Raytrace(bool? state = null)
        {
            if (state == null)
                Settings.Other.RaytraceEnabled = !Settings.Other.RaytraceEnabled;
            else
                Settings.Other.RaytraceEnabled = (bool)state;

            CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Notifications.Commands.Header, string.Format(Settings.Other.RaytraceEnabled ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "Raytace"));
        }
        #endregion

        #region Chat
        [Command("fontsize", false, "Задать размер шрифта в чате", "fsize")]
        public static void FontSize(int value)
        {
            if (value > 30 || value < 1)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.Commands.Chat.Header, Locale.Notifications.Commands.Chat.WrongValue);

                return;
            }

            Settings.Chat.FontSize = value;
        }

        [Command("chatheight", false, "Задать высоту чата" , "cheight")]
        public static void ChatHeight(int value)
        {
            if (value > 276 || value < 0)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.Commands.Chat.Header, Locale.Notifications.Commands.Chat.WrongValue);

                return;
            }

            Settings.Chat.Height = value;
        }

        [Command("chathide", false, "Скрыть/показать чат" , "chide")]
        public static void ChatHide()
        {
            Settings.Chat.Height = Settings.Chat.Height == 0 ? Settings.Chat.Default.Height : 0;
        }
        #endregion

        #region Other Stuff

        #region Lock
        [Command("lock", false, "Блокировка транспорта")]
        public static void Lock(uint? id = null, bool? state = null)
        {
            if (id == null)
                Sync.Vehicles.Lock(state, null);
            else
            {
                var veh = RAGE.Elements.Entities.Vehicles.Streamed.Where(x => x?.RemoteId == id).FirstOrDefault();

                if (veh == null)
                {
                    CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.Interaction.Header, Locale.Notifications.Interaction.NotFound);

                    return;
                }

                if (Vector3.Distance(Player.LocalPlayer.Position, veh.Position) > 10f)
                {
                    CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.Interaction.Header, Locale.Notifications.Interaction.DistanceTooLarge);

                    return;
                }

                Sync.Vehicles.Lock(state, veh);
            }
        }
        #endregion

        #region Colshapes
        [Command("colshape_delete", true, "Удалить колшейп", "cs_del")]
        public static void ColshapeDelete(uint id)
        {
            var cs = Additional.ExtraColshape.GetById((int)id);

            cs?.Delete();
        }

        [Command("poly_stop", true, "Закончить создание полигона")]
        public static void PolygonStop()
        {
            if (Additional.ExtraColshapes.PolygonCreationTask != null)
            {
                Additional.ExtraColshapes.PolygonCreationTask.Cancel();
                Additional.ExtraColshapes.PolygonCreationTask = null;

                Additional.ExtraColshapes.TempPolygon = null;
            }
        }

        [Command("poly_start", true, "Начать создание полигона")]
        public static void PolygonStart(float height = 0, float step = 1f)
        {
            if (step <= 0f)
                return;

            var newVertice = Player.LocalPlayer.Position;
            newVertice.Z -= 0.5f;

            if (Additional.ExtraColshapes.PolygonCreationTask != null)
            {
                Additional.ExtraColshapes.PolygonCreationTask.Cancel();

                Additional.ExtraColshapes.TempPolygon?.Delete();
            }

            Additional.ExtraColshapes.TempPolygon = new Additional.Polygon(new List<Vector3>() { newVertice }, height, 0f, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null);

            Additional.ExtraColshapes.PolygonCreationTask = new AsyncTask(() =>
            {
                if (Additional.ExtraColshapes.TempPolygon == null)
                    return true;

                var newVertice = Player.LocalPlayer.Position;
                newVertice.Z -= 1f;

                var vertices = Additional.ExtraColshapes.TempPolygon.Vertices;

                if (vertices[vertices.Count - 1].DistanceTo(newVertice) < step)
                    return false;

                Additional.ExtraColshapes.TempPolygon.AddVertice(newVertice);

                //Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Polygon_{(height > 0 ? "3D" : "2D")}] New pos: {newVertice}");

                return false;
            }, 100, true, 0);

            Additional.ExtraColshapes.PolygonCreationTask.Run();
        }

        [Command("poly_rotate", true, "Повернуть полигон", "poly_rt")]
        public static void PolygonRotate(uint id, float angle)
        {
            var col = Additional.ExtraColshape.GetById((int)id);

            if (!(col is Additional.Polygon))
                return;

            (col as Additional.Polygon).Rotate(angle);
        }

        [Command("poly_angle", true, "Задать поворот полигона", "poly_ang")]
        public static void PolygonAngle(uint id, float angle)
        {
            var col = Additional.ExtraColshape.GetById((int)id);

            if (!(col is Additional.Polygon))
                return;

            (col as Additional.Polygon).SetHeading(angle);
        }

/*        [Command("colshape_new_cuboid2d", true, "Создать КШ Прямоугольник", "cs_n_c2d")]
        public static void ColshapeNewRectangle(float width = 0, float depth = 0)
        {
            if (width < 0 || depth < 0)
                return;

            var newVertice = Player.LocalPlayer.Position;
            newVertice.Z -= 1f;

            if (width != 0 && depth != 0)
            {
                Additional.ExtraColshape.CreateRectangle(newVertice, width, depth, Player.LocalPlayer.GetHeading(), Player.LocalPlayer.Dimension, Utils.RedColor, 125, false);

                Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Cuboid_2D] Pos: {newVertice} | Width: {width} | Depth: {depth}");
            }
            else if (Additional.ExtraColshape.TempPosition == null)
            {
                Additional.ExtraColshape.TempPosition = newVertice;

                Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Cuboid_2D] Pos1: {newVertice}");
            }
            else
            {
                Additional.ExtraColshape.CreateRectangle(Additional.ExtraColshape.TempPosition, newVertice, 0, Player.LocalPlayer.Dimension, Utils.RedColor, 125, false);

                Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Cuboid_2D] Pos2: {newVertice}");

                Additional.ExtraColshape.TempPosition = null;
            }
        }*/

        [Command("colshape_new_circle", true, "Создать КШ Круг", "cs_n_crl")]
        public static void ColshapeNewCircle(float radius)
        {
            if (radius <= 0)
                return;

            var newVertice = Player.LocalPlayer.Position;
            newVertice.Z -= 1f;

            new Additional.Circle(newVertice, radius, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null);

            Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Circle_2D] Pos: {newVertice} | Radius: {radius}");
        }

        [Command("colshape_new_tube", true, "Создать КШ Цилиндр", "cs_n_tube")]
        public static void ColshapeNewTube(float radius, float height)
        {
            if (radius <= 0 || height <= 0)
                return;

            var newVertice = Player.LocalPlayer.Position;
            newVertice.Z -= 1f;

            new Additional.Cylinder(newVertice, radius, height, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null);

            Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Tube_3D] Pos: {newVertice} | Radius: {radius} | Height: {height}");
        }

        [Command("colshape_new_sphere", true, "Создать КШ Сфера", "cs_n_sph")]
        public static void ColshapeNewSphere(float radius)
        {
            if (radius <= 0)
                return;

            var newVertice = Player.LocalPlayer.Position;
            newVertice.Z -= 1f;

            new Additional.Sphere(newVertice, radius, false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension, null);

            Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Sphere_3D] Pos: {newVertice} | Radius: {radius}");
        }

        [Command("colshape_new_cuboid3d", true, "Создать КШ Параллелепипед", "cs_n_c3d")]
        public static void ColshapeNew3d(float sX = 0, float sY = 0, float height = 0)
        {
            if (sX < 0 || sY < 0 || height < 0)
                return;

            var newVertice = Player.LocalPlayer.Position;
            newVertice.Z -= 1f;

            if (sX != 0 && sY != 0)
            {
                Additional.Polygon.CreateCuboid(newVertice, sX, sY, height, Player.LocalPlayer.GetHeading(), false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension);

                Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Cuboid_3D] Pos: {newVertice} | Width: {sX} | Depth: {sY} | Height: {height}");
            }
            else if (Additional.ExtraColshapes.TempPosition == null)
            {
                Additional.ExtraColshapes.TempPosition = newVertice;

                Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Cuboid_3D] Pos1: {newVertice}");
            }
            else
            {
                Additional.Polygon.CreateCuboid(Additional.ExtraColshapes.TempPosition, newVertice, Player.LocalPlayer.GetHeading(), false, new Utils.Colour(255, 0, 0, 255), Player.LocalPlayer.Dimension);

                Events.CallLocal("Chat::ShowServerMessage", $"[TColshapes::Cuboid_3D] Pos2: {newVertice}");

                Additional.ExtraColshapes.TempPosition = null;
            }
        }

        [Command("poly_addvertice", true, "Добавить вершину к полигону", "poly_addv")]
        public static void PolyAddVertice(uint id)
        {
            var poly = Additional.ExtraColshape.GetById((int)id) as Additional.Polygon;

            if (poly == null)
                return;

            var newVertice = Player.LocalPlayer.Position;
            newVertice.Z -= 1f;

            poly.AddVertice(newVertice);
        }

        [Command("poly_removevertice", true, "Удалить вершину у полигона", "poly_rmv")]
        public static void PolyRemoveVertice(uint id, uint vertId)
        {
            var poly = Additional.ExtraColshape.GetById((int)id) as Additional.Polygon;

            if (poly == null)
                return;

            poly.RemoveVertice((int)vertId);
        }

        [Command("poly_setheight", true, "Задать высоту полигону", "poly_sheight")]
        public static void PolySetHeight(uint id, float height)
        {
            if (height < 0f)
                return;

            var poly = Additional.ExtraColshape.GetById((int)id) as Additional.Polygon;

            if (poly == null)
                return;

            poly.SetHeight(height);
        }

        [Command("highpolymode", true, "Сменить режим вида полигонов", "hpolymode")]
        public static void HighPolyMode(bool? state = null)
        {
            if (state == null)
                Settings.Other.HighPolygonsMode = !Settings.Other.HighPolygonsMode;
            else
                Settings.Other.HighPolygonsMode = (bool)state;

            CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Notifications.Commands.Header, string.Format(Settings.Other.HighPolygonsMode ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "HighPolyMode"));
        }

        [Command("colshapes_visible", true, "Сменить видимость колшейпов", "cs_vis")]
        public static void ColshapesVisible(bool? state = null)
        {
            if (state == null)
                Settings.Other.ColshapesVisible = !Settings.Other.ColshapesVisible;
            else
                Settings.Other.ColshapesVisible = (bool)state;

            CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Notifications.Commands.Header, string.Format(Settings.Other.ColshapesVisible ? Locale.Notifications.Commands.Enabled : Locale.Notifications.Commands.Disabled, "ColshapesVisible"));
        }
        #endregion

        [Command("loadipl", true, "Загрузить IPL", "lipl")]
        public static void LoadIpl(string ipl, float x = 0, float y = 0, float z = 0)
        {
            var interior = RAGE.Game.Interior.GetInteriorAtCoords(x, y, z);

            RAGE.Game.Streaming.RequestIpl(ipl);

            RAGE.Game.Interior.RefreshInterior(interior);
        }

        [Command("unloadipl", true, "Выгрузить IPL", "unipl")]
        public static void RemoveIpl(string ipl, float x = 0, float y = 0, float z = 0)
        {
            var interior = RAGE.Game.Interior.GetInteriorAtCoords(x, y, z);

            RAGE.Game.Streaming.RemoveIpl(ipl);

            RAGE.Game.Interior.RefreshInterior(interior);
        }

        [Command("pos", true, "Получить текущую позицию", "position")]
        public static void Position(bool onGround = false)
        {
            var pos = Player.LocalPlayer.Position;

            if (onGround)
                pos.Z -= 1f;

            Events.CallLocal("Chat::ShowServerMessage", string.Format(Locale.Notifications.Commands.Position, pos.X, pos.Y, pos.Z, Player.LocalPlayer.GetHeading()));
        }

        [Command("posv", true, "Получить текущую позицию (veh)", "position")]
        public static void PositionVehicle()
        {
            var pos = Player.LocalPlayer.Vehicle?.Position;

            if (pos == null)
                return;

            Events.CallLocal("Chat::ShowServerMessage", string.Format(Locale.Notifications.Commands.Position, pos.X, pos.Y, pos.Z, Player.LocalPlayer.Vehicle.GetHeading()));
        }

        [Command("objpos", true, "Получить текущую позицию объекта типа", "objectpos")]
        public static void ObjectPosition(string model, float radius = 1f)
        {
            var pos = Player.LocalPlayer.Position;

            var handle = RAGE.Game.Object.GetClosestObjectOfType(pos.X, pos.Y, pos.Z, radius, RAGE.Util.Joaat.Hash(model), false, true, true);

            if (handle <= 0)
                return;

            Events.CallLocal("Chat::ShowServerMessage", $"Model: {model} | Pos: {RAGE.Game.Entity.GetEntityCoords(handle, true)} | Rot: {RAGE.Game.Entity.GetEntityRotation(handle, 5)}");
        }

        [Command("5sound", true, "Проиграть звук из GTA5", "gta5sound")]
        public static void Gta5Sound(string name, string setName)
        {
            RAGE.Game.Audio.PlaySoundFrontend(-1, name, setName, true);
        }

        [Command("newplacemarker", true, "Получить текущую позицию", "npmarker")]
        public static void NewPlaceMarker()
        {
            var pos = Player.LocalPlayer.Position;

            float z = pos.Z;

            RAGE.Game.Misc.GetGroundZFor3dCoord(pos.X, pos.Y, pos.Z, ref z, false);

            pos.Z = z;

            float minDist = 1000f;

            Marker marker = null;

            foreach (var x in RAGE.Elements.Entities.Markers.Streamed.ToList())
                if (Vector3.Distance(Player.LocalPlayer.Position, x.Position) < minDist)
                {
                    minDist = Vector3.Distance(Player.LocalPlayer.Position, x.Position);

                    marker = x;
                }

            if (marker == null)
                return;

            marker.Position = pos;
        }

        #endregion

        #endregion

        #region Execute
        /// <summary>Метод для выполнения команды</summary>
        /// <param name="cmdName">Название команды (может быть основным либо одним из псевдонимов)</param>
        /// <param name="args">Аргументы</param>
        public static void Execute(string cmdName, params string[] args)
        {
            var inst = All.Where(x => x.Attribute.Name == cmdName || x.Attribute.Aliases.Contains(cmdName)).FirstOrDefault();

            if (inst == null || inst.Attribute.AdminOnly && (Sync.Players.GetData(Player.LocalPlayer)?.AdminLevel ?? -1) < 0)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.Commands.Header, Locale.Notifications.Commands.NotFound);

                return;
            }

            bool correct = true;

            object[] newArgs = new object[inst.Parameters.Length];

            for (int i = 0; i < inst.Parameters.Length; i++)
            {
                if (i < args.Length)
                {
                    try
                    {
                        if (inst.Parameters[i].ParameterType == typeof(bool) || inst.Parameters[i].ParameterType == typeof(bool?))
                            args[i] = args[i].Replace(args[i][0], char.ToUpper(args[i][0]));

                        newArgs[i] = Convert.ChangeType(args[i], Nullable.GetUnderlyingType(inst.Parameters[i].ParameterType) ?? inst.Parameters[i].ParameterType, Settings.CultureInfo);
                    }
                    catch (Exception ex)
                    {
                        correct = false;

                        break;
                    }
                }
                else if (inst.Parameters[i].HasDefaultValue)
                    newArgs[i] = inst.Parameters[i].DefaultValue;
                else
                {
                    correct = false;

                    break;
                }
            }

            if (!correct)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.Commands.Header, string.Format(Locale.Notifications.Commands.WrongUsing, $"/{inst.Attribute.Name} {string.Join(", ", inst.Parameters.Select(x => x.HasDefaultValue ? x.Name.ToUpper() + "?" : x.Name.ToUpper()))}"));

                return;
            }
            else
                inst.MethodInfo.Invoke(null, newArgs.Length > 0 ? newArgs : null);
        }
        #endregion

        public Commands()
        {
            LastSent = DateTime.Now;

            All = new List<Instance>();

            foreach (MethodInfo method in typeof(Commands).GetMethods().Where(x => x.IsStatic))
            {
                if (method.GetCustomAttribute<CommandAttribute>() == null)
                    continue;

                All.Add(new Instance(method));
            }

            Events.Add("Item::Info", (object[] args) =>
            {
                CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Notifications.Commands.Item.Header, string.Format(Locale.Notifications.Commands.Item.Info, (string)args[0], Data.Items.GetName((string)args[0]), (string)args[1], (string)args[2], (string)args[3]), 10000);
            });
        }
    }
}
