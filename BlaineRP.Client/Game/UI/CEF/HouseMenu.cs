using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Estates;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Input.Enums;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class HouseMenu
    {
        public static DateTime LastSent;

        public HouseMenu()
        {
            LastSent = DateTime.MinValue;

            EscBind = -1;

            Events.Add("HouseMenu::Show",
                (args) =>
                {
                    Dictionary<string, bool[]> data = RAGE.Util.Json.Deserialize<Dictionary<string, bool[]>>((string)args[0]);

                    var balance = Utils.Convert.ToUInt64(args[1]);
                    var dState = (bool)args[2];
                    var cState = (bool)args[3];

                    Show(data.Select(x =>
                                  {
                                      string[] sData = x.Key.Split('_');
                                      return new object[]
                                      {
                                          uint.Parse(sData[2]),
                                          $"{sData[0]} {sData[1]}",
                                          x.Value,
                                      };
                                  }
                              )
                             .ToArray(),
                        balance,
                        dState,
                        cState
                    );
                }
            );

            Events.Add("MenuHome::Action",
                async (args) =>
                {
                    HouseBase house = Player.LocalPlayer.GetData<HouseBase>("House::CurrentHouse");

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

                        MapObject furn = Estates.Core.Furniture.GetValueOrDefault(fUid);
                        Furniture pFurn = pData.Furniture.GetValueOrDefault(fUid);

                        if (id == "locate")
                        {
                            if (furn == null)
                                return;

                            Estates.Core.FindObject(furn);
                        }
                        else if (id == "rearrange")
                        {
                            if (furn == null && pFurn == null)
                                return;

                            Estates.Core.Style curStyle = Player.LocalPlayer.GetData<Estates.Core.Style>("House::CurrentHouse::Style");

                            if (curStyle == null)
                                return;

                            if (LastSent.IsSpam(1000, false, true))
                                return;

                            LastSent = World.Core.ServerTime;

                            if ((bool)await Events.CallRemoteProc("House::Menu::Furn::Start", fUid))
                            {
                                if (!IsActive)
                                    return;

                                MapEditor.Close();

                                if (furn == null)
                                {
                                    Vector3 pos = Management.Camera.Core.GetFrontOf(Player.LocalPlayer.Position, Player.LocalPlayer.GetHeading(), 2f);

                                    furn = Streaming.CreateObjectNoOffsetImmediately(pFurn.Model, pos.X, pos.Y, pos.Z);

                                    furn.SetAlpha(125, false);

                                    furn.SetTotallyInvincible(true);
                                    furn.SetCollision(false, false);
                                }
                                else
                                {
                                    Vector3 pos = furn.GetCoords(false);
                                    Vector3 rot = furn.GetRotation(2);
                                    uint model = furn.GetData<Furniture>("Data")?.Model ?? 0;

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

                                MapEditor.PositionLimit = curStyle.InteriorPosition;

                                MapEditor.Show(furn,
                                    "HouseFurnitureEdit",
                                    new MapEditor.Mode(true, true, true, false, true, false),
                                    () =>
                                    {
                                        FurnitureEditOnStart(furn);
                                    },
                                    () => MapEditor.RenderFurnitureEdit(),
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

                        if (!Player.LocalPlayer.HasData("HouseMenu::SellGov::ApproveTime") ||
                            World.Core.ServerTime.Subtract(Player.LocalPlayer.GetData<DateTime>("HouseMenu::SellGov::ApproveTime")).TotalMilliseconds > 5000)
                        {
                            Player.LocalPlayer.SetData("HouseMenu::SellGov::ApproveTime", World.Core.ServerTime);

                            Notification.Show(Notification.Types.Question,
                                Locale.Get("NOTIFICATION_HEADER_APPROVE"),
                                string.Format(Locale.Notifications.Money.AdmitToSellGov1, Locale.Get("GEN_MONEY_0", Utils.Misc.GetGovSellPrice(house.Price))),
                                5000
                            );
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

                        var style = Estates.Core.Style.Get(layoutId);

                        if (style == null)
                            return;

                        Estates.Core.Style curStyle = Player.LocalPlayer.GetData<Estates.Core.Style>("House::CurrentHouse::Style");

                        if (curStyle == null)
                            return;

                        if (id == "browse")
                        {
                            if (LastSent.IsSpam(1000, false, true))
                                return;

                            LastSent = World.Core.ServerTime;

                            Browser.Ghostify(Browser.IntTypes.MenuHome, true);

                            var res = (bool)await Events.CallRemoteProc("House::SSOV",
                                layoutId,
                                Estates.Core.CurrentOverviewStyle ?? Estates.Core.Style.All.Where(x => x.Value == curStyle).FirstOrDefault().Key
                            );

                            if (!res)
                            {
                                Browser.Ghostify(Browser.IntTypes.MenuHome, false);

                                return;
                            }

                            await RAGE.Game.Invoker.WaitAsync(1);

                            Browser.Ghostify(Browser.IntTypes.MenuHome, false);

                            StyleOverviewStart(layoutId);
                        }
                        else if (id == "cash" || id == "bank")
                        {
                            bool useCash = id == "cash";

                            var approveContext = $"HouseMenuBuyStyle_{layoutId}";
                            var approveTime = 5_000;

                            if (Notification.HasApproveTimedOut(approveContext, World.Core.ServerTime, approveTime))
                            {
                                if (LastSent.IsSpam(1000, false, true))
                                    return;

                                LastSent = World.Core.ServerTime;

                                Notification.SetCurrentApproveContext(approveContext, World.Core.ServerTime);

                                Notification.Show(Notification.Types.Question,
                                    Locale.Get("NOTIFICATION_HEADER_APPROVE"),
                                    curStyle.IsTypeFamiliar(layoutId) ? Locale.Get("HOUSE_STYLE_APPROVE_0") : Locale.Get("HOUSE_STYLE_APPROVE_1"),
                                    approveTime
                                );
                            }
                            else
                            {
                                Notification.ClearAll();

                                Notification.SetCurrentApproveContext(null, DateTime.MinValue);

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

                        Estates.Core.LightsPack light = Estates.Core.Lights.GetValueOrDefault(lIdx);

                        if (light == null)
                            return;

                        Colour rgb = Estates.Core.DefaultLightColour;

                        if (id == "apply-color")
                            rgb = new Colour((string)args[2]);

                        Colour curRgb = light.RGB;

                        if (rgb.Red == curRgb.Red && rgb.Green == curRgb.Green && rgb.Blue == curRgb.Blue)
                            return;

                        if (LastSent.IsSpam(1000, false, false))
                            return;

                        Events.CallRemote("House::Menu::Light::RGB", lIdx, rgb.Red, rgb.Green, rgb.Blue);

                        LastSent = World.Core.ServerTime;
                    }
                }
            );

            Events.Add("MenuHome::CheckBox",
                (args) =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    var id = (string)args[0];

                    var state = (bool)args[1];

                    if (id == "light" || id == "doors" || id == "closet" || id == "wardrobe" || id == "fridge")
                    {
                        int permId = id == "light" ? 0 : id == "doors" ? 1 : id == "closet" ? 2 : id == "wardrobe" ? 3 : 4; // 4 - "fridge"

                        var cid = (uint)(int)args[2];

                        Events.CallRemote("House::Menu::Permission", permId, cid, state);
                    }
                    else if (id.Contains("ls_"))
                    {
                        var lIdx = int.Parse(id.Replace("ls_", ""));

                        Estates.Core.LightsPack light = Estates.Core.Lights.GetValueOrDefault(lIdx);

                        if (light == null)
                            return;

                        Events.CallRemote("House::Menu::Light::State", lIdx, state);
                    }

                    LastSent = World.Core.ServerTime;
                }
            );

            Events.Add("HomeMenu::UpdateLightColor",
                (args) =>
                {
                    var id = (string)args[0];

                    Estates.Core.LightsPack light = Estates.Core.Lights.GetValueOrDefault(int.Parse(id.Replace("ls_", "")));

                    if (light == null)
                        return;

                    var rgb = new Colour((string)args[1]);

                    foreach (MapObject x in light.Objects)
                    {
                        x.SetLightColour(rgb);
                    }
                }
            );

            Events.Add("HouseMenu::SettlerPerm",
                (args) =>
                {
                    if (!IsActive)
                        return;

                    var cid = (uint)(int)args[0];

                    var idx = (int)args[1];

                    var state = (bool)args[2];

                    Browser.Window.ExecuteJs("MenuHome.setPermit", idx, state, cid);
                }
            );

            Events.Add("HouseMenu::SettlerUpd",
                (args) =>
                {
                    if (!IsActive)
                        return;

                    if (args[0] is string str)
                    {
                        string[] data = str.Split('_');

                        AddSettler(uint.Parse(data[0]), $"{data[1]} {data[2]}", new bool[5]);
                    }
                    else
                    {
                        RemoveSettler(Utils.Convert.ToUInt32(args[0]));
                    }
                }
            );

            Events.Add("MenuHome::Close", (args) => Close(false));
        }

        public static bool IsActive => Browser.IsRendered(Browser.IntTypes.MenuHome);

        private static int EscBind { get; set; }

        public static async System.Threading.Tasks.Task Show(object[] settlers, ulong balance, bool doorState, bool contState)
        {
            if (IsActive)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            if (!Player.LocalPlayer.HasData("House::CurrentHouse"))
                return;

            HouseBase house = Player.LocalPlayer.GetData<HouseBase>("House::CurrentHouse");

            if (house == null)
                return;

            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            Estates.Core.Style style = Player.LocalPlayer.GetData<Estates.Core.Style>("House::CurrentHouse::Style");

            object[] info = null;

            if (house is House rHouse)
                info = new object[]
                {
                    house.Id,
                    house.OwnerName,
                    house.Price,
                    balance,
                    house.Tax,
                    (int)house.RoomType,
                    rHouse.GarageType == null ? 0 : (int)rHouse.GarageType,
                    new object[]
                    {
                        doorState,
                        contState,
                    },
                };
            else if (house is Apartments rApartments)
                info = new object[]
                {
                    rApartments.NumberInRoot + 1,
                    house.OwnerName,
                    house.Price,
                    balance,
                    house.Tax,
                    (int)house.RoomType,
                    0,
                    new object[]
                    {
                        doorState,
                        contState,
                    },
                };

            var layouts = new object[]
            {
                Estates.Core.Style.All.Where(x => x.Value == style || x.Value.IsHouseTypeSupported(house.Type) && x.Value.IsRoomTypeSupported(house.RoomType))
                       .OrderBy(x => x.Key)
                       .Select(x => new object[]
                            {
                                $"hlo_{x.Key}",
                                Estates.Core.Style.GetName(x.Key),
                                x.Value.Price,
                            }
                        ),
                $"hlo_{Estates.Core.Style.All.Where(x => x.Value == style).FirstOrDefault().Key}",
            };

            var furns = new object[]
            {
                Estates.Core.Furniture.Select(x =>
                    {
                        Furniture fData = x.Value.GetData<Furniture>("Data");
                        return new object[]
                        {
                            x.Key,
                            fData.Id,
                            fData.Name,
                        };
                    }
                ),
                pData.Furniture.Select(x => new object[]
                    {
                        x.Key,
                        x.Value.Id,
                        x.Value.Name,
                    }
                ),
                50,
            };

            IEnumerable<object[]> lights = Estates.Core.Lights.Select(x => new object[]
                {
                    $"ls_{x.Key}",
                    Locale.Get(x.Value.Objects.Count > 1 ? "HOUSEMENU_LAMPS_SET" : "HOUSEMENU_LAMPS_SINGLE", x.Key + 1),
                    x.Value.State,
                    x.Value.RGB.HEX,
                }
            );

            await Browser.Render(Browser.IntTypes.MenuHome, true, true);

            Browser.Window.ExecuteJs("MenuHome.draw",
                house.Type == Estates.HouseBase.Types.Apartments ? 1 : 0,
                new object[]
                {
                    info,
                    layouts,
                    settlers,
                    furns,
                    lights,
                }
            );

            Cursor.Show(true, true);

            EscBind = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape,
                true,
                () =>
                {
                    if (Estates.Core.CurrentOverviewStyle is ushort curStyle)
                    {
                        if (Browser.IsActive(Browser.IntTypes.MenuHome))
                        {
                            Browser.Switch(Browser.IntTypes.MenuHome, false);

                            Cursor.Show(false, false);
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
                }
            );
        }

        public static void ShowRequest()
        {
            if (IsActive)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            if (!Player.LocalPlayer.HasData("House::CurrentHouse"))
            {
                Notification.ShowError(Locale.Notifications.House.NotInAnyHouse);

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

            if (MapEditor.CurrentContext == "HouseFurnitureEdit")
                MapEditor.Close(true);

            Browser.Render(Browser.IntTypes.MenuHome, false);

            Cursor.Show(false, false);

            Input.Core.Unbind(EscBind);

            foreach (Estates.Core.LightsPack x in Estates.Core.Lights.Values)
            {
                if (x == null)
                    continue;

                foreach (MapObject y in x.Objects)
                {
                    y?.SetLightColour(x.RGB);
                }
            }

            Player.LocalPlayer.ResetData("HouseMenu::SellGov::ApproveTime");

            if (Estates.Core.CurrentOverviewStyle is ushort curStyle)
            {
                StyleOverviewStop();

                Events.CallRemote("House::FSOV", curStyle);
            }
        }

        public static void SetButtonState(string id, bool state)
        {
            Browser.Window.ExecuteJs("MenuHome.setButton", id, state);
        }

        private static void SetCheckboxState(string id, bool state)
        {
            Browser.Window.ExecuteJs("MenuHome.setCheckBox", id, state);
        }

        public static void SetLightState(int id, bool state)
        {
            SetCheckboxState($"ls_{id}", state);
        }

        public static void SetLightColour(int id, Colour colour)
        {
            Browser.Window.ExecuteJs("MenuHome.applyColor", $"ls_{id}", colour.HEX);
        }

        public static void AddSettler(uint cid, string name, bool[] permissions)
        {
            Browser.Window.ExecuteJs("MenuHome.newRoommate", cid, name, permissions);
        }

        public static void RemoveSettler(uint cid)
        {
            Browser.Window.ExecuteJs("MenuHome.removeRoommate", cid);
        }

        public static void AddInstalledFurniture(uint uid, Furniture fData)
        {
            Browser.Window.ExecuteJs("MenuHome.newFurnitureElem", "installed", uid, fData.Id, fData.Name);
        }

        public static void RemoveInstalledFurniture(uint uid)
        {
            Browser.Window.ExecuteJs("MenuHome.removeFurniture", "installed", uid);
        }

        public static void AddOwnedFurniture(uint uid, Furniture fData)
        {
            Browser.Window.ExecuteJs("MenuHome.newFurnitureElem", "possible", uid, fData.Id, fData.Name);
        }

        public static void RemoveOwnedFurniture(uint uid)
        {
            Browser.Window.ExecuteJs("MenuHome.removeFurniture", "possible", uid);
        }

        public static void FurnitureEditOnStart(MapObject mObj)
        {
            if (!IsActive)
                return;

            Input.Core.Unbind(EscBind);

            Browser.SwitchTemp(Browser.IntTypes.MenuHome, false);
        }

        public static void FurnitureEditOnEnd(MapObject mObj)
        {
            if (!IsActive)
                return;

            mObj?.Destroy();

            EscBind = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));

            Browser.SwitchTemp(Browser.IntTypes.MenuHome, true);

            foreach (MapObject x in Estates.Core.Furniture.Values)
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
                MapEditor.Close();
            }
            else if (res == 255)
            {
            }
            else if (res == 0)
            {
                MapEditor.Close();
            }
        }

        public static void StyleOverviewStart(ushort styleId)
        {
            StyleOverviewStop();

            Estates.Core.CurrentOverviewStyle = styleId;

            Main.Render -= StyleOverviewRender;
            Main.Render += StyleOverviewRender;

            Browser.Switch(Browser.IntTypes.MenuHome, false);

            Input.Core.DisableAll(BindTypes.Cursor, BindTypes.MicrophoneOn, BindTypes.MicrophoneOff);

            Cursor.Show(false, false);
        }

        public static void StyleOverviewStop()
        {
            if (Estates.Core.CurrentOverviewStyle == null)
                return;

            Estates.Core.CurrentOverviewStyle = null;

            Main.Render -= StyleOverviewRender;

            Input.Core.EnableAll();

            if (IsActive)
            {
                Browser.Switch(Browser.IntTypes.MenuHome, true);

                Cursor.Show(true, true);
            }
        }

        private static void StyleOverviewRender()
        {
            if (Estates.Core.CurrentOverviewStyle is ushort styleId)
            {
                bool isMenuActive = Browser.IsActive(Browser.IntTypes.MenuHome);

                if (!isMenuActive)
                {
                    string text = Estates.Core.Style.GetName(styleId);

                    Graphics.DrawText(text, 0.5f, 0.850f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, true, true);

                    text = Locale.Get("HOUSE_STYLE_OVERVIEW_T1", Input.Core.GetKeyString(RAGE.Ui.VirtualKeys.Escape));

                    Graphics.DrawText(text, 0.5f, 0.925f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, true, true);

                    text = Locale.Get("HOUSE_STYLE_OVERVIEW_T2", Input.Core.GetKeyString(RAGE.Ui.VirtualKeys.M));

                    Graphics.DrawText(text, 0.5f, 0.950f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                }

                if (Input.Core.IsDown(RAGE.Ui.VirtualKeys.M))
                    if (IsActive && !isMenuActive)
                    {
                        Browser.Switch(Browser.IntTypes.MenuHome, true);

                        Cursor.Show(true, true);
                    }
            }
        }
    }
}