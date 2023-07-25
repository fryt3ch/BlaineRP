using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Data;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Management.Camera;
using BlaineRP.Client.Game.World;
using BlaineRP.Client.Game.Wrappers.Blips;
using BlaineRP.Client.Input.Enums;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;
using Core = BlaineRP.Client.Input.Core;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class HouseMenu
    {
        public static bool IsActive => CEF.Browser.IsRendered(Browser.IntTypes.MenuHome);

        public static DateTime LastSent;

        private static int EscBind { get; set; }

        public HouseMenu()
        {
            LastSent = DateTime.MinValue;

            EscBind = -1;

            Events.Add("HouseMenu::Show", (args) =>
            {
                var data = RAGE.Util.Json.Deserialize<Dictionary<string, bool[]>>((string)args[0]);

                var balance = Utils.Convert.ToUInt64(args[1]);
                var dState = (bool)args[2];
                var cState = (bool)args[3];

                Show(data.Select(x => { var sData = x.Key.Split('_'); return new object[] { uint.Parse(sData[2]), $"{sData[0]} {sData[1]}", x.Value }; }).ToArray(), balance, dState, cState);
            });

            Events.Add("MenuHome::Action", async (args) =>
            {
                var house = Player.LocalPlayer.GetData<Client.Data.Locations.HouseBase>("House::CurrentHouse");

                if (house == null)
                    return;

                var id = (string)args[0];

                if (id == "entry" || id == "closet") // states
                {
                    var state = (bool)args[1];

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    LastSent = World.Core.ServerTime;

                    Events.CallRemote("House::Lock", id == "entry", !state);
                }
                else if (id == "locate" || id == "rearrange" || id == "remove" || id == "sellfurn") // furn
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    var fUid = uint.Parse((string)args[1]);

                    var furn = Sync.House.Furniture.GetValueOrDefault(fUid);
                    var pFurn = pData.Furniture.GetValueOrDefault(fUid);

                    if (id == "locate")
                    {
                        if (furn == null)
                            return;

                        Sync.House.FindObject(furn);
                    }
                    else if (id == "rearrange")
                    {
                        if (furn == null && pFurn == null)
                            return;

                        var curStyle = Player.LocalPlayer.GetData<Sync.House.Style>("House::CurrentHouse::Style");

                        if (curStyle == null)
                            return;

                        if (LastSent.IsSpam(1000, false, true))
                            return;

                        LastSent = World.Core.ServerTime;

                        if ((bool)await Events.CallRemoteProc("House::Menu::Furn::Start", fUid))
                        {
                            if (!IsActive)
                                return;

                            CEF.MapEditor.Close();

                            if (furn == null)
                            {
                                var pos = Management.Camera.Core.GetFrontOf(Player.LocalPlayer.Position, Player.LocalPlayer.GetHeading(), 2f);

                                furn = Streaming.CreateObjectNoOffsetImmediately(pFurn.Model, pos.X, pos.Y, pos.Z);

                                furn.SetAlpha(125, false);

                                furn.SetTotallyInvincible(true);
                                furn.SetCollision(false, false);
                            }
                            else
                            {
                                var pos = furn.GetCoords(false);
                                var rot = furn.GetRotation(2);
                                var model = furn.GetData<Client.Data.Furniture>("Data")?.Model ?? 0;

                                furn.SetVisible(false, false);
                                furn.SetCollision(false, false);

                                furn.GetData<ExtraBlip>("Blip")?.Destroy();

                                furn = Streaming.CreateObjectNoOffsetImmediately(model, pos.X, pos.Y, pos.Z);

                                furn.SetRotation(rot.X, rot.Y, rot.Z, 2, false);
                                furn.SetAlpha(125, false);

                                furn.SetTotallyInvincible(true);
                                furn.SetCollision(false, false);
                            }

                            furn.SetData("UID", fUid);

                            CEF.MapEditor.PositionLimit = curStyle.InteriorPosition;

                            CEF.MapEditor.Show
                            (
                                furn, "HouseFurnitureEdit", new MapEditor.Mode(true, true, true, false, true, false),

                                () =>
                                {
                                    FurnitureEditOnStart(furn);
                                },

                                () => CEF.MapEditor.RenderFurnitureEdit(),

                                () =>
                                {
                                    FurnitureEditOnEnd(furn);
                                },

                                (pos, rot) =>
                                {
                                    FurntureEditFinish(furn, pos, rot);
                                }
                            );

                            return;
                        }
                    }
                    else if (id == "remove")
                    {
                        if (furn == null)
                            return;

                        if (LastSent.IsSpam(1000, false, true))
                            return;

                        Events.CallRemote("House::Menu::Furn::Remove", fUid);

                        LastSent = World.Core.ServerTime;
                    }
                    else if (id == "sellfurn")
                    {
                        if (pFurn == null)
                            return;
                    }
                }
                else if (id == "sell2gov")
                {
                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    if (!Player.LocalPlayer.HasData("HouseMenu::SellGov::ApproveTime") || World.Core.ServerTime.Subtract(Player.LocalPlayer.GetData<DateTime>("HouseMenu::SellGov::ApproveTime")).TotalMilliseconds > 5000)
                    {
                        Player.LocalPlayer.SetData("HouseMenu::SellGov::ApproveTime", World.Core.ServerTime);

                        CEF.Notification.Show(CEF.Notification.Types.Question, Locale.Get("NOTIFICATION_HEADER_APPROVE"), string.Format(Locale.Notifications.Money.AdmitToSellGov1, Locale.Get("GEN_MONEY_0", Utils.Misc.GetGovSellPrice(house.Price))), 5000);
                    }
                    else
                    {
                        Player.LocalPlayer.ResetData("HouseMenu::SellGov::ApproveTime");

                        if ((bool)await Events.CallRemoteProc("House::STG"))
                            Close();
                    }
                }
                else if (id == "browse" || id == "cash" || id == "bank") // layouts
                {
                    var id2 = (string)args[1];

                    if (!id2.Contains("hlo_"))
                        return;

                    var layoutId = ushort.Parse(id2.Replace("hlo_", ""));

                    var style = Sync.House.Style.Get(layoutId);

                    if (style == null)
                        return;

                    var curStyle = Player.LocalPlayer.GetData<Sync.House.Style>("House::CurrentHouse::Style");

                    if (curStyle == null)
                        return;

                    if (id == "browse")
                    {
                        if (LastSent.IsSpam(1000, false, true))
                            return;

                        LastSent = World.Core.ServerTime;

                        CEF.Browser.Ghostify(Browser.IntTypes.MenuHome, true);

                        var res = (bool)await Events.CallRemoteProc("House::SSOV", layoutId, Sync.House.CurrentOverviewStyle ?? Sync.House.Style.All.Where(x => x.Value == curStyle).FirstOrDefault().Key);

                        if (!res)
                        {
                            CEF.Browser.Ghostify(Browser.IntTypes.MenuHome, false);

                            return;
                        }

                        await RAGE.Game.Invoker.WaitAsync(1);

                        CEF.Browser.Ghostify(Browser.IntTypes.MenuHome, false);

                        StyleOverviewStart(layoutId);
                    }
                    else if (id == "cash" || id == "bank")
                    {
                        var useCash = id == "cash";

                        var approveContext = $"HouseMenuBuyStyle_{layoutId}";
                        var approveTime = 5_000;

                        if (CEF.Notification.HasApproveTimedOut(approveContext, World.Core.ServerTime, approveTime))
                        {
                            if (LastSent.IsSpam(1000, false, true))
                                return;

                            LastSent = World.Core.ServerTime;

                            CEF.Notification.SetCurrentApproveContext(approveContext, World.Core.ServerTime);

                            CEF.Notification.Show(CEF.Notification.Types.Question, Locale.Get("NOTIFICATION_HEADER_APPROVE"), curStyle.IsTypeFamiliar(layoutId) ? Locale.Get("HOUSE_STYLE_APPROVE_0") : Locale.Get("HOUSE_STYLE_APPROVE_1"), approveTime);
                        }
                        else
                        {
                            CEF.Notification.ClearAll();

                            CEF.Notification.SetCurrentApproveContext(null, DateTime.MinValue);

                            var res = (bool)await Events.CallRemoteProc("House::BST", layoutId, useCash);
                        }
                    }
                }
                else if (id == "expel") // expel settler
                {
                    var cid = Utils.Convert.ToUInt32(args[1]);

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    Events.CallRemote("House::Menu::Expel", cid);
                }
                else if (id == "apply-color" || id == "reset-color") // light colours
                {
                    var lIdx = int.Parse(((string)args[1]).Replace("ls_", ""));

                    var light = Sync.House.Lights.GetValueOrDefault(lIdx);

                    if (light == null)
                        return;

                    var rgb = Sync.House.DefaultLightColour;

                    if (id == "apply-color")
                    {
                        rgb = new Utils.Colour((string)args[2]);
                    }

                    var curRgb = light.RGB;

                    if (rgb.Red == curRgb.Red && rgb.Green == curRgb.Green && rgb.Blue == curRgb.Blue)
                        return;

                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    Events.CallRemote("House::Menu::Light::RGB", lIdx, rgb.Red, rgb.Green, rgb.Blue);

                    LastSent = World.Core.ServerTime;
                }
            });

            Events.Add("MenuHome::CheckBox", (args) =>
            {
                if (LastSent.IsSpam(1000, false, false))
                    return;

                string id = (string)args[0];

                bool state = (bool)args[1];

                if (id == "light" || id == "doors" || id == "closet" || id == "wardrobe" || id == "fridge")
                {
                    var permId = id == "light" ? 0 : id == "doors" ? 1 : id == "closet" ? 2 : id == "wardrobe" ? 3 : 4; // 4 - "fridge"

                    var cid = (uint)(int)args[2];

                    Events.CallRemote("House::Menu::Permission", permId, cid, state);
                }
                else if (id.Contains("ls_"))
                {
                    var lIdx = int.Parse(id.Replace("ls_", ""));

                    var light = Sync.House.Lights.GetValueOrDefault(lIdx);

                    if (light == null)
                        return;

                    Events.CallRemote("House::Menu::Light::State", lIdx, state);
                }

                LastSent = World.Core.ServerTime;
            });

            Events.Add("HomeMenu::UpdateLightColor", (args) =>
            {
                var id = (string)args[0];

                var light = Sync.House.Lights.GetValueOrDefault(int.Parse(id.Replace("ls_", "")));

                if (light == null)
                    return;

                var rgb = new Utils.Colour((string)args[1]);

                foreach (var x in light.Objects)
                    x.SetLightColour(rgb);
            });

            Events.Add("HouseMenu::SettlerPerm", (args) =>
            {
                if (!IsActive)
                    return;

                var cid = (uint)(int)args[0];

                var idx = (int)args[1];

                var state = (bool)args[2];

                CEF.Browser.Window.ExecuteJs("MenuHome.setPermit", idx, state, cid);
            });

            Events.Add("HouseMenu::SettlerUpd", (args) =>
            {
                if (!IsActive)
                    return;

                if (args[0] is string str)
                {
                    var data = str.Split('_');

                    AddSettler(uint.Parse(data[0]), $"{data[1]} {data[2]}", new bool[5]);
                }
                else
                {
                    RemoveSettler(Utils.Convert.ToUInt32(args[0]));
                }
            });

            Events.Add("MenuHome::Close", (args) => Close(false));
        }

        public static async System.Threading.Tasks.Task Show(object[] settlers, ulong balance, bool doorState, bool contState)
        {
            if (IsActive)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            if (!Player.LocalPlayer.HasData("House::CurrentHouse"))
                return;

            var house = Player.LocalPlayer.GetData<Client.Data.Locations.HouseBase>("House::CurrentHouse");

            if (house == null)
                return;

            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var style = Player.LocalPlayer.GetData<Sync.House.Style>("House::CurrentHouse::Style");

            object[] info = null;

            if (house is Client.Data.Locations.House rHouse)
            {
                info = new object[] { house.Id, house.OwnerName, house.Price, balance, house.Tax, (int)house.RoomType, rHouse.GarageType == null ? 0 : (int)rHouse.GarageType, new object[] { doorState, contState } };
            }
            else if (house is Client.Data.Locations.Apartments rApartments)
            {
                info = new object[] { rApartments.NumberInRoot + 1, house.OwnerName, house.Price, balance, house.Tax, (int)house.RoomType, 0, new object[] { doorState, contState } };
            }

            var layouts = new object[] { Sync.House.Style.All.Where(x => x.Value == style || (x.Value.IsHouseTypeSupported(house.Type) && x.Value.IsRoomTypeSupported(house.RoomType))).OrderBy(x => x.Key).Select(x => new object[] { $"hlo_{x.Key}", Sync.House.Style.GetName(x.Key), x.Value.Price }), $"hlo_{Sync.House.Style.All.Where(x => x.Value == style).FirstOrDefault().Key}" };

            var furns = new object[] { Sync.House.Furniture.Select(x => { var fData = x.Value.GetData<Client.Data.Furniture>("Data"); return new object[] { x.Key, fData.Id, fData.Name }; }), pData.Furniture.Select(x => new object[] { x.Key, x.Value.Id, x.Value.Name }), 50 };

            var lights = Sync.House.Lights.Select(x => new object[] { $"ls_{x.Key}", Locale.Get(x.Value.Objects.Count > 1 ? "HOUSEMENU_LAMPS_SET" : "HOUSEMENU_LAMPS_SINGLE", x.Key + 1), x.Value.State, x.Value.RGB.HEX });

            await CEF.Browser.Render(Browser.IntTypes.MenuHome, true, true);

            CEF.Browser.Window.ExecuteJs("MenuHome.draw", house.Type == Sync.House.HouseTypes.Apartments ? 1 : 0, new object[] { info, layouts, settlers, furns, lights });

            CEF.Cursor.Show(true, true);

            EscBind = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () =>
            {
                if (Sync.House.CurrentOverviewStyle is ushort curStyle)
                {
                    if (CEF.Browser.IsActive(Browser.IntTypes.MenuHome))
                    {
                        CEF.Browser.Switch(Browser.IntTypes.MenuHome, false);

                        CEF.Cursor.Show(false, false);
                    }
                    else
                    {
                        Close(false);
                    }
                }
                else
                {
                    Close(false);
                }
            });
        }

        public static void ShowRequest()
        {
            if (IsActive)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            if (!Player.LocalPlayer.HasData("House::CurrentHouse"))
            {
                CEF.Notification.ShowError(Locale.Notifications.House.NotInAnyHouse);

                return;
            }

            if (LastSent.IsSpam(1000, false, false))
                return;

            Events.CallRemote("House::Menu::Show");

            LastSent = World.Core.ServerTime;
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            if (CEF.MapEditor.CurrentContext == "HouseFurnitureEdit")
                CEF.MapEditor.Close(true);

            CEF.Browser.Render(Browser.IntTypes.MenuHome, false);

            CEF.Cursor.Show(false, false);

            Core.Unbind(EscBind);

            foreach (var x in Sync.House.Lights.Values)
            {
                if (x == null)
                    continue;

                foreach (var y in x.Objects)
                    y?.SetLightColour(x.RGB);
            }

            Player.LocalPlayer.ResetData("HouseMenu::SellGov::ApproveTime");

            if (Sync.House.CurrentOverviewStyle is ushort curStyle)
            {
                StyleOverviewStop();

                Events.CallRemote("House::FSOV", curStyle);
            }
        }

        public static void SetButtonState(string id, bool state) => CEF.Browser.Window.ExecuteJs("MenuHome.setButton", id, state);

        private static void SetCheckboxState(string id, bool state) => CEF.Browser.Window.ExecuteJs("MenuHome.setCheckBox", id, state);

        public static void SetLightState(int id, bool state) => SetCheckboxState($"ls_{id}", state);

        public static void SetLightColour(int id, Utils.Colour colour) => CEF.Browser.Window.ExecuteJs("MenuHome.applyColor", $"ls_{id}", colour.HEX);

        public static void AddSettler(uint cid, string name, bool[] permissions) => CEF.Browser.Window.ExecuteJs("MenuHome.newRoommate", cid, name, permissions);

        public static void RemoveSettler(uint cid) => CEF.Browser.Window.ExecuteJs("MenuHome.removeRoommate", cid);

        public static void AddInstalledFurniture(uint uid, Client.Data.Furniture fData) => CEF.Browser.Window.ExecuteJs("MenuHome.newFurnitureElem", "installed", uid, fData.Id, fData.Name);

        public static void RemoveInstalledFurniture(uint uid) => CEF.Browser.Window.ExecuteJs("MenuHome.removeFurniture", "installed", uid);

        public static void AddOwnedFurniture(uint uid, Client.Data.Furniture fData) => CEF.Browser.Window.ExecuteJs("MenuHome.newFurnitureElem", "possible", uid, fData.Id, fData.Name);

        public static void RemoveOwnedFurniture(uint uid) => CEF.Browser.Window.ExecuteJs("MenuHome.removeFurniture", "possible", uid);

        public static void FurnitureEditOnStart(MapObject mObj)
        {
            if (!IsActive)
                return;

            Core.Unbind(EscBind);

            CEF.Browser.SwitchTemp(Browser.IntTypes.MenuHome, false);
        }

        public static void FurnitureEditOnEnd(MapObject mObj)
        {
            if (!IsActive)
                return;

            mObj?.Destroy();

            EscBind = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));

            CEF.Browser.SwitchTemp(Browser.IntTypes.MenuHome, true);

            foreach (var x in Sync.House.Furniture.Values)
            {
                if (x?.IsVisible() == false)
                {
                    x.SetVisible(true, false);
                    x.SetCollision(true, true);
                }
            }
        }

        public static async void FurntureEditFinish(MapObject mObj, Vector3 pos, Vector3 rot)
        {
            if (LastSent.IsSpam(1000, false, false))
                return;

            LastSent = World.Core.ServerTime;

            var res = Utils.Convert.ToByte(await Events.CallRemoteProc("House::Menu::Furn::End", mObj.GetData<uint>("UID"), pos.X, pos.Y, pos.Z, rot.Z));

            if (res == 1)
            {
                CEF.MapEditor.Close();
            }
            else if (res == 255)
            {

            }
            else if (res == 0)
            {
                CEF.MapEditor.Close();
            }
        }

        public static void StyleOverviewStart(ushort styleId)
        {
            StyleOverviewStop();

            Sync.House.CurrentOverviewStyle = styleId;

            Main.Render -= StyleOverviewRender;
            Main.Render += StyleOverviewRender;

            CEF.Browser.Switch(Browser.IntTypes.MenuHome, false);

            Core.DisableAll(BindTypes.Cursor, BindTypes.MicrophoneOn, BindTypes.MicrophoneOff);

            CEF.Cursor.Show(false, false);
        }

        public static void StyleOverviewStop()
        {
            if (Sync.House.CurrentOverviewStyle == null)
                return;

            Sync.House.CurrentOverviewStyle = null;

            Main.Render -= StyleOverviewRender;

            Core.EnableAll();

            if (IsActive)
            {
                CEF.Browser.Switch(Browser.IntTypes.MenuHome, true);

                CEF.Cursor.Show(true, true);
            }
        }

        private static void StyleOverviewRender()
        {
            if (Sync.House.CurrentOverviewStyle is ushort styleId)
            {
                var isMenuActive = CEF.Browser.IsActive(Browser.IntTypes.MenuHome);

                if (!isMenuActive)
                {
                    var text = Sync.House.Style.GetName(styleId);

                    Graphics.DrawText(text, 0.5f, 0.850f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, true, true);

                    text = Locale.Get("HOUSE_STYLE_OVERVIEW_T1", Core.GetKeyString(RAGE.Ui.VirtualKeys.Escape));

                    Graphics.DrawText(text, 0.5f, 0.925f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, true, true);

                    text = Locale.Get("HOUSE_STYLE_OVERVIEW_T2", Core.GetKeyString(RAGE.Ui.VirtualKeys.M));

                    Graphics.DrawText(text, 0.5f, 0.950f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                }

                if (Core.IsDown(RAGE.Ui.VirtualKeys.M))
                {
                    if (IsActive && !isMenuActive)
                    {
                        CEF.Browser.Switch(Browser.IntTypes.MenuHome, true);

                        CEF.Cursor.Show(true, true);
                    }
                }
            }
        }
    }

    [Script(int.MaxValue)]
    public class Elevator
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.Elevator);

        private static int TempEscBind { get; set; }

        private static ContextTypes CurrentContext { get; set; }

        public enum ContextTypes
        {
            None = 0,

            ApartmentsRoot,
        }

        public Elevator()
        {
            TempEscBind = -1;

            Events.Add("Elevator::Floor", (args) =>
            {
                var floor = (int)args[0];

                if (HouseMenu.LastSent.IsSpam(500, false, false))
                    return;

                if (CurrentContext == ContextTypes.ApartmentsRoot)
                {
                    var aRoot = Player.LocalPlayer.GetData<Locations.ApartmentsRoot>("ApartmentsRoot::Current");

                    if (aRoot == null)
                        return;

                    var shell = aRoot.Shell;

                    int elevI, elevJ;

                    if (!shell.GetClosestElevator(Player.LocalPlayer.Position, out elevI, out elevJ))
                        return;

                    if (floor < shell.StartFloor)
                    {
                        floor = shell.StartFloor;
                    }

                    var curFloor = elevI + shell.StartFloor;

                    if (curFloor == floor)
                    {
                        CEF.Notification.ShowError(Locale.Notifications.General.ElevatorCurrentFloor);

                        return;
                    }

                    Events.CallRemote("ARoot::Elevator", elevI, elevJ, floor - shell.StartFloor);

                    HouseMenu.LastSent = World.Core.ServerTime;

                    Close();
                }
            });
        }

        public static async System.Threading.Tasks.Task Show(int maxFloor, int? curFloor, ContextTypes contextType)
        {
            if (IsActive)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            await CEF.Browser.Render(Browser.IntTypes.Elevator, true, true);

            CurrentContext = contextType;

            CEF.Browser.Window.ExecuteJs("Elevator.setMaxFloor", maxFloor);
            CEF.Browser.Window.ExecuteJs("Elevator.setCurrentFloor", curFloor);

            CEF.Cursor.Show(true, true);

            TempEscBind = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CurrentContext = ContextTypes.None;

            CEF.Browser.Render(Browser.IntTypes.Elevator, false);

            if (TempEscBind != -1)
                Core.Unbind(TempEscBind);

            TempEscBind = -1;

            CEF.Cursor.Show(false, false);
        }
    }
}
