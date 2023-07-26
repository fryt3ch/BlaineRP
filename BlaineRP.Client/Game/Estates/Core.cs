using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Estates.Garages;
using BlaineRP.Client.Game.Estates.Houses;
using BlaineRP.Client.Game.Management.Misc;
using BlaineRP.Client.Game.Misc;
using BlaineRP.Client.Game.World;
using BlaineRP.Client.Game.Wrappers.Blips;
using BlaineRP.Client.Game.Wrappers.Colshapes;
using BlaineRP.Client.Game.Wrappers.Colshapes.Enums;
using BlaineRP.Client.Game.Wrappers.Colshapes.Types;
using BlaineRP.Client.UI.CEF;
using Core = BlaineRP.Client.Game.Management.Doors.Core;
using Players = BlaineRP.Client.Sync.Players;

namespace BlaineRP.Client.Sync
{
    [Script(int.MaxValue)]
    public class House
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

        public class LightsPack
        {
            public bool State { get; set; }

            public Utils.Colour RGB { get; set; }

            public List<MapObject> Objects { get; set; }

            public LightsPack()
            {
                Objects = new List<MapObject>();
            }
        }

        public class Style
        {
            public class DoorInfo
            {
                [JsonProperty(PropertyName = "M")]
                public uint Model { get; set; }

                [JsonProperty(PropertyName = "P")]
                public Vector3 Position { get; set; }

                public DoorInfo(uint Model, Vector3 Position)
                {
                    this.Model = Model;
                    this.Position = Position;
                }
            }

            public class LightInfo
            {
                [JsonProperty(PropertyName = "M")]
                public uint Model { get; set; }

                [JsonProperty(PropertyName = "P")]
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

            public LightInfo[][] Lights { get; set; }
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
                this.Lights = RAGE.Util.Json.Deserialize<LightInfo[][]>(LightsJs);

                this.SupportedHouseTypes = RAGE.Util.Json.Deserialize<HashSet<HouseTypes>>(SupportedHouseTypesJs);
                this.SupportedRoomTypes = RAGE.Util.Json.Deserialize<HashSet<RoomTypes>>(SupportedRoomTypesJs);
                this.FamiliarTypes = RAGE.Util.Json.Deserialize<HashSet<ushort>>(FamiliarTypesJs);
            }

            public static string GetName(ushort type) => Locale.Get($"HOUSE_STYLE_{type}@Name");

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

        private static List<ExtraColshape> TempColshapes;
        private static List<ExtraBlip> TempBlips;

        public static Dictionary<int, LightsPack> Lights { get; private set; }
        public static Dictionary<int, MapObject> Doors { get; private set; }

        public static Dictionary<uint, MapObject> Furniture { get; private set; }

        public static ushort? CurrentOverviewStyle { get; set; }

        public static DateTime LastSent;

        public House()
        {
            Data.Furniture.LoadAll();

            Style.LoadAll();

            TempColshapes = new List<ExtraColshape>();
            TempBlips = new List<ExtraBlip>();

            Lights = new Dictionary<int, LightsPack>();
            Doors = new Dictionary<int, MapObject>();

            Furniture = new Dictionary<uint, MapObject>();

            Events.Add("Garage::Enter", (args) =>
            {
                var taskKey = "GarageEnter";

                AsyncTask.Methods.CancelPendingTask(taskKey);

                AsyncTask task = null;

                task = new AsyncTask(async () =>
                {
                    while (SkyCamera.IsFadedOut)
                        await RAGE.Game.Invoker.WaitAsync(25);

                    if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                        return;

                    Players.CloseAll(false);

                    var garage = Garage.All[(uint)(int)args[0]];

                    var style = Garage.Style.Get(garage.Type, garage.Variation);

                    var gExitCs = new Cylinder(new Vector3(style.EnterPosition.X, style.EnterPosition.Y, style.EnterPosition.Z - 1f), 1f, 2f, false, Utils.Misc.RedColor, Player.LocalPlayer.Dimension, null)
                    {
                        InteractionType = InteractionTypes.GarageExit,
                    };

                    TempColshapes.Add(gExitCs);

                    AsyncTask.Methods.CancelPendingTask(taskKey);
                });

                AsyncTask.Methods.SetAsPending(task, taskKey);
            });

            Events.Add("Garage::Exit", (args) =>
            {
                var taskKey = "GarageEnter";

                AsyncTask.Methods.CancelPendingTask(taskKey);

                foreach (var x in TempColshapes)
                    x?.Destroy();

                TempColshapes.Clear();
            });

            Events.Add("ARoot::Enter", async (args) =>
            {
                var taskKey = "ARootEnter";

                AsyncTask.Methods.CancelPendingTask(taskKey);

                AsyncTask task = null;

                task = new AsyncTask(async () =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    var arId = Utils.Convert.ToUInt32(args[0]);

                    var aRoot = ApartmentsRoot.All[arId];

                    while (SkyCamera.IsFadedOut)
                        await RAGE.Game.Invoker.WaitAsync(25);

                    if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                        return;

                    aRoot.Load();

                    Player.LocalPlayer.SetData("ApartmentsRoot::Current", aRoot);

                    foreach (var x in pData.OwnedApartments.ToList())
                        if (x?.RootId == arId)
                            x.ToggleOwnerBlip(true);

                    AsyncTask.Methods.CancelPendingTask(taskKey);
                });

                AsyncTask.Methods.SetAsPending(task, taskKey);
            });

