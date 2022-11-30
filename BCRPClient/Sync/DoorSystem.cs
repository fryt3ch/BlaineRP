using RAGE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            new DoorInfo("s_gar_0", "v_ilev_rc_door2", new Vector3(179.6684f, -1004.762f, -98.85f));
            new DoorInfo("s_gar_0", "v_ilev_rc_door2", new Vector3(207.7825f, -999.6905f, -98.85f));
            new DoorInfo("s_gar_0", "v_ilev_garageliftdoor", new Vector3(239.0922f, -1005.571f, 100f));

            // Bank
            new DoorInfo("s_bank_0", "v_ilev_bk_gate", new Vector3(256.3116f, 220.6579f, 106.4296f));
            new DoorInfo("s_bank_0", "v_ilev_bk_door", new Vector3(237.7704f, 227.87f, 106.426f));
        }

        public static void ToggleLock(DoorInfo doorInfo, bool state) => ToggleLock(doorInfo.Model, doorInfo.Position, state);

        public static void ToggleLock(string model, Vector3 position, bool state) => ToggleLock(RAGE.Util.Joaat.Hash(model), position, state);

        public static void ToggleLock(uint model, Vector3 position, bool state)
        {
            RAGE.Game.Object.DoorControl(model, position.X, position.Y, position.Z, state, 0f, 0f, 0f);
        }
    }
}
