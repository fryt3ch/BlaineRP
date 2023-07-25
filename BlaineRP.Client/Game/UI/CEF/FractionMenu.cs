using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Fractions;
using BlaineRP.Client.Game.Fractions.Enums;
using BlaineRP.Client.Game.World;
using BlaineRP.Client.Game.Wrappers.Blips;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using Core = BlaineRP.Client.Input.Core;
using VehicleData = BlaineRP.Client.Game.EntitiesData.VehicleData;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class FractionMenu
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.MenuFraction);

        private static int EscBindIdx { get; set; } = -1;

        private static DateTime LastSent;

        private const int NEWS_TEXT_MIN_CHAR = 10;
        private const int NEWS_TEXT_MAX_CHAR = 250;
        private const int NEWS_TEXT_MAX_NL = 10;

        private static Regex RankNamePattern = new Regex(@"^[a-zA-Zа-яА-Я0-9\s]{2,12}$", RegexOptions.Compiled);

        public FractionMenu()
        {
            Events.Add("MenuFrac::Close", (args) => Close());

            Events.Add("MenuFrac::Veh", async (args) =>
            {
                var vid = Utils.Convert.ToUInt32(args[0]);

                var actionId = (int)args[1];

                if (actionId == 0) // gps
                {
                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    LastSent = World.Core.ServerTime;

                    var coords = (Vector3)await Events.CallRemoteProc("Fraction::VGPS", vid);

                    if (coords == null)
                        return;

                    Wrappers.Blips.Core.CreateGPS(coords, Settings.App.Static.MainDimension, true);
                }
                else if (actionId == 1) // respawn
                {
                    if (LastSent.IsSpam(1500, false, true))
                        return;

                    if (Fraction.AllVehicles == null)
                        return;

                    var vData = Fraction.AllVehicles.GetValueOrDefault(vid);

                    if (vData == null)
                        return;

                    if (VehicleData.GetData(Player.LocalPlayer.Vehicle)?.VID == vid)
                    {
                        CEF.Notification.ShowError(Locale.Notifications.General.QuitThisVehicle);

                        return;
                    }

                    LastSent = World.Core.ServerTime;

                    if ((bool)await Events.CallRemoteProc("Fraction::VRSP", vid))
                    {
                        CEF.Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), $"Транспорт {vData.Numberplate} был возвращен на свое парковочное место!");
                    }
                }
            });

            Events.Add("MenuFrac::ChangeVehAccess", async (args) =>
            {
                var pData = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

                if (pData == null)
                    return;

                var fData = pData.CurrentFraction;

                if (fData == null)
                    return;

                if (LastSent.IsSpam(1000, false, true))
                    return;

                if ((Fraction.AllMembers?.GetValueOrDefault(pData.CID)?.Rank ?? 0) != fData.MaxRank && fData.LeaderCID != pData.CID)
                {
                    CEF.Notification.Show("Fraction::NAL");

                    return;
                }

                var vid = Utils.Convert.ToUInt32(args[0]);

                var newMinRank = (byte)(Utils.Convert.ToInt32(args[1]) - 1);

                if (Fraction.AllVehicles == null)
                    return;

                var vData = Fraction.AllVehicles.GetValueOrDefault(vid);

                if (vData == null)
                    return;

                if (vData.MinRank == newMinRank)
                {
                    CEF.Notification.ShowError($"Этот ранг уже установлен в качестве минимального для данного транспорта!");

                    return;
                }

                LastSent = World.Core.ServerTime;

                if ((bool)await Events.CallRemoteProc("Fraction::VCMR", vid, newMinRank))
                {
                    CEF.Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), $"Вы изменили минимальный ранг для доступа к транспорту {vData.Numberplate} на {newMinRank + 1} - {fData.GetRankName(newMinRank)}!");
                }
            });

            Events.Add("MenuFrac::EmployeeAction", async (args) =>
            {
                var cid = Utils.Convert.ToUInt32(args[0]);

                var actionId = (int)args[1];

                if (Fraction.AllMembers == null)
                    return;

                var mData = Fraction.AllMembers.GetValueOrDefault(cid);

                if (mData == null)
                    return;

                var pData = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

                if (pData == null)
                    return;

                var fData = pData.CurrentFraction;

                if (fData == null)
                    return;

                if (LastSent.IsSpam(1000, false, true))
                    return;

                if (pData.CID == cid)
                {
                    CEF.Notification.Show("SA");

                    return;
                }

                if (actionId == 0) // rank up
                {
                    if (mData.Rank >= fData.MaxRank)
                    {
                        CEF.Notification.ShowError("Этот сотрудник уже имеет максимально возможную должность!");

                        return;
                    }

                    LastSent = World.Core.ServerTime;

                    var newRank = (byte)(mData.Rank + 1);

                    if ((bool)await Events.CallRemoteProc("Fraction::MRC", cid, newRank))
                    {
                        CEF.Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), $"Вы повысили сотрудника {mData.Name} #{cid}!\nЕго новая должность - {newRank + 1} - {fData.GetRankName(newRank)}");
                    }
                }
                else if (actionId == 1) // rank down
                {
                    if (mData.Rank <= 0)
                    {
                        CEF.Notification.ShowError("Этот сотрудник уже имеет минимально возможную должность!");

                        return;
                    }

                    LastSent = World.Core.ServerTime;

                    var newRank = (byte)(mData.Rank - 1);

                    if ((bool)await Events.CallRemoteProc("Fraction::MRC", cid, newRank))
                    {
                        CEF.Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), $"Вы понизили сотрудника {mData.Name} #{cid}!\nЕго новая должность - {newRank + 1} - {fData.GetRankName(newRank)}");
                    }
                }
                else if (actionId == 2) // fire
                {
                    LastSent = World.Core.ServerTime;

                    if ((bool)await Events.CallRemoteProc("Fraction::MF", cid))
                    {
                        CEF.Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), $"Вы уволили сотрудника {mData.Name} #{cid}!");
                    }
                }
            });

            Events.Add("MenuFrac::EditPosition", async (args) =>
            {
                var pData = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

                if (pData == null)
                    return;

                var fData = pData.CurrentFraction;

                if (fData == null)
                    return;

                var rankToEdit = (byte)(Utils.Convert.ToInt32(args[0]) - 1);

                if (LastSent.IsSpam(1000, false, true))
                    return;

                if ((Fraction.AllMembers?.GetValueOrDefault(pData.CID)?.Rank ?? 0) != fData.MaxRank && fData.LeaderCID != pData.CID)
                {
                    CEF.Notification.Show("Fraction::NAL");

                    return;
                }

                LastSent = World.Core.ServerTime;

                var rankData = ((JObject)await Events.CallRemoteProc("Fraction::RE", rankToEdit))?.ToObject<Dictionary<uint, int>>();

                if (rankData == null)
                    return;

                if (!IsActive)
                    return;

                Player.LocalPlayer.SetData("FractionMenu::RankEdit::CurrentRank", rankToEdit);

                CEF.Browser.Window.ExecuteJs("MenuFrac.showEdit", true, new object[] { rankToEdit + 1, rankData.Select(x => new object[] { x.Key, Fraction.GetFractionPermissionName(x.Key) ?? "null", x.Value == 1 }) });
            });

            Events.Add("MenuFrac::AccessButtons", async (args) =>
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var fData = pData.CurrentFraction;

                if (fData == null)
                    return;

                var id = (string)args[0];
                var state = (bool)args[1];

                if (LastSent.IsSpam(500, false, true))
                    return;

                LastSent = World.Core.ServerTime;

                if ((Fraction.AllMembers?.GetValueOrDefault(pData.CID)?.Rank ?? 0) != fData.MaxRank && fData.LeaderCID != pData.CID)
                {
                    CEF.Notification.Show("Fraction::NAL");

                    return;
                }

                if (id == "storage")
                {
                    if ((bool)await Events.CallRemoteProc("Fraction::SL", state))
                    {
                        CEF.Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), state ? "Вы закрыли склад!" : "Вы открыли склад!");
                    }
                }
                else if (id == "workbench")
                {
                    if ((bool)await Events.CallRemoteProc("Fraction::CWBL", state))
                    {
                        CEF.Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), state ? "Вы закрыли создание предметов из материалов!" : "Вы открыли создание предметов из материалов!");
                    }
                }
            });

            Events.Add("MenuFrac::NewsAction", async (args) =>
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var fData = pData.CurrentFraction;

                if (fData == null)
                    return;

                var actionId = (int)args[0];

                if (actionId == 0) // add news
                {
                    if (LastSent.IsSpam(500, false, true))
                        return;

                    LastSent = World.Core.ServerTime;

                    if ((Fraction.AllMembers?.GetValueOrDefault(pData.CID)?.Rank ?? 0) != fData.MaxRank && fData.LeaderCID != pData.CID)
                    {
                        CEF.Notification.Show("Fraction::NAL");

                        return;
                    }

                    Player.LocalPlayer.ResetData("MenuFrac::News::Edit::CurrentId");

                    CEF.Browser.Window.ExecuteJs("MenuFrac.editNews", true, null, null);
                }
                else if (actionId == 2) // cancel edit/add
                {
                    Player.LocalPlayer.ResetData("MenuFrac::News::Edit::CurrentId");

                    CEF.Browser.Window.ExecuteJs("MenuFrac.editNews", false);
                }
                else if (actionId == 1) // add/edit news
                {
                    if (LastSent.IsSpam(500, false, true))
                        return;

                    var text = ((string)args[2])?.Trim();

                    if (!text.IsTextLengthValid(NEWS_TEXT_MIN_CHAR, NEWS_TEXT_MAX_CHAR, true))
                        return;

                    if (text.Where(x => x == '\n').Count() > NEWS_TEXT_MAX_NL)
                    {
                        CEF.Notification.ShowError(string.Format(Locale.Notifications.General.MaximalNewLineCharacterCount, NEWS_TEXT_MAX_NL), -1);

                        return;
                    }

                    if (Player.LocalPlayer.HasData("MenuFrac::News::Edit::CurrentId"))
                    {
                        var editIdx = Player.LocalPlayer.GetData<int>("MenuFrac::News::Edit::CurrentId");

                        if ((bool)await Events.CallRemoteProc("Fraction::NEWSE", editIdx, text))
                        {
                            if (IsActive)
                            {
                                CEF.Browser.Window.ExecuteJs("MenuFrac.editNews", false);
                            }
                        }
                    }
                    else
                    {
                        if ((bool)await Events.CallRemoteProc("Fraction::NEWSE", -1, text))
                        {
                            if (IsActive)
                            {
                                CEF.Browser.Window.ExecuteJs("MenuFrac.editNews", false);
                            }
                        }
                    }
                }
            });

            Events.Add("MenuFrac::Tooltip", async (args) =>
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var fData = pData.CurrentFraction;

                if (fData == null)
                    return;

                if (Fraction.NewsData == null)
                    return;

                var tooltipId = (string)args[0];

                var idxD = Utils.Convert.ToDecimal(args[2]);

                var actionId = (int)args[1];

                if (tooltipId.Contains("news"))
                {
                    if (LastSent.IsSpam(500, false, true))
                        return;

                    LastSent = World.Core.ServerTime;

                    if ((Fraction.AllMembers?.GetValueOrDefault(pData.CID)?.Rank ?? 0) != fData.MaxRank && fData.LeaderCID != pData.CID)
                    {
                        CEF.Notification.Show("Fraction::NAL");

                        return;
                    }

                    var idx = (int)idxD;

                    if (actionId == 0) // pin/unpin
                    {
                        if ((bool)await Events.CallRemoteProc("Fraction::NEWSP", Fraction.NewsData.PinnedId == idx ? -1 : idx))
                        {

                        }
                    }
                    else if (actionId == 1) // edit
                    {
                        var text = Fraction.NewsData.All.GetValueOrDefault(idx);

                        if (text == null)
                            return;

                        Player.LocalPlayer.SetData("MenuFrac::News::Edit::CurrentId", idx);

                        CEF.Browser.Window.ExecuteJs("MenuFrac.editNews", true, idx, text);
                    }
                    else if (actionId == 2) // delete
                    {
                        if ((bool)await Events.CallRemoteProc("Fraction::NEWSD", idx))
                        {

                        }
                    }
                }
            });

            Events.Add("MenuFrac::SetPermit", async (args) =>
            {
                var permId = Utils.Convert.ToUInt32(args[0]);

                var state = (bool)args[1];

                if (!Player.LocalPlayer.HasData("FractionMenu::RankEdit::CurrentRank"))
                    return;

                if (LastSent.IsSpam(500, false, true))
                    return;

                LastSent = World.Core.ServerTime;

                if ((bool)await Events.CallRemoteProc("Fraction::RUP", Player.LocalPlayer.GetData<byte>("FractionMenu::RankEdit::CurrentRank"), permId, state))
                {
                    if (!IsActive)
                        return;

                    CEF.Browser.Window.ExecuteJs("MenuFrac.setPermit", permId, state);
                }
            });

            Events.Add("MenuFrac::PositionName", async (args) =>
            {
                if (args == null || args.Length < 1)
                    return;

                var name = ((string)args[0])?.Trim();

                if (name == null)
                    return;

                if (!RankNamePattern.IsMatch(name))
                {
                    CEF.Notification.ShowError("Название ранга должно быть от 2х до 12ти букв, цифр или знака пробел!");

                    return;
                }

                if (!Player.LocalPlayer.HasData("FractionMenu::RankEdit::CurrentRank"))
                    return;

                var rankToEdit = Player.LocalPlayer.GetData<byte>("FractionMenu::RankEdit::CurrentRank");

                var fData = PlayerData.GetData(Player.LocalPlayer)?.CurrentFraction;

                if (fData == null)
                    return;

                if (fData.GetRankName(rankToEdit) == name)
                {
                    CEF.Notification.ShowError("Эта должность уже имеет такое название!");

                    return;
                }

                for (byte i = 0; i <= fData.MaxRank; i++)
                {
                    if (fData.GetRankName(i) == name)
                    {
                        if (i == rankToEdit)
                            CEF.Notification.ShowError("Эта должность уже имеет такое название!");
                        else
                            CEF.Notification.ShowError("Одна из должностей уже имеет такое название!");

                        return;
                    }
                }

                if (LastSent.IsSpam(500, false, true))
                    return;

                LastSent = World.Core.ServerTime;

                if ((bool)await Events.CallRemoteProc("Fraction::RUN", rankToEdit, name))
                {
                    CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), $"Вы изменили название должности {rankToEdit + 1} на \"{name}\"!");
                }
            });
        }

        public static async void Show(FractionTypes type, NewsData newsData, Dictionary<uint, MemberData> members, Dictionary<uint, Fractions.VehicleData> vehs, decimal balance, byte myRank)
        {
            if (IsActive)
                return;

            var fData = Fraction.Get(type);

            await CEF.Browser.Render(Browser.IntTypes.MenuFraction, true, true);

            CEF.Cursor.Show(true, true);

            EscBindIdx = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());

            CEF.Browser.Window.ExecuteJs
            (
                "MenuFrac.draw",

                new List<object>()
                {
                    new object[]
                    {
                        new object[] { newsData.All.Select(x => new object[] { x.Key, x.Value }), newsData.PinnedId < 0 ? (int?)null : newsData.PinnedId },

                        new object[] { fData.Name, members.GetValueOrDefault(fData.LeaderCID)?.Name ?? "Отсутствует", balance, fData.Materials, myRank + 1 },
                    },

                    members.Select(x => new object[] { x.Value.IsOnline, x.Value.SubStatus, x.Value.Name, x.Key, x.Value.LastSeenDate.ToString("dd.MM.yyyy HH:mm"), x.Value.Rank + 1 }),

                    new object[]
                    {
                        Enumerable.Range(0, fData.MaxRank + 1).Select(x => new object[] { x + 1, fData.GetRankName((byte)x) }),

                        vehs.Select(x => new object[] { x.Key, x.Value.Numberplate, x.Value.MinRank + 1 }),
                    },
                }
            );

            SetLockButtonState("storage", fData.StorageLocked);
            SetLockButtonState("workbench", fData.CreationWorkbenchLocked);
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CEF.Browser.Render(Browser.IntTypes.MenuFraction, false);

            if (EscBindIdx >= 0)
            {
                Core.Unbind(EscBindIdx);

                EscBindIdx = -1;
            }

            CEF.Cursor.Show(false, false);

            Player.LocalPlayer.ResetData("FractionMenu::RankEdit::CurrentRank");

            Player.LocalPlayer.ResetData("MenuFrac::News::Edit::CurrentId");
        }

        public static void SetLockButtonState(string id, bool state)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("MenuFrac.setButton", id, state);
        }

        public static void UpdateVehicleInfo(uint vid, string where, object value)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("MenuFrac.updateVeh", vid, where, value);
        }

        public static void RemoveMember(uint cid)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("MenuFrac.deleteEmployee", cid);
        }

        public static void UpdateMember(uint cid, string where, object value)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("MenuFrac.updateEmployee", cid, where, value);
        }

        public static void AddMember(uint cid, MemberData mData)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("MenuFrac.addEmployee", mData.IsOnline, mData.SubStatus, mData.Name, cid, mData.LastSeenDate.ToString("dd.MM.yyyy HH:mm"), mData.Rank + 1);
        }

        public static void UpdateRankName(byte rank, string name)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("MenuFrac.updatePositionName", rank, name);
        }

        public static void UpdateInfoLine(int num, object value)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("MenuFrac.updateInfoLine", num, value);
        }

        public static void AddNews(int idx, string text)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("MenuFrac.addNews", idx, text);
        }

        public static void UpdateNews(int idx, string text)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("MenuFrac.updateNews", idx, text);
        }

        public static void DeleteNews(int idx)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("MenuFrac.deleteNews", idx);
        }

        public static void PinNews(int idx)
        {
            if (!IsActive)
                return;

            if (idx < 0)
                CEF.Browser.Window.ExecuteJs("MenuFrac.unpinNews();");
            else
                CEF.Browser.Window.ExecuteJs("MenuFrac.pinNews", idx);
        }
    }
}
