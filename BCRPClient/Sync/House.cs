using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Sync
{
    public class House : Events.Script
    {
        public static Utils.Colour DefaultLightColour => new Utils.Colour(255, 187, 96, 255);

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
            public class DoorInfo
            {
                public uint Model { get; set; }

                public Vector3 Position { get; set; }

                public DoorInfo(uint Model, Vector3 Position)
                {
                    this.Model = Model;
                    this.Position = Position;
                }
            }

            public class LightInfo
            {
                public uint Model { get; set; }

                public Vector3 Position { get; set; }

                public LightInfo(uint Model, Vector3 Position)
                {
                    this.Model = Model;
                    this.Position = Position;
                }
            }

            /// <summary>Типы комнат</summary>
            public enum RoomTypes : byte
            {
                One = 1,
                Two = 2,
                Three = 3,
                Four = 4,
                Five = 5,
            }

            private HashSet<RoomTypes> SupportedRoomTypes { get; }
            private HashSet<HouseTypes> SupportedHouseTypes { get; }
            private HashSet<ushort> FamiliarTypes { get; }

            public Utils.Vector4 InteriorPosition { get; private set; }

            public Vector3 Position { get; private set; }

            public uint Price { get; private set; }

            public LightInfo[] Lights { get; set; }
            public DoorInfo[] Doors { get; set; }

            /// <summary>Словарь планировок</summary>
            public static Dictionary<ushort, Style> All { get; private set; }

            public static Style Get(ushort sType) => All.GetValueOrDefault(sType);

            public Style(ushort Type, Vector3 Position, Utils.Vector4 InteriorPosition, uint Price, string DoorsJs, string LightsJs, string SupportedRoomTypesJs, string SupportedHouseTypesJs, string FamiliarTypesJs)
            {
                All.Add(Type, this);

                this.Position = Position;
                this.InteriorPosition = InteriorPosition;
                this.Price = Price;

                this.Doors = RAGE.Util.Json.Deserialize<DoorInfo[]>(DoorsJs);
                this.Lights = RAGE.Util.Json.Deserialize<LightInfo[]>(LightsJs);

                this.SupportedHouseTypes = RAGE.Util.Json.Deserialize<HashSet<HouseTypes>>(SupportedHouseTypesJs);
                this.SupportedRoomTypes = RAGE.Util.Json.Deserialize<HashSet<RoomTypes>>(SupportedRoomTypesJs);
                this.FamiliarTypes = RAGE.Util.Json.Deserialize<HashSet<ushort>>(FamiliarTypesJs);
            }

            public static string GetName(ushort type) => Locale.Get($"HOUSE_STYLE_{type}@Name", "null");

            public bool IsHouseTypeSupported(HouseTypes hType) => SupportedHouseTypes.Contains(hType);
            public bool IsRoomTypeSupported(RoomTypes rType) => SupportedRoomTypes.Contains(rType);
            public bool IsTypeFamiliar(ushort type) => FamiliarTypes.Contains(type);

            public static void LoadAll()
            {
                if (All != null)
                    return;

                All = new Dictionary<ushort, Style>();

                #region STYLES_TO_REPLACE

                #endregion
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
                    x?.Destroy();

                TempColshapes.Clear();

                Utils.SetActionAsPending("Garage::Load", false);
            });

            Events.Add("ARoot::Enter", async (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var arId = Convert.ToUInt32(args[0]);

                var aRoot = Data.Locations.ApartmentsRoot.All[arId];

                Utils.SetActionAsPending("ApartmentsRoot::Load", true);

                while (Additional.SkyCamera.IsFadedOut)
                    await RAGE.Game.Invoker.WaitAsync(25);

                if (!Utils.IsActionPending("ApartmentsRoot::Load"))
                    return;

                Utils.SetActionAsPending("ApartmentsRoot::Load", false);

                aRoot.Load();

                Player.LocalPlayer.SetData("ApartmentsRoot::Current", aRoot);

                foreach (var x in pData.OwnedApartments.ToList())
                    if (x?.RootId == arId)
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
                    if (x?.RootId == aRoot.Id)
                        x.ToggleOwnerBlip(true);
            });

            Events.Add("House::Enter", (object[] args) =>
            {
                HouseExit();

                AsyncTask task = null;

                task = new AsyncTask(async () =>
                {
                    while (Additional.SkyCamera.IsFadedOut)
                        await RAGE.Game.Invoker.WaitAsync(250);

                    if (!Utils.IsTaskStillPending("House::Enter", task))
                        return;

                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    Sync.Players.CloseAll(false);

                    var data = RAGE.Util.Json.Deserialize<JObject>((string)args[0]);

                    var id = (uint)(int)data["I"];

                    var hType = (HouseTypes)(int)data["T"];

                    var sType = Convert.ToUInt16(data["S"]);

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

                    var style = Style.Get(sType);

                    Player.LocalPlayer.SetData("House::CurrentHouse::Style", style);

                    Player.LocalPlayer.SetData("House::CurrentHouse", house);

                    await RAGE.Game.Invoker.WaitAsync(1000);

                    if (!Utils.IsTaskStillPending("House::Enter", task))
                        return;

                    for (int i = 0; i < style.Doors.Length; i++)
                    {
                        var x = style.Doors[i];

                        var handle = RAGE.Game.Object.GetClosestObjectOfType(x.Position.X, x.Position.Y, x.Position.Z, 1f, x.Model, false, true, true);

                        if (handle <= 0)
                            continue;

                        var state = i >= doors.Length ? false : doors[i];

                        var t = new MapObject(handle)
                        {
                            Dimension = uint.MaxValue,
                        };

                        var coords = t.GetCoords(false);

                        t.SetCoordsNoOffset(coords.X, coords.Y, coords.Z, false, false, false);

                        Sync.DoorSystem.ToggleLock(RAGE.Game.Entity.GetEntityModel(t.Handle), t.GetCoords(false), state);

                        Doors.Add(i, t);

                        t.SetData("Interactive", true);

                        t.SetData("DoorIdx", i);
                        t.SetData("DoorState", state);

                        t.SetData("CustomAction", new Action<MapObject>((obj) =>
                        {
                            if (LastSent.IsSpam(1000))
                                return;

                            Events.CallRemote("House::Door", obj.GetData<int>("DoorIdx"), !t.GetData<bool>("DoorState"));

                            LastSent = Sync.World.ServerTime;
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

                        var handle = RAGE.Game.Object.GetClosestObjectOfType(x.Position.X, x.Position.Y, x.Position.Z, 1f, x.Model, false, true, true);

                        if (handle <= 0)
                            continue;

                        var t = new MapObject(handle)
                        {
                            Dimension = uint.MaxValue,
                        };

                        var rgb = i >= lights.Length ? DefaultLightColour : lights[i]["C"].ToObject<Utils.Colour>();

                        var state = i >= lights.Length ? true : (bool)lights[i]["S"];

                        t.SetData("State", state);
                        t.SetData("RGB", rgb);

                        Lights.Add(i, t);

                        RAGE.Game.Entity.SetEntityLights(handle, !state);

                        t.SetLightColour(rgb);
                    }

                    var exitCs = new Additional.Cylinder(new Vector3(style.Position.X, style.Position.Y, style.Position.Z - 1f), 1f, 2f, false, Utils.RedColor, dimension);

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

                    TempBlips.Add(new Additional.ExtraBlip(40, style.Position, Locale.Property.HouseExitTextLabel, 0.75f, 1, 255, 0, true, 0, 0, dimension));

                    Utils.CancelPendingTask("House::Enter");

                }, 0, false, 0);

                Utils.SetTaskAsPending("House::Enter", task);
            });

            Events.Add("House::Exit", (args) => HouseExit(args));

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
                obj = await fData.CreateObject(fProps.Position, new Vector3(0f, 0f, fProps.RotationZ), Player.LocalPlayer.Dimension, fUid, fUid);
            }
            else
            {
                obj = await fData.CreateObject(fProps.Position, new Vector3(0f, 0f, fProps.RotationZ), Player.LocalPlayer.Dimension, fUid);
            }

            obj.SetData("Data", fData);

            Furniture.Add(fUid, obj);
        }

        private static void HouseExit(params object[] args)
        {
            Utils.CancelPendingTask("House::Enter");

            Player.LocalPlayer.ResetData("House::CurrentHouse");

            Sync.Players.CloseAll(false);

            foreach (var x in TempBlips)
                x?.Destroy();

            TempBlips.Clear();

            foreach (var x in TempColshapes)
                x?.Destroy();

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

            CEF.HUD.Menu.UpdateCurrentTypes(false, CEF.HUD.Menu.Types.Menu_House);
            CEF.HUD.Menu.UpdateCurrentTypes(false, CEF.HUD.Menu.Types.Menu_Apartments);
        }

        public static void UpdateAllLights()
        {
            var style = Player.LocalPlayer.GetData<Style>("House::CurrentHouse::Style");

            if (style == null)
                return;

            for (int i = 0; i < style.Lights.Length; i++)
            {
                var t = Lights.GetValueOrDefault(i);

                if (t == null)
                    continue;

                var lInfo = style.Lights[i];

                var coords = lInfo.Position;

                var handle = RAGE.Game.Object.GetClosestObjectOfType(coords.X, coords.Y, coords.Z, 1f, lInfo.Model, false, true, true);

                if (handle <= 0)
                    continue;

                var x = new RAGE.Elements.MapObject(handle)
                {
                    Dimension = uint.MaxValue,
                };

                x.SetData("State", t.GetData<bool>("State"));
                x.SetData("RGB", t.GetData<Utils.Colour>("RGB"));

                t.Destroy();

                Lights[i] = x;

                RAGE.Game.Entity.SetEntityLights(x.Handle, !x.GetData<bool>("State"));

                var rgb = x.GetData<Utils.Colour>("RGB");

                if (rgb != null)
                    x.SetLightColour(rgb);
            }
        }
    }
}