            Events.Add("ARoot::Exit", (args) =>
            {
                var taskKey = "ARootEnter";

                AsyncTask.Methods.CancelPendingTask(taskKey);

                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var aRoot = Player.LocalPlayer.GetData<ApartmentsRoot>("ApartmentsRoot::Current");

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
                var taskKey = "House::Enter";

                HouseExit();

                AsyncTask task = null;

                void onStopTask()
                {
                    SkyCamera.FadeScreen(false, 500, -1);

                    Main.DisableAllControls(false);
                }

                task = new AsyncTask(async () =>
                {
                    var data = RAGE.Util.Json.Deserialize<JObject>((string)args[0]);

                    var id = (uint)(int)data["I"];

                    var hType = (HouseTypes)(int)data["T"];

                    var sType = Utils.Convert.ToUInt16(data["S"]);

                    var doors = RAGE.Util.Json.Deserialize<bool[]>((string)data["DS"]);
                    var lights = RAGE.Util.Json.Deserialize<JObject[]>((string)data["LS"]);

                    var house = hType == HouseTypes.House ? (HouseBase)Game.Estates.Houses.House.All[id] : (HouseBase)Apartments.All[id];

                    var style = Style.Get(sType);

                    while (SkyCamera.IsFadedOut && AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                        await RAGE.Game.Invoker.WaitAsync(25);

                    if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                        return;

                    SkyCamera.FadeScreen(true, 0, -1);

                    Main.DisableAllControls(true);

                    var interior = RAGE.Game.Interior.GetInteriorAtCoords(style.InteriorPosition.X, style.InteriorPosition.Y, style.InteriorPosition.Z);

                    if (!RAGE.Game.Interior.IsValidInterior(interior))
                    {
                        onStopTask();

                        return;
                    }

                    while (!RAGE.Game.Interior.IsInteriorReady(interior) && AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                        await RAGE.Game.Invoker.WaitAsync(5);

                    if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                    {
                        onStopTask();

                        return;
                    }

                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    Players.CloseAll(false);

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

                    Player.LocalPlayer.SetData("House::CurrentHouse::Style", style);

                    Player.LocalPlayer.SetData("House::CurrentHouse", house);

                    var doorLockedStr = Locale.Get("HOUSE_DOOR_LOCKED_L");
                    var doorNotLockedStr = Locale.Get("HOUSE_DOOR_NOTLOCKED_L");

                    for (int i = 0; i < style.Doors.Length; i++)
                    {
                        var x = style.Doors[i];

                        int handle = 0;

                        while ((handle = RAGE.Game.Object.GetClosestObjectOfType(x.Position.X, x.Position.Y, x.Position.Z, 1f, x.Model, false, true, true)) <= 0 && AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                        {
                            await RAGE.Game.Invoker.WaitAsync(5);

                            if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                            {
                                onStopTask();

                                return;
                            }
                        }

                        if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                        {
                            onStopTask();

                            return;
                        }

                        var state = i >= doors.Length ? false : doors[i];

                        var t = new MapObject(handle)
                        {
                            Dimension = uint.MaxValue,
                        };

                        var coords = t.GetCoords(false);

                        t.SetCoordsNoOffset(coords.X, coords.Y, coords.Z, false, false, false);

                        Core.ToggleLock(RAGE.Game.Entity.GetEntityModel(t.Handle), t.GetCoords(false), state);

                        Doors.Add(i, t);

                        t.SetData("Interactive", true);

                        t.SetData("DoorIdx", i);
                        t.SetData("DoorState", state);

                        t.SetData("CustomAction", new Action<MapObject>((obj) =>
                        {
                            if (LastSent.IsSpam(1000, false, true))
                                return;

                            LastSent = Game.World.Core.ServerTime;

                            Events.CallRemote("House::Door", obj.GetData<int>("DoorIdx"), !t.GetData<bool>("DoorState"));
                        }));

                        t.SetData("CustomText", new Action<float, float>((x, y) =>
                        {
                            if (t.GetData<bool>("DoorState"))
                            {
                                Graphics.DrawText(doorLockedStr, x, y -= NameTags.Interval, 255, 0, 0, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                            }
                            else
                            {
                                Graphics.DrawText(doorNotLockedStr, x, y -= NameTags.Interval, 0, 255, 0, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                            }

                            Graphics.DrawText("Дверь", x, y -= NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
                        }));
                    }

                    for (int i = 0; i < style.Lights.Length; i++)
                    {
                        var x = style.Lights[i];

                        var info = new LightsPack();

                        info.RGB = i >= lights.Length ? DefaultLightColour : lights[i]["C"].ToObject<Utils.Colour>();
                        info.State = i >= lights.Length ? true : (bool)lights[i]["S"];

                        Lights.Add(i, info);

                        for (int j = 0; j < x.Length; j++)
                        {
                            var y = x[j];

                            int handle = 0;

                            while ((handle = RAGE.Game.Object.GetClosestObjectOfType(y.Position.X, y.Position.Y, y.Position.Z, 1f, y.Model, false, true, true)) <= 0 && AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                            {
                                await RAGE.Game.Invoker.WaitAsync(5);

                                if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                                {
                                    onStopTask();

                                    return;
                                }
                            }

                            if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                            {
                                onStopTask();

                                return;
                            }

                            var t = new MapObject(handle)
                            {
                                Dimension = uint.MaxValue,
                            };

                            info.Objects.Add(t);

                            RAGE.Game.Entity.SetEntityLights(handle, !info.State);

                            t.SetLightColour(info.RGB);

                            //new TextLabel(t.GetOffsetFromInWorldCoords(0f, -0.15f, -0.15f), $"{i + 1}", new RGBA(255, 255, 255, 255), 5f, 0, false, uint.MaxValue) {  LOS = false, };
                        }
                    }

                    var exitCs = new Cylinder(new Vector3(style.Position.X, style.Position.Y, style.Position.Z - 1f), 1f, 2f, false, Utils.Misc.RedColor, uint.MaxValue);

                    exitCs.InteractionType = InteractionTypes.HouseExit;

                    TempColshapes.Add(exitCs);

                    if (house is Game.Estates.Houses.House rHouse)
                    {
                        if (rHouse.GarageType is Garage.Types grType)
                        {
                            var gData = Garage.Style.Get(grType, 0);

                            var gExitCs = new Cylinder(new Vector3(gData.EnterPosition.X, gData.EnterPosition.Y, gData.EnterPosition.Z - 1f), 1f, 2f, false, Utils.Misc.RedColor, uint.MaxValue, null)
                            {
                                InteractionType = InteractionTypes.GarageExit,
                            };

                            TempColshapes.Add(gExitCs);
                        }

                        HUD.Menu.UpdateCurrentTypes(true, HUD.Menu.Types.Menu_House);
                    }
                    else if (house is Apartments rApartments)
                    {
                        HUD.Menu.UpdateCurrentTypes(true, HUD.Menu.Types.Menu_Apartments);
                    }

                    TempBlips.Add(new ExtraBlip(40, style.Position, Locale.Property.HouseExitTextLabel, 0.75f, 1, 255, 0, true, 0, 0, uint.MaxValue));

                    AsyncTask.Methods.CancelPendingTask(taskKey);

                    Main.DisableAllControls(false);

                    SkyCamera.FadeScreen(false, 500, -1);
                }, 0, false, 0);

                AsyncTask.Methods.SetAsPending(task, taskKey);
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

                Core.ToggleLock(RAGE.Game.Entity.GetEntityModel(door.Handle), door.GetCoords(false), state);
            });

            Events.Add("House::Light", (object[] args) =>
            {
                var lIdx = (int)args[0];

                var lightsPack = Lights.GetValueOrDefault(lIdx);

                if (lightsPack == null)
                    return;

                if (args[1] is bool state)
                {
                    lightsPack.State = state;

                    foreach (var x in lightsPack.Objects)
                    {
                        RAGE.Game.Entity.SetEntityLights(x.Handle, !state);
                    }

                    if (HouseMenu.IsActive)
                        HouseMenu.SetLightState(lIdx, state);
                }
                else if (args[1] is string str)
                {
                    var rgb = RAGE.Util.Json.Deserialize<Utils.Colour>(str);

                    if (rgb != null)
                    {
                        lightsPack.RGB = rgb;

                        foreach (var x in lightsPack.Objects)
                        {
                            x.SetLightColour(rgb);
                        }

                        if (HouseMenu.IsActive)
                            HouseMenu.SetLightColour(lIdx, rgb);
                    }
                }
            });

            Events.Add("House::Furn", (args) =>
            {
                if (args[0] is int)
                {
                    var fUid = Utils.Convert.ToUInt32(args[0]);

                    var furn = Furniture.GetValueOrDefault(fUid);

                    furn?.Destroy();

                    Furniture.Remove(fUid);

                    if (HouseMenu.IsActive)
                        HouseMenu.RemoveInstalledFurniture(fUid);
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

                        if (HouseMenu.IsActive)
                            HouseMenu.AddInstalledFurniture(fUid, fData);
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
                if (!HouseMenu.IsActive)
                    return;

                HouseMenu.SetButtonState((bool)args[0] ? "entry" : "closet", (bool)args[1]);
            });
        }

        public static void FindObject(MapObject obj)
        {
            for (int i = 0; i < TempBlips.Count; i++)
            {
                var x = TempBlips[i];

                if (x?.Exists != true)
                    TempBlips.Remove(x);
            }

            var blip = new ExtraBlip(162, obj.GetCoords(false), null, 0.75f, 3, 255, 0f, true, 0, 0f, Player.LocalPlayer.Dimension, ExtraBlip.Types.Furniture);

            blip.SetAsReachable(2.5f);

            TempBlips.Add(blip);

            AsyncTask.Methods.Run(async () =>
            {
                for (int i = 0; i < 5; i++)
                {
                    obj.SetAlpha(125, false);

                    await RAGE.Game.Invoker.WaitAsync(250);

                    if (obj?.Exists != true || blip?.Exists != true)
                        break;

                    obj.SetAlpha(255, false);

                    await RAGE.Game.Invoker.WaitAsync(250);

                    if (obj?.Exists != true || blip?.Exists != true)
                        break;
                }

                obj?.SetAlpha(255, false);
            }, 0);
        }

        private static void CreateObject(uint fUid, Data.Furniture fData, Utils.Vector4 fProps)
        {
            MapObject obj = null;

            if (fData.Type == Data.Furniture.Types.Locker)
            {
                obj = fData.CreateObject(fProps.Position, new Vector3(0f, 0f, fProps.RotationZ), Player.LocalPlayer.Dimension, fUid, Player.LocalPlayer.GetData<uint>("House::CurrentHouse::LI"));
            }
            else if (fData.Type == Data.Furniture.Types.Wardrobe)
            {
                obj = fData.CreateObject(fProps.Position, new Vector3(0f, 0f, fProps.RotationZ), Player.LocalPlayer.Dimension, fUid, Player.LocalPlayer.GetData<uint>("House::CurrentHouse::WI"));
            }
            else if (fData.Type == Data.Furniture.Types.Fridge)
            {
                obj = fData.CreateObject(fProps.Position, new Vector3(0f, 0f, fProps.RotationZ), Player.LocalPlayer.Dimension, fUid, Player.LocalPlayer.GetData<uint>("House::CurrentHouse::FI"));
            }
            else if (fData.Type == Data.Furniture.Types.KitchenSet)
            {
                obj = fData.CreateObject(fProps.Position, new Vector3(0f, 0f, fProps.RotationZ), Player.LocalPlayer.Dimension, fUid, fUid);
            }
            else
            {
                obj = fData.CreateObject(fProps.Position, new Vector3(0f, 0f, fProps.RotationZ), Player.LocalPlayer.Dimension, fUid);
            }

            obj.SetData("Data", fData);

            Furniture.Add(fUid, obj);
        }

        private static void HouseExit(params object[] args)
        {
            AsyncTask.Methods.CancelPendingTask("House::Enter");

            Player.LocalPlayer.ResetData("House::CurrentHouse");

            Players.CloseAll(false);

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
                foreach (var y in x.Objects)
                    y?.Destroy();

            Lights.Clear();

            foreach (var x in Furniture.Values)
                x?.Destroy();

            Furniture.Clear();

            HUD.Menu.UpdateCurrentTypes(false, HUD.Menu.Types.Menu_House);
            HUD.Menu.UpdateCurrentTypes(false, HUD.Menu.Types.Menu_Apartments);
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

                for (int j = 0; j < style.Lights[i].Length; j++)
                {
                    var lInfo = style.Lights[i][j];

                    var coords = lInfo.Position;

                    var handle = RAGE.Game.Object.GetClosestObjectOfType(coords.X, coords.Y, coords.Z, 1f, lInfo.Model, false, true, true);

                    if (handle <= 0)
                        continue;

                    var x = new RAGE.Elements.MapObject(handle)
                    {
                        Dimension = uint.MaxValue,
                    };

                    t.Objects[j].Destroy();

                    t.Objects[j] = x;

                    RAGE.Game.Entity.SetEntityLights(x.Handle, !t.State);

                    var rgb = t.RGB;

                    if (rgb != null)
                        x.SetLightColour(rgb);
                }
            }
        }
    }
}
