using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.CEF
{
    public class ArrestsMenu : Events.Script
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.MenuArrest);

        private static int EscBindIdx { get; set; } = -1;

        private static Data.Fractions.Types FractionType { get; set; }
        private static int MenuPosIdx { get; set; }

        private static DateTime LastSent;

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
                var id = Utils.ToUInt32(args[0]);

                if (LastSent.IsSpam(1000, false, true))
                    return;

                LastSent = Sync.World.ServerTime;

                var res = ((string)await Events.CallRemoteProc("Police::ARGI", (int)FractionType, MenuPosIdx, id))?.Split('_');

                if (res == null)
                    return;

                if (!IsActive)
                    return;

                var date = DateTimeOffset.FromUnixTimeSeconds(long.Parse(res[0])).DateTime;
                var endDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(res[1])).DateTime;
                var name = res[2];
                var cid = res[3];
                var memberStr = res[4];

                var reasonS = res[5].Split('^');

                CEF.Browser.Window.ExecuteJs("MenuArrest.fillArrestFull", new List<object> { date.ToString("dd.MM.yyyy HH:mm"), name, $"#{cid}", memberStr, endDate.Subtract(date).GetBeautyString(), Sync.World.ServerTime.Subtract(date).GetBeautyString(), reasonS[0], reasonS[1] });

                Player.LocalPlayer.SetData("ArrestsMenu::CAD", id);
            });

            Events.Add("MenuArrest::Button", async (args) =>
            {
                var action = Utils.ToByte(args[0]);

                if (action == 0)
                {
                    Player.LocalPlayer.ResetData("ArrestsMenu::CAD");

                    CEF.Browser.Window.ExecuteJs("MenuArrest.switchContainer", 0);
                }
                else if (action == 1)
                {
                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    LastSent = Sync.World.ServerTime;

                    var res = (bool)await Events.CallRemoteProc("Police::ARF", (int)FractionType, MenuPosIdx, Player.LocalPlayer.GetData<uint>("ArrestsMenu::CAD"), null);

                    if (res)
                    {
                        if (!IsActive)
                            return;

                        await CEF.ActionBox.ShowInputWithText
                        (
                            "MenuArrestFreeInput", $"Вы хотите закрыть дело #{Player.LocalPlayer.GetData<uint>("ArrestsMenu::CAD")}", $"Введите причину амнистии", 100, "", null, null,

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

                                if (LastSent.IsSpam(500, false, true))
                                    return;

                                LastSent = Sync.World.ServerTime;

                                var res = (bool)await Events.CallRemoteProc("Police::ARF", (int)FractionType, MenuPosIdx, Player.LocalPlayer.GetData<uint>("ArrestsMenu::CAD"), str);

                                if (res)
                                {
                                    CEF.ActionBox.Close();

                                    Events.CallLocal("MenuArrest::Button", 0);
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

                    LastSent = Sync.World.ServerTime;

                    var res = (bool)await Events.CallRemoteProc("Police::ARCT", (int)FractionType, MenuPosIdx, Player.LocalPlayer.GetData<uint>("ArrestsMenu::CAD"), 0, null);

                    if (res)
                    {
                        if (!IsActive)
                            return;

                        await CEF.ActionBox.ShowInputWithText
                        (
                            "MenuArrestChangeTimeInput", "Изменение срока наказания", $"Введите число минут, на которое вы хотите изменить срок по делу #{Player.LocalPlayer.GetData<uint>("ArrestsMenu::CAD")} и причину.\n\nПример: -10, Хорошее поведение", 100, "0", null, null,

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

                                if (strD.Length < 2)
                                {
                                    return;
                                }

                                ulong minsU;

                                if (!ulong.TryParse(strD[0], out minsU))
                                {
                                    return;
                                }

                                var reasonStr = string.Join(',', strD.Skip(1));

                                var res = (bool)await Events.CallRemoteProc("Police::ARCT", (int)FractionType, MenuPosIdx, Player.LocalPlayer.GetData<uint>("ArrestsMenu::CAD"), minsU, reasonStr);

                                if (res)
                                {
                                    CEF.ActionBox.Close();

                                    Events.CallLocal("MenuArrest::Button", 0);
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

        public static async System.Threading.Tasks.Task Show(Data.Fractions.Types fType, int menuPosIdx, List<Data.Fractions.Police.ArrestInfo> arrests)
        {
            if (IsActive)
                return;

            await CEF.Browser.Render(Browser.IntTypes.MenuArrest, true, true);

            FractionType = fType;
            MenuPosIdx = menuPosIdx;

            CEF.Browser.Window.ExecuteJs("MenuArrest.fillArrests", true, arrests.Select(x => new object[] { x.Id, x.Time.ToString("dd.MM.yyyy HH:mm"), x.TargetName, x.MemberName }));

            CEF.Cursor.Show(true, true);

            EscBindIdx = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CEF.Browser.Render(Browser.IntTypes.MenuArrest, false, false);

            Player.LocalPlayer.ResetData("ArrestsMenu::CAD");

            KeyBinds.Unbind(EscBindIdx);

            EscBindIdx = -1;

            CEF.Cursor.Show(false, false);
        }
    }
}
