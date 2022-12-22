using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

                var balance = (int)args[1];
                var dState = (bool)args[2];
                var cState = (bool)args[3];

                Show(data.Select(x => { var sData = x.Key.Split('_'); return new object[] { uint.Parse(sData[2]), $"{sData[0]} {sData[1]}", x.Value }; }).ToArray(), balance, dState, cState);
            });

            Events.Add("MenuHome::Action", async (args) =>
            {
                string id = (string)args[0];

                if (id == "entry" || id == "closet") // states
                {
                    var state = (bool)args[1];

                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    LastSent = DateTime.Now;

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
                        if (furn == null && pFurn == null)
                            return;

                        if (LastSent.IsSpam(1000, false, false))
                            return;

                        LastSent = DateTime.Now;

                        if ((bool)await Events.CallRemoteProc("House::Menu::Furn::Start", fUid))
                        {
                            if (!IsActive)
                                return;

                            CEF.MapEditor.Close();

                            if (furn == null)
                            {
                                await Utils.RequestModel(pFurn.Model);

                                furn = new MapObject(pFurn.Model, Additional.Camera.GetFrontOf(Player.LocalPlayer.Position, Player.LocalPlayer.GetHeading(), 2f), new Vector3(0f, 0f, 0f), 125, Player.LocalPlayer.Dimension);
                            }
                            else
                            {
                                furn.SetVisible(false, false);
                                furn.SetCollision(false, true);

                                furn.GetData<Blip>("Blip")?.Destroy();

                                furn = new MapObject(RAGE.Game.Entity.GetEntityModel(furn.Handle), furn.GetCoords(false), furn.GetRotation(2), 125, Player.LocalPlayer.Dimension);
                            }

                            furn.SetData("UID", fUid);

                            CEF.MapEditor.Show(furn, MapEditor.ModeTypes.FurnitureEdit, false);

                            return;
                        }
                    }
                    else if (id == "remove")
                    {
                        if (furn == null)
                            return;

                        if (LastSent.IsSpam(1000, false, false))
                            return;

                        Events.CallRemote("House::Menu::Furn::Remove", fUid);

                        LastSent = DateTime.Now;
                    }
                    else if (id == "sellfurn")
                    {
                        if (pFurn == null)
                            return;
                    }
                }
                else if (id == "sell2gov")
                {

                }
                else if (id == "browse" || id == "cash" || id == "bank") // layouts
                {
                    string id2 = (string)args[1];

                    if (!id2.Contains("hlo_"))
                        return;

                    var layout = (Sync.House.Style.Types)Enum.Parse(typeof(Sync.House.Style.Types), id2.Replace("hlo_", ""));
                }
                else if (id == "expel") // expel settler
                {
                    uint cid = (uint)(int)args[1];

                    if (LastSent.IsSpam(1000, false, false))
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

                    var curRgb = light.GetData<Utils.Colour>("RGB");

                    if (rgb.Red == curRgb.Red && rgb.Green == curRgb.Green && rgb.Blue == curRgb.Blue)
                        return;

                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    Events.CallRemote("House::Menu::Light::RGB", lIdx, rgb.Red, rgb.Green, rgb.Blue);

                    LastSent = DateTime.Now;
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

                LastSent = DateTime.Now;
            });

            Events.Add("HomeMenu::UpdateLightColor", (args) =>
            {
                var id = (string)args[0];

                var light = Sync.House.Lights.GetValueOrDefault(int.Parse(id.Replace("ls_", "")));

                if (light == null)
                    return;

                var rgb = ((string)args[1]).ToColour();

                light.SetLightColour(rgb);
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
                    RemoveSettler((uint)(int)args[0]);
                }
            });

            Events.Add("MenuHome::Close", (args) => Close(false));
        }

        public static async System.Threading.Tasks.Task Show(object[] settlers, int balance, bool doorState, bool contState)
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

            var layouts = new object[] { Sync.House.Style.All[style.HouseType][style.RoomType].Select(x => new object[] { "hlo_" + x.Value.Type.ToString(), x.Value.Name, x.Value.Price }), "hlo_" + style.Type.ToString() };

            var furns = new object[] { Sync.House.Furniture.Select(x => { var fData = x.Value.GetData<Data.Furniture>("Data"); return new object[] { x.Key, fData.Id, fData.Name }; }), pData.Furniture.Select(x => new object[] { x.Key, x.Value.Id, x.Value.Name }), 50 };

            var lights = Sync.House.Lights.Select(x => new object[] { $"ls_{x.Key}", $"Лампа #{x.Key}", x.Value.GetData<bool>("State"), x.Value.GetData<Utils.Colour>("RGB").HEX });

            await CEF.Browser.Render(Browser.IntTypes.MenuHome, true, true);

            CEF.Browser.Window.ExecuteJs("MenuHome.draw", style.HouseType == Sync.House.HouseTypes.Apartments ? 1 : 0, new object[] { info, layouts, settlers, furns, lights });

            CEF.Cursor.Show(true, true);

            EscBind = RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));
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

            LastSent = DateTime.Now;
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            CEF.MapEditor.Close();

            CEF.Browser.Render(Browser.IntTypes.MenuHome, false);

            CEF.Cursor.Show(false, false);

            RAGE.Input.Unbind(EscBind);

            foreach (var x in Sync.House.Lights.Values)
            {
                x?.SetLightColour(x.GetData<Utils.Colour>("RGB"));
            }
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

            RAGE.Input.Unbind(EscBind);

            EscBind = RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => CEF.MapEditor.Close());

            SwitchMenu(false);
        }

        public static void FurnitureEditOnEnd(MapObject mObj)
        {
            if (!IsActive)
                return;

            RAGE.Input.Unbind(EscBind);

            EscBind = RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));

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

            LastSent = DateTime.Now;
        }
    }

    public class Elevator : Events.Script
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.Elevator);

        private static int TempEscBind { get; set; }

        private static ContextTypes? CurrentContext { get; set; }

        private static int CurrentFloor { get; set; }

        public enum ContextTypes
        {
            ApartmentsRoot = 0,
        }

        public Elevator()
        {
            TempEscBind = -1;

            Events.Add("Elevator::Floor", (args) =>
            {
                var floor = (int)args[0];

                if (HouseMenu.LastSent.IsSpam(500, false, false))
                    return;

                if (CurrentContext is ContextTypes context)
                {
                    if (context == ContextTypes.ApartmentsRoot)
                    {
                        if (CurrentFloor == floor)
                        {
                            CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.ElevatorCurrentFloor);

                            return;
                        }

                        Events.CallRemote("ARoot::Elevator", CurrentFloor, floor);

                        HouseMenu.LastSent = DateTime.Now;

                        Close();
                    }
                }
            });
        }

        public static async System.Threading.Tasks.Task Show(int maxFloor, int curFloor, ContextTypes contextType)
        {
            if (IsActive)
                return;

            if (Utils.IsAnyCefActive(true))
                return;

            await CEF.Browser.Render(Browser.IntTypes.Elevator, true, true);

            CurrentContext = contextType;

            CurrentFloor = curFloor;

            CEF.Browser.Window.ExecuteJs("Elevator.setMaxFloor", maxFloor);
            CEF.Browser.Window.ExecuteJs("Elevator.setCurrentFloor", curFloor);

            CEF.Cursor.Show(true, true);

            TempEscBind = RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CurrentContext = null;

            CEF.Browser.Render(Browser.IntTypes.Elevator, false);

            if (TempEscBind != -1)
                RAGE.Input.Unbind(TempEscBind);

            TempEscBind = -1;

            CEF.Cursor.Show(false, false);
        }
    }
}
