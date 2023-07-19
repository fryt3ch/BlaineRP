using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Sync
{
    [Script(int.MaxValue)]
    public class DoorSystem 
    {
        private static Dictionary<HashSet<uint>, string> Names { get; set; } = new Dictionary<HashSet<uint>, string>()
        {
            { new HashSet<uint>() { 1 }, "Центр управления" },
        };

        public static List<Door> AllDoors { get; set; } = new List<Door>();

        public static DateTime LastSent;

        public class Door
        {
            private MapObject DoorObject { get; set; }

            public uint Model { get; set; }

            public Vector3 Position { get; set; }

            public string Name => Names.Where(x => x.Key.Contains(Id)).Select(x => x.Value).FirstOrDefault();

            public uint Id { get; set; }

            public bool IsLocked => Sync.World.GetSharedData<bool>($"DOORS_{Id}_L", false);

            public Door(uint Id, uint Model, Vector3 Position, uint Dimension)
            {
                this.Id = Id;

                this.Model = Model;
                this.Position = Position;

                All.Add(this);

                var cs = new Additional.Sphere(new Vector3(Position.X, Position.Y, Position.Z), 5f, false, Utils.RedColor, Dimension, null)
                {
                    Name = $"DoorSys_{Id}",

                    OnEnter = OnEnterColshape,
                    OnExit = OnExitColshape,
                };

                Sync.World.AddDataHandler($"DOORS_{Id}_L", OnLockStateChange);
            }

            public Door(uint Id, string Model, Vector3 Position, uint Dimension) : this(Id, RAGE.Util.Joaat.Hash(Model), Position, Dimension) { }

            public void ToggleLock(bool state) => DoorSystem.ToggleLock(this, state);

            private async void OnEnterColshape(RAGE.Events.CancelEventArgs cancel)
            {
                var id = $"DoorSys_{Id}";

                var cs = Additional.ExtraColshape.All.Where(x => x.Name == id).FirstOrDefault();

                if (cs == null)
                    return;

                int handle = 0;

                while (handle <= 0)
                {
                    handle = RAGE.Game.Object.GetClosestObjectOfType(Position.X, Position.Y, Position.Z, 1f, Model, false, true, true);

                    if (handle <= 0)
                    {
                        await RAGE.Game.Invoker.WaitAsync(25);

                        if (!cs.IsInside)
                            return;
                    }
                }

                DoorObject = new MapObject(handle);

                var name = Name;

                DoorObject.SetData("Interactive", true);

                DoorObject.SetData("CustomText", (Action<float, float>)((x, y) =>
                {
                    Utils.DrawText(name == null ? "Дверь" : $"Дверь - {name}", x, y - NameTags.Interval * 2f, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);

                    var isLocked = IsLocked;

                    if (isLocked)
                        Utils.DrawText($"[Закрыта]", x, y - NameTags.Interval, 255, 0, 0, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                    else
                        Utils.DrawText($"[Открыта]", x, y - NameTags.Interval, 0, 255, 0, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                }));

                DoorObject.SetData("CustomAction", (Action<MapObject>)(async (obj) =>
                {
                    if (LastSent.IsSpam(500, false, true))
                        return;

                    LastSent = Sync.World.ServerTime;

                    var res = await Events.CallRemoteProc("Door::Lock", Id, !IsLocked);
                }));
            }

            private void OnExitColshape(RAGE.Events.CancelEventArgs cancel)
            {
                var id = $"DoorSys_{Id}";

                var cs = Additional.ExtraColshape.All.Where(x => x.Name == id).FirstOrDefault();

                if (cs == null)
                    return;

                DoorObject?.Destroy();
            }

            private static void OnLockStateChange(string key, object value, object oldBalue)
            {
                var dataD = key.Split('_');

                var id = uint.Parse(dataD[1]);

                var door = GetDoorById(id);

                if (door == null)
                    return;

                door.ToggleLock((bool?)value ?? false);
            }

            public static void PostInitializeAll()
            {
                foreach (var x in All)
                {
                    x.ToggleLock(x.IsLocked);
                }
            }
        }

        public static List<Door> All { get; set; } = new List<Door>();

        public static Door GetDoorById(uint id) => All.Where(x => x.Id == id).FirstOrDefault();

        public DoorSystem()
        {
            #region DOORS_TO_REPLACE

            #endregion

            // Garages
            ToggleLock("v_ilev_rc_door2", 179.6684f, -1004.762f, -98.85f, true);
            ToggleLock("v_ilev_rc_door2", 207.7825f, -999.6905f, -98.85f, true);
            ToggleLock("v_ilev_garageliftdoor", 239.0922f, -1005.571f, 100f, true);

            // Bank
            ToggleLock("v_ilev_bk_gate", 256.3116f, 220.6579f, 106.4296f, true);
            ToggleLock("v_ilev_bk_door", 237.7704f, 227.87f, 106.426f, true);

            // Shooting range
            ToggleLock("v_ilev_gc_door01", 6.81789f, -1098.209f, 29.94685f, true);

            // Paleto Police Cells
            ToggleLock("v_ilev_ph_cellgate", -431.1921f, 5999.741f, 31.87312f, true);
            ToggleLock("v_ilev_ph_cellgate", -428.0646f, 5996.672f, 31.87312f, true);

            // Mission Row Police Cells
            ToggleLock("v_ilev_ph_cellgate", 461.8065f, -994.4086f, 25.06443f, true);
            ToggleLock("v_ilev_ph_cellgate", 461.8065f, -997.6584f, 25.06443f, true);
            ToggleLock("v_ilev_ph_cellgate", 461.8065f, -1001.302f, 25.06443f, true);
            ToggleLock("v_ilev_gtdoor", 467.1922f, -996.4594f, 25.00599f, true);
            ToggleLock("v_ilev_gtdoor", 471.4755f, -996.4594f, 25.00599f, true);
            ToggleLock("v_ilev_gtdoor", 475.7543f, -996.4594f, 25.00599f, true);
            ToggleLock("v_ilev_gtdoor", 480.0301f, -996.4594f, 25.00599f, true);

            // Amunnation Club 1
            ToggleLock("v_ilev_gc_door04", 16.12787f, -1114.606f, 29.94694f, true);
            ToggleLock("v_ilev_gc_door03", 18.572f, -1115.495f, 29.94694f, true);

            // Amunnation Club 2
            ToggleLock("v_ilev_gc_door04", 813.1779f, -2148.27f, 29.76892f, true);
            ToggleLock("v_ilev_gc_door03", 810.5769f, -2148.27f, 29.76892f, true);
        }

        public static void ToggleLock(Door doorInfo, bool state) => ToggleLock(doorInfo.Model, doorInfo.Position, state);

        public static void ToggleLock(string model, float x, float y, float z, bool state) => ToggleLock(RAGE.Util.Joaat.Hash(model), x, y, z, state);

        public static void ToggleLock(string model, Vector3 position, bool state) => ToggleLock(RAGE.Util.Joaat.Hash(model), position, state);

        public static void ToggleLock(uint model, Vector3 position, bool state) => ToggleLock(model, position.X, position.Y, position.Z, state);

        public static void ToggleLock(uint model, float x, float y, float z, bool state)
        {
            RAGE.Game.Object.DoorControl(model, x, y, z, state, 0f, 0f, 0f);
        }
    }
}
