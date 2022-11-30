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

        private static DateTime LastSent;

        private static int EscBind { get; set; }

        public HouseMenu()
        {
            LastSent = DateTime.MinValue;

            EscBind = -1;

            Events.Add("HouseMenu::Show", (args) =>
            {
                var data = RAGE.Util.Json.Deserialize<List<JObject>>((string)args[0]);

                var balance = (int)args[1];
                var dState = (bool)args[2];
                var cState = (bool)args[3];

                Show(data.Select(x => new object[] { (uint)x["C"], (string)x["N"] + " " + x["S"], RAGE.Util.Json.Deserialize<bool[]>((string)x["P"]) }).ToArray(), balance, dState, cState);
            });

            Events.Add("MenuHome::Action", async (args) =>
            {
                string id = (string)args[0];

                if (id == "entry" || id == "closet") // states
                {
                    var state = (bool)args[1];
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

                            if (furn == null)
                            {
                                furn = new MapObject(pFurn.Model, Additional.Camera.GetFrontOf(Player.LocalPlayer.Position, Player.LocalPlayer.GetHeading(), 2f), new Vector3(0f, 0f, 0f), 125, Player.LocalPlayer.Dimension);
                            }
                            else
                            {
                                CEF.MapEditor.Close();

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
                        if (args[2] == null || !(args[2] is string))
                            return;

                        rgb = ((string)args[2]).ToColor();
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

                if (id.Contains("-permit"))
                {
                    var permId = id.Contains("light") ? 0 : id.Contains("doors") ? 1 : id.Contains("closet") ? 2 : id.Contains("wardrobe") ? 3 : 4; // 4 - fridge

                    var cid = (uint)(int)args[2];

                    Events.CallRemote("House::Menu::Permission", cid, permId);
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

                var rgb = ((string)args[1]).ToColor();

                light.SetLightColour(rgb);
            });

            Events.Add("HouseMenu::SettlerPerm", (args) =>
            {
                if (!IsActive)
                    return;

                var cid = (uint)(int)args[0];

                var idx = (int)args[1];

                var state = (bool)args[2];

                if (IsActive)
                {
                    //todo
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

            var house = Player.LocalPlayer.GetData<Data.Locations.House>("House::CurrentHouse");

            if (house == null)
                return;

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var style = Player.LocalPlayer.GetData<Sync.House.Style>("House::CurrentHouse::Style");

            var info = new object[] { house.Id, house.OwnerName, house.Price, balance, 90, (int)house.RoomType, house.GarageType == null ? 0 : (int)house.GarageType, new object[] { !doorState, !contState } };

            var layouts = new object[] { Sync.House.Style.All[style.HouseType][style.RoomType].Select(x => new object[] { "hlo_" + x.Value.Type.ToString(), x.Value.Name, x.Value.Price }), "hlo_" + style.Type.ToString() };

            var furns = new object[] { Sync.House.Furniture.Select(x => { var fData = x.Value.GetData<Data.Furniture>("Data"); return new object[] { x.Key, fData.Id, fData.Name }; }), pData.Furniture.Select(x => new object[] { x.Key, x.Value.Id, x.Value.Name }) };

            var lights = Sync.House.Lights.Select(x => new object[] { $"ls_{x.Key}", $"Лампа #{x.Key}", x.Value.GetData<bool>("State"), x.Value.GetData<Utils.Colour>("RGB").ToHEX() });

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

        private static void SetCheckboxState(string id, bool state) => CEF.Browser.Window.ExecuteJs("MenuHome.setCheckBox", id, state);

        public static void SetLightState(int id, bool state) => SetCheckboxState($"ls_{id}", state);

        public static void SetLightColour(int id, Utils.Colour colour) => CEF.Browser.Window.ExecuteJs("MenuHome.applyColor", $"ls_{id}", colour.ToHEX());

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

            Events.CallRemote("House::Menu::Furn::End", mObj.GetData<uint>("UID"), RAGE.Util.Json.Serialize(pos), RAGE.Util.Json.Serialize(rot));

            LastSent = DateTime.Now;
        }
    }
}
