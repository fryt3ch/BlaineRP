using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Components;
using BlaineRP.Client.Input;
using BlaineRP.Client.Input.Enums;
using BlaineRP.Client.Utils;
using Core = BlaineRP.Client.Game.Management.Commands.Core;
using Players = BlaineRP.Client.Sync.Players;

namespace BlaineRP.Client.CEF
{
    [Script(int.MaxValue)]
    public class Chat
    {
        public static bool IsActive { get => Browser.IsActive(Browser.IntTypes.Chat); }
        public static bool InputVisible = false;

        public static DateTime LastSent;

        private static Regex TodoMessageRegex = new Regex(@".+\*.+");

        private static string TimeStr { get => Settings.User.Chat.ShowTime ? (Settings.User.Interface.UseServerTime ? "[" + Sync.World.ServerTime.ToString("HH:mm:ss") + "] " : "[" + Sync.World.LocalTime.ToString("HH:mm:ss") + "] ") : ""; }

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

        private static Queue<(string, object[])> Queue = new Queue<(string, object[])>();

        private static void AddToQueue(string command, params object[] args) => Queue.Enqueue((command, args));

        private static int EscBindIdx { get; set; } = -1;

        public Chat()
        {
            #region Events
            #region Send
            Events.Add("Chat::Send", async (args) =>
            {
                if (!IsActive)
                    return;

                ShowInput(false);

                /*                Browser.Window.ExecuteCachedJs("Chat.needScroll();");

                                Browser.Window.ExecuteCachedJs("Chat.tryScroll();");*/

                var type = (MessageTypes)((int)args[0]);
                var msg = (string)args[1];

                msg = msg.Trim();

                if (msg.Length < 1)
                    return;

                if (type == MessageTypes.Say && msg[0] == '/' && msg.Length > 1)
                {
                    var endOfCmd = msg.IndexOf(' ');

                    if (endOfCmd == -1 || endOfCmd + 1 == msg.Length)
                    {
                        Core.Execute(msg.Substring(1));
                    }
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
                            Core.Execute(cmd, parameters.ToArray());
                        else
                            Core.Execute(cmd);
                    }

                    return;
                }

                if (type == MessageTypes.Todo)
                {
                    if (!TodoMessageRegex.IsMatch(msg))
                    {
                        CEF.Notification.ShowError(Locale.Get("CHAT_MSG_TODO_E_0"));

                        return;
                    }
                }

                if (LastSent.IsSpam(500, false, true))
                    return;

                var mute = type == MessageTypes.Fraction ? Punishment.All.Where(x => x.Type == Punishment.Types.Mute || x.Type == Punishment.Types.FractionMute).FirstOrDefault() : Punishment.All.Where(x => x.Type == Punishment.Types.Mute).FirstOrDefault(); ;

                if (mute != null)
                {
                    mute.ShowErrorNotification();

                    return;
                }

                var res = (int)await Events.CallRemoteProc("Chat::Send", (int)type, msg);

                if (res == 255)
                    return;
            });
            #endregion

            #region Show Casual Message
            Events.Add("Chat::SCM", (object[] args) =>
            {
                if (!CEF.Browser.IsRendered(CEF.Browser.IntTypes.Chat))
                    return;

                var timeStr = TimeStr;

                var player = Entities.Players.GetAtRemote((ushort)(int)args[0]);
                var type = (MessageTypes)((int)args[1]);
                var message = (string)args[2];
                var player2 = args.Length > 3 ? Entities.Players.GetAtRemote((ushort)(int)args[3]) : null;

                var data = PlayerData.GetData(player);

                if (data == null)
                    return;

                var name = player != Player.LocalPlayer ? Utils.Game.Players.GetPlayerName(player, true, false, false) : Player.LocalPlayer.Name;

                var name2 = player2 != null ? Utils.Game.Players.GetPlayerName(player2, true, false, true) : null;

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

                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (Data.Fractions.Fraction.AllMembers == null)
                    return;

                var fData = pData.CurrentFraction;

                if (fData == null)
                    return;

                var timeStr = TimeStr;

                var cid = Utils.Convert.ToUInt32(args[0]);
                var rid = Utils.Convert.ToUInt16(args[1]);
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

            Events.Add("Chat::SDM", (args) =>
            {
                if (!CEF.Browser.IsRendered(CEF.Browser.IntTypes.Chat))
                    return;

                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (Data.Fractions.Fraction.AllMembers == null)
                    return;

                var fData = pData.CurrentFraction;

                if (fData == null)
                    return;

                var timeStr = TimeStr;

                var cid = Utils.Convert.ToUInt32(args[0]);
                var rid = Utils.Convert.ToUInt16(args[1]);
                var message = (string)args[2];
                var fType = (Data.Fractions.Types)Utils.Convert.ToInt32(args[3]);
                var fRank = Utils.Convert.ToByte(args[4]);

                var tFData = Data.Fractions.Fraction.Get(fType);

                if (tFData == null)
                    return;

                var player = Entities.Players.GetAtRemote(rid);

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
            });

            #region Show Global Message
            Events.Add("Chat::ShowGlobalMessage", (object[] args) =>
            {
                if (!CEF.Browser.IsRendered(CEF.Browser.IntTypes.Chat))
                    return;

                var timeStr = TimeStr;

                var playerStr = (string)args[0];
                var type = (MessageTypes)((int)args[1]);
                var message = (string)args[2];

                if (Settings.User.Chat.UseFilter)
                    message = StringFilter.Process(message, true, '♡');

                if (type == MessageTypes.Admin)
                    AddToQueue("Messages.admin_message", timeStr, playerStr, message);
                else if (type == MessageTypes.Goverment)
                    AddToQueue("Messages.government", timeStr, playerStr, message);
                else if (type == MessageTypes.News)
                    AddToQueue("Messages.news", timeStr, playerStr, message);
                else if (type == MessageTypes.Advert)
                {
                    var targetStr = (string)args[3];

                    AddToQueue("Messages.advert", timeStr, playerStr, targetStr, message, ""); // phone number
                }
                else if (type == MessageTypes.Kick || type == MessageTypes.Ban || type == MessageTypes.BanHard || type == MessageTypes.Mute || type == MessageTypes.Jail || type == MessageTypes.Warn)
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
            });
            #endregion

            #region Show Server Message
            Events.Add("Chat::ShowServerMessage", (object[] args) =>
            {
                if (!CEF.Browser.IsRendered(CEF.Browser.IntTypes.Chat))
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
            if (!IsActive || value == InputVisible)
                return;

            if (value && Utils.Misc.IsAnyCefActive(true) && !CEF.Death.IsActive && !Phone.Phone.IsActive)
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
