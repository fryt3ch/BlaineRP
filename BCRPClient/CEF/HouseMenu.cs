using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.CEF
{
    public class HouseMenu : Events.Script
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

                var balance = args[1].ToUInt64();
                var dState = (bool)args[2];
                var cState = (bool)args[3];

                Show(data.Select(x => { var sData = x.Key.Split('_'); return new object[] { uint.Parse(sData[2]), $"{sData[0]} {sData[1]}", x.Value }; }).ToArray(), balance, dState, cState);
            });

            Events.Add("MenuHome::Action", async (args) =>
            {
                var house = Player.LocalPlayer.GetData<Data.Locations.HouseBase>("House::CurrentHouse");

                if (house == null)
                    return;

                var id = (string)args[0];

                if (id == "entry" || id == "closet") // states
                {
                    var state = (bool)args[1];

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    LastSent = Sync.World.ServerTime;

                    Events.CallRemote("House::Lock", id == "entry", !state);
                }
                else if (id == "locate" || id == "rearrange" || id == "remove" || id == "sellfurn") // furn
                {
                    var pData = Sync.Players.GetData(Player.LocalPlayer);

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
                        if (furn != null)
                        {
                            await Utils.RequestModel(furn.Model);
                        }
                        else if (pFurn != null)
                        {
                            await Utils.RequestModel(pFurn.Model);
                        }
                        else
                        {
                            return;
                        }

                        if (LastSent.IsSpam(1000, false, true))
                            return;

                        LastSent = Sync.World.ServerTime;

                        if ((bool)await Events.CallRemoteProc("House::Menu::Furn::Start", fUid))
                        {
                            if (!IsActive)
                                return;

                            CEF.MapEditor.Close();

                            if (furn == null)
                            {
                                await Utils.RequestModel(pFurn.Model);

                                var pos = Additional.Camera.GetFrontOf(Player.LocalPlayer.Position, Player.LocalPlayer.GetHeading(), 2f);

                                furn = new MapObject(RAGE.Game.Object.CreateObjectNoOffset(pFurn.Model, pos.X, pos.Y, pos.Z, false, false, false));

                                furn.SetAlpha(125, false);
                            }
                            else
                            {
                                await Utils.RequestModel(furn.Model);

                                var pos = furn.GetCoords(false);
                                var rot = furn.GetRotation(2);
                                var model = furn.Model;

                                furn.SetVisible(false, false);
                                furn.SetCollision(false, false);

                                furn.GetData<Additional.ExtraBlip>("Blip")?.Destroy();

                                furn = new MapObject(RAGE.Game.Object.CreateObjectNoOffset(model, pos.X, pos.Y, pos.Z, false, false, false));

                                furn.SetRotation(rot.X, rot.Y, rot.Z, 2, false);
                                furn.SetAlpha(125, false);
                            }

                            furn.SetData("UID", fUid);

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

                        LastSent = Sync.World.ServerTime;
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

                    if (!Player.LocalPlayer.HasData("HouseMenu::SellGov::ApproveTime") || Sync.World.ServerTime.Subtract(Player.LocalPlayer.GetData<DateTime>("HouseMenu::SellGov::ApproveTime")).TotalMilliseconds > 5000)
                    {
                        Player.LocalPlayer.SetData("HouseMenu::SellGov::ApproveTime", Sync.World.ServerTime);

                        CEF.Notification.Show(CEF.Notification.Types.Question, Locale.Notifications.ApproveHeader, string.Format(Locale.Notifications.Money.AdmitToSellGov1, Utils.GetPriceString(Utils.GetGovSellPrice(house.Price))), 5000);
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
                    string id2 = (string)args[1];

                    if (!id2.Contains("hlo_"))
                        return;

                    var layout = ushort.Parse(id2.Replace("hlo_", ""));
                }
                else if (id == "expel") // expel settler
                {
                    uint cid = (uint)(int)args[1];

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
                        rgb = ((string)args[2]).ToColour();
                    }

                    var curRgb = light.RGB;

                    if (rgb.Red == curRgb.Red && rgb.Green == curRgb.Green && rgb.Blue == curRgb.Blue)
                        return;

                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    Events.CallRemote("House::Menu::Light::RGB", lIdx, rgb.Red, rgb.Green, rgb.Blue);

                    LastSent = Sync.World.ServerTime;
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

                LastSent = Sync.World.ServerTime;
            });

            Events.Add("HomeMenu::UpdateLightColor", (args) =>
            {
                var id = (string)args[0];

                var light = Sync.House.Lights.GetValueOrDefault(int.Parse(id.Replace("ls_", "")));

                if (light == null)
                    return;

                var rgb = ((string)args[1]).ToColour();

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
                    RemoveSettler(args[0].ToUInt32());
                }
            });

            Events.Add("MenuHome::Close", (args) => Close(false));
        }

        public static async System.Threading.Tasks.Task Show(object[] settlers, ulong balance, bool doorState, bool contState)
        {
            if (IsActive)
                return;

            if (Utils.IsAnyCefActive(true))
                return;

            if (!Player.LocalPlayer.HasData("House::CurrentHouse"))
                return;

            var house = Player.LocalPlayer.GetData<Data.Locations.HouseBase>("House::CurrentHouse");

            if (house == null)
                return;

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var style = Player.LocalPlayer.GetData<Sync.House.Style>("House::CurrentHouse::Style");

            object[] info = null;

            if (house is Data.Locations.House rHouse)
            {
                info = new object[] { house.Id, house.OwnerName, house.Price, balance, house.Tax, (int)house.RoomType, rHouse.GarageType == null ? 0 : (int)rHouse.GarageType, new object[] { doorState, contState } };
            }
            else if (house is Data.Locations.Apartments rApartments)
            {
                info = new object[] { rApartments.NumberInRoot + 1, house.OwnerName, house.Price, balance, house.Tax, (int)house.RoomType, 0, new object[] { doorState, contState } };
            }

            var layouts = new object[] { Sync.House.Style.All.Where(x => x.Value == style || (x.Value.IsHouseTypeSupported(house.Type) && x.Value.IsRoomTypeSupported(house.RoomType))).Select(x => new object[] { $"hlo_{x.Key}", Sync.House.Style.GetName(x.Key), x.Value.Price }), $"hlo_{Sync.House.Style.All.Where(x => x.Value == style).FirstOrDefault().Key}" };

            var furns = new object[] { Sync.House.Furniture.Select(x => { var fData = x.Value.GetData<Data.Furniture>("Data"); return new object[] { x.Key, fData.Id, fData.Name }; }), pData.Furniture.Select(x => new object[] { x.Key, x.Value.Id, x.Value.Name }), 50 };

            var lights = Sync.House.Lights.Select(x => new object[] { $"ls_{x.Key}", x.Value.Objects.Count > 1 ? $"Набор ламп #{x.Key + 1}" : $"Лампа #{x.Key + 1}", x.Value.State, x.Value.RGB.HEX });

            await CEF.Browser.Render(Browser.IntTypes.MenuHome, true, true);

            CEF.Browser.Window.ExecuteJs("MenuHome.draw", house.Type == Sync.House.HouseTypes.Apartments ? 1 : 0, new object[] { info, layouts, settlers, furns, lights });

            CEF.Cursor.Show(true, true);

            EscBind = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));
        }

        public static void ShowRequest()
        {
            if (IsActive)
                return;

            if (Utils.IsAnyCefActive(true))
                return;

            if (!Player.LocalPlayer.HasData("House::CurrentHouse"))
            {
                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.House.NotInAnyHouse);

                return;
            }

            if (LastSent.IsSpam(1000, false, false))
                return;

            Events.CallRemote("House::Menu::Show");

            LastSent = Sync.World.ServerTime;
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            CEF.MapEditor.Close();

            CEF.Browser.Render(Browser.IntTypes.MenuHome, false);

            CEF.Cursor.Show(false, false);

            KeyBinds.Unbind(EscBind);

            foreach (var x in Sync.House.Lights.Values)
            {
                if (x == null)
                    continue;

                foreach (var y in x.Objects)
                    y?.SetLightColour(x.RGB);
            }

            Player.LocalPlayer.ResetData("HouseMenu::SellGov::ApproveTime");
        }

        public static void SetButtonState(string id, bool state) => CEF.Browser.Window.ExecuteJs("MenuHome.setButton", id, state);

        private static void SetCheckboxState(string id, bool state) => CEF.Browser.Window.ExecuteJs("MenuHome.setCheckBox", id, state);

        public static void SetLightState(int id, bool state) => SetCheckboxState($"ls_{id}", state);

        public static void SetLightColour(int id, Utils.Colour colour) => CEF.Browser.Window.ExecuteJs("MenuHome.applyColor", $"ls_{id}", colour.HEX);

        public static void AddSettler(uint cid, string name, bool[] permissions) => CEF.Browser.Window.ExecuteJs("MenuHome.newRoommate", cid, name, permissions);

        public static void RemoveSettler(uint cid) => CEF.Browser.Window.ExecuteJs("MenuHome.removeRoommate", cid);

        public static void AddInstalledFurniture(uint uid, Data.Furniture fData) => CEF.Browser.Window.ExecuteJs("MenuHome.newFurnitureElem", "installed", uid, fData.Id, fData.Name);

        public static void RemoveInstalledFurniture(uint uid) => CEF.Browser.Window.ExecuteJs("MenuHome.removeFurniture", "installed", uid);

        public static void AddOwnedFurniture(uint uid, Data.Furniture fData) => CEF.Browser.Window.ExecuteJs("MenuHome.newFurnitureElem", "possible", uid, fData.Id, fData.Name);

        public static void RemoveOwnedFurniture(uint uid) => CEF.Browser.Window.ExecuteJs("MenuHome.removeFurniture", "possible", uid);

        private static void SwitchMenu(bool state)
        {
            if (!IsActive)
                return;

            CEF.Browser.Switch(Browser.IntTypes.MenuHome, state);

            if (state)
                CEF.Cursor.Show(true, true);
        }

        public static void FurnitureEditOnStart(MapObject mObj)
        {
            if (!IsActive)
                return;

            KeyBinds.Unbind(EscBind);

            SwitchMenu(false);
        }

        public static void FurnitureEditOnEnd(MapObject mObj)
        {
            if (!IsActive)
                return;

            mObj?.Destroy();

            EscBind = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));

            SwitchMenu(true);

            foreach (var x in Sync.House.Furniture.Values)
            {
                if (x?.IsVisible() == false)
                {
                    x.SetVisible(true, false);
                    x.SetCollision(true, true);
                }
            }
        }

        public static void FurntureEditFinish(MapObject mObj, Vector3 pos, Vector3 rot)
        {
            if (LastSent.IsSpam(1000, false, false))
                return;

            Events.CallRemote("House::Menu::Furn::End", mObj.GetData<uint>("UID"), pos.X, pos.Y, pos.Z, rot.Z);

            LastSent = Sync.World.ServerTime;
        }
    }

    public class Elevator : Events.Script
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
                    var aRoot = Player.LocalPlayer.GetData<BCRPClient.Data.Locations.ApartmentsRoot>("ApartmentsRoot::Current");

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
                        CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.ElevatorCurrentFloor);

                        return;
                    }

                    Events.CallRemote("ARoot::Elevator", elevI, elevJ, floor - shell.StartFloor);

                    HouseMenu.LastSent = Sync.World.ServerTime;

                    Close();
                }
            });
        }

        public static async System.Threading.Tasks.Task Show(int maxFloor, int? curFloor, ContextTypes contextType)
        {
            if (IsActive)
                return;

            if (Utils.IsAnyCefActive(true))
                return;

            await CEF.Browser.Render(Browser.IntTypes.Elevator, true, true);

            CurrentContext = contextType;

            CEF.Browser.Window.ExecuteJs("Elevator.setMaxFloor", maxFloor);
            CEF.Browser.Window.ExecuteJs("Elevator.setCurrentFloor", curFloor);

            CEF.Cursor.Show(true, true);

            TempEscBind = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CurrentContext = ContextTypes.None;

            CEF.Browser.Render(Browser.IntTypes.Elevator, false);

            if (TempEscBind != -1)
                KeyBinds.Unbind(TempEscBind);

            TempEscBind = -1;

            CEF.Cursor.Show(false, false);
        }
    }
}
