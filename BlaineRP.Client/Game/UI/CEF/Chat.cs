﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Game.Fractions;
using BlaineRP.Client.Game.Input.Enums;
using BlaineRP.Client.Game.Management.Commands;
using BlaineRP.Client.Game.Management.Punishments;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class Chat
    {
        public enum MessageTypes
        {
            /// <summary>/say</summary>
            Say,

            /// <summary>/s</summary>
            Shout,

            /// <summary>/w</summary>
            Whisper,

            /// <summary>/n - OOC чат</summary>
            NonRP,

            /// <summary>/me</summary>
            Me,

            /// <summary>/do</summary>
            Do,

            /// <summary>/todo</summary>
            Todo,

            /// <summary>/try</summary>
            Try,

            /// <summary>/f /r</summary>
            Fraction,

            /// <summary>/o</summary>
            Organisation,

            /// <summary>/d</summary>
            Department,

            /// <summary>/gov</summary>
            Goverment,

            /// <summary>/amsg</summary>
            Admin,

            Ban,
            BanHard,
            Kick,
            Mute,
            Jail,
            Warn,
            UnBan,
            UnMute,
            UnJail,
            UnWarn,
            News,
            Advert,
        }

        public static bool InputVisible = false;

        public static DateTime LastSent;

        private static Regex TodoMessageRegex = new Regex(@".+\*.+");

        private static Queue<(string, object[])> Queue = new Queue<(string, object[])>();

        public Chat()
        {
            #region Events

            #region Send

            RAGE.Events.Add("Chat::Send",
                async (args) =>
                {
                    if (!IsActive)
                        return;

                    ShowInput(false);

                    /*                Browser.Window.ExecuteCachedJs("Chat.needScroll();");
    
                                    Browser.Window.ExecuteCachedJs("Chat.tryScroll();");*/

                    var type = (MessageTypes)(int)args[0];
                    var msg = (string)args[1];

                    msg = msg.Trim();

                    if (msg.Length < 1)
                        return;

                    if (type == MessageTypes.Say && msg[0] == '/' && msg.Length > 1)
                    {
                        int endOfCmd = msg.IndexOf(' ');

                        if (endOfCmd == -1 || endOfCmd + 1 == msg.Length)
                        {
                            Service.Execute(msg.Substring(1));
                        }
                        else
                        {
                            string cmd = msg.Substring(1, endOfCmd - 1);
                            string cmdInfo = msg.Substring(endOfCmd + 1);
                            var parameters = new List<string>();

                            var currentParam = new StringBuilder();
                            var textDetected = false;

                            for (var i = 0; i < cmdInfo.Length; i++)
                            {
                                if (!textDetected)
                                {
                                    if (cmdInfo[i] == '"')
                                    {
                                        textDetected = true;
                                    }
                                    else if (cmdInfo[i] == ' ' || cmdInfo[i] == ',')
                                    {
                                        if (currentParam.Length > 0)
                                        {
                                            parameters.Add(currentParam.ToString().ToLower());

                                            currentParam.Clear();
                                        }
                                    }
                                    else
                                    {
                                        currentParam.Append(cmdInfo[i]);
                                    }
                                }
                                else
                                {
                                    if (cmdInfo[i] == '"')
                                    {
                                        textDetected = false;

                                        if (currentParam.Length > 0)
                                        {
                                            parameters.Add(currentParam.ToString());

                                            currentParam.Clear();
                                        }
                                    }
                                    else
                                    {
                                        currentParam.Append(cmdInfo[i]);
                                    }
                                }

                                if (i == cmdInfo.Length - 1 && currentParam.Length > 0)
                                    parameters.Add(currentParam.ToString().ToLower());
                            }

                            //Utils.ConsoleOutput($"Cmd: {cmd}, Params: {string.Join(" | ", parameters)}");

                            if (parameters.Count > 0)
                                Service.Execute(cmd, parameters.ToArray());
                            else
                                Service.Execute(cmd);
                        }

                        return;
                    }

                    if (type == MessageTypes.Todo)
                        if (!TodoMessageRegex.IsMatch(msg))
                        {
                            Notification.ShowError(Locale.Get("CHAT_MSG_TODO_E_0"));

                            return;
                        }

                    if (LastSent.IsSpam(500, false, true))
                        return;

                    Punishment mute = type == MessageTypes.Fraction
                        ? Punishment.All.Where(x => x.Type == PunishmentType.Mute || x.Type == PunishmentType.FractionMute).FirstOrDefault()
                        : Punishment.All.Where(x => x.Type == PunishmentType.Mute).FirstOrDefault();
                    ;

                    if (mute != null)
                    {
                        mute.ShowErrorNotification();

                        return;
                    }

                    var res = (int)await RAGE.Events.CallRemoteProc("Chat::Send", (int)type, msg);

                    if (res == 255)
                        return;
                }
            );

            #endregion

            #region Show Casual Message

            RAGE.Events.Add("Chat::SCM",
                (object[] args) =>
                {
                    if (!Browser.IsRendered(Browser.IntTypes.Chat))
                        return;

                    string timeStr = TimeStr;

                    Player player = Entities.Players.GetAtRemote((ushort)(int)args[0]);
                    var type = (MessageTypes)(int)args[1];
                    var message = (string)args[2];
                    Player player2 = args.Length > 3 ? Entities.Players.GetAtRemote((ushort)(int)args[3]) : null;

                    var data = PlayerData.GetData(player);

                    if (data == null)
                        return;

                    string name = player != Player.LocalPlayer ? Utils.Game.Players.GetPlayerName(player, true, false, false) : Player.LocalPlayer.Name;

                    string name2 = player2 != null ? Utils.Game.Players.GetPlayerName(player2, true, false, true) : null;

                    if (name2 != null)
                        message = string.Format(message, name2);

                    if (Settings.User.Chat.UseFilter)
                        message = StringFilter.Process(message, true, '♡');

                    if (type == MessageTypes.Say)
                        AddToQueue("Messages.showNormal", type, timeStr, name, player.RemoteId, message);
                    else if (type == MessageTypes.Shout)
                        AddToQueue("Messages.showNormal", type, timeStr, name, player.RemoteId, message);
                    else if (type == MessageTypes.Whisper)
                        AddToQueue("Messages.showNormal", type, timeStr, name, player.RemoteId, message);
                    else if (type == MessageTypes.NonRP)
                        AddToQueue("Messages.showOOC", timeStr, name, player.RemoteId, message);
                    else if (type == MessageTypes.Me)
                        AddToQueue("Messages.showMe", timeStr, name, player.RemoteId, message);
                    else if (type == MessageTypes.Do)
                        AddToQueue("Messages.showDo", timeStr, name, player.RemoteId, message);
                    else if (type == MessageTypes.Todo)
                        AddToQueue("Messages.showToDo", timeStr, name, player.RemoteId, message.Substring(0, message.IndexOf('*')), message.Substring(message.IndexOf('*') + 1));
                    else if (type == MessageTypes.Try)
                        AddToQueue("Messages.showTry",
                            timeStr,
                            name,
                            player.RemoteId,
                            message.Substring(0, message.IndexOf('*')),
                            message.Substring(message.IndexOf('*') + 1) == "1"
                        );

                    if (IsActive)
                    {
                        (string, object[]) t;

                        if (Queue.TryDequeue(out t))
                        {
                            Browser.Window.ExecuteCachedJs("Chat.needScroll();");

                            Browser.Window.ExecuteJs(t.Item1, t.Item2);

                            Browser.Window.ExecuteCachedJs("Chat.tryScroll();");
                        }
                    }
                }
            );

            #endregion

            RAGE.Events.Add("Chat::SFM",
                (args) =>
                {
                    if (!Browser.IsRendered(Browser.IntTypes.Chat))
                        return;

                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (Fraction.AllMembers == null)
                        return;

                    Fraction fData = pData.CurrentFraction;

                    if (fData == null)
                        return;

                    string timeStr = TimeStr;

                    var cid = Utils.Convert.ToUInt32(args[0]);
                    var rid = Utils.Convert.ToUInt16(args[1]);
                    var message = (string)args[2];

                    MemberData mData = Fraction.AllMembers.GetValueOrDefault(cid);

                    if (mData == null)
                        return;

                    AddToQueue("Messages.showFraction", timeStr, $"{fData.GetRankName(mData.Rank)} [{mData.Rank + 1}]", mData.Name, rid, message);

                    if (IsActive)
                    {
                        (string, object[]) t;

                        if (Queue.TryDequeue(out t))
                        {
                            Browser.Window.ExecuteCachedJs("Chat.needScroll();");

                            Browser.Window.ExecuteJs(t.Item1, t.Item2);

                            Browser.Window.ExecuteCachedJs("Chat.tryScroll();");
                        }
                    }
                }
            );

            RAGE.Events.Add("Chat::SDM",
                (args) =>
                {
                    if (!Browser.IsRendered(Browser.IntTypes.Chat))
                        return;

                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (Fraction.AllMembers == null)
                        return;

                    Fraction fData = pData.CurrentFraction;

                    if (fData == null)
                        return;

                    string timeStr = TimeStr;

                    var cid = Utils.Convert.ToUInt32(args[0]);
                    var rid = Utils.Convert.ToUInt16(args[1]);
                    var message = (string)args[2];
                    var fType = (FractionTypes)Utils.Convert.ToInt32(args[3]);
                    var fRank = Utils.Convert.ToByte(args[4]);

                    var tFData = Fraction.Get(fType);

                    if (tFData == null)
                        return;

                    Player player = Entities.Players.GetAtRemote(rid);

                    AddToQueue("Messages.showDepartment", timeStr, tFData.Name, $"{tFData.GetRankName(fRank)} [{fRank + 1}]", player?.Name ?? "null", rid, message);

                    if (IsActive)
                    {
                        (string, object[]) t;

                        if (Queue.TryDequeue(out t))
                        {
                            Browser.Window.ExecuteCachedJs("Chat.needScroll();");

                            Browser.Window.ExecuteJs(t.Item1, t.Item2);

                            Browser.Window.ExecuteCachedJs("Chat.tryScroll();");
                        }
                    }
                }
            );

            #region Show Global Message

            RAGE.Events.Add("Chat::ShowGlobalMessage",
                (object[] args) =>
                {
                    if (!Browser.IsRendered(Browser.IntTypes.Chat))
                        return;

                    string timeStr = TimeStr;

                    var playerStr = (string)args[0];
                    var type = (MessageTypes)(int)args[1];
                    var message = (string)args[2];

                    if (Settings.User.Chat.UseFilter)
                        message = StringFilter.Process(message, true, '♡');

                    if (type == MessageTypes.Admin)
                    {
                        AddToQueue("Messages.admin_message", timeStr, playerStr, message);
                    }
                    else if (type == MessageTypes.Goverment)
                    {
                        AddToQueue("Messages.government", timeStr, playerStr, message);
                    }
                    else if (type == MessageTypes.News)
                    {
                        AddToQueue("Messages.news", timeStr, playerStr, message);
                    }
                    else if (type == MessageTypes.Advert)
                    {
                        var targetStr = (string)args[3];

                        AddToQueue("Messages.advert", timeStr, playerStr, targetStr, message, ""); // phone number
                    }
                    else if (type == MessageTypes.Kick ||
                             type == MessageTypes.Ban ||
                             type == MessageTypes.BanHard ||
                             type == MessageTypes.Mute ||
                             type == MessageTypes.Jail ||
                             type == MessageTypes.Warn)
                    {
                        var targetStr = (string)args[3];

                        if (type == MessageTypes.BanHard)
                        {
                            AddToQueue("Messages.admin_ban_hard", timeStr, playerStr, targetStr, message);
                        }
                        else if (type == MessageTypes.Kick)
                        {
                            AddToQueue("Messages.admin_kick", timeStr, playerStr, targetStr, message);
                        }
                        else if (type == MessageTypes.Warn)
                        {
                            AddToQueue("Messages.admin_warn", timeStr, playerStr, targetStr, message);
                        }
                        else
                        {
                            var time = (string)args[4];

                            if (type == MessageTypes.Ban)
                                AddToQueue("Messages.admin_ban", timeStr, playerStr, targetStr, time, message);
                            else if (type == MessageTypes.Mute)
                                AddToQueue("Messages.admin_mute", timeStr, playerStr, targetStr, time, message);
                            else if (type == MessageTypes.Jail)
                                AddToQueue("Messages.admin_jail", timeStr, playerStr, targetStr, time, message);
                        }
                    }
                    else if (type == MessageTypes.UnBan || type == MessageTypes.UnMute || type == MessageTypes.UnJail || type == MessageTypes.UnWarn)
                    {
                        var targetStr = (string)args[3];

                        if (type == MessageTypes.UnBan)
                            AddToQueue("Messages.admin_unban", timeStr, playerStr, targetStr, message);
                        else if (type == MessageTypes.UnMute)
                            AddToQueue("Messages.admin_unmute", timeStr, playerStr, targetStr, message);
                        else if (type == MessageTypes.UnJail)
                            AddToQueue("Messages.admin_unjail", timeStr, playerStr, targetStr, message);
                        else if (type == MessageTypes.UnWarn)
                            AddToQueue("Messages.admin_unwarn", timeStr, playerStr, targetStr, message);
                    }

                    if (IsActive)
                    {
                        (string, object[]) t;

                        if (Queue.TryDequeue(out t))
                        {
                            Browser.Window.ExecuteCachedJs("Chat.needScroll();");

                            Browser.Window.ExecuteJs(t.Item1, t.Item2);

                            Browser.Window.ExecuteCachedJs("Chat.tryScroll();");
                        }
                    }
                }
            );

            #endregion

            #region Show Server Message

            RAGE.Events.Add("Chat::ShowServerMessage",
                (object[] args) =>
                {
                    if (!Browser.IsRendered(Browser.IntTypes.Chat))
                        return;

                    var message = (string)args[0];

                    if (Settings.User.Chat.UseFilter)
                        message = StringFilter.Process(message, true, '♡');

                    if (IsActive)
                    {
                        Browser.Window.ExecuteCachedJs("Chat.needScroll();");

                        Browser.Window.ExecuteJs("Messages.server", TimeStr, message);

                        Browser.Window.ExecuteCachedJs("Chat.tryScroll();");
                    }
                    else
                    {
                        AddToQueue("Messages.server", TimeStr, message);
                    }
                }
            );

            #endregion

            #endregion
        }

        public static bool IsActive => Browser.IsActive(Browser.IntTypes.Chat);

        private static string TimeStr => Settings.User.Chat.ShowTime
            ? Settings.User.Interface.UseServerTime ? "[" + World.Core.ServerTime.ToString("HH:mm:ss") + "] " : "[" + World.Core.LocalTime.ToString("HH:mm:ss") + "] "
            : "";

        private static int EscBindIdx { get; set; } = -1;

        private static void AddToQueue(string command, params object[] args)
        {
            Queue.Enqueue((command, args));
        }

        public static void Show(bool value)
        {
            Browser.Switch(Browser.IntTypes.Chat, value);

            ShowInput(false);

            if (Queue.Count > 0)
            {
                Browser.Window.ExecuteCachedJs($"Chat.needScroll();");

                (string Function, object[] Args) cmd;

                while (Queue.TryDequeue(out cmd))
                {
                    Browser.Window.ExecuteJs(cmd.Function, cmd.Args);
                }

                Browser.Window.ExecuteCachedJs("Chat.tryScroll();");
            }
        }

        #region Stuff

        public static void ShowInput(bool value)
        {
            if (!IsActive || value == InputVisible)
                return;

            if (value && Utils.Misc.IsAnyCefActive(true) && !Death.IsActive && !Phone.Phone.IsActive)
                return;

            Browser.Window.ExecuteJs("Chat.switchInput", value);

            InputVisible = value;

            if (value)
            {
                EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => ShowInput(false));

                Input.Core.Get(BindTypes.ChatInput).Disable();
            }
            else
            {
                Input.Core.Get(BindTypes.ChatInput).Enable();

                Input.Core.Unbind(EscBindIdx);

                EscBindIdx = -1;
            }

            Cursor.Show(value, value);
        }

        public static void SetHeight(int height)
        {
            Browser.Window.ExecuteJs("Chat.switchHeight", height);
        }

        public static void SetFontSize(int value)
        {
            Browser.Window.ExecuteJs("Chat.switchFont", value);
        }

        #endregion
    }
}