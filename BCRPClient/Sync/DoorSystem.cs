using RAGE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Sync
{
    class DoorSystem : Events.Script
    {
        public enum DoorTypes
        {
            HouseEnter1 = 0,

        }

        public class DoorInfo
        {
            public DoorTypes DoorType { get; set; }
            public uint Model { get; set; }
            public Vector3 Position { get; set; }

            public DoorInfo(DoorTypes DoorType, uint Model, Vector3 Position)
            {
                this.DoorType = DoorType;
                this.Model = Model;
                this.Position = Position;
            }

            public DoorInfo(DoorTypes DoorType, string Model, Vector3 Position) : this(DoorType, RAGE.Util.Joaat.Hash(Model), Position) { }

            public void ToggleLock(bool state) => DoorSystem.ToggleLock(this, state);
        }

        private static List<DoorInfo> All { get; set; }

        public static DoorInfo Get(DoorTypes doorType) => All.Where(x => x.DoorType == doorType).FirstOrDefault();

        public DoorSystem()
        {
            All = new List<DoorInfo>()
            {
                new DoorInfo(DoorTypes.HouseEnter1, "hei_v_ilev_fh_heistdoor2", new Vector3(286.0411f, -998.7357f, -92.67875f)),
            };

            Get(DoorTypes.HouseEnter1).ToggleLock(true);
        }

        public static void ToggleLock(DoorInfo doorInfo, bool state)
        {
            RAGE.Game.Object.DoorControl(doorInfo.Model, doorInfo.Position.X, doorInfo.Position.Y, doorInfo.Position.Z, state, 0f, 0f, 0f);
        }

        public static void ToggleLock(string Model, Vector3 Position, bool state)
        {
            RAGE.Game.Object.DoorControl(RAGE.Util.Joaat.Hash(Model), Position.X, Position.Y, Position.Z, state, 0f, 0f, 0f);
        }
    }
}
