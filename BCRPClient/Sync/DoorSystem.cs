using RAGE;
using System.Collections.Generic;

namespace BCRPClient.Sync
{
    class DoorSystem : Events.Script
    {
        public class DoorInfo
        {
            public uint Model { get; set; }

            public Vector3 Position { get; set; }

            public string Name { get; set; }

            public string Id { get; set; }

            public DoorInfo(string Id, uint Model, Vector3 Position, bool DefaultState = true)
            {
                this.Id = Id;

                this.Model = Model;
                this.Position = Position;

                All.Add(this);

                ToggleLock(DefaultState);
            }

            public DoorInfo(string Id, string Model, Vector3 Position, bool DefaultState = true) : this(Id, RAGE.Util.Joaat.Hash(Model), Position, DefaultState) { }

            public void ToggleLock(bool state) => DoorSystem.ToggleLock(this, state);
        }

        private static List<DoorInfo> All { get; set; } = new List<DoorInfo>();

        public DoorSystem()
        {
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
        }

        public static void ToggleLock(DoorInfo doorInfo, bool state) => ToggleLock(doorInfo.Model, doorInfo.Position, state);

        public static void ToggleLock(string model, float x, float y, float z, bool state) => ToggleLock(RAGE.Util.Joaat.Hash(model), x, y, z, state);

        public static void ToggleLock(string model, Vector3 position, bool state) => ToggleLock(RAGE.Util.Joaat.Hash(model), position, state);

        public static void ToggleLock(uint model, Vector3 position, bool state) => ToggleLock(model, position.X, position.Y, position.Z, state);

        public static void ToggleLock(uint model, float x, float y, float z, bool state)
        {
            RAGE.Game.Object.DoorControl(model, x, y, z, state, 0f, 0f, 0f);
        }
    }
}
