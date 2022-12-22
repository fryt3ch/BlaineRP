using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using static BCRPClient.Locale.Notifications.Players;

namespace BCRPClient.Sync
{
    public class House : Events.Script
    {
        public static Utils.Colour DefaultLightColour = new Utils.Colour(255, 187, 96, 255);

        /// <summary>Типы домов</summary>
        public enum HouseTypes
        {
            /// <summary>Дом</summary>
            House = 0,
            /// <summary>Квартира</summary>
            Apartments,
        }

        public class Style
        {
            /// <summary>Типы планировки</summary>
            public enum Types
            {
                First = 0,
                Second = 2,
                Third = 3,
                Fourth = 4,
                Fifth = 5,
            }

            /// <summary>Типы комнат</summary>
            public enum RoomTypes
            {
                One = 1,
                Two = 2,
                Three = 3,
                Four = 4,
                Five = 5,
            }

            public Types Type { get; private set; }

            public RoomTypes RoomType { get; private set; }

            public HouseTypes HouseType { get; private set; }

            public Vector3 Position { get; private set; }

            public Vector3 EnterancePos { get; private set; }

            public string Name { get; private set; }

            public int Price { get; private set; }

            public (string Model, Vector3 Position)[] Lights { get; set; }
            public (string Model, Vector3 Position)[] Doors { get; set; }

            /// <summary>Словарь планировок</summary>
            public static Dictionary<HouseTypes, Dictionary<RoomTypes, Dictionary<Types, Style>>> All { get; private set; } = new Dictionary<HouseTypes, Dictionary<RoomTypes, Dictionary<Types, Style>>>();

            public static Style Get(HouseTypes hType, RoomTypes rType, Types sType) => All.GetValueOrDefault(hType)?.GetValueOrDefault(rType)?.GetValueOrDefault(sType);

            public Style(HouseTypes HouseType, RoomTypes RoomType, Types Type, string Name, int Price, Vector3 Position, Vector3 EnterancePos, (string Model, Vector3 Position)[] Lights, (string Model, Vector3 Position)[] Doors)
            {
                this.HouseType = HouseType;
                this.RoomType = RoomType;

                this.Type = Type;

                this.Position = Position;

                this.EnterancePos = EnterancePos;

                this.Price = Price;
                this.Name = Name;

                for (int i = 0; i < Lights.Length; i++)
                    Lights[i].Position += Position;

                for (int i = 0; i < Doors.Length; i++)
                    Doors[i].Position += Position;

                this.Lights = Lights;
                this.Doors = Doors;

                if (!All.ContainsKey(HouseType))
                    All.Add(HouseType, new Dictionary<RoomTypes, Dictionary<Types, Style>>());

                if (!All[HouseType].ContainsKey(RoomType))
                    All[HouseType].Add(RoomType, new Dictionary<Types, Style>());

                All[HouseType][RoomType].Add(Type, this);
            }

