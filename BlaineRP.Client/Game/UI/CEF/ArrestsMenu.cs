using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Fractions.Enums;
using BlaineRP.Client.Game.Fractions.Types;
using RAGE;
using RAGE.Elements;
using Core = BlaineRP.Client.Game.World.Core;

namespace BlaineRP.Client.UI.CEF
{
    [Script(int.MaxValue)]
    public class ArrestsMenu
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.MenuArrest);

        private static int EscBindIdx { get; set; } = -1;

        private static FractionTypes FractionType { get; set; }
        private static int MenuPosIdx { get; set; }

        private static DateTime LastSent;

        public static uint? CurrentArrestId { get => Player.LocalPlayer.GetData<uint?>("ArrestsMenu::CAD"); set { if (value == null) Player.LocalPlayer.ResetData("ArrestsMenu::CAD"); else Player.LocalPlayer.SetData("ArrestsMenu::CAD", value); } }

        public ArrestsMenu()
        {
            Events.Add("MenuArrest::Close", (args) =>
            {
                if (CEF.ActionBox.CurrentContextStr != null && (CEF.ActionBox.CurrentContextStr == "MenuArrestFreeInput" || CEF.ActionBox.CurrentContextStr == "MenuArrestChangeTimeInput"))
                    return;

                Close();
            });

            Events.Add("MenuArrest::MoreInfo", async (args) =>
            {
                var id = Utils.Convert.ToUInt32(args[0]);

                if (LastSent.IsSpam(1000, false, true))
                    return;

                LastSent = Core.ServerTime;

                var res = ((string)await Events.CallRemoteProc("Police::ARGI", (int)FractionType, MenuPosIdx, id))?.Split('_');

                if (res == null)
                    return;

                if (!IsActive)
                    return;

                var startDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(res[0])).DateTime;

                var secondsLeft = ulong.Parse(res[1]);
                var secondsPassed = ulong.Parse(res[2]);

                var name = res[3];
                var cid = res[4];
                var memberStr = res[5];

                var reasonS = res[6].Split('^');

                var totalTime = secondsPassed + secondsLeft;

                CEF.Browser.Window.ExecuteJs("MenuArrest.fillArrestFull", new List<object> { startDate.ToString("dd.MM.yyyy HH:mm"), name, $"#{cid}", memberStr, TimeSpan.FromSeconds(totalTime).GetBeautyString(), TimeSpan.FromSeconds(secondsPassed).GetBeautyString(), reasonS[0], reasonS[1] });

                CurrentArrestId = id;
            });

            Events.Add("MenuArrest::Button", async (args) =>
            {
                var action = Utils.Convert.ToByte(args[0]);

                if (CurrentArrestId == null)
                    return;

                var arrestId = (uint)CurrentArrestId;

                if (action == 0)
                {
                    CurrentArrestId = null;

                    CEF.Browser.Window.ExecuteJs("MenuArrest.switchContainer", 0);
                }
                else if (action == 1)
                {
                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    LastSent = Core.ServerTime;

                    var res = (bool)await Events.CallRemoteProc("Police::ARF", (int)FractionType, MenuPosIdx, arrestId, null);

                    if (res)
                    {
                        if (!IsActive)
                            return;

                        await CEF.ActionBox.ShowInputWithText
                        (
                            "MenuArrestFreeInput", Locale.Get("ARRESTMENU_FREE_HEADER", arrestId), Locale.Get("ARRESTMENU_FREE_CONTENT"), 100, "", null, null,

                            () =>
                            {
                                CEF.ActionBox.DefaultBindAction.Invoke();

                                CEF.Browser.SwitchTemp(Browser.IntTypes.MenuArrest, false);
                            },

                            async (rType, str) =>
                            {
                                if (rType == ActionBox.ReplyTypes.Cancel)
                                {
                                    CEF.ActionBox.Close();

                                    return;
                                }

                                str = str?.Trim();

                                if (!(new Regex(@"^[0-9a-zA-Zа-яА-Я\-\s,()!.?:+]{1,18}$")).IsMatch(str))
                                {
                                    CEF.Notification.ShowError(Locale.Get("ARRESTMENU_E_2"));

                                    return;
                                }

                                if (LastSent.IsSpam(500, false, true))
                                    return;

                                LastSent = Core.ServerTime;

                                var res = (bool)await Events.CallRemoteProc("Police::ARF", (int)FractionType, MenuPosIdx, arrestId, str);

                                if (res)
                                {
                                    CEF.ActionBox.Close();

                                    //Events.CallLocal("MenuArrest::Button", 0);
                                }
                            },

                            () =>
                            {
                                if (IsActive)
                                {
                                    CEF.Browser.SwitchTemp(Browser.IntTypes.MenuArrest, true);

                                    CEF.Cursor.Show(true, true);
                                }
                            }
                        );
                    }
                }
                else if (action == 2)
                {
                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    LastSent = Core.ServerTime;

                    var res = await Events.CallRemoteProc("Police::ARCT", (int)FractionType, MenuPosIdx, arrestId, 0, null) as bool?;

                    if (res == true)
                    {
                        if (!IsActive)
                            return;

                        await CEF.ActionBox.ShowInputWithText
                        (
                            "MenuArrestChangeTimeInput", Locale.Get("ARRESTMENU_CHTIME_HEADER"), Locale.Get("ARRESTMENU_CHTIME_CONTENT", Player.LocalPlayer.GetData<uint>("ArrestsMenu::CAD")), 100, "0", null, null,

                            () =>
                            {
                                CEF.ActionBox.DefaultBindAction.Invoke();

                                CEF.Browser.SwitchTemp(Browser.IntTypes.MenuArrest, false);
                            },

                            async (rType, str) =>
                            {
                                if (rType == ActionBox.ReplyTypes.Cancel)
                                {
                                    CEF.ActionBox.Close();

                                    return;
                                }

                                var strD = str?.Trim()?.Split(',');

                                long minsU;

                                if (strD.Length < 2 || !long.TryParse(strD[0], out minsU))
                                {
                                    CEF.Notification.ShowError(Locale.Get("ARRESTMENU_E_0"));

                                    return;
                                }

                                if (!((decimal)minsU).IsNumberValid<decimal>(-120, +120, out _, true))
                                    return;

                                if (minsU == 0)
                                {
                                    CEF.Notification.ShowError(Locale.Get("ARRESTMENU_E_1"));

                                    return;
                                }
                                var reasonStr = string.Join(',', strD.Skip(1)).Trim();

                                if (!(new Regex(@"^[0-9a-zA-Zа-яА-Я\-\s,()!.?:+]{1,18}$")).IsMatch(reasonStr))
                                {
                                    CEF.Notification.ShowError(Locale.Get("ARRESTMENU_E_2"));

                                    return;
                                }

                                if (LastSent.IsSpam(1000, false, true))
                                    return;

                                LastSent = Core.ServerTime;

                                var res = await Events.CallRemoteProc("Police::ARCT", (int)FractionType, MenuPosIdx, arrestId, minsU, reasonStr);

                                if (res != null)
                                {
                                    CEF.ActionBox.Close();

                                    if (CurrentArrestId != arrestId)
                                        return;

                                    CEF.Browser.Window.ExecuteJs("MenuArrest.updateInfoLine", 4, TimeSpan.FromSeconds(Utils.Convert.ToUInt64(res)).GetBeautyString());

                                    //Events.CallLocal("MenuArrest::Button", 0);
                                }
                            },

                            () =>
                            {
                                if (IsActive)
                                {
                                    CEF.Browser.SwitchTemp(Browser.IntTypes.MenuArrest, true);

                                    CEF.Cursor.Show(true, true);
                                }
                            }
                        );
                    }
                }
            });
        }

        public static async System.Threading.Tasks.Task Show(FractionTypes fType, int menuPosIdx, List<Police.ArrestInfo> arrests)
        {
            if (IsActive)
                return;

            await CEF.Browser.Render(Browser.IntTypes.MenuArrest, true, true);

            FractionType = fType;
            MenuPosIdx = menuPosIdx;

            CEF.Browser.Window.ExecuteJs("MenuArrest.fillArrests", fType != FractionTypes.PRISON_BB, arrests.Select(x => new object[] { x.Id, x.Time.ToString("dd.MM.yyyy HH:mm"), x.TargetName, x.MemberName }).ToList());

            CEF.Cursor.Show(true, true);

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CEF.Browser.Render(Browser.IntTypes.MenuArrest, false, false);

            CurrentArrestId = null;

            Input.Core.Unbind(EscBindIdx);

            EscBindIdx = -1;

            CEF.Cursor.Show(false, false);
        }
    }
}
