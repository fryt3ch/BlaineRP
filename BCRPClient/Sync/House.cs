using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using static BCRPClient.Locale.Notifications.Players;

namespace BCRPClient.Sync
{
    public class House : Events.Script
    {
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

            public (string Model, Vector3 Position)[] Lights { get; set; }
            public (string Model, Vector3 Position)[] Doors { get; set; }

            /// <summary>Словарь планировок</summary>
            private static Dictionary<HouseTypes, Dictionary<RoomTypes, Dictionary<Types, Style>>> All { get; set; } = new Dictionary<HouseTypes, Dictionary<RoomTypes, Dictionary<Types, Style>>>();

            public static Style Get(HouseTypes hType, RoomTypes rType, Types sType) => All.GetValueOrDefault(hType)?.GetValueOrDefault(rType)?.GetValueOrDefault(sType);

            public Style(HouseTypes HouseType, RoomTypes RoomType, Types Type, Vector3 Position, Vector3 EnterancePos, (string Model, Vector3 Position)[] Lights, (string Model, Vector3 Position)[] Doors)
            {
                this.HouseType = HouseType;
                this.RoomType = RoomType;

                this.Type = Type;

                this.Position = Position;

                this.EnterancePos = EnterancePos;

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
                new Style(HouseTypes.House, RoomTypes.Two, Types.First, new Vector3(70f, 70f, -10f), new Vector3(67.955511f, 70.03592f, -10f),
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

                new Style(HouseTypes.House, RoomTypes.Two, Types.Second, new Vector3(70f, 70f, -20f), new Vector3(67.955511f, 70.03592f, -20f),
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
        private static List<Blip> TempBlips;

        private static List<MapObject> TempObjects;

        private static List<(int Handle, Utils.Colour Colour, bool State)> Lights;
        private static List<(string Model, Vector3 Position, bool IsLocked)> Doors;

        private static Dictionary<uint, MapObject> Furniture;

        public House()
        {
            Data.Furniture.LoadAll();

            Style.LoadAll();

            TempColshapes = new List<Additional.ExtraColshape>();
            TempBlips = new List<Blip>();
            TempObjects = new List<MapObject>();

            Lights = new List<(int Handle, Utils.Colour Color, bool State)>();
            Doors = new List<(string Model, Vector3 Position, bool IsLocked)>();

            Furniture = new Dictionary<uint, MapObject>();

            Events.Add("House::Enter", async (object[] args) =>
            {
                Additional.ExtraColshape.InteractionColshapesAllowed = false;

                Additional.SkyCamera.FadeScreen(true);

                var data = RAGE.Util.Json.Deserialize<JObject>((string)args[0]);

                var id = (uint)(int)data["I"];

                var hType = (HouseTypes)(int)data["T"];

                var sType = (Style.Types)(int)data["S"];

                var dimension = (uint)data["Dim"];

                var doors = RAGE.Util.Json.Deserialize<bool[]>((string)data["DS"]);
                var lights = RAGE.Util.Json.Deserialize<(Utils.Colour Colour, bool State)[]>((string)data["LS"]);

                foreach (var x in RAGE.Util.Json.Deserialize<List<JObject>>((string)data["F"]))
                {
                    var fData = Data.Furniture.GetData((string)x["I"]);
                    var fUid = (uint)x["U"];

                    var pos = new Vector3((float)x["PX"], (float)x["PY"], (float)x["PZ"]);
                    var rot = new Vector3((float)x["RX"], (float)x["RY"], (float)x["RZ"]);

                    if (fData.Type == Data.Furniture.Types.Locker)
                    {
                        Furniture.Add(fUid, fData.CreateObject(pos, rot, dimension, fUid, (uint)data["LI"]));
                    }
                    else if (fData.Type == Data.Furniture.Types.Wardrobe)
                    {
                        Furniture.Add(fUid, fData.CreateObject(pos, rot, dimension, fUid, (uint)data["WI"]));
                    }
                    else if (fData.Type == Data.Furniture.Types.Fridge)
                    {
                        Furniture.Add(fUid, fData.CreateObject(pos, rot, dimension, fUid, (uint)data["FI"]));
                    }
                    else if (fData.Type == Data.Furniture.Types.KitchenSet)
                    {

                    }
                    else
                    {
                        Furniture.Add(fUid, fData.CreateObject(pos, rot, dimension, fUid));
                    }
                }

                Player.LocalPlayer.SetData("House::CurrentHouse::Type", hType);

                if (hType == HouseTypes.House)
                {
                    var house = Data.Locations.House.All[id];

                    var style = Style.Get(hType, house.RoomType, sType);

                    Player.LocalPlayer.SetData("House::CurrentHouse", house);

                    var exitCs = new Additional.Cylinder(style.EnterancePos, 1f, 2f, false, Utils.RedColor, dimension);

                    exitCs.InteractionType = Additional.ExtraColshape.InteractionTypes.HouseExit;

                    TempColshapes.Add(exitCs);

                    if (house.GarageType != null)
                    {
                        var gData = Data.Locations.Garage.Style.Get((Data.Locations.Garage.Types)house.GarageType);

                        var gExitCs = new Additional.Cylinder(gData.EnterPosition, 1f, 2f, false, Utils.RedColor, dimension, null);

                        gExitCs.InteractionType = Additional.ExtraColshape.InteractionTypes.GarageExit;

                        TempColshapes.Add(gExitCs);
                    }

                    TempBlips.Add(new RAGE.Elements.Blip(40, style.EnterancePos, "Выход", 0.75f, 1, 255, 0, true, 0, 0, dimension));

                    Lights.Clear();
                    Doors.Clear();

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

                        Doors.Add((x.Model, x.Position, doors[i]));

                        Sync.DoorSystem.ToggleLock(x.Model, x.Position, doors[i]);

                        var handle = RAGE.Game.Object.GetClosestObjectOfType(x.Position.X, x.Position.Y, x.Position.Z, 1f, RAGE.Util.Joaat.Hash(x.Model), false, true, true);

                        if (handle <= 0)
                            continue;

                        var t = new MapObject(handle);

                        t.SetData("Interactive", true);

                        t.SetData("DoorIdx", i);
                        t.SetData("DoorState", doors[i]);

                        t.SetData("CustomAction", new Action<MapObject>((obj) =>
                        {

                        }));

                        t.SetData("CustomText", new Action<float, float>((x, y) =>
                        {

                        }));

                        TempObjects.Add(t);
                    }

                    for (int i = 0; i < style.Lights.Length; i++)
                    {
                        var x = style.Lights[i];

                        var handle = RAGE.Game.Object.GetClosestObjectOfType(x.Position.X, x.Position.Y, x.Position.Z, 1f, RAGE.Util.Joaat.Hash(x.Model), false, true, true);

                        if (handle <= 0)
                            continue;

                        Lights.Add((handle, lights[i].Colour, lights[i].State));

                        RAGE.Game.Entity.SetEntityLights(handle, !lights[i].State);
                        RAGE.Game.Invoker.Invoke(0x5F048334B4A4E774, handle, true, lights[i].Colour.Red, lights[i].Colour.Green, lights[i].Colour.Blue);
                    }

                    Additional.SkyCamera.FadeScreen(false);

                    Additional.ExtraColshape.InteractionColshapesAllowed = true;
                }
                else
                {
                    return;
                }
            });

            Events.Add("House::Exit", (object[] args) =>
            {
                Additional.ExtraColshape.InteractionColshapesAllowed = false;

                Additional.SkyCamera.FadeScreen(true);

                (new AsyncTask(() =>
                {
                    Additional.SkyCamera.FadeScreen(false);

                    Additional.ExtraColshape.InteractionColshapesAllowed = true;
                }, 1500)).Run();

                foreach (var x in TempColshapes)
                    x?.Delete();

                TempColshapes.Clear();

                foreach (var x in TempBlips)
                    x?.Destroy();

                TempBlips.Clear();

                Doors.Clear(); Lights.Clear();

                foreach (var x in TempObjects)
                    x?.Destroy();

                TempObjects.Clear();

                foreach (var x in Furniture.Values)
                    x?.Destroy();

                Furniture.Clear();
            });
        }

        public static bool DoorLock()
        {
            if (Additional.ExtraColshape.LastSent.IsSpam(1000, false, false))
                return false;

            if (!Player.LocalPlayer.HasData("House::CurrentDoorIdx"))
                return true;

            var idx = Player.LocalPlayer.GetData<int>("House::CurrentDoorIdx");
            var door = Doors[idx];

            Sync.DoorSystem.ToggleLock(door.Model, door.Position, !door.IsLocked);

            door.IsLocked = !door.IsLocked;
            Doors[idx] = door;

            if (door.IsLocked)
            {
                var dBlip = new Blip(255, door.Position, "Закрытая дверь", 0.75f, 1, 255, 0, false, 0, 0, Player.LocalPlayer.Dimension);

                dBlip.SetData("DoorIdx", idx);

                TempBlips.Add(dBlip);
            }
            else
            {
                for (int i = 0; i < TempBlips.Count; i++)
                    if (TempBlips[i].HasData("DoorIdx") && TempBlips[i].GetData<int>("DoorIdx") == idx)
                    {
                        TempBlips[i].Destroy();

                        TempBlips.Remove(TempBlips[i]);
                    }
            }

            return true;
        }
    }
}