            public static void LoadAll()
            {
                new Style(HouseTypes.House, RoomTypes.Two, Types.First, "2 комнаты | Стандартный стиль", 100, new Vector3(70f, 70f, -10f), new Vector3(67.955511f, 70.03592f, -10f),
                    new (string, Vector3)[]
                    {
                        ("brp_p_light_3_1", new Vector3(0f, 0f, 3.976f)),
                        ("brp_p_light_3_1", new Vector3(0f, -5.349999f, 3.976f)),
                        ("brp_p_light_3_1", new Vector3(5.400002f, -6.099998f, 3.976f)),
                        ("brp_p_light_3_1", new Vector3(5.641205f, 4.258736f, 3.976f)),
                        ("brp_p_light_3_1", new Vector3(3.800003f, -0.75f, 3.976f)),
                        ("brp_p_light_2_1", new Vector3(6.75f, -0.75f, 3.95f))
                    },
                    new (string, Vector3)[]
                    {
                        ("v_ilev_ra_door3", new Vector3(-0.6799995f, -1.300002f, 1.152f)),
                        ("v_ilev_ra_door3", new Vector3(2.550006f, -5.550001f, 1.152f)),
                        ("v_ilev_ra_door3", new Vector3(3.25f, -2.800003f, 1.152f)),
                        ("v_ilev_ra_door3", new Vector3(4.850007f, -0.2000022f, 1.152f)),
                        ("v_ilev_ra_door3", new Vector3(3.249999f, 1.3f, 1.152f)),
                    });

                new Style(HouseTypes.House, RoomTypes.Two, Types.Second, "2 комнаты | Темный стиль", 100, new Vector3(70f, 70f, -20f), new Vector3(67.955511f, 70.03592f, -20f),
                    new (string, Vector3)[]
                    {
                        ("brp_p_light_3_1", new Vector3(0f, 0f, 3.976f)),
                        ("brp_p_light_3_1", new Vector3(0f, -5.349999f, 3.976f)),
                        ("brp_p_light_3_1", new Vector3(5.400002f, -6.099998f, 3.976f)),
                        ("brp_p_light_3_1", new Vector3(5.641205f, 4.258736f, 3.976f)),
                        ("brp_p_light_3_1", new Vector3(3.800003f, -0.75f, 3.976f)),
                        ("brp_p_light_2_1", new Vector3(6.75f, -0.75f, 3.95f))
                    },
                    new (string, Vector3)[]
                    {
                        ("v_ilev_ra_door3", new Vector3(-0.6799995f, -1.300002f, 1.152f)),
                        ("v_ilev_ra_door3", new Vector3(2.550006f, -5.550001f, 1.152f)),
                        ("v_ilev_ra_door3", new Vector3(3.25f, -2.800003f, 1.152f)),
                        ("v_ilev_ra_door3", new Vector3(4.850007f, -0.2000022f, 1.152f)),
                        ("v_ilev_ra_door3", new Vector3(3.249999f, 1.3f, 1.152f)),
                    });

                new Style(HouseTypes.Apartments, RoomTypes.Two, Types.First, "2 комнаты | Стандартный стиль", 100, new Vector3(70f, 70f, -10f), new Vector3(67.955511f, 70.03592f, -10f),
                    new (string, Vector3)[]
                    {
                        ("brp_p_light_3_1", new Vector3(0f, 0f, 3.976f)),
                        ("brp_p_light_3_1", new Vector3(0f, -5.349999f, 3.976f)),
                        ("brp_p_light_3_1", new Vector3(5.400002f, -6.099998f, 3.976f)),
                        ("brp_p_light_3_1", new Vector3(5.641205f, 4.258736f, 3.976f)),
                        ("brp_p_light_3_1", new Vector3(3.800003f, -0.75f, 3.976f)),
                        ("brp_p_light_2_1", new Vector3(6.75f, -0.75f, 3.95f))
                    },
                    new (string, Vector3)[]
                    {
                        ("v_ilev_ra_door3", new Vector3(-0.6799995f, -1.300002f, 1.152f)),
                        ("v_ilev_ra_door3", new Vector3(2.550006f, -5.550001f, 1.152f)),
                        ("v_ilev_ra_door3", new Vector3(3.25f, -2.800003f, 1.152f)),
                        ("v_ilev_ra_door3", new Vector3(4.850007f, -0.2000022f, 1.152f)),
                        ("v_ilev_ra_door3", new Vector3(3.249999f, 1.3f, 1.152f)),
                    });

                new Style(HouseTypes.Apartments, RoomTypes.Two, Types.Second, "2 комнаты | Темный стиль", 100, new Vector3(70f, 70f, -20f), new Vector3(67.955511f, 70.03592f, -20f),
                    new (string, Vector3)[]
                    {
                        ("brp_p_light_3_1", new Vector3(0f, 0f, 3.976f)),
                        ("brp_p_light_3_1", new Vector3(0f, -5.349999f, 3.976f)),
                        ("brp_p_light_3_1", new Vector3(5.400002f, -6.099998f, 3.976f)),
                        ("brp_p_light_3_1", new Vector3(5.641205f, 4.258736f, 3.976f)),
                        ("brp_p_light_3_1", new Vector3(3.800003f, -0.75f, 3.976f)),
                        ("brp_p_light_2_1", new Vector3(6.75f, -0.75f, 3.95f))
                    },
                    new (string, Vector3)[]
                    {
                        ("v_ilev_ra_door3", new Vector3(-0.6799995f, -1.300002f, 1.152f)),
                        ("v_ilev_ra_door3", new Vector3(2.550006f, -5.550001f, 1.152f)),
                        ("v_ilev_ra_door3", new Vector3(3.25f, -2.800003f, 1.152f)),
                        ("v_ilev_ra_door3", new Vector3(4.850007f, -0.2000022f, 1.152f)),
                        ("v_ilev_ra_door3", new Vector3(3.249999f, 1.3f, 1.152f)),
                    });
            }
        }

