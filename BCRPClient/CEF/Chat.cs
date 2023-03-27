﻿using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.CEF
{
    class Chat : Events.Script
    {
        public static bool IsActive { get => Browser.IsActive(Browser.IntTypes.Chat); }
        public static bool InputVisible = false;

        public static DateTime LastSent;

        private static string TimeStr { get => Settings.Chat.ShowTime ? (Settings.Interface.UseServerTime ? "[" + Sync.World.ServerTime.ToString("HH:mm:ss") + "] " : "[" + Sync.World.LocalTime.ToString("HH:mm:ss") + "] ") : ""; }

        #region All Types
        public enum Type
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

            /// <summary>/me над игроком</summary>
            MePlayer,
            /// <summary>/try над игроком</summary>
            TryPlayer,
            Ban,
            BanHard,
            Kick,
            Mute,
            Jail,
            UnBan,
            UnMute,
            UnJail,
            News,
            Advert,
        }
        #endregion

        private static Queue<(string, object[])> Queue = new Queue<(string, object[])>();

        private static void AddToQueue(string command, params object[] args) => Queue.Enqueue((command, args));

        private static List<int> TempBinds { get; set; } = new List<int>();

        public Chat()
        {
            #region Events
            #region Send
            Events.Add("Chat::Send", (object[] args) =>
            {
                if (!IsActive)
                    return;

                ShowInput(false);

/*                Browser.Window.ExecuteCachedJs("Chat.needScroll();");

                Browser.Window.ExecuteCachedJs("Chat.tryScroll();");*/

                var type = (Type)((int)args[0]);
                var msg = (string)args[1];

                if (msg.Length < 1 || char.IsWhiteSpace(msg[0]))
                    return;

                if (type == Type.Say && msg[0] == '/' && msg.Length > 1)
                {
                    var endOfCmd = msg.IndexOf(' ');

                    if (endOfCmd == -1 || endOfCmd + 1 == msg.Length)
                        Data.Commands.Execute(msg.Substring(1));
                    else
                    {
                        var cmd = msg.Substring(1, endOfCmd - 1);
                        var cmdInfo = msg.Substring(endOfCmd + 1);
                        var parameters = new List<string>();

                        var currentParam = new StringBuilder();
                        var textDetected = false;

                        for (int i = 0; i < cmdInfo.Length; i++)
                        {
                            if (!textDetected)
                            {
                                if (cmdInfo[i] != '"' && cmdInfo[i] != ' ' && cmdInfo[i] != ',')
                                {
                                    currentParam.Append(cmdInfo[i]);
                                }
                                else if (cmdInfo[i] == '"')
                                {
                                    textDetected = true;
                                }
                                else if (currentParam.Length > 0)
                                {
                                    parameters.Add(currentParam.ToString().ToLower());

                                    currentParam.Clear();
                                }
                            }
                            else
                            {
                                if (cmdInfo[i] != '"')
                                {
                                    currentParam.Append(cmdInfo[i]);
                                }
                                else
                                {
                                    textDetected = false;

                                    if (currentParam.Length > 0)
                                    {
                                        parameters.Add(currentParam.ToString());

                                        currentParam.Clear();
                                    }
                                }
                            }

                            if (i == cmdInfo.Length - 1 && currentParam.Length > 0)
                                parameters.Add(currentParam.ToString().ToLower());
                        }

                        //Utils.ConsoleOutput($"Cmd: {cmd}, Params: {string.Join(" | ", parameters)}");

                        if (parameters.Count > 0)
                            Data.Commands.Execute(cmd, parameters.ToArray());
                        else
                            Data.Commands.Execute(cmd);
                    }

                    return;
                }

                if (LastSent.IsSpam(500, false, true))
                    return;

                Events.CallRemote("Chat::Send", (int)type, msg);
            });
            #endregion

            #region Show Casual Message
            Events.Add("Chat::SCM", (object[] args) =>
            {
                if (!CEF.Browser.IsRendered(CEF.Browser.IntTypes.Chat))
                    return;

                var timeStr = TimeStr;

                var player = Entities.Players.GetAtRemote((ushort)(int)args[0]);
                var type = (Type)((int)args[1]);
                var message = (string)args[2];
                var player2 = args.Length > 3 ? Entities.Players.GetAtRemote((ushort)(int)args[3]) : null;

                var data = Sync.Players.GetData(player);

                if (data == null)
                    return;

                var name = player != Player.LocalPlayer ? Utils.GetPlayerName(player, true, false, false) : Player.LocalPlayer.Name;

                var name2 = player2 != null ? Utils.GetPlayerName(player2, true, false, true) : "null";

                if (Settings.Chat.UseFilter)
                    message = Additional.StringFilter.Process(message, true, '♡');

                if (type == Type.Say)
                    AddToQueue("Messages.showNormal", type, timeStr, name, player.RemoteId, message);
                else if (type == Type.Shout)
                    AddToQueue("Messages.showNormal", type, timeStr, name, player.RemoteId, message);
                else if (type == Type.Whisper)
                    AddToQueue("Messages.showNormal", type, timeStr, name, player.RemoteId, message);
                else if (type == Type.NonRP)
                    AddToQueue("Messages.showOOC", timeStr, name, player.RemoteId, message);
                else if (type == Type.Me)
                    AddToQueue("Messages.showMe", timeStr, name, player.RemoteId, message);
                else if (type == Type.MePlayer)
                    AddToQueue("Messages.showMe", timeStr, name, player.RemoteId, message + " " + name2);
                else if (type == Type.TryPlayer)
                    AddToQueue("Messages.showTry", timeStr, name, player.RemoteId, message.Substring(0, message.IndexOf('*')) + " " + name2, message.Substring(message.IndexOf('*') + 1) == "1");
                else if (type == Type.Do)
                    AddToQueue("Messages.showDo", timeStr, name, player.RemoteId, message);
                else if (type == Type.Todo)
                    AddToQueue("Messages.showToDo", timeStr, name, player.RemoteId, message.Substring(0, message.IndexOf('*')), message.Substring(message.IndexOf('*') + 1));
                else if (type == Type.Try)
                    AddToQueue("Messages.showTry", timeStr, name, player.RemoteId, message.Substring(0, message.IndexOf('*')), message.Substring(message.IndexOf('*') + 1) == "1");

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
            });
            #endregion

            Events.Add("Chat::SFM", (args) =>
            {
                if (!CEF.Browser.IsRendered(CEF.Browser.IntTypes.Chat))
                    return;

                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (Data.Fractions.Fraction.AllMembers == null)
                    return;

                var fData = pData.CurrentFraction;

                if (fData == null)
                    return;

                var timeStr = TimeStr;

                var cid = args[0].ToUInt32();
                var rid = (ushort)(int)args[1];
                var message = (string)args[2];

                var mData = Data.Fractions.Fraction.AllMembers.GetValueOrDefault(cid);

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
            });

            #region Show Global Message
            Events.Add("Chat::ShowGlobalMessage", (object[] args) =>
            {
                if (!CEF.Browser.IsRendered(CEF.Browser.IntTypes.Chat))
                    return;

                var timeStr = TimeStr;

                var playerStr = (string)args[0];
                var type = (Type)((int)args[1]);
                var message = (string)args[2];

                if (Settings.Chat.UseFilter)
                    message = Additional.StringFilter.Process(message, true, '♡');

                if (type == Type.Admin)
                    AddToQueue("Messages.admin_message", timeStr, playerStr, message);
                else if (type == Type.Goverment)
                    AddToQueue("Messages.government", timeStr, playerStr, message);
                else if (type == Type.News)
                    AddToQueue("Messages.news", timeStr, playerStr, message);
                else if (type == Type.Advert)
                {
                    var targetStr = (string)args[3];

                    AddToQueue("Messages.advert", timeStr, playerStr, targetStr, message, ""); // phone number
                }
                else if (type == Type.Kick || type == Type.Ban || type == Type.BanHard || type == Type.Mute || type == Type.Jail)
                {
                    var targetStr = (string)args[3];

                    if (type == Type.BanHard)
                    {
                        AddToQueue("Messages.admin_ban_hard", timeStr, playerStr, targetStr, message);
                    }
                    else if (type == Type.Kick)
                    {
                        AddToQueue("Messages.admin_kick", timeStr, playerStr, targetStr, message);
                    }
                    else
                    {
                        var time = (string)args[4];

                        if (type == Type.Ban)
                            AddToQueue("Messages.admin_ban", timeStr, playerStr, targetStr, time, message);
                        else if (type == Type.Mute)
                            AddToQueue("Messages.admin_mute", timeStr, playerStr, targetStr, time, message);
                        else if (type == Type.Jail)
                            AddToQueue("Messages.admin_jail", timeStr, playerStr, targetStr, time, message);
                    }
                }
                else if (type == Type.UnBan || type == Type.UnMute || type == Type.UnJail)
                {
                    var targetStr = (string)args[3];

                    if (type == Type.UnBan)
                        AddToQueue("Messages.admin_unban", timeStr, playerStr, targetStr, message);
                    else if (type == Type.UnMute)
                        AddToQueue("Messages.admin_unmute", timeStr, playerStr, targetStr, message);
                    else if (type == Type.UnJail)
                        AddToQueue("Messages.admin_unjail", timeStr, playerStr, targetStr, message);
                }

                if (IsActive)
                {
                    (string, object[]) t;

                    if (Queue.TryPeek(out t))
                    {
                        Browser.Window.ExecuteCachedJs("Chat.needScroll();");

                        Browser.Window.ExecuteJs(t.Item1, t.Item2);

                        Browser.Window.ExecuteCachedJs("Chat.tryScroll();");
                    }
                }
            });
            #endregion

            #region Show Server Message
            Events.Add("Chat::ShowServerMessage", (object[] args) =>
            {
                if (!CEF.Browser.IsRendered(CEF.Browser.IntTypes.Chat))
                    return;

                string message = (string)args[0];

                if (Settings.Chat.UseFilter)
                    message = Additional.StringFilter.Process(message, true, '♡');

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
            });
            #endregion
            #endregion
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
                    CEF.Browser.Window.ExecuteJs(cmd.Function, cmd.Args);

                Browser.Window.ExecuteCachedJs("Chat.tryScroll();");
            }
        }

        #region Stuff
        public static void ShowInput(bool value)
        {
            if (!IsActive)
                return;

            if (value && Utils.IsAnyCefActive(true) && !CEF.Death.IsActive && !CEF.Phone.IsActive)
                return;

            Browser.Window.ExecuteJs("Chat.switchInput", value);

            if (value)
            {
                TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => ShowInput(false)));

                KeyBinds.Get(KeyBinds.Types.ChatInput).Disable();
            }
            else
            {
                KeyBinds.Get(KeyBinds.Types.ChatInput).Enable();

                foreach (var x in TempBinds.ToList())
                    KeyBinds.Unbind(x);

                TempBinds.Clear();

                if (!Utils.IsAnyCefActive(false))
                    CEF.Cursor.SwitchEscMenuAccess(true);
            }

            Cursor.Show(value, value);

            InputVisible = value;
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
