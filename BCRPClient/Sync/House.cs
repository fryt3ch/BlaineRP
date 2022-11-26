using Newtonsoft.Json;
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

        public class SettlerPermissions
        {
            [JsonProperty(PropertyName = "L")]
            public bool Lights { get; set; }

            [JsonProperty(PropertyName = "D")]
            public bool Doors { get; set; }

            [JsonProperty(PropertyName = "C")]
            public bool Closet { get; set; }

            [JsonProperty(PropertyName = "W")]
            public bool Wardrobe { get; set; }

            [JsonProperty(PropertyName = "F")]
            public bool Fridge { get; set; }

            public SettlerPermissions() { }
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

        private static bool HasGarage;

        private static List<Additional.ExtraColshape> TempColshapes;
        private static List<Blip> TempBlips;

        private static List<MapObject> TempObjects;

        private static List<(int Handle, Utils.Colour Colour, bool State)> Lights;
        private static List<(string Model, Vector3 Position, bool IsLocked)> Doors;

        public House()
        {
            Style.LoadAll();

            HasGarage = false;

            TempColshapes = new List<Additional.ExtraColshape>();
            TempBlips = new List<Blip>();
            TempObjects = new List<MapObject>();

            Lights = new List<(int Handle, Utils.Colour Color, bool State)>();
            Doors = new List<(string Model, Vector3 Position, bool IsLocked)>();

            Events.Add("House::Enter", async (object[] args) =>
            {
                Additional.ExtraColshape.InteractionColshapesAllowed = false;

                Additional.SkyCamera.FadeScreen(true);

                var id = (uint)(int)args[0];

                var hType = (HouseTypes)(int)args[1];

                if (hType == HouseTypes.House)
                {
                    var house = Data.Locations.House.All[id];

                    Player.LocalPlayer.SetData("House::CurrentHouse", house);
                    Player.LocalPlayer.SetData("House::CurrentHouse::Type", hType);

                    Style.Types sType = (Style.Types)(int)args[1];

                    var dimension = (uint)(int)args[2];

                    var doors = RAGE.Util.Json.Deserialize<bool[]>((string)args[3]);
                    var lights = RAGE.Util.Json.Deserialize<(Utils.Colour Colour, bool State)[]>((string)args[4]);

                    var style = Style.Get(hType, house.RoomType, sType);

                    var exitCs = new Additional.Cylinder(style.EnterancePos, 1f, 2f, false, Utils.RedColor, dimension);

                    exitCs.InteractionType = Additional.ExtraColshape.InteractionTypes.HouseExit;
                    exitCs.ActionType = Additional.ExtraColshape.ActionTypes.HouseExit;

                    TempColshapes.Add(exitCs);

                    TempBlips.Add(new RAGE.Elements.Blip(40, style.EnterancePos, "Выход", 0.75f, 1, 255, 0, false, 0, 0, dimension));

                    Lights.Clear();
                    Doors.Clear();

                    await RAGE.Game.Invoker.WaitAsync(1500);

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

                    await RAGE.Game.Invoker.WaitAsync(500);

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
            });
        }

        public static bool Exit()
        {
            if (Additional.ExtraColshape.LastSent.IsSpam(1000, false, false))
                return false;

            Events.CallRemote("House::Exit");

            Additional.ExtraColshape.LastSent = DateTime.Now;

            return true;
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

        public static bool OpenContainer()
        {
            if (Additional.ExtraColshape.LastSent.IsSpam(1000, false, false))
                return false;

            if (!Player.LocalPlayer.HasData("CurrentContainer"))
                return false;

            CEF.Inventory.Show(CEF.Inventory.Types.Container);

            Additional.ExtraColshape.LastSent = DateTime.Now;

            return true;
        }
    }
}