        private static List<Additional.ExtraColshape> TempColshapes;
        private static List<Additional.ExtraBlip> TempBlips;

        public static Dictionary<int, MapObject> Lights { get; private set; }
        public static Dictionary<int, MapObject> Doors { get; private set; }

        public static Dictionary<uint, MapObject> Furniture { get; private set; }

        private static DateTime LastSent;

        public House()
        {
            LastSent = DateTime.MinValue;

            Data.Furniture.LoadAll();

            Style.LoadAll();

            TempColshapes = new List<Additional.ExtraColshape>();
            TempBlips = new List<Additional.ExtraBlip>();

            Lights = new Dictionary<int, MapObject>();
            Doors = new Dictionary<int, MapObject>();

            Furniture = new Dictionary<uint, MapObject>();

            Events.Add("Garage::Enter", async (args) =>
            {
                Utils.SetActionAsPending("Garage::Load", true);

                while (Additional.SkyCamera.IsFadedOut)
                    await RAGE.Game.Invoker.WaitAsync(250);

                if (!Utils.IsActionPending("Garage::Load"))
                    return;

                Sync.Players.CloseAll(false);

                var garage = Data.Locations.Garage.All[(uint)(int)args[0]];

                var style = Data.Locations.Garage.Style.Get(garage.Type, garage.Variation);

                var gExitCs = new Additional.Cylinder(style.EnterPosition, 1f, 2f, false, Utils.RedColor, Player.LocalPlayer.Dimension, null)
                {
                    InteractionType = Additional.ExtraColshape.InteractionTypes.GarageExit,
                };

                TempColshapes.Add(gExitCs);

                Utils.SetActionAsPending("Garage::Load", false);
            });

            Events.Add("Garage::Exit", (args) =>
            {
                foreach (var x in TempColshapes)
                    x?.Delete();

                TempColshapes.Clear();

                Utils.SetActionAsPending("Garage::Load", false);
            });

            Events.Add("ARoot::Enter", async (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var arType = (Data.Locations.ApartmentsRoot.Types)(int)args[0];

                var aRoot = Data.Locations.ApartmentsRoot.All[arType];

                Utils.SetActionAsPending("ApartmentsRoot::Load", true);

                while (Additional.SkyCamera.IsFadedOut)
                    await RAGE.Game.Invoker.WaitAsync(25);

                if (!Utils.IsActionPending("ApartmentsRoot::Load"))
                    return;

                Utils.SetActionAsPending("ApartmentsRoot::Load", false);

                aRoot.Load();

                Player.LocalPlayer.SetData("ApartmentsRoot::Current", aRoot);

                foreach (var x in pData.OwnedApartments.ToList())
                    if (x?.RootType == arType)
                        x.ToggleOwnerBlip(true);
            });

            Events.Add("ARoot::Exit", (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var aRoot = Player.LocalPlayer.GetData<Data.Locations.ApartmentsRoot>("ApartmentsRoot::Current");

                if (aRoot == null)
                    return;

                aRoot.Unload();

                Player.LocalPlayer.ResetData("ApartmentsRoot::Current");

                foreach (var x in pData.OwnedApartments.ToList())
                    if (x?.RootType == aRoot.Type)
                        x.ToggleOwnerBlip(true);
            });

            Events.Add("House::Enter", async (object[] args) =>
            {
                Utils.SetActionAsPending("House::Requested", true);

                while (Additional.SkyCamera.IsFadedOut)
                    await RAGE.Game.Invoker.WaitAsync(250);

                if (!Utils.IsActionPending("House::Requested"))
                    return;

                Utils.SetActionAsPending("House::Requested", false);

                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                Sync.Players.CloseAll(false);

                var data = RAGE.Util.Json.Deserialize<JObject>((string)args[0]);

                var id = (uint)(int)data["I"];

                var hType = (HouseTypes)(int)data["T"];

                var sType = (Style.Types)(int)data["S"];

                var dimension = (uint)data["Dim"];

                var doors = RAGE.Util.Json.Deserialize<bool[]>((string)data["DS"]);
                var lights = RAGE.Util.Json.Deserialize<JObject[]>((string)data["LS"]);

                Player.LocalPlayer.SetData("House::CurrentHouse::WI", (uint)data["WI"]);
                Player.LocalPlayer.SetData("House::CurrentHouse::LI", (uint)data["LI"]);
                Player.LocalPlayer.SetData("House::CurrentHouse::FI", (uint)data["FI"]);

                foreach (var x in RAGE.Util.Json.Deserialize<List<JObject>>((string)data["F"]))
                {
                    var fData = Data.Furniture.GetData((string)x["I"]);
                    var fUid = (uint)x["U"];

                    var fProps = x["D"].ToObject<Utils.Vector4>();

                    CreateObject(fUid, fData, fProps);
                }

                var house = hType == HouseTypes.House ? (Data.Locations.HouseBase)Data.Locations.House.All[id] : (Data.Locations.HouseBase)Data.Locations.Apartments.All[id];

                var style = Style.Get(hType, house.RoomType, sType);

                Player.LocalPlayer.SetData("House::CurrentHouse::Style", style);

                Player.LocalPlayer.SetData("House::CurrentHouse", house);

                var exitCs = new Additional.Cylinder(style.EnterancePos, 1f, 2f, false, Utils.RedColor, dimension);

                exitCs.InteractionType = Additional.ExtraColshape.InteractionTypes.HouseExit;

                TempColshapes.Add(exitCs);

                if (house is Data.Locations.House rHouse)
                {
                    if (rHouse.GarageType is Data.Locations.Garage.Types grType)
                    {
                        var gData = Data.Locations.Garage.Style.Get(grType, 0);

                        var gExitCs = new Additional.Cylinder(gData.EnterPosition, 1f, 2f, false, Utils.RedColor, dimension, null)
                        {
                            InteractionType = Additional.ExtraColshape.InteractionTypes.GarageExit,
                        };

                        TempColshapes.Add(gExitCs);
                    }

                    CEF.HUD.Menu.UpdateCurrentTypes(true, CEF.HUD.Menu.Types.Menu_House);
                }
                else if (house is Data.Locations.Apartments rApartments)
                {
                    CEF.HUD.Menu.UpdateCurrentTypes(true, CEF.HUD.Menu.Types.Menu_Apartments);
                }

                TempBlips.Add(new Additional.ExtraBlip(40, style.EnterancePos, Locale.Property.HouseExitTextLabel, 0.75f, 1, 255, 0, true, 0, 0, dimension));

                while (Player.LocalPlayer.Dimension != dimension)
                {
                    await RAGE.Game.Invoker.WaitAsync(250);

                    if (!Player.LocalPlayer.HasData("House::CurrentHouse"))
                        return;
                }

                await RAGE.Game.Invoker.WaitAsync(1000);

                if (!Player.LocalPlayer.HasData("House::CurrentHouse"))
                    return;

                for (int i = 0; i < style.Doors.Length; i++)
                {
                    var x = style.Doors[i];

                    var handle = RAGE.Game.Object.GetClosestObjectOfType(x.Position.X, x.Position.Y, x.Position.Z, 1f, RAGE.Util.Joaat.Hash(x.Model), false, true, true);

                    if (handle <= 0)
                        continue;

                    var t = new MapObject(handle);

                    Sync.DoorSystem.ToggleLock(RAGE.Game.Entity.GetEntityModel(t.Handle), t.GetCoords(false), doors[i]);

                    Doors.Add(i, t);

                    t.SetData("Interactive", true);

                    t.SetData("DoorIdx", i);
                    t.SetData("DoorState", doors[i]);

                    t.SetData("CustomAction", new Action<MapObject>((obj) =>
                    {
                        if (LastSent.IsSpam(1000))
                            return;

                        Events.CallRemote("House::Door", obj.GetData<int>("DoorIdx"), !t.GetData<bool>("DoorState"));

                        LastSent = DateTime.Now;
                    }));

                    t.SetData("CustomText", new Action<float, float>((x, y) =>
                    {
                        if (t.GetData<bool>("DoorState"))
                        {
                            Utils.DrawText("[Закрыта]", x, y -= NameTags.Interval, 255, 0, 0, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
                        }
                        else
                        {
                            Utils.DrawText("[Открыта]", x, y -= NameTags.Interval, 0, 255, 0, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
                        }

                        Utils.DrawText("Дверь", x, y -= NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true, true);
                    }));
                }

                for (int i = 0; i < style.Lights.Length; i++)
                {
                    var x = style.Lights[i];

                    var handle = RAGE.Game.Object.GetClosestObjectOfType(x.Position.X, x.Position.Y, x.Position.Z, 1f, RAGE.Util.Joaat.Hash(x.Model), false, true, true);

                    if (handle <= 0)
                        continue;

                    var t = new MapObject(handle);

                    var rgb = lights[i]["C"].ToObject<Utils.Colour>();

                    t.SetData("State", (bool)lights[i]["S"]);
                    t.SetData("RGB", rgb);

                    Lights.Add(i, t);

                    RAGE.Game.Entity.SetEntityLights(handle, !(bool)lights[i]["S"]);

                    t.SetLightColour(rgb);
                }
            });

            Events.Add("House::Exit", async (object[] args) =>
            {
                Player.LocalPlayer.ResetData("House::CurrentHouse");

                Sync.Players.CloseAll(false);

                foreach (var x in TempBlips)
                    x?.Destroy();

                TempBlips.Clear();

                foreach (var x in TempColshapes)
                    x?.Delete();

                TempColshapes.Clear();

                foreach (var x in Doors.Values)
                    x?.Destroy();

                Doors.Clear();

                foreach (var x in Lights.Values)
                    x?.Destroy();

                Lights.Clear();

                foreach (var x in Furniture.Values)
                    x?.Destroy();

                Furniture.Clear();

                Utils.SetActionAsPending("House::Requested", false);

                CEF.HUD.Menu.UpdateCurrentTypes(false, CEF.HUD.Menu.Types.Menu_House);
                CEF.HUD.Menu.UpdateCurrentTypes(false, CEF.HUD.Menu.Types.Menu_Apartments);
            });

            Events.Add("House::Door", (object[] args) =>
            {
                var dIdx = (int)args[0];
                var state = (bool)args[1];

                var door = Doors.GetValueOrDefault(dIdx);

                if (door == null)
                    return;

                door.SetData("DoorState", state);

                Sync.DoorSystem.ToggleLock(RAGE.Game.Entity.GetEntityModel(door.Handle), door.GetCoords(false), state);
            });

            Events.Add("House::Light", (object[] args) =>
            {
                var lIdx = (int)args[0];

                var light = Lights.GetValueOrDefault(lIdx);

                if (light == null)
                    return;

                if (args[1] is bool state)
                {
                    light.SetData("State", state);

                    RAGE.Game.Entity.SetEntityLights(light.Handle, !state);

                    if (CEF.HouseMenu.IsActive)
                        CEF.HouseMenu.SetLightState(lIdx, state);
                }
                else if (args[1] is string str)
                {
                    var rgb = RAGE.Util.Json.Deserialize<Utils.Colour>(str);

                    if (rgb != null)
                    {
                        light.SetData("RGB", rgb);

                        light.SetLightColour(rgb);

                        if (CEF.HouseMenu.IsActive)
                            CEF.HouseMenu.SetLightColour(lIdx, rgb);
                    }
                }
            });

            Events.Add("House::Furn", (args) =>
            {
                CEF.MapEditor.Close();

                if (args[0] is int)
                {
                    var fUid = (uint)(int)args[0];

                    var furn = Furniture.GetValueOrDefault(fUid);

                    furn?.Destroy();

                    Furniture.Remove(fUid);

                    if (CEF.HouseMenu.IsActive)
                        CEF.HouseMenu.RemoveInstalledFurniture(fUid);
                }
                else
                {
                    var data = RAGE.Util.Json.Deserialize<JObject>((string)args[0]);

                    var fUid = (uint)data["U"];

                    var furn = Furniture.GetValueOrDefault(fUid);

                    var props = data["D"].ToObject<Utils.Vector4>();

                    if (furn == null)
                    {
                        var fData = Data.Furniture.GetData((string)data["I"]);

                        CreateObject(fUid, fData, props);

                        if (CEF.HouseMenu.IsActive)
                            CEF.HouseMenu.AddInstalledFurniture(fUid, fData);
                    }
                    else
                    {

                        furn.SetCoords(props.Position.X, props.Position.Y, props.Position.Z, false, false, false, false);

                        furn.SetRotation(0f, 0f, props.RotationZ, 2, true);
                    }
                }
            });

            Events.Add("House::Lock", (args) =>
            {
                if (!CEF.HouseMenu.IsActive)
                    return;

                CEF.HouseMenu.SetButtonState((bool)args[0] ? "entry" : "closet", (bool)args[1]);
            });
        }

        public static void FindObject(MapObject obj)
        {
            AsyncTask.RunSlim(async () =>
            {
                for (int i = 0; i < 5; i++)
                {
                    obj?.SetAlpha(125, false);

                    await RAGE.Game.Invoker.WaitAsync(250);

                    obj?.SetAlpha(255, false);

                    await RAGE.Game.Invoker.WaitAsync(250);
                }
            }, 0);

            var blip = new Additional.ExtraBlip(162, obj.GetCoords(false), null, 0.75f, 3, 255, 0f, true, 0, 0f, Player.LocalPlayer.Dimension, Additional.ExtraBlip.Types.Furniture);

            blip.SetAsReachable(2.5f);

            TempBlips.Add(blip);
        }

        private static async void CreateObject(uint fUid, Data.Furniture fData, Utils.Vector4 fProps)
        {
            MapObject obj = null;

            if (fData.Type == Data.Furniture.Types.Locker)
            {
                obj = await fData.CreateObject(fProps.Position, new Vector3(0f, 0f, fProps.RotationZ), Player.LocalPlayer.Dimension, fUid, Player.LocalPlayer.GetData<uint>("House::CurrentHouse::LI"));
            }
            else if (fData.Type == Data.Furniture.Types.Wardrobe)
            {
                obj = await fData.CreateObject(fProps.Position, new Vector3(0f, 0f, fProps.RotationZ), Player.LocalPlayer.Dimension, fUid, Player.LocalPlayer.GetData<uint>("House::CurrentHouse::WI"));
            }
            else if (fData.Type == Data.Furniture.Types.Fridge)
            {
                obj = await fData.CreateObject(fProps.Position, new Vector3(0f, 0f, fProps.RotationZ), Player.LocalPlayer.Dimension, fUid, Player.LocalPlayer.GetData<uint>("House::CurrentHouse::FI"));
            }
            else if (fData.Type == Data.Furniture.Types.KitchenSet)
            {

            }
            else
            {
                obj = await fData.CreateObject(fProps.Position, new Vector3(0f, 0f, fProps.RotationZ), Player.LocalPlayer.Dimension, fUid);
            }

            obj.SetData("Data", fData);

            Furniture.Add(fUid, obj);
        }
    }
}
